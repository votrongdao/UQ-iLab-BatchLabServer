<%@ Page Language="C#" MasterPageFile="~/UQiLabServiceBroker.Master" AutoEventWireup="true"
    CodeBehind="LostPassword.aspx.cs" Inherits="iLabs.ServiceBroker.iLabSB.LostPassword"
    Title="Lost Password" %>

<%@ MasterType VirtualPath="~/UQiLabServiceBroker.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <p>
        Submit your username and email address. A random password will be generated
        and emailed to you.
    </p>
    <table cols="2" cellspacing="2">
        <tr>
            <td class="textlabel">
                Username:
            </td>
            <td>
                <asp:TextBox ID="txtUsername" runat="server" Width="200px"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td class="textlabel">
                <nobr>Email Address:</nobr>
            </td>
            <td>
                <asp:TextBox ID="txtEmail" runat="server" Width="200px"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td class="textlabel">
                &nbsp;
            </td>
            <td>
                <asp:Button ID="btnSubmit" runat="server" CssClass="button" Text="Submit" OnClick="btnSubmit_Click">
                </asp:Button>
            </td>
        </tr>
    </table>
    <p />
    <asp:Label ID="lblResponse" runat="server" Visible="False"></asp:Label>
</asp:Content>
