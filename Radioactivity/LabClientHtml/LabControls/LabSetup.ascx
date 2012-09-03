<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LabSetup.ascx.cs" Inherits="LabClientHtml.LabControls.LabSetup" %>
<table id="labsetup" cols="3" border="0" cellspacing="0" cellpadding="0">
    <tr>
        <td class="label">
            Source:
        </td>
        <td class="dataright" colspan="2">
            <asp:DropDownList ID="ddlSources" runat="server" Width="140px">
            </asp:DropDownList>
        </td>
    </tr>
    <tr>
        <td class="label">
            Absorber:
        </td>
        <td class="data">
            <asp:DropDownList ID="ddlAbsorbers" runat="server" Width="140px">
            </asp:DropDownList>
        </td>
        <td class="dataright">
            <asp:Button ID="btnSelectedAbsorbersAdd" runat="server" Text="Add" CssClass="aspbutton"
                OnClick="btnSelectedAbsorbersAdd_Click" />
        </td>
    </tr>
    <tr id="trAbsorberList" runat="server">
        <td class="label">
            <asp:Label ID="lblSelectedAbsorbers" runat="server" Text="Absorber&nbsp;List:"></asp:Label>
        </td>
        <td class="data">
            <asp:DropDownList ID="ddlSelectedAbsorbers" runat="server" Width="140px">
            </asp:DropDownList>
        </td>
        <td class="dataright">
            <asp:Button ID="btnSelectedAbsorbersClear" runat="server" Text="Clear" CssClass="aspbutton"
                OnClick="btnSelectedAbsorbersClear_Click" />
        </td>
    </tr>
    <tr>
        <td class="label">
            Distance:
        </td>
        <td class="data">
            <asp:DropDownList ID="ddlDistances" runat="server" Width="66px">
            </asp:DropDownList>
            &nbsp;(mm)
        </td>
        <td class="dataright">
            <asp:Button ID="btnSelectedDistancesAdd" runat="server" Text="Add" CssClass="aspbutton"
                OnClick="btnSelectedDistancesAdd_Click" />
        </td>
    </tr>
    <tr id="trDistanceList" runat="server">
        <td class="label">
            <asp:Label ID="lblSelectedDistances" runat="server" Text="Distance&nbsp;List:"></asp:Label>
        </td>
        <td class="data">
            <asp:DropDownList ID="ddlSelectedDistances" runat="server" Width="66px">
            </asp:DropDownList>
        </td>
        <td class="dataright">
            <asp:Button ID="btnSelectedDistancesClear" runat="server" Text="Clear" CssClass="aspbutton"
                OnClick="btnSelectedDistancesClear_Click" />
        </td>
    </tr>
    <tr>
        <td class="label">
            Duration:
        </td>
        <td class="dataright" colspan="2">
            <asp:TextBox ID="txbDuration" runat="server" Width="60px"></asp:TextBox>&nbsp;(secs)
        </td>
    </tr>
    <tr>
        <td class="label">
            Trials:
        </td>
        <td class="dataright" colspan="2">
            <asp:TextBox ID="txbRepeat" runat="server" Width="60px"></asp:TextBox>
        </td>
    </tr>
</table>
