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
                //
                // Nothing to do here
                //
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
                if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_LockedRotor))
                {
                    sw.WriteLine(swArgument, LabConsts.STR_PhasePhaseVoltage, resultInfo.voltage);
                    sw.WriteLine(swArgument, LabConsts.STR_PhaseCurrent, resultInfo.current);
                    sw.WriteLine(swArgument, LabConsts.STR_PowerFactor, resultInfo.powerFactor);
                    if (resultInfo.title != null)
                    {
                        sw.WriteLine(swArgument, LabConsts.STR_MotorSpeed, resultInfo.speed);
                    }
                }
                else if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_NoLoad))
                {
                    sw.WriteLine(swArgument, LabConsts.STR_PhasePhaseVoltage, resultInfo.voltage);
                    sw.WriteLine(swArgument, LabConsts.STR_PhaseCurrent, resultInfo.current);
                    sw.WriteLine(swArgument, LabConsts.STR_PowerFactor, resultInfo.powerFactor);
                    if (resultInfo.title != null)
                    {
                        sw.WriteLine(swArgument, LabConsts.STR_MotorSpeed, resultInfo.speed);
                    }
                }
                else if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_SynchronousSpeed))
                {
                    sw.WriteLine(swArgument, LabConsts.STR_PhasePhaseVoltage, resultInfo.voltage);
                    sw.WriteLine(swArgument, LabConsts.STR_PhaseCurrent, resultInfo.current);
                    sw.WriteLine(swArgument, LabConsts.STR_PowerFactor, resultInfo.powerFactor);
                    if (resultInfo.title != null)
                    {
                        sw.WriteLine(swArgument, LabConsts.STR_MotorSpeed, resultInfo.speed);
                    }
                }
                else if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_FullLoad))
                {
                    sw.WriteLine(swArgument, LabConsts.STR_PhasePhaseVoltage, resultInfo.voltage);
                    sw.WriteLine(swArgument, LabConsts.STR_PhaseCurrent, resultInfo.current);
                    sw.WriteLine(swArgument, LabConsts.STR_PowerFactor, resultInfo.powerFactor);
                    if (resultInfo.title != null)
                    {
                        sw.WriteLine(swArgument, LabConsts.STR_MotorSpeed, resultInfo.speed);
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