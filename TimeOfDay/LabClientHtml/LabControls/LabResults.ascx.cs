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
                if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_NTPServer))
                {
                    if (resultInfo.title != null)
                    {
                        // Write the server url
                        sw.WriteLine(swArgument, LabConsts.STR_Server_Url, resultInfo.serverUrl);
                    }
                    else
                    {
                        // Previous version - write the server name
                        sw.WriteLine(swArgument, LabConsts.STR_TimeServer, resultInfo.serverName);
                    }
                }

                // Write the format name
                sw.WriteLine(swArgument, LabConsts.STR_TimeFormat, resultInfo.formatName);
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
                if (resultInfo.title != null)
                {
                    sw.WriteLine(swArgument, LabConsts.STR_TimeOfDay, resultInfo.timeofday);
                }
                sw.WriteLine(swArgument, LabConsts.STR_DayOfWeek, resultInfo.dayofweek);
                sw.WriteLine(swArgument, LabConsts.STR_Day, resultInfo.day);
                sw.WriteLine(swArgument, LabConsts.STR_Month, resultInfo.month);
                sw.WriteLine(swArgument, LabConsts.STR_Year, resultInfo.year);
                sw.WriteLine(swArgument, LabConsts.STR_Hours, resultInfo.hours);
                sw.WriteLine(swArgument, LabConsts.STR_Minutes, resultInfo.minutes);
                sw.WriteLine(swArgument, LabConsts.STR_Seconds, resultInfo.seconds);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            return sw.ToString();
        }
    }

}