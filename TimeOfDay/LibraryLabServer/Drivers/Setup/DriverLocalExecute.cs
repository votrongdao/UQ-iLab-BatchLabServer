using System;
using System.Diagnostics;
using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServer.Drivers.Module;

namespace Library.LabServer.Drivers.Setup
{
    public partial class DriverLocal : DriverModuleGeneric
    {
        #region Class Constants and Variables

        //
        // String constants for logfile messages
        //

        //
        // String constants for error messages
        //

        //
        // Data driven state machine for Execute()
        //
        private enum States_Execute
        {
            sGetTimeOfDay,
            sCompleted,
        }
        private struct SMTableEntry_Execute
        {
            public States_Execute currentState;
            public States_Execute nextState;
            public States_Execute exitState;

            public SMTableEntry_Execute(States_Execute currentState, States_Execute nextState, States_Execute exitState)
            {
                this.currentState = currentState;
                this.nextState = nextState;
                this.exitState = exitState;
            }
        }
        private SMTableEntry_Execute[] smTable_Execute = new SMTableEntry_Execute[] {
            //
            // GetTimeOfDay
            //
            new SMTableEntry_Execute(States_Execute.sGetTimeOfDay, States_Execute.sCompleted, States_Execute.sCompleted),
        };

        #endregion

        //---------------------------------------------------------------------------------------//

        public override ExperimentResultInfo Execute(ExperimentSpecification experimentSpecification)
        {
            const string STRLOG_MethodName = "Execute";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Determine how long it actually take to execute
            //
            DateTime startDateTime = DateTime.Now;

            // Typecast the specification so that it can be used here
            Specification specification = (Specification)experimentSpecification;

            //
            // Log the specification
            //
            string logMessage = string.Empty;

            //
            // Create an instance of the result info ready to fill in
            //
            ResultInfo resultInfo = new ResultInfo();
            resultInfo.statusCode = StatusCodes.Running;

            //
            // Initialise variables used in the state machine
            //

            try
            {
                //
                // Run the state machine to execute the experiment specification
                //
                States_Execute state = States_Execute.sCompleted;
                if (smTable_Execute.Length > 0)
                {
                    state = smTable_Execute[0].currentState;
                }
                while (state != States_Execute.sCompleted)
                {
                    //
                    // Find table entry
                    //
                    int index = -1;
                    for (int i = 0; i < smTable_Execute.Length; i++)
                    {
                        if (smTable_Execute[i].currentState == state)
                        {
                            // Entry found
                            index = i;
                            break;
                        }
                    }
                    if (index == -1)
                    {
                        throw new ArgumentOutOfRangeException(state.ToString(), STRERR_StateNotFound);
                    }

                    //
                    // Get table entry and save next state
                    //
                    SMTableEntry_Execute entry = smTable_Execute[index];
                    States_Execute nextState = entry.nextState;

                    logMessage = " [ " + STRLOG_MethodName + ": " + entry.currentState.ToString() + " ]";
                    Logfile.Write(logMessage);

                    Trace.WriteLine(logMessage);

                    //
                    // Check if experiment was cancelled
                    //
                    if (this.cancelExperiment != null && this.cancelExperiment.IsCancelled == true &&
                        resultInfo.statusCode == StatusCodes.Running)
                    {
                        //
                        // Experiment was cancelled
                        //
                        resultInfo.statusCode = StatusCodes.Cancelled;
                        state = entry.exitState;
                        continue;
                    }

                    //
                    // Process commands
                    //
                    switch (entry.currentState)
                    {
                        case States_Execute.sGetTimeOfDay:

                            resultInfo.dateTime = this.timeOfDay.GetTimeOfDay(EXECUTION_TIME);

                            //
                            // Save the timestamp string in the specified format
                            //
                            if (specification.FormatName.Equals(Consts.STRXML_Format_12Hour))
                            {
                                resultInfo.timeofday = resultInfo.dateTime.ToString(Consts.STR_DateTimeFormat_12Hour);
                            }
                            else if (specification.FormatName.Equals(Consts.STRXML_Format_24Hour))
                            {
                                resultInfo.timeofday = resultInfo.dateTime.ToString(Consts.STR_DateTimeFormat_24Hour);
                            }
                            break;

                        default:
                            break;
                    }

                    //
                    // Next state
                    //
                    state = nextState;
                }

                //
                // Update status code
                //
                if (resultInfo.statusCode == StatusCodes.Running)
                {
                    resultInfo.statusCode = StatusCodes.Completed;
                }
            }
            catch (Exception ex)
            {
                resultInfo.statusCode = StatusCodes.Failed;
                resultInfo.errorMessage = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            TimeSpan timeSpan = DateTime.Now - startDateTime;
            logMessage = STRLOG_ExecutionTime + timeSpan.TotalSeconds.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return resultInfo;
        }
    }
}
