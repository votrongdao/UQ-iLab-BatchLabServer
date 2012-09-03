using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Xml;
using Library.Lab;

namespace Library.LabServer.Drivers.Module
{
    public class Simulation
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "Simulation";

        //
        // Constants
        //

        //
        // String constants for logfile messages
        //
        private const string STRLOG_Filename = " Filename: ";
        private const string STRLOG_ParsingSimulationConfig = " Parsing SimulationConfig...";
        private const string STRLOG_Title = " Title: ";
        private const string STRLOG_Version = " Version: ";
        private const string STRLOG_SimulateDelays = " SimulateDelays: ";

        //
        // Local variables
        //

        #endregion

        #region Properties

        private string filename;
        private string title;
        private string version;
        private bool simulateDelays;

        public string Filename
        {
            get { return this.filename; }
        }

        public string Title
        {
            get { return this.title; }
        }

        public string Version
        {
            get { return this.version; }
        }

        public bool SimulateDelays
        {
            get { return this.simulateDelays; }
            set { this.simulateDelays = value; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public Simulation(Configuration configuration, bool simulateDelays)
            : this(configuration, simulateDelays, null, null)
        {
        }

        //---------------------------------------------------------------------------------------//

        public Simulation(Configuration configuration, bool simulateDelays, string xmlSimulationConfig)
            : this(configuration, simulateDelays, xmlSimulationConfig, null)
        {
        }

        //---------------------------------------------------------------------------------------//

        public Simulation(Configuration configuration, bool simulateDelays, string xmlSimulationConfig, string simulationConfigFilename)
        {

            const string STRLOG_MethodName = "Simulation";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            this.simulateDelays = simulateDelays;

            try
            {
                XmlDocument xmlDocument;

                //
                // Check if an XML simulation configuration string is specified
                //
                if (xmlSimulationConfig != null)
                {
                    //
                    // Load the simulation configuration from an XML string
                    //
                    xmlDocument = XmlUtilities.GetXmlDocument(xmlSimulationConfig);
                }
                else
                {
                    //
                    // Check if an XML simulation configuration filename is specified
                    //
                    if (simulationConfigFilename == null)
                    {
                        //
                        // Get simulation configuration filename from Application's configuration file
                        //
                        this.filename = Utilities.GetAppSetting(Consts.STRCFG_XmlSimulationConfigFilename);
                        this.filename = Path.Combine(configuration.RootFilePath, this.filename);
                    }
                    else
                    {
                        // Prepend full file path
                        this.filename = Path.Combine(configuration.RootFilePath, simulationConfigFilename);
                    }

                    Logfile.Write(STRLOG_Filename + this.filename);

                    // Load the simulation configuration from the specified file
                    xmlDocument = XmlUtilities.GetXmlDocumentFromFile(this.filename);
                }

                // Get the simulation configuration XML node
                XmlNode xmlNodeSimulationConfig = XmlUtilities.GetXmlRootNode(xmlDocument, Consts.STRXML_simulationConfig);

                Logfile.Write(STRLOG_ParsingSimulationConfig);

                //
                // Get information from the simulation configuration node
                //
                this.title = XmlUtilities.GetXmlValue(xmlNodeSimulationConfig, Consts.STRXMLPARAM_title, false);
                this.version = XmlUtilities.GetXmlValue(xmlNodeSimulationConfig, Consts.STRXMLPARAM_version, false);

                //
                // YOUR CODE HERE
                //
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
