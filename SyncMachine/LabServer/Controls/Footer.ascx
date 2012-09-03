<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Footer.ascx.cs" Inherits="LabServer.Controls.Footer" %>
<table id="footer" cols="3" border="0" cellspacing="0" cellpadding="3">
    <tr>
        <td class="ceit-logo">
            <a href="http://www.ceit.uq.edu.au/" target="_blank">
                <asp:Image ID="Image1" runat="server" ImageUrl="~/images/ceit_logo.gif" Width="176px"
                    Height="64px" /></a>
        </td>
        <td class="spacer">
            &nbsp;
        </td>
        <td class="mit-logo">
            <a href="http://www.mit.edu/" target="_blank">
                <asp:Image ID="imgFooter" runat="server" ImageUrl="~/images/mit-blackred-footer1.gif"
                    Width="334px" Height="36px" /></a>
        </td>
    </tr>
</table>
