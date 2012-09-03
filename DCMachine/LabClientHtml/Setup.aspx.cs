using System;
using System.Drawing;
using System.IO;
using System.Xml;
using Library.Lab;
using Library.LabClient;

namespace LabClientHtml
{
    public partial class Setup : System.Web.UI.Page
    {
        #region Class Constants and Variables

        //
        // String constants
        //
        private const string STR_SpecificationValid = "Specification is valid. ";
        private const string STR_SubmissionSuccessful = "Submission was successful. ";
        private const string STR_ExperimentNo = "Experiment #";
        private const string STR_HasBeenSubmitted = " has been submitted.";
        private const string STR_ExperimentNos = "Experiments #";
        private const string STR_HaveBeenSubmitted = " have been submitted.";
        private const string STRWTR_ExperimentNoIs = " Experiment # is {0}.";
        private const string STRWTR_ExecutionTimeWillBe = " Execution time will be ";
        private const string STRWTR_ExecutionTimeIs = " Execution time is ";
        private const string STRWTR_MinutesAnd = "{0} minute{1} and ";
        private const string STRWTR_Seconds = "{0} second{1}.";

        //
        // String constants for error messages
        //
        private const string STRERR_NullConfiguration = "Configuration is null!";
        private const string STRERR_ValidationFailed = "Failed to validate experiment!";
        private const string STRERR_SubmissionFailed = "Failed to submit experiment!";

        #endregion

        //---------------------------------------------------------------------------------------//

        protected void Page_Init(object sender, EventArgs e)
        {
            //
            // Set webpage title
            //
            Master.HeaderTitle = this.Title;
            this.Title = Master.PageTitle + this.Title;
        }

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            //
            // Hide message box
            //
            this.ShowMessageNormal(string.Empty);

            //
            // Update local variables
            //
            labSetup.XmlNodeConfiguration = Master.XmlNodeConfiguration;
            labSetup.XmlNodeValidation = Master.XmlNodeValidation;

            if (!IsPostBack)
            {
                //
                // It is not a postback. This page has been navigated to or been refreshed.
                //
                PopulatePageControls();
            }

            //
            // Check if an experiment has been submitted
            //
            if (Master.MultiSubmit == true)
            {
                if (Session[Consts.STRSSN_SubmittedIDs] != null)
                {
                    int[] submittedIDs = (int[])Session[Consts.STRSSN_SubmittedIDs];
                    if (submittedIDs.Length == 1 && submittedIDs[0] > 0)
                    {
                        this.ShowMessage(STR_ExperimentNo + submittedIDs[0].ToString() + STR_HasBeenSubmitted);
                    }
                    else if (submittedIDs.Length > 1)
                    {
                        string submittedIds = string.Empty;
                        for (int i = 0; i < submittedIDs.Length; i++)
                        {
                            submittedIds += submittedIDs[i].ToString() + " ";
                        }
                        this.ShowMessage(STR_ExperimentNos + submittedIds + STR_HaveBeenSubmitted);
                    }
                }
            }
            else
            {
                if (Session[Consts.STRSSN_SubmittedID] != null)
                {
                    int submittedID = (int)Session[Consts.STRSSN_SubmittedID];
                    if (submittedID > 0)
                    {
                        //
                        // Experiment has been submitted but not checked
                        //
                        this.ShowMessage(STR_ExperimentNo + submittedID.ToString() + STR_HasBeenSubmitted);
                        btnSubmit.Enabled = false;
                    }
                }
            }
        }

        //-------------------------------------------------------------------------------------------------//

