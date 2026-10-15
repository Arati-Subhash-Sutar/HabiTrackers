<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/MasterUser.master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="HabiTrackers.Dashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div class="page-container">
        <!-- Welcome Banner with Inspirational Quote -->
        <div class="welcome-banner">
            <div class="banner-content">
                <h1>Welcome back, <asp:Label ID="lblUsername" runat="server" />! </h1>
                <p id="inspirationalQuote" runat="server" class="inspirational-quote"></p>
            </div>
        </div>

        <!-- User Streaks Section (Top) -->
        <div class="streaks-container">
            <div class="card streak-card">
                <div class="streak-info">
                    <h3>Current Streak</h3>
                    <div class="stat-number-large" runat="server" id="currentStreak">0</div>
                    <p>days with habit completed</p>
                </div>
            </div>
            <div class="card streak-card">
                <div class="streak-info">
                    <h3>Longest Streak</h3>
                    <div class="stat-number-large" runat="server" id="longestStreak">0</div>
                    <p>days</p>
                </div>
            </div>
            <div class="card streak-card">
                <div class="streak-info">
                    <h3>Today's Habits</h3>
                    <div class="stat-number-large" runat="server" id="todayHabits">0</div>
                    <p>habits completed today</p>
                    <div class="goal-progress" id="goalStatus" runat="server"></div>
                </div>
            </div>
        </div>

        <!-- Stats Cards - Smaller, in one line -->
        <div class="stats-row">
            <div class="stat-mini-card" data-url="DailyTasks.aspx">
                <div class="stat-mini-info">
                    <div class="stat-mini-number" runat="server" id="totalTasks">0</div>
                    <div class="stat-mini-label">Tasks</div>
                </div>
            </div>
            <div class="stat-mini-card" data-url="Notes.aspx">
                <div class="stat-mini-info">
                    <div class="stat-mini-number" runat="server" id="totalNotes">0</div>
                    <div class="stat-mini-label">Notes</div>
                </div>
            </div>
            <div class="stat-mini-card" data-url="Habits.aspx">
                <div class="stat-mini-info">
                    <div class="stat-mini-number" runat="server" id="totalHabits">0</div>
                    <div class="stat-mini-label">Habits</div>
                </div>
            </div>
            <div class="stat-mini-card" data-url="StudyTimer.aspx">
                <div class="stat-mini-info">
                    <div class="stat-mini-number" runat="server" id="totalSessions">0</div>
                    <div class="stat-mini-label">Sessions</div>
                </div>
            </div>
        </div>

        <!-- Reports Section - Actual Graphs -->
        <div class="reports-grid">
            <div class="card report-card">
                <h2>Weekly Activity</h2>
                <canvas id="weeklyChart" style="max-height: 300px; width: 100%;"></canvas>
            </div>
            <div class="card report-card">
                <h2>Monthly Activity</h2>
                <canvas id="monthlyChart" style="max-height: 300px; width: 100%;"></canvas>
            </div>
        </div>

        <!-- Recent Activities (Bottom) -->
        <div class="card">
            <h2>Recent Activities</h2>
            <asp:GridView ID="gvRecentActivities" runat="server" AutoGenerateColumns="False" CssClass="activity-table" 
                GridLines="None" EmptyDataText="No recent activities." OnRowDataBound="gvRecentActivities_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="ActivityDate" HeaderText="Date" DataFormatString="{0:dd MMM yyyy HH:mm}" />
                    <asp:BoundField DataField="ActivityType" HeaderText="Type" />
                    <asp:BoundField DataField="ActivityTitle" HeaderText="Title" />
                    <asp:BoundField DataField="Details" HeaderText="Details" />
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <script type="text/javascript">
        // Weekly Chart Data
        let weeklyDays = <%= GetWeeklyDays() %>;
        let weeklyHabits = <%= GetWeeklyHabits() %>;
        let weeklyTasks = <%= GetWeeklyTasks() %>;
        let weeklySessions = <%= GetWeeklySessions() %>;
        
        // Monthly Chart Data
        let monthlyDays = <%= GetMonthlyDays() %>;
        let monthlyHabits = <%= GetMonthlyHabits() %>;
        let monthlyTasks = <%= GetMonthlyTasks() %>;
        let monthlySessions = <%= GetMonthlySessions() %>;

        const weeklyCtx = document.getElementById('weeklyChart').getContext('2d');
        new Chart(weeklyCtx, {
            type: 'bar',
            data: {
                labels: weeklyDays,
                datasets: [
                    {
                        label: 'Habits Completed',
                        data: weeklyHabits,
                        backgroundColor: 'rgba(1, 136, 159, 0.7)',
                        borderColor: '#01889f',
                        borderWidth: 1
                    },
                    {
                        label: 'Tasks Completed',
                        data: weeklyTasks,
                        backgroundColor: 'rgba(255, 193, 7, 0.7)',
                        borderColor: '#ffc107',
                        borderWidth: 1
                    },
                    {
                        label: 'Study Sessions',
                        data: weeklySessions,
                        backgroundColor: 'rgba(40, 167, 69, 0.7)',
                        borderColor: '#28a745',
                        borderWidth: 1
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: { legend: { position: 'top' } },
                scales: { y: { beginAtZero: true, title: { display: true, text: 'Count' } } }
            }
        });

        const monthlyCtx = document.getElementById('monthlyChart').getContext('2d');
        new Chart(monthlyCtx, {
            type: 'line',
            data: {
                labels: monthlyDays,
                datasets: [
                    {
                        label: 'Habits Completed',
                        data: monthlyHabits,
                        backgroundColor: 'rgba(1, 136, 159, 0.2)',
                        borderColor: '#01889f',
                        borderWidth: 2,
                        tension: 0.3,
                        fill: true
                    },
                    {
                        label: 'Tasks Completed',
                        data: monthlyTasks,
                        backgroundColor: 'rgba(255, 193, 7, 0.2)',
                        borderColor: '#ffc107',
                        borderWidth: 2,
                        tension: 0.3,
                        fill: true
                    },
                    {
                        label: 'Study Sessions',
                        data: monthlySessions,
                        backgroundColor: 'rgba(40, 167, 69, 0.2)',
                        borderColor: '#28a745',
                        borderWidth: 2,
                        tension: 0.3,
                        fill: true
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: true,
                plugins: { legend: { position: 'top' } },
                scales: { y: { beginAtZero: true, title: { display: true, text: 'Count' } } }
            }
        });

        document.querySelectorAll('.stat-mini-card').forEach(card => {
            card.addEventListener('click', function () {
                const url = this.getAttribute('data-url');
                if (url) window.location.href = url;
            });
        });
    </script>

    <style>
        .welcome-banner {
            background: linear-gradient(135deg, #01889f 0%, #016c7e 100%);
            border-radius: 20px;
            padding: 2rem;
            margin-bottom: 2rem;
            color: white;
            text-align: center;
            box-shadow: 0 10px 30px rgba(0,0,0,0.1);
        }
        .banner-content h1 { font-size: 1.8rem; margin-bottom: 0.5rem; }
        .inspirational-quote { font-size: 1.1rem; font-style: italic; opacity: 0.9; margin-top: 0.5rem; color: white; }
        .streaks-container { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 1.5rem; margin-bottom: 2rem; }
        .streak-card { display: flex; align-items: center; justify-content: center; padding: 1.5rem; transition: transform 0.3s ease; text-align: center; }
        .streak-card:hover { transform: translateY(-5px); }
        .streak-info { flex: 1; }
        .streak-info h3 { font-size: 1rem; color: #666; margin-bottom: 0.5rem; }
        .stat-number-large { font-size: 2.5rem; font-weight: bold; color: #01889f; }
        .goal-progress { margin-top: 0.5rem; }
        .stats-row { display: flex; justify-content: space-between; gap: 1rem; margin-bottom: 2rem; flex-wrap: wrap; }
        .stat-mini-card { background: rgba(255,255,255,0.95); border-radius: 15px; padding: 1rem; flex: 1; min-width: 100px; display: flex; align-items: center; justify-content: center; box-shadow: 0 5px 15px rgba(0,0,0,0.08); transition: transform 0.3s ease; cursor: pointer; text-align: center; }
        .stat-mini-card:hover { transform: translateY(-3px); box-shadow: 0 8px 25px rgba(0,0,0,0.12); }
        .stat-mini-info { flex: 1; }
        .stat-mini-number { font-size: 1.5rem; font-weight: bold; color: #01889f; line-height: 1; }
        .stat-mini-label { font-size: 0.7rem; color: #666; text-transform: uppercase; letter-spacing: 0.5px; }
        .reports-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(450px, 1fr)); gap: 1.5rem; margin-bottom: 2rem; }
        .report-card { padding: 1.5rem; }
        .report-card h2 { margin-bottom: 1rem; font-size: 1.2rem; }
        .activity-table { width: 100%; border-collapse: collapse; }
        .activity-table th, .activity-table td { padding: 12px; text-align: left; border-bottom: 1px solid #e1e5e9; }
        .activity-table th { background: #f8f9fa; color: #01889f; font-weight: 600; }
        .activity-table tr { cursor: pointer; transition: background 0.3s ease; }
        .activity-table tr:hover { background: rgba(1, 136, 159, 0.05); }
        @media (max-width: 768px) {
            .reports-grid { grid-template-columns: 1fr; }
            .streaks-container { grid-template-columns: 1fr; }
            .stats-row { flex-direction: column; }
            .stat-mini-card { width: 100%; }
        }
    </style>
</asp:Content>