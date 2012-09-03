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
            //
            // Nothing to do here
            //
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
            // Nothing to do here
            //

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
            //
            // Nothing to do here
            //
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
            //
            // Nothing to do here
            //

            return xmlNodeSpecification;
        }

    }
}