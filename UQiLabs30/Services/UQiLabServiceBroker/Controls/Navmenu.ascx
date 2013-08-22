<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Navmenu.ascx.cs" Inherits="iLabs.ServiceBroker.iLabSB.Controls.Navmenu" %>
<table id="navmenu" cols="1" border="0" cellspacing="0" cellpadding="0">
    <tr>
        <td class="photo">
            <asp:Image ID="NavmenuPhoto" runat="server" Width="200px" />
        </td>
    </tr>
    <tr>
        <td class="header">
            ON THIS SITE
        </td>
    </tr>
    <tr>
        <td>
            <ul>
                <li runat="server"><a href="~/Home.aspx" runat="server">&#187; Home</a></li>
                <li id="liMyGroups" runat="server"><a href="~/MyGroups.aspx" runat="server">&#187; My
                    Groups</a></li>
                <li id="liMyLabs" runat="server"><a href="~/MyLabs.aspx" runat="server">&#187; My Labs</a></li>
                <li id="liExperiments" runat="server"><a href="~/MyExperiments.aspx" runat="server">
                    &#187; My Experiments</a></li>
                <li id="liMyAccount" runat="server"><a href="~/MyAccount.aspx" runat="server">&#187;
                    My Account</a></li>
                <li id="liManageUsers" runat="server"><a href="~/Admin/ManageUsers.aspx" runat="server">&#187;
                    ManageUsers</a></li>
                <li runat="server"><a href="~/Help.aspx" runat="server">&#187; Help</a></li>
                <li id="liLogout" runat="server"><a href="~/Logout.aspx" runat="server">&#187; Logout</a></li>
            </ul>
        </td>
    </tr>
</table>
