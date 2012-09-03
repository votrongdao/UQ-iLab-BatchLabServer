<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Navmenu.ascx.cs" Inherits="LabClientHtml.Controls.Navmenu" %>
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
                <li runat="server"><a href="~/LabClient.aspx" runat="server">&#187;&nbsp;Home</a></li>
                <li runat="server"><a href="~/Setup.aspx" runat="server">&#187;&nbsp;Setup</a></li>
                <li runat="server"><a href="~/Status.aspx" runat="server">&#187;&nbsp;Status</a></li>
                <li runat="server"><a href="~/Results.aspx" runat="server">&#187;&nbsp;Results</a></li>
                <li id="liCamera" runat="server"><a id="aCamera" href="" target="_blank" runat="server">&#187;&nbsp;Camera</a></li>
            </ul>
        </td>
    </tr>
</table>
