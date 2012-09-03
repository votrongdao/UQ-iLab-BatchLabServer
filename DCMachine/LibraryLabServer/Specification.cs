using System;
using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Equipment;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServer.Drivers.Setup;

namespace Library.LabServer
{
    public class Specification : ExperimentSpecification
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "Specification";

        //
        // Constants
        //
        private const int TIME_SECS_AdministrationExecution = 6;

        //
        // String constants for logfile messages
        //

        //
        // String constants for error messages
        //

        //
        // Local variables
        //
        private Configuration configuration;
        private Validation validation;

        #endregion

        #region Properties

        private MinMaxStep speed;
        private MinMaxStep field;
        private MinMaxStep load;

        public MinMaxStep Speed
        {
            get { return speed; }
        }

        public MinMaxStep Field
        {
            get { return field; }
        }

        public MinMaxStep Load
        {
            get { return load; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public Specification(Configuration configuration, EquipmentService equipmentServiceProxy)
            : base(configuration, equipmentServiceProxy)
        {
            const string STRLOG_MethodName = "Specification";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Save these for use by the Parse() method
            //
            this.configuration = configuration;

            //
            // Check that the specification template is valid. This is used by the LabClient to submit
            // the experiment specification to the LabServer for execution.
            //
            try
            {
                //
                // Check that all required XML nodes exist
                //
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_speedMin, true);
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_speedMax, true);
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_speedStep, true);
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_fieldMin, true);
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_fieldMax, true);
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_fieldStep, true);
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_loadMin, true);
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_loadMax, true);
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_loadStep, true);

                //
                // Create an instance fo the Validation class
                //
                this.validation = new Validation(configuration);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Parse the XML specification string to check its validity. No exceptions are thrown back to the
        /// calling method. If an error occurs, 'accepted' is set to false and the error message is placed
        /// in 'errorMessage' where it can be examined by the calling method.
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

                // Create new validation report
                validationReport = new ValidationReport();

                //
                // Create an instance of the driver for the specified setup and then
                // get the driver's execution time for this specification
                //
                int executionTime = -1;
                if (this.SetupId.Equals(Consts.STRXML_SetupId_VoltageVsSpeed))
                {
                    //
                    // Get the speed range and validate
                    //
                    this.speed = new MinMaxStep();
                    this.speed.min = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_speedMin);
                    this.speed.max = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_speedMax);
                    this.speed.step = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_speedStep);

                    this.validation.ValidateSpeed(this.speed);

                    DriverVoltageVsSpeed driver = new DriverVoltageVsSpeed(this.equipmentServiceProxy, this.configuration);
                    executionTime = driver.GetExecutionTime(this);
                }
                else if (this.SetupId.Equals(Consts.STRXML_SetupId_VoltageVsField))
                {
                    //
                    // Get the field range and validate
                    //
                    this.field = new MinMaxStep();
                    this.field.min = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_fieldMin);
                    this.field.max = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_fieldMax);
                    this.field.step = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_fieldStep);

                    this.validation.ValidateField(this.field);

                    DriverVoltageVsField driver = new DriverVoltageVsField(this.equipmentServiceProxy, this.configuration);
                    executionTime = driver.GetExecutionTime(this);
                }
                else if (this.SetupId.Equals(Consts.STRXML_SetupId_VoltageVsLoad))
                {
                    //
                    // Get the load range and validate
                    //
                    this.load = new MinMaxStep();
                    this.load.min = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_loadMin);
                    this.load.max = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_loadMax);
                    this.load.step = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_loadStep);

                    this.validation.ValidateLoad(this.load);

                    DriverVoltageVsLoad driver = new DriverVoltageVsLoad(this.equipmentServiceProxy, this.configuration);
                    executionTime = driver.GetExecutionTime(this);
                }
                else if (this.SetupId.Equals(Consts.STRXML_SetupId_SpeedVsVoltage))
                {
                    //
                    // Get the speed range and validate
                    //
                    this.speed = new MinMaxStep();
                    this.speed.min = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_speedMin);
                    this.speed.max = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_speedMax);
                    this.speed.step = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_speedStep);

                    this.validation.ValidateSpeed(this.speed);

                    DriverSpeedVsVoltage driver = new DriverSpeedVsVoltage(this.equipmentServiceProxy, this.configuration);
                    executionTime = driver.GetExecutionTime(this);
                }
                else if (this.SetupId.Equals(Consts.STRXML_SetupId_SpeedVsField))
                {
                    //
                    // Get the field range and validate
                    //
                    this.field = new MinMaxStep();
                    this.field.min = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_fieldMin);
                    this.field.max = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_fieldMax);
                    this.field.step = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_fieldStep);

                    this.validation.ValidateField(this.field);

                    DriverSpeedVsField driver = new DriverSpeedVsField(this.equipmentServiceProxy, this.configuration);
                    executionTime = driver.GetExecutionTime(this);
                }

                //
                // Specification is valid
                //
                validationReport.estRuntime = executionTime + TIME_SECS_AdministrationExecution;
                validationReport.accepted = true;
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
