using System;
using System.Collections.Generic;

using iLabs.ServiceBroker.Administration;
using iLabs.ServiceBroker.Authorization;

namespace iLabs.ServiceBroker.iLabSB
{
    public partial class MyClientList : System.Web.UI.Page
    {
        AuthorizationWrapperClass wrapper = new AuthorizationWrapperClass();
        protected LabClient[] lcList = null;

        //---------------------------------------------------------------------------------------//

        protected void Page_Init(object sender, EventArgs e)
        {
            Master.HeaderTitle = this.Title;
            this.Title = Master.PageTitle + this.Title;
        }

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["GroupName"] != null)
                {
                    string groupName = Session["GroupName"].ToString();
                    lblGroupNameTitle.Text = groupName;
                    lblGroupNameSystemMessage.Text = groupName;
                    lblGroupNameLabList.Text = groupName;
                }

            }
            // This doesn't work - is it possible to stick an int array in the session?
            //int[] lcIDList = (int[])(Session["LabClientList"]);

            //Temporarily getting the list again from using the Utilities class
            int[] lcIDList = AdministrativeUtilities.GetGroupLabClients(Convert.ToInt32(Session["GroupID"]));
            lcList = wrapper.GetLabClientsWrapper(lcIDList);

            repLabs.DataSource = lcList;
            repLabs.DataBind();

            List<SystemMessage> messagesList = new List<SystemMessage>();
            SystemMessage[] groupMessages = null;

            groupMessages = wrapper.GetSystemMessagesWrapper(SystemMessage.GROUP, Convert.ToInt32(Session["GroupID"]), 0, 0);
            if (groupMessages != null)
                messagesList.AddRange(groupMessages);

            if (messagesList != null && messagesList.Count > 0)
            {
                lblNoMessages.Visible = false;

                messagesList.Sort(SystemMessage.CompareDateDesc);
                //messagesList.Reverse();
                repSystemMessage.DataSource = messagesList;
                repSystemMessage.DataBind();
            }

            else
            {
                lblNoMessages.Visible = true;
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void repLabs_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            if (Session["UserID"] == null)
                Response.Redirect("login.aspx");
            else
            {
                if (e.CommandName == "SetLabClient")
                {
                    // get the labClientID from the lcList.
                    // The indexer of the List will match the index of the repeater
                    // since the repeater was loaded from the List.
                    int clientID = ((LabClient)lcList[e.Item.ItemIndex]).clientID;

                    // Set the LabClient session value and redirect
                    Session["ClientID"] = clientID;
                    AdministrativeAPI.SetSessionClient(Convert.ToInt64(Session["SessionID"]), clientID);
                    Response.Redirect("myClient.aspx");
                }
            }
        }

    }
}
