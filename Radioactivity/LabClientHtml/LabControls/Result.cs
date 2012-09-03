using System;
using System.Xml;
using Library.Lab;
using Library.LabClient;

namespace LabClientHtml.LabControls
{
    public class Result : ExperimentResult
    {

        //-------------------------------------------------------------------------------------------------//

        public Result(string xmlExperimentResult)
            : base(xmlExperimentResult, new ResultInfo())
        {
            ResultInfo resultInfo = (ResultInfo)this.experimentResultInfo;

            //
            // Parse the experiment result
            //
            try
            {
                //
                // Extract result values from the XML experiment result string and place into ResultInfo
                //

                //
                // Get the source name
                //
                resultInfo.sourceName = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_sourceName, true);

                //
                // Get the absorber list into a one dimension array
                //
                string csvString = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_absorberName, true);
                resultInfo.absorbers = csvString.Split(new char[] { LabConsts.CHR_CsvSplitter });

                //
                // Get the distance list into a one dimension array
                //
                csvString = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_distance, true);
                string[] csvStringSplit = csvString.Split(new char[] { LabConsts.CHR_CsvSplitter });
                resultInfo.distances = new int[csvStringSplit.Length];
                for (int i = 0; i < csvStringSplit.Length; i++)
                {
                    try
                    {
                        resultInfo.distances[i] = Int32.Parse(csvStringSplit[i]);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException(ex.Message, LabConsts.STRXML_distance);
                    }
                }

                // Get the duration
                resultInfo.duration = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_duration, 0);

                // Get the repeat count
                resultInfo.repeat = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_repeat, 0);

                //
                // Get the radioactivity counts into a two dimensional array. Each data vector contains the repeat counts for
                // a particular distance and is provided as a comma-seperated-value string.
                //

                // Get the list of data vectors
                XmlNodeList xmlNodeList = XmlUtilities.GetXmlNodeList(this.xmlNodeExperimentResult, LabConsts.STRXML_dataVector, true);

                // Create a two-dimensional array for the repeat counts for the data vectors
                resultInfo.datavectors = new int[xmlNodeList.Count, resultInfo.repeat];

                // Process each data vector
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    XmlNode xmlNodeTemp = xmlNodeList.Item(i);

                    //
                    // Get the repeat counts from the CSV string and process
                    //
                    csvString = xmlNodeTemp.InnerXml;
                    csvStringSplit = csvString.Split(new char[] { LabConsts.CHR_CsvSplitter });
                    for (int j = 0; j < csvStringSplit.Length; j++)
                    {
                        try
                        {
                            // Save the radioactivity count
                            resultInfo.datavectors[i, j] = Int32.Parse(csvStringSplit[j]);
                        }
                        catch (Exception ex)
                        {
                            throw new ArgumentException(ex.Message, LabConsts.STRXML_distance);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public ResultInfo GetResultInfo()
        {
            return (ResultInfo)this.experimentResultInfo;
        }
    }
}
