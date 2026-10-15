using System;
using System.Data;
using System.Web.UI.WebControls;

namespace HabiTrackers
{
    public partial class Habits : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
            {
                Response.Redirect("Default.aspx");
            }

            if (!IsPostBack)
            {
                LoadHabits();
                UpdateBatchCounts();
                pnlBatchDetails.Visible = false;
            }
        }

        private void LoadHabits()
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetUserHabits(userId);
            gvHabits.DataSource = dt;
            gvHabits.DataBind();
        }

        private void UpdateBatchCounts()
        {
            int userId = (int)Session["UserId"];

            int bronzeCount = DBHelper.GetBronzeBatchCount(userId);
            int silverCount = DBHelper.GetSilverBatchCount(userId);
            int goldCount = DBHelper.GetGoldBatchCount(userId);
            int totalBatches = bronzeCount + silverCount + goldCount;

            lblBronzeCount.Text = bronzeCount.ToString();
            lblSilverCount.Text = silverCount.ToString();
            lblGoldCount.Text = goldCount.ToString();
            lblBatchCount.Text = totalBatches.ToString();
        }

        protected void gvHabits_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int habitId = Convert.ToInt32(gvHabits.DataKeys[e.Row.RowIndex].Value);
                Literal litBatch = (Literal)e.Row.FindControl("litBatch");

                if (litBatch != null)
                {
                    if (DBHelper.HasBatch(habitId, "Gold"))
                    {
                        litBatch.Text = "<span style='background:#ffd700; color:black; padding:2px 10px; border-radius:15px;'> Gold</span>";
                    }
                    else if (DBHelper.HasBatch(habitId, "Silver"))
                    {
                        litBatch.Text = "<span style='background:#c0c0c0; color:black; padding:2px 10px; border-radius:15px;'> Silver</span>";
                    }
                    else if (DBHelper.HasBatch(habitId, "Bronze"))
                    {
                        litBatch.Text = "<span style='background:#cd7f32; color:white; padding:2px 10px; border-radius:15px;'> Bronze</span>";
                    }
                    else
                    {
                        litBatch.Text = "<span style='color:#999;'>-</span>";
                    }
                }
            }
        }

        protected void btnBronze_Click(object sender, EventArgs e)
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetHabitsByBatchLevel(userId, "Bronze");
            gvBatchDetails.DataSource = dt;
            gvBatchDetails.DataBind();
            batchTitle.InnerHtml = " Bronze Batches (" + dt.Rows.Count + ")";
            pnlBatchDetails.Visible = true;
        }

        protected void btnSilver_Click(object sender, EventArgs e)
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetHabitsByBatchLevel(userId, "Silver");
            gvBatchDetails.DataSource = dt;
            gvBatchDetails.DataBind();
            batchTitle.InnerHtml = " Silver Batches (" + dt.Rows.Count + ")";
            pnlBatchDetails.Visible = true;
        }

        protected void btnGold_Click(object sender, EventArgs e)
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetHabitsByBatchLevel(userId, "Gold");
            gvBatchDetails.DataSource = dt;
            gvBatchDetails.DataBind();
            batchTitle.InnerHtml = " Gold Batches (" + dt.Rows.Count + ")";
            pnlBatchDetails.Visible = true;
        }

        protected void btnCloseDetails_Click(object sender, EventArgs e)
        {
            pnlBatchDetails.Visible = false;
        }

        protected void btnAddHabit_Click(object sender, EventArgs e)
        {
            // Clear previous message
            lblAddHabitMessage.Text = "";

            // Validate input
            if (string.IsNullOrWhiteSpace(txtHabitName.Text))
            {
                lblAddHabitMessage.Text = "Please enter a habit name.";
                lblAddHabitMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            int userId = (int)Session["UserId"];
            string habitName = txtHabitName.Text.Trim();

            // Check for duplicate habit name
            if (DBHelper.HabitExists(userId, habitName))
            {
                lblAddHabitMessage.Text = "You already have a habit with that name. Please choose a different name.";
                lblAddHabitMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string level = ddlLevel.SelectedValue;

            // Add the habit
            DBHelper.AddHabit(userId, habitName, level);
            txtHabitName.Text = "";
            lblAddHabitMessage.Text = "Habit added!";
            lblAddHabitMessage.ForeColor = System.Drawing.ColorTranslator.FromHtml("#01889f");

            // Refresh the lists
            LoadHabits();
            UpdateBatchCounts();
            lblMessage.Text = ""; // clear any old general message
        }

        protected void gvHabits_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int habitId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "CompleteHabit")
            {
                DBHelper.MarkHabitCompleted(habitId, DateTime.Today);
                lblMessage.Text = "Habit completed!";
                lblMessage.ForeColor = System.Drawing.ColorTranslator.FromHtml("#01889f");
                LoadHabits();
                UpdateBatchCounts();
            }
            else if (e.CommandName == "DeleteHabit")
            {
                DBHelper.DeleteHabit(habitId);
                lblMessage.Text = "Habit deleted.";
                lblMessage.ForeColor = System.Drawing.ColorTranslator.FromHtml("#01889f");
                LoadHabits();
                UpdateBatchCounts();
            }
        }
    }
}