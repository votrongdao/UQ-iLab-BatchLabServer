using System;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Engine.Drivers;

namespace Library.LabEquipment.Engine
{
    public class ExperimentSpecification
    {
        #region Constants

        private const string STRLOG_ClassName = "ExperimentSpecification";

        //
        // String constants for logfile messages
        //
        protected const string STRLOG_Accepted = " Accepted: ";
        protected const string STRLOG_ExecutionTime = " ExecutionTime: ";
        protected const string STRLOG_seconds = " seconds";

        //
        // String constants for error messages
        //
        protected const string STRERR_UnknownSetupId = " Unknown SetupId: ";

        #endregion

        #region Variables

        protected XmlNode xmlNodeEquipmentConfig;
        protected XmlNode xmlNodeSpecification;
        protected string setupId;

        #endregion

        #region Properties

        public string SetupId
        {
            get { return this.setupId; }
        }

        #endregion

        public ExperimentSpecification(XmlNode xmlNodeEquipmentConfig)
        {
            const string STRLOG_MethodName = "ExperimentSpecification";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Save this for use by the Parse() method
            //
            this.xmlNodeEquipmentConfig = xmlNodeEquipmentConfig;

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Parse the XML specification string to check its validity. No exceptions are thrown back to the
        /// calling method. If an error occurs, 'accepted' is set to false and the error message is placed
        /// in 'errorMessage' where it can be examined by the calling method. Return 'accepted'.
        /// </summary>
        /// <param name="xmlSpecification"></param>
        public virtual ValidationReport Parse(string xmlSpecification)
        {
            const string STRLOG_MethodName = "Parse";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Create a new validation report ready to fill in
            //
            ValidationReport validationReport = new ValidationReport();

            //
            // Process the XML specification string
            //
            try
            {
                //
                // Load XML specification string and get the setup id
                //
                XmlDocument xmlDocument = XmlUtilities.GetXmlDocument(xmlSpecification);
                this.xmlNodeSpecification = XmlUtilities.GetXmlRootNode(xmlDocument, Consts.STRXML_ExperimentSpecification);
                this.setupId = XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_SetupId, false);

                //
                // Create an instance of the driver for the specified setup and then
                // get the driver's execution time for this specification
                //
                int executionTime = -1;
                if (this.setupId.Equals(Consts.STRXML_SetupId_DriverGeneric))
                {
                    DriverGeneric driver = new DriverGeneric(this.xmlNodeEquipmentConfig, this);
                    executionTime = driver.GetExecutionTime();
                }

                //
                // Specification is valid
                //
                validationReport.estRuntime = executionTime;
                validationReport.accepted = true;
            }
            catch (Exception ex)
            {
                validationReport.errorMessage = ex.Message;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return validationReport;
        }


    }
}
