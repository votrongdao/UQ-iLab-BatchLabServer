using System;
using System.Xml;
using Library.Lab;

namespace Library.LabServerEngine
{
    public class ExperimentValidation
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "ExperimentValidation";

        //
        // String constants for error messages
        //
        private const string STRERR_LabConfiguration = "labConfiguration";
        private const string STRERR_XmlValidation = "xmlValidation";

        //
        // Local variables
        //
        private LabConfiguration labConfiguration;

        //
        // Local variables available to a derived class
        //
        protected XmlNode xmlNodeConfiguration;
        protected XmlNode xmlNodeValidation;

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public ExperimentValidation(LabConfiguration labConfiguration)
        {
            const string STRLOG_MethodName = "ExperimentValidation";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            try
            {
                //
                // Check that the lab configuration exists
                //
                if (labConfiguration == null)
                {
                    throw new ArgumentNullException(STRERR_LabConfiguration);
                }

                // Save the lab configuration
                this.labConfiguration = labConfiguration;

                //
                // Check that the validation XML node exists
                //
                if (labConfiguration.XmlValidation == null)
                {
                    throw new ArgumentNullException(STRERR_XmlValidation);
                }

                //
                // Load XML validation string from the lab configuration and save a copy of the XML node
                //
                XmlDocument xmlDocument = XmlUtilities.GetXmlDocument(labConfiguration.XmlValidation);
                XmlNode xmlRootNode = XmlUtilities.GetXmlRootNode(xmlDocument, Consts.STRXML_validation);
                this.xmlNodeValidation = xmlRootNode.Clone();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

    }
}
