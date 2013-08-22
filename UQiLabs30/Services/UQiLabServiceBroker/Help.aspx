<%@ Page Language="C#" MasterPageFile="~/UQiLabServiceBroker.Master" AutoEventWireup="true"
    CodeBehind="Help.aspx.cs" Inherits="iLabs.ServiceBroker.iLabSB.Help" Title="Help" %>

<%@ MasterType VirtualPath="~/UQiLabServiceBroker.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="help">
        <p>
            On this page you can read the <a href="#faqs">FAQ's</a>, <a href="#help">Request Help</a>
            for a specific problem or lab, or <a href="ReportBug.aspx">Report a Bug</a> with
            the system.</p>
        <p>
            <b>IMPORTANT:</b> To be able to run the ServiceBroker you must have Pop-Ups enabled.</p>
        <h3>
            <a id="faqs" name="faqs"></a>FAQ's</h3>
        <ul>
            <li><a href="#q1">Create a ServiceBroker Account</a>
            <li><a href="#q2">Lost Password</a>
            <li><a href="#q3">Report a Bug</a>
        </ul>
        <a id="q1" name="q1"></a>
        <p class="question">
            Create a ServiceBroker Account</p>
        <p class="answer">
            When you entered the ServiceBroker URL, your browser was directed to the Login webpage.
            If you have already created an account, you may login. To create an account select
            the "Register" button, which will take you to the Register webpage.
            Fill out the form and submit your information. Usernames must be unique for each
            ServiceBroker. If the username is already registered, a message will be displayed
            asking you to choose another username. An error message is displayed if your password
            and confirm password entries do not match. Once your information is complete your
            account will be created and you will automatically be redirected to the "My Clients"
            webpage.
        </p>
        <a id="q2" name="q2"></a>
        <p class="question">
            Lost Password</p>
        <p class="answer">
            If you have an account, but do not remember your password, go to the <a href="LostPassword.aspx">
                Lost Password</a> webpage. Fill out the form and supply both username
            and email address. If a registered user with both username and email address is
            found, a new temporary password will be mailed to the email address.</p>
        <a id="q3" name="q3"></a>
        <p class="question">
            Report a Bug</p>
        <p class="answer">
            To report a bug, go to the <a href="reportBug.aspx">Report Bug</a> webpage. Please
            select the general type of problem and enter a detailed description of the problem.</p>
        <hr />
        <h3>
            <a id="help" name="help"></a>Request Help</h3>
        <p>
            Fill out the form below to request help with the iLab system or a particular lab.
            Someone will respond to you shortly.</p>
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
                    <asp:DropDownList ID="ddlHelpType" runat="server" CssClass="dropdownlist">
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
                    <asp:Button ID="btnRequestHelp" runat="server" Text="Request Help" CssClass="button"
                        OnClick="btnRequestHelp_Click"></asp:Button>
                </td>
            </tr>
        </table>
        <p />
        <asp:Label ID="lblResponse" Visible="False" runat="server"></asp:Label>
    </div>
</asp:Content>
