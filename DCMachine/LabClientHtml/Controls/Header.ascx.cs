using System;

namespace LabClientHtml.Controls
{
    public partial class Header : System.Web.UI.UserControl
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