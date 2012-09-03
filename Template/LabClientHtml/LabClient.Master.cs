using System;
using System.Net;
using System.Web;
using System.Xml;
using Library.Lab;
using Library.LabClient;

namespace LabClientHtml
{
    public partial class LabClientMaster : System.Web.UI.MasterPage
    {
        #region Class Constants

        private const string STRLOG_ClassName = "LabClientMaster";

        //
        // String constants
        //
        public const string STR_Initialising = " Initialising...";
        public const string STR_Version = "Version ";
        public const string STR_MailTo = "mailto:";

        //
        // String constants for logfile messages
        //
        public const string STRLOG_SessionExistsRemovingSession = " Session already exists! Removing session...";
        public const string STRLOG_SessionNotExist = " Session does not exist! Web page has expired.";
        public const string STRLOG_MultiSubmit = " MultiSubmit: ";
        public const string STRLOG_GettingLabStatus = " Getting Lab Status...";
        public const string STRLOG_Online = " Online: ";
        public const string STRLOG_LabStatusMessage = " LabStatus Message: ";
        public const string STRLOG_GettingLabConfiguration = " Getting Lab Configuration XML string...";
        public const string STRLOG_LoadingLabConfiguration = " Loading Lab Configuration XML string...";
        public const string STRLOG_ParsingLabConfiguration = " Parsing Lab Configuration...";
        public const string STRLOG_Title = " Title: ";
        public const string STRLOG_Version = " Version: ";
        public const string STRLOG_PhotoUrl = " Photo Url: ";
        public const string STRLOG_LabInfoText = " LabInfo Text: ";
        public const string STRLOG_LabInfoUrl = " LabInfo Url: ";
        public const string STRLOG_LabCameraUrl = " LabCamera Url: ";
        private const string STRLOG_UserHostAddress = " UserHostAddress: ";
        private const string STRLOG_UserHostName = " UserHostName: ";
        private const string STRLOG_CannotResolveToHostName = " Cannot resolve to HostName!";

        #endregion

        #region Properties

        public string Title
        {
            get
            {
                if (Session[Consts.STRSSN_LabClient] != null)
                {
                    LabClientSession labClientSession = (LabClientSession)Session[Consts.STRSSN_LabClient];
                    return labClientSession.bannerTitle;
                }
                else
                {
                    return null;
                }
            }
        }

        public string Version
        {
            get
            {
                if (Session[Consts.STRSSN_LabClient] != null)
                {
                    LabClientSession labClientSession = (LabClientSession)Session[Consts.STRSSN_LabClient];
                    return labClientSession.statusVersion;
                }
                else
                {
                    return null;
                }
            }
        }

        public string LabinfoText
        {
            get
            {
                if (Session[Consts.STRSSN_LabClient] != null)
                {
                    LabClientSession labClientSession = (LabClientSession)Session[Consts.STRSSN_LabClient];
                    return labClientSession.labInfoText;
                }
                else
                {
                    return null;
                }
            }
        }

        public string LabInfoUrl
        {
            get
            {
                if (Session[Consts.STRSSN_LabClient] != null)
                {
                    LabClientSession labClientSession = (LabClientSession)Session[Consts.STRSSN_LabClient];
                    return labClientSession.labInfoUrl;
                }
                else
                {
                    return null;
                }
            }
        }

        public string LabCameraUrl
        {
            get
            {
                if (Session[Consts.STRSSN_LabClient] != null)
                {
                    LabClientSession labClientSession = (LabClientSession)Session[Consts.STRSSN_LabClient];
                    return labClientSession.labCameraUrl;
                }
                else
                {
                    return null;
                }
            }
        }

        public string PageTitle
        {
            get
            {
                if (Session[Consts.STRSSN_LabClient] != null)
                {
                    LabClientSession labClientSession = (LabClientSession)Session[Consts.STRSSN_LabClient];
                    return labClientSession.bannerTitle + " - ";
                }
                else
                {
                    return null;
                }
            }
        }

        public string HeaderTitle
        {
            get { return Header.Title; }
            set { Header.Title = value; }
        }

        public bool MultiSubmit
        {
            get
            {
                if (Session[Consts.STRSSN_LabClient] != null)
                {
                    LabClientSession labClientSession = (LabClientSession)Session[Consts.STRSSN_LabClient];
                    return labClientSession.multiSubmit;
                }
                else
                {
                    return false;
                }
            }
        }

        public XmlNode XmlNodeLabConfiguration
        {
            get
            {
                if (Session[Consts.STRSSN_LabClient] != null)
                {
                    LabClientSession labClientSession = (LabClientSession)Session[Consts.STRSSN_LabClient];
                    return labClientSession.xmlNodeLabConfiguration;
                }
                else
                {
                    return null;
                }
            }
        }

