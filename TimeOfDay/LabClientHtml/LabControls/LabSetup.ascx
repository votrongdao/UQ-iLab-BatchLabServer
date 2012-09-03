<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LabSetup.ascx.cs" Inherits="LabClientHtml.LabControls.LabSetup" %>
<table id="labsetup" cols="2" border="0" cellspacing="0" cellpadding="0">
    <tr>
        <td class="label">
            <asp:Label ID="lblTimeServerUrl" runat="server" Text="<nobr>Server Url:</nobr>"></asp:Label>
        </td>
        <td class="dataright">
            <asp:DropDownList ID="ddlTimeServerUrl" runat="server" Width="160px">
            </asp:DropDownList>
        </td>
    </tr>
    <tr>
        <td class="label">
            <nobr>
                Time format:
            </nobr>
        </td>
        <td class="dataright">
            <asp:DropDownList ID="ddlTimeFormat" runat="server" Width="100px">
            </asp:DropDownList>
        </td>
    </tr>
</table>
