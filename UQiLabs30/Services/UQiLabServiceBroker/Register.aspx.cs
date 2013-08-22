using System;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Web.Security;

using iLabs.ServiceBroker.Administration;
using iLabs.ServiceBroker.Authentication;
using iLabs.ServiceBroker.Authorization;
using iLabs.ServiceBroker.Internal;
using iLabs.UtilLib;

namespace iLabs.ServiceBroker.iLabSB
{
    public partial class Register : System.Web.UI.Page
    {
        string registrationMailAddress = ConfigurationManager.AppSettings["registrationMailAddress"];
        string supportMailAddress = ConfigurationManager.AppSettings["supportMailAddress"];
        bool chooseGroup = true;
        bool useRequestGroups = true;
        string serviceBrokerName;

        //---------------------------------------------------------------------------------------//

        protected void Page_Init(object sender, EventArgs e)
        {
            Master.HeaderTitle = this.Title;
            this.Title = Master.PageTitle + this.Title;
        }

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            this.serviceBrokerName = Master.Title;

            AuthorizationWrapperClass wrapper = new AuthorizationWrapperClass();

            // Group options -- Default is to use the DropDownList with request groups, if no request group is selected
            //		the specified initialGroup will be used. Default to newUserGroup if no initialGroup
            // If useRequestGroup is set to false the dropdownList will be populated with actual groups and user will be
            //		made a member of the selected group. If defaultGroups is set the comma delimited list of groups will be used.
            // If chooseGroup is set to false the dropdown list will not be displayed and user will be assigned to the initialGroup

