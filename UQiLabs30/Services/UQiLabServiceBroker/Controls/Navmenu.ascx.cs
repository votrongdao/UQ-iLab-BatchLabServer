using System;

namespace iLabs.ServiceBroker.iLabSB.Controls
{
    public partial class Navmenu : System.Web.UI.UserControl
    {
        public string PhotoUrl
        {
            get { return NavmenuPhoto.ImageUrl; }
            set { NavmenuPhoto.ImageUrl = value; }
        }

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            liManageUsers.Visible = false;
            liMyGroups.Visible = false;
            liMyLabs.Visible = false;
            liExperiments.Visible = false;
            liMyAccount.Visible = false;

            if (Session[Consts.STRSSN_UserID] == null)
            {
                liLogout.Visible = false;
            }
            else
            {
                liMyAccount.Visible = true;
                liLogout.Visible = true;
                SetNavList();
            }
        }

        //---------------------------------------------------------------------------------------//

        private void SetNavList()
        {
            // check that this user has admin privilidges, in which case, the manageUsers page should be sent to it.
            object adminState = Session[Consts.STRSSN_IsAdmin];
            bool isAdmin = ((adminState != null) && Convert.ToBoolean(adminState));

            // check that this user has service admin privilidges, in which case, the adminServices page should be sent to it.
            object serviceAdminState = Session[Consts.STRSSN_IsServiceAdmin];
            bool isServiceAdmin = ((serviceAdminState != null) && Convert.ToBoolean(serviceAdminState));

            // Do not show Labs or Experiments if Effective Group has not been specified
            if (Session[Consts.STRSSN_GroupID] != null)
            {
                if (isAdmin == false && isServiceAdmin == false)
                {
                    liManageUsers.Visible = false;
                    liMyLabs.Visible = true;
                    liExperiments.Visible = true;
                    //liSchedule.Visible = true;
                }
                else
                {
                    liManageUsers.Visible = true;
                    liMyLabs.Visible = false;
                    liExperiments.Visible = false;
                    //liSchedule.Visible = false;
                }
            }
            else
            {
                liManageUsers.Visible = false;
                liMyLabs.Visible = false;
                liExperiments.Visible = false;
                //liSchedule.Visible = false;
            }

            // Only show the groups page if the user has more than one lab
            if (Session[Consts.STRSSN_GroupCount] != null)
            {
                if (Convert.ToInt32(Session[Consts.STRSSN_GroupCount]) > 1)
                {
                    liMyGroups.Visible = true;
                }
                else
                {
                    liMyGroups.Visible = false;
                }
            }
        }

    }
}