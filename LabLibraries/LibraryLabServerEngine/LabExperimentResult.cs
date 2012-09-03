using System;
using System.IO;
using System.Xml;
using Library.Lab;

namespace Library.LabServerEngine
{
    public class LabExperimentResult
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "LabExperimentResult";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_experimentId = " experimentId: ";
        private const string STRLOG_sbName = " sbName: ";
        private const string STRLOG_unitId = " unitId: ";

        //
        // Constants
        //
        private const string STR_DateTimeFormat = "ddd dd MMM yyyy h:mm:ss tt";

        //
        // Local variables available to a derived class
        //
        protected XmlNode xmlNodeExperimentResult;

        #endregion

        //---------------------------------------------------------------------------------------//

        public LabExperimentResult(LabConfiguration labConfiguration)
        {
            try
            {
                //
                // Load XML experiment result string from the lab configuration 
                //
                XmlDocument xmlDocument = XmlUtilities.GetXmlDocument(labConfiguration.XmlExperimentResult);
                XmlNode xmlRootNode = XmlUtilities.GetXmlRootNode(xmlDocument, Consts.STRXML_experimentResult);
                this.xmlNodeExperimentResult = xmlRootNode.Clone();

                //
                // Check that all required XML nodes exist
                //
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_timestamp, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_title, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_version, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_experimentId, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_unitId, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_setupId, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_setupName, true);
            }
            catch (Exception ex)
            {
                // Log the message and throw the exception back to the caller
                Logfile.WriteError(ex.Message);
                throw;
            }
        }

        //---------------------------------------------------------------------------------------//

        public LabExperimentResult(int experimentId, string sbName, DateTime timestamp, string setupId,
            int unitId, LabConfiguration labConfiguration)
        {
            const string STRLOG_MethodName = "LabExperimentResult";

            string logMessage = STRLOG_experimentId + experimentId.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + sbName + Logfile.STRLOG_Quote +
                Logfile.STRLOG_Spacer + STRLOG_unitId + unitId.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            try
            {
                //
                // Load XML experiment result string from the lab configuration 
                //
                XmlDocument xmlDocument = XmlUtilities.GetXmlDocument(labConfiguration.XmlExperimentResult);
                XmlNode xmlRootNode = XmlUtilities.GetXmlRootNode(xmlDocument, Consts.STRXML_experimentResult);
                this.xmlNodeExperimentResult = xmlRootNode.Clone();

                //
                // Add the result information
                //
                XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_timestamp, timestamp.ToString(STR_DateTimeFormat), false);
                XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_title, labConfiguration.Title, false);
                XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_version, labConfiguration.Version, false);
                XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_experimentId, experimentId);
                XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_unitId, unitId);
                XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_setupId, setupId, false);
                int setupIndex = Array.IndexOf(labConfiguration.SetupIds, setupId);
                if (setupIndex >= 0)
                {
                    string setupName = labConfiguration.SetupNames[setupIndex];
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_setupName, setupName, false);
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //---------------------------------------------------------------------------------------//

        public override string ToString()
        {
            return XmlUtilities.ToXmlString(this.xmlNodeExperimentResult);
        }

    }
}
