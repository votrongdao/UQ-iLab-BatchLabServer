using System;
using System.Diagnostics;
using System.Threading;
using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServer.Drivers.Module;

namespace Library.LabServer.Drivers.Setup
{
    public partial class DriverSimActivity : DriverModuleGeneric
    {
        #region Class Constants and Variables

        //
        // String constants for logfile messages
        //
        private const string STRLOG_SourceLocation = " SourceLocation: ";
        private const string STRLOG_AbsorberLocation = " AbsorberLocation: ";
        private const string STRLOG_TubeDistance = " TubeDistance: ";
        private const string STRLOG_Distance = " Distance: ";
        private const string STRLOG_Duration = " Duration: ";
        private const string STRLOG_Repeat = " Repeat: ";
        private const string STRLOG_Count = " Count: ";

        //
        // String constants for error messages
        //

        //
        // Data driven state machine for Execute()
        //
        private enum States_Execute
        {
            sSelectAbsorber, sSelectSource, sSetTubeDistance,
            sCaptureData, sReturnSource, sReturnAbsorber, sReturnTube,
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
            // Select absorber
            //
            new SMTableEntry_Execute(States_Execute.sSelectAbsorber, States_Execute.sSelectSource, States_Execute.sCompleted),

            //
            // Select source
            //
            new SMTableEntry_Execute(States_Execute.sSelectSource, States_Execute.sSetTubeDistance, States_Execute.sReturnAbsorber),

            //
            // Select tube distance
            //
            new SMTableEntry_Execute(States_Execute.sSetTubeDistance, States_Execute.sCaptureData, States_Execute.sReturnSource),

            //
            // Get capture data
            //
            new SMTableEntry_Execute(States_Execute.sCaptureData, States_Execute.sReturnSource, States_Execute.sReturnSource),

            //
            // Return source to home position
            //
            new SMTableEntry_Execute(States_Execute.sReturnSource, States_Execute.sReturnAbsorber, States_Execute.sReturnAbsorber),

            //
            // Return absorber to home position
            //
            new SMTableEntry_Execute(States_Execute.sReturnAbsorber, States_Execute.sReturnTube, States_Execute.sReturnTube),

            //
            // Return tube to home position
            //
            new SMTableEntry_Execute(States_Execute.sReturnTube, States_Execute.sCompleted, States_Execute.sCompleted),
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
            string strDistanceList = null;
            for (int i = 0; i < specification.DistanceList.Length; i++)
            {
                if (i > 0)
                {
                    strDistanceList += Consts.CHR_CsvSplitter.ToString();
                }
                strDistanceList += specification.DistanceList[i].ToString();
            }
            string logMessage = STRLOG_Distance + strDistanceList;
            logMessage += Logfile.STRLOG_Spacer + STRLOG_Duration + specification.Duration.ToString();
            logMessage += Logfile.STRLOG_Spacer + STRLOG_Repeat + specification.Repeat.ToString();
            Logfile.Write(logMessage);

            //
            // Create an instance of the result info ready to fill in
            //
            ResultInfo resultInfo = new ResultInfo();
            resultInfo.statusCode = StatusCodes.Running;
            if (this.simActivity.SimulateDelays == true)
            {
                resultInfo.dataType = DataTypes.Simulated;
            }
            else
            {
                resultInfo.dataType = DataTypes.Calculated;
            }

            //
            // Create data structures to hold the results
            //
            resultInfo.dataVectors = new int[specification.DistanceList.Length, specification.Repeat];

            //
            // Initialise variables used in the state machine
            //
            int distanceIndex = 0;
            int repeatIndex = 0;
            int[] generatedData = null;

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
                        case States_Execute.sSelectAbsorber:

                            //
                            // Get absorber location from specification
                            //
                            char absorberLocation = specification.AbsorberList[0].location;
                            Logfile.Write(STRLOG_AbsorberLocation + absorberLocation.ToString());

                            //
                            // Set absorber location
                            //
                            this.simActivity.SetAbsorberLocation(absorberLocation);
                            break;

                        case States_Execute.sSelectSource:

                            //
                            // Get source location from specification
                            //
                            char sourceLocation = specification.SourceLocation;
                            Logfile.Write(STRLOG_SourceLocation + sourceLocation.ToString());

                            //
                            // Set source location
                            //
                            this.simActivity.SetSourceLocation(sourceLocation);
                            break;

                        case States_Execute.sSetTubeDistance:

                            //
                            // Get tube distance from specification
                            //
                            int tubeDistance = specification.DistanceList[distanceIndex];
                            Logfile.Write(STRLOG_TubeDistance + tubeDistance.ToString());

                            //
                            // Set tube distance
                            //
                            this.simActivity.SetTubeDistance(tubeDistance);
                            break;

                        case States_Execute.sCaptureData:

                            if (repeatIndex == 0)
                            {
                                //
                                // Generate data for repeat counts at this distance
                                //
                                generatedData = this.simActivity.GenerateData(
                                    specification.DistanceList[distanceIndex], specification.Duration, specification.Repeat);
                            }

                            //
                            // Get capture data for this repeat count
                            //
                            int[] counts = new int[1];
                            this.simActivity.CaptureData(specification.Duration, counts, generatedData, repeatIndex);
                            resultInfo.dataVectors[distanceIndex, repeatIndex] = counts[0];
                            Logfile.Write(STRLOG_Duration + specification.Duration.ToString() +
                                Logfile.STRLOG_Spacer + STRLOG_Count + counts[0].ToString());

                            //
                            // Determine next state
                            //
                            if (++repeatIndex == specification.Repeat)
                            {
                                if (++distanceIndex == specification.DistanceList.Length)
                                {
                                    // All distances completed
                                    break;
                                }

                                // Next distance
                                repeatIndex = 0;
                                nextState = States_Execute.sSetTubeDistance;
                                break;
                            }

                            // Next repeat
                            nextState = States_Execute.sCaptureData;
                            break;

                        case States_Execute.sReturnSource:

                            //
                            // Get source home location
                            //
                            sourceLocation = this.simActivity.SourceHomeLocation;
                            Logfile.Write(STRLOG_SourceLocation + sourceLocation.ToString());

                            //
                            // Set source location
                            //
                            this.simActivity.SetSourceLocation(sourceLocation);
                            break;

                        case States_Execute.sReturnAbsorber:

                            //
                            // Get absorber home location
                            //
                            absorberLocation = this.simActivity.AbsorberHomeLocation;
                            Logfile.Write(STRLOG_AbsorberLocation + absorberLocation.ToString());

                            //
                            // Set absorber location
                            //
                            this.simActivity.SetAbsorberLocation(absorberLocation);
                            break;

                        case States_Execute.sReturnTube:

                            //
                            // Get tube home distance
                            //
                            tubeDistance = this.simActivity.TubeHomeDistance;
                            Logfile.Write(STRLOG_TubeDistance + tubeDistance.ToString());

                            //
                            // Set tube distance
                            //
                            this.simActivity.SetTubeDistance(tubeDistance);
                            break;

                        default:
                            break;
                    }

                    Trace.WriteLine("nextState: " + entry.nextState.ToString());

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
