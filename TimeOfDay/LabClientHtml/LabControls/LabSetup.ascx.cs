using System;
using System.Drawing;
using System.IO;
using System.Xml;
using Library.Lab;
using Library.LabClient;

namespace LabClientHtml.LabControls
{
    public partial class LabSetup : System.Web.UI.UserControl
    {
        #region Class Constants and Variables

        //
        // String constants
        //

        #endregion

        #region Properties

        private XmlNode xmlNodeConfiguration;
        private XmlNode xmlNodeValidation;
        private XmlNode xmlNodeSelectedSetup;

        public XmlNode XmlNodeConfiguration
        {
            get { return this.xmlNodeConfiguration; }
            set { this.xmlNodeConfiguration = value; }
        }

        public XmlNode XmlNodeValidation
        {
            get { return this.xmlNodeValidation; }
            set { this.xmlNodeValidation = value; }
        }

        public XmlNode XmlNodeSelectedSetup
        {
            get { return this.xmlNodeSelectedSetup; }
            set { this.xmlNodeSelectedSetup = value; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulatePageControls();
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private void ClearPageControls()
        {
            // Nothing to clear
        }

        //-------------------------------------------------------------------------------------------------//

        private void PopulatePageControls()
        {
            //
            // Cannot do anything without a configuration
            //
            if (this.xmlNodeConfiguration == null)
            {
                return;
            }

            //
            // Populate dropdown lists, etc. and select default selection, etc.
            //

            //
            // Get all network time server URLs and add to the dropdown list
            //
            XmlNode xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeConfiguration, LabConsts.STRXML_timeServers);
            string[] timeServerUrls = XmlUtilities.GetXmlValues(xmlNode, LabConsts.STRXML_url, false);
            for (int i = 0; i < timeServerUrls.Length; i++)
            {
                ddlTimeServerUrl.Items.Add(timeServerUrls[i]);
            }

            //
            // Get a list of all time formats and add to the dropdownlist
            //
            xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeConfiguration, LabConsts.STRXML_timeFormats);
            string[] timeFormats = XmlUtilities.GetXmlValues(xmlNode, LabConsts.STRXML_timeFormat, false);
            for (int i = 0; i < timeFormats.Length; i++)
            {
                ddlTimeFormat.Items.Add(timeFormats[i]);
            }

            //
            // Set default selection for time format
            //
            string defaultFormat = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_default, true);
            if (defaultFormat.Length > 0)
            {
                ddlTimeFormat.SelectedValue = defaultFormat;
            }

            //
            // Update controls for selected setup
            //
            UpdatePageControls();
        }

        //-------------------------------------------------------------------------------------------------//

        private void UpdatePageControls()
        {
            //
            // Cannot do anything without a configuration
            //
            if (this.xmlNodeConfiguration == null)
            {
                return;
            }

            //
            // Get the ID of the selected setup
            //
            string setupId = XmlUtilities.GetXmlValue(this.xmlNodeSelectedSetup, Consts.STRXMLPARAM_id, false);

            //
            // Show/hide the page controls for the specified setup
            //
            if (setupId.Equals(LabConsts.STRXML_SetupId_LocalClock) == true)
            {
                lblTimeServerUrl.Visible = false;
                ddlTimeServerUrl.Visible = false;
            }
            else if (setupId.Equals(LabConsts.STRXML_SetupId_NTPServer) == true)
            {
                lblTimeServerUrl.Visible = true;
                ddlTimeServerUrl.Visible = true;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void ddlExperimentSetups_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear page controls before selecting another setup
            ClearPageControls();

            // Update page controls for the selected index
            UpdatePageControls();
        }

        //---------------------------------------------------------------------------------------//

        public XmlNode BuildSpecification(XmlNode xmlNodeSpecification, string setupId)
        {
            //
            // Fill in specification information only for selected setup
            //
            XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_formatName, ddlTimeFormat.SelectedValue, false);
            if (setupId.Equals(LabConsts.STRXML_SetupId_NTPServer) == true)
            {
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_serverUrl, ddlTimeServerUrl.SelectedValue, false);
            }

            return xmlNodeSpecification;
        }

    }
}