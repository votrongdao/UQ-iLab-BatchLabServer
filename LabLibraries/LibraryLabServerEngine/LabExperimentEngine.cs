using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Threading;
using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Equipment;
using Library.LabServerEngine.Drivers.Setup;

namespace Library.LabServerEngine
{
    public class LabExperimentEngine : IDisposable
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "LabExperimentEngine";

        //
        // Constants
        //
        private const int MAX_RUNEXPERIMENT_RETRIES = 3;
        private const int MAX_SBNOTIFY_RETRIES = 3;

        //
        // String constants for logfile messages
        //
        private const string STRLOG_unitId = " unitId: ";
        private const string STRLOG_experimentId = " experimentId: ";
        private const string STRLOG_sbName = " sbName: ";
        private const string STRLOG_statusCode = " statusCode: ";
        private const string STRLOG_remainingRuntime = " remainingRuntime: ";
        private const string STRLOG_estRuntime = " estRuntime: ";
        private const string STRLOG_QueuePosition = " Queue Position: ";
        private const string STRLOG_LabExperimentEngineIsReady = " Lab experiment engine is ready.";
        private const string STRLOG_QueuedExperimentCancelled = " Experiment was cancelled while queued";
        private const string STRLOG_ActualExecutionTime = " Actual execution time: ";
        private const string STRLOG_seconds = " seconds";
        private const string STRLOG_LabExperimentEngineAlreadyStarted = "Lab experiment engine is already started!";
        private const string STRLOG_LabExperimentEngineThreadState = " LabExperimentEngine thread state -> ";
        private const string STRLOG_IsRunning = " IsRunning: ";
        private const string STRLOG_success = "success: ";
        private const string STRLOG_disposing = " disposing: ";
        private const string STRLOG_MailMessageSubject_arg2 = "[{0} LabServer] Experiment {1}";
        private const string STRLOG_MailMessageBody_arg7 = "An experiment has completed with the following details:\r\n" +
            "ServiceBroker: {0}\r\nUsergroup:     {1}\r\nExperiment Id: {2}\r\nUnit Id:       {3}\r\nSetupId:       {4}\r\nStatusCode:    {5}\r\n{6}";
        private const string STRLOG_MailMessageError_arg = "Error Message: {0}\r\n";
        private const string STRLOG_SendingEmail_arg3 = "Sending email - To: '{0}  From: '{1}'  Subject: '{2}'";

        protected const string STRLOG_online = " online: ";
        protected const string STRLOG_labStatusMessage = " labStatusMessage: ";

        //
        // String constants for exception messages
        //
        private const string STRERR_appData = "appData";
        private const string STRERR_allowedServiceBrokers = "allowedServiceBrokers";
        private const string STRERR_experimentResults = "experimentResults";
        private const string STRERR_experimentStatistics = "experimentStatistics";
        private const string STRERR_labConfiguration = "labConfiguration";
        private const string STRERR_signalCompleted = "signalCompleted";
        private const string STRERR_labEquipmentEngine = "labEquipmentEngine";
        private const string STRERR_experimentQueue = "experimentQueue";
        private const string STRERR_statusLock = "statusLock";
        private const string STRERR_signalSubmitted = "signalSubmitted";
        private const string STRERR_threadLabExperimentEngine = "threadLabExperimentEngine";
        private const string STRERR_LabExperimentEngineFailedStart = "Lab experiment engine failed to start!";
        private const string STRERR_FailedToUpdateQueueStatus = "Failed to update queue statistics!";
        private const string STRERR_FailedToSaveExperimentResults = "Failed to save experiment results!";
        private const string STRERR_FailedToUpdateStatisticsCancelled = "Failed to update statistics cancelled!";
        private const string STRERR_FailedToUpdateStatisticsCompleted = "Failed to update statistics completed!";
        private const string STRERR_ReRunningExperiment = "Re-Running Experiment... Retry #";


