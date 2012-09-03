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
                if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_VoltageVsSpeed) == true ||
                    resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_SpeedVsVoltage) == true)
                {
                    sw.WriteLine(swArgument, LabConsts.STR_MinSpeed, resultInfo.speedMin);
                    sw.WriteLine(swArgument, LabConsts.STR_MaxSpeed, resultInfo.speedMax);
                    sw.WriteLine(swArgument, LabConsts.STR_SpeedStep, resultInfo.speedStep);
                }
                else if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_VoltageVsField) == true ||
                    resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_SpeedVsField) == true)
                {
                    sw.WriteLine(swArgument, LabConsts.STR_MinField, resultInfo.fieldMin);
                    sw.WriteLine(swArgument, LabConsts.STR_MaxField, resultInfo.fieldMax);
                    sw.WriteLine(swArgument, LabConsts.STR_FieldStep, resultInfo.fieldStep);
                }
                else if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_VoltageVsLoad) == true)
                {
                    sw.WriteLine(swArgument, LabConsts.STR_MinLoad, resultInfo.loadMin);
                    sw.WriteLine(swArgument, LabConsts.STR_MaxLoad, resultInfo.loadMax);
                    sw.WriteLine(swArgument, LabConsts.STR_LoadStep, resultInfo.loadStep);
                }
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
                if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_VoltageVsSpeed))
                {
                    sw.WriteLine(swArgument, LabConsts.STR_MotorSpeed, resultInfo.speedVector);
                    sw.WriteLine(swArgument, LabConsts.STR_ArmatureVoltage, resultInfo.voltageVector);
                }
                else if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_VoltageVsField))
                {
                    sw.WriteLine(swArgument, LabConsts.STR_FieldCurrent, resultInfo.fieldVector);
                    sw.WriteLine(swArgument, LabConsts.STR_ArmatureVoltage, resultInfo.voltageVector);
                }
                else if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_VoltageVsLoad))
                {
                    sw.WriteLine(swArgument, LabConsts.STR_LoadTorque, resultInfo.loadVector);
                    sw.WriteLine(swArgument, LabConsts.STR_ArmatureVoltage, resultInfo.voltageVector);
                    sw.WriteLine(swArgument, LabConsts.STR_MotorSpeed, resultInfo.speedVector);
                }
                else if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_SpeedVsVoltage))
                {
                    sw.WriteLine(swArgument, LabConsts.STR_ArmatureVoltage, resultInfo.voltageVector);
                    sw.WriteLine(swArgument, LabConsts.STR_MotorSpeed, resultInfo.speedVector);
                }
                else if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_SpeedVsField))
                {
                    sw.WriteLine(swArgument, LabConsts.STR_FieldCurrent, resultInfo.fieldVector);
                    sw.WriteLine(swArgument, LabConsts.STR_MotorSpeed, resultInfo.speedVector);
                    sw.WriteLine(swArgument, LabConsts.STR_ArmatureVoltage, resultInfo.voltageVector);
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