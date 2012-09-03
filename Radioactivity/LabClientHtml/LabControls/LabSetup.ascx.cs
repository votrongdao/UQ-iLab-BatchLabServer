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
        private const string STR_Range = "Range: ";
        private const string STR_to = " to ";
        private const string STR_TotalTime = " - Total Time: ";
        private const string STR_seconds = " seconds";

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
            PopulateSources();
            PopulateAbsorbers();
            PopulateDistances();

            //
            // Update controls for selected setup
            //
            UpdatePageControls();
        }

        //-------------------------------------------------------------------------------------------------//

        private void PopulateSources()
        {
            //
            // Get a list of all sources and add to the dropdownlist
            //
            ddlSources.Items.Clear();
            XmlNode xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeConfiguration, LabConsts.STRXML_sources, false);
            XmlNodeList xmlNodeList = XmlUtilities.GetXmlNodeList(xmlNode, LabConsts.STRXML_source, false);
            for (int i = 0; i < xmlNodeList.Count; i++)
            {
                XmlNode xmlNodeTemp = xmlNodeList.Item(i);

                string sourceName = XmlUtilities.GetXmlValue(xmlNodeTemp, LabConsts.STRXML_name, false);
                ddlSources.Items.Add(sourceName);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private void PopulateAbsorbers()
        {
            //
            // Get a list of all absorbers and add to the dropdownlist
            //
            ddlAbsorbers.Items.Clear();
            XmlNode xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeConfiguration, LabConsts.STRXML_absorbers, false);
            XmlNodeList xmlNodeList = XmlUtilities.GetXmlNodeList(xmlNode, LabConsts.STRXML_absorber, false);
            for (int i = 0; i < xmlNodeList.Count; i++)
            {
                XmlNode xmlNodeTemp = xmlNodeList.Item(i);

                string absorberName = XmlUtilities.GetXmlValue(xmlNodeTemp, LabConsts.STRXML_name, false);
                ddlAbsorbers.Items.Add(absorberName);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private void PopulateDistances()
        {
            //
            // Get a list of distances and add to the dropdownlist
            //
            ddlDistances.Items.Clear();
            XmlNode xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeConfiguration, LabConsts.STRXML_distances, true);
            if (xmlNode != null)
            {
                // Get minimum distance
                int minimum = XmlUtilities.GetIntValue(xmlNode, LabConsts.STRXML_minimum);

                // Get maximum distance
                int maximum = XmlUtilities.GetIntValue(xmlNode, LabConsts.STRXML_maximum);

                // Get distance stepsize
                int stepsize = XmlUtilities.GetIntValue(xmlNode, LabConsts.STRXML_stepsize);

                //
                // Add numbers to the Distance dropdownlist if range is valid
                //
                if (minimum >= 0 && maximum > 0 && stepsize > 0)
                {
                    for (int i = minimum; i <= maximum; i += stepsize)
                    {
                        ddlDistances.Items.Add(i.ToString());
                    }
                }
            }
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
            if (setupId.Equals(LabConsts.STRXML_SetupId_RadioactivityVsTime) ||
                setupId.Equals(LabConsts.STRXML_SetupId_SimActivityVsTime) ||
                setupId.Equals(LabConsts.STRXML_SetupId_SimActivityVsTimeNoDelay))
            {
                //
                // Set default selection for source
                //
                XmlNode xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeConfiguration, LabConsts.STRXML_sources, true);
                string defaultSource = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_default, true);
                if (defaultSource.Length > 0)
                {
                    ddlSources.SelectedValue = defaultSource;
                }

                //
                // Set default selection for absorber
                //
                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeConfiguration, LabConsts.STRXML_absorbers, true);
                string defaultAbsorber = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_default, true);
                if (defaultAbsorber.Length > 0)
                {
                    ddlAbsorbers.SelectedValue = defaultAbsorber;
                }

                //
                // Hide DistanceList controls
                //
                //lblSelectedDistances.Visible = false;
                btnSelectedDistancesAdd.Visible = false;
                //btnSelectedDistancesClear.Visible = false;
                //ddlSelectedDistances.Visible = false;
                trDistanceList.Visible = false;

                //
                // Hide AbsorberList controls
                //
                //lblSelectedAbsorbers.Visible = false;
                btnSelectedAbsorbersAdd.Visible = false;
                //btnSelectedAbsorbersClear.Visible = false;
                //ddlSelectedAbsorbers.Visible = false;
                trAbsorberList.Visible = false;

                //
                // Set default distance
                //
                ddlDistances.SelectedValue = XmlUtilities.GetXmlValue(this.xmlNodeSelectedSetup, LabConsts.STRXML_distance, true);
            }
            else if (setupId.Equals(LabConsts.STRXML_SetupId_RadioactivityVsDistance) ||
                setupId.Equals(LabConsts.STRXML_SetupId_SimActivityVsDistance) ||
                setupId.Equals(LabConsts.STRXML_SetupId_SimActivityVsDistanceNoDelay))
            {
                //
                // Set default selection for source
                //
                XmlNode xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeConfiguration, LabConsts.STRXML_sources, true);
                string defaultSource = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_default, true);
                try
                {
                    ddlSources.SelectedValue = defaultSource;
                }
                catch
                {
                }

                //
                // Set default selection for absorber
                //
                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeConfiguration, LabConsts.STRXML_absorbers, true);
                string defaultAbsorber = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_default, true);
                try
                {
                    ddlAbsorbers.SelectedValue = defaultAbsorber;
                }
                catch
                {
                }

                //
                // Show DistanceList controls
                //
                //lblSelectedDistances.Visible = true;
                btnSelectedDistancesAdd.Visible = true;
                btnSelectedDistancesAdd.Enabled = true;
                //btnSelectedDistancesClear.Visible = true;
                //ddlSelectedDistances.Visible = true;
                trDistanceList.Visible = true;

                //
                // Hide AbsorberList controls
                //
                //lblSelectedAbsorbers.Visible = false;
                btnSelectedAbsorbersAdd.Visible = false;
                //btnSelectedAbsorbersClear.Visible = false;
                //ddlSelectedAbsorbers.Visible = false;
                trAbsorberList.Visible = false;

                //
                // Set default selected distance list
                //
                string csvDistances = XmlUtilities.GetXmlValue(this.xmlNodeSelectedSetup, LabConsts.STRXML_distance, true);
                string[] csvDistancesSplit = csvDistances.Split(new char[] { LabConsts.CHR_CsvSplitter });
                for (int i = 0; i < csvDistancesSplit.Length; i++)
                {
                    try
                    {
                        ddlDistances.SelectedValue = csvDistancesSplit[i].Trim();
                        btnSelectedDistancesAdd_Click(this, new EventArgs());
                    }
                    catch
                    {
                    }
                }
            }
            else if (setupId.Equals(LabConsts.STRXML_SetupId_RadioactivityVsAbsorber))
            {
                //
                // Set default selection for source
                //
                XmlNode xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeConfiguration, LabConsts.STRXML_sources, true);
                string defaultSource = XmlUtilities.GetXmlValue(xmlNode, LabConsts.STRXMLPARAM_default, true);
                try
                {
                    ddlSources.SelectedValue = defaultSource;
                }
                catch
                {
                }

                //
                // Set default selected absorber list
                //
                string csvAbsorbers = XmlUtilities.GetXmlValue(this.xmlNodeSelectedSetup, LabConsts.STRXML_absorberName, true);
                string[] csvAbsorbersSplit = csvAbsorbers.Split(new char[] { LabConsts.CHR_CsvSplitter });
                for (int i = 0; i < csvAbsorbersSplit.Length; i++)
                {
                    try
                    {
                        ddlAbsorbers.SelectedValue = csvAbsorbersSplit[i].Trim();
                        btnSelectedAbsorbersAdd_Click(this, new EventArgs());
                    }
                    catch
                    {
                    }
                }

                //
                // Hide DistanceList controls
                //
                //lblSelectedDistances.Visible = false;
                btnSelectedDistancesAdd.Visible = false;
                //btnSelectedDistancesClear.Visible = false;
                //ddlSelectedDistances.Visible = false;
                trDistanceList.Visible = false;

                //
                // Show AbsorberList controls
                //
                //lblSelectedAbsorbers.Visible = true;
                btnSelectedAbsorbersAdd.Visible = true;
                btnSelectedAbsorbersAdd.Enabled = true;
                //btnSelectedAbsorbersClear.Visible = true;
                //ddlSelectedAbsorbers.Visible = true;
                trAbsorberList.Visible = true;
            }

            //
            // Boundary values for total time
            //
            XmlNode xmlNodeTemp = XmlUtilities.GetXmlNode(this.xmlNodeValidation, LabConsts.STRXML_vdnTotaltime);
            int totalTimeMin = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_minimum);
            int totalTimeMax = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_maximum);

            //
            // Set default duration
            //
            int duration = XmlUtilities.GetIntValue(this.xmlNodeSelectedSetup, LabConsts.STRXML_duration);
            txbDuration.Text = duration.ToString();

            //
            // Boundary values and tooltips for duration
            //
            xmlNodeTemp = XmlUtilities.GetXmlNode(this.xmlNodeValidation, LabConsts.STRXML_vdnDuration);
            int durationMin = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_minimum);
            int durationMax = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_maximum);
            txbDuration.ToolTip = STR_Range + durationMin.ToString() + STR_to + durationMax.ToString()
                + STR_TotalTime + totalTimeMin.ToString() + STR_to + totalTimeMax.ToString() + STR_seconds;

            //
            // Set default repeat count
            //
            int repeat = XmlUtilities.GetIntValue(this.xmlNodeSelectedSetup, LabConsts.STRXML_repeat);
            txbRepeat.Text = repeat.ToString();

            //
            // Boundary values and tooltips for repeat count
            //
            xmlNodeTemp = XmlUtilities.GetXmlNode(this.xmlNodeValidation, LabConsts.STRXML_vdnRepeat);
            int repeatMin = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_minimum);
            int repeatMax = XmlUtilities.GetIntValue(xmlNodeTemp, LabConsts.STRXML_maximum);
            txbRepeat.ToolTip = STR_Range + repeatMin.ToString() + STR_to + repeatMax.ToString()
                + STR_TotalTime + totalTimeMin.ToString() + STR_to + totalTimeMax.ToString() + STR_seconds;
        }

        //-------------------------------------------------------------------------------------------------//

        public void ddlExperimentSetups_SelectedIndexChanged(object sender, EventArgs e)
        {
            //
            // Clear page controls before selecting another setup
            //
            ddlSelectedAbsorbers.Items.Clear();
            ddlSelectedDistances.Items.Clear();
            txbDuration.Text = string.Empty;
            txbRepeat.Text = string.Empty;

            //
            // Update page controls for the selected index
            //
            PopulatePageControls();
        }

        //---------------------------------------------------------------------------------------//

        public XmlNode BuildSpecification(XmlNode xmlNodeSpecification, string setupId)
        {
            //
            // Fill in specification information only for selected setup
            //
            if (setupId.Equals(LabConsts.STRXML_SetupId_RadioactivityVsTime) ||
                setupId.Equals(LabConsts.STRXML_SetupId_SimActivityVsTime) ||
                setupId.Equals(LabConsts.STRXML_SetupId_SimActivityVsTimeNoDelay))
            {
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_sourceName, ddlSources.SelectedValue, false);
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_absorberName, ddlAbsorbers.SelectedValue, false);
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_distance, ddlDistances.SelectedValue, false);
            }
            else if (setupId.Equals(LabConsts.STRXML_SetupId_RadioactivityVsDistance) ||
                setupId.Equals(LabConsts.STRXML_SetupId_SimActivityVsDistance) ||
                setupId.Equals(LabConsts.STRXML_SetupId_SimActivityVsDistanceNoDelay))
            {
                //
                // Create CSV string of distances
                //
                string csvDistances = string.Empty;
                for (int i = 0; i < ddlSelectedDistances.Items.Count; i++)
                {
                    if (i > 0)
                    {
                        csvDistances += LabConsts.CHR_CsvSplitter.ToString();
                    }
                    csvDistances += ddlSelectedDistances.Items[i].Text;
                }

                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_sourceName, ddlSources.SelectedValue, false);
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_absorberName, ddlAbsorbers.SelectedValue, false);
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_distance, csvDistances, false);
            }
            else if (setupId.Equals(LabConsts.STRXML_SetupId_RadioactivityVsAbsorber))
            {
                //
                // Create CSV string of absorbers
                //
                string csvAbsorbers = string.Empty;
                for (int i = 0; i < ddlSelectedAbsorbers.Items.Count; i++)
                {
                    if (i > 0)
                    {
                        csvAbsorbers += LabConsts.CHR_CsvSplitter.ToString();
                    }
                    csvAbsorbers += ddlSelectedAbsorbers.Items[i].Text;
                }

                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_sourceName, ddlSources.SelectedValue, false);
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_absorberName, csvAbsorbers, false);
                XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_distance, ddlDistances.SelectedValue, false);
            }
            XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_duration, txbDuration.Text, false);
            XmlUtilities.SetXmlValue(xmlNodeSpecification, LabConsts.STRXML_repeat, txbRepeat.Text, false);

            return xmlNodeSpecification;
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnSelectedDistancesAdd_Click(object sender, EventArgs e)
        {
            //
            // Add distance to the selected list and remove from available list
            //
            if (ddlDistances.Items.Count > 0)
            {
                ddlSelectedDistances.Items.Add(ddlDistances.Text);
                ddlDistances.Items.Remove(ddlDistances.Text);
            }

            //
            // Disable Add button if no more distances to select
            //
            if (ddlDistances.Items.Count == 0)
            {
                btnSelectedDistancesAdd.Enabled = false;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnSelectedDistancesClear_Click(object sender, EventArgs e)
        {
            //
            // Clear the list of selected distances, enable the Add button
            // and repopulate the list of available distances
            //
            ddlSelectedDistances.Items.Clear();
            btnSelectedDistancesAdd.Enabled = true;
            PopulateDistances();
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnSelectedAbsorbersAdd_Click(object sender, EventArgs e)
        {
            //
            // Add absorber to the selected list and remove from available list
            //
            if (ddlAbsorbers.Items.Count > 0)
            {
                ddlSelectedAbsorbers.Items.Add(ddlAbsorbers.Text);
                ddlAbsorbers.Items.Remove(ddlAbsorbers.Text);
            }

            //
            // Disable Add button if no more absorbers to select
            //
            if (ddlAbsorbers.Items.Count == 0)
            {
                btnSelectedAbsorbersAdd.Enabled = false;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnSelectedAbsorbersClear_Click(object sender, EventArgs e)
        {
            //
            // Clear the list of selected absorbers, enable the Add button
            // and repopulate the list of available absorbers
            //
            ddlSelectedAbsorbers.Items.Clear();
            btnSelectedAbsorbersAdd.Enabled = true;
            PopulateAbsorbers();
        }

    }
}