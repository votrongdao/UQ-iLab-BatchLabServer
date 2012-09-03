using System;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Engine.Drivers
{
    public class DriverGeneric
    {
        #region Constants

        private const string STRLOG_ClassName = "DriverGeneric";

        //
        // Constants
        //
        private const int DELAY_MS_SignalRunningCheck = 5000;

        //
        // String constants for logfile messages
        //
        protected const string STRLOG_NotInitialised = " Not Initialised! ";
        protected const string STRLOG_Initialising = " Initialising... ";
        protected const string STRLOG_Initialised = " Initialised. ";
        protected const string STRLOG_Online = " Online: ";
        protected const string STRLOG_Success = " Success: ";
        protected const string STRLOG_DriverThreadIsStarting = " Driver thread is starting...";
        protected const string STRLOG_DriverThreadIsRunning = " Driver thread is running.";
        protected const string STRLOG_ExecutingStatus = " Executing Status: ";
        protected const string STRLOG_ResultStatus = " Result Status: ";
        protected const string STRLOG_ExecuteTimes_Fmt = " ExecuteTimes: Initialise {0} - Start {1} - Run {2} - Stop {3} - Finalise {4}";
        protected const string STRLOG_ExecuteTime_Fmt = " ExecuteTime: {0:f01} secs";
        protected const string STRLOG_ExecutionTimeRemaining_arg = " ExecutionTimeRemaining: {0} secs";

        //
        // String constants for error messages
        //
        private const string STRERR_signalRunning = "signalRunning";
        private const string STRERR_DriverThreadFailedToStart = "Driver thread failed to start!";

        #endregion

        #region Types

        protected struct ExecutionTimes
        {
            public int initialise;
            public int start;
            public int run;
            public int stop;
            public int finalise;
        }

        public enum ExecutionStatus
        {
            None, Initialising, Starting, Running, Stopping, Finalising, Done, Completed, Failed
        }

        #endregion

        #region Variables

        //
        // Local variables
        //
        private static bool firstInstance = true;
        private Object signalRunning;
        private bool running;

        protected static bool initialised = false;
        protected Logfile.LoggingLevels logLevel;
        protected XmlNode xmlNodeEquipmentConfig;
        protected ExperimentSpecification specification;
        protected ExecutionTimes executionTimes;
        protected DateTime executionCompletionTime;

        #endregion

        #region Properties

        protected static bool online;
        protected static string statusMessage;
        protected static int initialiseDelay;
        protected ExecutionStatus executionStatus;
        protected ExecutionStatus executionResultStatus;
        protected string lastError;

        /// <summary>
        /// Time in seconds for the equipment to initialise after power has been applied.
        /// </summary>
        public int InitialiseDelay
        {
            get { return DriverGeneric.initialiseDelay; }
        }

        /// <summary>
        /// Returns true if the equipment has been initialised successfully and is ready for use.
        /// </summary>
        public bool Online
        {
            get { return DriverGeneric.online; }
        }

        public string StatusMessage
        {
            get { return DriverGeneric.statusMessage; }
        }

        public string LastError
        {
            get
            {
                string errorMsg = lastError;
                lastError = null;
                return errorMsg;
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public DriverGeneric(XmlNode xmlNodeEquipmentConfig, ExperimentSpecification specification)
        {
            const string STRLOG_MethodName = "DriverGeneric";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Initialise static variables
            //
            if (DriverGeneric.firstInstance == true)
            {
                DriverGeneric.initialised = false;
                DriverGeneric.online = false;
                DriverGeneric.statusMessage = STRLOG_NotInitialised;
                DriverGeneric.firstInstance = false;
            }

            //
            // Initialise local variables
            //
            this.xmlNodeEquipmentConfig = xmlNodeEquipmentConfig;
            this.specification = specification;
            this.lastError = null;
            this.running = false;
            this.executionStatus = ExecutionStatus.None;
            this.executionResultStatus = ExecutionStatus.None;

            //
            // Initialise execution times for this driver, won't used when overridden
            //
            this.executionTimes = new ExecutionTimes { initialise = 3, start = 5, run = 7, stop = 4, finalise = 2 };

            try
            {
                //
                // Create thread objects
                //
                if ((this.signalRunning = new Object()) == null)
                {
                    throw new ArgumentNullException(STRERR_signalRunning);
                }
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

        public int GetExecutionTimeRemaining()
        {
            int seconds = 0;

            switch (this.executionStatus)
            {
                case ExecutionStatus.None:
                    seconds = -1;
                    break;

                case ExecutionStatus.Done:
                case ExecutionStatus.Completed:
                case ExecutionStatus.Failed:
                    break;

                default:
                    //
                    // Get the time in seconds from now until the expected completion time
                    //
                    TimeSpan timeSpan = this.executionCompletionTime - DateTime.Now;
                    seconds = (int)timeSpan.TotalSeconds;

                    //
                    // Ensure seconds are greater than zero
                    //
                    if (seconds < 1)
                    {
                        seconds = 1;
                    }
                    break;
            }

            return seconds;
        }

        //-------------------------------------------------------------------------------------------------//

        public ExecutionStatus GetExecutionStatus()
        {
            return this.executionStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        public ExecutionStatus GetExecutionResultStatus()
        {
            return this.executionResultStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool Start()
        {
            const string STRLOG_MethodName = "Start";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            try
            {
                //
                // Start the driver thread
                //
                Logfile.Write(STRLOG_DriverThreadIsStarting);
                Thread t = new Thread(new ThreadStart(this.DriverThread));
                t.Start();

                //
                // Wait for the thread to start and then check that it has started
                //
                lock (this.signalRunning)
                {
                    if (Monitor.Wait(this.signalRunning, DELAY_MS_SignalRunningCheck) == true)
                    {
                        if ((success = this.running) == true)
                        {
                            Logfile.Write(STRLOG_DriverThreadIsRunning);
                        }
                    }
                }

                if (success == false)
                {
                    throw new ArgumentException(STRERR_DriverThreadFailedToStart);
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

        public virtual bool Initialise()
        {
            const string STRLOG_MethodName = "Initialise";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            if (DriverGeneric.initialised == false)
            {
                DriverGeneric.statusMessage = STRLOG_Initialising;

                try
                {
                    //
                    // Initialisation is complete
                    //
                    DriverGeneric.initialised = true;
                    DriverGeneric.online = true;
                    DriverGeneric.statusMessage = StatusCodes.Ready.ToString();
                }
                catch (Exception ex)
                {
                    Logfile.WriteError(ex.Message);
                }
            }

            string logMessage = STRLOG_Online + DriverGeneric.online.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return DriverGeneric.online;
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual int GetExecutionTime()
        {
            const string STRLOG_MethodName = "GetExecutionTime";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            int executionTime = 0;

            executionTime += this.executionTimes.initialise;
            executionTime += this.executionTimes.start;
            executionTime += this.executionTimes.run;
            executionTime += this.executionTimes.stop;
            executionTime += this.executionTimes.finalise;

            string logMessage = string.Format(STRLOG_ExecuteTime_Fmt, executionTime);

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual string GetExecutionResults()
        {
            const string STRLOG_MethodName = "GetExecutionResults";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            string xmlExperimentResult = "<experimentResult />";

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return xmlExperimentResult;
        }

        //-------------------------------------------------------------------------------------------------//

        protected virtual bool ExecuteInitialising()
        {
            const string STRLOG_MethodName = "ExecuteInitialising";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            //
            // Calculate execution completion time
            //
            int executionTime = 0;
            executionTime += this.executionTimes.initialise;
            executionTime += this.executionTimes.start;
            executionTime += this.executionTimes.run;
            executionTime += this.executionTimes.stop;
            executionTime += this.executionTimes.finalise;
            this.executionCompletionTime = DateTime.Now + TimeSpan.FromSeconds(executionTime);

            for (int i = 0; i < this.executionTimes.initialise; i++)
            {
                Trace.Write("i");
                Thread.Sleep(1000);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        protected virtual bool ExecuteStarting()
        {
            const string STRLOG_MethodName = "ExecuteStarting";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            //
            // Calculate execution completion time
            //
            int executionTime = 0;
            executionTime += this.executionTimes.start;
            executionTime += this.executionTimes.run;
            executionTime += this.executionTimes.stop;
            executionTime += this.executionTimes.finalise;
            this.executionCompletionTime = DateTime.Now + TimeSpan.FromSeconds(executionTime);

            for (int i = 0; i < this.executionTimes.start; i++)
            {
                Trace.Write("s");
                Thread.Sleep(1000);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        protected virtual bool ExecuteRunning()
        {
            const string STRLOG_MethodName = "ExecuteRunning";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            //
            // Calculate execution completion time
            //
            int executionTime = 0;
            executionTime += this.executionTimes.run;
            executionTime += this.executionTimes.stop;
            executionTime += this.executionTimes.finalise;
            this.executionCompletionTime = DateTime.Now + TimeSpan.FromSeconds(executionTime);

            for (int i = 0; i < this.executionTimes.run; i++)
            {
                Trace.Write("r");
                Thread.Sleep(1000);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        protected virtual bool ExecuteStopping()
        {
            const string STRLOG_MethodName = "ExecuteStopping";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            //
            // Calculate execution completion time
            //
            int executionTime = 0;
            executionTime += this.executionTimes.stop;
            executionTime += this.executionTimes.finalise;
            this.executionCompletionTime = DateTime.Now + TimeSpan.FromSeconds(executionTime);

            for (int i = 0; i < this.executionTimes.stop; i++)
            {
                Trace.Write("p");
                Thread.Sleep(1000);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        protected virtual bool ExecuteFinalising()
        {
            const string STRLOG_MethodName = "ExecuteFinalising";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            //
            // Calculate execution completion time
            //
            int executionTime = 0;
            executionTime += this.executionTimes.finalise;
            this.executionCompletionTime = DateTime.Now + TimeSpan.FromSeconds(executionTime);

            for (int i = 0; i < this.executionTimes.finalise; i++)
            {
                Trace.Write("f");
                Thread.Sleep(1000);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //=================================================================================================//

        private void DriverThread()
        {
            const string STRLOG_MethodName = "DriverThread";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            Trace.WriteLine("DriverGeneric.DriverThread:");

            try
            {
                //
                // Initialise state machine
                //
                this.lastError = null;
                this.executionStatus = ExecutionStatus.Initialising;
                this.executionResultStatus = ExecutionStatus.None;
                this.running = true;

                //
                // Wait a moment before signalling
                //
                Thread.Sleep(1000);
                lock (this.signalRunning)
                {
                    Monitor.Pulse(this.signalRunning);
                }

                //
                // State machine loop
                //
                while (this.running == true)
                {
                    int seconds;

                    //Trace.WriteLine("executingStatus: " + this.executingStatus.ToString());

                    switch (this.executionStatus)
                    {
                        case ExecutionStatus.Initialising:
                            //
                            // Determine the time at which execution is expected to complete
                            //
                            seconds = this.executionTimes.initialise + this.executionTimes.start + this.executionTimes.run +
                                this.executionTimes.stop + this.executionTimes.finalise;
                            this.executionCompletionTime = DateTime.Now + TimeSpan.FromSeconds(seconds);

                            //
                            // Execute this part of the driver
                            //
                            if (this.ExecuteInitialising() == false)
                            {
                                this.executionResultStatus = ExecutionStatus.Failed;
                                this.executionStatus = ExecutionStatus.Completed;
                                break;
                            }

                            this.executionStatus = ExecutionStatus.Starting;
                            break;

                        case ExecutionStatus.Starting:
                            //
                            // Determine the time at which execution is expected to complete
                            //
                            seconds = this.executionTimes.start + this.executionTimes.run +
                                this.executionTimes.stop + this.executionTimes.finalise;
                            this.executionCompletionTime = DateTime.Now + TimeSpan.FromSeconds(seconds);

                            //
                            // Execute this part of the driver
                            //
                            if (this.ExecuteStarting() == false)
                            {
                                this.executionResultStatus = ExecutionStatus.Failed;
                                this.executionStatus = ExecutionStatus.Stopping;
                                break;
                            }

                            this.executionStatus = ExecutionStatus.Running;
                            break;

                        case ExecutionStatus.Running:
                            //
                            // Determine the time at which execution is expected to complete
                            //
                            seconds = this.executionTimes.run +
                                this.executionTimes.stop + this.executionTimes.finalise;
                            this.executionCompletionTime = DateTime.Now + TimeSpan.FromSeconds(seconds);

                            //
                            // Execute this part of the driver
                            //
                            if (this.ExecuteRunning() == false)
                            {
                                this.executionResultStatus = ExecutionStatus.Failed;
                                this.executionStatus = ExecutionStatus.Stopping;
                                break;
                            }

                            this.executionStatus = ExecutionStatus.Stopping;
                            break;

                        case ExecutionStatus.Stopping:
                            //
                            // Determine the time at which execution is expected to complete
                            //
                            seconds = this.executionTimes.stop + this.executionTimes.finalise;
                            this.executionCompletionTime = DateTime.Now + TimeSpan.FromSeconds(seconds);

                            //
                            // Execute this part of the driver
                            //
                            if (this.ExecuteStopping() == false)
                            {
                                this.executionResultStatus = ExecutionStatus.Failed;
                            }

                            this.executionStatus = ExecutionStatus.Finalising;
                            break;

                        case ExecutionStatus.Finalising:
                            //
                            // Determine the time at which execution is expected to complete
                            //
                            seconds = this.executionTimes.finalise;
                            this.executionCompletionTime = DateTime.Now + TimeSpan.FromSeconds(seconds);

                            //
                            // Execute this part of the driver
                            //
                            if (this.ExecuteFinalising() == false)
                            {
                                this.executionResultStatus = ExecutionStatus.Failed;
                            }

                            this.executionStatus = ExecutionStatus.Done;
                            break;

                        case ExecutionStatus.Done:
                            this.running = false;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            //
            // Update execution result if all was successful
            //
            if (this.executionResultStatus == ExecutionStatus.None)
            {
                this.executionResultStatus = ExecutionStatus.Completed;
            }

            //
            // Now, execution is completed
            //
            this.executionStatus = ExecutionStatus.Completed;

            String logMessage = STRLOG_ExecutingStatus + this.executionStatus.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_ResultStatus + this.executionResultStatus.ToString();

            Trace.WriteLine("DriverGeneric.DriverThread: Completed");

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);
        }

    }
}
