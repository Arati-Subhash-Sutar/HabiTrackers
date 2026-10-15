using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Web.UI;

namespace HabiTrackers
{
    public partial class StudyTimer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                LoadUserSettings();
            }
        }

        private void LoadUserSettings()
        {
            if (Session["WorkDuration"] == null)
            {
                Session["WorkDuration"] = 25;
                Session["BreakDuration"] = 5;
            }
            txtWorkMinutes.Text = Session["WorkDuration"].ToString();
            txtBreakMinutes.Text = Session["BreakDuration"].ToString();
        }

        // Returns today's session count for JavaScript
        protected string GetTodaySessions()
        {
            try
            {
                if (Session["UserId"] == null) return "0";
                int userId = (int)Session["UserId"];
                string query = @"
                    SELECT ISNULL(SUM(SessionsCompleted), 0) 
                    FROM StudySessions 
                    WHERE UserId = @uid AND SessionDate = CAST(GETDATE() AS DATE)";
                SqlParameter[] parameters = { new SqlParameter("@uid", userId) };
                DataTable dt = DBHelper.ExecuteDataTable(query, parameters);
                if (dt.Rows.Count > 0 && dt.Rows[0][0] != DBNull.Value)
                    return dt.Rows[0][0].ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting sessions: " + ex.Message);
            }
            return "0";
        }

        protected void btnUpdateSettings_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(txtWorkMinutes.Text) && !string.IsNullOrEmpty(txtBreakMinutes.Text))
                {
                    Session["WorkDuration"] = int.Parse(txtWorkMinutes.Text);
                    Session["BreakDuration"] = int.Parse(txtBreakMinutes.Text);
                }
            }
            catch
            {
                // Handle parsing error
            }
            Response.Redirect("StudyTimer.aspx");
        }

        [WebMethod]
        public static void SaveWorkSession(int workSeconds)
        {
            try
            {
                if (System.Web.HttpContext.Current.Session["UserId"] != null)
                {
                    int userId = (int)System.Web.HttpContext.Current.Session["UserId"];
                    string query = @"INSERT INTO StudySessions 
                        (UserId, WorkDuration, BreakDuration, SessionsCompleted, TotalWorkTime, TotalBreakTime, SessionDate)
                        VALUES (@uid, @work, 0, 1, @work, 0, GETDATE())";
                    SqlParameter[] parameters = {
                        new SqlParameter("@uid", userId),
                        new SqlParameter("@work", workSeconds)
                    };
                    DBHelper.ExecuteNonQuery(query, parameters);

                    // Add recent activity
                    int minutes = workSeconds / 60;
                    DBHelper.AddRecentActivity(userId, "Session", "Study Session", minutes + " minutes completed");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error saving session: " + ex.Message);
            }
        }
    }
}