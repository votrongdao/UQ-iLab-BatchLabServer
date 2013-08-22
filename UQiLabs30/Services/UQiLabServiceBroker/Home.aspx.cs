using System;
using System.Collections;
using System.Configuration;
using System.Net;
using System.Web;

using iLabs.ServiceBroker.Administration;
using iLabs.ServiceBroker.Authorization;
using Library.Lab;

namespace iLabs.ServiceBroker.iLabSB
{
    public partial class Home : System.Web.UI.Page
    {
        #region Constants

        private const string STRLOG_ClassName = "Home";
        /*
         * String constants for logfile messages
         */
        private const string STRLOG_UserHost_arg2 = "UserHostAddress: {0}  UserHostName: {1}";
        private const string STRLOG_CannotResolveToHostName = " Cannot resolve to HostName!";

        #endregion

        //---------------------------------------------------------------------------------------//

        protected void Page_Init(object sender, EventArgs e)
        {
            const string STRLOG_MethodName = "Page_Init";
            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            Master.HeaderTitle = this.Title;
            this.Title = Master.PageTitle + this.Title;

            /*
             * Log the caller's IP address and hostname
             */
            HttpRequest httpRequest = this.Request;
            string hostName;
            try
            {
                IPHostEntry ipHostEntry = Dns.GetHostEntry(httpRequest.UserHostAddress);
                hostName = ipHostEntry.HostName;
            }
            catch
            {
                hostName = STRLOG_CannotResolveToHostName;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName,
                String.Format(STRLOG_UserHost_arg2, httpRequest.UserHostAddress, hostName));
        }

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session[Consts.STRSSN_UserID] == null)
            {
                bool requireSSL = Convert.ToBoolean(ConfigurationManager.AppSettings[Consts.STRCFG_HaveSSL]);
                string Url;
                if ((requireSSL) && (!Request.IsSecureConnection))
                {
                    Url = Global.FormatSecureURL(Request, Consts.STRURL_Home);
                    Response.Redirect(Url);
                }
                else if ((!requireSSL) && (Request.IsSecureConnection))
                {
                    Url = Global.FormatRegularURL(Request, Consts.STRURL_Home);
                    Response.Redirect(Url);
                }
            }

            //
            // Get system messages and display
            //
            AuthorizationWrapperClass wrapper = new AuthorizationWrapperClass();
            SystemMessage[] messages = wrapper.GetSystemMessagesWrapper(SystemMessage.SYSTEM, 0, 0, 10);
            if (messages == null || messages.Length == 0)
            {
                lblNoMessages.Visible = true;
            }
            else
            {
                lblNoMessages.Visible = false;

                ArrayList messagesList = new ArrayList();
                foreach (SystemMessage message in messages)
                {
                    messagesList.Add(message);
                }

                messagesList.Sort(new DateComparer());
                messagesList.Reverse();

                repSystemMessage.DataSource = messagesList;
                repSystemMessage.DataBind();
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            string redirectUrl = null;

            string haveSSL = ConfigurationManager.AppSettings[Consts.STRCFG_HaveSSL];
            bool requireSSL = false;
            try
            {
                requireSSL = Convert.ToBoolean(haveSSL);
            }
            catch
            {
            }
            if (requireSSL == true)
            {
                redirectUrl = Global.FormatSecureURL(Request, Consts.STRURL_Register);
            }
            else
            {
                redirectUrl = Global.FormatRegularURL(Request, Consts.STRURL_Register);
            }

            Response.Redirect(redirectUrl);
        }
    }
}
