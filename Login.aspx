<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="HabiTrackers.Login" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Login / Signup - HabiTrackers</title>
    <link href="Styles/site.css" rel="stylesheet" />
    <style>
        /* (same as before – unchanged) */
        body {
            margin: 0;
            padding: 0;
            min-height: 100vh;
            background-image: url('../images/login background_1.jpg');
            background-repeat: no-repeat;
            background-size: cover;
            background-position: center center;
            background-attachment: fixed;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        }
        .wrapper {
            max-width: 1300px;
            width: 90%;
            margin: 2rem auto;
            display: flex;
            gap: 2rem;
            min-height: 80vh;
        }
        .left-panel { flex: 1; }
        .right-panel {
            flex: 1;
            background: white;
            border-radius: 28px;
            padding: 2rem;
            box-shadow: 0 20px 35px rgba(0,0,0,0.2);
        }
        .form-container { max-width: 450px; margin: 0 auto; }
        .form-container h2 { text-align: center; color: #01889f; margin-bottom: 1.5rem; }
        .form-control {
            width: 100%;
            padding: 12px 16px;
            margin: 10px 0;
            border: 2px solid #e1e5e9;
            border-radius: 16px;
            font-size: 16px;
            box-sizing: border-box;
        }
        .form-control:focus {
            outline: none;
            border-color: #01889f;
            box-shadow: 0 0 0 3px rgba(1,136,159,0.2);
        }
        .btn-primary {
            background: #01889f;
            color: white;
            padding: 12px 24px;
            border: none;
            border-radius: 40px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            width: 100%;
            margin-top: 1rem;
        }
        .btn-primary:hover { background: #016c7e; transform: translateY(-2px); }
        .forgot-link { text-align: right; margin-top: 0.5rem; }
        .forgot-link a { color: #01889f; text-decoration: none; font-size: 0.85rem; }
        .message-label { display: block; text-align: center; margin-top: 1rem; font-weight: 500; }
        .toggle-link { text-align: center; margin-top: 1rem; }
        .toggle-link a { color: #01889f; cursor: pointer; text-decoration: underline; }
        .modal {
            display: none;
            position: fixed;
            z-index: 1000;
            left: 0;
            top: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0,0,0,0.5);
            align-items: center;
            justify-content: center;
        }
        .modal-content {
            background: white;
            border-radius: 20px;
            padding: 2rem;
            width: 90%;
            max-width: 400px;
            position: relative;
        }
        .close {
            position: absolute;
            right: 1rem;
            top: 0.5rem;
            font-size: 1.5rem;
            cursor: pointer;
        }
    </style>
    <script type="text/javascript">
        function showLogin() {
            document.getElementById('loginPanel').style.display = 'block';
            document.getElementById('signupPanel').style.display = 'none';
        }
        function showSignup() {
            document.getElementById('loginPanel').style.display = 'none';
            document.getElementById('signupPanel').style.display = 'block';
        }
        function showForgotModal() {
            document.getElementById('forgotModal').style.display = 'flex';
            document.getElementById('modalEmail').value = '';
            document.getElementById('modalOtp').value = '';
            document.getElementById('modalNewPassword').value = '';
            document.getElementById('modalConfirmPassword').value = '';
            document.getElementById('stepEmail').style.display = 'block';
            document.getElementById('stepReset').style.display = 'none';
            document.getElementById('modalMessage').innerHTML = '';
        }
        function closeModal() {
            document.getElementById('forgotModal').style.display = 'none';
        }
        function sendOTP() {
            var email = document.getElementById('modalEmail').value.trim();
            if (!email) { document.getElementById('modalMessage').innerHTML = 'Please enter your email address.'; return; }
            PageMethods.SendOTP(email, onSendOTPSuccess, onSendOTPFail);
        }
        function onSendOTPSuccess(result) {
            if (result.success) {
                document.getElementById('stepEmail').style.display = 'none';
                document.getElementById('stepReset').style.display = 'block';
                document.getElementById('modalMessage').innerHTML = result.message;
            } else {
                document.getElementById('modalMessage').innerHTML = result.message;
            }
        }
        function onSendOTPFail(error) {
            document.getElementById('modalMessage').innerHTML = 'Server error. Please try again.';
        }
        function resetPassword() {
            var email = document.getElementById('modalEmail').value.trim();
            var otp = document.getElementById('modalOtp').value.trim();
            var newPwd = document.getElementById('modalNewPassword').value;
            var confirmPwd = document.getElementById('modalConfirmPassword').value;
            if (!otp || !newPwd) { document.getElementById('modalMessage').innerHTML = 'Please enter OTP and new password.'; return; }
            if (newPwd !== confirmPwd) { document.getElementById('modalMessage').innerHTML = 'Passwords do not match.'; return; }
            PageMethods.ResetPassword(email, otp, newPwd, onResetSuccess, onResetFail);
        }
        function onResetSuccess(result) {
            if (result.success) {
                document.getElementById('modalMessage').innerHTML = result.message;
                setTimeout(function () { closeModal(); }, 2000);
            } else {
                document.getElementById('modalMessage').innerHTML = result.message;
            }
        }
        function onResetFail(error) {
            document.getElementById('modalMessage').innerHTML = 'Server error. Please try again.';
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
        <div class="wrapper">
            <div class="left-panel"></div>
            <div class="right-panel">
                <div class="form-container">
                    <!-- LOGIN PANEL -->
                    <div id="loginPanel">
                        <h2>Login</h2>
                        <asp:TextBox ID="txtUsername" runat="server" placeholder="Username" CssClass="form-control"></asp:TextBox>
                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="Password" CssClass="form-control"></asp:TextBox>
                        <asp:Button ID="btnLogin" runat="server" Text="Login" OnClick="btnLogin_Click" CssClass="btn-primary" />

                        <div id="forgotLinkDiv" runat="server" class="forgot-link">
                            <a href="#" onclick="showForgotModal(); return false;">Forgot Password?</a>
                        </div>

                        <div id="toggleLinkDiv" runat="server" class="toggle-link">
                            <a onclick="showSignup(); return false;">Don't have an account? Sign up</a>
                        </div>
                    </div>

                    <!-- SIGNUP PANEL -->
                    <div id="signupPanel" runat="server" style="display:none;">
                        <h2>Create Account</h2>
                        <asp:TextBox ID="txtNewUsername" runat="server" placeholder="Username" CssClass="form-control"></asp:TextBox>
                        <asp:TextBox ID="txtEmail" runat="server" placeholder="Email" CssClass="form-control" TextMode="Email"></asp:TextBox>
                        <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password" placeholder="Password" CssClass="form-control"></asp:TextBox>
                        <asp:Button ID="btnSignup" runat="server" Text="Sign Up" OnClick="btnSignup_Click" CssClass="btn-primary" />
                        <div class="toggle-link">
                            <a onclick="showLogin(); return false;">Already have an account? Login</a>
                        </div>
                    </div>

                    <asp:Label ID="lblMessage" runat="server" CssClass="message-label" ForeColor="#c0392b"></asp:Label>
                </div>
            </div>
        </div>

        <!-- Forgot password modal (unchanged) -->
        <div id="forgotModal" class="modal">
            <div class="modal-content">
                <span class="close" onclick="closeModal()">&times;</span>
                <h3>Reset Password</h3>
                <div id="modalMessage" style="color:red;"></div>
                <div id="stepEmail">
                    <label>Email Address:</label>
                    <input type="email" id="modalEmail" class="form-control" />
                    <button type="button" class="btn-primary" onclick="sendOTP()">Send OTP</button>
                </div>
                <div id="stepReset" style="display:none;">
                    <label>OTP:</label>
                    <input type="text" id="modalOtp" class="form-control" maxlength="6" />
                    <label>New Password:</label>
                    <input type="password" id="modalNewPassword" class="form-control" />
                    <label>Confirm Password:</label>
                    <input type="password" id="modalConfirmPassword" class="form-control" />
                    <button type="button" class="btn-primary" onclick="resetPassword()">Reset Password</button>
                </div>
            </div>
        </div>
    </form>
</body>
</html>