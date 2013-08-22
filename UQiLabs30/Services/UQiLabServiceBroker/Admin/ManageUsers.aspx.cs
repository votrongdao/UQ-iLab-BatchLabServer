using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Hosting;
using Library.Lab;
using Library.UQiLabServiceBroker;
using iLabs.ServiceBroker.Administration;
using iLabs.ServiceBroker.Authentication;
using iLabs.ServiceBroker.Authorization;

namespace iLabs.ServiceBroker.iLabSB.Admin
{
    public partial class ManageUsers : System.Web.UI.Page
    {
        #region Constants

        private const string STRLOG_ClassName = "ManageUsers";
        /*
         * String constants for logfile messages
         */
        private const string STRLOG_Filename_arg = "Filename: {0}";
        private const string STRLOG_FileUploadedSuccessfully = "File uploaded successfully.";
        private const string STRLOG_FileParsedSuccessfully = "File parsed successfully.";
        private const string STRLOG_SendingEmail_arg4 = "Sending email - To: '{0}  Cc: '{1}'  From: '{2}'  Subject: '{3}'";
        private const string STRLOG_UserAdded_arg3 = "User added - {0}:  Username: {1}  Password: {2}";
        private const string STRLOG_UsersAddedSuccessfully_arg2 = "Users added: {0} out of {1}";
        /*
         * String constants for exception messages
         */
        private const string STRERR_FileNotSpecified = "File not specified!";
        private const string STRERR_FileMustBeCsvDocument = "File must be a comma-seperated value (*.csv) document!";
        private const string STRERR_UserAlreadyExists_arg = "User ({0}) already exists!";
        private const string STRERR_AffiliationNotSelected = "An affiliation has not been selected.";
        private const string STRERR_GroupNotSelected = "A group has not been selected.";
        private const string STRERR_FailedToSetPasswordForUser_arg = "Failed to set password for user: {0}";
        private const string STRERR_FailedToAddUserToGroup_arg2 = "Failed to add user to group - Username: {0}  Group: {1}";
        /*
         * String constants
         */
        private const string STR_Csv = ".csv";
        private const string STR_MakeSelection = "-- Make selection --";
        private const string STR_ClasslistAddedUser = "User added by class list.";

        #endregion

        #region Variables

        private string serviceBrokerName;
        private string serviceBrokerUrl;
        private string registrationEmail;
        private AuthorizationWrapperClass wrapper;

        #endregion

        //---------------------------------------------------------------------------------------//

        protected void Page_Init(object sender, EventArgs e)
        {
            Master.HeaderTitle = this.Title;
            this.Title = Master.PageTitle + this.Title;

            this.serviceBrokerName = Master.Title;
            this.serviceBrokerUrl = Master.ServiceBrokerUrl;
            this.registrationEmail = Master.RegistrationEmail;
            this.wrapper = new AuthorizationWrapperClass();
        }

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            /*
             * Check that the user has admin privileges
             */
            object adminState = Session[Consts.STRSSN_IsAdmin];
            bool isAdmin = ((adminState != null) && Convert.ToBoolean(adminState));
            if (isAdmin == false)
            {
                Response.Redirect("../Home.aspx");
            }

            /*
             * User has admin privileges to access this page
             */
            if (!IsPostBack)
            {
                PopulatePageControls();
            }

            /*
             * Hide message box
             */
            this.ShowMessageNormal(string.Empty);
        }

