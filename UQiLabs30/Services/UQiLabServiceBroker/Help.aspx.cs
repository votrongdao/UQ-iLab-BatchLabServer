using System;
using System.Configuration;
using System.Net.Mail;
using System.Text;
using System.Web.UI.WebControls;

using iLabs.Core;
using iLabs.ServiceBroker.Administration;
using iLabs.ServiceBroker.Authorization;
using iLabs.UtilLib;

namespace iLabs.ServiceBroker.iLabSB
{
    public partial class Help : System.Web.UI.Page
    {
        string bugReportMailAddress = ConfigurationManager.AppSettings["bugReportMailAddress"];
        AuthorizationWrapperClass wrapper = new AuthorizationWrapperClass();

        int userID = -1;

        User currentUser;

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
                int[] lsIDs = AdministrativeAPI.ListLabServerIDs();
                ProcessAgentInfo[] ls = wrapper.GetProcessAgentInfosWrapper(lsIDs);
                //ddlWhichLab.Items.Add("System-wide error");

                String optList = ConfigurationManager.AppSettings["helpOptions"];
                if ((optList != null) && (optList.Length > 0))
                {
                    char[] delimiter = { ',' };
                    String[] options = optList.Split(delimiter, 100);
                    for (int i = 0; i < options.Length; i++)
                    {
                        //ddlHelpType.Items.Add(new ListItem(options[i],i.ToString()));						
                        ddlHelpType.Items.Add(options[i]);
                    }
                    if (options.Length > 0)
                    {
                        ddlHelpType.Items[0].Selected = false;
                    }
                }

                foreach (ProcessAgentInfo l in ls)
                {
                    ddlHelpType.Items.Add(new ListItem(l.agentName));
                }
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void btnRequestHelp_Click(object sender, System.EventArgs e)
        {
            if ((userID == -1) && (txtEmail.Text.Length == 0))
            {
                lblResponse.Text = "<div class=errormessage><p>Please enter an email address, so that we can respond to you.</p></div>";
                lblResponse.Visible = true;
            }

            else if (ddlHelpType.SelectedItem.Text.CompareTo("") == 0)
            {
                lblResponse.Text = "<div class=errormessage><p>Please select the type of help you need.</p></div>";
                lblResponse.Visible = true;
            }
            else if (txtDescription.Text == "")
            {
                lblResponse.Text = "<div class=errormessage><p>Enter a description of the problem!</p></div>";
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

                string helpType = ddlHelpType.SelectedItem.Text;
                string subject = "[iLabs] Help Request: " + helpType;

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

                sb.Append("requests help - '" + helpType + "':  \n\r\n\r");
                sb.Append(txtDescription.Text);
                sb.Append("\n\r\n\r");
                sb.Append("Additional Information:\n\r");
                sb.Append("User Browser: " + Request.Browser.Type + "\n\r");
                sb.Append("User Browser Agent: " + Request.UserAgent + "\n\r");
                sb.Append("User Platform: " + Request.Browser.Platform + "\n\r");
                sb.Append("URL used to access page: " + Request.Url + "\n\r");
                sb.Append("Machine Name: " + Server.MachineName + "\n\r");

                sb.Append("Server Type: " + Server.GetType() + "\n\r");

                string body = sb.ToString();
                string from = userEmail;
                string to = ConfigurationManager.AppSettings["supportMailAddress"]; ;
                MailMessage mailMessage = new MailMessage(from, to, subject, body);
                SmtpClient smtpClient = new SmtpClient(Consts.STR_LocalhostIP);

                try
                {
                    smtpClient.Send(mailMessage);
                    if (userEmail != null)
                    {
                        body = "Thank you for taking the time to request help from us:\n\r";
                        body += txtDescription.Text;

                        to = userEmail;
                        from = ConfigurationManager.AppSettings["supportMailAddress"];
                        mailMessage = new MailMessage(from, to, subject, body);
                        smtpClient.Send(mailMessage);
                    }
                    lblResponse.Text = "<div class=errormessage><p>Thank-you! Your help request has been submitted. An administrator will contact you within 24-48 hours.</p></div>";
                    lblResponse.Visible = true;
                }
                catch (Exception ex)
                {
                    // Report detailed SMTP Errors
                    StringBuilder smtpErrorMsg = new StringBuilder();
                    smtpErrorMsg.Append("Exception: " + ex.Message);
                    //check the InnerException
                    if (ex.InnerException != null)
                        smtpErrorMsg.Append("<br>Inner Exceptions:");
                    while (ex.InnerException != null)
                    {
                        smtpErrorMsg.Append("<br>" + ex.InnerException.Message);
                        ex = ex.InnerException;
                    }
                    lblResponse.Text = Utilities.FormatErrorMessage("Error sending your help request, please email " + ConfigurationManager.AppSettings["supportMailAddress"] + ".<br>" + smtpErrorMsg.ToString());
                    lblResponse.Visible = true;
                }
            }
        }

    }
}
