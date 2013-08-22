using System;
using System.Configuration;
using System.Net.Mail;
using System.Text;
using System.Web.UI.WebControls;

using iLabs.Core;
using iLabs.DataTypes;
using iLabs.ServiceBroker.Administration;
using iLabs.ServiceBroker.Authorization;

namespace iLabs.ServiceBroker.iLabSB
{
    public partial class ReportBug : System.Web.UI.Page
    {
        AuthorizationWrapperClass wrapper = new AuthorizationWrapperClass();
        int userID = -1;
        User currentUser;
        Exception excep;

        //---------------------------------------------------------------------------------------//

        protected void Page_Init(object sender, EventArgs e)
        {
            Master.HeaderTitle = this.Title;
            this.Title = Master.PageTitle + this.Title;
        }

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            userID = -1;
            currentUser = new User();
            if ((Session != null) && (Session["UserID"] != null))
            {
                userID = Convert.ToInt32(Session["UserID"]);
                currentUser = wrapper.GetUsersWrapper(new int[] { userID })[0];
            }

            if (!IsPostBack)
            {
                if (Request.Params["ex"] != null)
                    excep = Server.GetLastError();

                String optList = ConfigurationManager.AppSettings["bugReportOptions"];
                if ((optList != null) && (optList.Length > 0))
                {
                    char[] delimiter = { ',' };
                    String[] options = optList.Split(delimiter, 100);
                    for (int i = 0; i < options.Length; i++)
                    {
                        //ddlArea.Items.Add(new ListItem(options[i],i.ToString()));
                        ddlBugType.Items.Add(options[i]);
                    }
                    if (options.Length > 0)
                    {
                        ddlBugType.Items[0].Selected = false;
                    }
                }
                // TO DO: this is not what I would expect
                IntTag[] ls = wrapper.GetProcessAgentTagsByTypeWrapper(ProcessAgentType.LAB_SERVER);

                ddlBugType.Items.Add("Content - need to change");
                foreach (IntTag l in ls)
                {
                    ddlBugType.Items.Add(new ListItem(l.tag, l.id.ToString()));
                }
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void btnReportBug_Click(object sender, System.EventArgs e)
        {
            if ((userID == -1) && (txtEmail.Text.Length == 0))
            {
                lblResponse.Text = "<div class=errormessage><p>Please enter an emailaddress, so we can respond to your report.</p></div>";
                lblResponse.Visible = true;
            }
            else if (ddlBugType.SelectedItem.Text.CompareTo("") == 0)
            {
                lblResponse.Text = "<div class=errormessage><p>Please select a general problem catagory.</p></div>";
                lblResponse.Visible = true;
            }
            else if (txtDescription.Text == "")
            {
                lblResponse.Text = "<div class=errormessage><p>Please enter a description of the problem!</p></div>";
                lblResponse.Visible = true;
            }
            else
            {
                string userEmail = null;
                if ((currentUser.email != null) && (currentUser.email != ""))
                {
                    userEmail = currentUser.email;
                }
                else if ((txtEmail != null) && (txtEmail.Text != null) && (txtEmail.Text != ""))
                {
                    userEmail = txtEmail.Text;
                }

                //
                // Generate email
                //
                string bugType = ddlBugType.SelectedItem.Text;
                string subject = "[iLabs] " + Server.MachineName + " Bug Report: " + bugType;

                StringBuilder sb = new StringBuilder();
                if (userID == -1)
                {
                    sb.Append("User Not Logged In:\n\r");
                    sb.Append("Username: " + txtUsername.Text + "\n\r");
                    sb.Append("Email:  " + txtEmail.Text + "\n\r");
                    if (Session["GroupName"] != null)
                        sb.Append("Group: " + Session["GroupName"].ToString() + "\n\r");
                }
                else
                {
                    sb.Append(currentUser.firstName + " " + currentUser.lastName + "\n\r");
                    sb.Append("Username: " + currentUser.userName + "\n\r");
                    sb.Append("Email:  " + currentUser.email + "\n\r");
                    if (Session["GroupName"] != null)
                        sb.Append("Group: " + Session["GroupName"].ToString() + "\n\r");
                }
                sb.Append("\n\r");

                sb.Append("reports the following bug '" + bugType + "':  \n\r\n\r");
                sb.Append(txtDescription.Text);
                sb.Append("\n\r\n\r");
                sb.Append("Additional Information:\n\r");
                sb.Append("User Browser: " + Request.Browser.Type + "\n\r");
                sb.Append("User Browser Agent: " + Request.UserAgent + "\n\r");
                sb.Append("User Platform: " + Request.Browser.Platform + "\n\r");
                sb.Append("URL used to access page: " + Request.Url + "\n\r");
                sb.Append("Machine Name: " + Server.MachineName + "\n\r");

                sb.Append("Server Type: " + Server.GetType() + "\n\r");

                if (excep != null)
                {
                    sb.Append("\n\rException Thrown:\n\r");
                    sb.Append(excep.Message + "\n\r\n\r");
                    sb.Append(excep.StackTrace);
                    Server.ClearError();
                }

                string body = sb.ToString();
                string from = userEmail;
                string to = ConfigurationManager.AppSettings["bugReportMailAddress"];
                MailMessage mailMessage = new MailMessage(from, to, subject, body);
                SmtpClient smtpClient = new SmtpClient(Consts.STR_LocalhostIP);

                try
                {
                    smtpClient.Send(mailMessage);

                    if (userEmail != null)
                    {
                        to = userEmail;
                        from = ConfigurationManager.AppSettings["bugReportMailAddress"];
                        body = "Thank you for taking the time to report the following bug:\n\r";
                        body += txtDescription.Text;
                        mailMessage = new MailMessage(from, to, subject, body);
                        smtpClient.Send(mailMessage);
                    }
                    lblResponse.Text = "<div class=errormessage><p>Thank-you! Your bug report has been submitted. An administrator will contact you within 24-48 hours.</p></div>";
                    lblResponse.Visible = true;
                }
                catch (Exception ex)
                {
                    lblResponse.Text = "<div class=errormessage><p>Error sending your bug report, please email "
                        + ConfigurationManager.AppSettings["bugReportMailAddress"] + ". " + ex.Message + "</p></div>";
                    lblResponse.Visible = true;
                }
            }
        }

    }
}
