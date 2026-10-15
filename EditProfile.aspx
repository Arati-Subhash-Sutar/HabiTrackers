<%@ Page Title="Edit Profile" Language="C#" MasterPageFile="~/MasterUser.master" AutoEventWireup="true" CodeBehind="EditProfile.aspx.cs" Inherits="HabiTrackers.EditProfile" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div class="page-container">
        <div class="card" style="max-width:500px; margin:0 auto;">
            <h2>Edit Profile</h2>
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />
            <div style="margin-bottom:1rem;">
                <label>Username:</label>
                <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" />
                <asp:RequiredFieldValidator ID="rfvUsername" runat="server" ControlToValidate="txtUsername" ErrorMessage="Username is required." ForeColor="Red" Display="Dynamic" />
            </div>
            <div style="margin-bottom:1rem;">
                <label>Email:</label>
                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" />
                <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="Email is required." ForeColor="Red" Display="Dynamic" />
                <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" ErrorMessage="Invalid email format." ForeColor="Red" Display="Dynamic" />
            </div>
            <div style="margin-bottom:1rem;">
                <label>New Password (leave blank to keep current):</label>
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" />
            </div>
            <div style="margin-bottom:1rem;">
                <asp:CheckBox ID="chkChangePassword" runat="server" Text="Change Password" AutoPostBack="true" OnCheckedChanged="chkChangePassword_CheckedChanged" />
            </div>
            <div style="margin-bottom:1rem; display:flex; gap:1rem;">
                <asp:Button ID="btnSave" runat="server" Text="Save Changes" OnClick="btnSave_Click" CssClass="btn-primary" style="padding:10px 20px;" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" CssClass="btn-secondary" CausesValidation="false" style="padding:10px 20px;" />
            </div>
        </div>
    </div>
</asp:Content>