<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Login.ascx.cs" Inherits="iLabs.ServiceBroker.iLabSB.Controls.Login" %>

<script language="javascript">
        var visitortime = new Date();
        document.write('<input type="hidden" name="userTZ" id="userTZ"');
        if (visitortime) {
            document.write('value="' + -visitortime.getTimezoneOffset() + '">');
        }
        else {
            document.write('value="JavaScript not Date() enabled">');
        }
</script>

<table id="login" cols="2" border="0" cellspacing="0" cellpadding="0">
    <tr>
        <td class="label">
            Username:
        </td>
        <td class="textentry">
            <asp:TextBox ID="txtUsername" runat="server" CssClass="textbox"></asp:TextBox>
            <%--
                        <asp:RegularExpressionValidator ID="rexValidator" runat="server" ControlToValidate="txtUsername" ValidationExpression="\d{1,40}"
                ErrorMessage="Username cannot be longer than 40 characters"></asp:RegularExpressionValidator>
--%>
        </td>
    </tr>
    <tr>
        <td class="label">
            Password:
        </td>
        <td class="textentry">
            <asp:TextBox ID="txtPassword" runat="server" CssClass="textbox" TextMode="Password"></asp:TextBox>
        </td>
    </tr>
    <tr>
        <td>
            &nbsp;
        </td>
        <td>
            <asp:Button ID="btnLogIn" runat="server" Text="Login" CssClass="button" OnClick="btnLogIn_Click">
            </asp:Button>
        </td>
    </tr>
</table>
<table>
    <tr>
        <td>
            <p>
                <asp:Label ID="lblLoginErrorMessage" runat="server" Visible="False"></asp:Label>
            </p>
        </td>
    </tr>
</table>
