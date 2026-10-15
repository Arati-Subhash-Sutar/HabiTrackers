using System;
using System.Data;
using System.Web.UI.WebControls;

namespace HabiTrackers
{
    public partial class UserDetails : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                int userId = 0;
                if (Request.QueryString["id"] != null)
                {
                    userId = Convert.ToInt32(Request.QueryString["id"]);
                }
                else if (Session["SelectedUserId"] != null)
                {
                    userId = Convert.ToInt32(Session["SelectedUserId"]);
                    Session["SelectedUserId"] = null;
                }
                else
                {
                    Response.Redirect("AdminDashboard.aspx");
                }

                LoadUserDetails(userId);
            }
        }

        private void LoadUserDetails(int userId)
        {
            DataTable dt = DBHelper.GetUserFullDetails(userId);
            if (dt.Rows.Count == 0) return;

            DataRow row = dt.Rows[0];

            // Basic Info
            lblUserId.Text = userId.ToString();
            lblUsername.Text = row["Username"].ToString();
            lblEmail.Text = row["Email"].ToString();
            lblRole.Text = row["Role"].ToString();
            lblRegistered.Text = Convert.ToDateTime(row["CreatedDate"]).ToString("dd MMM yyyy");
            lblLastLogin.Text = row["LastLogin"] != DBNull.Value ? Convert.ToDateTime(row["LastLogin"]).ToString("dd MMM yyyy HH:mm") : "Never";

            // Streak Info
            lblCurrentStreak.Text = row["CurrentStreak"].ToString();
            lblLongestStreak.Text = row["LongestStreak"].ToString();
            lblTodayHabits.Text = row["TodayHabits"].ToString();

            // Tasks
            lblTotalTasks.Text = row["TotalTasks"].ToString();
            lblPendingTasks.Text = row["PendingTasks"].ToString();
            lblCompletedTasks.Text = row["CompletedTasks"].ToString();

            // Habits
            lblTotalHabits.Text = row["TotalHabits"].ToString();
            lblTotalCompletions.Text = row["TotalCompletions"].ToString();

            // Batches
            string batches = row["BatchesEarned"].ToString();
            if (!string.IsNullOrEmpty(batches))
            {
                if (batches.Contains("Bronze"))
                    phBatches.Controls.Add(new Literal { Text = "<span class='badge-batch badge-bronze'> Bronze</span> " });
                if (batches.Contains("Silver"))
                    phBatches.Controls.Add(new Literal { Text = "<span class='badge-batch badge-silver'> Silver</span> " });
                if (batches.Contains("Gold"))
                    phBatches.Controls.Add(new Literal { Text = "<span class='badge-batch badge-gold'> Gold</span> " });
            }
            else
            {
                phBatches.Controls.Add(new Literal { Text = "No badges earned yet." });
            }

            // Habit Details
            DataTable habitTable = DBHelper.GetUserHabits(userId);
            rptHabits.DataSource = habitTable;
            rptHabits.DataBind();

            // Notes
            lblTotalNotes.Text = row["TotalNotes"].ToString();
            lblRecentNotes.Text = row["RecentNotes"].ToString();
            DataTable noteTable = DBHelper.GetUserNotes(userId);
            rptNotes.DataSource = noteTable;
            rptNotes.DataBind();

            // Timetable
            lblTotalTimetableEntries.Text = row["TotalTimetableEntries"].ToString();
            DataTable timetableTable = DBHelper.GetUserTimetable(userId);
            rptTimetable.DataSource = timetableTable;
            rptTimetable.DataBind();

            // Study Sessions
            lblTotalStudySessions.Text = row["TotalStudySessions"].ToString();
            lblTotalStudyTime.Text = row["TotalStudyTime"].ToString();
            DataTable sessionTable = DBHelper.GetUserStudySessions(userId);
            rptStudySessions.DataSource = sessionTable;
            rptStudySessions.DataBind();
        }
    }
}