        protected void ddlExperimentSetups_SelectedIndexChanged(object sender, EventArgs e)
        {
            //
            // Cannot do anything without a configuration
            //
            if (Master.XmlNodeConfiguration == null)
            {
                return;
            }

            // Update page controls for the selected index
            UpdatePageControls();

            //
            // Tell LabSetup control that a different setup has been selected
            //
            XmlNodeList xmlNodeList = XmlUtilities.GetXmlNodeList(Master.XmlNodeConfiguration, Consts.STRXML_setup, false);
            labSetup.XmlNodeSelectedSetup = xmlNodeList.Item(ddlExperimentSetups.SelectedIndex);
            labSetup.ddlExperimentSetups_SelectedIndexChanged(sender, e);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnValidate_Click1(object sender, EventArgs e)
        {
            try
            {
                // Build the XML specification string
                string xmlSpecification = this.BuildSpecification();

                //
                // Validate the experiment specification
                //
                ValidationReport validationReport = Master.ServiceBroker.Validate(xmlSpecification);
                if (validationReport.accepted)
                {
                    // Specification was accepted
                    ShowMessageNormal(STR_SpecificationValid + FormatValidation((int)validationReport.estRuntime));
                }
                else
                {
                    // Specification was rejected
                    this.ShowMessageError(validationReport.errorMessage);
                }
            }
            catch (Exception ex)
            {
                // LabServer error
                Logfile.WriteError(ex.Message);
                this.ShowMessageFailure(STRERR_ValidationFailed);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnSubmit_Click1(object sender, EventArgs e)
        {
            //
            // Check if an experiment has already been submitted. The Submit button's
            // enable state gets set when page is loaded.
            //
            if (btnSubmit.Enabled == false)
            {
                return;
            }

            try
            {
                // Build the XML specification string
                string xmlSpecification = this.BuildSpecification();

                //
                // Submit the experiment specification
                //
                SubmissionReport submissionReport = Master.ServiceBroker.Submit(xmlSpecification);
                if (submissionReport.vReport.accepted == true)
                {
                    //
                    // Submission was accepted
                    //
                    ShowMessageNormal(STR_SubmissionSuccessful +
                        FormatSubmission(submissionReport.experimentID, (int)submissionReport.vReport.estRuntime));

                    //
                    // Update session with submitted experiment ID
                    //
                    if (Master.MultiSubmit == true)
                    {
                        // Add experiment ID to the list in the session
                        if (Session[Consts.STRSSN_SubmittedIDs] != null)
                        {
                            // Get the list of submitted experiment IDs
                            int[] submittedIDs = (int[])Session[Consts.STRSSN_SubmittedIDs];

                            // Create a bigger array and copy submitted experiment IDs
                            int[] newSubmittedIDs = new int[submittedIDs.Length + 1];
                            submittedIDs.CopyTo(newSubmittedIDs, 0);

                            // Add the experiment ID to the bigger array
                            newSubmittedIDs[submittedIDs.Length] = submissionReport.experimentID;

                            // Save experiment IDs in the session
                            Session[Consts.STRSSN_SubmittedIDs] = newSubmittedIDs;
                        }
                        else
                        {
                            // Create an array and add the experiment ID
                            int[] submittedIDs = new int[1];
                            submittedIDs[0] = submissionReport.experimentID;

                            // Save experiment IDs in the session
                            Session[Consts.STRSSN_SubmittedIDs] = submittedIDs;
                        }
                    }
                    else
                    {
                        // Save experiment ID in the session
                        Session[Consts.STRSSN_SubmittedID] = submissionReport.experimentID;

                        // Update buttons
                        btnValidate.Enabled = false;
                        btnSubmit.Enabled = false;
                    }
                }
                else
                {
                    // Submission was rejected
                    this.ShowMessageError(submissionReport.vReport.errorMessage);
                }
            }
            catch (Exception ex)
            {
                // LabServer error
                Logfile.WriteError(ex.Message);
                this.ShowMessageFailure(STRERR_SubmissionFailed);
            }
        }

        //=================================================================================================//

        private void PopulatePageControls()
        {
            //
            // Cannot do anything without a configuration
            //
            if (Master.XmlNodeConfiguration == null)
            {
                return;
            }

            //
            // Get all setups and add to the dropdownlist
            //
            XmlNodeList xmlNodeList = XmlUtilities.GetXmlNodeList(Master.XmlNodeConfiguration, Consts.STRXML_setup, true);
            for (int i = 0; i < xmlNodeList.Count; i++)
            {
                XmlNode xmlNodeTemp = xmlNodeList.Item(i);

                //
                // Get the setup id and setup name and add to the dropdown list
                //
                string setupId = XmlUtilities.GetXmlValue(xmlNodeTemp, Consts.STRXMLPARAM_id, true);
                string setupName = XmlUtilities.GetXmlValue(xmlNodeTemp, Consts.STRXML_name, true);
                if (setupId.Length > 0 && setupName.Length > 0)
                {
                    ddlExperimentSetupIds.Items.Add(setupId);
                    ddlExperimentSetups.Items.Add(setupName);
                }
            }

            //
            // Set the selected index for the experiment setups and tell LabSetup control
            //
            ddlExperimentSetups.SelectedIndex = 0;
            labSetup.XmlNodeSelectedSetup = xmlNodeList.Item(ddlExperimentSetups.SelectedIndex);

            //
            // Update page controls for the selected index
            //
            UpdatePageControls();
        }

        //-------------------------------------------------------------------------------------------------//

        private void UpdatePageControls()
        {
            //
            // Cannot do anything without a configuration
            //
            if (Master.XmlNodeConfiguration == null)
            {
                return;
            }

            //
            // Update page controls for the selected index
            //
            XmlNodeList xmlNodeList = XmlUtilities.GetXmlNodeList(Master.XmlNodeConfiguration, Consts.STRXML_setup, false);
            XmlNode xmlNodeSetup = xmlNodeList.Item(ddlExperimentSetups.SelectedIndex);

            // Set the description for the setup
            lblSetupDescription.Text = XmlUtilities.GetXmlValue(xmlNodeSetup, Consts.STRXML_description, false);
        }

        //---------------------------------------------------------------------------------------//

        private string BuildSpecification()
        {
            //
            // Get setup Id
            //
            XmlNodeList xmlNodeList = XmlUtilities.GetXmlNodeList(Master.XmlNodeConfiguration, Consts.STRXML_setup, false);
            XmlNode xmlNodeSetup = xmlNodeList.Item(ddlExperimentSetups.SelectedIndex);
            string setupId = XmlUtilities.GetXmlValue(xmlNodeSetup, Consts.STRXMLPARAM_id, false);

            //
            // Get a copy of the XML specification node and fill in
            //
            XmlNode xmlNodeSpecification = Master.XmlNodeSpecification.Clone();
            XmlUtilities.SetXmlValue(xmlNodeSpecification, Consts.STRXML_setupId, setupId, true);
            xmlNodeSpecification = labSetup.BuildSpecification(xmlNodeSpecification, setupId);

            //
            // Write the Xml specification to a string
            //
            StringWriter xmlSpecification = new StringWriter();
            XmlTextWriter xtw = new XmlTextWriter(xmlSpecification);
            xtw.Formatting = Formatting.Indented;
            xmlNodeSpecification.WriteTo(xtw);
            xtw.Flush();

            return xmlSpecification.ToString();
        }

        //-------------------------------------------------------------------------------------------------//

        private void ShowMessage(string message)
        {
            message = message.Trim();
            lblMessage.Text = message;
            lblMessage.Visible = (message.Length > 0);
        }

        //-------------------------------------------------------------------------------------------------//

        private void ShowMessageNormal(string message)
        {
            lblMessage.ForeColor = Color.Black;
            ShowMessage(message);
        }

        //-------------------------------------------------------------------------------------------------//

        private void ShowMessageError(string message)
        {
            lblMessage.ForeColor = Color.Red;
            ShowMessage(message);
        }

        //-------------------------------------------------------------------------------------------------//

        private void ShowMessageFailure(string message)
        {
            lblMessage.ForeColor = Color.Blue;
            ShowMessage(message);
        }

        //-------------------------------------------------------------------------------------------------//

        private string FormatValidation(int seconds)
        {
            int minutes = seconds / 60;
            seconds -= minutes * 60;

            string message = string.Empty;

            try
            {
                StringWriter sw = new StringWriter();

                sw.Write(STRWTR_ExecutionTimeWillBe);
                if (minutes > 0)
                {
                    // Display minutes
                    sw.Write(STRWTR_MinutesAnd, minutes.ToString(), FormatPlural(minutes));
                }
                // Display seconds
                sw.Write(STRWTR_Seconds, seconds.ToString(), FormatPlural(seconds));

                message = sw.ToString();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            return message;
        }

        //-------------------------------------------------------------------------------------------------//

        private string FormatSubmission(int experimentID, int seconds)
        {
            int minutes = seconds / 60;
            seconds -= minutes * 60;

            string message = string.Empty;

            try
            {
                StringWriter sw = new StringWriter();

                sw.Write(STRWTR_ExperimentNoIs, experimentID);

                sw.Write(STRWTR_ExecutionTimeIs);
                if (minutes > 0)
                {
                    // Display minutes
                    sw.Write(STRWTR_MinutesAnd, minutes.ToString(), FormatPlural(minutes));
                }
                // Display seconds
                sw.Write(STRWTR_Seconds, seconds.ToString(), FormatPlural(seconds));

                message = sw.ToString();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            return message;
        }

        //-------------------------------------------------------------------------------------------------//

        private string FormatPlural(int value)
        {
            return (value == 1) ? string.Empty : "s";
        }

    }
}
