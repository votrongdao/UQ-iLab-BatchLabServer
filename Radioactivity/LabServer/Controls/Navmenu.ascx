<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Navmenu.ascx.cs" Inherits="LabServer.Controls.Navmenu" %>
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
                <li runat="server"><a href="~/Administration.aspx" runat="server">&#187;&nbsp;Administration</a></li>
            </ul>
        </td>
    </tr>
</table>
