using System;

namespace iLabs.ServiceBroker.iLabSB.Controls
{
    public partial class Banner : System.Web.UI.UserControl
    {
        public string Title
        {
            get
            {
                return lblTitle.Text;
            }
            set
            {
                lblTitle.Text = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}