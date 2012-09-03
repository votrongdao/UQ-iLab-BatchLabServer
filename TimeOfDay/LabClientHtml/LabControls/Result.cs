using System;
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
                try
                {
                    resultInfo.serverUrl = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_serverUrl, true);
                    resultInfo.timeofday = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_timeofday, true);
                }
                catch
                {
                    // Previous version
                    resultInfo.serverName = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_serverName, true);
                }
                resultInfo.formatName = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_formatName, true);
                resultInfo.dayofweek = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_dayofweek, true);
                resultInfo.day = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_day, 0);
                resultInfo.month = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_month, 0);
                resultInfo.year = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_year, 0);
                resultInfo.hours = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_hours, 0);
                resultInfo.minutes = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_minutes, 0);
                resultInfo.seconds = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_seconds, 0);
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
