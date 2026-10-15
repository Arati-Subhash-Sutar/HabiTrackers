using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HabiTrackers
{
    public partial class Dashboard : System.Web.UI.Page
    {
        private string[] quotes = new string[]
        {
            " \"The secret of getting ahead is getting started.\" - Mark Twain",
            " \"Small daily improvements are the key to staggering long-term results.\"",
            " \"Your future self will thank you for what you do today.\"",
            " \"Consistency beats intensity. Every single time.\"",
            " \"Don't count the days. Make the days count.\"",
            " \"The only bad workout is the one that didn't happen.\"",
            " \"Success is the sum of small efforts, repeated day in and day out.\"",
            " \"You don't have to be great to start, but you have to start to be great.\""
        };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null) Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                int userId = (int)Session["UserId"];

                lblUsername.Text = Session["Username"]?.ToString() ?? "User";

                Random rand = new Random();
                inspirationalQuote.InnerHtml = quotes[rand.Next(quotes.Length)];

                LoadStats(userId);
                LoadUserStreaks(userId);
                LoadRecentActivities(userId);
            }
        }

        private void LoadStats(int userId)
        {
            DataTable tasks = DBHelper.GetUserTasks(userId);
            totalTasks.InnerText = tasks.Rows.Count.ToString();

            DataTable notes = DBHelper.GetUserNotes(userId);
            totalNotes.InnerText = notes.Rows.Count.ToString();

            DataTable habits = DBHelper.GetUserHabits(userId);
            totalHabits.InnerText = habits.Rows.Count.ToString();

            string sessionQuery = "SELECT COUNT(*) FROM StudySessions WHERE UserId = @uid";
            SqlParameter[] sessionParams = { new SqlParameter("@uid", userId) };
            DataTable sessions = DBHelper.ExecuteDataTable(sessionQuery, sessionParams);
            if (sessions.Rows.Count > 0)
                totalSessions.InnerText = sessions.Rows[0][0].ToString();
        }

        private void LoadUserStreaks(int userId)
        {
            bool hasCompletedToday = DBHelper.HasCompletedHabitToday(userId);
            int storedStreak = DBHelper.GetCurrentUserStreak(userId);
            DateTime? lastActiveDate = DBHelper.GetLastActiveDate(userId);

            int effectiveStreak = storedStreak;

            // If no habit completed today, check if the streak is still valid
            if (!hasCompletedToday)
            {
                if (lastActiveDate.HasValue)
                {
                    // Streak continues only if last active was yesterday
                    if (lastActiveDate.Value.Date != DateTime.Today.AddDays(-1))
                    {
                        effectiveStreak = 0; // streak broken
                    }
                    // else (last active was yesterday) – keep stored streak
                }
                else
                {
                    effectiveStreak = 0; // no streak history
                }
            }
            else
            {
                // Today's completion already happened – stored streak is up‑to‑date
                effectiveStreak = storedStreak;
            }

            int longestStreakValue = DBHelper.GetLongestUserStreak(userId);
            int todayHabitsValue = DBHelper.GetTodayHabitCount(userId);

            // Assign values to the HTML controls (these are declared in the .aspx file)
            currentStreak.InnerText = effectiveStreak.ToString();
            longestStreak.InnerText = longestStreakValue.ToString();
            todayHabits.InnerText = todayHabitsValue.ToString();

            // Update the goal status message
            if (hasCompletedToday)
            {
                goalStatus.InnerHtml = "<span style='color:#01889f; font-size:0.8rem;'>✓ Habit completed today! Streak continues </span>";
            }
            else
            {
                goalStatus.InnerHtml = "<span style='color:#ffc107; font-size:0.8rem;'> Complete at least one habit today to maintain your streak</span>";
            }
        }
        private void LoadRecentActivities(int userId)
        {
            DataTable activities = DBHelper.GetRecentActivities(userId, 5);
            gvRecentActivities.DataSource = activities;
            gvRecentActivities.DataBind();
        }

        // ========== Weekly Chart Data Methods (Direct from new GetWeeklyReport) ==========
        public string GetWeeklyDays()
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetWeeklyReport(userId);
            string[] days = new string[7];
            for (int i = 0; i < 7; i++)
            {
                DateTime date = DateTime.Today.AddDays(-6 + i);
                days[i] = date.ToString("ddd");
            }
            return new JavaScriptSerializer().Serialize(days);
        }

        public string GetWeeklyHabits()
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetWeeklyReport(userId);
            int[] habits = new int[7];
            for (int i = 0; i < 7; i++)
            {
                DateTime targetDate = DateTime.Today.AddDays(-6 + i);
                DataRow[] rows = dt.Select("Date = #" + targetDate.ToString("yyyy-MM-dd") + "#");
                if (rows.Length > 0)
                    habits[i] = Convert.ToInt32(rows[0]["HabitsCompleted"]);
                else
                    habits[i] = 0;
            }
            return new JavaScriptSerializer().Serialize(habits);
        }

        public string GetWeeklyTasks()
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetWeeklyReport(userId);
            int[] tasks = new int[7];
            for (int i = 0; i < 7; i++)
            {
                DateTime targetDate = DateTime.Today.AddDays(-6 + i);
                DataRow[] rows = dt.Select("Date = #" + targetDate.ToString("yyyy-MM-dd") + "#");
                if (rows.Length > 0)
                    tasks[i] = Convert.ToInt32(rows[0]["TasksCompleted"]);
                else
                    tasks[i] = 0;
            }
            return new JavaScriptSerializer().Serialize(tasks);
        }

        public string GetWeeklySessions()
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetWeeklyReport(userId);
            int[] sessions = new int[7];
            for (int i = 0; i < 7; i++)
            {
                DateTime targetDate = DateTime.Today.AddDays(-6 + i);
                DataRow[] rows = dt.Select("Date = #" + targetDate.ToString("yyyy-MM-dd") + "#");
                if (rows.Length > 0)
                    sessions[i] = Convert.ToInt32(rows[0]["StudySessions"]);
                else
                    sessions[i] = 0;
            }
            return new JavaScriptSerializer().Serialize(sessions);
        }

        // ========== Monthly Chart Data Methods ==========
        public string GetMonthlyDays()
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetMonthlyReport(userId);
            string[] days = new string[30];
            for (int i = 0; i < 30; i++)
            {
                DateTime date = DateTime.Today.AddDays(-29 + i);
                days[i] = date.ToString("dd MMM");
            }
            return new JavaScriptSerializer().Serialize(days);
        }

        public string GetMonthlyHabits()
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetMonthlyReport(userId);
            int[] habits = new int[30];
            for (int i = 0; i < 30; i++)
            {
                DateTime targetDate = DateTime.Today.AddDays(-29 + i);
                DataRow[] rows = dt.Select("Date = #" + targetDate.ToString("yyyy-MM-dd") + "#");
                if (rows.Length > 0)
                    habits[i] = Convert.ToInt32(rows[0]["HabitsCompleted"]);
                else
                    habits[i] = 0;
            }
            return new JavaScriptSerializer().Serialize(habits);
        }

        public string GetMonthlyTasks()
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetMonthlyReport(userId);
            int[] tasks = new int[30];
            for (int i = 0; i < 30; i++)
            {
                DateTime targetDate = DateTime.Today.AddDays(-29 + i);
                DataRow[] rows = dt.Select("Date = #" + targetDate.ToString("yyyy-MM-dd") + "#");
                if (rows.Length > 0)
                    tasks[i] = Convert.ToInt32(rows[0]["TasksCompleted"]);
                else
                    tasks[i] = 0;
            }
            return new JavaScriptSerializer().Serialize(tasks);
        }

        public string GetMonthlySessions()
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetMonthlyReport(userId);
            int[] sessions = new int[30];
            for (int i = 0; i < 30; i++)
            {
                DateTime targetDate = DateTime.Today.AddDays(-29 + i);
                DataRow[] rows = dt.Select("Date = #" + targetDate.ToString("yyyy-MM-dd") + "#");
                if (rows.Length > 0)
                    sessions[i] = Convert.ToInt32(rows[0]["StudySessions"]);
                else
                    sessions[i] = 0;
            }
            return new JavaScriptSerializer().Serialize(sessions);
        }
        protected void gvRecentActivities_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string activityType = DataBinder.Eval(e.Row.DataItem, "ActivityType").ToString();
                string pageUrl = GetPageUrl(activityType);
                e.Row.Attributes["onclick"] = "window.location='" + pageUrl + "'";
                e.Row.Style["cursor"] = "pointer";
                e.Row.ToolTip = "Click to go to " + activityType + " page";
            }
        }

        private string GetPageUrl(string activityType)
        {
            switch (activityType.ToLower())
            {
                case "habit": return "Habits.aspx";
                case "task": return "DailyTasks.aspx";
                case "note": return "Notes.aspx";
                case "session": return "StudyTimer.aspx";
                default: return "#";
            }
        }
    }
}