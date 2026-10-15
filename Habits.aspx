<%@ Page Title="Habits" Language="C#" MasterPageFile="~/MasterUser.master" AutoEventWireup="true" CodeBehind="Habits.aspx.cs" Inherits="HabiTrackers.Habits" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div class="page-container">
        <!-- Badgeses Summary with Counts -->
        <div class="card" style="margin-bottom:2rem;">
            <h2>Your Badges</h2>
            <div style="display:flex; align-items:center; justify-content:space-between; flex-wrap:wrap; gap:1rem;">
                <div style="display:flex; gap:0.5rem;">
                    <asp:LinkButton ID="btnBronze" runat="server" OnClick="btnBronze_Click" 
                        style="background:#cd7f32; color:white; padding:0.5rem 1.5rem; border-radius:20px; text-decoration:none; display:inline-block;">
                         Bronze (<asp:Label ID="lblBronzeCount" runat="server" Text="0" />)
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnSilver" runat="server" OnClick="btnSilver_Click" 
                        style="background:#c0c0c0; color:black; padding:0.5rem 1.5rem; border-radius:20px; text-decoration:none; display:inline-block;">
                         Silver (<asp:Label ID="lblSilverCount" runat="server" Text="0" />)
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnGold" runat="server" OnClick="btnGold_Click" 
                        style="background:#ffd700; color:black; padding:0.5rem 1.5rem; border-radius:20px; text-decoration:none; display:inline-block;">
                         Gold (<asp:Label ID="lblGoldCount" runat="server" Text="0" />)
                    </asp:LinkButton>
                </div>
                <div>
                    <span style="font-size:1.2rem;">Total Badges: </span>
                    <asp:Label ID="lblBatchCount" runat="server" Text="0" style="font-size:1.5rem; font-weight:bold;"></asp:Label>
                </div>
            </div>
        </div>

        <!-- Badge Details Panel -->
        <asp:Panel ID="pnlBatchDetails" runat="server" Visible="false" CssClass="card" style="margin-bottom:2rem;">
            <div style="display:flex; justify-content:space-between; align-items:center;">
                <h2 id="batchTitle" runat="server">Batch Details</h2>
                <asp:LinkButton ID="btnCloseDetails" runat="server" Text="✖ Close" OnClick="btnCloseDetails_Click" 
                    style="color:#dc3545; text-decoration:none; font-size:1.2rem;" />
            </div>
            <asp:GridView ID="gvBatchDetails" runat="server" AutoGenerateColumns="False" CssClass="table" 
                GridLines="None" EmptyDataText="No habits found for this batch.">
                <Columns>
                    <asp:BoundField DataField="HabitName" HeaderText="Habit Name" />
                    <asp:BoundField DataField="Level" HeaderText="Level" />
                    <asp:BoundField DataField="TargetDays" HeaderText="Target Days" />
                    <asp:BoundField DataField="TotalCompletions" HeaderText="Days Completed" />
                    <asp:BoundField DataField="AwardedDate" HeaderText="Awarded Date" DataFormatString="{0:dd MMM yyyy}" />
                </Columns>
            </asp:GridView>
        </asp:Panel>

        <!-- Add New Habit -->
        <div class="card" style="margin-bottom:2rem;">
            <h2>Add New Habit</h2>
            <div style="display:flex; gap:1rem; align-items:flex-end;">
                <div style="flex:2;">
                    <label>Habit Name</label>
                        <asp:TextBox ID="txtHabitName" runat="server" CssClass="form-control" 
                placeholder="e.g., Morning Meditation, Daily Exercise, Read 30 mins" />                </div>
                <div style="flex:1;">
                    <label>Level</label>
                    <asp:DropDownList ID="ddlLevel" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Easy (21 days)" Value="Easy" />
                        <asp:ListItem Text="Medium (30 days)" Value="Medium" />
                        <asp:ListItem Text="Hard (45 days)" Value="Hard" />
                    </asp:DropDownList>
                </div>
                <div>
                    <asp:Button ID="btnAddHabit" runat="server" Text="Add Habit" OnClick="btnAddHabit_Click" CssClass="btn-primary" />
                </div>
            </div>
            <asp:Label ID="lblAddHabitMessage" runat="server" ForeColor="Red" style="margin-top:0.5rem; display:block;" />
        </div>

        <!-- Habits List -->
        <div class="card">
            <h2>Your Habits</h2>
            <asp:GridView ID="gvHabits" runat="server" AutoGenerateColumns="False" CssClass="table" 
                OnRowCommand="gvHabits_RowCommand" OnRowDataBound="gvHabits_RowDataBound"
                GridLines="None" DataKeyNames="HabitId">
                <Columns>
                    <asp:BoundField DataField="HabitName" HeaderText="Habit" />
                    <asp:BoundField DataField="Level" HeaderText="Level" />
                    <asp:BoundField DataField="TargetDays" HeaderText="Target Days" />
                    <asp:BoundField DataField="TotalCompletions" HeaderText="Days Completed" />
                    <asp:TemplateField HeaderText="Today">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnComplete" runat="server" 
                                CommandName="CompleteHabit" 
                                CommandArgument='<%# Eval("HabitId") %>'
                                Text='<%# Convert.ToBoolean(Eval("CompletedToday")) ? "✓ Completed" : "Mark Complete" %>'
                                CssClass='<%# Convert.ToBoolean(Eval("CompletedToday")) ? "badge-success" : "btn-complete" %>'
                                Enabled='<%# !Convert.ToBoolean(Eval("CompletedToday")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Badge">
                        <ItemTemplate>
                            <asp:Literal ID="litBatch" runat="server" />
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:LinkButton ID="lnkDelete" runat="server" CommandName="DeleteHabit" 
                                CommandArgument='<%# Eval("HabitId") %>' Text="Delete" 
                                OnClientClick="return confirm('Delete this habit?');" style="color:#dc3545;" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <asp:Label ID="lblMessage" runat="server" />
        </div>
    </div>
</asp:Content>