<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Header.ascx.cs" Inherits="LabClientHtml.Controls.Header" %>
<table id="header" cols="1" border="0" cellspacing="0" cellpadding="0">
    <tr>
        <td>
            <asp:Image ID="Image1" runat="server" ImageUrl="../Images/level2-arrow.gif" Width="28px"
                Height="28px" />
            <asp:Label ID="lblTitle" runat="server" CssClass="header-title" Text="Label"></asp:Label>
        </td>
    </tr>
    <tr>
        <td colspan="2">
            <asp:Image ID="Image2" runat="server" ImageUrl="../Images/level2-underline.gif" Width="395px"
                Height="1px" />
        </td>
    </tr>
</table>
