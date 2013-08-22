<%@ Page Language="C#" MasterPageFile="~/UQiLabServiceBroker.Master" AutoEventWireup="true"
    CodeBehind="ShowExperiment.aspx.cs" Inherits="iLabs.ServiceBroker.iLabSB.ShowExperiment"
    Title="Show Experiment" %>

<%@ MasterType VirtualPath="~/UQiLabServiceBroker.Master" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="showexperiment">
        <h2>
            Experiment Information</h2>
        <asp:Label ID="lblResponse" runat="server" Visible="False"></asp:Label>
        <table cols="2" cellspacing="2">
            <tr>
                <td class="textlabel">
                    <nobr>
                        Experiment ID:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtExperimentID" runat="server" CssClass="textbox-short" ReadOnly="true"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    <nobr>
                        LabClient Name:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtClientName" runat="server" CssClass="textbox-long" ReadOnly="true"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    <nobr>
                        LabServer Name:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtLabServerName" runat="server" CssClass="textbox-long" ReadOnly="true"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    <nobr>
                        User Name:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtUsername" runat="server" CssClass="textbox-short" ReadOnly="true"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    <nobr>
                        Effective Group:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtGroupName" runat="server" CssClass="textbox-long" ReadOnly="true"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    <nobr>
                        Status:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtStatus" runat="server" CssClass="textbox-short" ReadOnly="true"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    <nobr>
                        Submission Time:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtSubmissionTime" runat="server" CssClass="textbox-short" ReadOnly="true"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    <nobr>
                        Completion Time:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtCompletionTime" runat="server" CssClass="textbox-short" ReadOnly="true"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    <nobr>
                        Total Records:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtRecordCount" runat="server" CssClass="textbox-short" ReadOnly="true"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    <nobr>
                        Annotation:</nobr>
                </td>
                <td>
                    <asp:TextBox ID="txtAnnotation" runat="server" CssClass="textbox-long" ReadOnly="true"
                        TextMode="MultiLine" Rows="5"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    &nbsp;
                </td>
                <td>
                    <asp:Button ID="btnSaveAnnotation" CssClass="button" runat="server" Text="Save Annotation"
                        OnClick="btnSaveAnnotation_Click"></asp:Button>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    &nbsp;
                </td>
                <td class="button">
                    <asp:Button ID="btnDeleteExperiment" CssClass="button" runat="server" Text="Delete Experiment"
                        OnClick="btnDeleteExperiment_Click"></asp:Button>
                </td>
            </tr>
            <tr>
                <td class="textlabel">
                    &nbsp;
                </td>
                <td>
                    <asp:CheckBox ID="cbxContents" runat="server" TextAlign="Right" Text="Data Only">
                    </asp:CheckBox>
                    &nbsp;&nbsp;
                    <asp:Button ID="btnDisplayRecords" runat="server" Text="Get Records" CssClass="button"
                        OnClick="btnDisplayRecords_Click"></asp:Button>
                </td>
            </tr>
        </table>
        <div id="divRecords" runat="server">
            <p>
                &nbsp;</p>
            <h4>
                Experiment Records</h4>
            <asp:TextBox ID="txtExperimentRecords" runat="server" Width="700px" Height="156px"
                TextMode="MultiLine"></asp:TextBox>
            <asp:GridView ID="grvExperimentRecords" runat="server" Width="700px" CellPadding="5"
                AutoGenerateColumns="False" HeaderStyle-Font-Bold="true" BorderColor="black">
                <Columns>
                    <asp:BoundField DataField="Seq_Num" HeaderText="Seq_Num" ReadOnly="True">
                        <HeaderStyle Font-Bold="True" HorizontalAlign="center" Width="80px" Wrap="False" />
                        <ItemStyle HorizontalAlign="Right" Wrap="False" />
                    </asp:BoundField>
                    <asp:BoundField DataField="Record Type" HeaderText="Record Type" ReadOnly="True">
                        <HeaderStyle Font-Bold="True" HorizontalAlign="center" Width="200px" />
                        <ItemStyle HorizontalAlign="Left" />
                    </asp:BoundField>
                    <asp:BoundField DataField="Contents" HeaderText="Data" ReadOnly="True">
                        <HeaderStyle Font-Bold="True" HorizontalAlign="left" Width="420px" Wrap="True" />
                        <ItemStyle HorizontalAlign="Left" Width="420px" Wrap="True" />
                    </asp:BoundField>
                </Columns>
            </asp:GridView>
        </div>
        <div id="divBlobs" runat="server" visible="false">
            <p>
                &nbsp;</p>
            <h4>
                Experiment BLOBS</h4>
            <asp:GridView ID="grvBlobs" runat="server" CellPadding="5" Width="700px" AutoGenerateColumns="False"
                HeaderStyle-Font-Bold="True" OnRowDataBound="On_BindBlobRow" OnRowCommand="On_BlobSelected"
                HeaderStyle-HorizontalAlign="Center" BorderColor="black">
                <Columns>
                    <asp:TemplateField HeaderText="Select" HeaderStyle-HorizontalAlign="Center">
                        <HeaderStyle Font-Bold="True" HorizontalAlign="Center" Width="80px" Wrap="False" />
                        <ItemStyle HorizontalAlign="Center" />
                        <ItemTemplate>
                            <asp:Button ID="Button1" runat="server" CausesValidation="false" CommandName="" CommandArgument='<%# Eval("Blob_ID") %>'
                                Text='<%# Eval("Blob_ID") %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Seq_Num" HeaderText="Seq_Num" ReadOnly="True">
                        <HeaderStyle Font-Bold="True" HorizontalAlign="center" Width="80px" Wrap="False" />
                        <ItemStyle HorizontalAlign="Right" Wrap="False" />
                    </asp:BoundField>
                    <asp:BoundField DataField="MimeType" HeaderText="MimeType" ReadOnly="True">
                        <HeaderStyle Font-Bold="True" HorizontalAlign="center" Width="200px" />
                        <ItemStyle HorizontalAlign="Left" />
                    </asp:BoundField>
                    <asp:BoundField DataField="Description" HeaderText="Description" ReadOnly="True">
                        <HeaderStyle Font-Bold="True" HorizontalAlign="left" Width="440px" Wrap="True" />
                        <ItemStyle HorizontalAlign="Left" Wrap="true" />
                    </asp:BoundField>
                </Columns>
                <HeaderStyle Font-Bold="True" />
            </asp:GridView>
        </div>
    </div>
</asp:Content>
