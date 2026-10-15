<%@ Page Title="Study Timer" Language="C#" MasterPageFile="~/MasterUser.master" AutoEventWireup="true" CodeBehind="StudyTimer.aspx.cs" Inherits="HabiTrackers.StudyTimer" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-container">
        <div class="card" style="text-align:center; max-width:500px; margin:2rem auto;">
            <h2>Study Timer</h2>
            
            <!-- Timer Display -->
            <div class="timer-circle" id="timerDisplay">25:00</div>
            <div id="timerStatus" style="font-size:1.2rem; margin:1rem 0; font-weight:500;">Work Session</div>
            
            <!-- Session Counter - Improved styling -->
            <div style="background:#01889f; padding:1rem; border-radius:10px; margin:1rem 0; color:white;">
                <div style="display:flex; align-items:center; justify-content:center; gap:0.5rem;">
                    <span style="font-size:1.1rem;">Sessions completed today:</span>
                    <span id="sessionCounter" style="font-size:2.5rem; font-weight:bold; margin-left:0.5rem;">0</span>
                </div>
            </div>

            <!-- Controls -->
            <div style="display:flex; gap:1rem; justify-content:center; margin:2rem 0;">
                <input type="button" id="btnStartPause" value="Start" class="btn-primary" style="padding:15px 30px; font-size:1.1rem; border:none; cursor:pointer;" />
                <input type="button" id="btnReset" value="Reset" class="btn-primary" style="padding:15px 30px; font-size:1.1rem; background:#6c757d; border:none; cursor:pointer;" />
            </div>

            <!-- Timer Settings -->
            <div style="background:#f8f9fa; padding:1.5rem; border-radius:10px; margin-top:2rem;">
                <h3>Settings</h3>
                <div style="display:flex; gap:1rem; align-items:center; flex-wrap:wrap; justify-content:center;">
                    <label>Work (min):</label>
                    <asp:TextBox ID="txtWorkMinutes" runat="server" Text="25" style="width:60px; padding:5px;" />
                    <label>Break (min):</label>
                    <asp:TextBox ID="txtBreakMinutes" runat="server" Text="5" style="width:60px; padding:5px;" />
                    <asp:Button ID="btnUpdateSettings" runat="server" Text="Update" OnClick="btnUpdateSettings_Click" CssClass="btn-primary" />
                </div>
            </div>
        </div>
    </div>

    <!-- Hidden audio elements for notifications -->
    <audio id="workCompleteSound" preload="auto">
        <source src="data:audio/wav;base64,UklGRlwAAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YVQAAACAgICAf39/f39/f3+AgICAf39/f39/f3+AgICAf39/f39/f3+AgICAf39/f39/f3+AgICAf39/f39/f3+AgICAf39/f39/f38=" type="audio/wav">
    </audio>
    <audio id="breakCompleteSound" preload="auto">
        <source src="data:audio/wav;base64,UklGRlwAAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YVQAAACAgICAf39/f39/f3+AgICAf39/f39/f3+AgICAf39/f39/f3+AgICAf39/f39/f3+AgICAf39/f39/f3+AgICAf39/f39/f38=" type="audio/wav">
    </audio>

    <script type="text/javascript">
        // Global timer variables
        let workDuration = parseInt('<%= Session["WorkDuration"] ?? 25 %>') * 60;
        let breakDuration = parseInt('<%= Session["BreakDuration"] ?? 5 %>') * 60;
        let sessionsCompleted = parseInt('<%= GetTodaySessions() %>');

        let timerInterval = null;
        let timeLeft = workDuration;
        let isRunning = false;
        let isWork = true;

        // DOM elements
        const timerDisplay = document.getElementById('timerDisplay');
        const timerStatus = document.getElementById('timerStatus');
        const sessionCounter = document.getElementById('sessionCounter');
        const startPauseBtn = document.getElementById('btnStartPause');
        const resetBtn = document.getElementById('btnReset');

        // Audio elements
        const workSound = document.getElementById('workCompleteSound');
        const breakSound = document.getElementById('breakCompleteSound');

        // Initialize display
        function updateDisplay() {
            let minutes = Math.floor(timeLeft / 60);
            let seconds = timeLeft % 60;
            timerDisplay.innerText = minutes.toString().padStart(2, '0') + ':' + seconds.toString().padStart(2, '0');
        }

        // Play sound only - no alert
        function playSound(type) {
            try {
                if (type === 'work') {
                    workSound.play().catch(e => console.log('Sound play failed:', e));
                } else {
                    breakSound.play().catch(e => console.log('Sound play failed:', e));
                }
            } catch (e) {
                console.log('Sound error:', e);
            }
        }

        // Timer logic - removed alerts
        function startTimer() {
            if (isRunning) return;
            isRunning = true;
            startPauseBtn.value = 'Pause';

            timerInterval = setInterval(function () {
                if (timeLeft <= 0) {
                    // Timer finished
                    if (isWork) {
                        // Work session completed
                        sessionsCompleted++;
                        sessionCounter.innerText = sessionsCompleted;
                        saveWorkSession(workDuration);
                        playSound('work');
                        isWork = false;
                        timeLeft = breakDuration;
                        timerStatus.innerText = 'Break Session';
                    } else {
                        // Break session completed
                        playSound('break');
                        isWork = true;
                        timeLeft = workDuration;
                        timerStatus.innerText = 'Work Session';
                    }
                } else {
                    timeLeft--;
                }
                updateDisplay();
            }, 1000);
        }

        function pauseTimer() {
            if (timerInterval) {
                clearInterval(timerInterval);
                timerInterval = null;
            }
            isRunning = false;
            startPauseBtn.value = 'Start';
        }

        function toggleTimer() {
            if (isRunning) {
                pauseTimer();
            } else {
                startTimer();
            }
        }

        function resetTimer() {
            pauseTimer();
            isWork = true;
            timeLeft = workDuration;
            timerStatus.innerText = 'Work Session';
            updateDisplay();
        }

        // AJAX function to save completed work session
        function saveWorkSession(workSeconds) {
            var xhr = new XMLHttpRequest();
            xhr.open('POST', 'StudyTimer.aspx/SaveWorkSession', true);
            xhr.setRequestHeader('Content-Type', 'application/json');

            var data = JSON.stringify({ workSeconds: workSeconds });
            xhr.send(data);
        }

        // Add event listeners when page loads
        document.addEventListener('DOMContentLoaded', function () {
            // Initialize display
            timeLeft = workDuration;
            updateDisplay();
            sessionCounter.innerText = sessionsCompleted;

            // Add click event listeners
            startPauseBtn.addEventListener('click', function (e) {
                e.preventDefault();
                toggleTimer();
            });

            resetBtn.addEventListener('click', function (e) {
                e.preventDefault();
                resetTimer();
            });
        });
    </script>
</asp:Content>