<%@ Page Language="C#" MasterPageFile="~/UQiLabServiceBroker.Master" AutoEventWireup="true"
    CodeBehind="AdminServices.aspx.cs" Inherits="iLabs.ServiceBroker.iLabSB.Admin.AdminServices"
    Title="Administer Services" %>

<%@ MasterType VirtualPath="~/UQiLabServiceBroker.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="../Styles/Admin.css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h2>
        Service administration and management is not available!
    </h2>
    <p>
        Please use the original ServiceBroker interface for all administration and management
        tasks.
    </p>
</asp:Content>
