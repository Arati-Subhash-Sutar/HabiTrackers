using System;
using System.Data;

namespace HabiTrackers
{
    public partial class MasterUser : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "User")
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                LoadUserProfile();
            }

            // Set active navigation link on every page load
            SetActiveNavigationLink();
        }

        private void LoadUserProfile()
        {
            int userId = (int)Session["UserId"];
            string username = Session["Username"]?.ToString();
            string email = Session["Email"]?.ToString();

            // If email not in session, fetch from DB
            if (string.IsNullOrEmpty(email))
            {
                DataTable dt = DBHelper.GetUserById(userId);
                if (dt.Rows.Count > 0)
                {
                    username = dt.Rows[0]["Username"].ToString();
                    email = dt.Rows[0]["Email"].ToString();
                    Session["Username"] = username;
                    Session["Email"] = email;
                }
            }

            lblUsername.Text = username;
            lblEmail.Text = email;
            spnInitials.InnerText = username.Length > 0 ? username[0].ToString().ToUpper() : "U";
        }

        private void SetActiveNavigationLink()
        {
            string currentPage = System.IO.Path.GetFileName(Request.Path);

            // Remove the "active" class from all links first
            RemoveActiveClass();

            // Add "active" class to the current page link
            switch (currentPage.ToLower())
            {
                case "dashboard.aspx":
                    lnkDashboard.Attributes["class"] = "active";
                    break;
                case "habits.aspx":
                    lnkHabits.Attributes["class"] = "active";
                    break;
                case "dailytasks.aspx":
                    lnkDailyTasks.Attributes["class"] = "active";
                    break;
                case "notes.aspx":
                    lnkNotes.Attributes["class"] = "active";
                    break;
                case "diary.aspx":
                    lnkDiary.Attributes["class"] = "active";
                    break;
                case "timetable.aspx":
                    lnkTimetable.Attributes["class"] = "active";
                    break;
                case "studytimer.aspx":
                    lnkStudyTimer.Attributes["class"] = "active";
                    break;
                default:
                    break;
            }
        }

        private void RemoveActiveClass()
        {
            lnkDashboard.Attributes.Remove("class");
            lnkHabits.Attributes.Remove("class");
            lnkDailyTasks.Attributes.Remove("class");
            lnkNotes.Attributes.Remove("class");
            lnkDiary.Attributes.Remove("class");
            lnkTimetable.Attributes.Remove("class");
            lnkStudyTimer.Attributes.Remove("class");
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("Default.aspx");
        }
    }
}