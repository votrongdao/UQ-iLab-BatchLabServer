using System;
using System.Xml;
using Library.Lab;

namespace Library.LabClient
{
    public class ExperimentResult
    {
        public XmlNode xmlNodeExperimentResult;

        protected ExperimentResultInfo experimentResultInfo;

        //-------------------------------------------------------------------------------------------------//

        public ExperimentResult(string xmlExperimentResult, ExperimentResultInfo experimentResultInfo)
        {
            try
            {
                //
                // Load XML experiment result string and make a copy of the XML node
                //
                XmlDocument xmlDocument = XmlUtilities.GetXmlDocument(xmlExperimentResult);
                XmlNode xmlRootNode = XmlUtilities.GetXmlRootNode(xmlDocument, Consts.STRXML_experimentResult);
                this.xmlNodeExperimentResult = xmlRootNode.Clone();

                //
                // Parse the experiment result
                //
                this.experimentResultInfo = experimentResultInfo;
                this.experimentResultInfo.timestamp = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_timestamp, true);
                this.experimentResultInfo.experimentId = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, Consts.STRXML_experimentId);
                try
                {
                    this.experimentResultInfo.setupId = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_setupId, true);
                    this.experimentResultInfo.setupName = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_setupName, true);
                    this.experimentResultInfo.title = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_title, true);
                    this.experimentResultInfo.version = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_version, true);
                    this.experimentResultInfo.unitId = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, Consts.STRXML_unitId, 0);
                    this.experimentResultInfo.dataType = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_dataType, true);
                }
                catch
                {
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }
        }

    }
}