        /// <summary>
        /// Information about the currently running experiment.
        /// </summary>
        public class LabExperimentInfo
        {
            public ExperimentInfo experimentInfo;
            public CancelExperiment cancelExperiment;
            public DateTime startDateTime;
            public double minTimeToLive;
            public string errorMessage;

            public LabExperimentInfo(ExperimentInfo experimentInfo, DateTime startDateTime)
            {
                this.experimentInfo = experimentInfo;
                this.cancelExperiment = new CancelExperiment();
                this.startDateTime = startDateTime;
                this.minTimeToLive = 0.0;
                this.errorMessage = null;
            }
        }

        //
        // Local variables
        //
        private AllowedServiceBrokersDB allowedServiceBrokers;
        private ExperimentQueueDB experimentQueue;
        private ExperimentResults experimentResults;
        private ExperimentStatistics experimentStatistics;
        private bool disposed;
        private Object signalCompleted;
        private Object statusLock;
        private Thread threadLabExperimentEngine;
        private EquipmentServiceAPI equipmentServiceAPI;
        private string emailAddressLabServer;
        private string[] emailAddressesExperimentCompleted;
        private string[] emailAddressesExperimentFailed;

        //
        // Local variables available to a derived class
        //
        protected int unitId;
        protected LabConfiguration labConfiguration;
        protected LabExperimentInfo labExperimentInfo;
        protected EquipmentService equipmentServiceProxy;

        #endregion

        #region Private Properties

        private bool slRunning;

        //
        // Private properties
        //
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

        #endregion

        #region Public Properties

        public int UnitId
        {
            get { return this.unitId; }
        }

        public bool IsRunning
        {
            get
            {
                lock (this.statusLock)
                {
                    return this.slRunning;
                }
            }
        }

