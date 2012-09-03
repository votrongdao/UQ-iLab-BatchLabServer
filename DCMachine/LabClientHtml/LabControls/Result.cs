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
                //
                // Extract result values from the XML experiment result string and place into ResultInfo
                //
                resultInfo.speedMin = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_speedMin, 0);
                resultInfo.speedMax = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_speedMax, 0);
                resultInfo.speedStep = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_speedStep, 0);
                resultInfo.fieldMin = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_fieldMin, 0);
                resultInfo.fieldMax = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_fieldMax, 0);
                resultInfo.fieldStep = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_fieldStep, 0);
                resultInfo.loadMin = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_loadMin, 0);
                resultInfo.loadMax = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_loadMax, 0);
                resultInfo.loadStep = XmlUtilities.GetIntValue(this.xmlNodeExperimentResult, LabConsts.STRXML_loadStep, 0);
                resultInfo.speedVector = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_speedVector, true);
                resultInfo.fieldVector = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_fieldVector, true);
                resultInfo.voltageVector = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_voltageVector, true);
                resultInfo.loadVector = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_loadVector, true);
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
