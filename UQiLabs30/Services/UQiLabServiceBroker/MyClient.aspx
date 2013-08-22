<%@ Page Language="C#" MasterPageFile="~/UQiLabServiceBroker.Master" AutoEventWireup="true"
    CodeBehind="MyClient.aspx.cs" Inherits="iLabs.ServiceBroker.iLabSB.MyClient"
    Title="My Client" %>

<%@ MasterType VirtualPath="~/UQiLabServiceBroker.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="myclient">
        <table>
            <tr>
                <td valign="top">
                    <!-- Div id "singlelab" is displayed if only one client is available. Otherwise, div class "group" is displayed, which has a list of available labs. -->
                    <div class="singlelab-left">
                        <h3>
                            Group:
                            <asp:Label ID="lblGroupNameTitle" runat="server"></asp:Label>
                        </h3>
                        <h3>
                            LabClient:
                            <asp:Label ID="lblClientName" runat="server"></asp:Label>
                        </h3>
                        <p>
                            <strong>Version:</strong>
                            <asp:Label ID="lblVersion" runat="server"></asp:Label>
                        </p>
                        <p>
                            <strong>Description:</strong>
                            <asp:Label ID="lblLongDescription" runat="server"></asp:Label>
                        </p>
                        <p>
                            <asp:Label ID="lblNotes" runat="server"></asp:Label>
                        </p>
                        <p>
                            <asp:Label ID="lblDocURL" runat="server"></asp:Label>
                        </p>
                        <p>
                            <strong>Contact Email:</strong>
                            <asp:Label ID="lblEmail" runat="server"></asp:Label>
                        </p>
                        <p>
                            <asp:Button ID="btnLaunchLab" runat="server" CssClass="button" Text="Launch Lab"
                                OnClick="btnLaunchLab_Click" Visible="false"></asp:Button>
                            <asp:Button ID="btnReenter" runat="server" CssClass="button" Text="Re-enter Experiment"
                                OnClick="btnReenter_Click" Visible="false" Width="171px"></asp:Button>
                            <asp:Button ID="btnSchedule" runat="server" CssClass="button" Text="Schedule/Redeem Session"
                                OnClick="btnSchedule_Click" Visible="false" Width="170px"></asp:Button>&nbsp;
                        </p>
                        <asp:Repeater ID="repClientInfos" runat="server">
                        </asp:Repeater>
                        <asp:PlaceHolder ID="phBatchApplet" runat="server"></asp:PlaceHolder>
                    </div>
                </td>
                <td valign="top">
                    <div id="messagebox-right">
                        <h3>
                            <asp:Label ID="lblGroupNameSystemMessage" runat="server"></asp:Label>
                        </h3>
                        <asp:Label ID="lblNoMessages" runat="server" Text="<p>No messages at this time.</p>"></asp:Label>
                        <asp:Repeater ID="repSystemMessage" runat="server">
                            <ItemTemplate>
                                <p class="message">
                                    <%# Convert.ToString(DataBinder.Eval(Container.DataItem, "MessageBody")) %>
                                </p>
                                <p class="date">
                                    Date Posted:
                                    <%# Convert.ToString(DataBinder.Eval(Container.DataItem, "LastModified")) %>
                                </p>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
