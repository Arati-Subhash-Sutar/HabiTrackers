<%@ Page Title="Daily Tasks" Language="C#" MasterPageFile="~/MasterUser.master" AutoEventWireup="true" CodeBehind="DailyTasks.aspx.cs" Inherits="HabiTrackers.DailyTasks" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div class="page-container">
        <div class="card">
            <h2>Daily Tasks</h2>
            
            <!-- Search - Fixed button sizes -->
            <div style="margin-bottom:1.5rem; display:flex; gap:10px; align-items:center;">
                <asp:TextBox ID="txtSearch" runat="server" placeholder="Search tasks..." CssClass="form-control" style="width:300px;" />
                <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" CssClass="btn-primary" style="padding:10px 20px;" />
                <asp:Button ID="btnClear" runat="server" Text="Clear" OnClick="btnClear_Click" CssClass="btn-secondary" style="padding:10px 20px;" />
            </div>

            <!-- Add Task -->
            <div style="margin-bottom:1.5rem;">
                <asp:TextBox ID="txtTaskName" runat="server" placeholder="New task name" CssClass="form-control" style="width:300px; display:inline-block;"></asp:TextBox>
                <asp:TextBox ID="txtTaskDesc" runat="server" placeholder="Description (optional)" CssClass="form-control" style="width:300px; display:inline-block; margin-left:10px;"></asp:TextBox>
                <asp:Label ID="lblReminder" runat="server" Text="Reminder:" style="margin-left:10px;" />
                <asp:TextBox ID="txtReminder" runat="server" TextMode="DateTimeLocal" CssClass="form-control" style="width:220px; display:inline-block;" />
                <asp:Button ID="btnAddTask" runat="server" Text="Add Task" OnClick="btnAddTask_Click" CssClass="btn-primary" style="margin-left:10px; padding:10px 20px;" />
            </div>
            
            <!-- Pending Tasks Section -->
            <h3 style="margin-top:2rem; margin-bottom:1rem; color:#01889f;">Pending Tasks</h3>
            <asp:GridView ID="gvPendingTasks" runat="server" AutoGenerateColumns="false" CssClass="table" 
                OnRowCommand="gvPendingTasks_RowCommand" OnRowEditing="gvPendingTasks_RowEditing" 
                OnRowCancelingEdit="gvPendingTasks_RowCancelingEdit" OnRowUpdating="gvPendingTasks_RowUpdating" 
                DataKeyNames="TaskId" GridLines="None" AllowPaging="true" PageSize="10" 
                OnPageIndexChanging="gvPendingTasks_PageIndexChanging" EmptyDataText="No pending tasks.">
                <Columns>
                    <asp:TemplateField HeaderText="Complete">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkCompleted" runat="server" Checked='<%# Convert.ToBoolean(Eval("IsCompleted")) %>' 
                                AutoPostBack="true" OnCheckedChanged="chkCompleted_CheckedChanged" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="TaskName" HeaderText="Task" SortExpression="TaskName" />
                    <asp:BoundField DataField="Description" HeaderText="Description" />
                    <asp:BoundField DataField="TaskDate" HeaderText="Date" DataFormatString="{0:dd/MM/yyyy}" SortExpression="TaskDate" />
                    <asp:TemplateField HeaderText="Reminder">
                        <ItemTemplate>
                            <%# Eval("ReminderTime") != DBNull.Value ? Convert.ToDateTime(Eval("ReminderTime")).ToString("dd/MM/yyyy HH:mm") : "" %>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="txtEditReminder" runat="server" TextMode="DateTimeLocal" Text='<%# Bind("ReminderTime", "{0:yyyy-MM-ddTHH:mm}") %>' CssClass="form-control" />
                        </EditItemTemplate>
                    </asp:TemplateField>
                    <asp:CommandField ShowEditButton="true" ButtonType="Link" EditText="Edit" CancelText="Cancel" UpdateText="Update" />
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkDelete" runat="server" CommandName="DeleteTask" 
                                CommandArgument='<%# Eval("TaskId") %>' Text="Delete" style="color:#dc3545;" OnClientClick="return confirm('Delete this task?');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

            <!-- Completed Tasks Section (Read-only) -->
            <h3 style="margin-top:2rem; margin-bottom:1rem; color:#01889f;">Completed Tasks</h3>
            <asp:GridView ID="gvCompletedTasks" runat="server" AutoGenerateColumns="false" CssClass="table" 
                OnRowCommand="gvCompletedTasks_RowCommand"
                DataKeyNames="TaskId" GridLines="None" AllowPaging="true" PageSize="10" 
                OnPageIndexChanging="gvCompletedTasks_PageIndexChanging" EmptyDataText="No completed tasks.">
                <Columns>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span style="color:#01889f; font-weight:bold;">✓ Completed</span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="TaskName" HeaderText="Task" SortExpression="TaskName" />
                    <asp:BoundField DataField="Description" HeaderText="Description" />
                    <asp:BoundField DataField="TaskDate" HeaderText="Date" DataFormatString="{0:dd/MM/yyyy}" SortExpression="TaskDate" />
                    <asp:TemplateField HeaderText="Reminder">
                        <ItemTemplate>
                            <%# Eval("ReminderTime") != DBNull.Value ? Convert.ToDateTime(Eval("ReminderTime")).ToString("dd/MM/yyyy HH:mm") : "" %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkDelete" runat="server" CommandName="DeleteTask" 
                                CommandArgument='<%# Eval("TaskId") %>' Text="Delete" style="color:#dc3545;" OnClientClick="return confirm('Delete this task?');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            
            <asp:Label ID="lblMessage" runat="server" style="color:#01889f; font-weight:bold;"></asp:Label>
        </div>
    </div>
</asp:Content>