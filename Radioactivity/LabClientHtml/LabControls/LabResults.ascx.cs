using System;
using System.IO;
using System.Xml;
using Library.Lab;
using Library.LabClient;

namespace LabClientHtml.LabControls
{
    public partial class LabResults : System.Web.UI.UserControl
    {
        #region Class Constants and Variables

        //
        // String constants
        //
        private const string STR_Source = "Source";
        private const string STR_Absorber = "Absorber";
        private const string STR_AbsorberList = "Absorber List";
        private const string STR_Distance = "Distance (mm)";
        private const string STR_DistanceList = "Distance List (mm)";
        private const string STR_Duration = "Duration (secs)";
        private const string STR_Trials = "Trials";
        private const string STR_CountsAtDistance = "Counts at distance";
        private const string STR_CountsForAbsorber = "Counts for absorber";
        private const string STR_Millimetres = "mm";

        #endregion

        //-------------------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Create a string which represents the experiment specification. Each line contains two fields
        /// which are the name of the field and its value. The format of the string will be different
        /// for comma-seperated-values and applet parameters.
        /// </summary>
        /// <param name="xmlNodeExperimentResult"></param>
        /// <param name="swArgument"></param>
        /// <returns></returns>
        public string CreateSpecificationString(ResultInfo resultInfo, string swArgument)
        {
            StringWriter sw = new StringWriter();
            try
            {
                // Write the source name
                sw.WriteLine(swArgument, STR_Source, resultInfo.sourceName);

                //
                // Create a CSV string of absorbers from the absorber list
                //
                string csvAbsorbers = string.Empty;
                for (int i = 0; i < resultInfo.absorbers.Length; i++)
                {
                    if (i > 0)
                    {
                        csvAbsorbers += LabConsts.CHR_CsvSplitter.ToString();
                    }
                    csvAbsorbers += resultInfo.absorbers[i].ToString();
                }

                // Write the absorbers
                if (resultInfo.absorbers.Length > 1)
                {
                    // Write as an absorber list
                    sw.WriteLine(swArgument, STR_AbsorberList, csvAbsorbers);
                }
                else
                {
                    // Write as a single absorber
                    sw.WriteLine(swArgument, STR_Absorber, csvAbsorbers);
                }

                //
                // Create a CSV string of distances from the distance list
                //
                string csvDistances = string.Empty;
                for (int i = 0; i < resultInfo.distances.Length; i++)
                {
                    if (i > 0)
                    {
                        csvDistances += LabConsts.CHR_CsvSplitter.ToString();
                    }
                    csvDistances += resultInfo.distances[i].ToString();
                }

                // Write the distances
                if (resultInfo.distances.Length > 1)
                {
                    // Write as a distance list
                    sw.WriteLine(swArgument, STR_DistanceList, csvDistances);
                }
                else
                {
                    // Write as a single distance
                    sw.WriteLine(swArgument, STR_Distance, csvDistances);
                }

                // Write the duration
                sw.WriteLine(swArgument, STR_Duration, resultInfo.duration);

                // Write the repeat count (number of trials)
                sw.WriteLine(swArgument, STR_Trials, resultInfo.repeat);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            return sw.ToString();
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Create a string which represents the experiment result. Each line contains two fields
        /// which are the name of the field and its value. The format of the string will be different
        /// for comma-seperated-values and applet parameters.
        /// </summary>
        /// <param name="xmlNodeExperimentResult"></param>
        /// <param name="swArgument"></param>
        /// <returns></returns>
        public string CreateResultsString(ResultInfo resultInfo, string swArgument)
        {
            StringWriter sw = new StringWriter();
            try
            {
                if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_RadioactivityVsAbsorber) == true)
                {
                    sw.WriteLine(swArgument, STR_CountsForAbsorber, string.Empty);

                    for (int i = 0; i < resultInfo.datavectors.GetLength(0); i++)
                    {
                        //
                        // Create a CSV string of radioactivity counts from the data vector
                        //
                        string csvCounts = string.Empty;
                        for (int j = 0; j < resultInfo.datavectors.GetLength(1); j++)
                        {
                            if (j > 0)
                            {
                                csvCounts += LabConsts.CHR_CsvSplitter.ToString();
                            }
                            csvCounts += resultInfo.datavectors[i, j].ToString();
                        }

                        // Write the string of counts showing the absorber
                        sw.WriteLine(swArgument, resultInfo.absorbers[i], csvCounts);
                    }
                }
                else
                {
                    sw.WriteLine(swArgument, STR_CountsAtDistance, string.Empty);

                    for (int i = 0; i < resultInfo.datavectors.GetLength(0); i++)
                    {
                        //
                        // Create a CSV string of radioactivity counts from the data vector
                        //
                        string csvCounts = string.Empty;
                        for (int j = 0; j < resultInfo.datavectors.GetLength(1); j++)
                        {
                            if (j > 0)
                            {
                                csvCounts += LabConsts.CHR_CsvSplitter.ToString();
                            }
                            csvCounts += resultInfo.datavectors[i, j].ToString();
                        }

                        // Write the string of counts showing the distance
                        sw.WriteLine(swArgument, resultInfo.distances[i].ToString() + STR_Millimetres, csvCounts);
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            return sw.ToString();
        }
    }

}