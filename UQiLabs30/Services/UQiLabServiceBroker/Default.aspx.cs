using System;
using System.Configuration;

namespace iLabs.ServiceBroker.iLabSB
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
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
                redirectUrl = Global.FormatSecureURL(Request, Consts.STRURL_Home);
            }
            else
            {
                redirectUrl = Global.FormatRegularURL(Request, Consts.STRURL_Home);
            }

            Response.Redirect(redirectUrl);
        }
    }
}
