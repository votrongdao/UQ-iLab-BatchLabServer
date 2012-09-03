using System;
using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Equipment;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServer.Drivers.Setup;

namespace Library.LabServer
{
    public class Specification : ExperimentSpecification
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "Specification";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_serverUrl = " serverUrl: ";
        private const string STRLOG_timeout = " timeout: ";
        private const string STRLOG_formatName = " formatName: ";

        //
        // String constants for error messages
        //
        private const string STRERR_InvalidTimeFormat = "Invalid time format";

        //
        // Local variables
        //
        private Configuration configuration;

        #endregion

        #region Properties

        private string serverUrl;
        private string formatName;

        public string ServerUrl
        {
            get { return this.serverUrl; }
        }

        public string FormatName
        {
            get { return this.formatName; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public Specification(Configuration configuration, EquipmentService equipmentServiceProxy)
            : base(configuration, equipmentServiceProxy)
        {
            const string STRLOG_MethodName = "Specification";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Save these for use by the Parse() method
            //
            this.configuration = configuration;

            //
            // Check that the specification template is valid. This is used by the LabClient to submit
            // the experiment specification to the LabServer for execution.
            //
            try
            {
                //
                // Check that all required XML nodes exist
                //
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_serverUrl, true);
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_formatName, true);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Parse the XML specification string to check its validity. No exceptions are thrown back to the
        /// calling method. If an error occurs, 'accepted' is set to false and the error message is placed
        /// in 'errorMessage' where it can be examined by the calling method.
        /// </summary>
        /// <param name="xmlSpecification"></param>
        public override ValidationReport Parse(string xmlSpecification)
        {
            const string STRLOG_MethodName = "Parse";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Catch all exceptions and log errors, don't throw back to caller
            //
            ValidationReport validationReport = null;
            try
            {
                //
                // Call the base class to parse its part
                //
                validationReport = base.Parse(xmlSpecification);
                if (validationReport.accepted == false)
                {
                    throw new Exception(validationReport.errorMessage);
                }

                // Create new validation report
                validationReport = new ValidationReport();

                //
                // Get the time format name and check that it is valid - search is case-sensitive
                //
                string formatName = XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_formatName, false);
                int index = Array.IndexOf(this.configuration.TimeFormats, formatName);
                if (index < 0)
                {
                    throw new ArgumentException(STRERR_InvalidTimeFormat, formatName);
                }
                this.formatName = this.configuration.TimeFormats[index];

                //
                // Create an instance of the driver for the specified setup and then
                // get the driver's execution time for this specification
                //
                int executionTime = -1;
                if (this.setupId.Equals(Consts.STRXML_SetupId_LocalClock))
                {
                    DriverLocal driver = new DriverLocal(this.configuration);
                    executionTime = driver.GetExecutionTime(this);
                }
                else if (this.setupId.Equals(Consts.STRXML_SetupId_NTPServer))
                {
                    // Get the server URL from the specification
                    this.serverUrl = XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_serverUrl, false);

                    DriverNetwork driver = new DriverNetwork(this.equipmentServiceProxy, this.configuration);
                    executionTime = driver.GetExecutionTime(this);
                }

                //
                // Specification is valid
                //
                validationReport.estRuntime = executionTime;
                validationReport.accepted = true;
            }
            catch (Exception ex)
            {
                validationReport.errorMessage = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Accepted + validationReport.accepted.ToString();
            if (validationReport.accepted == true)
            {
                logMessage += Logfile.STRLOG_Spacer + STRLOG_ExecutionTime + validationReport.estRuntime.ToString() + STRLOG_seconds;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return validationReport;
        }

    }
}
