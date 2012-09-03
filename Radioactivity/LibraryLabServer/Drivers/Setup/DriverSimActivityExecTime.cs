using System;
using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServer.Drivers.Module;

namespace Library.LabServer.Drivers.Setup
{
    public partial class DriverSimActivity : DriverModuleGeneric
    {
        #region Class Constants and Variables

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public override int GetExecutionTime(ExperimentSpecification experimentSpecification)
        {
            const string STRLOG_MethodName = "GetExecutionTime";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            // Typecast the specification so that it can be used here
            Specification specification = (Specification)experimentSpecification;

            //
            // Initialise variables
            //
            double executionTime = 1.0;
            int tubeHomeDistance = this.simActivity.GetTubeHomeDistance();
            int distanceIndex = 0;
            int fromDistance = 0;
            int toDistance = 0;

            //
            // Get source and absorber select times
            //
            executionTime += this.simActivity.GetAbsorberSelectTime(specification.AbsorberList[0].location);
            executionTime += this.simActivity.GetSourceSelectTime(specification.SourceLocation);

            //
            // Get tube move times
            //
            while (true)
            {
                //
                // Determine the 'from' and 'to' distances
                //
                if (distanceIndex == 0)
                {
                    // From home to first distance
                    fromDistance = tubeHomeDistance;
                    toDistance = specification.DistanceList[distanceIndex];
                }
                else if (distanceIndex < specification.DistanceList.Length)
                {
                    // Everything in between
                    fromDistance = specification.DistanceList[distanceIndex - 1];
                    toDistance = specification.DistanceList[distanceIndex];
                }

                //
                // Get tube move time
                //
                executionTime += this.simActivity.GetTubeMoveTime(fromDistance, toDistance);

                //
                // Get capture data time
                //
                executionTime += this.simActivity.GetCaptureDataTime(specification.Duration) * specification.Repeat;
                if (++distanceIndex == specification.DistanceList.Length)
                {
                    // All distances are done
                    break;
                }
            }

            //
            // Get source and absorber return times
            //
            executionTime += this.simActivity.GetSourceReturnTime(specification.SourceLocation);
            executionTime += this.simActivity.GetAbsorberReturnTime(specification.AbsorberList[0].location);

            //
            // Get tube return to home time
            //
            fromDistance = specification.DistanceList[specification.DistanceList.Length - 1];
            toDistance = tubeHomeDistance;
            executionTime += this.simActivity.GetTubeMoveTime(fromDistance, toDistance);

            string logMessage = STRLOG_ExecutionTime + executionTime.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return (int)executionTime;
        }

    }
}
