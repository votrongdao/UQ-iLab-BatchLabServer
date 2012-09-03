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
        private const string STRXML_sources = "sources";
        private const string STRXML_source = "source";
        private const string STRXML_absorbers = "absorbers";
        private const string STRXML_absorber = "absorber";
        private const string STRXML_name = "name";
        private const string STRXML_location = "location";
        private const string STRXMLPARAM_default = "@default";
        private const string STRXML_distances = "distances";
        private const string STRXML_minimum = "minimum";
        private const string STRXML_maximum = "maximum";
        private const string STRXML_stepsize = "stepsize";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_Source = " Source: ";
        private const string STRLOG_Absorber = " Absorber: ";
        private const string STRLOG_Location = " Location: ";

        //
        // String constants for error messages
        //
        private const string STRERR_InvalidSourceLocation = "Invalid source location";
        private const string STRERR_InvalidAbsorberLocation = "Invalid absorber location";

        #endregion

        #region Properties

        private string[] sourceNames;
        private char[] sourceLocations;
        private string[] absorberNames;
        private char[] absorberLocations;

        public string[] SourceNames
        {
            get { return this.sourceNames; }
        }

        public char[] SourceLocations
        {
            get { return this.sourceLocations; }
        }

        public string[] AbsorberNames
        {
            get { return this.absorberNames; }
        }

        public char[] AbsorberLocations
        {
            get { return this.absorberLocations; }
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
                // Get a list of all sources, must have at least one
                //
                XmlNode xmlNodeSources = XmlUtilities.GetXmlNode(this.xmlNodeConfiguration, STRXML_sources);
                XmlNodeList xmlNodeList = XmlUtilities.GetXmlNodeList(xmlNodeSources, STRXML_source, false);
                if (xmlNodeList.Count > 0)
                {
                    this.sourceNames = new string[xmlNodeList.Count];
                    this.sourceLocations = new char[xmlNodeList.Count];
                    for (int i = 0; i < xmlNodeList.Count; i++)
                    {
                        XmlNode xmlNodeTemp = xmlNodeList.Item(i);

                        this.sourceNames[i] = XmlUtilities.GetXmlValue(xmlNodeTemp, STRXML_name, false);
                        string location = XmlUtilities.GetXmlValue(xmlNodeTemp, STRXML_location, false);
                        if (location.Length != 1 || location[0] < 'A' || location[0] > 'Z')
                        {
                            throw new ArgumentException(STRERR_InvalidSourceLocation, STRXML_location);
                        }
                        this.sourceLocations[i] = location[0];

                        Logfile.Write(STRLOG_Source + this.sourceNames[i]);
                        Logfile.Write(STRLOG_Location + this.sourceLocations[i]);
                    }
                }

                //
                // Get a list of all absorbers, must have at least one
                //
                XmlNode xmlNodeAbsorbers = XmlUtilities.GetXmlNode(this.xmlNodeConfiguration, STRXML_absorbers);
                xmlNodeList = XmlUtilities.GetXmlNodeList(xmlNodeAbsorbers, STRXML_absorber, false);
                if (xmlNodeList.Count > 0)
                {
                    this.absorberNames = new string[xmlNodeList.Count];
                    this.absorberLocations = new char[xmlNodeList.Count];
                    for (int i = 0; i < xmlNodeList.Count; i++)
                    {
                        XmlNode xmlNodeTemp = xmlNodeList.Item(i);

                        this.absorberNames[i] = XmlUtilities.GetXmlValue(xmlNodeTemp, STRXML_name, false);
                        string location = XmlUtilities.GetXmlValue(xmlNodeTemp, STRXML_location, false);
                        if (location.Length != 1 || location[0] < 'A' || location[0] > 'Z')
                        {
                            throw new ArgumentException(STRERR_InvalidAbsorberLocation, STRXML_location);
                        }
                        this.absorberLocations[i] = location[0];

                        Logfile.Write(STRLOG_Absorber + this.absorberNames[i]);
                        Logfile.Write(STRLOG_Location + this.absorberLocations[i]);
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
