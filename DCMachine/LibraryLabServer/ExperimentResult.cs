using System;
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
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_speedMin, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_speedMax, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_speedStep, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_fieldMin, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_fieldMax, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_fieldStep, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_loadMin, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_loadMax, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_loadStep, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_speedVector, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_fieldVector, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_voltageVector, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_loadVector, true);
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
                if (specification.SetupId.Equals(Consts.STRXML_SetupId_VoltageVsSpeed))
                {
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_speedMin, specification.Speed.min);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_speedMax, specification.Speed.max);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_speedStep, specification.Speed.step);
                }
                else if (specification.SetupId.Equals(Consts.STRXML_SetupId_VoltageVsField))
                {
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_fieldMin, specification.Field.min);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_fieldMax, specification.Field.max);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_fieldStep, specification.Field.step);
                }
                else if (specification.SetupId.Equals(Consts.STRXML_SetupId_VoltageVsLoad))
                {
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_loadMin, specification.Load.min);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_loadMax, specification.Load.max);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_loadStep, specification.Load.step);
                }
                else if (specification.SetupId.Equals(Consts.STRXML_SetupId_SpeedVsVoltage))
                {
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_speedMin, specification.Speed.min);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_speedMax, specification.Speed.max);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_speedStep, specification.Speed.step);
                }
                else if (specification.SetupId.Equals(Consts.STRXML_SetupId_SpeedVsField))
                {
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_fieldMin, specification.Field.min);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_fieldMax, specification.Field.max);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_fieldStep, specification.Field.step);
                }

                //
                // Add the result information 
                //
                if (resultInfo.statusCode == StatusCodes.Completed)
                {
                    if (specification.SetupId.Equals(Consts.STRXML_SetupId_VoltageVsSpeed))
                    {
                        XmlUtilities.SetXmlValues(this.xmlNodeExperimentResult, Consts.STRXML_speedVector, resultInfo.speedVector, Consts.CHR_Splitter, false);
                        XmlUtilities.SetXmlValues(this.xmlNodeExperimentResult, Consts.STRXML_voltageVector, resultInfo.voltageVector, Consts.CHR_Splitter, false);
                    }
                    else if (specification.SetupId.Equals(Consts.STRXML_SetupId_VoltageVsField))
                    {
                        XmlUtilities.SetXmlValues(this.xmlNodeExperimentResult, Consts.STRXML_fieldVector, resultInfo.fieldVector, "F02", Consts.CHR_Splitter, false);
                        XmlUtilities.SetXmlValues(this.xmlNodeExperimentResult, Consts.STRXML_voltageVector, resultInfo.voltageVector, Consts.CHR_Splitter, false);
                    }
                    else if (specification.SetupId.Equals(Consts.STRXML_SetupId_VoltageVsLoad))
                    {
                        XmlUtilities.SetXmlValues(this.xmlNodeExperimentResult, Consts.STRXML_loadVector, resultInfo.loadVector, Consts.CHR_Splitter, false);
                        XmlUtilities.SetXmlValues(this.xmlNodeExperimentResult, Consts.STRXML_voltageVector, resultInfo.voltageVector, Consts.CHR_Splitter, false);
                        XmlUtilities.SetXmlValues(this.xmlNodeExperimentResult, Consts.STRXML_speedVector, resultInfo.speedVector, Consts.CHR_Splitter, false);
                    }
                    else if (specification.SetupId.Equals(Consts.STRXML_SetupId_SpeedVsVoltage))
                    {
                        XmlUtilities.SetXmlValues(this.xmlNodeExperimentResult, Consts.STRXML_speedVector, resultInfo.speedVector, Consts.CHR_Splitter, false);
                        XmlUtilities.SetXmlValues(this.xmlNodeExperimentResult, Consts.STRXML_voltageVector, resultInfo.voltageVector, Consts.CHR_Splitter, false);
                    }
                    else if (specification.SetupId.Equals(Consts.STRXML_SetupId_SpeedVsField))
                    {
                        XmlUtilities.SetXmlValues(this.xmlNodeExperimentResult, Consts.STRXML_fieldVector, resultInfo.fieldVector, "F02", Consts.CHR_Splitter, false);
                        XmlUtilities.SetXmlValues(this.xmlNodeExperimentResult, Consts.STRXML_speedVector, resultInfo.speedVector, Consts.CHR_Splitter, false);
                        XmlUtilities.SetXmlValues(this.xmlNodeExperimentResult, Consts.STRXML_voltageVector, resultInfo.voltageVector, Consts.CHR_Splitter, false);
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
