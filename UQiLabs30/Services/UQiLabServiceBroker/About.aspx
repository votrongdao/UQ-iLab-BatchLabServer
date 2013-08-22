<%@ Page Language="C#" MasterPageFile="~/UQiLabServiceBroker.Master" AutoEventWireup="true"
    CodeBehind="About.aspx.cs" Inherits="iLabs.ServiceBroker.iLabSB.About" Title="About" %>

<%@ MasterType VirtualPath="~/UQiLabServiceBroker.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <p>
        <b><i>iLabs</i></b> is dedicated to the proposition that online laboratories - real
        laboratories accessed through the Internet - can enrich science and engineering
        education by greatly expanding the range of experiments that students are exposed
        to in the course of their education. Unlike conventional laboratories, iLabs can
        be shared across a university or across the world. The iLabs vision is to share
        expensive equipment and educational materials associated with lab experiments as
        broadly as possible within higher education and beyond.</p>
    <p>
        iLab teams have created remote laboratories at MIT in microelectronics, chemical
        engineering, polymer crystallization, structural engineering, and signal processing
        as case studies for understanding the complex requirements of operating remote lab
        experiments and scaling their use to large groups of students at MIT and around
        the world.</p>
    <p>
        Based on the experiences of the different iLab development teams, <i>The iLabs Project</i>
        is developing a suite of software tools that makes it efficient to bring online
        complex laboratory experiments, and provides the infrastructure for user management.
        The
        <nobr>
            <a href="http://icampus.mit.edu/ilabs/architecture">iLabs Shared Architecture</a></nobr>
        has the following design goals:</p>
    <ul>
        <li>Minimize development and management effort for users and providers of remote labs.</li>
        <li>Provide a common set of services and development tools.</li>
        <li>Scale to large numbers of users worldwide.</li>
        <li>Allow multiple universities with diverse network infrastructures to share remote
            labs.</li>
    </ul>
</asp:Content>
