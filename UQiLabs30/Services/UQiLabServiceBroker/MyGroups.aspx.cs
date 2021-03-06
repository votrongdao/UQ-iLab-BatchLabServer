﻿using System;
using System.Collections;
using System.Web.UI.WebControls;

using iLabs.ServiceBroker.Administration;
using iLabs.ServiceBroker.Authorization;

namespace iLabs.ServiceBroker.iLabSB
{
    public partial class MyGroups : System.Web.UI.Page
    {
        protected ArrayList nonRequestGroups = new ArrayList();
        protected ArrayList requestGroups = new ArrayList();
        AuthorizationWrapperClass wrapper = new AuthorizationWrapperClass();

        //---------------------------------------------------------------------------------------//

        protected void Page_Init(object sender, EventArgs e)
        {
            Master.HeaderTitle = this.Title;
            this.Title = Master.PageTitle + this.Title;
        }

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            //if(! IsPostBack)
            {

                // To list all the groups a user belongs to
                int userID = Convert.ToInt32(Session["UserID"]);
                int[] groupIDs = wrapper.ListGroupsForAgentWrapper(userID);

                //since we already have the groups a user has access
                // if we use wrapper here, it will deny authentication
                Group[] gps = AdministrativeAPI.GetGroups(groupIDs);

                foreach (Group g in gps)
                {
                    if (g.groupName.EndsWith("request"))
                        requestGroups.Add(g);
                    else
                        if (!g.groupName.Equals(Group.NEWUSERGROUP))
                            nonRequestGroups.Add(g);
                }
            }

            if ((nonRequestGroups == null) || (nonRequestGroups.Count == 0))
            {
                lblNoGroups.Text = "<p>You currently do not have access to any group. It can take upto 48 hours for a group administrator to give you permission to the groups you've requested.</p>";
                lblNoGroups.Visible = true;
                Session["GroupCount"] = 0;
            }
            else
            {
                //Redirect to single lab single group page?
                if (nonRequestGroups.Count == 1)
                {

                    if (nonRequestGroups[0] != null)
                    {
                        Session["GroupID"] = ((Group)nonRequestGroups[0]).groupID;
                        Session["GroupName"] = ((Group)nonRequestGroups[0]).groupName;
                        Session["GroupCount"] = 1;
                        AdministrativeAPI.SetSessionGroup(Convert.ToInt64(Session["SessionID"]), ((Group)nonRequestGroups[0]).groupID);
                        PageRedirect((Group)nonRequestGroups[0]);
                    }

                }
                else
                {
                    Session["GroupCount"] = nonRequestGroups.Count;
                }
                repGroups.DataSource = nonRequestGroups;
                repGroups.DataBind();
            }

            int repCount = 1;
            // To list all the labs belonging to a group
            foreach (Group g in nonRequestGroups)
            {
                int[] lcIDsList = AdministrativeUtilities.GetGroupLabClients(g.groupID);

                LabClient[] lcList = wrapper.GetLabClientsWrapper(lcIDsList);

                Label lblGroupLabs = new Label();
                lblGroupLabs.Visible = true;
                lblGroupLabs.Text = "<ul>";

                for (int i = 0; i < lcList.Length; i++)
                {
                    lblGroupLabs.Text += "<li><strong class=lab>" +
                        lcList[i].clientName + "</strong> - " +
                        lcList[i].clientShortDescription + "</li>";
                }
                lblGroupLabs.Text += "</ul>";
                repGroups.Controls.AddAt(repCount, lblGroupLabs);
                repCount += 2;
            }

            if ((requestGroups != null) && (requestGroups.Count > 0))
            {
                for (int i = 0; i < requestGroups.Count; i++)
                {
                    int origGroupID = AdministrativeAPI.GetAssociatedGroupID(((Group)requestGroups[i]).groupID);
                    string origGroupName = AdministrativeAPI.GetGroups(new int[] { origGroupID })[0].groupName;
                    lblRequestGroups.Text += origGroupName;
                    if (i != requestGroups.Count - 1)
                        lblRequestGroups.Text += ", ";
                }
            }
            else
            {
                lblRequestGroups.Text = "No group";
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void repGroups_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            if (Session["UserID"] == null)
                Response.Redirect("login.aspx");
            else
            {
                if (e.CommandName == "SetEffectiveGroup")
                {
                    // get the groupID from the nonRequestGroups ArrayList.
                    // The index of the ArrayList will match the index of the repeater
                    // since the repeater was loaded from the ArrayList.
                    int groupID = ((Group)nonRequestGroups[e.Item.ItemIndex]).groupID;

                    // Set the GroupID session value and redirect
                    Session["GroupID"] = groupID;
                    Session["GroupName"] = ((Group)nonRequestGroups[e.Item.ItemIndex]).groupName;
                    AdministrativeAPI.SetSessionGroup(Convert.ToInt64(Session["SessionID"]), groupID);
                    PageRedirect((Group)nonRequestGroups[e.Item.ItemIndex]);
                }
            }
        }

        //---------------------------------------------------------------------------------------//

        private void PageRedirect(Group effectiveGroup)
        {
            // initialize boolean session variables that indicate what type of effective group this is
            Session["IsAdmin"] = false;
            Session["IsServiceAdmin"] = false;

            if ((effectiveGroup.groupName.Equals(Group.SUPERUSER)) || (effectiveGroup.groupType.Equals(GroupType.COURSE_STAFF)))
            {
                Session["IsAdmin"] = true;
                Response.Redirect("admin/manageUsers.aspx");
            }

            // if the effective group is a service admin group, then redirect to the service admin page.
            // the session variable is used in the userNav page to check whether to make the corresponing tab visible
            else if (effectiveGroup.groupType.Equals(GroupType.SERVICE_ADMIN))
            {
                Session["IsServiceAdmin"] = true;
                Response.Redirect("admin/adminServices.aspx");
            }

            else
            {
                Session["IsAdmin"] = false;
                Session["IsServiceAdmin"] = false;
                Response.Redirect("myLabs.aspx");
            }
        }

    }
}
