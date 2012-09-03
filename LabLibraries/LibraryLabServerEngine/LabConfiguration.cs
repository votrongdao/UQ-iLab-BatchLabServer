using System;
using System.IO;
using System.Xml;
using Library.Lab;

namespace Library.LabServerEngine
{
    public class LabConfiguration
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "LabConfiguration";

        //
        // String constants for the XML lab configuration
        //

        //
        // String constants for logfile messages
        //
        private const string STRLOG_Filename = " Filename: ";
        private const string STRLOG_ParsingLabConfiguration = " Parsing LabConfiguration...";
        private const string STRLOG_Title = " Title: ";
        private const string STRLOG_Version = " Version: ";
        private const string STRLOG_PhotoUrl = " Photo Url: ";
        private const string STRLOG_SetupId = " SetupId: ";
        private const string STRLOG_SetupName = " SetupName: ";

        //
        // Local variables accessible by a derived class
        //
        protected XmlNode xmlNodeConfiguration;

        #endregion

        #region Properties

        private string rootFilePath;
        private string filename;
        private string title;
        private string version;
        private string photoUrl;
        private string xmlConfiguration;
        private string[] setupIds;
        private string[] setupNames;
        private string xmlSpecification;
        private string xmlExperimentResult;
        private string xmlValidation;

        public string RootFilePath
        {
            get { return this.rootFilePath; }
        }

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

        public string PhotoUrl
        {
            get { return this.photoUrl; }
        }

        public string XmlConfiguration
        {
            get { return this.xmlConfiguration; }
        }

        public string[] SetupIds
        {
            get { return this.setupIds; }
        }

        public string[] SetupNames
        {
            get { return this.setupNames; }
        }

        public string XmlSpecification
        {
            get { return this.xmlSpecification; }
        }

        public string XmlExperimentResult
        {
            get { return this.xmlExperimentResult; }
        }

        public string XmlValidation
        {
            get { return this.xmlValidation; }
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        public LabConfiguration(string rootFilePath)
            : this(rootFilePath, null, null)
        {
        }

        //---------------------------------------------------------------------------------------//

        public LabConfiguration(string rootFilePath, string xmlLabConfiguration)
            : this(rootFilePath, xmlLabConfiguration, null)
        {
        }

        //---------------------------------------------------------------------------------------//

        public LabConfiguration(string rootFilePath, string xmlLabConfiguration, string labConfigFilename)
        {
            const string STRLOG_MethodName = "LabConfiguration";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            this.rootFilePath = rootFilePath;

            try
            {
                XmlDocument xmlDocument;

                //
                // Check if an XML lab configuration string is specified
                //
                if (xmlLabConfiguration != null)
                {
                    //
                    // Load the lab configuration from an XML string
                    //
                    xmlDocument = XmlUtilities.GetXmlDocument(xmlLabConfiguration);
                }
                else
                {
                    //
                    // Check if an XML lab configuration filename is specified
                    //
                    if (labConfigFilename == null)
                    {
                        //
                        // Get lab configuration filename from Application's configuration file
                        //
                        this.filename = Utilities.GetAppSetting(Consts.STRCFG_XmlLabConfigurationFilename);
                        this.filename = Path.Combine(this.rootFilePath, this.filename);
                    }
                    else
                    {
                        // Prepend full file path
                        this.filename = Path.Combine(this.rootFilePath, labConfigFilename);
                    }

                    Logfile.Write(STRLOG_Filename + this.filename);

                    // Load the lab configuration from the specified file
                    xmlDocument = XmlUtilities.GetXmlDocumentFromFile(this.filename);
                }

                // Get the lab configuration XML node
                XmlNode xmlNodeLabConfiguration = XmlUtilities.GetXmlRootNode(xmlDocument, Consts.STRXML_labConfiguration);

                Logfile.Write(STRLOG_ParsingLabConfiguration);

                //
                // Get information from the lab configuration node
                //
                this.title = XmlUtilities.GetXmlValue(xmlNodeLabConfiguration, Consts.STRXMLPARAM_title, false);
                this.version = XmlUtilities.GetXmlValue(xmlNodeLabConfiguration, Consts.STRXMLPARAM_version, false);
                this.photoUrl = XmlUtilities.GetXmlValue(xmlNodeLabConfiguration, Consts.STRXML_navmenuPhoto_image, true);

                Logfile.Write(STRLOG_Title + this.title);
                Logfile.Write(STRLOG_Version + this.version);
                Logfile.Write(STRLOG_PhotoUrl + this.photoUrl);

                //
                // Get the configuration XML node and save a copy
                //
                XmlNode xmlNode = XmlUtilities.GetXmlNode(xmlNodeLabConfiguration, Consts.STRXML_configuration);
                this.xmlNodeConfiguration = xmlNode.Clone();

                //
                // Get a list of all setups, must have at least one
                //
                XmlNodeList xmlNodeList = XmlUtilities.GetXmlNodeList(this.xmlNodeConfiguration, Consts.STRXML_setup, false);
                this.setupIds = new string[xmlNodeList.Count];
                this.setupNames = new string[xmlNodeList.Count];
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    XmlNode xmlNodeSetup = xmlNodeList.Item(i);

                    //
                    // Check that the required setup information exists
                    //
                    this.setupIds[i] = XmlUtilities.GetXmlValue(xmlNodeSetup, Consts.STRXMLPARAM_id, false);
                    this.setupNames[i] = XmlUtilities.GetXmlValue(xmlNodeSetup, Consts.STRXML_name, false);

                    Logfile.Write(STRLOG_SetupId + this.setupIds[i]);
                    Logfile.Write(STRLOG_SetupName + this.setupNames[i]);
                }

                //
                // These are mandatory
                //
                xmlNode = XmlUtilities.GetXmlNode(xmlNodeLabConfiguration, Consts.STRXML_configuration, false);
                this.xmlConfiguration = xmlNode.OuterXml;
                xmlNode = XmlUtilities.GetXmlNode(xmlNodeLabConfiguration, Consts.STRXML_experimentSpecification, false);
                this.xmlSpecification = xmlNode.OuterXml;
                xmlNode = XmlUtilities.GetXmlNode(xmlNodeLabConfiguration, Consts.STRXML_experimentResult, false);
                this.xmlExperimentResult = xmlNode.OuterXml;

                //
                // This is optional and depends on the LabServer implementation
                //
                xmlNode = XmlUtilities.GetXmlNode(xmlNodeLabConfiguration, Consts.STRXML_validation, true);
                if (xmlNode != null)
                {
                    this.xmlValidation = xmlNode.OuterXml;
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
