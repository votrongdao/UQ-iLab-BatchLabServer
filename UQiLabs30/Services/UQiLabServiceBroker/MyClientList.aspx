<%@ Page Language="C#" MasterPageFile="~/UQiLabServiceBroker.Master" AutoEventWireup="true"
    CodeBehind="MyClientList.aspx.cs" Inherits="iLabs.ServiceBroker.iLabSB.MyClientList"
    Title="My Labs" %>

<%@ MasterType VirtualPath="~/UQiLabServiceBroker.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="myclientlist">
        <!-- Div id "singlelab" is displayed if only one client is available. Otherwise, div class "group" is displayed, which has a list of available labs. -->
        <table>
            <tr>
                <td valign="top">
                    <div class="group-left">
                        <h3>
                            Group:
                            <asp:Label ID="lblGroupNameTitle" runat="server"></asp:Label>
                        </h3>
                        <h3>
                            Labs for
                            <asp:Label ID="lblGroupNameLabList" runat="server"></asp:Label>
                        </h3>
                        <asp:Repeater ID="repLabs" runat="server" OnItemCommand="repLabs_ItemCommand">
                            <ItemTemplate>
                                <p>
                                    <strong>
                                        <asp:LinkButton ID="lblLabs" runat="server" CommandName="SetLabClient">
												<%# Convert.ToString(DataBinder.Eval(Container.DataItem, "ClientName")) %>
                                        </asp:LinkButton>
                                        - </strong>
                                    <%# Convert.ToString(DataBinder.Eval(Container.DataItem, "ClientShortDescription")) %>
                                </p>
                            </ItemTemplate>
                        </asp:Repeater>
                        <p />
                    </div>
                </td>
                <td valign="top">
                    <div id="messagebox-right">
                        <h3>
                            Messages for
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