            if (ConfigurationManager.AppSettings["chooseGroups"] != null)
            {
                if (ConfigurationManager.AppSettings["chooseGroups"].Equals("false"))
                    chooseGroup = false;
            }
            if (ConfigurationManager.AppSettings["useRequestGroup"] != null)
            {
                if (ConfigurationManager.AppSettings["useRequestGroup"].Equals("false"))
                    useRequestGroups = false;
            }
            if (!IsPostBack)
            {
                // Set up affiliation options
                if (ConfigurationManager.AppSettings["useAffiliationDDL"].Equals("true"))
                {
                    ddlAffiliation.Visible = true;
                    txtAffiliation.Visible = false;

                    String afList = ConfigurationManager.AppSettings["affiliationOptions"];
                    char[] delimiter = { ',' };
                    String[] options = afList.Split(delimiter, 100);
                    for (int i = 0; i < options.Length; i++)
                    {
                        ddlAffiliation.Items.Add(options[i]);
                    }
                    if (options.Length > 0)
                    {
                        ddlAffiliation.Items[0].Selected = false;
                    }
                }
                else
                {
                    // Setup default affiliation
                    ddlAffiliation.Visible = false;
                    txtAffiliation.Visible = true;
                }

                if (chooseGroup)
                {

                    //ddlGroup.Items.Add("-- None --");
                    //Don' t use wrapper since it only lists a user's group
                    int[] gpIDs = wrapper.ListGroupIDsWrapper();
                    Group[] gps = AdministrativeAPI.GetGroups(gpIDs);

                    ArrayList aList = new ArrayList();
                    for (int i = 0; i < gps.Length; i++)
                    {
                        if (useRequestGroups)
                        {
                            if (gps[i].groupType.Equals(GroupType.REQUEST))
                            {
                                int origGroupID = AdministrativeAPI.GetAssociatedGroupID(((Group)gps[i]).groupID);
                                string origGroupName = AdministrativeAPI.GetGroups(new int[] { origGroupID })[0].groupName;
                                aList.Add(origGroupName);
                            }
                        }
                        else
                        {
                            if (gps[i].groupType.Equals(GroupType.REGULAR) && (gps[i].groupID >= 10))
                            {
                                aList.Add(gps[i].groupName);
                            }
                        }
                    }
                    for (int i = 0; i < aList.Count; i++)
                    {
                        ddlGroup.Items.Add(aList[i].ToString());
                    }
                }
                else
                {
                    ddlGroup.Visible = false;
                    trowRequestGroup.Visible = false;
                }
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void btnSubmit_Click(object sender, System.EventArgs e)
        {
            AuthorizationWrapperClass wrapper = new AuthorizationWrapperClass();

            string userName = txtUsername.Text.Trim();
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();
            string confirmpassword = txtConfirmPassword.Text.Trim();

            string prompt = "Please enter ";
            string errorMessage = null;
            if (userName.Length == 0)
            {
                errorMessage = prompt + "Username";
            }
            else if (firstName.Length == 0)
            {
                errorMessage = prompt + "First Name";
            }
            else if (lastName.Length == 0)
            {
                errorMessage = prompt + "Last Name";
            }
            else if (email.Length == 0)
            {
                errorMessage = prompt + "Email Address";
            }
            else if (password.Length == 0)
            {
                errorMessage = prompt + "Password";
            }
            else if (confirmpassword.Length == 0)
            {
                errorMessage = prompt + "Confirm Password";
            }
            if (errorMessage != null)
            {
                lblResponse.Text = Utilities.FormatErrorMessage(errorMessage);
                lblResponse.Visible = true;
                return;
            }

            string affiliation;
            if (ConfigurationManager.AppSettings["useAffiliationDDL"].Equals("true"))
            {
                if (ddlAffiliation.SelectedIndex < 1)
                {
                    lblResponse.Text = Utilities.FormatErrorMessage("Please select an affiliation.");
                    lblResponse.Visible = true;
                    return;
                }
                affiliation = ddlAffiliation.Items[ddlAffiliation.SelectedIndex].Value;
            }
            else
            {
                affiliation = txtAffiliation.Text.Trim();
                if (affiliation.Length == 0)
                {
                    lblResponse.Text = Utilities.FormatErrorMessage("Please enter an affiliation.");
                    lblResponse.Visible = true;
                    return;
                }
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                lblResponse.Text = Utilities.FormatErrorMessage("Password fields don't match, please enter again.");
                lblResponse.Visible = true;
                txtPassword.Text = null;
                txtConfirmPassword.Text = null;
                return;
            }

            int curUser = AdministrativeAPI.GetUserID(userName);
            if (curUser > 0)
            {
                lblResponse.Text = Utilities.FormatErrorMessage("The username you entered is already registered. Please check to see if you have a forgotten password, or choose another username.");
                lblResponse.Visible = true;
                txtPassword.Text = null;
                txtConfirmPassword.Text = null;
                return;
            }

            try
            {
                string principalString = userName;
                string authenType = AuthenticationType.NativeAuthentication;
                string reason = txtReason.Text.Trim();
                if (ConfigurationManager.AppSettings["chooseGroups"] != null)
                {
                    if (ConfigurationManager.AppSettings["chooseGroups"].Equals("false"))
                        chooseGroup = false;
                }
                int initialGroup = wrapper.GetGroupIDWrapper(Group.NEWUSERGROUP);
                int newUserGroupID = initialGroup;
                if (ConfigurationManager.AppSettings["initialGroup"] != null)
                {
                    int tmpID = wrapper.GetGroupIDWrapper(ConfigurationManager.AppSettings["initialGroup"]);
                    if (tmpID > 0)
                        initialGroup = tmpID;
                }
                if (chooseGroup)
                {
                    if (ConfigurationManager.AppSettings["useRequestGroup"] != null)
                    {
                        if (ConfigurationManager.AppSettings["useRequestGroup"].Equals("false"))
                            useRequestGroups = false;
                    }

                    if (ddlGroup.SelectedIndex > 0)

                        initialGroup = wrapper.GetGroupIDWrapper(ddlGroup.Items[ddlGroup.SelectedIndex].Text);
                }

                int userID = -1;
                try
                {
                    // adduserwrapper doesn't work here since there the user isn't logged in yet.
                    // user the admin API call directly instead
                    if ((useRequestGroups) && (initialGroup != newUserGroupID))
                    {
                        userID = AdministrativeAPI.AddUser(userName, principalString, authenType, firstName, lastName, email,
                            affiliation, reason, "", AdministrativeUtilities.GetGroupRequestGroup(initialGroup), false);
                    }
                    else
                    {
                        userID = AdministrativeAPI.AddUser(userName, principalString, authenType, firstName, lastName, email,
                            affiliation, reason, "", initialGroup, false);
                    }
                }
                catch (Exception ex)
                {
                    lblResponse.Text = Utilities.FormatErrorMessage("User could not be added. " + ex.Message + "<br>Please notify " + supportMailAddress);
                    lblResponse.Visible = true;
                    return;
                }

                if (userID != -1)
                {
                    Session["UserID"] = userID;
                    Session["UserName"] = userName;
                    AuthenticationAPI.SetNativePassword(userID, txtPassword.Text);
                    // setnativepasswordwrapper doesn't work here since there the user isn't logged in yet.
                    // user the admin API call directly instead
                    //wrapper.SetNativePasswordWrapper (userID, txtPassword.Text );

                    FormsAuthentication.SetAuthCookie(userName, false);
                    try
                    {
                        // Check for GroupItems, since the user may not be in the target group at this time
                        // We can not recusively check all groups, but will us the initial target group.
                        //int[] groupIDs = AdministrativeAPI.ListGroupsForAgentRecursively(userID);
                        Group[] groups = AdministrativeAPI.GetGroups(new int[] { initialGroup });
                        foreach (Group grp in groups)
                        {
                            if (ConfigurationManager.AppSettings[grp.groupName + "Item"] != null)
                            {
                                string docUrl = ConfigurationManager.AppSettings[grp.groupName + "Item"];

                                if (docUrl != null)
                                {
                                    addClientItems(docUrl, userID);
                                }
                            }
                        }
                    }
                    catch (Exception ge)
                    {
                        lblResponse.Text = Utilities.FormatErrorMessage(ge.Message);
                    }

                    //
                    // Email registration and CC user
                    //
                    string subject = "[" + this.serviceBrokerName + "] New User Registration";

                    StringWriter message = new StringWriter();
                    message.WriteLine();
                    message.WriteLine("Username: " + userName);
                    message.WriteLine("Name:     " + firstName + " " + lastName);
                    message.WriteLine("Email:    " + email);
                    message.WriteLine();

                    Group[] myGroups = AdministrativeAPI.GetGroups(new int[] { initialGroup });
                    if (useRequestGroups)
                    {
                        subject += " Request";
                        message.WriteLine("You have requested to be added to: " + myGroups[0].GroupName);
                        message.WriteLine();
                        message.WriteLine("Your request has been forwarded to the administrator.");
                        message.WriteLine("An email will be sent to you once your request has been processed.");
                        message.WriteLine();
                    }
                    else
                    {
                        message.WriteLine("You have been added to: " + myGroups[0].GroupName);
                    }

                    string body = message.ToString();
                    string from = registrationMailAddress;
                    string to = registrationMailAddress;
                    MailMessage mailMessage = new MailMessage(from, to, subject, body);
                    mailMessage.CC.Add(email);
                    SmtpClient smtpClient = new SmtpClient(Consts.STR_LocalhostIP);

                    try
                    {
                        smtpClient.Send(mailMessage);
                        Response.Redirect(Consts.STRURL_Home);
                    }
                    catch (Exception ex)
                    {
                        // Report detailed SMTP Errors
                        string smtpErrorMsg;
                        smtpErrorMsg = "Exception: " + ex.Message;
                        //check the InnerException
                        if (ex.InnerException != null)
                            smtpErrorMsg += "<br>Inner Exceptions:";
                        while (ex.InnerException != null)
                        {
                            smtpErrorMsg += "<br>" + ex.InnerException.Message;
                            ex = ex.InnerException;
                        }

                        string msg;
                        msg = "Your request could not be submitted. Please cut & paste this entire message, and send it to " + registrationMailAddress;
                        msg += "<br><br>" + subject + "<br>" + body;
                        msg += "<br><br>" + smtpErrorMsg;
                        lblResponse.Text = Utilities.FormatErrorMessage(msg);
                        lblResponse.Visible = true;
                    }
                }
                else
                {
                    lblResponse.Text = Utilities.FormatErrorMessage("Your ID has been taken. Please choose a different user ID.");
                    lblResponse.Visible = true;
                }
                // moved 2 statements into if block which sets user ID to the session - Karim
            }
            catch (Exception ex)
            {
                lblResponse.Text = Utilities.FormatErrorMessage("Error registering this user. Please report to an administrator at " + supportMailAddress + ".<br>" + ex.Message);
                lblResponse.Visible = true;
            }
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Parses the contents of a URL, expected contents are line delimited.
        /// Each entry must contain 3 lines of text. 
        ///		client name on this service broker
        ///		item name - must be unique for the specified client
        ///		data
        /// There may be multiple entries
        /// </summary>
        /// <param name="url"></param>
        /// <param name="userID"></param>
        private void addClientItems(String url, int userID)
        {
            WebResponse result = null;
            try
            {
                WebRequest req = WebRequest.Create(url);
                result = req.GetResponse();
                Stream ReceiveStream = result.GetResponseStream();
                StreamReader sr = new StreamReader(ReceiveStream);


                string client = null;
                string name = null;
                string data = null;

                while ((client = sr.ReadLine()) != null)
                {
                    name = sr.ReadLine();
                    data = sr.ReadLine();
                    int clientID = InternalAdminDB.GetLabClientIDFromName(client);
                    AdministrativeAPI.SaveClientItemValue(clientID, userID, name, data);
                }

            }
            catch (Exception ex)
            {
                lblResponse.Text = Utilities.FormatErrorMessage("Error registering this user. Please report to an administrator at " + supportMailAddress + ".<br>" + ex.Message);
                lblResponse.Visible = true;
            }
            finally
            {
                if (result != null)
                {
                    result.Close();
                }
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(Consts.STRURL_Home);
        }

    }
}
