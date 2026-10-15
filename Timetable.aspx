<%@ Page Title="Timetable" Language="C#" MasterPageFile="~/MasterUser.master" AutoEventWireup="true" CodeBehind="Timetable.aspx.cs" Inherits="HabiTrackers.Timetable" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div class="page-container">
        <div class="card">
            <h2>My Timetable</h2>
            
            <!-- Export Button -->
            <div style="text-align:right; margin-bottom:1rem;">
                <asp:Button ID="btnExportPDF" runat="server" Text="Export as PDF" OnClick="btnExportPDF_Click" CssClass="btn-primary" style="padding:10px 20px;" />
            </div>

            <!-- Add/Edit Entry Form -->
            <div style="margin-bottom:2rem; background:#f8f9fa; padding:1.5rem; border-radius:10px;">
                <h3 id="formTitle" runat="server">Add New Entry</h3>
                <asp:HiddenField ID="hfEntryId" runat="server" Value="0" />
                <div style="display:flex; gap:1rem; flex-wrap:wrap; align-items:center;">
                    <div>
                        <label>Day:</label>
                        <asp:DropDownList ID="ddlDay" runat="server" CssClass="form-control" style="width:120px;">
                            <asp:ListItem Text="Sunday" Value="Sunday" />
                            <asp:ListItem Text="Monday" Value="Monday" />
                            <asp:ListItem Text="Tuesday" Value="Tuesday" />
                            <asp:ListItem Text="Wednesday" Value="Wednesday" />
                            <asp:ListItem Text="Thursday" Value="Thursday" />
                            <asp:ListItem Text="Friday" Value="Friday" />
                            <asp:ListItem Text="Saturday" Value="Saturday" />
                        </asp:DropDownList>
                    </div>
                    <div>
                        <label>Start:</label>
                        <asp:TextBox ID="txtStartTime" runat="server" TextMode="Time" CssClass="form-control" style="width:120px;" />
                    </div>
                    <div>
                        <label>End:</label>
                        <asp:TextBox ID="txtEndTime" runat="server" TextMode="Time" CssClass="form-control" style="width:120px;" />
                    </div>
                    <div>
                        <label>Task:</label>
                        <asp:TextBox ID="txtTask" runat="server" placeholder="Task/Activity" CssClass="form-control" style="width:200px;" />
                    </div>
                    <div>
                        <asp:CheckBox ID="chkIsBreak" runat="server" Text="Break" style="margin-top:20px; display:block;" />
                    </div>
                    <div style="display:flex; gap:0.5rem;">
                        <asp:Button ID="btnSave" runat="server" Text="Save Entry" OnClick="btnSave_Click" CssClass="btn-primary" style="padding:10px 20px;" />
                        <asp:Button ID="btnUpdate" runat="server" Text="Update Entry" OnClick="btnUpdate_Click" CssClass="btn-primary" style="padding:10px 20px; background-color:#01889f;" Visible="false" />
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" CssClass="btn-secondary" style="padding:10px 20px;" Visible="false" CausesValidation="false" />
                    </div>
                </div>
                <asp:Label ID="lblMessage" runat="server" ForeColor="#01889f" style="margin-top:1rem; display:block;" />
            </div>

            <!-- Timetable Display with Edit/Delete buttons -->
            <div style="overflow-x:auto;">
                <asp:Repeater ID="rptTimetable" runat="server" OnItemCommand="rptTimetable_ItemCommand" OnItemDataBound="rptTimetable_ItemDataBound">
                    <HeaderTemplate>
                        <table class="table" style="width:100%; border-collapse:collapse;">
                            <thead>
                                <tr style="background:#01889f; color:white;">
                                    <th style="padding:10px; text-align:left;">Time</th>
                                    <th style="padding:10px; text-align:left;">Sun</th>
                                    <th style="padding:10px; text-align:left;">Mon</th>
                                    <th style="padding:10px; text-align:left;">Tue</th>
                                    <th style="padding:10px; text-align:left;">Wed</th>
                                    <th style="padding:10px; text-align:left;">Thu</th>
                                    <th style="padding:10px; text-align:left;">Fri</th>
                                    <th style="padding:10px; text-align:left;">Sat</th>
                                    <th style="padding:10px; text-align:left;">Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr style='border-bottom:1px solid #ddd;'>
                            <td style='padding:10px; font-weight:bold;'><%# Eval("TimeSlot") %></td>
                            <td id="cellSunday" runat="server" style='padding:10px;'><%# Eval("Sunday") %></td>
                            <td id="cellMonday" runat="server" style='padding:10px;'><%# Eval("Monday") %></td>
                            <td id="cellTuesday" runat="server" style='padding:10px;'><%# Eval("Tuesday") %></td>
                            <td id="cellWednesday" runat="server" style='padding:10px;'><%# Eval("Wednesday") %></td>
                            <td id="cellThursday" runat="server" style='padding:10px;'><%# Eval("Thursday") %></td>
                            <td id="cellFriday" runat="server" style='padding:10px;'><%# Eval("Friday") %></td>
                            <td id="cellSaturday" runat="server" style='padding:10px;'><%# Eval("Saturday") %></td>
                            <td style='padding:10px;'>
                                <asp:LinkButton ID="lnkEdit" runat="server" CommandName="EditEntry" 
                                    CommandArgument='<%# Eval("RowData") %>' Text="Edit" style="margin-right:10px; color:#01889f;" />
                                <asp:LinkButton ID="lnkDelete" runat="server" CommandName="DeleteEntry" 
                                    CommandArgument='<%# Eval("RowData") %>' Text="Delete" 
                                    OnClientClick="return confirm('Delete this entry?');" style="color:#dc3545;" />
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
                
                <asp:Label ID="lblNoData" runat="server" Text="No timetable entries yet. Add your first entry above!" 
                    Visible="false" style="text-align:center; color:#999; padding:2rem; display:block;" />
            </div>
        </div>
    </div>
</asp:Content>