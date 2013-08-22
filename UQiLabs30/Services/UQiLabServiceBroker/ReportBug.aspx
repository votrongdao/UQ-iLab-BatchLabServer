<%@ Page Language="C#" MasterPageFile="~/UQiLabServiceBroker.Master" AutoEventWireup="true"
    CodeBehind="ReportBug.aspx.cs" Inherits="iLabs.ServiceBroker.iLabSB.ReportBug"
    Title="Report Bug" %>

<%@ MasterType VirtualPath="~/UQiLabServiceBroker.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="reportbug">
        <p>
            Fill out the form below to report a bug with the iLab system or a particular lab.
        </p>
        <% if (Session["UserID"] == null)
           { %>
        <p>
            You are not currently logged in. Please include your name and email address, so
            that we can respond to you.</p>
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
                        Email Address:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="textbox"></asp:TextBox>
                </td>
            </tr>
        </table>
        <% } %>
        <table cols="2" cellspacing="2">
            <tr>
                <td>
                    <nobr>
                        Type of help:</nobr>
                </td>
                <td>
                    <asp:DropDownList ID="ddlBugType" runat="server" CssClass="dropdownlist">
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    Description:
                </td>
                <td>
                    <asp:TextBox ID="txtDescription" runat="server" Rows="6" TextMode="MultiLine" CssClass="editbox"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    &nbsp;
                </td>
                <td>
                    <asp:Button ID="btnReportBug" runat="server" Text="Report Bug" CssClass="button"
                        OnClick="btnReportBug_Click"></asp:Button>
                </td>
            </tr>
        </table>
        <p />
        <asp:Label ID="lblResponse" Visible="False" runat="server"></asp:Label>
    </div>
</asp:Content>
