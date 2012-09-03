using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;
using Library.Lab;

namespace Library.LabServerEngine
{
    public class LabExperimentManager : IDisposable
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "LabExperimentManager";

        //
        // String constants for log messages
        //
        private const string STRLOG_farmSize = " farmSize: ";
        private const string STRLOG_unitId = " unitId: ";
        private const string STRLOG_experimentId = " experimentId: ";
        private const string STRLOG_sbName = " sbName: ";
        private const string STRLOG_statusCode = " statusCode: ";
        private const string STRLOG_remainingRuntime = " remainingRuntime: ";
        private const string STRLOG_estRuntime = " estRuntime: ";
        private const string STRLOG_QueuePosition = " QueuePosition: ";
        private const string STRLOG_QueueWaitTime = " QueueWaitTime: ";
        private const string STRLOG_QueuedExperimentCancelled = " Queued experiment was cancelled.";
        private const string STRLOG_RunningExperimentCancelled = " Running experiment was cancelled.";
        private const string STRLOG_ExperimentNotCancelled = " Experiment was not cancelled.";
        private const string STRLOG_RevertingToWaiting = " Reverting to waiting... ";
        private const string STRLOG_IsRunning = " IsRunning: ";
        private const string STRLOG_success = " success: ";
        private const string STRLOG_disposing = " disposing: ";

        //
        // String constants for exception messages
        //
        private const string STRERR_appData = "appData";
        private const string STRERR_allowedServiceBrokers = "allowedServiceBrokers";
        private const string STRERR_experimentQueue = "experimentQueue";
        private const string STRERR_experimentResults = "experimentResults";
        private const string STRERR_experimentStatistics = "experimentStatistics";
        private const string STRERR_labConfiguration = "labConfiguration";
        private const string STRERR_signalCompleted = "signalCompleted";
        private const string STRERR_statusLock = "statusLock";
        private const string STRERR_managerLock = "managerLock";
        private const string STRERR_signalSubmitted = "signalSubmitted";
        private const string STRERR_threadLabExperimentManager = "threadLabExperimentManager";
        private const string STRERR_LabExperimentManagerFailedReady = "Lab experiment manager failed to become ready!";
        private const string STRERR_FailedToQueueExperiment = "Failed to queue experiment!";
        private const string STRERR_FarmSizeMinimum = "Farm size minimum is 1";
        private const string STRERR_FarmSizeMaximum = "Farm size exceeds maximum of ";
        private const string STRERR_FarmSizeInvalid = "Farm size is invalid";
        private const string STRERR_UserGroupNotSpecified = "User group is not specified";

        //
        // Local constants
        //
        private const int MAX_FARM_SIZE = 20;
        private const int ENGINE_CHECK_DELAY_SECS = 60;
        private const int QUEUE_CHECK_DELAY_SECS = 600;

        //
        // Local variables
        //
        private ExperimentQueueDB experimentQueue;
        private ExperimentResults experimentResults;
        private ExperimentStatistics experimentStatistics;
        private bool disposed;
        private Object signalSubmitted;
        private Object signalCompleted;
        private Object statusLock;
        private Thread threadLabExperimentManager;
        private bool submittedSinceLastNotifyCheck;
        private Object managerLock;

        //
        // Local variables available to a derived class
        //
        protected AppData appData;

        #endregion

        #region Private Properties

        //
        // These need to be locked by 'statusLock'
        //
        private bool slRunning;
        private StatusCodes slStatus;

        private bool Running
        {
            get
            {
                lock (this.statusLock)
                {
                    return this.slRunning;
                }
            }
            set
            {
                lock (this.statusLock)
                {
                    this.slRunning = value;
                }
            }
        }

        private StatusCodes ManagerStatus
        {
            get
            {
                lock (this.statusLock)
                {
                    return this.slStatus;
                }
            }
            set
            {
                lock (this.statusLock)
                {
                    this.slStatus = value;
                }
            }
        }

        #endregion

        #region Public Properties

        private bool slOnline;
        private string slLabStatusMessage;

        public bool Online
        {
            get { return this.slOnline; }
        }

        public string LabStatusMessage
        {
            get { return this.slLabStatusMessage; }
        }

