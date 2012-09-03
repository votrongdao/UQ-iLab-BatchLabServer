using System;

namespace LabClientHtml.Controls
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

        public string CameraUrl
        {
            get
            {
                return aCamera.InnerText;
            }
            set
            {
                if (value != null && value.Trim().Length > 0)
                {
                    liCamera.Visible = true;
                    aCamera.HRef = value;
                }
                else
                {
                    liCamera.Visible = false;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}