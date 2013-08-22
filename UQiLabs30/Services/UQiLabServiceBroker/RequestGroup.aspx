<%@ Page Language="C#" MasterPageFile="~/UQiLabServiceBroker.Master" AutoEventWireup="true"
    CodeBehind="RequestGroup.aspx.cs" Inherits="iLabs.ServiceBroker.iLabSB.RequestGroup"
    Title="Request Group" %>

<%@ MasterType VirtualPath="~/UQiLabServiceBroker.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="requestgroup">
        <h2>
            Request Membership in a New Group</h2>
        <p>
            Select the group(s) you would like to join below.
        </p>
        <div id="actionbox-right">
            <h3>
                More Information</h3>
            <p class="message">
                You are currently a member of the following group(s):
            </p>
            <strong>
                <asp:Label ID="lblGroups" runat="server"></asp:Label></strong>
            <p class="message">
                You have requested membership in the following group(s):
            </p>
            <strong>
                <asp:Label ID="lblRequestGroups" runat="server"></asp:Label></strong>
        </div>
        <div class="group">
            <asp:Label ID="lblNoGroups" runat="server"></asp:Label>
            <asp:Repeater ID="repAvailableGroups" runat="server">
                <ItemTemplate>
                    <asp:CheckBox ID="cbxGroup" runat="server" CssClass="checkbox"></asp:CheckBox>
                    <label for="group1">
                        <%# Convert.ToString(DataBinder.Eval(Container.DataItem, "groupName")) %>
                    </label>
                </ItemTemplate>
                <SeparatorTemplate>
                    <p>
                    </p>
                </SeparatorTemplate>
            </asp:Repeater>
        </div>
        <asp:Button ID="btnRequestMembership" runat="server" Text="Request Membership" CssClass="button"
            OnClick="btnRequestMembership_Click"></asp:Button>
        <p />
        <asp:Label ID="lblResponse" runat="server" Visible="False"></asp:Label>
    </div>
</asp:Content>