        public StatusCodes Status
        {
            get
            {
                lock (this.statusLock)
                {
                    return this.slStatus;
                }
            }
        }

        public bool IsAnyRunning
        {
            get
            {
                bool anyRunning = false;

                Trace.Write(" IsAnyRunning:");
                for (int i = 0; i < this.appData.farmSize; i++)
                {
                    LabExperimentEngine labExperimentEngine = this.appData.labExperimentEngines[i];
                    Trace.Write("  Unit #" + i.ToString() + "->" + labExperimentEngine.IsRunning.ToString());

                    anyRunning = anyRunning || labExperimentEngine.IsRunning;
                }
                Trace.WriteLine(string.Empty);

                return anyRunning;
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public LabExperimentManager(AllowedServiceBrokersDB allowedServiceBrokers, LabConfiguration labConfiguration)
            : this(allowedServiceBrokers, labConfiguration, 0)
        {
        }

        //-------------------------------------------------------------------------------------------------//

        public LabExperimentManager(AllowedServiceBrokersDB allowedServiceBrokers, LabConfiguration labConfiguration, int farmSize)
        {
            const string STRLOG_MethodName = "LabExperimentManager";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            try
            {
                // Thread objects have not been created yet
                this.disposed = true;

                //
                // Initialise local variables
                //
                this.appData = new AppData();
                if (this.appData == null)
                {
                    throw new ArgumentNullException(STRERR_appData);
                }

                if (allowedServiceBrokers == null)
                {
                    throw new ArgumentNullException(STRERR_allowedServiceBrokers);
                }
                this.appData.allowedServiceBrokers = allowedServiceBrokers;

                if (labConfiguration == null)
                {
                    throw new ArgumentNullException(STRERR_labConfiguration);
                }
                this.appData.labConfiguration = labConfiguration;

                //
                // Get the farm size
                //
                try
                {
                    if (farmSize == 0)
                    {
                        // Get the farm size from the Application's configuration file
                        this.appData.farmSize = Utilities.GetIntAppSetting(Consts.STRCFG_FarmSize);
                    }
                    else
                    {
                        this.appData.farmSize = farmSize;
                    }
                }
                catch (ArgumentNullException)
                {
                    // Farm size is not specified, default to 1
                    this.appData.farmSize = 1;
                }
                catch (Exception)
                {
                    throw new ArgumentException(STRERR_FarmSizeInvalid);
                }
                if (this.appData.farmSize < 1)
                {
                    throw new ArgumentException(STRERR_FarmSizeMinimum);
                }
                if (this.appData.farmSize > MAX_FARM_SIZE)
                {
                    throw new ArgumentException(STRERR_FarmSizeMaximum + MAX_FARM_SIZE.ToString());
                }

                Logfile.Write(STRLOG_farmSize + appData.farmSize);

                //
                // Create class instances and objects that are not derived
                //
                this.experimentQueue = new ExperimentQueueDB();
                if (this.experimentQueue == null)
                {
                    throw new ArgumentNullException(STRERR_experimentQueue);
                }
                this.appData.experimentQueue = this.experimentQueue;

                this.experimentResults = new ExperimentResults();
                if (this.experimentResults == null)
                {
                    throw new ArgumentNullException(STRERR_experimentResults);
                }
                this.appData.experimentResults = this.experimentResults;

                this.experimentStatistics = new ExperimentStatistics();
                if (this.experimentStatistics == null)
                {
                    throw new ArgumentNullException(STRERR_experimentStatistics);
                }
                this.appData.experimentStatistics = this.experimentStatistics;

                this.signalCompleted = new Object();
                if (this.signalCompleted == null)
                {
                    throw new ArgumentNullException(STRERR_signalCompleted);
                }
                this.appData.signalCompleted = this.signalCompleted;

                //
                // Get email addresses for the LabServer, experiment completion/cancelled and failed
                //
                try
                {
                    this.appData.emailAddressLabServer = Utilities.GetAppSetting(Consts.STRCFG_EmailAddressLabServer);
                    char[] splitterCharArray = new char[] { Consts.CHR_CsvSplitterChar };
                    string csvEmail = Utilities.GetAppSetting(Consts.STRCFG_EmailAddressesExperimentCompleted);
                    this.appData.emailAddressesExperimentCompleted = csvEmail.Split(splitterCharArray);
                    csvEmail = Utilities.GetAppSetting(Consts.STRCFG_EmailAddressesExperimentFailed);
                    this.appData.emailAddressesExperimentFailed = csvEmail.Split(splitterCharArray);
                }
                catch
                {
                }

                //
                // Initialise property variables
                //
                this.slRunning = false;
                this.slStatus = StatusCodes.Unknown;
                this.slOnline = false;
                this.slLabStatusMessage = string.Empty;

                //
                // Create thread objects
                //
                this.managerLock = new Object();
                if (this.managerLock == null)
                {
                    throw new ArgumentNullException(STRERR_managerLock);
                }
                this.statusLock = new Object();
                if (this.statusLock == null)
                {
                    throw new ArgumentNullException(STRERR_statusLock);
                }
                this.signalSubmitted = new Object();
                if (this.signalSubmitted == null)
                {
                    throw new ArgumentNullException(STRERR_signalSubmitted);
                }
                this.threadLabExperimentManager = new Thread(new ThreadStart(LabExperimentManagerThread));
                if (this.threadLabExperimentManager == null)
                {
                    throw new ArgumentNullException(STRERR_threadLabExperimentManager);
                }

                //
                // Don't start the thread yet, the method Start() must be called to start the thread
                // after the derived class has completed its initialisation.
                //

            }
            catch (Exception ex)
            {
                // Log the message and throw the exception back to the caller
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool Cancel(int experimentID, string sbName)
        {
            bool cancelled = false;

            lock (this.managerLock)
            {
                //
                // Try cancelling the experiment on the queue
                //
                cancelled = this.experimentQueue.Cancel(experimentID, sbName);
                if (cancelled == true)
                {
                    Logfile.Write(STRLOG_QueuedExperimentCancelled);
                }
                else
                {
                    //
                    // Experiment may be currently running, try cancelling it there
                    //
                    for (int i = 0; i < this.appData.farmSize; i++)
                    {
                        LabExperimentEngine labExperimentEngine = this.appData.labExperimentEngines[i];

                        cancelled = labExperimentEngine.Cancel(experimentID, sbName);
                        if (cancelled == true)
                        {
                            Logfile.Write(STRLOG_RunningExperimentCancelled);
                            break;
                        }
                    }
                }

                if (cancelled == false)
                {
                    Logfile.Write(STRLOG_ExperimentNotCancelled);
                }
            }

            return cancelled;
        }

        //-------------------------------------------------------------------------------------------------//

        public WaitEstimate GetEffectiveQueueLength(string userGroup, int priorityHint)
        {
            WaitEstimate waitEstimate = null;

            lock (this.managerLock)
            {
                //
                // NOTE: This implementation does not consider the group or priority of the user
                //

                // Get queue wait estimate
                waitEstimate = this.experimentQueue.GetWaitEstimate();

                // Add in time remaining before the next experiment can run
                LabExperimentStatus labExperimentStatus = this.GetLabExperimentStatus(0, null);
                waitEstimate.estWait += labExperimentStatus.statusReport.estRemainingRuntime;
            }

            return waitEstimate;
        }

        //-------------------------------------------------------------------------------------------------//

        public LabExperimentStatus GetExperimentStatus(int experimentId, string sbName)
        {
            const string STRLOG_MethodName = "GetExperimentStatus";

            LabExperimentStatus labExperimentStatus = null;

            lock (this.managerLock)
            {
                string logMessage = STRLOG_experimentId + experimentId.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + sbName + Logfile.STRLOG_Quote;

                Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

                labExperimentStatus = new LabExperimentStatus();
                logMessage = string.Empty;

                //
                // Get the status of the experiment from the queue table
                //
                StatusCodes status = this.experimentQueue.GetExperimentStatus(experimentId, sbName);
                if (status == StatusCodes.Unknown)
                {
                    //
                    // The experiment never existed
                    //
                }
                else if (status == StatusCodes.Waiting)
                {
                    //
                    // Experiment is waiting on the queue
                    //
                    QueuedExperimentInfo queuedExperimentInfo = this.experimentQueue.GetQueuedExperimentInfo(experimentId, sbName);
                    if (queuedExperimentInfo != null && queuedExperimentInfo.position > 0)
                    {
                        // Set the experiment status
                        labExperimentStatus.statusReport.statusCode = (int)queuedExperimentInfo.status;

                        // Get the queue position and wait time
                        labExperimentStatus.statusReport.wait =
                            new WaitEstimate(queuedExperimentInfo.position, queuedExperimentInfo.waitTime);

                        // Add in time for any currently running experiment ????
                        labExperimentStatus.statusReport.wait.estWait += GetMinRemainingRuntime();

                        // Get the time it takes to run the experiment 
                        labExperimentStatus.statusReport.estRuntime = queuedExperimentInfo.estExecutionTime;
                        labExperimentStatus.statusReport.estRemainingRuntime = queuedExperimentInfo.estExecutionTime;

                        logMessage =
                            Logfile.STRLOG_Spacer + STRLOG_QueuePosition + labExperimentStatus.statusReport.wait.effectiveQueueLength.ToString() +
                            Logfile.STRLOG_Spacer + STRLOG_QueueWaitTime + labExperimentStatus.statusReport.wait.estWait.ToString() +
                            Logfile.STRLOG_Spacer + STRLOG_estRuntime + labExperimentStatus.statusReport.estRuntime.ToString() +
                            Logfile.STRLOG_Spacer + STRLOG_remainingRuntime + labExperimentStatus.statusReport.estRemainingRuntime.ToString();
                    }
                }
                else if (status == StatusCodes.Running)
                {
                    //
                    // Experiment is currently running
                    //
                    labExperimentStatus = this.GetLabExperimentStatus(experimentId, sbName);
                }
                else
                {
                    //
                    // Experiment has completed, cancelled or failed
                    //
                    ResultReport resultReport = this.experimentResults.Load(experimentId, sbName);

                    // Set the experiment status
                    labExperimentStatus.statusReport.statusCode = resultReport.statusCode;
                }

                logMessage = STRLOG_statusCode + ((StatusCodes)labExperimentStatus.statusReport.statusCode).ToString() +
                    Logfile.STRLOG_Spacer + logMessage;

                Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);
            }

            return labExperimentStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        public string GetLabConfiguration(string userGroup)
        {
            string xmlLabConfiguration = string.Empty;

            lock (this.managerLock)
            {
                //
                // Load the lab configuration from the specified file and convert to a string
                //
                XmlDocument xmlDocument = XmlUtilities.GetXmlDocumentFromFile(this.appData.labConfiguration.Filename);
                XmlNode xmlNodeLabConfiguration = XmlUtilities.GetXmlRootNode(xmlDocument, Consts.STRXML_labConfiguration);

                //
                // Write the Xml document to a string
                //
                StringWriter sw = new StringWriter();
                XmlTextWriter xtw = new XmlTextWriter(sw);
                xtw.Formatting = Formatting.Indented;
                xmlDocument.WriteTo(xtw);
                xtw.Flush();
                xmlLabConfiguration = sw.ToString();
            }

            return xmlLabConfiguration;
        }

        //-------------------------------------------------------------------------------------------------//

        public string GetLabInfo()
        {
            string labInfo = null;

            lock (this.managerLock)
            {
            }

            return labInfo;
        }

        //-------------------------------------------------------------------------------------------------//

        public LabStatus GetLabStatus()
        {
            LabStatus labStatus = null;

            lock (this.managerLock)
            {
                bool isOnline;
                string message;

                //
                // Get the lab status of the experiment manager
                //
                lock (this.statusLock)
                {
                    isOnline = this.slOnline;
                    message = this.slLabStatusMessage;
                }

                //
                // Check the lab status
                //
                if (isOnline == false)
                {
                    labStatus = new LabStatus(isOnline, message);
                }
                else
                {
                    isOnline = true;
                    message = String.Empty;

                    //
                    // Check lab status of each experiment engine
                    //
                    for (int i = 0; i < this.appData.farmSize; i++)
                    {
                        LabExperimentEngine labExperimentEngine = this.appData.labExperimentEngines[i];

                        labStatus = labExperimentEngine.GetLabStatus();

                        //
                        // Keep a running tally
                        //
                        isOnline = isOnline && labStatus.online;
                        if (i > 0)
                        {
                            message += Logfile.STRLOG_Spacer;
                        }
                        message += i.ToString() + ":" + labStatus.labStatusMessage;
                    }

                    if (isOnline == true)
                    {
                        message = StatusCodes.Ready.ToString();
                    }

                    labStatus = new LabStatus(isOnline, message);
                }
            }

            return (labStatus);
        }

        //-------------------------------------------------------------------------------------------------//

        public ResultReport RetrieveResult(int experimentId, string sbName)
        {
            ResultReport resultReport = null;

            //
            // Try loading the experiment result from file
            //
            resultReport = this.experimentResults.Load(experimentId, sbName);
            if (resultReport.statusCode == (int)StatusCodes.Unknown)
            {
                //
                // No results found for the experiment, check the queue table to see if it ever existed
                //
                StatusCodes statusCode = this.experimentQueue.GetExperimentStatus(experimentId, sbName);
                resultReport.statusCode = (int)statusCode;
            }

            return resultReport;
        }

        //-------------------------------------------------------------------------------------------------//

        public SubmissionReport Submit(int experimentID, string sbName, string experimentSpecification,
            string userGroup, int priorityHint)
        {
            SubmissionReport submissionReport = null;

            //
            // Create a SubmissionReport object ready to fill in and return
            //
            submissionReport = new SubmissionReport(experimentID);

            //
            // Validate the experiment specification before submitting
            //
            ValidationReport validationReport = Validate(experimentSpecification, userGroup);
            if (validationReport.accepted == false)
            {
                // Experiment specification is invalid, cannot submit
                submissionReport.vReport = validationReport;
                return submissionReport;
            }

            //
            // Create an instance of the experiment
            //
            ExperimentInfo experimentInfo = new ExperimentInfo(experimentID, sbName,
                userGroup, priorityHint, experimentSpecification, (int)validationReport.estRuntime);

            //
            // Add the experiment to the queue
            //
            QueuedExperimentInfo queuedExperimentInfo = this.experimentQueue.Enqueue(experimentInfo);
            if (queuedExperimentInfo != null)
            {
                //
                // Update submission report
                //
                submissionReport.vReport.accepted = true;
                submissionReport.vReport.estRuntime = queuedExperimentInfo.estExecutionTime;
                submissionReport.wait = new WaitEstimate(queuedExperimentInfo.queueLength, queuedExperimentInfo.waitTime);

                //
                // Get minimum remaining runtime of any currently running experiments and add into the wait estimate
                //
                int minRemainingRuntime = GetMinRemainingRuntime();
                submissionReport.wait.estWait += minRemainingRuntime;

                //
                // Update the statistics with revised wait estimate
                //
                queuedExperimentInfo.waitTime = (int)submissionReport.wait.estWait;
                this.experimentStatistics.Submitted(queuedExperimentInfo, DateTime.Now);

                // Tell lab experiment manager thread that an experiment has been submitted
                this.SignalSubmitted();
            }
            else
            {
                //
                // Failed to add experiment to the queue
                //
                submissionReport.vReport.accepted = true;
                submissionReport.vReport.errorMessage = STRERR_FailedToQueueExperiment;
            }

            return submissionReport;
        }

        //-------------------------------------------------------------------------------------------------//

        public ValidationReport Validate(string xmlSpecification, string userGroup)
        {
            const string STRLOG_MethodName = "Validate";

            ValidationReport validationReport = null;

            lock (this.managerLock)
            {
                Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

                //
                // Check that usergroup is specified
                //
                if (userGroup == null || userGroup.Trim().Length == 0)
                {
                    validationReport = new ValidationReport(false);
                    validationReport.errorMessage = STRERR_UserGroupNotSpecified;
                }
                else
                {
                    validationReport = this.appData.labExperimentEngines[0].Validate(xmlSpecification);
                }

                Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
            }

            return validationReport;
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual void Create()
        {
            const string STRLOG_MethodName = "Create";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Create local class instances just to check that all is in order
            //
            ExperimentSpecification experimentSpecification = new ExperimentSpecification(this.appData.labConfiguration, null);
            LabExperimentResult labExperimentResult = new LabExperimentResult(this.appData.labConfiguration);

            //
            // Create instances of lab experiment engines
            //
            this.appData.labExperimentEngines = new LabExperimentEngine[this.appData.farmSize];
            for (int i = 0; i < this.appData.farmSize; i++)
            {
                this.appData.labExperimentEngines[i] = new LabExperimentEngine(i, this.appData);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public void Start()
        {
            const string STRLOG_MethodName = "Start";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Start the lab experiment manager thread running
            //
            if (this.threadLabExperimentManager.ThreadState == System.Threading.ThreadState.Unstarted)
            {
                this.disposed = false;
                this.Running = true;
                this.threadLabExperimentManager.Start();
            }

            //
            // Wait for thread to become ready, with timeout
            //
            for (int i = 0; i < 30; i++)
            {
                if (this.Status == StatusCodes.Ready)
                {
                    break;
                }
                Thread.Sleep(1000);
            }
            if (this.Status != StatusCodes.Ready)
            {
                Logfile.WriteError(STRERR_LabExperimentManagerFailedReady);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Signal the waiting thread that experiment has been submitted and is ready for execution.
        /// </summary>
        public bool SignalSubmitted()
        {
            const string STRLOG_MethodName = "SignalSubmitted";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            if (this.disposed == false)
            {
                // Signal waiting thread
                lock (this.signalSubmitted)
                {
                    Monitor.Pulse(this.signalSubmitted);
                }

                success = true;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        #region Close and Dispose

        /// <summary>
        /// Do not make this method virtual. A derived class should not be allowed to override this method.
        /// </summary>
        public void Close()
        {
            // Calls the Dispose method without parameters
            Dispose();
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Implement IDisposable. Do not make this method virtual. A derived class should not be able
        /// to override this method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            // Take yourself off the Finalization queue to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Use C# destructor syntax for finalization code. This destructor will run only if the Dispose
        /// method does not get called. It gives your base class the opportunity to finalize. Do not provide
        /// destructors in types derived from this class.
        /// </summary>
        ~LabExperimentManager()
        {
            Trace.WriteLine("~LabExperimentManager():");

            //
            // Do not re-create Dispose clean-up code here. Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            //
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios:
        /// 1. If disposing equals true, the method has been called directly or indirectly by a user's code.
        ///    Managed and unmanaged resources can be disposed.
        /// 2. If disposing equals false, the method has been called by the runtime from inside the finalizer
        ///    and you should not reference other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            const string STRLOG_MethodName = "Dispose";

            string logMessage = STRLOG_disposing + disposing.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Check to see if Dispose has already been called
            //
            if (this.disposed == false)
            {
                //
                // If disposing equals true, dispose all managed and unmanaged resources.
                //
                if (disposing == true)
                {
                    // Dispose managed resources here. Anything that has a Dispose() method.
                }

                //
                // Release unmanaged resources here. If disposing is false, only the following
                // code is executed.
                //
                for (int i = 0; i < this.appData.farmSize; i++)
                {
                    if (this.appData != null && this.appData.labExperimentEngines != null && this.appData.labExperimentEngines[i] != null)
                    {
                        this.appData.labExperimentEngines[i].Close();
                    }
                }

                //
                // Tell LabExperimentManager thread that it is no longer running
                //
                if (this.Running == true)
                {
                    this.Running = false;

                    //
                    // Lab experiment manager thread may be waiting for an experiment submission signal
                    //
                    lock (this.signalSubmitted)
                    {
                        Monitor.Pulse(this.signalSubmitted);
                    }

                    //
                    // Wait for LabExperimentManager thread to terminate
                    //
                    try
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (this.threadLabExperimentManager.Join(500) == true)
                            {
                                // Thread has terminated
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                }

                this.disposed = true;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        #endregion

        //=================================================================================================//

        private enum States
        {
            sStart, sInit, sIdle, sCheckExperimentQueue, sFindAvailableEngine, sWaitForAvailableEngine, sCheckNotified
        }

        //-------------------------------------------------------------------------------------------------//

        private void LabExperimentManagerThread()
        {
            const string STRLOG_MethodName = "LabExperimentManagerThread";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Initialise state machine
            //
            this.Running = true;
            this.ManagerStatus = StatusCodes.Running;
            States state = States.sInit;
            States lastState = States.sStart;
            int lastEngineIndex = this.appData.farmSize - 1;

            //
            // State machine loop
            //
            try
            {
                bool success;

                while (this.Running == true)
                {
                    //
                    // Display message on each state change
                    //
                    if (state != lastState)
                    {
                        string logMessage = " [ " + STRLOG_MethodName + ": " + lastState.ToString() + " -> " + state.ToString() + " ]";
                        Logfile.Write(logMessage);
                        Trace.WriteLine(logMessage);

                        lastState = state;
                    }

                    switch (state)
                    {
                        case States.sInit:

                            //
                            // Update LabManager status
                            //
                            lock (this.statusLock)
                            {
                                this.slOnline = true;
                                this.slLabStatusMessage = StatusCodes.Ready.ToString();
                            }

                            //
                            // Revert any 'Running' experiments back to 'Waiting' so that they can be run again
                            //
                            ExperimentInfo[] allRunning = this.experimentQueue.RetrieveAllWithStatus(StatusCodes.Running);
                            for (int i = 0; i < allRunning.Length; i++)
                            {
                                int experimentId = allRunning[i].experimentId;
                                string sbName = allRunning[i].sbName;

                                success = this.experimentQueue.UpdateStatus(allRunning[i].experimentId, allRunning[i].sbName, StatusCodes.Waiting);

                                string logMessage = STRLOG_RevertingToWaiting + STRLOG_experimentId + experimentId.ToString() +
                                    Logfile.STRLOG_Spacer + Logfile.STRLOG_Quote + STRLOG_sbName + sbName + Logfile.STRLOG_Quote +
                                    Logfile.STRLOG_Spacer + STRLOG_success + success.ToString();

                                Logfile.Write(logMessage);
                            }

                            //
                            // Check if any experiments have not notified their ServiceBroker
                            //
                            this.submittedSinceLastNotifyCheck = true;
                            state = States.sIdle;
                            break;

                        case States.sIdle:

                            this.ManagerStatus = StatusCodes.Ready;

                            //
                            // Wait for an experiment to be submitted or timeout after QUEUE_CHECK_DELAY_SECS.
                            // In either case, check the experiment queue. Maybe an experiment submission
                            // signal got missed and it didn't get seen here. It has happened before.
                            //
                            lock (this.signalSubmitted)
                            {
                                if (Monitor.Wait(this.signalSubmitted, QUEUE_CHECK_DELAY_SECS * 1000) == true)
                                {
                                    //
                                    // Signal received, go check the queue
                                    //
                                    this.submittedSinceLastNotifyCheck = true;
                                    state = States.sCheckExperimentQueue;
                                    break;
                                }
                            }

                            //
                            // Timed out, go check some other things
                            //
                            if (this.submittedSinceLastNotifyCheck == true)
                            {
                                this.ManagerStatus = StatusCodes.Running;
                                state = States.sCheckNotified;
                            }
                            break;

                        case States.sCheckExperimentQueue:

                            //
                            // Wait a bit before checking the queue
                            //
                            Thread.Sleep(1000);

                            //
                            // Check the queue to see if there are any experiments waiting
                            //
                            if (this.experimentQueue.GetWaitCount() > 0)
                            {
                                state = States.sFindAvailableEngine;
                                break;
                            }

                            state = States.sIdle;
                            break;

                        case States.sFindAvailableEngine:

                            //
                            // Find an available experiment engine to run a waiting experiment
                            //
                            bool foundAvailableEngine = false;
                            for (int i = 0; i < this.appData.farmSize; i++)
                            {
                                //
                                // Determine which engine to look at
                                //
#if x
                                if (++lastEngineIndex == this.appData.farmSize)
                                {
                                    lastEngineIndex = 0;
                                }
#else
                                lastEngineIndex = i;
#endif
                                //
                                // Determine if this experiment engine is currently running
                                //
                                LabExperimentEngine labExperimentEngine = this.appData.labExperimentEngines[lastEngineIndex];
                                bool isRunning = labExperimentEngine.IsRunning;

                                string logMessage = " i: " + i.ToString() +
                                    Logfile.STRLOG_Spacer + STRLOG_unitId + labExperimentEngine.UnitId.ToString() +
                                    Logfile.STRLOG_Spacer + STRLOG_IsRunning + isRunning.ToString();
                                Logfile.Write(logMessage);

                                //
                                // Try starting the experiment engine, may already be running or offline
                                //
                                if (labExperimentEngine.Start() == true)
                                {
                                    //
                                    // An available engine has been found and started, check the queue again
                                    //
                                    foundAvailableEngine = true;
                                    state = States.sCheckExperimentQueue;
                                    break;
                                }
                            }

                            //
                            // Check if an available engine was found
                            // 
                            if (foundAvailableEngine == false)
                            {
                                //
                                // Not found, have to wait for one
                                // 
                                state = States.sWaitForAvailableEngine;
                            }
                            break;

                        case States.sWaitForAvailableEngine:

                            //
                            // Wait for an experiment engine to complete execution or timeout after ENGINE_CHECK_DELAY_SECS.
                            // In either case, check for an available engine. Maybe an experiment engine completion
                            // signal got missed and it didn't get seen here. It has happened before.
                            //
                            lock (this.signalCompleted)
                            {
                                if (Monitor.Wait(this.signalCompleted, ENGINE_CHECK_DELAY_SECS * 1000) == true)
                                {
                                    //
                                    // Signal received, find an available engine to run the next experiment
                                    //
                                    state = States.sFindAvailableEngine;
                                }
                            }

                            //
                            // No experiment engine has signalled its completion, check the queue again
                            //
                            state = States.sCheckExperimentQueue;
                            break;

                        case States.sCheckNotified:

                            //
                            // Check if any experiments have not notified their ServiceBroker
                            //
                            success = true;
                            ResultsIdInfo[] allNotNotified = this.experimentResults.RetrieveAllNotNotified();


                            for (int i = 0; i < allNotNotified.Length && success == true; i++)
                            {
                                int experimentId = allNotNotified[i].experimentId;
                                string sbName = allNotNotified[i].sbName;

                                try
                                {
                                    //
                                    // Attempt to notify the ServiceBroker for this experiment
                                    //
                                    LabServerToSbAPI labServerToSbAPI = new LabServerToSbAPI(this.appData.allowedServiceBrokers);
                                    if ((success = labServerToSbAPI.Notify(experimentId, sbName)) == true)
                                    {
                                        success = this.experimentResults.UpdateNotified(experimentId, sbName);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logfile.WriteError(ex.Message);
                                    success = false;
                                }
                            }

                            if (success == true)
                            {
                                //
                                // All notifies completed, don't check again until next experiment submission
                                //
                                this.submittedSinceLastNotifyCheck = false;
                            }
                            state = States.sCheckExperimentQueue;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(STRLOG_MethodName + ": " + ex.Message);
            }

            Trace.WriteLine(STRLOG_MethodName + ": Exiting");

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        private LabExperimentStatus GetLabExperimentStatus(int experimentId, string sbName)
        {
            const string STRLOG_MethodName = "GetLabExperimentStatus";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            LabExperimentStatus labExperimentStatus = null;

            //
            // Check experiment status of each experiment engine
            //
            for (int i = 0; i < this.appData.farmSize; i++)
            {
                LabExperimentEngine labExperimentEngine = this.appData.labExperimentEngines[i];

                labExperimentStatus = labExperimentEngine.GetLabExperimentStatus(experimentId, sbName);
                if ((StatusCodes)labExperimentStatus.statusReport.statusCode != StatusCodes.Unknown)
                {
                    // This engine is running the experiment
                    break;
                }
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return (labExperimentStatus);
        }

        //-------------------------------------------------------------------------------------------------//

        private int GetMinRemainingRuntime()
        {
            const string STRLOG_MethodName = "GetLabExperimentStatus";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            int minRemainingRuntime = Int32.MaxValue;

            //
            // Check experiment staus of each experiment engine
            //
            for (int i = 0; i < this.appData.farmSize; i++)
            {
                LabExperimentEngine labExperimentEngine = this.appData.labExperimentEngines[i];

                //
                // Get the remaining runtime for this experiment engine
                //
                int remainingRuntime = labExperimentEngine.GetRemainingRuntime();

                // Check if this is a smaller value
                if (remainingRuntime < minRemainingRuntime)
                {
                    minRemainingRuntime = remainingRuntime;
                }
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return (minRemainingRuntime);
        }
    }
}
