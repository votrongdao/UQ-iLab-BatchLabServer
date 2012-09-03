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
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_voltage, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_current, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_powerFactor, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_speed, true);
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
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_voltage, resultInfo.voltage.ToString("F02"), false);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_current, resultInfo.current.ToString("F03"), false);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_powerFactor, resultInfo.powerFactor.ToString("F04"), false);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_speed, resultInfo.speed.ToString(), false);
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
