<%@ Page Language="C#" MasterPageFile="~/UQiLabServiceBroker.Master" AutoEventWireup="true"
    CodeBehind="MyGroups.aspx.cs" Inherits="iLabs.ServiceBroker.iLabSB.MyGroups"
    Title="My Groups" %>

<%@ MasterType VirtualPath="~/UQiLabServiceBroker.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table id="mygroups" cols="2" border="0" cellspacing="0" cellpadding="0">
        <tr>
            <td valign="top">
                <div class="group-left">
                    <h3>
                        Available Groups and Labs
                    </h3>
                    <asp:Label ID="lblNoGroups" runat="server"></asp:Label>
                    <asp:Repeater ID="repGroups" runat="server" OnItemCommand="repGroups_ItemCommand">
                        <ItemTemplate>
                            <p>
                                <strong>
                                    <asp:LinkButton runat="server" ID="lblGroups" CommandName="SetEffectiveGroup">
							<%# Convert.ToString(DataBinder.Eval(Container.DataItem, "groupName")) %>
                                    </asp:LinkButton>
                                    - </strong>
                                <%# Convert.ToString(DataBinder.Eval(Container.DataItem, "description")) %>
                            </p>
                            <p />
                        </ItemTemplate>
                    </asp:Repeater>
                    <p />
                </div>
            </td>
            <td valign="top">
                <div id="messagebox-right">
                    <h3>
                        More Information
                    </h3>
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
            </td>
        </tr>
    </table>
</asp:Content>
