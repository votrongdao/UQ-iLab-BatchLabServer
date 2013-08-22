using System;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Net.Mail;
using System.Web.Security;

using iLabs.Core;
using iLabs.ServiceBroker.Administration;
using iLabs.ServiceBroker.Authorization;
using iLabs.UtilLib;

namespace iLabs.ServiceBroker.iLabSB
{
    public partial class LostPassword : System.Web.UI.Page
    {
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

        }

        //---------------------------------------------------------------------------------------//

        protected void btnSubmit_Click(object sender, System.EventArgs e)
        {
            string userName = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();

            string prompt = "Please enter ";
            string errorMessage = null;
            if (userName.Length == 0)
            {
                errorMessage = prompt + "Username";
            }
            else if (email.Length == 0)
            {
                errorMessage = prompt + "Email Address";
            }
            if (errorMessage != null)
            {
                lblResponse.Text = Utilities.FormatErrorMessage(errorMessage);
                lblResponse.Visible = true;
                return;
            }

            AuthorizationWrapperClass wrapper = new AuthorizationWrapperClass();
            int userID = wrapper.GetUserIDWrapper(userName);
            if (userID < 0)
            {
                // userID does not exist in the database
                lblResponse.Text = Utilities.FormatErrorMessage("This username does not exist.");
                lblResponse.Visible = true;
                return;
            }

            User[] lostPassUsers = wrapper.GetUsersWrapper(new int[] { userID });

            if (lostPassUsers[0].userID == 0)
            {
                // userID does not exist in the database
                lblResponse.Text = Utilities.FormatErrorMessage("This username does not exist.");
                lblResponse.Visible = true;
            }
            else if (email.ToLower() != wrapper.GetUsersWrapper(new int[] { userID })[0].email.ToLower())
            {
                // email does not match email record in our database
                lblResponse.Text = Utilities.FormatErrorMessage("Please use the username AND email you were registered with.");
                lblResponse.Visible = true;
            }
            else // send password to requestor's email address
            {
                //
                // Email new password to user
                //
                string subject = "[" + this.serviceBrokerName + "] Lost Password";

                StringWriter message = new StringWriter();
                message.WriteLine("Username: " + userName);
                message.WriteLine("Email:    " + email);
                message.WriteLine();
                message.WriteLine("Your old password has been reset to the following password." +
                    " For security reasons, please login and use the 'My Account' page to reset your password.");
                message.WriteLine();
                message.WriteLine("Password: " + resetPassword(userID));

                string body = message.ToString();
                string from = registrationMailAddress;
                string to = email;
                MailMessage mailMessage = new MailMessage(from, to, subject, body);
                SmtpClient smtpClient = new SmtpClient(Consts.STR_LocalhostIP);

                try
                {
                    smtpClient.Send(mailMessage);

                    // email sent message
                    lblResponse.Text = Utilities.FormatConfirmationMessage("Your request has been submitted. A new password will be created and emailed to you at the email address specified.");
                    lblResponse.Visible = true;
                }
                catch (Exception ex)
                {
                    // trouble sending request for password
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

                    lblResponse.Text = Utilities.FormatErrorMessage("Trouble sending email. Your request could not be submitted - please inform an administrator.<br>" + smtpErrorMsg);
                    lblResponse.Visible = true;
                }
            }
        }

        /* This is what happens in the following method:
            1) A random number is generated
            2) This number is hashed to create a long alphanumeric string
            3) The resulting string is truncated to 8 positions. This becomes the new
                password.
            4) The 8-position password is hashed and stored in the database.
            5) The un-hashed 8-position password is returned (& then emailed to the student.)
            When the student logs in, the un-hashed 8-position password is presented.
            It is then hashed and compared to the hashed value that has been stored in
            the database.
        */
        public string resetPassword(int userID)
        {
            // 1. generate random number
            Random rnd = new Random();
            long rndNo = rnd.Next();

            //2. hash the random number 
            string hashed = FormsAuthentication.HashPasswordForStoringInConfigFile(rndNo.ToString(), "sha1");

            //3. get any 8 characters out of this & make this the new password.
            //ends with 24 since there are 32 characters in hashed string.
            string newPassword = hashed.Substring(rnd.Next(24), 8);

            //4. hash this password and store it into the database.
            string hashedPassword = FormsAuthentication.HashPasswordForStoringInConfigFile(newPassword, "sha1");

            DbConnection myConnection = FactoryDB.GetConnection();
            try
            {

                DbCommand cmd = myConnection.CreateCommand();
                cmd.CommandText = "UPDATE Users SET Password = '" + hashedPassword + "' WHERE User_ID = " + userID.ToString();
                myConnection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                lblResponse.Text = Utilities.FormatErrorMessage("Error retrieving lost password. " + ex.GetBaseException());
            }
            finally
            {
                myConnection.Close();
            }

            //5. return password
            return newPassword;
        }

    }
}
