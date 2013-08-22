using System;
using System.Web.Security;

using iLabs.ServiceBroker.Administration;

namespace iLabs.ServiceBroker.iLabSB
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            long sessionId = Convert.ToInt64(Session[Consts.STRSSN_SessionID]);
            AdministrativeAPI.SaveUserSessionEndTime(sessionId);
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            Response.Redirect(Consts.STRURL_Home);
        }
    }
}
