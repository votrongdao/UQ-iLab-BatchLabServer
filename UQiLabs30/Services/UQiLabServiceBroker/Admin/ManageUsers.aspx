<%@ Page Language="C#" MasterPageFile="~/UQiLabServiceBroker.Master" AutoEventWireup="true"
    CodeBehind="ManageUsers.aspx.cs" Inherits="iLabs.ServiceBroker.iLabSB.Admin.ManageUsers"
    Title="Manage Users" %>

<%@ MasterType VirtualPath="~/UQiLabServiceBroker.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h2>
        Add students by class list
    </h2>
    <table cols="2">
        <tr>
            <td colspan="2">
                1. Select class list and upload.
            </td>
        </tr>
        <tr>
            <td class="textlabel">
                Filename:
            </td>
            <td>
                <asp:FileUpload ID="FileUpload1" runat="server" Width="320" />
            </td>
        </tr>
        <tr>
            <td>
                &nbsp;
            </td>
            <td>
                <asp:Button ID="btnUpload" runat="server" Text="Upload" OnClick="btnUpload_Click" />
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <br />
                2. Check uploaded class lists and remove if required.
            </td>
        </tr>
        <tr>
            <td class="textlabel">
                <nobr>
                    Class Lists:</nobr>
            </td>
            <td>
                <asp:DropDownList ID="ddlFiles" runat="server" Width="480" AutoPostBack="true" OnSelectedIndexChanged="ddlFiles_SelectedIndexChanged">
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td>
                &nbsp;
            </td>
            <td>
                <asp:Button ID="btnFilesRemove" runat="server" Text="Remove" CssClass="aspbutton"
                    OnClick="btnFilesRemove_Click" />
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <br />
                3. Select class list above and parse for errors.
            </td>
        </tr>
        <tr>
            <td>
                &nbsp;
            </td>
            <td>
                <asp:Button ID="btnParseFile" runat="server" Text="Parse File" CssClass="aspbutton"
                    Width="120" OnClick="btnParseFile_Click" />
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <br />
                4. Select user affiliation.
            </td>
        </tr>
        <tr>
            <td class="textlabel">
                <nobr>
                    Affiliation:</nobr>
            </td>
            <td>
                <asp:DropDownList ID="ddlAffiliations" runat="server" Width="160">
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <br />
                5. Select group membership.
            </td>
        </tr>
        <tr>
            <td class="textlabel">
                <nobr>
                    Group:</nobr>
            </td>
            <td>
                <asp:DropDownList ID="ddlGroupNames" runat="server" Width="240">
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <br />
                6. Using selected class list, affiliation and group, parse for errors and add users.
            </td>
        </tr>
        <tr>
            <td>
                &nbsp;
            </td>
            <td>
                <asp:Button ID="btnAddUsers" runat="server" Text="Add Users" CssClass="aspbutton"
                    Width="120" OnClick="btnAddUsers_Click" />
            </td>
        </tr>
    </table>
    <br />
    <asp:Label ID="lblMessage" runat="server" CssClass="responsebox"></asp:Label>
</asp:Content>
