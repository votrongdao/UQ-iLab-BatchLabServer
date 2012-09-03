using System;
using System.Xml;
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
                //
                // Nothing to do here
                //
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
                //
                // Nothing to do here
                //

                //
                // Add the result information 
                //
                if (resultInfo.statusCode == StatusCodes.Completed)
                {
                    string[] measurements = null;

                    //
                    // Load XML measurements string
                    //
                    XmlDocument xmlDocument = XmlUtilities.GetXmlDocument(resultInfo.xmlMeasurements);
                    XmlNode xmlNodeRoot = XmlUtilities.GetXmlRootNode(xmlDocument, Consts.STRXML_measurements);

                    if (specification.SetupId.Equals(Consts.STRXML_SetupId_OpenCircuitVaryField) == true)
                    {
                        measurements = new string[] {
                            Consts.STRXML_fieldCurrent,
                            Consts.STRXML_speed,
                            Consts.STRXML_voltage,
                        };
                    }
                    else if (specification.SetupId.Equals(Consts.STRXML_SetupId_OpenCircuitVarySpeed) == true)
                    {
                        measurements = new string[] {
                            Consts.STRXML_speed,
                            Consts.STRXML_fieldCurrent,
                            Consts.STRXML_voltage,
                        };
                    }
                    else if (specification.SetupId.Equals(Consts.STRXML_SetupId_ShortCircuitVaryField) == true)
                    {
                        measurements = new string[] {
                            Consts.STRXML_fieldCurrent,
                            Consts.STRXML_speed,
                            Consts.STRXML_statorCurrent,
                        };
                    }
                    else if (specification.SetupId.Equals(Consts.STRXML_SetupId_PreSynchronisation) == true)
                    {
                        measurements = new string[] {
                            Consts.STRXML_fieldCurrent,
                            Consts.STRXML_speedSetpoint,
                            Consts.STRXML_mainsVoltage,
                            Consts.STRXML_mainsFrequency,
                            Consts.STRXML_syncVoltage,
                            Consts.STRXML_syncFrequency,
                            Consts.STRXML_syncMainsPhase,
                            Consts.STRXML_synchronism,
                        };
                    }
                    else if (specification.SetupId.Equals(Consts.STRXML_SetupId_Synchronisation) == true)
                    {
                        measurements = new string[] {
                            Consts.STRXML_torqueSetpoint,
                            Consts.STRXML_fieldCurrent,
                            Consts.STRXML_syncVoltage,
                            Consts.STRXML_syncFrequency,
                            Consts.STRXML_powerFactor,
                            Consts.STRXML_realPower,
                            Consts.STRXML_reactivePower,
                            Consts.STRXML_phaseCurrent,
                        };
                    }

                    if (measurements != null)
                    {
                        for (int i = 0; i < measurements.Length; i++)
                        {
                            XmlNode xmlNode = XmlUtilities.GetXmlNode(xmlNodeRoot, measurements[i]);
                            XmlDocumentFragment xmlFragment = this.xmlNodeExperimentResult.OwnerDocument.CreateDocumentFragment();
                            xmlFragment.InnerXml = xmlNode.OuterXml;
                            this.xmlNodeExperimentResult.AppendChild(xmlFragment);
                        }
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
