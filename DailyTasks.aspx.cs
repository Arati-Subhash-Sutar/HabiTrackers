using System;
using System.Data;
using System.Web.UI.WebControls;

namespace HabiTrackers
{
    public partial class DailyTasks : System.Web.UI.Page
    {
        private DataTable allTasks;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null) Response.Redirect("Default.aspx");
            if (!IsPostBack)
            {
                ViewState["SearchTerm"] = "";
                LoadTasks();
            }
        }

        void LoadTasks()
        {
            int userId = (int)Session["UserId"];
            allTasks = DBHelper.GetUserTasks(userId);

            string search = ViewState["SearchTerm"]?.ToString();
            if (!string.IsNullOrEmpty(search))
            {
                allTasks.DefaultView.RowFilter = string.Format("TaskName LIKE '%{0}%' OR Description LIKE '%{0}%'", search.Replace("'", "''"));
                allTasks = allTasks.DefaultView.ToTable();
            }

            // Split into pending and completed
            DataTable pendingTasks = allTasks.Clone();
            DataTable completedTasks = allTasks.Clone();

            foreach (DataRow row in allTasks.Rows)
            {
                if (Convert.ToBoolean(row["IsCompleted"]))
                    completedTasks.ImportRow(row);
                else
                    pendingTasks.ImportRow(row);
            }

            gvPendingTasks.DataSource = pendingTasks;
            gvPendingTasks.DataBind();

            gvCompletedTasks.DataSource = completedTasks;
            gvCompletedTasks.DataBind();
        }

        protected void btnAddTask_Click(object sender, EventArgs e)
        {
            int userId = (int)Session["UserId"];
            DateTime? reminder = null;
            if (!string.IsNullOrEmpty(txtReminder.Text))
                reminder = DateTime.Parse(txtReminder.Text);

            DBHelper.AddTask(userId, txtTaskName.Text, txtTaskDesc.Text, reminder);
            txtTaskName.Text = "";
            txtTaskDesc.Text = "";
            txtReminder.Text = "";
            lblMessage.Text = "Task added successfully!";
            LoadTasks();
        }

        protected void chkCompleted_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            GridViewRow row = (GridViewRow)chk.NamingContainer;
            int taskId = Convert.ToInt32(gvPendingTasks.DataKeys[row.RowIndex].Value);

            DBHelper.ToggleTaskComplete(taskId);
            lblMessage.Text = "Task marked as completed!";
            LoadTasks();
        }

        // PENDING TASKS EVENTS
        protected void gvPendingTasks_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteTask")
            {
                int taskId = Convert.ToInt32(e.CommandArgument);
                DBHelper.DeleteTask(taskId);
                lblMessage.Text = "Task deleted!";
                LoadTasks();
            }
        }

        protected void gvPendingTasks_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvPendingTasks.EditIndex = e.NewEditIndex;
            LoadTasks();
        }

        protected void gvPendingTasks_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvPendingTasks.EditIndex = -1;
            LoadTasks();
        }

        protected void gvPendingTasks_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int taskId = Convert.ToInt32(gvPendingTasks.DataKeys[e.RowIndex].Value);
            GridViewRow row = gvPendingTasks.Rows[e.RowIndex];

            TextBox txtName = (TextBox)row.Cells[1].Controls[0];
            TextBox txtDesc = (TextBox)row.Cells[2].Controls[0];
            TextBox txtReminder = (TextBox)row.FindControl("txtEditReminder");

            string name = txtName.Text;
            string desc = txtDesc.Text;
            DateTime? reminder = null;
            if (!string.IsNullOrEmpty(txtReminder.Text))
                reminder = DateTime.Parse(txtReminder.Text);

            DBHelper.UpdateTask(taskId, name, desc, reminder);

            gvPendingTasks.EditIndex = -1;
            LoadTasks();
            lblMessage.Text = "Task updated!";
        }

        protected void gvPendingTasks_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPendingTasks.PageIndex = e.NewPageIndex;
            LoadTasks();
        }

        // COMPLETED TASKS EVENTS (Only Delete)
        protected void gvCompletedTasks_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "DeleteTask")
            {
                int taskId = Convert.ToInt32(e.CommandArgument);
                DBHelper.DeleteTask(taskId);
                lblMessage.Text = "Task deleted!";
                LoadTasks();
            }
        }

        protected void gvCompletedTasks_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvCompletedTasks.PageIndex = e.NewPageIndex;
            LoadTasks();
        }

        // Search
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            ViewState["SearchTerm"] = txtSearch.Text.Trim();
            LoadTasks();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ViewState["SearchTerm"] = "";
            LoadTasks();
        }
    }
}