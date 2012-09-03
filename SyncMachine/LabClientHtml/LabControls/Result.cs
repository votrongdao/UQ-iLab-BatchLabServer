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
                XmlNode xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, LabConsts.STRXML_fieldCurrent, true);
                if (xmlNode != null)
                {
                    resultInfo.fieldCurrent.name = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_name, false);
                    resultInfo.fieldCurrent.units = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_units, false);
                    resultInfo.fieldCurrent.format = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_format, false);
                    resultInfo.fieldCurrent.values = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_fieldCurrent, false);
                }

                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, LabConsts.STRXML_speed, true);
                if (xmlNode != null)
                {
                    resultInfo.speed.name = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_name, false);
                    resultInfo.speed.units = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_units, false);
                    resultInfo.speed.format = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_format, false);
                    resultInfo.speed.values = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_speed, false);
                }

                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, LabConsts.STRXML_voltage, true);
                if (xmlNode != null)
                {
                    resultInfo.voltage.name = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_name, false);
                    resultInfo.voltage.units = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_units, false);
                    resultInfo.voltage.format = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_format, false);
                    resultInfo.voltage.values = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_voltage, false);
                }

                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, LabConsts.STRXML_statorCurrent, true);
                if (xmlNode != null)
                {
                    resultInfo.statorCurrent.name = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_name, false);
                    resultInfo.statorCurrent.units = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_units, false);
                    resultInfo.statorCurrent.format = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_format, false);
                    resultInfo.statorCurrent.values = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_statorCurrent, false);
                }

                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, LabConsts.STRXML_speedSetpoint, true);
                if (xmlNode != null)
                {
                    resultInfo.speedSetpoint.name = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_name, false);
                    resultInfo.speedSetpoint.units = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_units, false);
                    resultInfo.speedSetpoint.format = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_format, false);
                    resultInfo.speedSetpoint.values = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_speedSetpoint, false);
                }

                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, LabConsts.STRXML_syncVoltage, true);
                if (xmlNode != null)
                {
                    resultInfo.syncVoltage.name = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_name, false);
                    resultInfo.syncVoltage.units = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_units, false);
                    resultInfo.syncVoltage.format = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_format, false);
                    resultInfo.syncVoltage.values = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_syncVoltage, false);
                }

                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, LabConsts.STRXML_syncFrequency, true);
                if (xmlNode != null)
                {
                    resultInfo.syncFrequency.name = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_name, false);
                    resultInfo.syncFrequency.units = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_units, false);
                    resultInfo.syncFrequency.format = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_format, false);
                    resultInfo.syncFrequency.values = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_syncFrequency, false);
                }

                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, LabConsts.STRXML_mainsVoltage, true);
                if (xmlNode != null)
                {
                    resultInfo.mainsVoltage.name = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_name, false);
                    resultInfo.mainsVoltage.units = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_units, false);
                    resultInfo.mainsVoltage.format = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_format, false);
                    resultInfo.mainsVoltage.values = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_mainsVoltage, false);
                }

                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, LabConsts.STRXML_mainsFrequency, true);
                if (xmlNode != null)
                {
                    resultInfo.mainsFrequency.name = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_name, false);
                    resultInfo.mainsFrequency.units = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_units, false);
                    resultInfo.mainsFrequency.format = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_format, false);
                    resultInfo.mainsFrequency.values = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_mainsFrequency, false);
                }

                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, LabConsts.STRXML_syncMainsPhase, true);
                if (xmlNode != null)
                {
                    resultInfo.syncMainsPhase.name = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_name, false);
                    resultInfo.syncMainsPhase.units = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_units, false);
                    resultInfo.syncMainsPhase.format = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_format, false);
                    resultInfo.syncMainsPhase.values = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_syncMainsPhase, false);
                }

                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, LabConsts.STRXML_synchronism, true);
                if (xmlNode != null)
                {
                    resultInfo.synchronism.name = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_name, false);
                    resultInfo.synchronism.values = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_synchronism, false);
                }

                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, LabConsts.STRXML_torqueSetpoint, true);
                if (xmlNode != null)
                {
                    resultInfo.torqueSetpoint.name = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_name, false);
                    resultInfo.torqueSetpoint.units = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_units, false);
                    resultInfo.torqueSetpoint.format = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_format, false);
                    resultInfo.torqueSetpoint.values = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_torqueSetpoint, false);
                }

                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, LabConsts.STRXML_powerFactor, true);
                if (xmlNode != null)
                {
                    resultInfo.powerFactor.name = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_name, false);
                    resultInfo.powerFactor.values = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_powerFactor, false);
                }

                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, LabConsts.STRXML_realPower, true);
                if (xmlNode != null)
                {
                    resultInfo.realPower.name = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_name, false);
                    resultInfo.realPower.units = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_units, false);
                    resultInfo.realPower.format = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_format, false);
                    resultInfo.realPower.values = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_realPower, false);
                }

                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, LabConsts.STRXML_reactivePower, true);
                if (xmlNode != null)
                {
                    resultInfo.reactivePower.name = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_name, false);
                    resultInfo.reactivePower.units = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_units, false);
                    resultInfo.reactivePower.format = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_format, false);
                    resultInfo.reactivePower.values = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_reactivePower, false);
                }

                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeExperimentResult, LabConsts.STRXML_phaseCurrent, true);
                if (xmlNode != null)
                {
                    resultInfo.phaseCurrent.name = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_name, false);
                    resultInfo.phaseCurrent.units = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_units, false);
                    resultInfo.phaseCurrent.format = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_format, false);
                    resultInfo.phaseCurrent.values = XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, LabConsts.STRXML_phaseCurrent, false);
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
