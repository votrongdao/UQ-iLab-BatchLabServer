using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Engine
{
    public class LabEquipmentEngine : IDisposable
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "LabEquipmentEngine";

        //
        // Constants
        //
        private const int DELAY_MS_SignalCommandCheck = 5000;
        private const int DELAY_MS_SignalResultCheck = 1000;

        //
        // String constants
        //
        protected const string STR_NotInitialised = "Not Initialised!";
        protected const string STR_PoweringUp = "Powering up ...";
        protected const string STR_Initialising = "Initialising ...";
        protected const string STR_PoweringDown = "Powering down ...";
        protected const string STR_Ready = "Ready";
        protected const string STR_PoweredDown = "Powered down";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_Filename = " Filename: ";
        private const string STRLOG_ParsingEquipmentConfig = " Parsing EquipmentConfig...";
        private const string STRLOG_Title = " Title: ";
        private const string STRLOG_Version = " Version: ";

        private const string STRLOG_PowerupDelayNotSpecified = "Powerup delay is not specified!";
        private const string STRLOG_PowerdownTimeoutNotSpecified = "Powerdown timeout is not specified!";
        private const string STRLOG_PowerdownDisabled = " Powerdown: Disabled";
        private const string STRLOG_LabEquipmentEngineThreadIsStarting = " LabEquipmentEngine thread is starting...";
        private const string STRLOG_LabEquipmentEngineThreadIsRunning = " LabEquipmentEngine thread is running.";

        protected const string STRLOG_disposing = " disposing: ";
        protected const string STRLOG_disposed = " disposed: ";
        protected const string STRLOG_PowerupDelay = " Powerup Delay: ";
        protected const string STRLOG_PowerdownTimeout = " Powerdown Timeout: ";
        protected const string STRLOG_PoweroffDelay = " PowerOff Delay: ";
        protected const string STRLOG_InitialiseDelay = " Initialise Delay: ";
        protected const string STRLOG_TimeUntilReady = " TimeUntilReady: ";
        protected const string STRLOG_Seconds = " seconds";
        protected const string STRLOG_Success = "Success: ";
        protected const string STRLOG_ErrorMessage = " ErrorMessage: ";
        protected const string STRLOG_Online = " Online: ";
        protected const string STRLOG_StatusMessage = " StatusMessage: ";

        //
        // String constants for exception messages
        //
        private const string STRERR_NumberIsNegative = "Number cannot be negative!";
        private const string STRERR_NumberIsInvalid = "Number is invalid!";
        private const string STRERR_executeCommandLock = "executeCommandLock";
        private const string STRERR_signalCommand = "signalCommand";
        private const string STRERR_signalResult = "signalResult";
        private const string STRERR_statusLock = "statusLock";
        private const string STRERR_threadLabEquipmentEngine = "threadLabEquipmentEngine";
        private const string STRERR_LabEquipmentEngineThreadFailedToStart = "Lab equipment engine thread failed to start!";
        private const string STRERR_LabEquipmentFailedToBecomeReady = "Lab equipment failed to become ready!";
        private const string STRERR_InitiliaseFailedRetrying_arg2 = "Lab equipment failed to initialise! Retrying {0} of {1}...";
        private const string STRERR_InitiliaseFailedAfterRetrying_arg = "Lab equipment failed to initialise after {0} attempts!";
        protected const string STRERR_UnknownCommand = " Unknown Command: ";
        protected const string STRERR_UnknownSetupId = " Unknown SetupId: ";

        //
        // Local variables
        //
        private Logfile.LoggingLevels logLevel;
        private bool disposed;
        private Object statusLock;
        private Thread threadLabEquipmentEngine;
        private Object executeCommandLock;
        private Object signalCommand;
        private Object signalResult;
        private ExecuteCommandInfo executeCommandInfo;
        private ExecuteCommandInfo executeResultInfo;

        //
        // These need to be locked by 'statusLock'
        //
        private int slPowerupTimeRemaining;
        private int slPowerdownTimeRemaining;
        private DateTime slPowerupInitialiseStartTime;
        protected bool slOnline;
        protected string slStatusMessage;

        //
        // Local variables accessible by a derived class
        //
        protected string rootFilePath;
        protected XmlNode xmlNodeEquipmentConfig;

        //
        // Constants
        //

        /// <summary>
        /// Time in seconds to wait after the equipment is powered up if not already specified.
        /// </summary>
        private const int DEFAULT_DelayPowerup = 5;

        /// <summary>
        /// Time in seconds to wait for equipment finalisation.
        /// </summary>
        private const int DEFAULT_DelayFinalise = 3;

        /// <summary>
        /// Minimum time in seconds to wait to powerup the equipment after it has been powered down.
        /// </summary>
        private const int MIN_DelayPoweroff = 10;

        private const int RETRY_PowerOnInit = 2;

        #endregion

        #region Public Properties

        private string filename;
        private string title;
        private string version;
        private bool powerdownEnabled;
        private int powerupDelay;
        private int powerdownTimeout;
        private int poweroffDelay;
        protected int powerupInitialiseDelay;
        private int finaliseDelay;
        private bool debug_Retry;

        public string Filename
        {
            get { return this.filename; }
        }

        public string Title
        {
            get { return this.title; }
        }

        public string Version
        {
            get { return this.version; }
        }

        /// <summary>
        /// True if the powerup delay and powerdown timeout have been specified
        /// in the Application's configuration file and are valid.
        /// </summary>
        public bool PowerdownEnabled
        {
            get { return this.powerdownEnabled; }
        }

        /// <summary>
        /// Time in seconds for the equipment to become ready to initialise after power has been applied.
        /// </summary>
        public int PowerupDelay
        {
            get { return this.powerupDelay; }
            set { this.powerupDelay = value; }
        }

        /// <summary>
        /// Time in seconds for the equipment to initialise after power has been applied.
        /// </summary>
        public int InitialiseDelay
        {
            get { return this.powerupInitialiseDelay; }
        }

        /// <summary>
        /// Time in seconds for the equipment to finalise before power is removed.
        /// </summary>
        public int FinaliseDelay
        {
            get { return this.finaliseDelay; }
            set { this.finaliseDelay = value; }
        }

        /// <summary>
        /// Time in seconds of inactivity that the equipment will wait before power is removed.
        /// </summary>
        public int PowerdownTimeout
        {
            get { return this.powerdownTimeout; }
        }

        /// <summary>
        /// Time in seconds to wait after equipment power is removed before it can be applied again.
        /// </summary>
        public int PoweroffDelay
        {
            get { return this.poweroffDelay; }
        }

        /// <summary>
        /// Time in seconds remaining before the equipment powers down.
        /// </summary>
        public int TimeUntilPowerdown
        {
            get
            {
                lock (this.statusLock)
                {
                    return this.slPowerdownTimeRemaining;
                }
            }
        }

        /// <summary>
        /// True if powerdown has been suspended
        /// </summary>
        public bool IsPowerdownSuspended
        {
            get
            {
                lock (this.statusLock)
                {
                    return this.slPowerdownIsSuspended;
                }
            }
        }

        public bool Debug_Retry
        {
            get { return this.debug_Retry; }
            set { this.debug_Retry = value; }
        }

        #endregion

        #region Private Properties

        //
        // These need to be locked by 'statusLock'
        //
        private bool slRunning;
        private bool slIsReady;
        private bool slPowerdownSuspended;
        private bool slPowerdownIsSuspended;

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

        /// <summary>
        /// True if the equipment is powered up and ready to use.
        /// </summary>
        private bool IsReady
        {
            get
            {
                lock (this.statusLock)
                {
                    return this.slIsReady;
                }
            }
        }

        /// <summary>
        /// True if powerdown has been suspended
        /// </summary>
        private bool PowerdownSuspended
        {
            get
            {
                lock (this.statusLock)
                {
                    return this.slPowerdownSuspended;
                }
            }
            set
            {
                lock (this.statusLock)
                {
                    this.slPowerdownSuspended = value;
                }
            }
        }

        public bool PowerdownIsSuspended
        {
            get
            {
                lock (this.statusLock)
                {
                    return this.slPowerdownIsSuspended;
                }
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public LabEquipmentEngine(string rootFilePath)
            : this(rootFilePath, null, null)
        {
        }

        //-------------------------------------------------------------------------------------------------//

        public LabEquipmentEngine(string rootFilePath, string xmlEquipmentConfig, string equipmentConfigFilename)
        {
            const string STRLOG_MethodName = "LabEquipmentEngine";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Initialise local variables
            //
            this.disposed = true;
            this.rootFilePath = rootFilePath;

            //
            // Initialise properties
            //
            this.powerupInitialiseDelay = 0;
            this.slRunning = false;
            this.slIsReady = false;
            this.slPowerdownSuspended = false;
            this.slOnline = false;
            this.slStatusMessage = STR_NotInitialised;
            this.debug_Retry = false;

            //
            // Determine the logging level for this class
            //
            try
            {
                this.logLevel = (Logfile.LoggingLevels)Utilities.GetIntAppSetting(STRLOG_ClassName);
            }
            catch
            {
                this.logLevel = Logfile.LoggingLevels.Minimum;
            }
            Logfile.Write(Logfile.STRLOG_LogLevel + this.logLevel.ToString());

            try
            {
                XmlDocument xmlDocument;

                //
                // Check if an XML equipment configuration string is specified
                //
                if (xmlEquipmentConfig != null)
                {
                    //
                    // Load the equipment configuration from an XML string
                    //
                    xmlDocument = XmlUtilities.GetXmlDocument(xmlEquipmentConfig);
                }
                else
                {
                    //
                    // Check if an XML equipment configuration filename is specified
                    //
                    if (equipmentConfigFilename == null)
                    {
                        //
                        // Get equipment configuration filename from Application's configuration file
                        //
                        this.filename = Utilities.GetAppSetting(Consts.STRCFG_XmlEquipmentConfigFilename);
                        this.filename = Path.Combine(this.rootFilePath, this.filename);
                    }
                    else
                    {
                        // Prepend full file path
                        this.filename = Path.Combine(this.rootFilePath, equipmentConfigFilename);
                    }

                    Logfile.Write(STRLOG_Filename + this.filename);

                    // Load the equipment configuration from the specified file
                    xmlDocument = XmlUtilities.GetXmlDocumentFromFile(this.filename);
                }

                //
                // Get the equipment configuration XML node and save a copy
                //
                XmlNode xmlNode = XmlUtilities.GetXmlRootNode(xmlDocument, Consts.STRXML_equipmentConfig);
                this.xmlNodeEquipmentConfig = xmlNode.Clone();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.Write(STRLOG_ParsingEquipmentConfig);

            //
            // Get information from the equipment configuration node
            //
            this.title = XmlUtilities.GetXmlValue(this.xmlNodeEquipmentConfig, Consts.STRXMLPARAM_title, false);
            this.version = XmlUtilities.GetXmlValue(this.xmlNodeEquipmentConfig, Consts.STRXMLPARAM_version, false);

            Logfile.Write(STRLOG_Title + this.title);
            Logfile.Write(STRLOG_Version + this.version);

            //
            // Get powerup delay, may not be specified
            //
            try
            {
                this.powerupDelay = XmlUtilities.GetIntValue(this.xmlNodeEquipmentConfig, Consts.STRXML_powerupDelay);
                if (this.powerupDelay < 0)
                {
                    throw new ArgumentException(STRERR_NumberIsNegative);
                }
            }
            catch (ArgumentNullException)
            {
                Logfile.Write(STRLOG_PowerupDelayNotSpecified);

                // Powerup delay is not specified, set default powerup delay
                this.powerupDelay = DEFAULT_DelayPowerup;
            }
            catch (FormatException)
            {
                // Value cannot be converted
                throw new ArgumentException(STRERR_NumberIsInvalid, Consts.STRXML_powerupDelay);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw new ArgumentException(ex.Message, Consts.STRXML_powerupDelay);
            }
            this.slPowerupTimeRemaining = this.powerupDelay;

            //
            // Get powerdown timeout, may not be specified
            //
            try
            {
                this.powerdownTimeout = XmlUtilities.GetIntValue(this.xmlNodeEquipmentConfig, Consts.STRXML_powerdownTimeout);
                if (this.powerdownTimeout < 0)
                {
                    // Value cannot be negative
                    throw new Exception(STRERR_NumberIsNegative);
                }

                //
                // Powerdown timeout is specified so enable powerdown
                //
                this.powerdownEnabled = true;
                this.poweroffDelay = MIN_DelayPoweroff;
            }
            catch (ArgumentNullException)
            {
                Logfile.Write(STRLOG_PowerdownTimeoutNotSpecified);

                //
                // Powerdown timeout is not specified, disable powerdown
                //
                this.powerdownEnabled = false;
                this.poweroffDelay = 0;
            }
            catch (FormatException)
            {
                // Value cannot be converted
                throw new ArgumentException(STRERR_NumberIsInvalid, Consts.STRXML_powerdownTimeout);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw new ArgumentException(ex.Message, Consts.STRXML_powerdownTimeout);
            }
            this.slPowerdownTimeRemaining = this.powerdownTimeout;

            //
            // Log details
            //
            string logMessage = STRLOG_PowerupDelay + this.powerupDelay.ToString() + STRLOG_Seconds;

            if (this.powerdownEnabled == true)
            {
                logMessage += Logfile.STRLOG_Spacer + STRLOG_PowerdownTimeout + this.PowerdownTimeout.ToString() + STRLOG_Seconds +
                    Logfile.STRLOG_Spacer + STRLOG_PoweroffDelay + this.PoweroffDelay + STRLOG_Seconds;
            }
            else
            {
                logMessage += Logfile.STRLOG_Spacer + STRLOG_PowerdownDisabled;
            }
            Logfile.Write(logMessage);

            try
            {
                //
                // Create thread objects
                //
                this.executeCommandLock = new Object();
                if (this.executeCommandLock == null)
                {
                    throw new ArgumentNullException(STRERR_executeCommandLock);
                }
                this.signalCommand = new Object();
                if (this.signalCommand == null)
                {
                    throw new ArgumentNullException(STRERR_signalCommand);
                }
                this.signalResult = new Object();
                if (this.signalResult == null)
                {
                    throw new ArgumentNullException(STRERR_signalResult);
                }
                this.statusLock = new Object();
                if (this.statusLock == null)
                {
                    throw new ArgumentNullException(STRERR_statusLock);
                }
                this.threadLabEquipmentEngine = new Thread(new ThreadStart(LabEquipmentEngineThread));
                if (this.threadLabEquipmentEngine == null)
                {
                    throw new ArgumentNullException(STRERR_threadLabEquipmentEngine);
                }

                //
                // Don't start the thread yet, the method Start() must be called to start the thread
                // after the derived class has completed its initialisation.
                //
                this.slOnline = true;
                this.slPowerupInitialiseStartTime = DateTime.Now;
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

        /// <summary>
        /// Start the lab experiment engine thread. Check that the thread has started and return true if
        /// successful. This method does not wait for the equipment to become ready.
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            const string STRLOG_MethodName = "Start";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            try
            {
                this.disposed = false;

                //
                // Start the lab equipment thread
                //
                Logfile.Write(STRLOG_LabEquipmentEngineThreadIsStarting);
                this.threadLabEquipmentEngine.Start();

                //
                // Give the thread a chance to start and then check that it has started
                //
                for (int i = 0; i < 5; i++)
                {
                    if ((success = this.Running) == true)
                    {
                        Logfile.Write(STRLOG_LabEquipmentEngineThreadIsRunning);
                        break;
                    }

                    Thread.Sleep(500);
                    Trace.Write('?');
                }
                if (success == false)
                {
                    throw new ArgumentException(STRERR_LabEquipmentEngineThreadFailedToStart);
                }
            }
            catch (ThreadStateException ex)
            {
                Logfile.Write(ex.Message);
            }

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Suspend equipment powerdown. If the equipment is already powered down, the equipment is
        /// powered up. Wait until the equipment is powered up and ready to use before returning.
        /// </summary>
        public bool SuspendPowerdown()
        {
            const string STRLOG_MethodName = "SuspendPowerdown";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            if (this.powerdownEnabled == true)
            {
                //
                // Start by suspending equipment powerdown
                //
                this.PowerdownSuspended = true;

                //
                // Check if thread is still running
                //
                if (this.Running == false)
                {
                    try
                    {
                        //
                        // Create a new thread and start it
                        //
                        Logfile.Write(STRLOG_LabEquipmentEngineThreadIsStarting);
                        this.threadLabEquipmentEngine = new Thread(new ThreadStart(LabEquipmentEngineThread));
                        this.threadLabEquipmentEngine.Start();

                        //
                        // Give the thread a chance to start and then check that it has started
                        //
                        for (int i = 0; i < 5; i++)
                        {
                            if ((success = this.Running) == true)
                            {
                                Logfile.Write(STRLOG_LabEquipmentEngineThreadIsRunning);
                                break;
                            }

                            Thread.Sleep(500);
                            Trace.Write('?');
                        }
                        if (success == false)
                        {
                            throw new ArgumentException(STRERR_LabEquipmentEngineThreadFailedToStart);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logfile.Write(ex.Message);
                        success = false;
                    }
                }

                //
                // Wait for the equipment to powerup if necessary and become ready to use.
                // Use a timeout so that we don't wait forever if something goes wrong.
                //
                if (success == true)
                {
                    int timeout = this.GetTimeUntilReady() + 5;
                    do
                    {
                        Thread.Sleep(1000);
                        Trace.Write("?");
                    } while (this.PowerdownIsSuspended == false && --timeout > 0);

                    if (timeout == 0)
                    {
                        // Equipment failed to become ready ???
                        Logfile.WriteError(STRERR_LabEquipmentFailedToBecomeReady);
                        success = false;
                    }
                }
            }

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Resume equipment powerdown.
        /// </summary>
        public bool ResumePowerdown()
        {
            const string STRLOG_MethodName = "ResumePowerdown";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            if (this.powerdownEnabled == true && this.disposed == false)
            {
                //
                // The equipment may already be powered down, doesn't matter
                //
                this.PowerdownSuspended = false;
            }

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            return true;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Return the time in seconds before the equipment engine is ready to execute commands
        /// after the equipment has been powered up.
        /// </summary>
        public int GetTimeUntilReady()
        {
            const string STRLOG_MethodName = "GetTimeUntilReady";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            int timeUntilReady = 0;
            string logMessage = string.Empty;

            lock (this.statusLock)
            {
                if (this.slIsReady == true)
                {
                    // Equipment is powered up and ready to use
                    logMessage += "Ready";
                }
                else if (this.slRunning == false)
                {
                    if (this.slOnline == true)
                    {
                        // Equipment is powered down
                        timeUntilReady = this.powerupDelay + this.powerupInitialiseDelay;
                        logMessage += "Not Running";
                    }
                    else
                    {
                        // Equipment failed to powerup or initialise
                        timeUntilReady = -1;
                    }
                }
                else if (this.slPowerupTimeRemaining > this.powerupDelay)
                {
                    // Equipment has powered off
                    timeUntilReady = this.slPowerupTimeRemaining + this.powerupInitialiseDelay;
                    logMessage += "Poweroff";
                }
                else if (this.slPowerupTimeRemaining > 0)
                {
                    // Equipment is still powering up
                    timeUntilReady = this.slPowerupTimeRemaining + this.powerupInitialiseDelay;
                    logMessage += "Powerup";
                }
                else
                {
                    //
                    // Equipment has powered up and is initialising
                    //
                    TimeSpan timeSpan = DateTime.Now - this.slPowerupInitialiseStartTime;
                    timeUntilReady = this.powerupInitialiseDelay;
                    try
                    {
                        // Get the time to complete initialisation
                        timeUntilReady -= Convert.ToInt32(timeSpan.TotalSeconds);

                        //
                        // Don't say initialisation has completed until it actually has
                        //
                        if (timeUntilReady < 1)
                        {
                            timeUntilReady = 1;
                        }
                    }
                    catch
                    {
                    }
                    logMessage += "Initialise";
                }

                logMessage = STRLOG_TimeUntilReady + timeUntilReady.ToString() + Logfile.STRLOG_Spacer + logMessage;
            }

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return timeUntilReady;
        }

        //-------------------------------------------------------------------------------------------------//

        public ExecuteCommandInfo ExecuteCommand(ExecuteCommandInfo executeCommandInfo)
        {
            const string STRLOG_MethodName = "ExecuteCommand";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            if (this.disposed == false)
            {
                //
                // Determine the timeout for waiting on the result
                //
                int timeout = executeCommandInfo.timeout;
                timeout = timeout * 1000 / DELAY_MS_SignalResultCheck;

                //
                // Save the info and signal LabEquipmentEngine thread to execute the command
                //
                lock (this.signalCommand)
                {
                    lock (this.executeCommandLock)
                    {
                        this.executeResultInfo = null;
                        this.executeCommandInfo = executeCommandInfo;
                    }
                    Monitor.Pulse(this.signalCommand);
                }

                //
                // Wait for the LabEquipmentEngine thread to execute the command and return the results
                //
                do
                {
                    lock (this.signalResult)
                    {
                        if (Monitor.Wait(this.signalResult, DELAY_MS_SignalResultCheck) == true)
                        {
                            Trace.WriteLine("Result received...");
                        }
                    }

                    lock (this.executeCommandLock)
                    {
                        if (this.executeResultInfo != null)
                        {
                            break;
                        }
                    }
                }
                while (--timeout > 0);
            }

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            return this.executeResultInfo;
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual LabStatus GetLabEquipmentStatus()
        {
            const string STRLOG_MethodName = "GetLabEquipmentStatus";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            LabStatus labStatus = new LabStatus();
            lock (this.statusLock)
            {
                labStatus.online = this.slOnline;
                labStatus.labStatusMessage = this.slStatusMessage;
            }

            string logMessage = STRLOG_Online + labStatus.online.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_StatusMessage + Logfile.STRLOG_Quote + labStatus.labStatusMessage + Logfile.STRLOG_Quote;

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return labStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual bool PowerupEquipment()
        {
            const string STRLOG_MethodName = "PowerupEquipment";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            //
            // Do nothing here, this will be overridden
            //

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual bool InitialiseEquipment()
        {
            const string STRLOG_MethodName = "InitialiseEquipment";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            //
            // Powerup initialisation delay, this will be overridden
            //
            for (int i = 0; i < this.powerupInitialiseDelay; i++)
            {
                Thread.Sleep(1000);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual bool PowerdownEquipment()
        {
            const string STRLOG_MethodName = "PowerdownEquipment";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            //
            // Do nothing here, this will be overridden
            //

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual ExecuteCommandInfo ProcessCommand(ExecuteCommandInfo executeCommandInfo)
        {
            const string STRLOG_MethodName = "ProcessCommand";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;
            string errorMessage = null;

            //
            // Process the execute command
            //
            ExecuteCommands executeCommand = (ExecuteCommands)executeCommandInfo.command;

            switch (executeCommand)
            {
                case ExecuteCommands.SetTime:

                    //
                    // Pretending to change the time, so nothing to do here
                    //
                    DateTime dateTime = (DateTime)executeCommandInfo.parameters[0];
                    Logfile.Write(" parameters: dateTime -> " + dateTime.ToString());

                    //
                    // Create some dummy results
                    //
                    executeCommandInfo.results = new object[] { DateTime.Now };
                    dateTime = (DateTime)executeCommandInfo.results[0];
                    Logfile.Write(" results: dateTime -> " + dateTime.ToString());
                    break;

                default:
                    //
                    // Unknown command
                    //
                    errorMessage = STRERR_UnknownCommand + executeCommand.ToString();
                    success = false;
                    break;
            }

            //
            // Update success of command execution
            //
            executeCommandInfo.success = success;

            string logMessage = STRLOG_Success + success.ToString();
            if (success == false)
            {
                executeCommandInfo.errorMessage = errorMessage;
                logMessage += Logfile.STRLOG_Spacer + STRLOG_ErrorMessage + errorMessage;
            }

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return executeCommandInfo;
        }

        //-------------------------------------------------------------------------------------------------//

        #region Close and Dispose

        /// <summary>
        /// Do not make this method virtual. A derived class should not be allowed to override this method.
        /// </summary>
        public void Close()
        {
            const string STRLOG_MethodName = "Close";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            // Calls the Dispose method without parameters
            Dispose();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
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
        ~LabEquipmentEngine()
        {
            Trace.WriteLine("~LabEquipmentEngine():");

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
                Logfile.STRLOG_Spacer + STRLOG_disposed + this.disposed.ToString();

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

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
                // Stop the LabEquipmentEngine thread
                //
                if (this.Running == true)
                {
                    this.Running = false;
                    this.threadLabEquipmentEngine.Join();

                    //
                    // Powerdown the equipment
                    //
                    PowerdownEquipment();
                }

                this.disposed = true;
            }

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
        }

        #endregion

        //=================================================================================================//

        private enum States
        {
            sPowerInit, sPowerUpDelay, sPowerOnInit, sPowerOnReady,
            sPowerSuspended, sProcessCommand,
            sPowerOffDelay, sPowerOff, sExitThread
        }

        //-------------------------------------------------------------------------------------------------//

        private void LabEquipmentEngineThread()
        {
            const string STRLOG_MethodName = "LabEquipmentEngineThread";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            //
            // Initialise state machine
            //
            int retryPowerOnInit = RETRY_PowerOnInit;
            bool retry = false;
            States state = States.sPowerInit;
            States lastState = States.sExitThread;
            this.slOnline = true;
            this.Running = true;

            try
            {
                //
                // State machine loop
                //
                while (this.Running == true)
                {
                    //
                    // Display message on each state change
                    //
                    if (state != lastState)
                    {
                        string logMessage = " [ " + STRLOG_MethodName + ": " + lastState.ToString() + " -> " + state.ToString() + " ]";
                        Logfile.Write(this.logLevel, logMessage);
                        Trace.WriteLine(logMessage);

                        lastState = state;
                    }

                    switch (state)
                    {
                        case States.sPowerInit:

                            lock (this.statusLock)
                            {
                                this.slStatusMessage = STR_PoweringUp;

                                // Initialise powerup delay
                                this.slPowerupTimeRemaining = this.powerupDelay;
                            }

                            // Powerup the equipment 
                            if (PowerupEquipment() == false)
                            {
                                this.slOnline = false;
                                this.slStatusMessage = STR_NotInitialised;
                                state = States.sExitThread;
                                break;
                            }

                            state = States.sPowerUpDelay;
                            break;

                        case States.sPowerUpDelay:

                            // Wait a bit
                            Thread.Sleep(1000);

                            lock (this.statusLock)
                            {
                                // Check timeout
                                if (--this.slPowerupTimeRemaining > 0)
                                {
                                    // Equipment is still powering up
                                    Trace.Write("u");
                                    continue;
                                }
                            }

                            // Equipment is now powered up
                            state = States.sPowerOnInit;
                            break;

                        case States.sPowerOnInit:

                            // Set the power intialisation start time
                            lock (this.statusLock)
                            {
                                this.slStatusMessage = STR_Initialising;
                                this.slPowerupInitialiseStartTime = DateTime.Now;
                            }

                            //
                            // Initialise the equipment
                            //
                            retry = false;
                            if (InitialiseEquipment() == false || this.debug_Retry == true)
                            {
                                if (--retryPowerOnInit >= 0)
                                {
                                    retry = true;

                                    Logfile.WriteError(
                                        String.Format(STRERR_InitiliaseFailedRetrying_arg2, RETRY_PowerOnInit - retryPowerOnInit, RETRY_PowerOnInit));

                                    //
                                    // Equipment failed to initialise, powerdown the equipment and try again
                                    //
                                    lock (this.statusLock)
                                    {
                                        this.slPowerupTimeRemaining = this.PoweroffDelay + this.PowerupDelay;
                                    }
                                    PowerdownEquipment();
                                    state = States.sPowerOffDelay;
                                }
                                else
                                {
                                    Logfile.WriteError(
                                        String.Format(STRERR_InitiliaseFailedAfterRetrying_arg, RETRY_PowerOnInit + 1));

                                    //
                                    // Could initialise the equipment after retrying, powerdown the equipment and exit
                                    //
                                    this.slOnline = false;
                                    this.slStatusMessage = STR_NotInitialised;
                                    PowerdownEquipment();
                                    state = States.sExitThread;
                                }
                                break;
                            }

                            lock (this.statusLock)
                            {
                                // Equipment is now ready to use
                                this.slStatusMessage = STR_Ready;
                                this.slIsReady = true;

                                // Initialise powerdown timeout
                                this.slPowerdownTimeRemaining = this.powerdownTimeout;
                            }

                            // Check if powerdown is enabled
                            if (this.powerdownEnabled == false)
                            {
                                this.PowerdownSuspended = true;
                            }
                            else
                            {
                                // Log the time remaining before power is removed
                                LogPowerDown(this.powerdownTimeout, true);
                            }
                            state = States.sPowerOnReady;
                            break;

                        case States.sPowerOnReady:

                            // Wait a bit
                            Thread.Sleep(1000);

                            //
                            // Check if equipment powerdown should be suspended
                            //
                            if (this.PowerdownSuspended == true)
                            {
                                lock (this.statusLock)
                                {
                                    this.slPowerdownIsSuspended = true;
                                }
                                state = States.sPowerSuspended;
                                break;
                            }

                            //
                            // Log the time remaining before power is removed
                            //
                            int timeRemaining;
                            lock (this.statusLock)
                            {
                                timeRemaining = this.slPowerdownTimeRemaining;
                            }
                            LogPowerDown(timeRemaining);

                            lock (this.statusLock)
                            {
                                // Check timeout
                                if (--this.slPowerdownTimeRemaining > 0)
                                {
                                    // Timeout is still counting down
                                    Trace.Write("t");
                                    continue;
                                }

                                //
                                // Equipment is powering down, determine the time before the equipment
                                // can be powered up again
                                //
                                this.slPowerupTimeRemaining = this.PoweroffDelay + this.PowerupDelay;
                                this.slIsReady = false;
                                this.slStatusMessage = STR_PoweringDown;
                            }

                            // Powerdown the equipment
                            PowerdownEquipment();

                            state = States.sPowerOffDelay;
                            break;

                        case States.sPowerSuspended:

                            //
                            // Check if there is a command to execute
                            //
                            lock (this.signalCommand)
                            {
                                if (Monitor.Wait(this.signalCommand, DELAY_MS_SignalCommandCheck) == true)
                                {
                                    Trace.WriteLine("Command received...");
                                }

                                //
                                // Whether we timed out or not, check for a command
                                //
                                lock (this.executeCommandLock)
                                {
                                    if (this.executeCommandInfo != null)
                                    {
                                        state = States.sProcessCommand;
                                        break;
                                    }
                                }
                            }

                            // Check if equipment powerdown is resumed
                            if (this.PowerdownSuspended == false)
                            {
                                // Log the time remaining before power is removed
                                LogPowerDown(this.powerdownTimeout, true);

                                lock (this.statusLock)
                                {
                                    // Reset the powerdown timeout
                                    this.slPowerdownTimeRemaining = this.powerdownTimeout;
                                    this.slPowerdownIsSuspended = false;
                                }
                                state = States.sPowerOnReady;
                            }
                            break;

                        case States.sProcessCommand:

                            ExecuteCommandInfo executeResultInfo = ProcessCommand(this.executeCommandInfo);

                            lock (this.signalResult)
                            {
                                lock (this.executeCommandLock)
                                {
                                    this.executeCommandInfo = null;
                                    this.executeResultInfo = executeResultInfo;
                                }
                                Monitor.Pulse(this.signalResult);
                            }

                            state = States.sPowerSuspended;
                            break;

                        case States.sPowerOffDelay:

                            // Wait a bit
                            Thread.Sleep(1000);

                            // Check timeout
                            lock (this.statusLock)
                            {
                                if (--this.slPowerupTimeRemaining > this.powerupDelay)
                                {
                                    // Equipment is still powering off
                                    Trace.Write("o");
                                }
                                else
                                {
                                    // Check if powerup has been requested
                                    if (this.slPowerdownSuspended == true)
                                    {
                                        state = States.sPowerInit;
                                    }
                                    else
                                    {
                                        // Powerdown has completed
                                        this.slStatusMessage = STR_PoweredDown;

                                        //
                                        // Check if retrying
                                        //
                                        if (retry == true)
                                        {
                                            state = States.sPowerInit;
                                        }
                                        else
                                        {
                                            state = States.sExitThread;
                                        }
                                    }
                                }
                            }
                            break;

                        case States.sExitThread:

                            //
                            // Thread is no longer running
                            //
                            this.Running = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Trace.WriteLine(STRLOG_MethodName + ": Exiting");

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        private void LogPowerDown(int seconds)
        {
            LogPowerDown(seconds, false);
        }

        //-------------------------------------------------------------------------------------------------//

        private void LogPowerDown(int seconds, bool now)
        {
            const string STR_PowerdownIn = " Powerdown in ";
            const string STR_Minute = " minute";
            const string STR_And = " and ";
            const string STR_Second = " second";

            int minutes = seconds / 60;
            if (now == true && seconds > 0)
            {
                // Log message now
                string message = STR_PowerdownIn;
                seconds %= 60;
                if (minutes > 0)
                {
                    message += minutes.ToString() + STR_Minute + ((minutes != 1) ? "s" : "");
                    if (seconds != 0)
                    {
                        message += STR_And;
                    }
                }
                if (seconds != 0)
                {
                    message += seconds.ToString() + STR_Second + ((seconds != 1) ? "s" : "");
                }
                Logfile.Write(message);
            }
            else
            {
                if (minutes > 5)
                {
                    if (seconds % (5 * 60) == 0)
                    {
                        // Log message every 5 minutes
                        Logfile.Write(STR_PowerdownIn + minutes.ToString() + STR_Minute + ((minutes != 1) ? "s" : ""));
                    }
                }
                else if (seconds > 5)
                {
                    if (seconds % 60 == 0 && seconds != 0)
                    {
                        // Log message every minute
                        Logfile.Write(STR_PowerdownIn + minutes.ToString() + STR_Minute + ((minutes != 1) ? "s" : ""));
                    }
                }
                else if (seconds > 0)
                {
                    // Log message every second
                    Logfile.Write(STR_PowerdownIn + seconds.ToString() + STR_Second + ((seconds != 1) ? "s" : ""));
                }
            }
        }

    }
}
