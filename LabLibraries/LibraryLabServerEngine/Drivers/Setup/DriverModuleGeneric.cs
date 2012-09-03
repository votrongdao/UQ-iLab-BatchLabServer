using System;
using System.Diagnostics;
using System.Threading;
using Library.Lab;

namespace Library.LabServerEngine.Drivers.Setup
{
    public class DriverModuleGeneric
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "DriverModuleGeneric";

        //
        // Constants
        //
        private const int EXECUTION_TIME = 10;

        //
        // String constants for logfile messages
        //
        protected const string STRLOG_ExecutionTime = " ExecutionTime: ";
        protected const string STRLOG_StatusCode = " StatusCode: ";

        //
        // String constants for error messages
        //
        protected const string STRERR_StateNotFound = "State not found!";

        //
        // Local variables available to a derived class
        //
        protected LabConfiguration labConfiguration;
        protected CancelExperiment cancelExperiment;

        #endregion

        //---------------------------------------------------------------------------------------//

        public DriverModuleGeneric(LabConfiguration labConfiguration)
            : this(labConfiguration, null)
        {
        }

        //---------------------------------------------------------------------------------------//

        public DriverModuleGeneric(LabConfiguration labConfiguration, CancelExperiment cancelExperiment)
        {
            this.labConfiguration = labConfiguration;
            this.cancelExperiment = cancelExperiment;
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual int GetExecutionTime(ExperimentSpecification experimentSpecification)
        {
            const string STRLOG_MethodName = "GetExecutionTime";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            int executionTime = EXECUTION_TIME;

            string logMessage = STRLOG_ExecutionTime + executionTime.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual ExperimentResultInfo Execute(ExperimentSpecification experimentSpecification)
        {
            const string STRLOG_MethodName = "Execute";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Create an instance of the experiment result info ready to fill in
            //
            ExperimentResultInfo experimentResultInfo = new ExperimentResultInfo();

            try
            {
                // Determine time to finish execution
                DateTime dateTimeEnd = DateTime.Now + new TimeSpan(0, 0, EXECUTION_TIME);

                //
                // Delay for the full execution time, unless cancelled
                //
                while (DateTime.Now < dateTimeEnd)
                {
                    Trace.Write("M");
                    Thread.Sleep(1000);

                    //
                    // Check if the experiment is being cancelled
                    //
                    if (this.cancelExperiment != null &&
                        this.cancelExperiment.IsCancelled == true)
                    {
                        // Experiment is cancelled
                        experimentResultInfo.statusCode = StatusCodes.Cancelled;
                        break;
                    }
                }
                Trace.WriteLine("");

                //
                // Check if the experiment was cancelled
                //
                if (experimentResultInfo.statusCode != StatusCodes.Cancelled)
                {
                    // Successful execution
                    experimentResultInfo.statusCode = StatusCodes.Completed;
                }
            }
            catch (Exception ex)
            {
                experimentResultInfo.statusCode = StatusCodes.Failed;
                experimentResultInfo.errorMessage = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            Logfile.Write(STRLOG_StatusCode + experimentResultInfo.statusCode);

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return experimentResultInfo;
        }
    }
}
