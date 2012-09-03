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
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_formatName, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_serverUrl, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_timeofday, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_dayofweek, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_day, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_month, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_year, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_hours, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_minutes, true);
                XmlUtilities.GetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_seconds, true);
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
                XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_formatName, specification.FormatName, false);
                if (specification.SetupId.Equals(Consts.STRXML_SetupId_NTPServer))
                {
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_serverUrl, specification.ServerUrl, false);
                }

                //
                // Add the result information 
                //
                if (resultInfo.statusCode == StatusCodes.Completed)
                {
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_timeofday, resultInfo.timeofday, false);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_dayofweek, resultInfo.dateTime.DayOfWeek.ToString(), false);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_day, resultInfo.dateTime.Day);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_month, resultInfo.dateTime.Month);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_year, resultInfo.dateTime.Year);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_hours, resultInfo.dateTime.Hour);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_minutes, resultInfo.dateTime.Minute);
                    XmlUtilities.SetXmlValue(this.xmlNodeExperimentResult, Consts.STRXML_seconds, resultInfo.dateTime.Second);
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
