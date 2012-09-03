using System;
using System.Xml;
using Library.Lab;
using Library.LabServerEngine;

namespace Library.LabServer
{
    public class ExperimentResult : LabExperimentResult
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "ExperimentResult";

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public ExperimentResult(Configuration configuration)
            : base(configuration)
        {
            try
            {
                //
                // Check that all required XML nodes exist
                //
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_sourceName, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_absorberName, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_distance, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_duration, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_repeat, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_dataType, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_dataVector, true);
            }
            catch (Exception ex)
            {
                // Log the message and throw the exception back to the caller
                Logfile.WriteError(ex.Message);
                throw;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public ExperimentResult(int experimentId, string sbName, DateTime dateTime, int unitId, Configuration configuration,
            Specification specification, ResultInfo resultInfo)
            : base(experimentId, sbName, dateTime, specification.SetupId, unitId, configuration)
        {
            const string STRLOG_MethodName = "ExperimentResult";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            try
            {
                //
                // Add the specification information
                //
                XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_sourceName, specification.SourceName, false);
                XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_duration, specification.Duration);
                XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_repeat, specification.Repeat);
                if (specification.SetupId.Equals(Consts.STRXML_SetupId_RadioactivityVsTime) ||
                    specification.SetupId.Equals(Consts.STRXML_SetupId_SimActivityVsTime) ||
                    specification.SetupId.Equals(Consts.STRXML_SetupId_SimActivityVsTimeNoDelay))
                {
                    Specification.Absorber absorber = specification.AbsorberList[0];
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_absorberName, absorber.name, false);

                    int distance = specification.DistanceList[0];
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_distance, distance.ToString(), false);
                }
                else if (specification.SetupId.Equals(Consts.STRXML_SetupId_RadioactivityVsDistance) ||
                    specification.SetupId.Equals(Consts.STRXML_SetupId_SimActivityVsDistance) ||
                    specification.SetupId.Equals(Consts.STRXML_SetupId_SimActivityVsDistanceNoDelay))
                {
                    Specification.Absorber absorber = specification.AbsorberList[0];
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_absorberName, absorber.name, false);

                    //
                    // Create a CSV string of distances
                    //
                    string csvDistances = string.Empty;
                    for (int i = 0; i < specification.DistanceList.Length; i++)
                    {
                        if (i > 0)
                        {
                            csvDistances += Consts.CHR_CsvSplitter;
                        }
                        csvDistances += specification.DistanceList[i].ToString();
                    }
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_distance, csvDistances, false);
                }
                else if (specification.SetupId.Equals(Consts.STRXML_SetupId_RadioactivityVsAbsorber))
                {
                    //
                    // Create a CSV string of absorbers
                    //
                    string csvAbsorbers = string.Empty;
                    for (int i = 0; i < specification.AbsorberList.Length; i++)
                    {
                        if (i > 0)
                        {
                            csvAbsorbers += Consts.CHR_CsvSplitter;
                        }
                        csvAbsorbers += specification.AbsorberList[i].name;
                    }
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_absorberName, csvAbsorbers, false);

                    int distance = specification.DistanceList[0];
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_distance, distance.ToString(), false);
                }

                //
                // Add the result information 
                //
                if (resultInfo.statusCode == StatusCodes.Completed)
                {
                    if (specification.SetupId.Equals(Consts.STRXML_SetupId_RadioactivityVsTime) ||
                        specification.SetupId.Equals(Consts.STRXML_SetupId_RadioactivityVsDistance) ||
                        specification.SetupId.Equals(Consts.STRXML_SetupId_RadioactivityVsAbsorber) ||
                        specification.SetupId.Equals(Consts.STRXML_SetupId_SimActivityVsTime) ||
                        specification.SetupId.Equals(Consts.STRXML_SetupId_SimActivityVsDistance) ||
                        specification.SetupId.Equals(Consts.STRXML_SetupId_SimActivityVsTimeNoDelay) ||
                        specification.SetupId.Equals(Consts.STRXML_SetupId_SimActivityVsDistanceNoDelay))
                    {
                        XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_dataType, resultInfo.dataType.ToString(), false);
                        XmlNode xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, Consts.STRXML_dataVector);
                        XmlNode xmlNodeCopy = null;
                        XmlNode xmlNodeTemp = null;
                        for (int i = 0; i < resultInfo.dataVectors.GetLength(0); i++)
                        {
                            if (i == 0)
                            {
                                // Keep a copy of the node for more dataVectors
                                xmlNodeCopy = xmlNode.Clone();
                                xmlNodeTemp = xmlNode;
                            }
                            else
                            {
                                // Get a copy of the nodeCopy to add another dataVector
                                xmlNodeTemp = xmlNodeCopy.Clone();
                            }

                            //
                            // Create CSV string from data vector
                            //
                            string csvString = string.Empty;
                            for (int j = 0; j < resultInfo.dataVectors.GetLength(1); j++)
                            {
                                if (j > 0)
                                {
                                    csvString += Consts.CHR_CsvSplitter;
                                }
                                csvString += resultInfo.dataVectors[i, j].ToString();
                            }
                            xmlNodeTemp.InnerXml = csvString;

                            if (i > 0)
                            {
                                // Append xml fragment to experiment result node
                                this.xmlNodeExperimentResult.AppendChild(xmlNodeTemp);
                            }
                        }
                    }
                }
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
