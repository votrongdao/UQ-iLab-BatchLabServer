using System;

namespace iLabs.ServiceBroker.iLabSB.Controls
{
    public partial class Statusbar : System.Web.UI.UserControl
    {
        #region Properties

        public string Version
        {
            get { return lblVersion.Text; }
            set { lblVersion.Text = value; }
        }

        public string Username
        {
            get { return lblUsername.Text; }
            set { lblUsername.Text = value; }
        }

        public string Groupname
        {
            get { return lblGroupname.Text; }
            set { lblGroupname.Text = value; }
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}