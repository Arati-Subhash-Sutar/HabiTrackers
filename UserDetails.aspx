<%@ Page Title="User Details" Language="C#" MasterPageFile="~/MasterAdmin.master" AutoEventWireup="true" CodeBehind="UserDetails.aspx.cs" Inherits="HabiTrackers.UserDetails" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .detail-card {
            background: white;
            border-radius: 15px;
            padding: 25px;
            margin-bottom: 20px;
            box-shadow: 0 5px 20px rgba(0,0,0,0.1);
        }
        .detail-header {
            background: linear-gradient(135deg, #01889f 0%, #016c7e 100%);
            color: white;
            padding: 20px;
            border-radius: 15px;
            margin-bottom: 20px;
        }
        .detail-section {
            margin-bottom: 25px;
            border: 1px solid #e0e0e0;
            border-radius: 10px;
            overflow: hidden;
            background: white;
        }
        .detail-section h3 {
            background: #01889f;
            color: white;
            padding: 12px 20px;
            margin: 0;
            font-size: 1.1rem;
        }
        .detail-section .content {
            padding: 20px;
            background: white;
        }
        .stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
        }
        .stat-card {
            background: #f8f9fa;
            padding: 15px;
            border-radius: 10px;
            text-align: center;
            border-left: 4px solid #01889f;
        }
        .stat-number {
            font-size: 28px;
            font-weight: bold;
            color: #01889f;
        }
        .stat-label {
            font-size: 12px;
            color: #666;
            text-transform: uppercase;
            margin-top: 5px;
        }
        .badge-batch {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 20px;
            font-size: 12px;
            margin-right: 8px;
        }
        .badge-bronze { background: #cd7f32; color: white; }
        .badge-silver { background: #c0c0c0; color: black; }
        .badge-gold { background: #ffd700; color: black; }
        .habit-list, .task-list, .note-list {
            max-height: 300px;
            overflow-y: auto;
        }
        .habit-item, .task-item, .note-item {
            background: #f8f9fa;
            padding: 10px 15px;
            margin: 8px 0;
            border-radius: 8px;
            border-left: 3px solid #01889f;
        }
        .back-btn {
            background: #6c757d;
            color: white;
            padding: 10px 20px;
            border-radius: 8px;
            text-decoration: none;
            display: inline-block;
            margin-bottom: 20px;
        }
        .back-btn:hover {
            background: #5a6268;
            color: white;
        }
        .streak-number {
            font-size: 36px;
            font-weight: bold;
            color: #ffc107;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-container">
        <a href="AdminDashboard.aspx" class="back-btn">← Back to Dashboard</a>
        
        <div class="detail-header">
            <h2>User Profile: <asp:Label ID="lblUsername" runat="server" /></h2>
            <p>User ID: <asp:Label ID="lblUserId" runat="server" /></p>
            <p>Email: <asp:Label ID="lblEmail" runat="server" /></p>
            <p>Role: <asp:Label ID="lblRole" runat="server" /></p>
            <p>Registered: <asp:Label ID="lblRegistered" runat="server" /></p>
            <p>Last Login: <asp:Label ID="lblLastLogin" runat="server" /></p>
        </div>

        <!-- Streak Information -->
        <div class="detail-section">
            <h3> Streak Information</h3>
            <div class="content">
                <div class="stats-grid">
                    <div class="stat-card">
                        <div class="stat-number"><asp:Label ID="lblCurrentStreak" runat="server" /> days</div>
                        <div class="stat-label">Current Streak</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-number"><asp:Label ID="lblLongestStreak" runat="server" /> days</div>
                        <div class="stat-label">Longest Streak</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-number"><asp:Label ID="lblTodayHabits" runat="server" /></div>
                        <div class="stat-label">Habits Completed Today</div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Tasks Summary -->
        <div class="detail-section">
            <h3> Tasks</h3>
            <div class="content">
                <div class="stats-grid">
                    <div class="stat-card">
                        <div class="stat-number"><asp:Label ID="lblTotalTasks" runat="server" /></div>
                        <div class="stat-label">Total Tasks</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-number"><asp:Label ID="lblPendingTasks" runat="server" /></div>
                        <div class="stat-label">Pending Tasks</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-number"><asp:Label ID="lblCompletedTasks" runat="server" /></div>
                        <div class="stat-label">Completed Tasks</div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Habits & Batches -->
        <div class="detail-section">
            <h3> Habits & Badges</h3>
            <div class="content">
                <div class="stats-grid">
                    <div class="stat-card">
                        <div class="stat-number"><asp:Label ID="lblTotalHabits" runat="server" /></div>
                        <div class="stat-label">Total Habits</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-number"><asp:Label ID="lblTotalCompletions" runat="server" /></div>
                        <div class="stat-label">Total Completions</div>
                    </div>
                </div>
                
                <div style="margin-top: 15px;">
                    <strong>Badges Earned:</strong><br />
                    <asp:PlaceHolder ID="phBatches" runat="server" />
                </div>

                <div style="margin-top: 15px;">
                    <strong>Habit Details:</strong>
                    <div class="habit-list">
                        <asp:Repeater ID="rptHabits" runat="server">
                            <ItemTemplate>
                                <div class="habit-item">
                                    <strong><%# Eval("HabitName") %></strong><br />
                                    Level: <%# Eval("Level") %> | Target: <%# Eval("TargetDays") %> days | Completed: <%# Eval("TotalCompletions") %> days
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:Label ID="lblNoHabits" runat="server" Text="No habits found." Visible='<%# rptHabits.Items.Count == 0 %>' />
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>
        </div>

        <!-- Notes Summary -->
        <div class="detail-section">
            <h3> Notes</h3>
            <div class="content">
                <div class="stats-grid">
                    <div class="stat-card">
                        <div class="stat-number"><asp:Label ID="lblTotalNotes" runat="server" /></div>
                        <div class="stat-label">Total Notes</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-number"><asp:Label ID="lblRecentNotes" runat="server" /></div>
                        <div class="stat-label">Recent (30 days)</div>
                    </div>
                </div>
                <div style="margin-top: 15px;">
                    <strong>Recent Notes:</strong>
                    <div class="note-list">
                        <asp:Repeater ID="rptNotes" runat="server">
                            <ItemTemplate>
                                <div class="note-item">
                                    <strong><%# Eval("NoteTitle") %></strong><br />
                                    <small><%# Eval("CreatedDate", "{0:dd MMM yyyy HH:mm}") %></small>
                                    <p><%# Eval("NoteContent").ToString().Length > 100 ? Eval("NoteContent").ToString().Substring(0, 100) + "..." : Eval("NoteContent") %></p>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:Label ID="lblNoNotes" runat="server" Text="No notes found." Visible='<%# rptNotes.Items.Count == 0 %>' />
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>
        </div>

        <!-- Timetable Summary -->
        <div class="detail-section">
            <h3> Timetable</h3>
            <div class="content">
                <div class="stats-grid">
                    <div class="stat-card">
                        <div class="stat-number"><asp:Label ID="lblTotalTimetableEntries" runat="server" /></div>
                        <div class="stat-label">Total Entries</div>
                    </div>
                </div>
                <div style="margin-top: 15px;">
                    <strong>Timetable Entries:</strong>
                    <div class="habit-list">
                        <asp:Repeater ID="rptTimetable" runat="server">
                            <ItemTemplate>
                                <div class="habit-item">
                                    <strong><%# Eval("Day") %></strong> - <%# Eval("StartTime") %> to <%# Eval("EndTime") %><br />
                                    <%# Eval("Task") %> <%# Convert.ToBoolean(Eval("IsBreak")) ? "(Break)" : "" %>
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:Label ID="lblNoTimetable" runat="server" Text="No timetable entries found." Visible='<%# rptTimetable.Items.Count == 0 %>' />
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>
        </div>

        <!-- Study Timer Summary -->
        <div class="detail-section">
            <h3> Study Timer</h3>
            <div class="content">
                <div class="stats-grid">
                    <div class="stat-card">
                        <div class="stat-number"><asp:Label ID="lblTotalStudySessions" runat="server" /></div>
                        <div class="stat-label">Total Sessions</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-number"><asp:Label ID="lblTotalStudyTime" runat="server" /> mins</div>
                        <div class="stat-label">Total Time</div>
                    </div>
                </div>
                <div style="margin-top: 15px;">
                    <strong>Recent Study Sessions:</strong>
                    <div class="habit-list">
                        <asp:Repeater ID="rptStudySessions" runat="server">
                            <ItemTemplate>
                                <div class="habit-item">
                                    <strong><%# Eval("SessionDate", "{0:dd MMM yyyy}") %></strong><br />
                                    Work: <%# Convert.ToInt32(Eval("TotalWorkTime")) / 60 %> min | Break: <%# Convert.ToInt32(Eval("TotalBreakTime")) / 60 %> min
                                </div>
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:Label ID="lblNoSessions" runat="server" Text="No study sessions found." Visible='<%# rptStudySessions.Items.Count == 0 %>' />
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>