using System;
using System.Diagnostics;
using System.Threading;
using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServer.Drivers.Setup;

namespace Library.LabServer
{
    public class ExperimentEngine : LabExperimentEngine
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "ExperimentEngine";

        //
        // Local variables
        //
        private Configuration configuration;

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public ExperimentEngine(int unitId, AppData appData)
            : base(unitId, appData)
        {
            this.configuration = (Configuration)appData.labConfiguration;
        }

        //-------------------------------------------------------------------------------------------------//

        public override ValidationReport Validate(string xmlSpecification)
        {
            const string STRLOG_MethodName = "Validate";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Parse the XML specification string to generate a validation report
            //
            ValidationReport validationReport = null;
            try
            {
                Specification specification = new Specification(this.configuration, this.equipmentServiceProxy);
                validationReport = specification.Parse(xmlSpecification);
            }
            catch (Exception ex)
            {
                validationReport = new ValidationReport(false);
                validationReport.errorMessage = ex.Message;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return validationReport;
        }

        //-------------------------------------------------------------------------------------------------//

        public override ExperimentInfo RunExperiment(ExperimentInfo experimentInfo)
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
                Specification specification = new Specification(this.configuration, this.equipmentServiceProxy);
                ValidationReport validationReport = specification.Parse(experimentInfo.xmlSpecification);
                if (validationReport.accepted == false)
                {
                    throw new ArgumentException(validationReport.errorMessage);
                }
                experimentInfo.setupId = specification.SetupId;

                //
                // Create an instance of the driver for the specified setup and then
                // execute the experiment and return the result information
                //
                ResultInfo resultInfo = null;
                if (specification.SetupId.Equals(Consts.STRXML_SetupId_LockedRotor))
                {
                    DriverLockedRotor driver = new DriverLockedRotor(this.equipmentServiceProxy, this.configuration, this.labExperimentInfo.cancelExperiment);
                    resultInfo = (ResultInfo)driver.Execute(specification);
                }
                else if (specification.SetupId.Equals(Consts.STRXML_SetupId_NoLoad))
                {
                    DriverNoLoad driver = new DriverNoLoad(this.equipmentServiceProxy, this.configuration, this.labExperimentInfo.cancelExperiment);
                    resultInfo = (ResultInfo)driver.Execute(specification);
                }
                else if (specification.SetupId.Equals(Consts.STRXML_SetupId_SynchronousSpeed))
                {
                    DriverSynchronousSpeed driver = new DriverSynchronousSpeed(this.equipmentServiceProxy, this.configuration, this.labExperimentInfo.cancelExperiment);
                    resultInfo = (ResultInfo)driver.Execute(specification);
                }
                else if (specification.SetupId.Equals(Consts.STRXML_SetupId_FullLoad))
                {
                    DriverFullLoad driver = new DriverFullLoad(this.equipmentServiceProxy, this.configuration, this.labExperimentInfo.cancelExperiment);
                    resultInfo = (ResultInfo)driver.Execute(specification);
                }

                //
                // Create an instance of LabExperimentResult to convert the experiment results to an XML string
                //
                ExperimentResult experimentResult = new ExperimentResult(
                    experimentInfo.experimentId, experimentInfo.sbName, DateTime.Now,
                    this.unitId, (Configuration)this.labConfiguration, specification, resultInfo);

                //
                // Fill in the result report
                //
                experimentInfo.resultReport.experimentResults = experimentResult.ToString();
                experimentInfo.resultReport.statusCode = (int)resultInfo.statusCode;
                experimentInfo.resultReport.errorMessage = resultInfo.errorMessage;
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
        
        protected override void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                // Dispose managed resources here. Anything that has a Dispose() method.
            }

            //
            // Release unmanaged resources here. Set large fields to null.
            //

            // Call Dispose on your base class.
            base.Dispose(disposing);
        }
    }
}