        public bool IsRunningExperiment
        {
            get
            {
                lock (this.statusLock)
                {
                    return (this.slRunning == true && this.labExperimentInfo != null);
                }
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public LabExperimentEngine(int unitId, AppData appData)
        {
            const string STRLOG_MethodName = "LabExperimentEngine";

            string logMessage = STRLOG_unitId + unitId.ToString();

            Logfile.WriteCalled(null, STRLOG_MethodName, logMessage);

            //
            // Initialise local variables
            //
            this.disposed = true;
            this.unitId = unitId;
            this.labExperimentInfo = null;

            //
            // Initialise private properties
            //
            this.slRunning = false;

            try
            {
                //
                // Initialise local variables from application data
                //
                if (appData == null)
                {
                    throw new ArgumentNullException(STRERR_appData);
                }

                this.allowedServiceBrokers = appData.allowedServiceBrokers;
                if (this.allowedServiceBrokers == null)
                {
                    throw new ArgumentNullException(STRERR_allowedServiceBrokers);
                }

                this.experimentQueue = appData.experimentQueue;
                if (this.experimentQueue == null)
                {
                    throw new ArgumentNullException(STRERR_experimentQueue);
                }

                this.experimentResults = appData.experimentResults;
                if (this.experimentResults == null)
                {
                    throw new ArgumentNullException(STRERR_experimentResults);
                }

                this.experimentStatistics = appData.experimentStatistics;
                if (this.experimentStatistics == null)
                {
                    throw new ArgumentNullException(STRERR_experimentStatistics);
                }

                this.labConfiguration = appData.labConfiguration;
                if (this.labConfiguration == null)
                {
                    throw new ArgumentNullException(STRERR_labConfiguration);
                }

                this.signalCompleted = appData.signalCompleted;
                if (this.signalCompleted == null)
                {
                    throw new ArgumentNullException(STRERR_signalCompleted);
                }

                //
                // Try to get a proxy to the equipment service, it may not exist
                //
                this.equipmentServiceProxy = null;
                this.equipmentServiceAPI = null;
                try
                {
                    this.equipmentServiceAPI = new EquipmentServiceAPI(this.unitId);
                    this.equipmentServiceProxy = this.equipmentServiceAPI.EquipmentServiceProxy;
                    if (this.equipmentServiceProxy != null)
                    {
                        this.equipmentServiceProxy.GetTimeUntilReady();
                    }
                }
                catch
                {
                    // No equipment service available
                }

                //
                // Save email addresses for experiment completion notification
                //
                this.emailAddressLabServer = appData.emailAddressLabServer;
                this.emailAddressesExperimentCompleted = appData.emailAddressesExperimentCompleted;
                this.emailAddressesExperimentFailed = appData.emailAddressesExperimentFailed;

                //
                // Create thread objects
                //
                this.statusLock = new Object();
                if (this.statusLock == null)
                {
                    throw new ArgumentNullException(STRERR_statusLock);
                }
                this.threadLabExperimentEngine = new Thread(new ThreadStart(LabExperimentEngineThread));
                if (this.threadLabExperimentEngine == null)
                {
                    throw new ArgumentNullException(STRERR_threadLabExperimentEngine);
                }

                //
                // Don't start the thread yet, the method Start() must be called to start the thread
                // after the derived class has completed its initialisation.
                //
                Logfile.Write(STRLOG_LabExperimentEngineIsReady + STRLOG_unitId + this.unitId.ToString());
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

        public bool Start()
        {
            const string STRLOG_MethodName = "Start";

            string logMessage = STRLOG_unitId + unitId.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_IsRunning + this.IsRunning.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_LabExperimentEngineThreadState + this.threadLabExperimentEngine.ThreadState.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = false;

            //
            // Start the thread if it is not already running
            //
            if (this.IsRunning == false)
            {
                //
                // Start the lab experiment engine thread running
                //
                try
                {
                    //
                    // Start the lab experiment engine thread running
                    //
                    this.threadLabExperimentEngine.Start();
                    this.disposed = false;
                    success = true;
                }
                catch (ThreadStateException)
                {
                    try
                    {
                        //
                        // Create a new thread and start it
                        //
                        this.threadLabExperimentEngine = new Thread(new ThreadStart(LabExperimentEngineThread));
                        this.threadLabExperimentEngine.Start();
                        this.disposed = false;
                        success = true;
                    }
                    catch (ThreadStateException ex)
                    {
                        Logfile.WriteError(STRERR_LabExperimentEngineFailedStart + Logfile.STRLOG_Spacer + ex.Message);
                    }
                }

                if (success == true)
                {
                    //
                    // Give the thread a chance to start running and then check that it
                    //
                    int timeout = 5;
                    while (--timeout > 0)
                    {
                        Thread.Sleep(500);
                        if (this.IsRunning == true)
                        {
                            // Lab experiment engine thread has started running
                            break;
                        }
                        Trace.Write('?');
                    }
                    if (timeout == 0)
                    {
                        Logfile.WriteError(STRERR_LabExperimentEngineFailedStart);
                        success = false;
                    }
                }
            }

            logMessage = STRLOG_success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public LabStatus GetLabStatus()
        {
            const string STRLOG_MethodName = "GetLabStatus";

            string logMessage = STRLOG_unitId + unitId.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            LabStatus labStatus;

            //
            // Check if there is an equipment service
            //
            if (this.equipmentServiceProxy == null)
            {
                //
                // No equipment service, just get the status of this engine
                //
                StatusCodes status = (this.IsRunningExperiment == true) ? StatusCodes.Running : StatusCodes.Ready;
                labStatus = new LabStatus(true, status.ToString());
            }
            else
            {
                //
                // Get the status of the equipment service
                //
                try
                {
                    LabEquipmentStatus labEquipmentStatus = this.equipmentServiceProxy.GetLabEquipmentStatus();
                    labStatus = new LabStatus(labEquipmentStatus.online, labEquipmentStatus.statusMessage);
                }
                catch (Exception ex)
                {
                    labStatus = new LabStatus(false, ex.Message);
                    Logfile.WriteError(ex.Message);
                }
            }

            logMessage = STRLOG_online + labStatus.online.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_labStatusMessage + Logfile.STRLOG_Quote + labStatus.labStatusMessage + Logfile.STRLOG_Quote;

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return labStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        public LabExperimentStatus GetLabExperimentStatus(int experimentId, string sbName)
        {
            const string STRLOG_MethodName = "GetLabExperimentStatus";

            string logMessage = STRLOG_experimentId + experimentId.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + sbName + Logfile.STRLOG_Quote +
                Logfile.STRLOG_Spacer + STRLOG_unitId + unitId.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            LabExperimentStatus labExperimentStatus = new LabExperimentStatus(
                new ExperimentStatus((int)StatusCodes.Unknown));

            int remainingRuntime = 0;

            lock (this.statusLock)
            {
                //
                // Check if the specified experiment is running on this engine
                //
                if ((this.labExperimentInfo != null) &&
                    (labExperimentInfo.experimentInfo.experimentId == experimentId) &&
                    (labExperimentInfo.experimentInfo.sbName.Equals(sbName, StringComparison.OrdinalIgnoreCase) == true))
                {
                    //
                    // Update the status code
                    //
                    StatusCodes status = this.labExperimentInfo.experimentInfo.status;
                    labExperimentStatus.statusReport.statusCode = (int)status;

                    if (status == StatusCodes.Running)
                    {
                        //
                        // The specified experiment is currently running, fill in information
                        //
                        labExperimentStatus.statusReport.estRuntime = (double)this.labExperimentInfo.experimentInfo.estExecutionTime;

                        // Calculate time already passed for the experiment
                        remainingRuntime = (int)((TimeSpan)(DateTime.Now - this.labExperimentInfo.startDateTime)).TotalSeconds;

                        // Now calculate the time remaining for the experiment
                        remainingRuntime = this.labExperimentInfo.experimentInfo.estExecutionTime - remainingRuntime;

                        //
                        // Estimated runtime may have been underestimated. Don't say remaining runtime is zero while
                        // the experiment is still running.
                        //
                        if (remainingRuntime < 1)
                        {
                            remainingRuntime = 1;
                        }

                        labExperimentStatus.statusReport.estRemainingRuntime = (double)remainingRuntime;

                        logMessage = STRLOG_estRuntime + labExperimentStatus.statusReport.estRuntime.ToString() +
                            Logfile.STRLOG_Spacer + STRLOG_remainingRuntime + labExperimentStatus.statusReport.estRemainingRuntime.ToString();
                    }
                }
                else
                {
                    // Unkown experiment
                    labExperimentStatus = new LabExperimentStatus(new ExperimentStatus((int)StatusCodes.Unknown));
                }
            }

            logMessage = STRLOG_statusCode + ((StatusCodes)labExperimentStatus.statusReport.statusCode).ToString() +
                Logfile.STRLOG_Spacer + logMessage;

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return (labExperimentStatus);
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetRemainingRuntime()
        {
            const string STRLOG_MethodName = "GetRemainingRuntime";

            string logMessage = STRLOG_unitId + unitId.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            int remainingRuntime = 0;

            lock (this.statusLock)
            {
                //
                // Check if any experiment is currently running
                //
                if (this.labExperimentInfo != null)
                {
                    // Calculate time already passed for the experiment
                    remainingRuntime = (int)((TimeSpan)(DateTime.Now - this.labExperimentInfo.startDateTime)).TotalSeconds;

                    // Now calculate the time remaining for the experiment
                    remainingRuntime = this.labExperimentInfo.experimentInfo.estExecutionTime - remainingRuntime;

                    // Cannot have a negative time
                    if (remainingRuntime < 0)
                    {
                        remainingRuntime = 0;
                    }
                }
            }

            logMessage = STRLOG_remainingRuntime + remainingRuntime;

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return (remainingRuntime);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool Cancel(int experimentId, string sbName)
        {
            const string STRLOG_MethodName = "Cancel";

            string logMessage = STRLOG_experimentId + experimentId.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + sbName + Logfile.STRLOG_Quote +
                Logfile.STRLOG_Spacer + STRLOG_unitId + unitId.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = false;

            //
            // Check if an experiment is currently running
            //
            lock (this.statusLock)
            {
                if (this.labExperimentInfo != null)
                {
                    //
                    // An experiment is currently running, check if it is this one
                    //
                    if (this.labExperimentInfo.experimentInfo.experimentId == experimentId &&
                        sbName != null && sbName.Equals(this.labExperimentInfo.experimentInfo.sbName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Cancel the experiment
                        this.labExperimentInfo.cancelExperiment.Cancel();
                        success = true;
                    }
                }
            }

            logMessage = STRLOG_success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual ValidationReport Validate(string xmlSpecification)
        {
            const string STRLOG_MethodName = "Validate";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            ValidationReport validationReport = new ValidationReport(false);

            //
            // Parse the XML specification string to generate a validation report
            //
            try
            {
                ExperimentSpecification experimentSpecification = new ExperimentSpecification(this.labConfiguration, this.equipmentServiceProxy);
                validationReport = experimentSpecification.Parse(xmlSpecification);
            }
            catch (Exception ex)
            {
                validationReport.errorMessage = ex.Message;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return validationReport;
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
        ~LabExperimentEngine()
        {
            Trace.WriteLine("~LabExperimentEngine():");

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

            string logMessage = STRLOG_disposing + disposing.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_unitId + this.unitId;

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

                //
                // Tell LabExperimentEngine thread that it is no longer running
                //
                if (this.Running == true)
                {
                    this.Running = false;

                    //
                    // Wait for LabExperimentEngine thread to terminate
                    //
                    try
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if (this.threadLabExperimentEngine.Join(500) == true)
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
            sStart,
            sGetExperiment, sPrepareExperiment, sRunExperiment, sConcludeExperiment,
            sNotifyServiceBroker, sNotifyEmail,
            sExitThread
        }

        //-------------------------------------------------------------------------------------------------//

        private void LabExperimentEngineThread()
        {
            const string STRLOG_MethodName = "LabExperimentEngineThread";

            string logMessage = STRLOG_unitId + unitId.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Initialise state machine
            //
            this.Running = true;
            States state = States.sGetExperiment;
            States lastState = States.sStart;
            ExperimentInfo experimentInfo = null;
            int runExperimentRetries = 0;

            try
            {
                //
                // State machine loop
                //
                while (this.IsRunning == true)
                {
                    //
                    // Display message on each state change
                    //
                    if (state != lastState)
                    {
                        logMessage = " [ " + STRLOG_MethodName + ": " + lastState.ToString() + "->" + state.ToString() +
                             STRLOG_unitId + unitId.ToString() + " ]";
                        Logfile.Write(logMessage);

                        Trace.WriteLine(logMessage);

                        lastState = state;
                    }

                    switch (state)
                    {
                        case States.sGetExperiment:

                            //
                            // Check if there is an experiment to run
                            //
                            experimentInfo = this.experimentQueue.Dequeue(this.unitId);
                            if (experimentInfo == null)
                            {
                                // No experiment to run
                                state = States.sExitThread;
                                break;
                            }

                            // Prepare experiment for running
                            state = States.sPrepareExperiment;
                            break;

                        case States.sPrepareExperiment:

                            // Prepare experiment ready for running
                            if (PrepareExperiment(experimentInfo) == false)
                            {
                                // Preparation failed
                                state = States.sConcludeExperiment;
                                break;
                            }

                            //
                            // Run the experiment
                            //
                            runExperimentRetries = MAX_RUNEXPERIMENT_RETRIES;
                            state = States.sRunExperiment;
                            break;

                        case States.sRunExperiment:

                            // Run the experiment
                            experimentInfo = RunExperiment(experimentInfo);

                            // Check if experiment failed, it should not!
                            if (experimentInfo.resultReport.statusCode == (int)StatusCodes.Failed)
                            {
                                // Log the error message
                                Logfile.WriteError(experimentInfo.resultReport.errorMessage);

                                if (runExperimentRetries-- > 0)
                                {
                                    //
                                    // Run the experiment again
                                    //
                                    int retry = MAX_RUNEXPERIMENT_RETRIES - runExperimentRetries;
                                    Logfile.Write(STRERR_ReRunningExperiment + retry.ToString());
                                    break;
                                }
                            }
                            state = States.sConcludeExperiment;
                            break;

                        case States.sConcludeExperiment:

                            // Conclude experiment after running
                            if (ConcludeExperiment(experimentInfo) == false)
                            {
                                // Conclude failed, don't run any more experiments
                                state = States.sExitThread;
                                break;
                            }

                            state = States.sNotifyServiceBroker;
                            break;

                        case States.sNotifyServiceBroker:

                            // Notify ServiceBroker of experiment completion
                            NotifyServiceBroker(experimentInfo);

                            state = States.sNotifyEmail;
                            break;

                        case States.sNotifyEmail:

                            // Notify by email of experiment completion
                            NotifyEmail(experimentInfo);

                            // Check for more experiments waiting to run
                            //state = States.sGetExperiment;
                            state = States.sExitThread;
                            break;

                        case States.sExitThread:

                            //
                            // Tell the lab experiment manager that this experiment is completed
                            //
                            lock (this.signalCompleted)
                            {
                                Monitor.Pulse(this.signalCompleted);
                            }

                            //
                            // Thread is no longer running
                            //
                            this.Running = false;
                            break;

                        default:

                            logMessage = " <<< " + STRLOG_MethodName + ": " + lastState.ToString() + "->" + state.ToString() +
                                 STRLOG_unitId + unitId.ToString() + " >>> ";
                            Logfile.WriteError(logMessage);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logMessage = " <<< " + STRLOG_MethodName + ": " + lastState.ToString() + "->" + state.ToString() +
                     STRLOG_unitId + unitId.ToString() + " >>> ";
                Logfile.WriteError(logMessage + ex.Message);
            }

            Trace.WriteLine(STRLOG_MethodName + ": Exiting");

            logMessage = STRLOG_unitId + unitId.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);
        }

        //-------------------------------------------------------------------------------------------------//

        private bool PrepareExperiment(ExperimentInfo experimentInfo)
        {
            const string STRLOG_MethodName = "PrepareExperiment";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            //
            // Check if the experiment has been cancelled
            //
            if (experimentInfo.cancelled == true)
            {
                //
                // Experiment was cancelled while waiting on the queue
                //
                experimentInfo.resultReport = new ResultReport((int)StatusCodes.Cancelled, STRLOG_QueuedExperimentCancelled);

                Logfile.Write(STRLOG_QueuedExperimentCancelled);
            }
            else
            {
                //
                // Update the statistics for starting the experiment
                //
                DateTime now = DateTime.Now;
                success = this.experimentStatistics.Started(experimentInfo.experimentId, experimentInfo.sbName, this.unitId, now);

                //
                // Create an instance of LabExperimentInfo to execute the experiment
                //
                lock (this.statusLock)
                {
                    experimentInfo.unitId = this.unitId;
                    experimentInfo.status = StatusCodes.Running;
                    this.labExperimentInfo = new LabExperimentInfo(experimentInfo, now);
                }
            }

            string logMessage = STRLOG_success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Run the experiment and fill in the result report. Override in a derived class.
        /// </summary>
        /// <param name="experimentInfo"></param>
        /// <returns></returns>
        public virtual ExperimentInfo RunExperiment(ExperimentInfo experimentInfo)
        {
            const string STRLOG_MethodName = "RunExperiment";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            // Create a result report ready to fill in
            experimentInfo.resultReport = new ResultReport();

            try
            {
                //
                // Parse the XML specification string to generate a validation report (should be accepted!)
                //
                ExperimentSpecification experimentSpecification = new ExperimentSpecification(this.labConfiguration, this.equipmentServiceProxy);
                ValidationReport validationReport = experimentSpecification.Parse(experimentInfo.xmlSpecification);
                if (validationReport.accepted == false)
                {
                    throw new ArgumentException(validationReport.errorMessage);
                }
                experimentInfo.setupId = experimentSpecification.SetupId;

                //
                // Create an instance of the driver for the specified setup and then
                // execute the experiment and return the result information
                //
                ExperimentResultInfo experimentResultInfo = null;
                if (experimentSpecification.SetupId.Equals(Consts.STRXML_SetupId_EquipmentGeneric))
                {
                    DriverEquipmentGeneric driver = new DriverEquipmentGeneric(this.equipmentServiceProxy, this.labConfiguration, this.labExperimentInfo.cancelExperiment);
                    experimentResultInfo = driver.Execute(experimentSpecification);
                }
                else if (experimentSpecification.SetupId.Equals(Consts.STRXML_SetupId_ModuleGeneric))
                {
                    DriverModuleGeneric driver = new DriverModuleGeneric(this.labConfiguration, this.labExperimentInfo.cancelExperiment);
                    experimentResultInfo = driver.Execute(experimentSpecification);
                }

                //
                // Create an instance of LabExperimentResult to convert the experiment results to an XML string
                //
                LabExperimentResult labExperimentResult = new LabExperimentResult(
                    experimentInfo.experimentId, experimentInfo.sbName, DateTime.Now,
                    experimentSpecification.SetupId, this.unitId, this.labConfiguration);

                //
                // Fill in the result report
                //
                experimentInfo.resultReport.experimentResults = labExperimentResult.ToString();
                experimentInfo.resultReport.statusCode = (int)experimentResultInfo.statusCode;
                experimentInfo.resultReport.errorMessage = experimentResultInfo.errorMessage;
            }
            catch (Exception ex)
            {
                experimentInfo.resultReport.statusCode = (int)StatusCodes.Failed;
                experimentInfo.resultReport.errorMessage = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return experimentInfo;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool ConcludeExperiment(ExperimentInfo experimentInfo)
        {
            const string STRLOG_MethodName = "ConcludeExperiment";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            try
            {
                //
                // Update experiment status both here and the queue table
                //
                experimentInfo.status = (StatusCodes)experimentInfo.resultReport.statusCode;
                if (this.experimentQueue.UpdateStatus(experimentInfo.experimentId, experimentInfo.sbName, experimentInfo.status) == false)
                {
                    throw new ArgumentException(STRERR_FailedToUpdateQueueStatus);
                }

                //
                // Save the experiment results
                //
                if (this.experimentResults.Save(experimentInfo) == false)
                {
                    throw new ArgumentException(STRERR_FailedToSaveExperimentResults);
                }

                //
                // Check experiment completion status for updating the statistics
                //
                DateTime now = DateTime.Now;
                if (experimentInfo.status == StatusCodes.Cancelled)
                {
                    // Update statistics for cancelled experiment
                    if (this.experimentStatistics.Cancelled(experimentInfo.experimentId, experimentInfo.sbName, now) == false)
                    {
                        throw new ArgumentException(STRERR_FailedToUpdateStatisticsCancelled);
                    }
                }
                else
                {
                    // Update statistics for completed experiment
                    if (this.experimentStatistics.Completed(experimentInfo.experimentId, experimentInfo.sbName, now) == false)
                    {
                        throw new ArgumentException(STRERR_FailedToUpdateStatisticsCompleted);
                    }

                    //
                    // Determine actual execution time of the experiment
                    //
                    TimeSpan timeSpan = now - this.labExperimentInfo.startDateTime;
                    int executionTime = (int)timeSpan.TotalSeconds;
                    Logfile.Write(STRLOG_ActualExecutionTime + executionTime.ToString() + STRLOG_seconds);
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            //
            // Experiment is finished
            //
            lock (this.statusLock)
            {
                if (this.labExperimentInfo != null)
                {
                    this.labExperimentInfo.cancelExperiment = null;
                    this.labExperimentInfo = null;
                }
            }

            string logMessage = STRLOG_success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool NotifyServiceBroker(ExperimentInfo experimentInfo)
        {
            const string STRLOG_MethodName = "NotifyServiceBroker";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            try
            {
                //
                // Notify the ServiceBroker so that the results can be retrieved
                //
                LabServerToSbAPI labServerToSbAPI = new LabServerToSbAPI(this.allowedServiceBrokers);
                if ((success = labServerToSbAPI.Notify(experimentInfo.experimentId, experimentInfo.sbName)) == true)
                {
                    success = this.experimentResults.UpdateNotified(experimentInfo.experimentId, experimentInfo.sbName);
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool NotifyEmail(ExperimentInfo experimentInfo)
        {
            const string STRLOG_MethodName = "NotifyEmail";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            try
            {
                /*
                 * Send an email to the iLab Administrator stating that an experiment has completed.
                 * If the experiment failed, send an email to the email address for reporting failed experiments.
                 */
                MailMessage mailMessage = null;
                string errorMessage = String.Empty;
                StatusCodes statusCode = (StatusCodes)experimentInfo.resultReport.statusCode;

                if (statusCode == StatusCodes.Failed)
                {
                    /*
                     * Email goes to all those listed for when the experiment fails
                     */
                    if (this.emailAddressesExperimentFailed != null && this.emailAddressesExperimentFailed.Length > 0)
                    {
                        if (mailMessage == null)
                        {
                            mailMessage = new MailMessage();
                        }
                        for (int i = 0; i < this.emailAddressesExperimentFailed.Length; i++)
                        {
                            mailMessage.To.Add(new MailAddress(this.emailAddressesExperimentFailed[i]));
                        }
                        errorMessage = String.Format(STRLOG_MailMessageError_arg, experimentInfo.resultReport.errorMessage);
                    }
                }
                else
                {
                    /*
                     * Email goes to all those listed for when the experiment completes successfully or is cancelled
                     */
                    if (this.emailAddressesExperimentCompleted != null && this.emailAddressesExperimentCompleted.Length > 0)
                    {
                        if (mailMessage == null)
                        {
                            mailMessage = new MailMessage();
                        }
                        for (int i = 0; i < this.emailAddressesExperimentCompleted.Length; i++)
                        {
                            mailMessage.To.Add(new MailAddress(this.emailAddressesExperimentCompleted[i]));
                        }
                    }
                }

                /*
                 * Check if recipient email addresses have been specified
                 */
                if (mailMessage != null)
                {
                    mailMessage.From = new MailAddress(this.emailAddressLabServer);
                    mailMessage.Subject = String.Format(STRLOG_MailMessageSubject_arg2, this.labConfiguration.Title, statusCode);
                    mailMessage.Body = String.Format(STRLOG_MailMessageBody_arg7,
                        experimentInfo.sbName, experimentInfo.userGroup, experimentInfo.experimentId, this.unitId, experimentInfo.setupId, statusCode, errorMessage);

                    Logfile.Write(
                        String.Format(STRLOG_SendingEmail_arg3, mailMessage.To.ToString(), mailMessage.From.Address, mailMessage.Subject));

                    /*
                     * Send the email
                     */
                    SmtpClient smtpClient = new SmtpClient(Consts.STR_LocalhostIP);
                    smtpClient.Send(mailMessage);
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }
    }
}
