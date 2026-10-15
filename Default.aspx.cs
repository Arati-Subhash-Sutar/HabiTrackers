using System;

namespace HabiTrackers
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void btnGetStarted_Click(object sender, EventArgs e)
        {
            Response.Redirect("Login.aspx");
        }

        protected void btnAdminRedirect_Click(object sender, EventArgs e)
        {
            Response.Redirect("Login.aspx?admin=true");
        }
    }
}