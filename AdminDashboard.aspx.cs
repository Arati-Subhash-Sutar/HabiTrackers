using System;
using System.Data;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HabiTrackers
{
    public partial class AdminDashboard : System.Web.UI.Page
    {
        private DateTime? selectedDate = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                LoadStats();
                LoadUsers();
            }
        }

        private void LoadStats()
        {
            DateTime? date = selectedDate;
            DataTable stats = DBHelper.GetAdminStats(date);
            if (stats.Rows.Count > 0)
            {
                todayLogins.InnerText = stats.Rows[0]["TodayLogins"].ToString();
                newUsers.InnerText = stats.Rows[0]["NewUsers"].ToString();
                tasksCompleted.InnerText = stats.Rows[0]["TasksCompletedToday"].ToString();
                notesToday.InnerText = stats.Rows[0]["NotesToday"].ToString();
                habitsCompleted.InnerText = stats.Rows[0]["HabitsCompletedToday"].ToString();
                totalSessions.InnerText = stats.Rows[0]["TotalSessions"].ToString();
                remindersSent.InnerText = stats.Rows[0]["RemindersSentToday"].ToString();

                // Total stats (overall)
                totalLogins.Text = DBHelper.GetTotalLogins().ToString();
                totalUsers.Text = DBHelper.GetTotalUsers().ToString();
                totalReminders.Text = DBHelper.GetTotalReminders().ToString();
                totalHabits.Text = DBHelper.GetTotalHabits().ToString();
                totalTasks.Text = DBHelper.GetTotalTasks().ToString();
                totalNotes.Text = DBHelper.GetTotalNotes().ToString();
                totalHabitsAll.InnerText = totalHabits.Text;
                totalNotesAll.InnerText = totalNotes.Text;
                totalTasksAll.InnerText = totalTasks.Text;
            }
        }

        private void LoadUsers()
        {
            DataTable dt = DBHelper.GetAllUsers();
            gvUsers.DataSource = dt;
            gvUsers.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearchDate.Text))
            {
                selectedDate = DateTime.Parse(txtSearchDate.Text);
            }
            LoadStats();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearchDate.Text = "";
            selectedDate = null;
            LoadStats();
        }

        protected void GvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int userId = Convert.ToInt32(gvUsers.DataKeys[e.Row.RowIndex].Value);
                e.Row.Attributes["onclick"] = "window.location='UserDetails.aspx?id=" + userId + "';";
                e.Row.Attributes["class"] = "clickable-row";
                e.Row.Style["cursor"] = "pointer";
                e.Row.ToolTip = "Click to view user details";

                string role = DataBinder.Eval(e.Row.DataItem, "Role").ToString();
                Button btnDelete = (Button)e.Row.FindControl("btnDelete");
                if (btnDelete != null && role == "Admin")
                {
                    btnDelete.Enabled = false;
                    btnDelete.ToolTip = "Cannot delete admin users";
                }
            }
        }

        protected void GvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteUser")
            {
                int userId = Convert.ToInt32(e.CommandArgument);
                DBHelper.DeleteUserWithAllData(userId);
                LoadUsers();
                LoadStats();
            }
        }

        // Chart methods remain unchanged
        public string GetWeeklyDays()
        {
            DataTable dt = DBHelper.GetWeeklyAdminReport();
            string[] days = new string[7];
            for (int i = 0; i < 7; i++)
            {
                days[i] = DateTime.Today.AddDays(-6 + i).ToString("ddd");
            }
            return new JavaScriptSerializer().Serialize(days);
        }

        public string GetWeeklyTasks()
        {
            DataTable dt = DBHelper.GetWeeklyAdminReport();
            int[] tasks = new int[7];
            for (int i = 0; i < 7; i++)
            {
                DateTime targetDate = DateTime.Today.AddDays(-6 + i);
                DataRow[] rows = dt.Select("Date = #" + targetDate.ToString("yyyy-MM-dd") + "#");
                tasks[i] = rows.Length > 0 ? Convert.ToInt32(rows[0]["TasksCompleted"]) : 0;
            }
            return new JavaScriptSerializer().Serialize(tasks);
        }

        public string GetWeeklyHabits()
        {
            DataTable dt = DBHelper.GetWeeklyAdminReport();
            int[] habits = new int[7];
            for (int i = 0; i < 7; i++)
            {
                DateTime targetDate = DateTime.Today.AddDays(-6 + i);
                DataRow[] rows = dt.Select("Date = #" + targetDate.ToString("yyyy-MM-dd") + "#");
                habits[i] = rows.Length > 0 ? Convert.ToInt32(rows[0]["HabitsCompleted"]) : 0;
            }
            return new JavaScriptSerializer().Serialize(habits);
        }

        public string GetWeeklyNotes()
        {
            DataTable dt = DBHelper.GetWeeklyAdminReport();
            int[] notes = new int[7];
            for (int i = 0; i < 7; i++)
            {
                DateTime targetDate = DateTime.Today.AddDays(-6 + i);
                DataRow[] rows = dt.Select("Date = #" + targetDate.ToString("yyyy-MM-dd") + "#");
                notes[i] = rows.Length > 0 ? Convert.ToInt32(rows[0]["NotesCreated"]) : 0;
            }
            return new JavaScriptSerializer().Serialize(notes);
        }

        public string GetWeeklySessions()
        {
            DataTable dt = DBHelper.GetWeeklyAdminReport();
            int[] sessions = new int[7];
            for (int i = 0; i < 7; i++)
            {
                DateTime targetDate = DateTime.Today.AddDays(-6 + i);
                DataRow[] rows = dt.Select("Date = #" + targetDate.ToString("yyyy-MM-dd") + "#");
                sessions[i] = rows.Length > 0 ? Convert.ToInt32(rows[0]["StudySessions"]) : 0;
            }
            return new JavaScriptSerializer().Serialize(sessions);
        }

        public string GetMonthlyDays()
        {
            DataTable dt = DBHelper.GetMonthlyAdminReport();
            string[] days = new string[30];
            for (int i = 0; i < 30; i++)
            {
                days[i] = DateTime.Today.AddDays(-29 + i).ToString("dd MMM");
            }
            return new JavaScriptSerializer().Serialize(days);
        }

        public string GetMonthlyTasks()
        {
            DataTable dt = DBHelper.GetMonthlyAdminReport();
            int[] tasks = new int[30];
            for (int i = 0; i < 30; i++)
            {
                DateTime targetDate = DateTime.Today.AddDays(-29 + i);
                DataRow[] rows = dt.Select("Date = #" + targetDate.ToString("yyyy-MM-dd") + "#");
                tasks[i] = rows.Length > 0 ? Convert.ToInt32(rows[0]["TasksCompleted"]) : 0;
            }
            return new JavaScriptSerializer().Serialize(tasks);
        }

        public string GetMonthlyHabits()
        {
            DataTable dt = DBHelper.GetMonthlyAdminReport();
            int[] habits = new int[30];
            for (int i = 0; i < 30; i++)
            {
                DateTime targetDate = DateTime.Today.AddDays(-29 + i);
                DataRow[] rows = dt.Select("Date = #" + targetDate.ToString("yyyy-MM-dd") + "#");
                habits[i] = rows.Length > 0 ? Convert.ToInt32(rows[0]["HabitsCompleted"]) : 0;
            }
            return new JavaScriptSerializer().Serialize(habits);
        }

        public string GetMonthlyNotes()
        {
            DataTable dt = DBHelper.GetMonthlyAdminReport();
            int[] notes = new int[30];
            for (int i = 0; i < 30; i++)
            {
                DateTime targetDate = DateTime.Today.AddDays(-29 + i);
                DataRow[] rows = dt.Select("Date = #" + targetDate.ToString("yyyy-MM-dd") + "#");
                notes[i] = rows.Length > 0 ? Convert.ToInt32(rows[0]["NotesCreated"]) : 0;
            }
            return new JavaScriptSerializer().Serialize(notes);
        }

        public string GetMonthlySessions()
        {
            DataTable dt = DBHelper.GetMonthlyAdminReport();
            int[] sessions = new int[30];
            for (int i = 0; i < 30; i++)
            {
                DateTime targetDate = DateTime.Today.AddDays(-29 + i);
                DataRow[] rows = dt.Select("Date = #" + targetDate.ToString("yyyy-MM-dd") + "#");
                sessions[i] = rows.Length > 0 ? Convert.ToInt32(rows[0]["StudySessions"]) : 0;
            }
            return new JavaScriptSerializer().Serialize(sessions);
        }
    }
}