        //---------------------------------------------------------------------------------------//

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            const string STRLOG_MethodName = "btnUpload_Click";
            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            try
            {
                if (FileUpload1.HasFile == false)
                {
                    throw new Exception(STRERR_FileNotSpecified);
                }

                string fileExtension = Path.GetExtension(FileUpload1.FileName).ToLower();
                if (fileExtension.Equals(STR_Csv) == false)
                {
                    throw new Exception(STRERR_FileMustBeCsvDocument);
                }

                /*
                 * Save uploaded file content to file on server
                 */
                HttpPostedFile postedFile = FileUpload1.PostedFile;
                int fileLength = postedFile.ContentLength;
                string filename = Path.GetFileName(postedFile.FileName);
                filename = Path.Combine(GetClientFilesPath(), filename);
                postedFile.SaveAs(filename);

                /*
                 * Update the file list
                 */
                ddlFiles.Items.Add(postedFile.FileName);
                btnFilesRemove.Enabled = true;
                btnParseFile.Enabled = true;

                ShowMessageNormal(STRLOG_FileUploadedSuccessfully);

                Logfile.Write(String.Format(STRLOG_Filename_arg, filename));
            }
            catch (Exception ex)
            {
                ShowMessageError(ex.Message);
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnFilesRemove_Click(object sender, EventArgs e)
        {
            /*
             * Delete the client file
             */
            string filename = Path.GetFileName(ddlFiles.SelectedValue);
            filename = Path.Combine(GetClientFilesPath(), filename);
            if (File.Exists(filename) == true)
            {
                File.Delete(filename);
            }

            ddlFiles.Items.Remove(ddlFiles.SelectedItem);

            /*
             * Disable the remove button if the dropdown list is empty
             */
            if (ddlFiles.Items.Count == 0)
            {
                btnFilesRemove.Enabled = false;
                btnParseFile.Enabled = false;
                btnAddUsers.Enabled = false;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        protected void ddlFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnAddUsers.Enabled = false;
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnParseFile_Click(object sender, EventArgs e)
        {
            const string STRLOG_MethodName = "btnParseFile_Click";
            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            try
            {
                if (ddlFiles.SelectedValue.Trim().Length == 0)
                {
                    throw new Exception(STRERR_FileNotSpecified);
                }

                string filename = Path.GetFileName(ddlFiles.SelectedValue);
                filename = Path.Combine(GetClientFilesPath(), filename);

                /*
                 * Parse the file for new user information
                 */
                ManageUserInfo manageUserInfo = new ManageUserInfo();
                UserInfo[] userInfos = manageUserInfo.ParseCsvUserFile(filename);

                /*
                 * Check if any user already exist
                 */
                for (int i = 0; i < userInfos.Length; i++)
                {
                    if (wrapper.GetUserIDWrapper(userInfos[i].studentID.ToString()) > 0)
                    {
                        throw new Exception(
                            String.Format(STRERR_UserAlreadyExists_arg, userInfos[i].studentID));
                    }
                }

                /*
                 * If we get to here, all is ok and users can be added
                 */
                btnAddUsers.Enabled = true;
                this.ShowMessageNormal(STRLOG_FileParsedSuccessfully);
            }
            catch (Exception ex)
            {
                ShowMessageError(ex.Message);
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnAddUsers_Click(object sender, EventArgs e)
        {
            const string STRLOG_MethodName = "btnParseFile_Click";
            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            try
            {
                string filename = Path.GetFileName(ddlFiles.SelectedValue);
                filename = Path.Combine(GetClientFilesPath(), filename);

                /*
                 * Parse the file for new user information
                 */
                ManageUserInfo manageUserInfo = new ManageUserInfo();
                UserInfo[] userInfos = manageUserInfo.ParseCsvUserFile(filename);
                SmtpClient smtpClient = new SmtpClient(Consts.STR_LocalhostIP);

                /*
                 * Check if the user already exists before adding the user
                 */
                int userCount;
                for (userCount = 0; userCount < userInfos.Length; userCount++)
                {
                    UserInfo userInfo = userInfos[userCount];
                    string username = userInfo.studentID.ToString();
                    if (wrapper.GetUserIDWrapper(username) > 0)
                    {
                        throw new Exception(
                            String.Format(STRERR_UserAlreadyExists_arg, username));
                    }

                    /*
                     * Get the affiliation
                     */
                    if (ddlAffiliations.SelectedIndex <= 0)
                    {
                        throw new Exception(STRERR_AffiliationNotSelected);
                    }
                    string affiliation = ddlAffiliations.SelectedValue;

                    /*
                     * Get the group Id
                     */
                    if (ddlGroupNames.SelectedIndex <= 0)
                    {
                        throw new Exception(STRERR_GroupNotSelected);
                    }
                    string groupName = ddlGroupNames.SelectedValue;
                    int groupId = wrapper.GetGroupIDWrapper(groupName);

                    /*
                     * Create the user
                     */
                    int userId = wrapper.AddUserWrapper(username, username, AuthenticationType.NativeAuthentication,
                        userInfo.givenNames, userInfo.familyName, userInfo.emailAddress, affiliation, STR_ClasslistAddedUser,
                        String.Empty, groupId, false);

                    /*
                     * Generate a user password - can only change password if superuser
                     */
                    string password = manageUserInfo.GeneratePassword();
                    if (Session[Consts.STRSSN_GroupName].Equals(ServiceBroker.Administration.Group.SUPERUSER))
                    {
                        if (wrapper.SetNativePasswordWrapper(userId, password) == false)
                        {
                            throw new Exception(
                                String.Format(STRERR_FailedToSetPasswordForUser_arg, username));
                        }
                    }

                    /*
                     * Give the user group membership - this throws an exception but group membership is given - don't know why???
                     */
                    try
                    {
                        if (wrapper.AddMemberToGroupWrapper(userId, groupId) == false)
                        {
                            throw new Exception(
                                String.Format(STRERR_FailedToAddUserToGroup_arg2, username, groupName));
                        }
                    }
                    catch (Exception)
                    {
                    }

                    /*
                     * Send an email to the new user and CC 
                     */
                    string to = userInfo.emailAddress;
                    string cc = registrationEmail;
                    string from = registrationEmail;
                    string subject = String.Format("[{0}] New iLab ServiceBroker Account", this.serviceBrokerName);

                    /*
                     * Create the message
                     */
                    StringWriter message = new StringWriter();
                    message.WriteLine();
                    message.WriteLine("An iLab ServiceBroker account has been created with the following details:");
                    message.WriteLine();
                    message.WriteLine(String.Format("ServiceBroker: {0}", serviceBrokerUrl));
                    message.WriteLine();
                    message.WriteLine(String.Format("Name:     {0} {1}", userInfo.givenNames, userInfo.familyName));
                    message.WriteLine(String.Format("Email:    {0}", userInfo.emailAddress));
                    message.WriteLine(String.Format("Username: {0}", username));
                    message.WriteLine(String.Format("Password: {0}", password));
                    message.WriteLine(String.Format("Group:    {0}", groupName));
                    message.WriteLine();
                    message.WriteLine("When you login to the ServiceBroker for the first time, change your password. This can be done from the \"My Account\" menu option.");
                    message.WriteLine();

                    Logfile.Write(
                        String.Format(STRLOG_SendingEmail_arg4, to, cc, from, subject));
                    try
                    {
                        MailMessage mailMessage = new MailMessage(from, to, subject, message.ToString());
                        mailMessage.CC.Add(cc);
                        smtpClient.Send(mailMessage);
                    }
                    catch (Exception ex)
                    {
                        Logfile.WriteError(ex.Message);
                    }

                    /*
                     * User added successfuly
                     */
                    Logfile.Write(
                        String.Format(STRLOG_UserAdded_arg3, userCount, username, password));
                }

                /*
                 * If we get to here, some or all users have been added
                 */
                btnAddUsers.Enabled = true;
                this.ShowMessageNormal(
                    String.Format(STRLOG_UsersAddedSuccessfully_arg2, userCount, userInfos.Length));
            }
            catch (Exception ex)
            {
                ShowMessageError(ex.Message);
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

        }

        //=================================================================================================//

        private void ClearPageControls()
        {
            /*
             * Clear file list
             */
            while (ddlFiles.Items.Count > 0)
            {
                /*
                 * Delete the uploaded client file
                 */
                string filename = Path.GetFileName(ddlFiles.Items[0].Text);
                filename = Path.Combine(GetClientFilesPath(), filename);
                if (File.Exists(filename) == true)
                {
                    File.Delete(filename);
                }

                ddlFiles.Items.Remove(ddlFiles.Items[0]);
            }

            /*
             * Clear affiliation options
             */
            ddlAffiliations.Items.Clear();

            /*
             * Clear groups
             */
            ddlGroupNames.Items.Clear();


            btnFilesRemove.Enabled = false;
            btnParseFile.Enabled = false;
            btnAddUsers.Enabled = false;
        }

        //-------------------------------------------------------------------------------------------------//

        private void PopulatePageControls()
        {
            /*
             * Populate dropdown lists, etc. and select default selection, etc.
             */
            ClearPageControls();
            UpdatePageControls();
        }

        //-------------------------------------------------------------------------------------------------//

        private void UpdatePageControls()
        {
            try
            {
                /*
                 * Populate the affiliations dropdown list
                 */
                string affiliations = Utilities.GetAppSetting(Consts.STRCFG_AffiliationOptions);
                if (affiliations != null)
                {
                    string[] affiliationsSplit = affiliations.Split(new char[] { Consts.CHR_CsvSplitter });
                    for (int i = 0; i < affiliationsSplit.Length; i++)
                    {
                        ddlAffiliations.Items.Add(affiliationsSplit[i].Trim());
                    }
                    if (affiliationsSplit.Length > 0)
                    {
                        ddlAffiliations.Items[0].Selected = false;
                    }
                }

                /*
                 * Populate the groups dropdownlist
                 */
                ddlGroupNames.Items.Add(STR_MakeSelection);
                int[] groupIds = wrapper.ListGroupIDsWrapper();
                Group[] groups = wrapper.GetGroupsWrapper(groupIds);
                for (int i = 0; i < groups.Length; i++)
                {
                    Group group = groups[i];
                    if (group.groupType.Equals(GroupType.REGULAR))
                    {
                        /*
                         * Don't want special regular groups
                         */
                        if (group.groupName.Equals(Group.NEWUSERGROUP) ||
                            group.groupName.Equals(Group.ORPHANEDGROUP) ||
                            group.groupName.Equals(Group.ROOT) ||
                            group.groupName.Equals(Group.SUPERUSER) ||
                            group.groupName.Equals(Group.UNKNOWN))
                        {
                            continue;
                        }

                        /*
                         * Found a regular group that is not a special group
                         */
                        ddlGroupNames.Items.Add(groups[i].groupName);
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private string GetClientFilesPath()
        {
            /*
             * Set the filepath for the client files 
             */
            string rootFilePath = HostingEnvironment.ApplicationPhysicalPath;
            string clientFilesPath = Utilities.GetAppSetting(Consts.STRCFG_ClientFilesPath);
            clientFilesPath = Path.Combine(rootFilePath, clientFilesPath);

            /*
             * The folder may not exist so may need to create it
             */
            Directory.CreateDirectory(clientFilesPath);

            return clientFilesPath;
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

    }
}
