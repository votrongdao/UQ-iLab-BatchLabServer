using System;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Drivers;

namespace Library.LabEquipment
{
    public class Specification : ExperimentSpecification
    {
        #region Constants

        private const string STRLOG_ClassName = "Specification";

        //
        // String constants for logfile messages
        //

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public Specification(XmlNode xmlNodeEquipmentConfig)
            : base(xmlNodeEquipmentConfig)
        {
            const string STRLOG_MethodName = "Specification";

            Logfile.WriteCalled(null, STRLOG_MethodName);


            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Parse the XML specification string to check its validity. No exceptions are thrown back to the
        /// calling method. If an error occurs, 'accepted' is set to false and the error message is placed
        /// in 'errorMessage' where it can be examined by the calling method. Return 'accepted'.
        /// </summary>
        /// <param name="xmlSpecification"></param>
        public override ValidationReport Parse(string xmlSpecification)
        {
            const string STRLOG_MethodName = "Parse";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Catch all exceptions and log errors, don't throw back to caller
            //
            ValidationReport validationReport = null;
            try
            {
                //
                // Call the base class to parse its part
                //
                validationReport = base.Parse(xmlSpecification);
                if (validationReport.accepted == false)
                {
                    throw new Exception(validationReport.errorMessage);
                }

                //
                // Create an instance of the driver for the specified setup and then
                // get the driver's execution time for this specification
                //
                if (this.setupId.Equals(Consts.STRXML_SetupId_OpenCircuitVaryField))
                {
                    DriverMachine_OCVF driver = new DriverMachine_OCVF(this.xmlNodeEquipmentConfig, this);
                    validationReport.estRuntime = driver.GetExecutionTime();
                }
                else if (this.setupId.Equals(Consts.STRXML_SetupId_OpenCircuitVarySpeed))
                {
                    DriverMachine_OCVS driver = new DriverMachine_OCVS(this.xmlNodeEquipmentConfig, this);
                    validationReport.estRuntime = driver.GetExecutionTime();
                }
                else if (this.setupId.Equals(Consts.STRXML_SetupId_ShortCircuitVaryField))
                {
                    DriverMachine_SCVF driver = new DriverMachine_SCVF(this.xmlNodeEquipmentConfig, this);
                    validationReport.estRuntime = driver.GetExecutionTime();
                }
                else if (this.setupId.Equals(Consts.STRXML_SetupId_PreSynchronisation))
                {
                    DriverMachine_PreSync driver = new DriverMachine_PreSync(this.xmlNodeEquipmentConfig, this);
                    validationReport.estRuntime = driver.GetExecutionTime();
                }
                else if (this.setupId.Equals(Consts.STRXML_SetupId_Synchronisation))
                {
                    DriverMachine_Sync driver = new DriverMachine_Sync(this.xmlNodeEquipmentConfig, this);
                    validationReport.estRuntime = driver.GetExecutionTime();
                }
                else
                {
                    validationReport.accepted = false;
                    throw new Exception(STRERR_UnknownSetupId + this.setupId);
                }
            }
            catch (Exception ex)
            {
                validationReport.errorMessage = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Accepted + validationReport.accepted.ToString();
            if (validationReport.accepted == true)
            {
                logMessage += Logfile.STRLOG_Spacer + STRLOG_ExecutionTime + validationReport.estRuntime.ToString() + STRLOG_seconds;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return validationReport;
        }

    }
}
