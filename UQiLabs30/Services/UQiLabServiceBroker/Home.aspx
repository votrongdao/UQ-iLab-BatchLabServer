<%@ Page Language="C#" MasterPageFile="~/UQiLabServiceBroker.Master" AutoEventWireup="true"
    CodeBehind="Home.aspx.cs" Inherits="iLabs.ServiceBroker.iLabSB.Home" Title="Home" %>

<%@ MasterType VirtualPath="~/UQiLabServiceBroker.Master" %>
<%@ Register TagPrefix="uc" TagName="Login" Src="~/Controls/Login.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h3>
        Welcome to iLab
    </h3>
    <p>
        iLab is dedicated to the proposition that online laboratories - real laboratories
        accessed through the Internet - can enrich science and engineering education by
        greatly expanding the range of experiments that the students are exposed to in the
        course of their education.
    </p>
    <p>
        Unlike conventional laboratories, iLabs can be shared across a university or across
        the world. The iLab vision is to share lab experiments as broadly as possible within
        higher education and beyond. The ultimate goal of the iLab project is to create
        a rich set of experiment resources that make it easier for faculty members around
        the world to share their labs over the Internet.
    </p>
    <ul>
        <li><a href="About.aspx"><strong>Read more about iLab</strong></a></li>
    </ul>
    <p />
    <hr />
    <p />
    <% if (Session["UserID"] == null)
       {%>
    <table>
        <tr>
            <td valign="top">
                <uc:Login ID="Login1" runat="server" />
                <p>
                    Don't have an account yet?
                    <p />
                    <asp:Button ID="btnRegister" runat="server" CssClass="button" Text="Register" OnClick="btnRegister_Click" />
                </p>
                <p>
                    Forgot your <a href="LostPassword.aspx">password</a>?
                </p>
            </td>
            <td valign="top">
                <div id="messagebox-right">
                    <h3>
                        System News and Messages
                    </h3>
                    <asp:Label ID="lblNoMessages" runat="server"></asp:Label>
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
    <% } %>
</asp:Content>
