using System;
using System.Xml;
using Library.Lab;
using Library.LabServerEngine;

namespace Library.LabServer
{
    public class Configuration : LabConfiguration
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "Configuration";

        //
        // String constants for the XML lab configuration
        //
        private const string STRXML_timeFormats = "timeFormats";
        private const string STRXML_timeFormat = "timeFormat";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_TimeFormat = " TimeFormat: ";

        //
        // String constants for error messages
        //

        #endregion

        #region Properties

        private string[] timeFormats;

        public string[] TimeFormats
        {
            get { return this.timeFormats; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public Configuration(string rootFilePath)
            : this(rootFilePath, null)
        {
        }

        //---------------------------------------------------------------------------------------//

        public Configuration(string rootFilePath, string xmlLabConfiguration)
            : base(rootFilePath, xmlLabConfiguration)
        {
            const string STRLOG_MethodName = "Configuration";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            try
            {
                //
                // Get a list of all time formats, must have at least one
                //
                XmlNode xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeConfiguration, STRXML_timeFormats);
                this.timeFormats = XmlUtilities.GetXmlValues(xmlNode, STRXML_timeFormat, false);

                for (int i = 0; i < this.timeFormats.Length; i++)
                {
                    Logfile.Write(STRLOG_TimeFormat + this.timeFormats[i]);
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
