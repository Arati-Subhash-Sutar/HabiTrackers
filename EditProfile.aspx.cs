using System;
using System.Data;

namespace HabiTrackers
{
    public partial class EditProfile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                LoadUserData();
            }
        }

        private void LoadUserData()
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetUserById(userId);
            if (dt.Rows.Count > 0)
            {
                txtUsername.Text = dt.Rows[0]["Username"].ToString();
                txtEmail.Text = dt.Rows[0]["Email"].ToString();
            }
            txtPassword.Enabled = false;
        }

        protected void chkChangePassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.Enabled = chkChangePassword.Checked;
            if (!chkChangePassword.Checked)
                txtPassword.Text = "";
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            int userId = (int)Session["UserId"];
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = chkChangePassword.Checked ? txtPassword.Text.Trim() : null;

            try
            {
                DBHelper.UpdateUser(userId, username, email, password);

                // Update session
                Session["Username"] = username;
                Session["Email"] = email;

                lblMessage.ForeColor = System.Drawing.Color.Green;
                lblMessage.Text = "Profile updated successfully!";
            }
            catch (Exception ex)
            {
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = ex.Message;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("Dashboard.aspx");
        }
    }
}