using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Text;

using iLabs.ServiceBroker.Administration;
using iLabs.ServiceBroker.Authentication;
using iLabs.ServiceBroker.Authorization;
using iLabs.UtilLib;

namespace iLabs.ServiceBroker.iLabSB
{
    public partial class MyAccount : System.Web.UI.Page
    {
        string supportMailAddress = ConfigurationManager.AppSettings["supportMailAddress"];
        string registrationMailAddress = ConfigurationManager.AppSettings["registrationMailAddress"];
        string serviceBrokerName;

        //---------------------------------------------------------------------------------------//

        protected void Page_Init(object sender, EventArgs e)
        {
            Master.HeaderTitle = this.Title;
            this.Title = Master.PageTitle + this.Title;

            this.serviceBrokerName = Master.Title;
        }

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            lblResponse.Visible = false;

            if (!IsPostBack)
            {
                //
                // Populate textboxes with the user's information
                //
                AuthorizationWrapperClass wrapper = new AuthorizationWrapperClass();
                User sessionUser = new User();
                int userID = Convert.ToInt32(Session[Consts.STRSSN_UserID]);
                sessionUser = wrapper.GetUsersWrapper(new int[] { userID })[0];

                txtUsername.Enabled = false;
                txtUsername.Text = sessionUser.userName;
                txtFirstName.Text = sessionUser.firstName;
                txtLastName.Text = sessionUser.lastName;
                txtEmail.Text = sessionUser.email;
                txtNewPassword.Text = "";
                txtConfirmPassword.Text = "";

                // To list all the groups a user belongs to
                int[] groupIDs = wrapper.ListGroupsForAgentWrapper(userID);

                //since we already have the groups a user has access
                // if we use wrapper here, it will deny authentication
                Group[] gps = AdministrativeAPI.GetGroups(groupIDs);
                ArrayList nonRequestGroups = new ArrayList();
                ArrayList requestGroups = new ArrayList();

                foreach (Group g in gps)
                {
                    if (g.groupName.EndsWith("request"))
                        requestGroups.Add(g);
                    else
                        if (!g.groupName.Equals("NewUserGroup"))
                            nonRequestGroups.Add(g);
                }

                //
                // List Groups for which the user is a member
                //
                StringBuilder sb = new StringBuilder();
                if ((nonRequestGroups != null) && (nonRequestGroups.Count > 0))
                {
                    for (int i = 0; i < nonRequestGroups.Count; i++)
                    {
                        sb.Append(((Group)nonRequestGroups[i]).groupName);
                        if (i < nonRequestGroups.Count - 1)
                        {
                            sb.Append("<br />");
                        }
                    }
                }
                else
                {
                    sb.Append("No group");
                }
                lblGroups.Text = sb.ToString();

                //
                // List Groups for which the user has requested membership
                //
                sb = new StringBuilder();
                if ((requestGroups != null) && (requestGroups.Count > 0))
                {
                    for (int i = 0; i < requestGroups.Count; i++)
                    {
                        int origGroupID = AdministrativeAPI.GetAssociatedGroupID(((Group)requestGroups[i]).groupID);
                        string origGroupName = AdministrativeAPI.GetGroups(new int[] { origGroupID })[0].groupName;

                        sb.Append(origGroupName);
                        if (i < requestGroups.Count - 1)
                        {
                            sb.Append("<br />");
                        }
                    }
                }
                else
                {
                    sb.Append("No group");
                }
                lblRequestGroups.Text = sb.ToString();
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void btnSave_Click(object sender, EventArgs e)
        {
            AuthorizationWrapperClass wrapper = new AuthorizationWrapperClass();

            if (txtNewPassword.Text.CompareTo(txtConfirmPassword.Text) != 0)
            {
                lblResponse.Text = Utilities.FormatErrorMessage("Password fields don't match. Try again!");
                lblResponse.Visible = true;
                txtNewPassword.Text = null;
                txtConfirmPassword.Text = null;
            }
            else
            {
                //if a field is left blank, it is not updated
                try
                {
                    User userInfo = wrapper.GetUsersWrapper(new int[] { Convert.ToInt32(Session["UserID"]) })[0];

                    if (txtUsername.Text.Trim() == "")
                    {
                        txtUsername.Text = userInfo.userName;
                    }
                    if (txtFirstName.Text.Trim() == "")
                    {
                        txtFirstName.Text = userInfo.firstName;
                    }
                    if (txtLastName.Text.Trim() == "")
                    {
                        txtLastName.Text = userInfo.lastName;
                    }
                    if (txtEmail.Text.Trim() == "")
                    {
                        txtEmail.Text = userInfo.email;
                    }

                    if (userInfo.reason == null)
                        userInfo.reason = "";
                    if (userInfo.affiliation == null)
                        userInfo.affiliation = "";
                    if (userInfo.xmlExtension == null)
                        userInfo.xmlExtension = "";

                    wrapper.ModifyUserWrapper(userInfo.userID, txtUsername.Text, txtUsername.Text, AuthenticationType.NativeAuthentication, txtFirstName.Text, txtLastName.Text, txtEmail.Text, userInfo.affiliation, userInfo.reason, userInfo.xmlExtension, userInfo.lockAccount);
                    lblResponse.Text = Utilities.FormatConfirmationMessage("User \"" + txtUsername.Text + "\" information has been updated.");
                    lblResponse.Visible = true;
                    if (txtNewPassword.Text != "")
                    {
                        wrapper.SetNativePasswordWrapper(Convert.ToInt32(Session["UserID"]), txtNewPassword.Text);
                    }

                    if (txtUsername.Text.CompareTo(Session["UserName"].ToString()) != 0)
                        Session["UserName"] = txtUsername.Text;

                    // Send a confirmation message to the user
                    string email;
                    if (txtEmail.Text.Trim() == "")
                    {
                        // use old email if it wasn't changed, new if it was
                        email = userInfo.email;
                    }
                    else
                    {
                        email = txtEmail.Text.Trim();
                    }

                    //
                    // Email account update confirmation
                    //
                    string subject = "[" + this.serviceBrokerName + "] Account Update Confirmation";

                    StringWriter message = new StringWriter();
                    message.WriteLine("Your ServiceBroker account has been updated to the following:");
                    message.WriteLine("------------------------------------------------------------");
                    message.WriteLine();
                    message.WriteLine("User Name:     " + txtUsername.Text);
                    message.WriteLine("First Name:    " + txtFirstName.Text);
                    message.WriteLine("Last Name:     " + txtLastName.Text);
                    message.WriteLine("Email Address: " + txtEmail.Text);
                    message.WriteLine();
                    message.WriteLine("For security reasons, your password has not been included in this message.");

                    string body = message.ToString();
                    string from = registrationMailAddress;
                    string to = email;
                    MailMessage mailMessage = new MailMessage(from, to, subject, body);
                    SmtpClient smtpClient = new SmtpClient(Consts.STR_LocalhostIP);

                    try
                    {
                        smtpClient.Send(mailMessage);
                    }
                    catch
                    {
                        // if the confirmation message fails, c'est la vie...
                    }
                }
                catch (Exception ex)
                {
                    string msg = "Error updating account (" + ex.Message + ". " + ex.GetBaseException() + "). Contact " + supportMailAddress + ".";
                    lblResponse.Text = Utilities.FormatErrorMessage(msg);
                    lblResponse.Visible = true;
                }
            }
        }

    }
}
