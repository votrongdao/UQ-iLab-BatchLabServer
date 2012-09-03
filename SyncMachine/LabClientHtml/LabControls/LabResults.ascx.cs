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
        private const string STR_NameUnits_fmt = "{0} ({1})";
        private const string STR_Name_fmt = "{0}";

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
            ResultInfo.Measurement[] measurements = null;
            StringWriter sw = new StringWriter();
            try
            {
                if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_OpenCircuitVaryField))
                {
                    measurements = new ResultInfo.Measurement[] {
                        resultInfo.fieldCurrent,
                        resultInfo.speed,
                        resultInfo.voltage,
                    };
                }
                else if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_OpenCircuitVarySpeed))
                {
                    measurements = new ResultInfo.Measurement[] {
                        resultInfo.speed,
                        resultInfo.fieldCurrent,
                        resultInfo.voltage,
                    };
                }
                else if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_ShortCircuitVaryField))
                {
                    measurements = new ResultInfo.Measurement[] {
                        resultInfo.fieldCurrent,
                        resultInfo.speed,
                        resultInfo.statorCurrent,
                    };
                }
                else if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_PreSynchronisation))
                {
                    measurements = new ResultInfo.Measurement[] {
                        resultInfo.fieldCurrent,
                        resultInfo.speedSetpoint,
                        resultInfo.mainsVoltage,
                        resultInfo.mainsFrequency,
                        resultInfo.syncVoltage,
                        resultInfo.syncFrequency,
                        resultInfo.syncMainsPhase,
                        resultInfo.synchronism,
                    };
                }
                else if (resultInfo.setupId.Equals(LabConsts.STRXML_SetupId_Synchronisation))
                {
                    measurements = new ResultInfo.Measurement[] {
                        resultInfo.torqueSetpoint,
                        resultInfo.fieldCurrent,
                        resultInfo.syncVoltage,
                        resultInfo.syncFrequency,
                        resultInfo.powerFactor,
                        resultInfo.realPower,
                        resultInfo.reactivePower,
                        resultInfo.phaseCurrent,
                    };
                }

                for (int i = 0; i < measurements.Length; i++)
                {
                    ResultInfo.Measurement measurement = measurements[i];

                    //
                    // Check if the units have been specified
                    //
                    string title;
                    if (measurement.units == null || measurement.units.Trim().Length == 0)
                    {
                        // Units not specified
                        title = string.Format(STR_Name_fmt, measurement.name);
                    }
                    else
                    {
                        // Units specified
                        title = string.Format(STR_NameUnits_fmt, measurement.name, measurement.units);
                    }
                    sw.WriteLine(swArgument, title, this.ReformatValues(measurement.values));
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
        /// Take the comma-seperated value string and split it into an array of string values. Recreate the
        /// comma-seperated value string but with a space character following each comma. This will allow the
        /// value string to wrap in the LabClient's web page.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        string ReformatValues(string values)
        {
            string[] splitValues = values.Split(new char[] { ',' });
            string reformattedValues = string.Empty;
            for (int i = 0; i < splitValues.Length; i++)
            {
                if (i == 0)
                {
                    reformattedValues = splitValues[i];
                }
                else
                {
                    reformattedValues += ", ";
                    reformattedValues += splitValues[i];
                }
            }

            return reformattedValues;
        }
    }

}