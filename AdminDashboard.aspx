<%@ Page Title="Admin Dashboard" Language="C#" MasterPageFile="~/MasterAdmin.master" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="HabiTrackers.AdminDashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
    <style>
        .clickable-row { cursor: pointer; transition: background 0.2s; }
        .clickable-row:hover { background: rgba(1, 136, 159, 0.1) !important; }
        .dashboard-title { color: white; margin-bottom: 2rem; font-size: 1.8rem; }
        .stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1.5rem; margin-bottom: 2rem; }
        .stat-card { text-align: center; padding: 1.5rem; background: rgba(255,255,255,0.95); border-radius: 15px; transition: transform 0.3s ease; cursor: pointer; }
        .stat-card:hover { transform: translateY(-5px); background: #f0f0f0; }
        .stat-card h3 { font-size: 0.9rem; color: #666; margin-bottom: 0.5rem; text-transform: uppercase; letter-spacing: 0.5px; }
        .stat-number { font-size: 2rem; font-weight: bold; color: #01889f; }
        .stat-sub { font-size: 0.8rem; color: #999; margin-top: 5px; }
        .search-bar { margin-bottom: 2rem; display: flex; gap: 1rem; align-items: center; justify-content: flex-end; }
        .charts-stack { display: flex; flex-direction: column; gap: 1.5rem; margin-bottom: 2rem; }
        .chart-card { padding: 1.5rem; width: 100%; }
        .users-card { padding: 1.5rem; overflow-x: auto; }
        .table { width: 100%; border-collapse: collapse; }
        .table th, .table td { padding: 12px; text-align: left; border-bottom: 1px solid #e1e5e9; }
        .table th { background: #f8f9fa; color: #01889f; font-weight: 600; }
        .btn-delete { background: #dc3545; color: white; border: none; padding: 5px 12px; border-radius: 5px; cursor: pointer; }
        .btn-delete:hover { background: #c82333; }
        @media (max-width: 768px) { .stats-grid { grid-template-columns: repeat(2, 1fr); } }
    </style>
    <script>
        function openDetails(type) {
            var searchDate = document.getElementById('<%= txtSearchDate.ClientID %>').value;
            if (!searchDate) {
                // If no date selected, use today's date
                var today = new Date();
                var dd = String(today.getDate()).padStart(2, '0');
                var mm = String(today.getMonth() + 1).padStart(2, '0');
                var yyyy = today.getFullYear();
                searchDate = yyyy + '-' + mm + '-' + dd;
            }
            window.location = 'AdminStatsDetails.aspx?type=' + type + '&date=' + searchDate;
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-container">
        <h1 class="dashboard-title">Admin Dashboard</h1>

        <!-- Date Search Bar -->
        <div class="search-bar">
            <asp:TextBox ID="txtSearchDate" runat="server" TextMode="Date" CssClass="form-control" style="width:200px;" />
            <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" CssClass="btn-primary" />
            <asp:Button ID="btnReset" runat="server" Text="Reset" OnClick="btnReset_Click" CssClass="btn-secondary" />
        </div>

        <!-- Stats Cards - Daily + Total -->
        <div class="stats-grid">
            <div class="card stat-card" onclick="openDetails('logins');">
                <h3>Today's Logins</h3>
                <div class="stat-number" runat="server" id="todayLogins">0</div>
                <div class="stat-sub">Total Logins: <asp:Label ID="totalLogins" runat="server" Text="0" /></div>
            </div>
            <div class="card stat-card" onclick="openDetails('newusers');">
                <h3>New Users Today</h3>
                <div class="stat-number" runat="server" id="newUsers">0</div>
                <div class="stat-sub">Total Users: <asp:Label ID="totalUsers" runat="server" Text="0" /></div>
            </div>
            <div class="card stat-card" onclick="openDetails('reminders');">
                <h3>Reminders Sent Today</h3>
                <div class="stat-number" runat="server" id="remindersSent">0</div>
                <div class="stat-sub">Total Reminders: <asp:Label ID="totalReminders" runat="server" Text="0" /></div>
            </div>
            <div class="card stat-card" onclick="openDetails('habits');">
                <h3>Habits Completed Today</h3>
                <div class="stat-number" runat="server" id="habitsCompleted">0</div>
                <div class="stat-sub">Total Habits: <asp:Label ID="totalHabits" runat="server" Text="0" /></div>
            </div>
            <div class="card stat-card" onclick="openDetails('tasks');">
                <h3>Tasks Completed Today</h3>
                <div class="stat-number" runat="server" id="tasksCompleted">0</div>
                <div class="stat-sub">Total Tasks: <asp:Label ID="totalTasks" runat="server" Text="0" /></div>
            </div>
            <div class="card stat-card" onclick="openDetails('notes');">
                <h3>Notes Added Today</h3>
                <div class="stat-number" runat="server" id="notesToday">0</div>
                <div class="stat-sub">Total Notes: <asp:Label ID="totalNotes" runat="server" Text="0" /></div>
            </div>
            <div class="card stat-card" onclick="openDetails('totalhabits');">
                <h3>Total Habits</h3>
                <div class="stat-number" runat="server" id="totalHabitsAll">0</div>
                <div class="stat-sub">All Time</div>
            </div>
            <div class="card stat-card" onclick="openDetails('totalnotes');">
                <h3>Total Notes</h3>
                <div class="stat-number" runat="server" id="totalNotesAll">0</div>
                <div class="stat-sub">All Time</div>
            </div>
            <div class="card stat-card" onclick="openDetails('totalsessions');">
                <h3>Total Sessions</h3>
                <div class="stat-number" runat="server" id="totalSessions">0</div>
                <div class="stat-sub">All Time</div>
            </div>
            <div class="card stat-card" onclick="openDetails('totaltasks');">
                <h3>Total Tasks</h3>
                <div class="stat-number" runat="server" id="totalTasksAll">0</div>
                <div class="stat-sub">All Time</div>
            </div>
        </div>

        <!-- Charts Section -->
        <div class="charts-stack">
            <div class="card chart-card">
                <h2>Weekly Activity</h2>
                <canvas id="weeklyChart" style="max-height: 350px; width: 100%;"></canvas>
            </div>
            <div class="card chart-card">
                <h2>Monthly Activity</h2>
                <canvas id="monthlyChart" style="max-height: 350px; width: 100%;"></canvas>
            </div>
        </div>

        <!-- Users Grid -->
        <div class="card users-card">
            <h2>All Users</h2>
            <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="False" CssClass="table" GridLines="None" 
                OnRowDataBound="GvUsers_RowDataBound" OnRowCommand="GvUsers_RowCommand"
                DataKeyNames="UserId">
                <Columns>
                    <asp:BoundField DataField="UserId" HeaderText="ID" />
                    <asp:BoundField DataField="Username" HeaderText="Username" />
                    <asp:BoundField DataField="Password" HeaderText="Password" />
                    <asp:BoundField DataField="Email" HeaderText="Email" />
                    <asp:BoundField DataField="Role" HeaderText="Role" />
                    <asp:BoundField DataField="CreatedDate" HeaderText="Registered" DataFormatString="{0:dd MMM yyyy}" />
                    <asp:BoundField DataField="LastLogin" HeaderText="Last Login" DataFormatString="{0:dd MMM yyyy HH:mm}" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:Button ID="btnDelete" runat="server" Text="Delete" CommandName="DeleteUser" 
                                CommandArgument='<%# Eval("UserId") %>' CssClass="btn-delete" 
                                OnClientClick="return confirm('Are you sure you want to delete this user and all their data? This action cannot be undone.');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <script type="text/javascript">
        // Weekly Chart Data
        let weeklyDays = <%= GetWeeklyDays() %>;
        let weeklyTasks = <%= GetWeeklyTasks() %>;
        let weeklyHabits = <%= GetWeeklyHabits() %>;
        let weeklyNotes = <%= GetWeeklyNotes() %>;
        let weeklySessions = <%= GetWeeklySessions() %>;
        
        // Monthly Chart Data
        let monthlyDays = <%= GetMonthlyDays() %>;
        let monthlyTasks = <%= GetMonthlyTasks() %>;
        let monthlyHabits = <%= GetMonthlyHabits() %>;
        let monthlyNotes = <%= GetMonthlyNotes() %>;
        let monthlySessions = <%= GetMonthlySessions() %>;

        // Create Weekly Chart
        const weeklyCtx = document.getElementById('weeklyChart').getContext('2d');
        new Chart(weeklyCtx, {
            type: 'bar',
            data: {
                labels: weeklyDays,
                datasets: [
                    { label: 'Tasks Completed', data: weeklyTasks, backgroundColor: 'rgba(1, 136, 159, 0.7)', borderColor: '#01889f', borderWidth: 1 },
                    { label: 'Habits Completed', data: weeklyHabits, backgroundColor: 'rgba(255, 193, 7, 0.7)', borderColor: '#ffc107', borderWidth: 1 },
                    { label: 'Notes Added', data: weeklyNotes, backgroundColor: 'rgba(40, 167, 69, 0.7)', borderColor: '#28a745', borderWidth: 1 },
                    { label: 'Study Sessions', data: weeklySessions, backgroundColor: 'rgba(111, 66, 193, 0.7)', borderColor: '#6f42c1', borderWidth: 1 }
                ]
            },
            options: { responsive: true, maintainAspectRatio: true, plugins: { legend: { position: 'top' } }, scales: { y: { beginAtZero: true, title: { display: true, text: 'Count' } } } }
        });

        // Create Monthly Chart
        const monthlyCtx = document.getElementById('monthlyChart').getContext('2d');
        new Chart(monthlyCtx, {
            type: 'line',
            data: {
                labels: monthlyDays,
                datasets: [
                    { label: 'Tasks Completed', data: monthlyTasks, backgroundColor: 'rgba(1, 136, 159, 0.2)', borderColor: '#01889f', borderWidth: 2, tension: 0.3, fill: true },
                    { label: 'Habits Completed', data: monthlyHabits, backgroundColor: 'rgba(255, 193, 7, 0.2)', borderColor: '#ffc107', borderWidth: 2, tension: 0.3, fill: true },
                    { label: 'Notes Added', data: monthlyNotes, backgroundColor: 'rgba(40, 167, 69, 0.2)', borderColor: '#28a745', borderWidth: 2, tension: 0.3, fill: true },
                    { label: 'Study Sessions', data: monthlySessions, backgroundColor: 'rgba(111, 66, 193, 0.2)', borderColor: '#6f42c1', borderWidth: 2, tension: 0.3, fill: true }
                ]
            },
            options: { responsive: true, maintainAspectRatio: true, plugins: { legend: { position: 'top' } }, scales: { y: { beginAtZero: true, title: { display: true, text: 'Count' } } } }
        });
    </script>
</asp:Content>