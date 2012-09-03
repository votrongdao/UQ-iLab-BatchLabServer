<%@ Page Language="C#" MasterPageFile="~/LabClient.Master" AutoEventWireup="true"
    CodeBehind="Setup.aspx.cs" Inherits="LabClientHtml.Setup" Title="Setup" %>

<%@ Register TagPrefix="uc" TagName="LabSetup" Src="~/LabControls/LabSetup.ascx" %>
<%@ MasterType VirtualPath="~/LabClient.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="styles/LabControls.css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table id="setup" cols="2" border="0" cellspacing="0" cellpadding="0">
        <tr>
            <th colspan="2">
                Experiment Setups
            </th>
        </tr>
        <tr>
            <td class="label">
                Setup:
            </td>
            <td class="information">
                <asp:DropDownList ID="ddlExperimentSetups" runat="server" CssClass="dropdownlist"
                    AutoPostBack="true" OnSelectedIndexChanged="ddlExperimentSetups_SelectedIndexChanged">
                </asp:DropDownList>
                <asp:DropDownList ID="ddlExperimentSetupIds" runat="server" Visible="false">
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td class="label">
                &nbsp;
            </td>
            <td class="description">
                <asp:Label ID="lblSetupDescription" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <uc:LabSetup ID="labSetup" runat="server" />
            </td>
        </tr>
        <tr>
            <td class="label">
                &nbsp;
            </td>
            <td class="button">
                <asp:Button ID="btnValidate" runat="server" Text="Validate" CssClass="aspbutton"
                    OnClick="btnValidate_Click1" />
                <asp:Button ID="btnSubmit" runat="server" Text="Submit" CssClass="aspbutton" OnClick="btnSubmit_Click1" />
            </td>
        </tr>
    </table>
    <p />
    <asp:Label ID="lblMessage" runat="server" CssClass="messagebox"></asp:Label>
</asp:Content>
