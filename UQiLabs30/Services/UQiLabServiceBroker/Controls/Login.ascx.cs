using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using iLabs.ServiceBroker.Administration;
using iLabs.ServiceBroker.Authentication;
using iLabs.ServiceBroker.Authorization;

namespace iLabs.ServiceBroker.iLabSB.Controls
{
    public partial class Login : System.Web.UI.UserControl
    {
        private const string STR_PreErrorMessage = "<div class=errormessage><p>";
        private const string STR_PostErrorMessage = "</p></div>";
        private const string STR_UsernameIsRequired = "Username is required.";
        private const string STR_PasswordIsRequired = "Password is required.";
        private const string STR_AccountIsLocked = "Account is locked - Email ";
        private const string STR_PasswordIsIncorrect = "Login failed - Password is incorrect.";
        private const string STR_UsernameDoesNotExist = "Login failed - Username does not exist.";

        AuthorizationWrapperClass wrapper = new AuthorizationWrapperClass();
        string supportMailAddress = ConfigurationManager.AppSettings["supportMailAddress"];

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        //---------------------------------------------------------------------------------------//

        protected void btnLogIn_Click(object sender, EventArgs e)
        {
            //
            // Check that a username has been entered
            //
            string username = txtUsername.Text.Trim();
            if (username.Length == 0)
            {
                lblLoginErrorMessage.Text = STR_PreErrorMessage + STR_UsernameIsRequired + STR_PostErrorMessage;
                lblLoginErrorMessage.Visible = true;
                return;
            }

            //
            // Check that a password has been entered
            //
            string password = txtPassword.Text.Trim();
            if (password.Length == 0)
            {
                lblLoginErrorMessage.Text = STR_PreErrorMessage + STR_PasswordIsRequired + STR_PostErrorMessage;
                lblLoginErrorMessage.Visible = true;
                return;
            }

            //
            // Get user ID, returns -1 if the user does not exist
            //
            int userID = wrapper.GetUserIDWrapper(txtUsername.Text);
            if (userID > 0)
            {
                User user = wrapper.GetUsersWrapper(new int[] { userID })[0];

                //
                // Check if account is locked
                //
                if (userID != -1 && user.lockAccount == true)
                {
                    lblLoginErrorMessage.Text = STR_PreErrorMessage + STR_AccountIsLocked + supportMailAddress + STR_PostErrorMessage;
                    lblLoginErrorMessage.Visible = true;
                    return;
                }

                //
                // Check password
                //
                if (AuthenticationAPI.Authenticate(userID, txtPassword.Text) == true)
                {
                    FormsAuthentication.SetAuthCookie(txtUsername.Text, false);
                    Session[Consts.STRSSN_UserID] = userID;
                    Session[Consts.STRSSN_UserName] = user.userName;

                    int tzOffset = Convert.ToInt32(Request.Params["userTZ"]);
                    Session[Consts.STRSSN_UserTZ] = Request.Params["userTZ"];
                    Session[Consts.STRSSN_SessionID] = AdministrativeAPI.InsertUserSession(userID, 0, tzOffset, Session.SessionID.ToString()).ToString();

                    string cookieName = ConfigurationManager.AppSettings[Consts.STRCFG_ISBAuthCookieName];
                    string cookieValue = Session[Consts.STRSSN_SessionID].ToString();
                    HttpCookie cookie = new HttpCookie(cookieName, cookieValue);
                    Response.AppendCookie(cookie);

                    Response.Redirect(Global.FormatRegularURL(Request, Consts.STRURL_MyGroups));
                }
                else
                {
                    lblLoginErrorMessage.Text = STR_PreErrorMessage + STR_PasswordIsIncorrect + STR_PostErrorMessage;
                    lblLoginErrorMessage.Visible = true;
                }
            }
            else
            {
                lblLoginErrorMessage.Text = STR_PreErrorMessage + STR_UsernameDoesNotExist + STR_PostErrorMessage;
                lblLoginErrorMessage.Visible = true;
            }
        }

    }
}