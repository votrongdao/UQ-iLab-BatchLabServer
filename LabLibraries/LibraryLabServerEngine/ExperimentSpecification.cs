using System;
using System.Xml;
using Library.Lab;
using Library.LabServerEngine.Drivers.Equipment;
using Library.LabServerEngine.Drivers.Setup;

namespace Library.LabServerEngine
{
    public class ExperimentSpecification
    {
        #region Class Constants and Variables

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
        private const string STRERR_LabConfiguration = "labConfiguration";
        private const string STRERR_XmlSpecification = "xmlSpecification";
        protected const string STRERR_SetupIdInvalid = "Setup ID is invalid!";
        protected const string STRERR_EquipmentServiceNotAvailable = "EquipmentService is not available!";

        //
        // Local variables
        //
        private LabConfiguration labConfiguration;

        //
        // Local variables available to a derived class
        //
        protected XmlNode xmlNodeConfiguration;
        protected XmlNode xmlNodeSpecification;
        protected EquipmentService equipmentServiceProxy;

        #endregion

        #region Properties

        protected string setupId;
        protected XmlNode xmlNodeSetup;

        public string SetupId
        {
            get { return this.setupId; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public ExperimentSpecification(LabConfiguration labConfiguration, EquipmentService equipmentServiceProxy)
        {
            const string STRLOG_MethodName = "ExperimentSpecification";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Save these for use by the Parse() method
            //
            this.labConfiguration = labConfiguration;
            this.equipmentServiceProxy = equipmentServiceProxy;

            try
            {
                //
                // Load XML specification string from the lab configuration and save a copy of the XML node
                //
                XmlDocument xmlDocument = XmlUtilities.GetXmlDocument(labConfiguration.XmlSpecification);
                XmlNode xmlRootNode = XmlUtilities.GetXmlRootNode(xmlDocument, Consts.STRXML_experimentSpecification);
                this.xmlNodeSpecification = xmlRootNode.Clone();

                //
                // Check that all required XML specification nodes exist
                //
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_setupId, true);

                //
                // Load XML configuration string from the lab configuration and save a copy of the XML node
                //
                xmlDocument = XmlUtilities.GetXmlDocument(labConfiguration.XmlConfiguration);
                xmlRootNode = XmlUtilities.GetXmlRootNode(xmlDocument, Consts.STRXML_configuration);
                this.xmlNodeConfiguration = xmlRootNode.Clone();
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
                // Load XML specification string
                XmlDocument xmlDocument = XmlUtilities.GetXmlDocument(xmlSpecification);

                //
                // Get a copy of the specification XML node
                //
                XmlNode xmlNode = XmlUtilities.GetXmlRootNode(xmlDocument, Consts.STRXML_experimentSpecification);
                this.xmlNodeSpecification = xmlNode.Clone();

                //
                // Get the setup id and check that it exists - search is case-sensitive
                //
                this.setupId = XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_setupId, false);
                int setupIndex = Array.IndexOf(this.labConfiguration.SetupIds, this.setupId);
                if (setupIndex < 0)
                {
                    throw new ArgumentException(STRERR_SetupIdInvalid, this.setupId);
                }

                //
                // Get the specified setup XML node
                //
                XmlNodeList xmlNodeList = XmlUtilities.GetXmlNodeList(this.xmlNodeConfiguration, Consts.STRXML_setup, true);
                this.xmlNodeSetup = xmlNodeList.Item(setupIndex);

                //
                // Create an instance of the driver for the specified setup and then
                // get the driver's execution time for this specification
                //
                int executionTime = -1;
                if (this.SetupId.Equals(Consts.STRXML_SetupId_EquipmentGeneric))
                {
                    if (this.equipmentServiceProxy == null)
                    {
                        throw new ArgumentException(STRERR_EquipmentServiceNotAvailable, this.setupId);
                    }

                    DriverEquipmentGeneric driver = new DriverEquipmentGeneric(this.equipmentServiceProxy, this.labConfiguration);
                    executionTime = driver.GetExecutionTime(this);
                }
                else if (this.SetupId.Equals(Consts.STRXML_SetupId_ModuleGeneric))
                {
                    DriverModuleGeneric driver = new DriverModuleGeneric(this.labConfiguration);
                    executionTime = driver.GetExecutionTime(this);
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

        //-------------------------------------------------------------------------------------------------//

        public override string ToString()
        {
            return XmlUtilities.ToXmlString(this.xmlNodeSpecification);
        }
    
    }
}
