using System;
using System.Drawing;
using System.IO;
using System.Xml;
using System.Web.UI;
using Library.Lab;
using Library.LabClient;
using LabClientHtml.LabControls;

namespace LabClientHtml
{
    public partial class Results : System.Web.UI.Page
    {
        #region Class Constants and Variables

        //
        // String constants
        //
        private const string STR_ExperimentNumber = "Experiment #";
        private const string STR_Spacer = " - ";

        private const string STR_ExperimentInformation = "Experiment Information";
        private const string STR_CsvExperimentInformation = "---Experiment Information---";
        private const string STR_Timestamp = "Timestamp";
        private const string STR_ExperimentID = "Experiment ID";
        private const string STR_UnitID = "Unit ID";
        private const string STR_setupName = "Setup Name";
        private const string STR_DataType = "Data Type";

        private const string STR_ExperimentSetup = "Experiment Setup";
        private const string STR_CsvExperimentSetup = "---Experiment Setup---";

        private const string STR_ExperimentResults = "Experiment Results";
        private const string STR_CsvExperimentResults = "---Experiment Results---";

        private const string STR_ErrorMessage = "Error Message";

        private const string STR_SwTblBegin = "<table id=\"results\" cols=\"3\" cellspacing=\"0\" cellpadding=\"0\">";
        private const string STR_SwTblHdrArgument = "<tr align=\"left\"><th colspan=\"3\"><nobr>{0}</nobr></th></tr>";
        private const string STR_SwTblArgument = "<tr><td class=\"label\"><nobr>{0}:</nobr></td><td class=\"dataright\">{1}</td></tr>";
        private const string STR_SwTblBlankRow = "<tr><td colspan=\"3\">&nbsp;</td></tr>";
        private const string STR_SwTblEnd = "</table>";

        private const string STR_SwCsvArgument = "{0},{1}";

        private const string STR_SwAppletArgument = "<param name=\"{0}\" value=\"{1}\">";
        private const string STR_HtmlAppletTag_Args5 =
            "<applet width=\"{0}\" height=\"{1}\" archive=\"{2}\" code=\"{3}\"\r\n" +
            "alt=\"This is ALT text.  Couldn't load the applet.\">\r\n" +
            "{4}\r\n" +
            "<strong>Could not load the applet!</strong>\r\n" +
            "</applet>\r\n";

        //
        // String constants for exception messages
        //
        private const string STRERR_ExperimentNumberNotSpecified = "Experiment number is not specified!";
        private const string STRERR_ExperimentNumberInvalid = "Experiment number is invalid!";
        private const string STRERR_IncorrectExperimentType = "Incorrect experiment type: ";

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
                lblHiddenResults.Visible = false;
                lblHiddenApplet.Visible = false;
                btnSave.Enabled = false;
                btnDisplay.Enabled = false;
                btnDisplay.Visible = false;

                //
                // Set the dropdown list of completed experiment IDs to not visible
                //
                ddlExperimentIDs.Visible = false;

