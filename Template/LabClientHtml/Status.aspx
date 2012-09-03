<%@ Page Language="C#" MasterPageFile="~/LabClient.Master" AutoEventWireup="true"
    CodeBehind="Status.aspx.cs" Inherits="LabClientHtml.Status" Title="Status" %>

<%@ MasterType VirtualPath="~/LabClient.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table id="status" cols="1" border="0" cellspacing="0" cellpadding="0">
        <tr>
            <th colspan="2">
                LabServer Status
            </th>
        </tr>
        <tr>
            <td class="label">
                Status:
            </td>
            <td class="information">
                <asp:Label ID="lblOnline" runat="server"></asp:Label>&nbsp;
            </td>
        </tr>
        <tr>
            <td class="label">
                Message:
            </td>
            <td class="information">
                <asp:Label ID="lblLabServerStatusMsg" runat="server"></asp:Label>&nbsp;
            </td>
        </tr>
        <tr>
            <td class="label">
                &nbsp;
            </td>
            <td class="button">
                <asp:Button ID="btnRefresh" runat="server" Text="Refresh" CssClass="aspbutton" OnClick="btnRefresh_Click1" />
            </td>
        </tr>
        <tr>
            <th colspan="2">
                Experiment Status
            </th>
        </tr>
        <tr>
            <td class="label">
                Experiment:
            </td>
            <td class="information">
                <asp:TextBox ID="txbExperimentID" runat="server" Width="60px"></asp:TextBox>
                <asp:DropDownList ID="ddlExperimentIDs" runat="server" Width="66px" AutoPostBack="true"
                    OnSelectedIndexChanged="ddlExperimentIDs_SelectedIndexChanged">
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td class="label">
                &nbsp;
            </td>
            <td class="button">
                <asp:Button ID="btnCheck" runat="server" Text="Check" CssClass="aspbutton" OnClick="btnCheck_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="aspbutton" OnClick="btnCancel_Click" />
            </td>
        </tr>
    </table>
    <p />
    <asp:Label ID="lblExpStatMessage" runat="server" CssClass="messagebox"></asp:Label>
</asp:Content>
