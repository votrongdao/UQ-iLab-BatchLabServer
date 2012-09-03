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
        // Constants
        //
 
        //
        // String constants for logfile messages
        //

        //
        // String constants for error messages
        //
        private const string STRERR_InvalidSource = "Invalid source";
        private const string STRERR_InvalidAbsorber = "Invalid absorber";

        //
        // Local variables
        //
        private Configuration configuration;
        private Validation validation;

        #endregion

        #region Properties

        public struct Absorber
        {
            public string name;
            public char location;
            public Absorber(string name, char location)
            {
                this.name = name;
                this.location = location;
            }
        }
        private string sourceName;
        private char sourceLocation;
        private Absorber[] absorberList;
        private int[] distanceList;
        private int duration;
        private int repeat;

        public string SourceName
        {
            get { return this.sourceName; }
        }

        public char SourceLocation
        {
            get { return this.sourceLocation; }
        }

        public Absorber[] AbsorberList
        {
            get { return this.absorberList; }
        }

        public int[] DistanceList
        {
            get { return this.distanceList; }
        }

        public int Duration
        {
            get { return this.duration; }
        }

        public int Repeat
        {
            get { return this.repeat; }
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
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_sourceName, true);
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_absorberName, true);
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_distance, true);
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_duration, true);
                XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_repeat, true);

                //
                // Create an instance fo the Validation class
                //
                this.validation = new Validation(configuration);
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
                // Validate the specification
                //

                //
                // Get the source name and check that it is valid - search is case-sensitive
                //
                string strSourceName = XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_sourceName, false);
                int index = Array.IndexOf(this.configuration.SourceNames, strSourceName);
                if (index < 0)
                {
                    throw new ArgumentException(STRERR_InvalidSource, strSourceName);
                }
                this.sourceName = this.configuration.SourceNames[index];
                this.sourceLocation = this.configuration.SourceLocations[index];

                //
                // Get the absorber list and validate - search is case-sensitive
                //
                string csvAbsorbers = XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_absorberName, false);
                string[] csvAbsorbersSplit = csvAbsorbers.Split(new char[] { Consts.CHR_CsvSplitter });
                this.absorberList = new Absorber[csvAbsorbersSplit.Length];
                for (int i = 0; i < csvAbsorbersSplit.Length; i++)
                {
                    index = Array.IndexOf(this.configuration.AbsorberNames, csvAbsorbersSplit[i]);
                    if (index < 0)
                    {
                        throw new ArgumentException(STRERR_InvalidAbsorber, csvAbsorbersSplit[i]);
                    }
                    string name = this.configuration.AbsorberNames[index];
                    char location = this.configuration.AbsorberLocations[index];
                    this.absorberList[i] = new Absorber(name, location);
                }

                //
                // Get duration and validate
                //
                this.duration = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_duration);
                this.validation.ValidateDuration(this.duration);

                //
                // Get repeat count and validate
                //
                this.repeat = XmlUtilities.GetIntValue(this.xmlNodeSpecification, Consts.STRXML_repeat);
                this.validation.ValidateRepeat(this.repeat);

                //
                // Get distance list and validate
                //
                string csvDistances = XmlUtilities.GetXmlValue(this.xmlNodeSpecification, Consts.STRXML_distance, false);
                string[] csvDistancesSplit = csvDistances.Split(new char[] { Consts.CHR_CsvSplitter });
                this.distanceList = new int[csvDistancesSplit.Length];
                for (int i = 0; i < csvDistancesSplit.Length; i++)
                {
                    try
                    {
                        this.distanceList[i] = Int32.Parse(csvDistancesSplit[i]);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException(ex.Message, Consts.STRXML_distance);
                    }
                    this.validation.ValidateDistance(this.distanceList[i]);
                }

                //
                // Sort the distance list with smallest distance first keeping duplicates
                //
                Array.Sort(this.distanceList);

                //
                // Create an instance of the driver for the specified setup and then
                // get the driver's execution time for this specification
                //
                int executionTime = -1;
                if (this.SetupId.Equals(Consts.STRXML_SetupId_RadioactivityVsTime) ||
                    this.SetupId.Equals(Consts.STRXML_SetupId_RadioactivityVsDistance))
                {
                    if (this.equipmentServiceProxy != null)
                    {
                        //
                        // Hardware is available to this unit, run it there
                        //
                        DriverRadioactivity driver = new DriverRadioactivity(this.equipmentServiceProxy, this.configuration);
                        executionTime = driver.GetExecutionTime(this);
                    }
                    else
                    {
                        //
                        // This unit does not have hardware available, run the simulation instead
                        //
                        DriverSimActivity driver = new DriverSimActivity(this.configuration);
                        executionTime = driver.GetExecutionTime(this);
                    }
                }
                else if (this.SetupId.Equals(Consts.STRXML_SetupId_RadioactivityVsAbsorber))
                {
                    if (this.equipmentServiceProxy != null)
                    {
                        //
                        // Hardware is available to this unit, run it there
                        //
                        DriverAbsorbers driver = new DriverAbsorbers(this.equipmentServiceProxy, this.configuration);
                        executionTime = driver.GetExecutionTime(this);
                    }
                    else
                    {
                        throw new ArgumentException(STRERR_EquipmentServiceNotAvailable, this.setupId);
                    }
                }
                else if (this.SetupId.Equals(Consts.STRXML_SetupId_SimActivityVsTime) ||
                    this.SetupId.Equals(Consts.STRXML_SetupId_SimActivityVsDistance))
                {
                    DriverSimActivity driver = new DriverSimActivity(this.configuration);
                    executionTime = driver.GetExecutionTime(this);
                }
                else if (this.SetupId.Equals(Consts.STRXML_SetupId_SimActivityVsTimeNoDelay) ||
                    this.SetupId.Equals(Consts.STRXML_SetupId_SimActivityVsDistanceNoDelay))
                {
                    DriverSimActivity driver = new DriverSimActivity(this.configuration, false);
                    executionTime = driver.GetExecutionTime(this);
                }
                else
                {
                    throw new ArgumentException(STRERR_SetupIdInvalid, this.SetupId);
                }

                // Validate total execution time
                this.validation.ValidateTotalTime(executionTime);

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
