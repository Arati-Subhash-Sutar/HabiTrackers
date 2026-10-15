<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="HabiTrackers._Default" %>
<!DOCTYPE html>
<html>
<head>
    <title>HabiTrackers - Welcome</title>
    <link href="Styles/site.css" rel="stylesheet" />
    <style>
        body {
            margin: 0;
            padding: 0;
            min-height: 100vh;
            background-image: url('../images/login background_1.jpg');
            background-repeat: no-repeat;
            background-size: cover;
            background-position: center center;
            background-attachment: fixed;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }
        .landing-wrapper {
            max-width: 1300px;
            width: 90%;
            margin: 2rem auto;
            display: flex;
            gap: 2rem;
            min-height: 80vh;
        }
        .left-panel {
            flex: 1;
            /* empty – only background */
        }
        .right-panel {
            flex: 1;
            background: white;
            border-radius: 28px;
            padding: 2rem;
            box-shadow: 0 20px 35px rgba(0,0,0,0.2);
        }
        .info-card h1 {
            font-size: 2.5rem;
            color: #ffcc00;
            margin-bottom: 1rem;
            border-left: 5px solid #01889f;
            padding-left: 1rem;
        }
        .info-card p {
            font-size: 1rem;
            line-height: 1.6;
            color: #2c3e50;
        }
        .feature-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
            gap: 1rem;
            margin: 1.5rem 0;
        }
        .feature-item {
            background: #f8f9fa;
            border-radius: 16px;
            padding: 0.8rem;
            text-align: center;
            border: 1px solid #e1e5e9;
        }
        .feature-title {
            font-weight: bold;
            color: #01889f;
        }
        .feature-desc {
            font-size: 0.75rem;
            color: #2c3e50;
            margin-top: 0.3rem;
        }
        .btn-get-started {
            background: #ffcc00;
            color: #1e2a36;
            border: none;
            border-radius: 40px;
            padding: 12px 28px;
            font-size: 1.1rem;
            font-weight: bold;
            cursor: pointer;
            width: 100%;
            margin-top: 1rem;
        }
        .btn-get-started:hover {
            background: #e6b800;
            transform: scale(1.02);
        }
        .btn-admin {
            background: #01889f;
            color: white;
            margin-top: 0.5rem;
        }
        @media (max-width: 768px) {
            .landing-wrapper { flex-direction: column; }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="landing-wrapper">
            <div class="left-panel"></div>
            <div class="right-panel">
                <div class="info-card">
                    <h1>HabiTrackers</h1>
                    <p>Your all‑in‑one productivity companion to build better habits, manage tasks, journal your day, and stay on track – all in one place.</p>
                    <div class="feature-grid">
                        <div class="feature-item"><div class="feature-title">Daily Tasks</div><div class="feature-desc">Create, prioritize, and complete tasks with reminders.</div></div>
                        <div class="feature-item"><div class="feature-title">Habit Tracker</div><div class="feature-desc">Build streaks, earn batches (Bronze, Silver, Gold).</div></div>
                        <div class="feature-item"><div class="feature-title">Notes & Attachments</div><div class="feature-desc">Write notes, attach files, export as PDF.</div></div>
                        <div class="feature-item"><div class="feature-title">Daily Diary</div><div class="feature-desc">Journal your thoughts with a notebook‑style editor.</div></div>
                        <div class="feature-item"><div class="feature-title">Timetable</div><div class="feature-desc">Plan your week, mark breaks with visual indicators.</div></div>
                        <div class="feature-item"><div class="feature-title">Study Timer</div><div class="feature-desc">Pomodoro timer, track sessions, get sound alerts.</div></div>
                        <div class="feature-item"><div class="feature-title">Insightful Reports</div><div class="feature-desc">Weekly & monthly activity charts.</div></div>
                        <div class="feature-item"><div class="feature-title">Email Reminders</div><div class="feature-desc">Never miss a task – get email notifications.</div></div>
                    </div>
                    <p style="font-style: italic;">Join hundreds of users who are transforming their daily routines. Sign up now and start your journey!</p>
                    <asp:Button ID="btnGetStarted" runat="server" Text="Get Started" CssClass="btn-get-started" OnClick="btnGetStarted_Click" />
                    <asp:Button ID="btnAdminRedirect" runat="server" Text="Login as Admin" CssClass="btn-get-started btn-admin" OnClick="btnAdminRedirect_Click" />
                </div>
            </div>
        </div>
    </form>
</body>
</html>