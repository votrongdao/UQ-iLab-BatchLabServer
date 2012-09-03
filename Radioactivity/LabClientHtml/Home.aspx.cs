using System;
using Library.Lab;

namespace LabClientHtml
{
    public partial class Home : System.Web.UI.Page
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "LabClient";

        //
        // String constants
        //
        private const string STR_Less = "&#171; Less";
        private const string STR_More = "More &#187;";
        private const string STR_ForMoreInfoSee = "For information specific to this LabClient, see";

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
            if (!IsPostBack)
            {
                //
                // Update lab info text and Url if specified
                //
                string labInfoText = Master.LabinfoText;
                string labInfoUrl = Master.LabInfoUrl;
                if (labInfoText != null && labInfoText.Length > 0 &&
                    labInfoUrl != null && labInfoUrl.Length > 0)
                {
                    //
                    // Update hyperlink on webpage
                    //
                    lblMoreInfo.Text = STR_ForMoreInfoSee;
                    lnkMoreInfo.Text = labInfoText;
                    lnkMoreInfo.NavigateUrl = labInfoUrl;
                }

                //
                // Don't display the extra information
                //
                litSetupInfo.Visible = false;
                lnkbtnSetupInfo.Text = STR_More;
                litStatusInfo.Visible = false;
                lnkbtnStatusInfo.Text = STR_More;
                litResultsInfo.Visible = false;
                lnkbtnResultsInfo.Text = STR_More;
            }
            else
            {
                //
                // Clear labels
                //
                lblMoreInfo.Text = null;
                lnkMoreInfo.Text = null;
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void lnkbtnSetupInfo_Click(object sender, EventArgs e)
        {
            if (litSetupInfo.Visible == false)
            {
                litSetupInfo.Visible = true;
                lnkbtnSetupInfo.Text = STR_Less;
            }
            else
            {
                litSetupInfo.Visible = false;
                lnkbtnSetupInfo.Text = STR_More;
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void lnkbtnStatusInfo_Click(object sender, EventArgs e)
        {
            if (litStatusInfo.Visible == false)
            {
                litStatusInfo.Visible = true;
                lnkbtnStatusInfo.Text = STR_Less;
            }
            else
            {
                litStatusInfo.Visible = false;
                lnkbtnStatusInfo.Text = STR_More;
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void lnkbtnResultsInfo_Click(object sender, EventArgs e)
        {
            if (litResultsInfo.Visible == false)
            {
                litResultsInfo.Visible = true;
                lnkbtnResultsInfo.Text = STR_Less;
            }
            else
            {
                litResultsInfo.Visible = false;
                lnkbtnResultsInfo.Text = STR_More;
            }
        }

    }
}
