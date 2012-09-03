using System;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Library.Lab;
using Library.LabClient;

namespace LabClientHtml
{
    public partial class Status : System.Web.UI.Page
    {
        #region Class Constants and Variables

        //
        // String constants
        //
        private const string STR_Online = "Online";
        private const string STR_Offline = "Offline";
        private const string STR_ExperimentNumber = "Experiment #";
        private const string STR_Spacer = " - ";
        private const string STR_HasBeenCancelled = " has been cancelled.";
        private const string STR_CouldNotBeCancelled = " could not be cancelled!";
        private const string STRWTR_TimeRemainingIs = " Time remaining is ";
        private const string STRWTR_QueueLengthAndWaitTimeIs = "Queue length is {0} and wait time is ";
        private const string STRWTR_QueuePositionAndRunIn = "Queue position is {0} and it will run in ";
        private const string STRWTR_MinutesAnd = "{0} minute{1} and ";
        private const string STRWTR_Seconds = "{0} second{1}.";

        //
        // String constants for error messages
        //
        private const string STRERR_ExperimentNumberNotSpecified = "Experiment number is not specified!";
        private const string STRERR_ExperimentNumberInvalid = "Experiment number is invalid!";
        private const string STRERR_GetExperimentStatusFailed = "Failed to get experiment status!";

        #endregion

        //-------------------------------------------------------------------------------------------------//

        protected void Page_Init(object sender, EventArgs e)
        {
            //
            // Set webpage title
            //
            Master.HeaderTitle = this.Title;
            this.Title = Master.PageTitle + this.Title;
        }

        //-------------------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            //
            // Hide message box
            //
            this.ShowMessageNormal(string.Empty);

