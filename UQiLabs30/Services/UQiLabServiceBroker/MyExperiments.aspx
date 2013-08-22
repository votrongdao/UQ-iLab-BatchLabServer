<%@ Page Language="C#" MasterPageFile="~/UQiLabServiceBroker.Master" AutoEventWireup="true"
    CodeBehind="MyExperiments.aspx.cs" Inherits="iLabs.ServiceBroker.iLabSB.MyExperiments"
    Title="My Experiments" %>

<%@ MasterType VirtualPath="~/UQiLabServiceBroker.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="myexperiments">
        <p>
            View your experiment records by entering a time range and then selecting an experiment
            below.
        </p>
        <asp:Label ID="lblResponse" runat="server" Visible="False"></asp:Label>
        <p />
        <h2>
            Search Experiments</h2>
        <p />
        <table cols="2" cellspacing="2">
            <tr>
                <td class="textlabel">
                    <nobr>
                        Time Event:</nobr>
                </td>
                <td>
                    <asp:DropDownList ID="ddlTimeAttribute" runat="server" CssClass="dropdownlist" AutoPostBack="True"
                        OnSelectedIndexChanged="ddlTimeAttribute_SelectedIndexChanged">
                        <asp:ListItem Value="before">before</asp:ListItem>
                        <asp:ListItem Value="after">after</asp:ListItem>
                        <asp:ListItem Value="between">between</asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr>
                <td>
                    <nobr>
                        Start Date/Time:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtTime1" runat="server" CssClass="textbox-time"></asp:TextBox>
                    &nbsp;(MM/DD/YYYY hh:mm:ss)
                </td>
            </tr>
            <tr>
                <td>
                    <nobr>
                        End Date/Time:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtTime2" runat="server" CssClass="textbox-time" Enabled="False"></asp:TextBox>
                    &nbsp;(MM/DD/YYYY hh:mm:ss)
                </td>
            </tr>
            <tr>
                <td>
                    &nbsp;
                </td>
                <td>
                    <asp:Button ID="btnGo" runat="server" CssClass="button" Text="Search" OnClick="btnGo_Click">
                    </asp:Button>
                </td>
            </tr>
        </table>
        <p />
        <table cellspacing="2">
            <tr>
                <td>
                    <asp:ListBox ID="lbxSelectExperiment" runat="server" CssClass="textbox-long" Rows="10">
                    </asp:ListBox>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button ID="btnShowExperiment" CssClass="button" runat="server" Text="Display Experiment Data"
                        Enabled="true" OnClick="btnShowExperiment_Click"></asp:Button>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
