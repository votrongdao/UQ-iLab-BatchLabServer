<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Feedback.ascx.cs" Inherits="iLabs.ServiceBroker.iLabSB.Controls.Feedback" %>
<table id="feedback" cols="2" border="0" cellspacing="0" cellpadding="0">
    <tr>
        <td class="timezone">
            <nobr>
                <asp:Label ID="lblTimezone" runat="server"></asp:Label></nobr>
        </td>
        <td>
            <asp:HyperLink ID="urlMailto" runat="server" Text="Feedback"></asp:HyperLink>
        </td>
    </tr>
</table>
