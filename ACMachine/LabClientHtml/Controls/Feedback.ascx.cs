using System;

namespace LabClientHtml.Controls
{
    public partial class Feedback : System.Web.UI.UserControl
    {
        public string MailtoUrl
        {
            get
            {
                return urlMailto.NavigateUrl;
            }
            set
            {
                urlMailto.NavigateUrl = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}