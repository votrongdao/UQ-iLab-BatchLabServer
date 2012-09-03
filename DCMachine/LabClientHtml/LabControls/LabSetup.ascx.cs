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
            lblParamsTitle.Text = null;
            txbSpeedMin.Text = null;
            txbSpeedMax.Text = null;
            txbSpeedStep.Text = null;
            txbFieldMin.Text = null;
            txbFieldMax.Text = null;
            txbFieldStep.Text = null;
            txbLoadMin.Text = null;
            txbLoadMax.Text = null;
            txbLoadStep.Text = null;
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

            // Set the title for the parameters
            lblParamsTitle.Text = XmlUtilities.GetXmlValue(this.xmlNodeSelectedSetup, LabConsts.STRXML_ParamsTitle, true);

            //
            // Show/hide the page controls for the specified setup
            //
            if (setupId.Equals(LabConsts.STRXML_SetupId_VoltageVsSpeed) == true ||
                setupId.Equals(LabConsts.STRXML_SetupId_SpeedVsVoltage) == true)
            {
                //
                // Default values for machine speed
                //
                txbSpeedMin.Text = XmlUtilities.GetXmlValue(this.xmlNodeSelectedSetup, LabConsts.STRXML_speedMin, true);
                txbSpeedMax.Text = XmlUtilities.GetXmlValue(this.xmlNodeSelectedSetup, LabConsts.STRXML_speedMax, true);
                txbSpeedStep.Text = XmlUtilities.GetXmlValue(this.xmlNodeSelectedSetup, LabConsts.STRXML_speedStep, true);
                txbSpeedMin.Visible = true;
                txbSpeedMax.Visible = true;
                txbSpeedStep.Visible = true;
                txbFieldMin.Visible = false;
                txbFieldMax.Visible = false;
                txbFieldStep.Visible = false;
                txbLoadMin.Visible = false;
                txbLoadMax.Visible = false;
                txbLoadStep.Visible = false;

                //
                // Boundary values and tooltips for machine speed
                //
                XmlNode xmlNodeTemp = XmlUtilities.GetXmlNode(this.xmlNodeValidation, LabConsts.STRXML_vdnSpeed);
                int speedMin = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_minimum);
                int speedMax = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_maximum);
                int speedStepMin = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_stepMin);
                int speedStepMax = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_stepMax);
                txbSpeedMin.ToolTip = LabConsts.STR_Range + speedMin.ToString() + LabConsts.STR_to + speedMax.ToString();
                txbSpeedMax.ToolTip = txbSpeedMin.ToolTip;
                txbSpeedStep.ToolTip = LabConsts.STR_Range + speedStepMin.ToString() + LabConsts.STR_to + speedStepMax.ToString();
            }
            else if (setupId.Equals(LabConsts.STRXML_SetupId_VoltageVsField) == true ||
                setupId.Equals(LabConsts.STRXML_SetupId_SpeedVsField) == true)
            {
                //
                // Default values for field current
                //
                txbFieldMin.Text = XmlUtilities.GetXmlValue(this.xmlNodeSelectedSetup, LabConsts.STRXML_fieldMin, true);
                txbFieldMax.Text = XmlUtilities.GetXmlValue(this.xmlNodeSelectedSetup, LabConsts.STRXML_fieldMax, true);
                txbFieldStep.Text = XmlUtilities.GetXmlValue(this.xmlNodeSelectedSetup, LabConsts.STRXML_fieldStep, true);
                txbSpeedMin.Visible = false;
                txbSpeedMax.Visible = false;
                txbSpeedStep.Visible = false;
                txbFieldMin.Visible = true;
                txbFieldMax.Visible = true;
                txbFieldStep.Visible = true;
                txbLoadMin.Visible = false;
                txbLoadMax.Visible = false;
                txbLoadStep.Visible = false;

                //
                // Boundary values and tooltips for field current
                //
                XmlNode xmlNodeTemp = XmlUtilities.GetXmlNode(this.xmlNodeValidation, LabConsts.STRXML_vdnField);
                int fieldMin = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_minimum);
                int fieldMax = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_maximum);
                int fieldStepMin = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_stepMin);
                int fieldStepMax = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_stepMax);
                txbFieldMin.ToolTip = LabConsts.STR_Range + fieldMin.ToString() + LabConsts.STR_to + fieldMax.ToString();
                txbFieldMax.ToolTip = txbFieldMin.ToolTip;
                txbFieldStep.ToolTip = LabConsts.STR_Range + fieldStepMin.ToString() + LabConsts.STR_to + fieldStepMax.ToString();
            }
            else if (setupId.Equals(LabConsts.STRXML_SetupId_VoltageVsLoad) == true)
            {
                //
                // Default values for machine load
                //
                txbLoadMin.Text = XmlUtilities.GetXmlValue(this.xmlNodeSelectedSetup, LabConsts.STRXML_loadMin, true);
                txbLoadMax.Text = XmlUtilities.GetXmlValue(this.xmlNodeSelectedSetup, LabConsts.STRXML_loadMax, true);
                txbLoadStep.Text = XmlUtilities.GetXmlValue(this.xmlNodeSelectedSetup, LabConsts.STRXML_loadStep, true);
                txbSpeedMin.Visible = false;
                txbSpeedMax.Visible = false;
                txbSpeedStep.Visible = false;
                txbFieldMin.Visible = false;
                txbFieldMax.Visible = false;
                txbFieldStep.Visible = false;
                txbLoadMin.Visible = true;
                txbLoadMax.Visible = true;
                txbLoadStep.Visible = true;

                //
                // Boundary values and tooltips for machine load
                //
                XmlNode xmlNodeTemp = XmlUtilities.GetXmlNode(this.xmlNodeValidation, LabConsts.STRXML_vdnLoad);
                int loadMin = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_minimum);
                int loadMax = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_maximum);
                int loadStepMin = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_stepMin);
                int loadStepMax = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_stepMax);
                txbLoadMin.ToolTip = LabConsts.STR_Range + loadMin.ToString() + LabConsts.STR_to + loadMax.ToString();
                txbLoadMax.ToolTip = txbLoadMin.ToolTip;
                txbLoadStep.ToolTip = LabConsts.STR_Range + loadStepMin.ToString() + LabConsts.STR_to + loadStepMax.ToString();
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
            if (setupId.Equals(LabConsts.STRXML_SetupId_VoltageVsSpeed) == true ||
                setupId.Equals(LabConsts.STRXML_SetupId_SpeedVsVoltage) == true)
            {
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_speedMin, txbSpeedMin.Text, true);
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_speedMax, txbSpeedMax.Text, true);
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_speedStep, txbSpeedStep.Text, true);
            }
            else if (setupId.Equals(LabConsts.STRXML_SetupId_VoltageVsField) == true ||
                setupId.Equals(LabConsts.STRXML_SetupId_SpeedVsField) == true)
            {
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_fieldMin, txbFieldMin.Text, true);
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_fieldMax, txbFieldMax.Text, true);
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_fieldStep, txbFieldStep.Text, true);
            }
            else if (setupId.Equals(LabConsts.STRXML_SetupId_VoltageVsLoad) == true)
            {
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_loadMin, txbLoadMin.Text, true);
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_loadMax, txbLoadMax.Text, true);
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_loadStep, txbLoadStep.Text, true);
            }

            return xmlNodeSpecification;
        }

    }
}