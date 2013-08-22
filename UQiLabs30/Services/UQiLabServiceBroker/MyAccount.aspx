<%@ Page Language="C#" MasterPageFile="~/UQiLabServiceBroker.Master" AutoEventWireup="true"
    CodeBehind="MyAccount.aspx.cs" Inherits="iLabs.ServiceBroker.iLabSB.MyAccount"
    Title="My Account" %>

<%@ MasterType VirtualPath="~/UQiLabServiceBroker.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table id="myaccount" cols="2" border="0" cellspacing="0" cellpadding="0">
        <tr>
            <td valign="top">
                <div class="account-left">
                    <h3>
                        Edit Account Information
                    </h3>
                    <p>
                        Complete all fields below to change your account information. You will be be emailed
                        a confirmation.
                    </p>
                    <table cols="2" border="0" cellspacing="2" cellpadding="0">
                        <tr>
                            <td class="label">
                                Username:
                            </td>
                            <td class="information">
                                <asp:TextBox ID="txtUsername" runat="server" CssClass="textbox"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="label">
                                <nobr>
                                    First Name:
                                </nobr>
                            </td>
                            <td class="information">
                                <asp:TextBox ID="txtFirstName" runat="server" CssClass="textbox"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="label">
                                <nobr>
                                    Last Name:
                                </nobr>
                            </td>
                            <td class="information">
                                <asp:TextBox ID="txtLastName" runat="server" CssClass="textbox"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="label">
                                <nobr>
                                    Email Address:
                                </nobr>
                            </td>
                            <td class="information">
                                <asp:TextBox ID="txtEmail" runat="server" CssClass="textbox"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="label">
                                <nobr>
                                    New Password:
                                </nobr>
                            </td>
                            <td class="information">
                                <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password" CssClass="textbox"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="label">
                                <nobr>
                                    Confirm Password:
                                </nobr>
                            </td>
                            <td class="information">
                                <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" CssClass="textbox"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="label">
                                &nbsp;
                            </td>
                            <td class="button">
                                <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="aspbutton" OnClick="btnSave_Click">
                                </asp:Button>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" width="100%">
                                <asp:Label ID="lblResponse" runat="server"></asp:Label>
                            </td>
                        </tr>
                    </table>
                </div>
            </td>
            <td valign="top">
                <div id="messagebox-right">
                    <h3>
                        More Information
                    </h3>
                    <h4>
                        Current group membership:
                    </h4>
                    <p class="group">
                        <asp:Label ID="lblGroups" runat="server"></asp:Label>
                    </p>
                    <h4>
                        Current group membership requests:
                    </h4>
                    <p class="group">
                        <asp:Label ID="lblRequestGroups" runat="server"></asp:Label>
                    </p>
                    <p>
                        <a href="requestgroup.aspx"><strong>Request membership in a new group.</strong></a>
                    </p>
                </div>
                <p />
            </td>
        </tr>
    </table>
</asp:Content>