            if (!IsPostBack)
            {
                //
                // It is not a postback. This page has been navigated to or been refreshed.
                //
                btnRefresh_Click1(sender, e);

                //
                // Set the dropdown list to not visible
                //
                ddlExperimentIDs.Visible = false;

                //
                // Initialise experiment number if experiment has been submitted
                //
                if (Master.MultiSubmit == true)
                {
                    if (Session[Consts.STRSSN_SubmittedIDs] != null)
                    {
                        //
                        // Get the list of submitted experiment IDs
                        //
                        int[] submittedIDs = (int[])Session[Consts.STRSSN_SubmittedIDs];
                        if (submittedIDs.Length > 0)
                        {
                            if (submittedIDs.Length == 1)
                            {
                                // There is only one
                                txbExperimentID.Text = submittedIDs[0].ToString();
                            }
                            else
                            {
                                //
                                // Populate the dropdown list with submitted experiment IDs
                                //
                                ddlExperimentIDs.Items.Clear();
                                for (int i = 0; i < submittedIDs.Length; i++)
                                {
                                    ddlExperimentIDs.Items.Add(submittedIDs[i].ToString());
                                }

                                //
                                // Insert an empty experiment ID at the start of the list so that
                                // the selected index can be changed
                                //
                                ddlExperimentIDs.Items.Insert(0, String.Empty);

                                // Make it visible
                                ddlExperimentIDs.Visible = true;
                            }
                        }
                    }
                }
                else
                {
                    //
                    // Get the submitted experiment ID
                    //
                    if (Session[Consts.STRSSN_SubmittedID] != null)
                    {
                        int submittedID = (int)Session[Consts.STRSSN_SubmittedID];
                        if (submittedID > 0)
                        {
                            txbExperimentID.Text = submittedID.ToString();
                        }
                    }
                }
            }
            else
            {
                //
                // It is a postback. A button on this page has been clicked to post back information.
                //
            }
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnRefresh_Click1(object sender, EventArgs e)
        {
            //
            // Get the LabServer status and display
            //
            try
            {
                LabStatus labStatus = Master.ServiceBroker.GetLabStatus();
                if (labStatus.online == true)
                {
                    //
                    // Display lab status
                    //
                    lblOnline.ForeColor = Color.Green;
                    lblOnline.Text = STR_Online;

                    //
                    // Display the queue length and wait time
                    //
                    WaitEstimate waitEstimate = Master.ServiceBroker.GetEffectiveQueueLength();
                    lblLabServerStatusMsg.Text = labStatus.labStatusMessage + STR_Spacer +
                        this.FormatQueueLengthWait(waitEstimate.effectiveQueueLength, (int)waitEstimate.estWait);
                }
                else
                {
                    //
                    // Display lab status and message
                    //
                    lblOnline.ForeColor = Color.Red;
                    lblOnline.Text = STR_Offline;
                    lblLabServerStatusMsg.Text = labStatus.labStatusMessage;
                }
            }
            catch (Exception ex)
            {
                // LabServer error
                Logfile.WriteError(ex.Message);
                lblLabServerStatusMsg.Text = STRERR_GetExperimentStatusFailed;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnCheck_Click(object sender, EventArgs e)
        {
            //
            // Get the experiment ID
            //
            int experimentID = ParseExperimentNumber(txbExperimentID.Text);
            if (experimentID <= 0)
            {
                return;
            }

            LabExperimentStatus labExperimentStatus = null;
            StatusCodes statusCode;

            //
            // Get the experiment status
            //
            try
            {
                //
                // Get the status of the selected experiment
                //
                labExperimentStatus = Master.ServiceBroker.GetExperimentStatus(experimentID);
                statusCode = (StatusCodes)labExperimentStatus.statusReport.statusCode;
            }
            catch (Exception)
            {
                statusCode = StatusCodes.Unknown;
            }

            //
            // Display the status code
            //
            ShowMessageNormal(STR_ExperimentNumber + experimentID.ToString() + STR_Spacer + statusCode.ToString());

            if (statusCode == StatusCodes.Running)
            {
                //
                // Experiment is currently running, display time remaining
                //
                int seconds = (int)Decimal.Round((decimal)labExperimentStatus.statusReport.estRemainingRuntime);
                ShowMessageNormal(this.FormatTimeRemaining(seconds));
            }
            else if (statusCode == StatusCodes.Waiting)
            {
                //
                // Experiment is waiting to run, get queue position (zero-based)
                //
                int position = labExperimentStatus.statusReport.wait.effectiveQueueLength;
                int seconds = (int)Decimal.Round((decimal)labExperimentStatus.statusReport.wait.estWait);
                seconds = (seconds < 0) ? 0 : seconds;
                ShowMessageNormal(this.FormatQueuePosition(position, seconds));
            }
            else if (statusCode == StatusCodes.Completed || statusCode == StatusCodes.Failed || statusCode == StatusCodes.Cancelled)
            {
                //
                // Experiment status no longer needs to be checked
                //
                if (Master.MultiSubmit == true)
                {
                    if (Session[Consts.STRSSN_SubmittedIDs] != null)
                    {
                        //
                        // Get the list of submitted experiment IDs
                        //
                        int[] submittedIDs = (int[])Session[Consts.STRSSN_SubmittedIDs];

                        //
                        // Find submitted experiment number
                        //
                        for (int i = 0; i < submittedIDs.Length; i++)
                        {
                            if (submittedIDs[i] == experimentID)
                            {
                                //
                                // Add experiment number to the completed list in the session
                                //
                                if (Session[Consts.STRSSN_CompletedIDs] != null)
                                {
                                    // Get the list of completed experiment IDs
                                    int[] completedIDs = (int[])Session[Consts.STRSSN_CompletedIDs];

                                    // Create a bigger array and copy completed experiment IDs
                                    int[] newCompletedIDs = new int[completedIDs.Length + 1];
                                    completedIDs.CopyTo(newCompletedIDs, 0);

                                    // Add the experiment ID to the bigger array
                                    newCompletedIDs[completedIDs.Length] = experimentID;

                                    // Save experiment ID in the session
                                    Session[Consts.STRSSN_CompletedIDs] = newCompletedIDs;
                                }
                                else
                                {
                                    // Create an array and add the experiment ID
                                    int[] completedIDs = new int[1];
                                    completedIDs[0] = experimentID;

                                    // Save experiment ID in the session
                                    Session[Consts.STRSSN_CompletedIDs] = completedIDs;
                                }

                                //
                                // Remove experiment number from the submitted list in the session
                                //
                                if (submittedIDs.Length == 1)
                                {
                                    Session[Consts.STRSSN_SubmittedIDs] = null;

                                    //
                                    // Clear dropdown list and hide
                                    //
                                    ddlExperimentIDs.Items.Clear();
                                    ddlExperimentIDs.Visible = false;
                                }
                                else
                                {
                                    //
                                    // Create a smaller array and copy submitted experiment IDs
                                    //
                                    int[] newSubmittedIDs = new int[submittedIDs.Length - 1];

                                    //
                                    // Copy experiment IDs up to the one being removed
                                    //
                                    for (int j = 0; j < i; j++)
                                    {
                                        newSubmittedIDs[j] = submittedIDs[j];
                                    }

                                    //
                                    // Copy experiment IDs after the one being removed
                                    //
                                    for (int j = i + 1; j < submittedIDs.Length; j++)
                                    {
                                        newSubmittedIDs[j - 1] = submittedIDs[j];
                                    }

                                    // Save experiment IDs in the session
                                    Session[Consts.STRSSN_SubmittedIDs] = newSubmittedIDs;

                                    // Remove experiment ID from dropdown list
                                    ddlExperimentIDs.Items.Remove(experimentID.ToString());
                                }

                                break;
                            }
                        }

                    }
                }
                else
                {
                    if (Session[Consts.STRSSN_SubmittedID] != null)
                    {
                        //
                        // Check experiment ID against submitted experiment ID
                        //
                        int submittedId = (int)Session[Consts.STRSSN_SubmittedID];
                        if (experimentID == submittedId)
                        {
                            Session[Consts.STRSSN_CompletedID] = submittedId;
                            Session[Consts.STRSSN_SubmittedID] = null;
                        }
                    }
                }
            }

            // Clear the LabServer status
            lblLabServerStatusMsg.Text = null;
        }

        //-------------------------------------------------------------------------------------------------//

        protected void ddlExperimentIDs_SelectedIndexChanged(object sender, EventArgs e)
        {
            txbExperimentID.Text = ddlExperimentIDs.SelectedValue;
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            //
            // Get the experiment number
            //
            int experimentNo = ParseExperimentNumber(txbExperimentID.Text);
            if (experimentNo < 0)
            {
                return;
            }

            try
            {
                //
                // Attempt to cancel the selected experiment
                //
                bool cancelled = Master.ServiceBroker.Cancel(experimentNo);

                //
                // Display cancel status
                //
                string message = STR_ExperimentNumber + experimentNo.ToString();
                if (cancelled == true)
                {
                    ShowMessageNormal(message + STR_HasBeenCancelled);
                }
                else
                {
                    ShowMessageError(message + STR_CouldNotBeCancelled);
                }
            }
            catch (Exception ex)
            {
                ShowMessageError(ex.Message);
            }
        }

        //=================================================================================================//

        private void ShowMessage(string message)
        {
            message = message.Trim();
            lblExpStatMessage.Text = message;
            lblExpStatMessage.Visible = (message.Length > 0);
        }

        //-------------------------------------------------------------------------------------------------//

        private void ShowMessageNormal(string message)
        {
            lblExpStatMessage.ForeColor = Color.Black;
            ShowMessage(message);
        }

        //-------------------------------------------------------------------------------------------------//

        private void ShowMessageError(string message)
        {
            lblExpStatMessage.ForeColor = Color.Red;
            ShowMessage(message);
        }

        //-------------------------------------------------------------------------------------------------//

        private int ParseExperimentNumber(string strExperimentNo)
        {
            //
            // Get the experiment number
            //
            int experimentNo = 0;
            try
            {
                //
                // Check if experiment number is entered
                //
                if (strExperimentNo.Trim().Length == 0)
                {
                    throw new ArgumentException(STRERR_ExperimentNumberNotSpecified);
                }

                //
                // Determine the experiment number
                //
                try
                {
                    experimentNo = Int32.Parse(strExperimentNo);
                }
                catch
                {
                    throw new ArgumentException(STRERR_ExperimentNumberInvalid);
                }

                //
                // Check that experiment ID is greater than 0
                //
                if (experimentNo <= 0)
                {
                    throw new ArgumentException(STRERR_ExperimentNumberInvalid);
                }
            }
            catch (Exception ex)
            {
                ShowMessageError(ex.Message);
            }

            return experimentNo;
        }

        //-------------------------------------------------------------------------------------------------//

        private string FormatTimeRemaining(int seconds)
        {
            int minutes = seconds / 60;
            seconds -= minutes * 60;

            string message = string.Empty;

            try
            {
                StringWriter sw = new StringWriter();

                sw.Write(STRWTR_TimeRemainingIs);
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

        private string FormatQueueLengthWait(int length, int seconds)
        {
            int minutes = seconds / 60;
            seconds -= minutes * 60;

            string message = string.Empty;

            try
            {
                StringWriter sw = new StringWriter();

                sw.Write(STRWTR_QueueLengthAndWaitTimeIs, length.ToString());
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

        private string FormatQueuePosition(int position, int seconds)
        {
            int minutes = seconds / 60;
            seconds -= minutes * 60;

            string message = string.Empty;

            try
            {
                StringWriter sw = new StringWriter();

                sw.Write(STRWTR_QueuePositionAndRunIn, position.ToString());
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
