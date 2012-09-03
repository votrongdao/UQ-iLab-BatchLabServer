using System;

namespace LabServer.Controls
{
    public partial class Navmenu : System.Web.UI.UserControl
    {
        public string PhotoUrl
        {
            get
            {
                return NavmenuPhoto.ImageUrl;
            }
            set
            {
                NavmenuPhoto.ImageUrl = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}