using System;

namespace iLabs.ServiceBroker.iLabSB.Controls
{
    public partial class Feedback : System.Web.UI.UserControl
    {
        #region Properties

        public string Timezone
        {
            get { return lblTimezone.Text; }
            set { lblTimezone.Text = value; }
        }

        public string MailtoUrl
        {
            get { return urlMailto.NavigateUrl; }
            set { urlMailto.NavigateUrl = value; }
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}