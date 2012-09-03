<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LabSetup.ascx.cs" Inherits="LabClientHtml.LabControls.LabSetup" %>
<table id="labsetup" cols="2" border="0" cellspacing="0" cellpadding="0">
    <tr>
        <th colspan="2">
            <asp:Label ID="lblParamsTitle" runat="server" Text="Label"></asp:Label>
        </th>
    </tr>
    <tr>
        <td class="label">
            Minimum:
        </td>
        <td class="dataright">
            <asp:TextBox ID="txbSpeedMin" runat="server" Width="60px"></asp:TextBox>
            <asp:TextBox ID="txbFieldMin" runat="server" Width="60px"></asp:TextBox>
            <asp:TextBox ID="txbLoadMin" runat="server" Width="60px"></asp:TextBox>
        </td>
    </tr>
    <tr>
        <td class="label">
            Maximum:
        </td>
        <td class="dataright">
            <asp:TextBox ID="txbSpeedMax" runat="server" Width="60px"></asp:TextBox>
            <asp:TextBox ID="txbFieldMax" runat="server" Width="60px"></asp:TextBox>
            <asp:TextBox ID="txbLoadMax" runat="server" Width="60px"></asp:TextBox>
        </td>
    </tr>
    <tr>
        <td class="label">
            <nobr>
                Step Size:</nobr>
        </td>
        <td class="dataright">
            <asp:TextBox ID="txbSpeedStep" runat="server" Width="60px"></asp:TextBox>
            <asp:TextBox ID="txbFieldStep" runat="server" Width="60px"></asp:TextBox>
            <asp:TextBox ID="txbLoadStep" runat="server" Width="60px"></asp:TextBox>
        </td>
    </tr>
</table>
