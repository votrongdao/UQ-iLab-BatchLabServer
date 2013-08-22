using System;

using iLabs.ServiceBroker.Administration;

namespace iLabs.ServiceBroker.iLabSB
{
    public partial class MyLabs : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            Master.HeaderTitle = this.Title;
            this.Title = Master.PageTitle + this.Title;
        }

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            int userID = Convert.ToInt32(Session["UserID"]);
            int groupID = Convert.ToInt32(Session["GroupID"]);

            int[] labClientIDList = AdministrativeUtilities.GetGroupLabClients(groupID);

            if (labClientIDList != null)
            {
                Session["ClientCount"] = labClientIDList.Length;
                if (labClientIDList.Length > 1)
                {
                    Response.Redirect("myClientList.aspx");
                    Session["LabClientList"] = labClientIDList;
                }
                else if (labClientIDList.Length == 1)
                {
                    // get the lab client
                    int clientID = labClientIDList[0];
                    Session["ClientID"] = clientID;
                    Response.Redirect("myClient.aspx");
                }
                else if (labClientIDList.Length == 0)
                {
                    Response.Redirect("myClient.aspx");
                }
            }
        }

    }
}