                //
                // Initialise experiment number if experiment has completed
                //
                if (Master.MultiSubmit == true)
                {
                    if (Session[Consts.STRSSN_CompletedIDs] != null)
                    {
                        //
                        // Get the list of completed experiment IDs
                        //
                        int[] completedIDs = (int[])Session[Consts.STRSSN_CompletedIDs];
                        if (completedIDs.Length > 0)
                        {
                            if (completedIDs.Length == 1)
                            {
                                // There is only one
                                txbExperimentID.Text = completedIDs[0].ToString();
                            }
                            else
                            {
                                //
                                // Populate the dropdown list with completed experiment IDs
                                //
                                ddlExperimentIDs.Items.Clear();
                                for (int i = 0; i < completedIDs.Length; i++)
                                {
                                    ddlExperimentIDs.Items.Add(completedIDs[i].ToString());
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
                    // Get the completed experiment ID
                    //
                    if (Session[Consts.STRSSN_CompletedID] != null)
                    {
                        int completedID = (int)Session[Consts.STRSSN_CompletedID];
                        if (completedID > 0)
                        {
                            txbExperimentID.Text = completedID.ToString();
                        }
                    }
                }
            }
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnRetrieve_Click(object sender, EventArgs e)
        {
            //
            // Clear hidden labels and disable buttons
            //
            lblHiddenResults.Text = null;
            lblHiddenApplet.Text = null;
            btnSave.Enabled = false;
            btnDisplay.Enabled = false;
            btnDisplay.Visible = false;

            //
            // Get the experiment ID
            //
            int experimentID = ParseExperimentNumber(txbExperimentID.Text);
            if (experimentID <= 0)
            {
                return;
            }

            //
            // Get the experiment results for the selected experiment
            //
            try
            {
                StatusCodes statusCode;
                string errorMessage = null;
                string experimentResults = null;

                //
                // Get ServiceBrokerAPI from session state and retrieve the result
                //
                try
                {
                    ResultReport resultReport = Master.ServiceBroker.RetrieveResult(experimentID);
                    statusCode = (StatusCodes)resultReport.statusCode;
                    errorMessage = resultReport.errorMessage;
                    experimentResults = resultReport.experimentResults;
                }
                catch (Exception ex)
                {
                    // LabServer error
                    Logfile.WriteError(ex.Message);
                    statusCode = StatusCodes.Unknown;
                }

                //
                // Display the status code
                //
                ShowMessageNormal(STR_ExperimentNumber + experimentID.ToString() + STR_Spacer + statusCode.ToString());

                if (statusCode != StatusCodes.Unknown && experimentResults != null)
                {
                    //
                    // Get result information
                    //
                    Result result = new Result(experimentResults);
                    ResultInfo resultInfo = result.GetResultInfo();

                    //
                    // Check experiment type
                    //
                    if (resultInfo != null && resultInfo.title != null &&
                        resultInfo.title.Equals(Master.Title, StringComparison.OrdinalIgnoreCase) == false)
                    {
                        //
                        // Wrong experiment type
                        //
                        ShowMessageWarning(STRERR_IncorrectExperimentType + Master.Title);
                        return;
                    }

                    //
                    // Build table to display results on the webpage
                    //
                    string strResultsTable = BuildTableResult(resultInfo, statusCode, errorMessage);
                    phResultsTable.Controls.Add(new LiteralControl(strResultsTable));

                    if (statusCode == StatusCodes.Completed)
                    {
                        try
                        {
                            //
                            // Build a CSV string from the result report and store in a hidden label
                            //
                            lblHiddenResults.Text = BuildCsvResult(resultInfo);

                            // Enable button
                            btnSave.Enabled = true;

                            //
                            // Create HTML applet tag for insertion into the webpage
                            //
                            string appletParams = BuildAppletParams(resultInfo);
                            string applet = CreateHtmlAppletTag(appletParams);
                            if (applet != null)
                            {
                                // Save applet for displaying in a hidden label
                                lblHiddenApplet.Text = applet;

                                // Enable applet display
                                btnDisplay.Enabled = true;
                                btnDisplay.Visible = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            ShowMessageWarning(ex.Message);
                        }
                    }

                    //
                    // Completed experiment is no longer needed
                    //
                    if (Master.MultiSubmit == true)
                    {
                    }
                    else
                    {
                        Session[Consts.STRSSN_CompletedID] = null;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessageWarning(ex.Message);
                return;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        protected void ddlExperimentIDs_SelectedIndexChanged(object sender, EventArgs e)
        {
            txbExperimentID.Text = ddlExperimentIDs.SelectedValue;
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Download the result string as an Excel csv file

            // Set the content type of the file to be downloaded
            Response.ContentType = Consts.STRRSP_ContentTypeCsv;

            // Clear all response headers
            Response.Clear();

            // Add response header
            Response.AddHeader(Consts.STRRSP_Disposition, Consts.STRRSP_AttachmentCsv);

            // Add specification string
            Response.Write(lblHiddenResults.Text);

            // End the http response
            Response.End();
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnDisplay_Click(object sender, EventArgs e)
        {
            //
            // Display the experiment result information
            //
            if (lblHiddenApplet.Text != null)
            {
                PlaceHolder1.Controls.Add(new LiteralControl(lblHiddenApplet.Text));
                PlaceHolder1.Controls.Add(new LiteralControl("</td></tr><tr><td>"));
            }
        }

        //=================================================================================================//

        private void ShowMessage(string message)
        {
            message = message.Trim();
            lblResultMessage.Text = message;
            lblResultMessage.Visible = (message.Length > 0);
        }

        //-------------------------------------------------------------------------------------------------//

        private void ShowMessageNormal(string message)
        {
            lblResultMessage.ForeColor = Color.Black;
            ShowMessage(message);
        }

        //-------------------------------------------------------------------------------------------------//

        private void ShowMessageWarning(string message)
        {
            lblResultMessage.ForeColor = Color.Blue;
            ShowMessage(message);
        }

        //-------------------------------------------------------------------------------------------------//

        private void ShowMessageError(string message)
        {
            lblResultMessage.ForeColor = Color.Red;
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

        private string BuildTableResult(ResultInfo resultInfo, StatusCodes statusCode, string errorMessage)
        {
            string strTableResult = string.Empty;

            try
            {
                StringWriter sw = new StringWriter();
                sw.WriteLine(STR_SwTblBegin);

                //
                // Experiment information
                //
                sw.WriteLine(STR_SwTblBlankRow);
                sw.WriteLine(STR_SwTblHdrArgument, STR_ExperimentInformation);
                sw.WriteLine(STR_SwTblArgument, STR_Timestamp, resultInfo.timestamp);
                sw.WriteLine(STR_SwTblArgument, STR_ExperimentID, resultInfo.experimentId);
                if (resultInfo.title != null)
                {
                    sw.WriteLine(STR_SwTblArgument, STR_UnitID, resultInfo.unitId);
                    if (resultInfo.dataType != null)
                    {
                        sw.WriteLine(STR_SwTblArgument, STR_DataType, resultInfo.dataType);
                    }
                }

                //
                // Experiment setup
                //
                sw.WriteLine(STR_SwTblBlankRow);
                sw.WriteLine(STR_SwTblHdrArgument, STR_ExperimentSetup);
                if (resultInfo.setupName != null)
                {
                    sw.WriteLine(STR_SwTblArgument, STR_setupName, resultInfo.setupName);
                }
                string csvSpecification = labResults.CreateSpecificationString(resultInfo, STR_SwTblArgument);
                sw.Write(csvSpecification);

                //
                // Experiment results
                //
                sw.WriteLine(STR_SwTblBlankRow);
                sw.WriteLine(STR_SwTblHdrArgument, STR_ExperimentResults);

                //
                // Check if experiment had completed successfully
                //
                if (statusCode == StatusCodes.Completed)
                {
                    // Include experiment results
                    string tblResults = labResults.CreateResultsString(resultInfo, STR_SwTblArgument);
                    sw.Write(tblResults);
                }
                else
                {
                    sw.WriteLine(STR_SwTblArgument, STR_ErrorMessage, errorMessage);
                }

                sw.WriteLine(STR_SwTblEnd);

                strTableResult = sw.ToString();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            return strTableResult;
        }

        //-------------------------------------------------------------------------------------------------//

        private string BuildCsvResult(ResultInfo resultInfo)
        {
            string strCsvResult = string.Empty;

            try
            {
                StringWriter sw = new StringWriter();

                //
                // Experiment information
                //
                sw.WriteLine();
                sw.WriteLine(STR_CsvExperimentInformation);
                sw.WriteLine(STR_SwCsvArgument, STR_Timestamp, resultInfo.timestamp);
                sw.WriteLine(STR_SwCsvArgument, STR_ExperimentID, resultInfo.experimentId);
                if (resultInfo.title != null)
                {
                    sw.WriteLine(STR_SwCsvArgument, STR_UnitID, resultInfo.unitId);
                    if (resultInfo.dataType != null)
                    {
                        sw.WriteLine(STR_SwCsvArgument, STR_DataType, resultInfo.dataType);
                    }
                }

                //
                // Experiment setup
                //
                sw.WriteLine();
                sw.WriteLine(STR_CsvExperimentSetup);
                if (resultInfo.title != null)
                {
                    sw.WriteLine(STR_SwCsvArgument, STR_setupName, resultInfo.setupName);
                }
                string csvSpecification = labResults.CreateSpecificationString(resultInfo, STR_SwCsvArgument);
                sw.Write(csvSpecification);

                //
                // Experiment results
                //
                sw.WriteLine();
                sw.WriteLine(STR_CsvExperimentResults);

                string csvResults = labResults.CreateResultsString(resultInfo, STR_SwCsvArgument);
                sw.Write(csvResults);

                strCsvResult = sw.ToString();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            return strCsvResult;
        }

        //-------------------------------------------------------------------------------------------------//

        private string BuildAppletParams(ResultInfo resultInfo)
        {
            string strAppletParams = string.Empty;

            try
            {
                StringWriter sw = new StringWriter();

                // Experiment specification
                sw.Write(labResults.CreateSpecificationString(resultInfo, STR_SwAppletArgument));

                // Experiment results
                sw.Write(labResults.CreateResultsString(resultInfo, STR_SwAppletArgument));

                strAppletParams = sw.ToString();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            return strAppletParams;
        }

        //-------------------------------------------------------------------------------------------------//

        private string CreateHtmlAppletTag(string appletParams)
        {
            // Clear applet string
            lblHiddenApplet.Text = null;

            string htmlAppletTag = null;
            try
            {
                int width = 1;
                int height = 1;

                //
                // Get applet archive and code
                //
                XmlNode xmlNodeLabConfiguration = Master.XmlNodeLabConfiguration;
                string appletArchive = XmlUtilities.GetXmlValue(xmlNodeLabConfiguration, Consts.STRXML_resultsApplet_archive, true);
                string appletCode = XmlUtilities.GetXmlValue(xmlNodeLabConfiguration, Consts.STRXML_resultsApplet_code, true);

                //
                // Ensure that the applet archive and code are valid
                //
                if (appletArchive != null && appletArchive.Length > 0 && appletCode != null && appletCode.Length > 0)
                {
                    //
                    // Check to see if the applet archive file exists
                    //
                    string filepath = Path.GetDirectoryName(appletArchive);
                    filepath = MapPath(filepath);
                    string filename = Path.GetFileName(appletArchive);
                    filename = Path.Combine(filepath, filename);
                    if (File.Exists(filename) == true)
                    {
                        //
                        // Create the HTML applet tag
                        //
                        StringWriter sw = new StringWriter();
                        sw.WriteLine(STR_HtmlAppletTag_Args5, width, height, appletArchive, appletCode, appletParams);
                        htmlAppletTag = sw.ToString();
                    }
                    else
                    {
                        // Doesn't exist
                        htmlAppletTag = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            return htmlAppletTag;
        }

    }
}
