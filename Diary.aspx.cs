using System;
using System.Data;
using System.Web.UI.WebControls;

namespace HabiTrackers
{
    public partial class Diary : System.Web.UI.Page
    {
        // Property to store the current edit date in ViewState (survives postbacks)
        private DateTime CurrentEditDate
        {
            get
            {
                if (ViewState["CurrentEditDate"] == null)
                    return DateTime.Today;
                return (DateTime)ViewState["CurrentEditDate"];
            }
            set
            {
                ViewState["CurrentEditDate"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                CurrentEditDate = DateTime.Today;
                LoadEntryForDate(CurrentEditDate);
                LoadEntriesList(null);
            }
        }

        private void LoadEntryForDate(DateTime date)
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetDiaryEntry(userId, date);

            if (dt.Rows.Count > 0)
            {
                txtContent.Text = dt.Rows[0]["Content"].ToString();
                lblDisplayDate.Text = date.ToString("dddd, dd MMMM yyyy");
            }
            else
            {
                txtContent.Text = "";
                lblDisplayDate.Text = date.ToString("dddd, dd MMMM yyyy") + " (No entry yet)";
            }

            // Update button text based on whether we're editing today or a past date
            btnSave.Text = (date.Date == DateTime.Today) ? "Save Today's Entry" : "Update Entry";
        }

        private void LoadEntriesList(DateTime? searchDate)
        {
            int userId = (int)Session["UserId"];
            DataTable dt;

            if (searchDate.HasValue)
            {
                dt = DBHelper.SearchDiaryEntries(userId, searchDate, searchDate);
            }
            else
            {
                dt = DBHelper.SearchDiaryEntries(userId, null, null);
            }

            rptEntries.DataSource = dt;
            rptEntries.DataBind();
            lblNoEntries.Visible = (dt.Rows.Count == 0);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtContent.Text))
            {
                lblMessage.Text = "Please write something.";
                return;
            }

            int userId = (int)Session["UserId"];
            DBHelper.SaveDiaryEntry(userId, CurrentEditDate, txtContent.Text);

            // After saving, if we were editing a past entry, switch back to today
            if (CurrentEditDate.Date != DateTime.Today)
            {
                CurrentEditDate = DateTime.Today;
                LoadEntryForDate(CurrentEditDate);
                lblMessage.Text = $"Entry for {CurrentEditDate:dd MMM yyyy} saved successfully!";
            }
            else
            {
                // If we were editing today, just refresh the display
                LoadEntryForDate(CurrentEditDate);
                lblMessage.Text = "Today's entry saved successfully!";
            }

            // Refresh the entry list (clear any search filter to show all entries)
            txtSearchDate.Text = "";
            LoadEntriesList(null);
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearchDate.Text))
            {
                DateTime searchDate = DateTime.Parse(txtSearchDate.Text);
                LoadEntriesList(searchDate);
                lblSearchMessage.Text = "";
            }
            else
            {
                lblSearchMessage.Text = "Please select a date to search.";
            }
        }

        protected void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearchDate.Text = "";
            lblSearchMessage.Text = "";
            LoadEntriesList(null);
            CurrentEditDate = DateTime.Today;
            LoadEntryForDate(CurrentEditDate);
        }

        protected void rptEntries_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "ViewEntry")
            {
                string dateStr = e.CommandArgument.ToString();
                DateTime selectedDate = DateTime.Parse(dateStr);
                CurrentEditDate = selectedDate;
                LoadEntryForDate(selectedDate);
            }
            else if (e.CommandName == "DeleteEntry")
            {
                string[] args = e.CommandArgument.ToString().Split('|');
                int entryId = Convert.ToInt32(args[0]);
                DateTime entryDate = DateTime.Parse(args[1]);

                int userId = (int)Session["UserId"];
                DBHelper.DeleteDiaryEntry(userId, entryDate);

                // If the deleted entry was the one currently displayed, revert to today
                if (CurrentEditDate.Date == entryDate.Date)
                {
                    CurrentEditDate = DateTime.Today;
                    LoadEntryForDate(CurrentEditDate);
                }

                lblMessage.Text = "Entry deleted.";

                // Refresh the list (clear search filter)
                txtSearchDate.Text = "";
                LoadEntriesList(null);
            }
        }
    }
}