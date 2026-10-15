using System;
using System.Data;
using System.Web.UI.WebControls;

namespace HabiTrackers
{
    public partial class AdminStatsDetails : System.Web.UI.Page
    {
        private DataTable originalData;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                string type = Request.QueryString["type"];
                string dateStr = Request.QueryString["date"];
                DateTime? date = null;
                if (!string.IsNullOrEmpty(dateStr))
                    date = DateTime.Parse(dateStr);

                // Load the full data and store it
                originalData = DBHelper.GetStatsDetails(type, date);
                ViewState["OriginalData"] = originalData;
                BindGrid(originalData);
                SetTitle(type, date);
            }
            else
            {
                // Retrieve the original data from ViewState for filtering
                originalData = ViewState["OriginalData"] as DataTable;
            }
        }

        private void SetTitle(string type, DateTime? date)
        {
            string title = "";
            switch (type)
            {
                case "logins":
                    title = "Users Who Logged In";
                    break;
                case "newusers":
                    title = "New Users Registered";
                    break;
                case "reminders":
                    title = "Reminders Sent";
                    break;
                case "habits":
                    title = "Habits Completed";
                    break;
                case "tasks":
                    title = "Tasks Completed";
                    break;
                case "notes":
                    title = "Notes Added";
                    break;
                case "totalhabits":
                    title = "All Habits";
                    break;
                case "totalnotes":
                    title = "All Notes";
                    break;
                case "totaltasks":
                    title = "All Tasks";
                    break;
                case "totalsessions":
                    title = "All Study Sessions";
                    break;
                default:
                    title = "Details";
                    break;
            }
            if (date.HasValue && type != "totalhabits" && type != "totalnotes" && type != "totaltasks" && type != "totalsessions")
                title += $" on {date.Value:dd MMM yyyy}";
            pageTitle.InnerHtml = title;
        }

        private void BindGrid(DataTable data)
        {
            gvDetails.DataSource = data;
            gvDetails.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (originalData == null || originalData.Rows.Count == 0)
                return;

            string searchTerm = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                BindGrid(originalData);
                return;
            }

            // Escape single quotes to avoid breaking the filter expression
            string escapedTerm = searchTerm.Replace("'", "''");

            // Verify that the expected columns exist (for debugging)
            if (!originalData.Columns.Contains("Username") ||
                !originalData.Columns.Contains("Email") ||
                !originalData.Columns.Contains("Details"))
            {
                System.Diagnostics.Debug.WriteLine("AdminStatsDetails: Missing expected columns in DataTable.");
                // Fallback to manual filtering (safe but slower)
                DataTable filtered = originalData.Clone();
                foreach (DataRow row in originalData.Rows)
                {
                    string username = row["Username"].ToString();
                    string email = row["Email"].ToString();
                    string details = row["Details"].ToString();
                    if (username.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        email.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        details.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        filtered.ImportRow(row);
                    }
                }
                BindGrid(filtered);
                return;
            }

            // Apply filter using DataView (faster, but requires exact column names)
            DataView dv = originalData.DefaultView;
            dv.RowFilter = string.Format(
                "Username LIKE '%{0}%' OR Email LIKE '%{0}%' OR Details LIKE '%{0}%'",
                escapedTerm);
            BindGrid(dv.ToTable());
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            if (originalData != null)
                BindGrid(originalData);
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("AdminDashboard.aspx");
        }
    }
}