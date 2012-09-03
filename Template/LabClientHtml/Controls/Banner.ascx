<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Banner.ascx.cs" Inherits="LabClientHtml.Controls.Banner" %>
<table id="banner" cols="2" border="0" cellspacing="0" cellpadding="0">
    <tr>
        <td class="logo">
            <a href="http://www.uq.edu.au/" target="_blank">
            <asp:Image ID="imgLogo" runat="server" ImageUrl="../images/uq-logo.gif" ImageAlign="Left"
                Width="200px" Height="72px" /></a>
        </td>
        <td class="title">
            <asp:Label ID="lblTitle" runat="server" Text="Error: Check logfile for possible cause!"></asp:Label>
        </td>
    </tr>
</table>
