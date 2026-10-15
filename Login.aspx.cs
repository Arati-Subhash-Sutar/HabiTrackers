using System;
using System.Data;
using System.Web.Services;
using System.Diagnostics;

namespace HabiTrackers
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                bool isAdminLogin = Request.QueryString["admin"] == "true";
                if (isAdminLogin)
                {
                    // Admin login mode – hide signup and forgot password
                    txtUsername.Text = "admin";
                    txtPassword.Focus();
                    lblMessage.Text = "Enter admin password";

                    signupPanel.Visible = false;
                    forgotLinkDiv.Visible = false;
                    toggleLinkDiv.Visible = false;
                }
                else
                {
                    // Normal user login – show everything
                    signupPanel.Visible = true;
                    forgotLinkDiv.Visible = true;
                    toggleLinkDiv.Visible = true;
                }
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                int userId;
                string role;
                if (DBHelper.ValidateUser(txtUsername.Text, txtPassword.Text, out userId, out role))
                {
                    Session["UserId"] = userId;
                    Session["Username"] = txtUsername.Text;
                    Session["Role"] = role;

                    DataTable user = DBHelper.GetUserById(userId);
                    if (user.Rows.Count > 0)
                        Session["Email"] = user.Rows[0]["Email"].ToString();

                    if (role == "Admin")
                        Response.Redirect("AdminDashboard.aspx");
                    else
                        Response.Redirect("Dashboard.aspx");
                }
                else
                {
                    lblMessage.Text = "Invalid username or password!";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Login error: " + ex.Message;
                Debug.WriteLine(ex.ToString());
            }
        }

        protected void btnSignup_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    lblMessage.Text = "Email is required.";
                    return;
                }

                int newUserId = DBHelper.RegisterUser(txtNewUsername.Text, txtNewPassword.Text, txtEmail.Text);
                int userId;
                string role;
                if (DBHelper.ValidateUser(txtNewUsername.Text, txtNewPassword.Text, out userId, out role))
                {
                    Session["UserId"] = userId;
                    Session["Username"] = txtNewUsername.Text;
                    Session["Role"] = role;
                    Session["Email"] = txtEmail.Text;
                    Response.Redirect("Dashboard.aspx");
                }
                else
                {
                    lblMessage.Text = "Account created but auto-login failed. Please login manually.";
                    ClientScript.RegisterStartupScript(GetType(), "ShowLogin", "showLogin();", true);
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Signup error: " + ex.Message;
            }
        }

        [WebMethod]
       
        public static object SendOTP(string email)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SendOTP called for email: " + email);

                if (string.IsNullOrWhiteSpace(email))
                    return new { success = false, message = "Email is required." };

                if (!DBHelper.EmailExists(email))
                    return new { success = false, message = "Email not registered." };

                int userId = DBHelper.GetUserIdByEmail(email);
                if (userId == 0)
                    return new { success = false, message = "User not found." };

                string otp = new Random().Next(100000, 999999).ToString();
                DBHelper.SaveOTP(userId, otp, 5);

                bool sent = DBHelper.SendOTPEmail(email, otp);
                System.Diagnostics.Debug.WriteLine("SendOTPEmail returned: " + sent);

                if (!sent)
                    return new { success = false, message = "Failed to send email. Please check SMTP settings." };

                return new { success = true, message = "OTP sent to your email address." };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SendOTP Exception: " + ex.ToString());
                return new { success = false, message = "Server error: " + ex.Message };
            }
        }
        [WebMethod]
        public static object ResetPassword(string email, string otp, string newPassword)
        {
            try
            {
                if (!DBHelper.ValidateOTP(email, otp))
                    return new { success = false, message = "Invalid or expired OTP." };
                DBHelper.UpdatePassword(email, newPassword);
                DBHelper.MarkOTPUsed(email, otp);
                return new { success = true, message = "Password reset successfully. You can now login." };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ResetPassword error: " + ex.ToString());
                return new { success = false, message = "An error occurred: " + ex.Message };
            }
        }
    }
}