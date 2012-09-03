<%@ Page Language="C#" MasterPageFile="~/LabServer.Master" AutoEventWireup="true"
    CodeBehind="Administration.aspx.cs" Inherits="LabServer.Administration" Title="Administration" %>

<%@ MasterType VirtualPath="~/LabServer.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <p>
        Welcome! This is the LabServer administration webpage. From here, you can download
        the experiment statistics and results from the database. You can also examine the
        current state of the experiment queue.
    </p>
    <h4>
        Experiment Statistics
    </h4>
    <p>
        Download all experiment statistics stored in the database and present in XML format.
    </p>
    <table>
        <tr>
            <td class="button">
                <asp:Button ID="btnDownloadStatistics" runat="server" Text="Download" CssClass="aspbutton"
                    OnClick="btnDownloadStatistics_Click" />
            </td>
        </tr>
    </table>
    <h4>
        Experiment Results
    </h4>
    <p>
        Download all experiment results stored in the database and present in XML format.
    </p>
    <table>
        <tr>
            <td class="button">
                <asp:Button ID="btnDownloadResults" runat="server" Text="Download" CssClass="aspbutton"
                    OnClick="btnDownloadResults_Click" />
            </td>
        </tr>
    </table>
    <h4>
        Experiment Queue
    </h4>
    <p>
        View the current state of the experiment queue and present in XML format.
    </p>
    <table>
        <tr>
            <td class="button">
                <asp:Button ID="btnDownloadQueue" runat="server" Text="Download" CssClass="aspbutton"
                    OnClick="btnDownloadQueue_Click" />
            </td>
        </tr>
    </table>
</asp:Content>