        public XmlNode XmlNodeConfiguration
        {
            get
            {
                if (Session[Consts.STRSSN_LabClient] != null)
                {
                    LabClientSession labClientSession = (LabClientSession)Session[Consts.STRSSN_LabClient];
                    return labClientSession.xmlNodeConfiguration;
                }
                else
                {
                    return null;
                }
            }
        }

        public XmlNode XmlNodeValidation
        {
            get
            {
                if (Session[Consts.STRSSN_LabClient] != null)
                {
                    LabClientSession labClientSession = (LabClientSession)Session[Consts.STRSSN_LabClient];
                    return labClientSession.xmlNodeValidation;
                }
                else
                {
                    return null;
                }
            }
        }

        public XmlNode XmlNodeSpecification
        {
            get
            {
                if (Session[Consts.STRSSN_LabClient] != null)
                {
                    LabClientSession labClientSession = (LabClientSession)Session[Consts.STRSSN_LabClient];
                    return labClientSession.xmlNodeSpecification;
                }
                else
                {
                    return null;
                }
            }
        }

        public LabClientToSbAPI ServiceBroker
        {
            get
            {
                if (Session[Consts.STRSSN_LabClient] != null)
                {
                    LabClientSession labClientSession = (LabClientSession)Session[Consts.STRSSN_LabClient];
                    return labClientSession.labClientToSbAPI;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        protected void Page_Init(object sender, EventArgs e)
        {
            const string STRLOG_MethodName = "Page_Init";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // This method gets called before any other Page_Init() or Page_Load() methods.
            //

            //
            // Get query string values for coupon ID and passkey
            //
            string query = Request.Url.Query;
            string queryCouponId = HttpUtility.ParseQueryString(query).Get(Consts.STRQRY_CouponID);
            string queryPasskey = HttpUtility.ParseQueryString(query).Get(Consts.STRQRY_Passkey);

            //
            // Check if this is new LabClient session
            //
            if (Session[Consts.STRSSN_LabClient] != null && queryCouponId != null)
            {
                //
                // Remove the existing session because this is a new LabClient launch
                //
                Logfile.Write(STRLOG_SessionExistsRemovingSession);
                Session.Remove(Consts.STRSSN_LabClient);
            }

            if (Session[Consts.STRSSN_LabClient] == null)
            {
                if (queryCouponId == null)
                {
                    //
                    // Session has timed out
                    //
                    Logfile.Write(STRLOG_SessionNotExist);
                    Response.Redirect(Consts.STRURL_Expired);
                }

                //
                // Carry out one-time initialisation for all LabClient instances
                //
                Logfile.Write(STR_Initialising);

                //
                // Log the caller's IP address and hostname
                //
                HttpRequest httpRequest = this.Request;
                string logMessage = STRLOG_UserHostAddress + httpRequest.UserHostAddress +
                    Logfile.STRLOG_Spacer + STRLOG_UserHostName;
                try
                {
                    IPHostEntry ipHostEntry = Dns.GetHostEntry(httpRequest.UserHostAddress);
                    logMessage += ipHostEntry.HostName;
                }
                catch
                {
                    logMessage += STRLOG_CannotResolveToHostName;
                }
                Logfile.Write(logMessage);

                //
                // Get query string values - the query string parameters are NOT case-sensensitive (good)
                //
                string queryServiceUrl = HttpUtility.ParseQueryString(query).Get(Consts.STRQRY_ServiceURL);
                string queryLabServerId = HttpUtility.ParseQueryString(query).Get(Consts.STRQRY_LabServerID);
                string queryMultiSubmit = HttpUtility.ParseQueryString(query).Get(Consts.STRQRY_MultiSubmit);

                //
                // Create a LabClient session information instance
                //
                LabClientSession labClientSession = new LabClientSession();

                //
                // Create ServiceBroker interface with authorisation information
                //
                LabClientToSbAPI serviceBroker = new LabClientToSbAPI(queryCouponId, queryPasskey, queryServiceUrl, queryLabServerId);
                labClientSession.labClientToSbAPI = serviceBroker;

                //
                // Get the lab status and lab configuration
                //
                try
                {
                    //
                    // Get the lab status
                    //
                    Logfile.Write(STRLOG_GettingLabStatus);

                    LabStatus labStatus = serviceBroker.GetLabStatus();

                    logMessage = STRLOG_Online + labStatus.online.ToString() +
                        Logfile.STRLOG_Spacer + STRLOG_LabStatusMessage + labStatus.labStatusMessage;
                    Logfile.Write(logMessage);

                    //
                    // Get the XML lab configuration string
                    //
                    Logfile.Write(STRLOG_GettingLabConfiguration);

                    string xmlLabConfiguration = serviceBroker.GetLabConfiguration();
                    if (xmlLabConfiguration != null)
                    {
                        // Save information from the lab configuration string
                        ParseLabConfiguration(labClientSession, xmlLabConfiguration);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        Logfile.Write(ex.InnerException.Message);
                    }
                    else
                    {
                        Logfile.WriteError(ex.Message);
                    }
                }

                //
                // Get feedback email URL
                //
                labClientSession.mailtoUrl = STR_MailTo + Utilities.GetAppSetting(Consts.STRCFG_FeedbackEmail);

                //
                // Determine if multiple submission is enabled
                //
                try
                {
                    if (queryMultiSubmit == null)
                    {
                        //
                        // Querystring parameter is not specified, try getting it from the application's configuration file
                        //
                        labClientSession.multiSubmit = Convert.ToBoolean(Utilities.GetAppSetting(Consts.STRCFG_MultiSubmit));
                    }
                    else
                    {
                        labClientSession.multiSubmit = Convert.ToBoolean(queryMultiSubmit);
                    }
                }
                catch
                {
                    labClientSession.multiSubmit = false;
                }
                Logfile.Write(STRLOG_MultiSubmit + labClientSession.multiSubmit.ToString());

                Session[Consts.STRSSN_LabClient] = labClientSession;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session[Consts.STRSSN_LabClient] != null)
            {
                LabClientSession labClientSession = (LabClientSession)Session[Consts.STRSSN_LabClient];

                Banner.Title = labClientSession.bannerTitle;
                Statusbar.Version = STR_Version + labClientSession.statusVersion;
                Navmenu.PhotoUrl = labClientSession.navmenuPhotoUrl;
                Navmenu.CameraUrl = labClientSession.labCameraUrl;
                Feedback.MailtoUrl = labClientSession.mailtoUrl;
            }
        }

        //---------------------------------------------------------------------------------------//

        private void ParseLabConfiguration(LabClientSession labClientSession, string xmlLabConfiguration)
        {
            const string STRLOG_MethodName = "ParseLabConfiguration";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            try
            {
                Logfile.Write(STRLOG_LoadingLabConfiguration);

                //
                // Load the lab configuration from an XML string
                //
                XmlDocument xmlDocument = XmlUtilities.GetXmlDocument(xmlLabConfiguration);

                //
                // Save a copy of the lab configuration XML node
                //
                XmlNode xmlNode = XmlUtilities.GetXmlRootNode(xmlDocument, Consts.STRXML_labConfiguration);
                XmlNode xmlNodeLabConfiguration = xmlNode.Clone();
                labClientSession.xmlNodeLabConfiguration = xmlNodeLabConfiguration;

                Logfile.Write(STRLOG_ParsingLabConfiguration);

                //
                // Get information from the lab configuration node
                //
                labClientSession.bannerTitle = XmlUtilities.GetXmlValue(xmlNodeLabConfiguration, Consts.STRXMLPARAM_title, false);
                labClientSession.statusVersion = XmlUtilities.GetXmlValue(xmlNodeLabConfiguration, Consts.STRXMLPARAM_version, false);
                labClientSession.navmenuPhotoUrl = XmlUtilities.GetXmlValue(xmlNodeLabConfiguration, Consts.STRXML_navmenuPhoto_image, false);
                labClientSession.labCameraUrl = XmlUtilities.GetXmlValue(xmlNodeLabConfiguration, Consts.STRXML_labCamera_url, true);
                labClientSession.labInfoText = XmlUtilities.GetXmlValue(xmlNodeLabConfiguration, Consts.STRXML_labInfo_text, true);
                labClientSession.labInfoUrl = XmlUtilities.GetXmlValue(xmlNodeLabConfiguration, Consts.STRXML_labInfo_url, true);

                Logfile.Write(STRLOG_Title + labClientSession.bannerTitle);
                Logfile.Write(STRLOG_Version + labClientSession.statusVersion);
                Logfile.Write(STRLOG_PhotoUrl + labClientSession.navmenuPhotoUrl);
                Logfile.Write(STRLOG_LabCameraUrl + labClientSession.labCameraUrl);
                Logfile.Write(STRLOG_LabInfoText + labClientSession.labInfoText);
                Logfile.Write(STRLOG_LabInfoUrl + labClientSession.labInfoUrl);

                //
                // These are mandatory
                //
                xmlNode = XmlUtilities.GetXmlNode(xmlNodeLabConfiguration, Consts.STRXML_configuration, false);
                labClientSession.xmlNodeConfiguration = xmlNode.Clone();
                xmlNode = XmlUtilities.GetXmlNode(xmlNodeLabConfiguration, Consts.STRXML_experimentSpecification, false);
                labClientSession.xmlNodeSpecification = xmlNode.Clone();

                //
                // These are optional and depend on the LabServer implementation
                //
                xmlNode = XmlUtilities.GetXmlNode(xmlNodeLabConfiguration, Consts.STRXML_validation, true);
                if (xmlNode != null)
                {
                    labClientSession.xmlNodeValidation = xmlNode.Clone();
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

    }
}