<%@ Page Language="C#" MasterPageFile="~/UQiLabServiceBroker.Master" AutoEventWireup="true"
    CodeBehind="Register.aspx.cs" Inherits="iLabs.ServiceBroker.iLabSB.Register"
    Title="Register" %>

<%@ MasterType VirtualPath="~/UQiLabServiceBroker.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="register">
        <p>
            Fill out the form below to register for an iLab account. You will be emailed a confirmation.
        </p>
        <table cols="2" cellspacing="2">
            <tr>
                <td class="textlabel">
                    Username:
                </td>
                <td>
                    <asp:TextBox ID="txtUsername" runat="server" CssClass="textbox"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    <nobr>
                        First Name:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtFirstName" runat="server" CssClass="textbox"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    <nobr>
                        Last Name:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtLastName" runat="server" CssClass="textbox"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    <nobr>
                        Email Address:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="textbox"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    Affiliation:
                </td>
                <td>
                    <asp:DropDownList ID="ddlAffiliation" runat="server" CssClass="dropdownlist">
                    </asp:DropDownList>
                    <asp:TextBox ID="txtAffiliation" runat="server" CssClass="textbox"></asp:TextBox>
                </td>
            </tr>
            <tr id="trowRequestGroup" runat="server">
                <td class="textlabel">
                    <nobr>
                        Requested Group:</nobr>
                </td>
                <td>
                    <asp:DropDownList ID="ddlGroup" runat="server" CssClass="groupdropdownlist">
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    Password:
                </td>
                <td>
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="textbox"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    <nobr>
                        Confirm Password:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" CssClass="textbox"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    Purpose for requesting account:
                </td>
                <td>
                    <asp:TextBox ID="txtReason" runat="server" TextMode="MultiLine" Rows="6" CssClass="editbox"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    &nbsp;
                </td>
                <td>
                    <asp:Button ID="btnSubmit" runat="server" CssClass="button" Text="Submit" OnClick="btnSubmit_Click">
                    </asp:Button>
                    <asp:Button ID="btnCancel" runat="server" CssClass="button" Text="Cancel" OnClick="btnCancel_Click">
                    </asp:Button>
                </td>
            </tr>
        </table>
        <p />
        <asp:Label ID="lblResponse" runat="server" Visible="False"></asp:Label>
    </div>
</asp:Content>
