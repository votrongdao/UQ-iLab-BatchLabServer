<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Statusbar.ascx.cs" Inherits="iLabs.ServiceBroker.iLabSB.Controls.Statusbar" %>
<table id="statusbar" cols="3" border="0" cellspacing="0" cellpadding="0">
    <tr>
        <td class="user">
            <nobr>
                <asp:Label ID="lblUsername" runat="server"></asp:Label></nobr>
        </td>
        <td class="group">
            <nobr>
                <asp:Label ID="lblGroupname" runat="server"></asp:Label></nobr>
        </td>
        <td class="version">
            <nobr>
                <asp:Label ID="lblVersion" runat="server"></asp:Label></nobr>
        </td>
    </tr>
</table>
