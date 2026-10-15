using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Net.Mail;

public static class DBHelper
{
    private static string connStr = WebConfigurationManager.ConnectionStrings["HabiConn"].ConnectionString;

    // ========== USER AUTHENTICATION & PROFILE ==========
    public static bool ValidateUser(string username, string password, out int userId, out string role)
    {
        userId = 0; role = "";
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = "UPDATE Users SET LastLogin=GETDATE() WHERE Username=@u; SELECT UserId, Role FROM Users WHERE Username=@u AND Password=@p";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password);
            conn.Open();
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    userId = reader.GetInt32(0);
                    // Handle NULL role
                    object roleObj = reader["Role"];
                    role = (roleObj == DBNull.Value) ? "User" : roleObj.ToString();
                    return true;
                }
            }
        }
        return false;
    }
    public static int RegisterUser(string username, string password, string email)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("INSERT INTO Users (Username, Password, Email) OUTPUT INSERTED.UserId VALUES (@u, @p, @e)", conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password);
            cmd.Parameters.AddWithValue("@e", email);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    public static DataTable GetUserById(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("SELECT UserId, Username, Email, Role FROM Users WHERE UserId=@uid", conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public static void UpdateUser(int userId, string username, string email, string password = null)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            conn.Open();
            if (!string.IsNullOrEmpty(username))
            {
                SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username=@u AND UserId != @uid", conn);
                checkCmd.Parameters.AddWithValue("@u", username);
                checkCmd.Parameters.AddWithValue("@uid", userId);
                int exists = (int)checkCmd.ExecuteScalar();
                if (exists > 0)
                    throw new Exception("Username already exists.");
            }

            string query = "UPDATE Users SET Username=@u, Email=@e";
            if (!string.IsNullOrEmpty(password))
                query += ", Password=@p";
            query += " WHERE UserId=@uid";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@uid", userId);
            if (!string.IsNullOrEmpty(password))
                cmd.Parameters.AddWithValue("@p", password);
            cmd.ExecuteNonQuery();
        }
    }

    // ========== FORGOT PASSWORD ==========
public static bool EmailExists(string email)
{
    using (SqlConnection conn = new SqlConnection(connStr))
    {
        SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @email", conn);
        cmd.Parameters.AddWithValue("@email", email);
        conn.Open();
        return (int)cmd.ExecuteScalar() > 0;
    }
}

    public static int GetUserIdByEmail(string email)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            // Changed column from "Id" to "UserId"
            SqlCommand cmd = new SqlCommand("SELECT UserId FROM Users WHERE Email = @email", conn);
            cmd.Parameters.AddWithValue("@email", email);
            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? (int)result : 0;
        }
    }
    // SaveOTP – already correct, but ensure it uses UserId
    public static void SaveOTP(int userId, string otp, int expiryMinutes = 5)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = "INSERT INTO UserOTP (UserId, OTP, ExpiryDate) VALUES (@uid, @otp, DATEADD(MINUTE, @expiry, GETDATE()))";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@otp", otp);
            cmd.Parameters.AddWithValue("@expiry", expiryMinutes);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    // CORRECTED ValidateOTP – use u.UserId instead of u.Id
    public static bool ValidateOTP(string email, string otp)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
            SELECT COUNT(*) 
            FROM UserOTP o
            INNER JOIN Users u ON o.UserId = u.UserId   -- fixed: u.UserId
            WHERE u.Email = @email AND o.OTP = @otp AND o.ExpiryDate > GETDATE() AND o.IsUsed = 0";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@otp", otp);
            conn.Open();
            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }
    }

    // CORRECTED MarkOTPUsed – same join fix
    public static void MarkOTPUsed(string email, string otp)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
            UPDATE o SET IsUsed = 1
            FROM UserOTP o
            INNER JOIN Users u ON o.UserId = u.UserId   -- fixed: u.UserId
            WHERE u.Email = @email AND o.OTP = @otp AND o.IsUsed = 0";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@otp", otp);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
    public static void UpdatePassword(string email, string newPassword)
{
    using (SqlConnection conn = new SqlConnection(connStr))
    {
        SqlCommand cmd = new SqlCommand("UPDATE Users SET Password = @pwd WHERE Email = @email", conn);
        cmd.Parameters.AddWithValue("@pwd", newPassword);
        cmd.Parameters.AddWithValue("@email", email);
        conn.Open();
        cmd.ExecuteNonQuery();
    }
}

    // ========== FORGOT PASSWORD METHODS ==========
    public static bool SendOTPEmail(string email, string otp)
    {
        try
        {
            string smtpServer = System.Web.Configuration.WebConfigurationManager.AppSettings["SmtpServer"];
            int smtpPort = Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["SmtpPort"]);
            string smtpUser = System.Web.Configuration.WebConfigurationManager.AppSettings["SmtpUser"];
            string smtpPass = System.Web.Configuration.WebConfigurationManager.AppSettings["SmtpPass"];
            bool enableSsl = Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["SmtpEnableSsl"]);
            string fromEmail = System.Web.Configuration.WebConfigurationManager.AppSettings["FromEmail"];

            using (System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(smtpServer, smtpPort))
            {
                client.EnableSsl = enableSsl;
                client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);

                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                message.From = new System.Net.Mail.MailAddress(fromEmail, "HabiTrackers");
                message.To.Add(email);
                message.Subject = "Password Reset OTP";
                message.Body = "<html><body><h2>Password Reset Request</h2><p>Your OTP for resetting your password is: <strong>" + otp + "</strong></p><p>This OTP is valid for 5 minutes.</p><p>If you did not request this, please ignore this email.</p></body></html>";
                message.IsBodyHtml = true;

                client.Send(message);
            }
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("SendOTPEmail error: " + ex.Message);
            return false;
        }
    }

    // ========== DAILY TASKS ==========
    public static DataTable GetUserTasks(int userId, DateTime? date = null)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string dateFilter = date.HasValue ? "AND CAST(TaskDate AS DATE)=@date" : "";
            string query = "SELECT TaskId, TaskName, Description, IsCompleted, TaskDate, CreatedTime, ReminderTime FROM DailyTasks WHERE UserId=@uid " + dateFilter + " ORDER BY CreatedTime DESC";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            if (date.HasValue) cmd.Parameters.AddWithValue("@date", date.Value.Date);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public static void AddTask(int userId, string taskName, string description, DateTime? reminderTime = null)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("INSERT INTO DailyTasks (UserId, TaskName, Description, ReminderTime) VALUES (@uid, @name, @desc, @reminder)", conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@name", taskName);
            cmd.Parameters.AddWithValue("@desc", description ?? "");
            cmd.Parameters.AddWithValue("@reminder", reminderTime.HasValue ? (object)reminderTime.Value : DBNull.Value);
            conn.Open();
            cmd.ExecuteNonQuery();
            AddRecentActivity(userId, "Task", taskName, "Added new task");
        }
    }

    public static void ToggleTaskComplete(int taskId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("UPDATE DailyTasks SET IsCompleted = CASE WHEN IsCompleted = 1 THEN 0 ELSE 1 END WHERE TaskId=@id", conn);
            cmd.Parameters.AddWithValue("@id", taskId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public static void DeleteTask(int taskId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("DELETE FROM DailyTasks WHERE TaskId=@id", conn);
            cmd.Parameters.AddWithValue("@id", taskId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public static void UpdateTask(int taskId, string taskName, string description, DateTime? reminderTime)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = "UPDATE DailyTasks SET TaskName=@name, Description=@desc, ReminderTime=@reminder WHERE TaskId=@id";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", taskName);
            cmd.Parameters.AddWithValue("@desc", description ?? "");
            cmd.Parameters.AddWithValue("@reminder", reminderTime.HasValue ? (object)reminderTime.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@id", taskId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public static DataTable GetTasksDueForReminder()
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                SELECT t.TaskId, t.TaskName, t.Description, t.ReminderTime, u.UserId, u.Email, u.Username
                FROM DailyTasks t
                INNER JOIN Users u ON t.UserId = u.UserId
                WHERE t.ReminderTime IS NOT NULL 
                    AND t.ReminderTime <= GETDATE() 
                    AND t.ReminderSent = 0";
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public static void MarkReminderSent(int taskId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("UPDATE DailyTasks SET ReminderSent = 1 WHERE TaskId = @id", conn);
            cmd.Parameters.AddWithValue("@id", taskId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    // ========== NOTES ==========
    public static DataTable GetUserNotes(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("SELECT NoteId, UserId, NoteTitle, NoteContent, AttachmentPath, CreatedDate FROM UserNotes WHERE UserId=@uid ORDER BY CreatedDate DESC", conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public static void SaveNote(int userId, string title, string content, string attachmentPath = null)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("INSERT INTO UserNotes (UserId, NoteTitle, NoteContent, AttachmentPath) VALUES (@uid, @title, @content, @att)", conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@content", content);
            cmd.Parameters.AddWithValue("@att", (object)attachmentPath ?? DBNull.Value);
            conn.Open();
            cmd.ExecuteNonQuery();
            AddRecentActivity(userId, "Note", title, "Added new note");
        }
    }

    public static DataTable GetNoteById(int noteId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("SELECT NoteId, UserId, NoteTitle, NoteContent, AttachmentPath, CreatedDate FROM UserNotes WHERE NoteId=@id", conn);
            cmd.Parameters.AddWithValue("@id", noteId);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public static void UpdateNote(int noteId, string title, string content, string attachmentPath = null)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = "UPDATE UserNotes SET NoteTitle=@title, NoteContent=@content";
            if (attachmentPath != null)
                query += ", AttachmentPath=@att";
            query += " WHERE NoteId=@id";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@content", content);
            cmd.Parameters.AddWithValue("@id", noteId);
            if (attachmentPath != null)
                cmd.Parameters.AddWithValue("@att", attachmentPath);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public static void DeleteNote(int noteId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("DELETE FROM UserNotes WHERE NoteId=@id", conn);
            cmd.Parameters.AddWithValue("@id", noteId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    // ========== TIMETABLE ==========
    public static DataTable GetUserTimetable(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("SELECT EntryId, UserId, Day, StartTime, EndTime, Task, IsBreak FROM TimetableEntries WHERE UserId=@uid ORDER BY CASE Day WHEN 'Sunday' THEN 0 WHEN 'Monday' THEN 1 WHEN 'Tuesday' THEN 2 WHEN 'Wednesday' THEN 3 WHEN 'Thursday' THEN 4 WHEN 'Friday' THEN 5 WHEN 'Saturday' THEN 6 END, StartTime", conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public static void SaveTimetableEntry(int userId, string day, string startTime, string endTime, string task, bool isBreak)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("INSERT INTO TimetableEntries (UserId, Day, StartTime, EndTime, Task, IsBreak) VALUES (@uid, @day, @start, @end, @task, @isbreak)", conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@day", day);
            cmd.Parameters.AddWithValue("@start", startTime);
            cmd.Parameters.AddWithValue("@end", endTime);
            cmd.Parameters.AddWithValue("@task", task);
            cmd.Parameters.AddWithValue("@isbreak", isBreak);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public static void UpdateTimetableEntry(int entryId, string day, string startTime, string endTime, string task, bool isBreak)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("UPDATE TimetableEntries SET Day=@day, StartTime=@start, EndTime=@end, Task=@task, IsBreak=@isbreak WHERE EntryId=@id", conn);
            cmd.Parameters.AddWithValue("@day", day);
            cmd.Parameters.AddWithValue("@start", startTime);
            cmd.Parameters.AddWithValue("@end", endTime);
            cmd.Parameters.AddWithValue("@task", task);
            cmd.Parameters.AddWithValue("@isbreak", isBreak);
            cmd.Parameters.AddWithValue("@id", entryId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public static void DeleteTimetableEntry(int entryId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("DELETE FROM TimetableEntries WHERE EntryId=@id", conn);
            cmd.Parameters.AddWithValue("@id", entryId);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    // ========== STUDY TIMER ==========
    public static void SaveStudySession(int userId, int workDuration, int breakDuration, int sessions, int totalWork, int totalBreak)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("INSERT INTO StudySessions (UserId, WorkDuration, BreakDuration, SessionsCompleted, TotalWorkTime, TotalBreakTime) VALUES (@uid, @workdur, @breakdur, @sessions, @totalwork, @totalbreak)", conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@workdur", workDuration);
            cmd.Parameters.AddWithValue("@breakdur", breakDuration);
            cmd.Parameters.AddWithValue("@sessions", sessions);
            cmd.Parameters.AddWithValue("@totalwork", totalWork);
            cmd.Parameters.AddWithValue("@totalbreak", totalBreak);
            conn.Open();
            cmd.ExecuteNonQuery();
            string minutes = (workDuration / 60).ToString();
            AddRecentActivity(userId, "Session", "Study Session", minutes + " minutes completed");
        }
    }

    // ========== ADMIN DASHBOARD ==========
    public static DataTable GetAdminStats()
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                SELECT 
                    (SELECT COUNT(*) FROM Users WHERE CAST(LastLogin AS DATE)=CAST(GETDATE() AS DATE)) as TodayLogins,
                    (SELECT COUNT(*) FROM Users WHERE CAST(CreatedDate AS DATE)=CAST(GETDATE() AS DATE)) as NewUsers,
                    (SELECT COUNT(*) FROM DailyTasks WHERE CAST(TaskDate AS DATE)=CAST(GETDATE() AS DATE) AND IsCompleted=1) as TasksCompletedToday,
                    (SELECT COUNT(*) FROM UserNotes WHERE CAST(CreatedDate AS DATE)=CAST(GETDATE() AS DATE)) as NotesToday,
                    (SELECT COUNT(*) FROM HabitCompletions WHERE CompletionDate = CAST(GETDATE() AS DATE)) as HabitsCompletedToday,
                    (SELECT COUNT(*) FROM StudySessions WHERE CAST(SessionDate AS DATE)=CAST(GETDATE() AS DATE)) as TotalSessions,
                    (SELECT COUNT(*) FROM DailyTasks WHERE CAST(ReminderTime AS DATE)=CAST(GETDATE() AS DATE) AND ReminderSent=1) as RemindersSentToday";
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public static DataTable GetAllUsers()
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                SELECT u.UserId, u.Password, u.Username, u.Email, u.Role, u.CreatedDate, u.LastLogin,
                    ISNULL((SELECT CurrentStreak FROM UserStreaks WHERE UserId = u.UserId), 0) as CurrentStreak,
                    ISNULL((SELECT LongestStreak FROM UserStreaks WHERE UserId = u.UserId), 0) as LongestStreak
                FROM Users u 
                ORDER BY u.UserId ASC";
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public static void DeleteUserWithAllData(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            conn.Open();
            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                SqlCommand cmd1 = new SqlCommand("DELETE hc FROM HabitCompletions hc INNER JOIN Habits h ON hc.HabitId = h.HabitId WHERE h.UserId = @uid", conn, trans);
                cmd1.Parameters.AddWithValue("@uid", userId);
                cmd1.ExecuteNonQuery();

                SqlCommand cmd2 = new SqlCommand("DELETE hb FROM HabitBatches hb INNER JOIN Habits h ON hb.HabitId = h.HabitId WHERE h.UserId = @uid", conn, trans);
                cmd2.Parameters.AddWithValue("@uid", userId);
                cmd2.ExecuteNonQuery();

                SqlCommand cmd3 = new SqlCommand("DELETE hs FROM HabitStreaks hs INNER JOIN Habits h ON hs.HabitId = h.HabitId WHERE h.UserId = @uid", conn, trans);
                cmd3.Parameters.AddWithValue("@uid", userId);
                cmd3.ExecuteNonQuery();

                SqlCommand cmd4 = new SqlCommand("DELETE FROM Habits WHERE UserId = @uid", conn, trans);
                cmd4.Parameters.AddWithValue("@uid", userId);
                cmd4.ExecuteNonQuery();

                SqlCommand cmd5 = new SqlCommand("DELETE FROM RecentActivities WHERE UserId = @uid", conn, trans);
                cmd5.Parameters.AddWithValue("@uid", userId);
                cmd5.ExecuteNonQuery();

                SqlCommand cmd6 = new SqlCommand("DELETE FROM DiaryEntries WHERE UserId = @uid", conn, trans);
                cmd6.Parameters.AddWithValue("@uid", userId);
                cmd6.ExecuteNonQuery();

                SqlCommand cmd7 = new SqlCommand("DELETE FROM TimetableEntries WHERE UserId = @uid", conn, trans);
                cmd7.Parameters.AddWithValue("@uid", userId);
                cmd7.ExecuteNonQuery();

                SqlCommand cmd8 = new SqlCommand("DELETE FROM StudySessions WHERE UserId = @uid", conn, trans);
                cmd8.Parameters.AddWithValue("@uid", userId);
                cmd8.ExecuteNonQuery();

                SqlCommand cmd9 = new SqlCommand("DELETE FROM DailyTasks WHERE UserId = @uid", conn, trans);
                cmd9.Parameters.AddWithValue("@uid", userId);
                cmd9.ExecuteNonQuery();

                SqlCommand cmd10 = new SqlCommand("DELETE FROM UserNotes WHERE UserId = @uid", conn, trans);
                cmd10.Parameters.AddWithValue("@uid", userId);
                cmd10.ExecuteNonQuery();

                SqlCommand cmd11 = new SqlCommand("DELETE FROM UserStreaks WHERE UserId = @uid", conn, trans);
                cmd11.Parameters.AddWithValue("@uid", userId);
                cmd11.ExecuteNonQuery();

                SqlCommand cmd12 = new SqlCommand("DELETE FROM Users WHERE UserId = @uid", conn, trans);
                cmd12.Parameters.AddWithValue("@uid", userId);
                cmd12.ExecuteNonQuery();

                trans.Commit();
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }
    }

    // ========== ADMIN DASHBOARD STATS (Direct from Source Tables) ==========
    public static DataTable GetWeeklyAdminReport()
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                WITH Dates AS (
                    SELECT DATEADD(day, -6, CAST(GETDATE() AS DATE)) AS Date
                    UNION ALL
                    SELECT DATEADD(day, 1, Date) FROM Dates WHERE Date < CAST(GETDATE() AS DATE)
                )
                SELECT 
                    d.Date,
                    DATENAME(WEEKDAY, d.Date) as DayOfWeek,
                    ISNULL(t.TasksCount, 0) as TasksCompleted,
                    ISNULL(h.HabitsCount, 0) as HabitsCompleted,
                    ISNULL(n.NotesCount, 0) as NotesCreated,
                    ISNULL(s.SessionsCount, 0) as StudySessions
                FROM Dates d
                LEFT JOIN (
                    SELECT TaskDate, COUNT(*) as TasksCount
                    FROM DailyTasks
                    WHERE IsCompleted = 1 AND TaskDate >= DATEADD(day, -6, CAST(GETDATE() AS DATE))
                    GROUP BY TaskDate
                ) t ON d.Date = t.TaskDate
                LEFT JOIN (
                    SELECT CompletionDate, COUNT(*) as HabitsCount
                    FROM HabitCompletions hc
                    INNER JOIN Habits h ON hc.HabitId = h.HabitId
                    WHERE hc.CompletionDate >= DATEADD(day, -6, CAST(GETDATE() AS DATE))
                    GROUP BY CompletionDate
                ) h ON d.Date = h.CompletionDate
                LEFT JOIN (
                    SELECT CAST(CreatedDate AS DATE) as NoteDate, COUNT(*) as NotesCount
                    FROM UserNotes
                    WHERE CreatedDate >= DATEADD(day, -6, CAST(GETDATE() AS DATE))
                    GROUP BY CAST(CreatedDate AS DATE)
                ) n ON d.Date = n.NoteDate
                LEFT JOIN (
                    SELECT SessionDate, COUNT(*) as SessionsCount
                    FROM StudySessions
                    WHERE SessionDate >= DATEADD(day, -6, CAST(GETDATE() AS DATE))
                    GROUP BY SessionDate
                ) s ON d.Date = s.SessionDate
                ORDER BY d.Date";
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public static DataTable GetMonthlyAdminReport()
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                WITH Dates AS (
                    SELECT DATEADD(day, -29, CAST(GETDATE() AS DATE)) AS Date
                    UNION ALL
                    SELECT DATEADD(day, 1, Date) FROM Dates WHERE Date < CAST(GETDATE() AS DATE)
                )
                SELECT 
                    d.Date,
                    DAY(d.Date) as DayOfMonth,
                    DATENAME(MONTH, d.Date) as Month,
                    ISNULL(t.TasksCount, 0) as TasksCompleted,
                    ISNULL(h.HabitsCount, 0) as HabitsCompleted,
                    ISNULL(n.NotesCount, 0) as NotesCreated,
                    ISNULL(s.SessionsCount, 0) as StudySessions
                FROM Dates d
                LEFT JOIN (
                    SELECT TaskDate, COUNT(*) as TasksCount
                    FROM DailyTasks
                    WHERE IsCompleted = 1 AND TaskDate >= DATEADD(day, -29, CAST(GETDATE() AS DATE))
                    GROUP BY TaskDate
                ) t ON d.Date = t.TaskDate
                LEFT JOIN (
                    SELECT CompletionDate, COUNT(*) as HabitsCount
                    FROM HabitCompletions hc
                    INNER JOIN Habits h ON hc.HabitId = h.HabitId
                    WHERE hc.CompletionDate >= DATEADD(day, -29, CAST(GETDATE() AS DATE))
                    GROUP BY CompletionDate
                ) h ON d.Date = h.CompletionDate
                LEFT JOIN (
                    SELECT CAST(CreatedDate AS DATE) as NoteDate, COUNT(*) as NotesCount
                    FROM UserNotes
                    WHERE CreatedDate >= DATEADD(day, -29, CAST(GETDATE() AS DATE))
                    GROUP BY CAST(CreatedDate AS DATE)
                ) n ON d.Date = n.NoteDate
                LEFT JOIN (
                    SELECT SessionDate, COUNT(*) as SessionsCount
                    FROM StudySessions
                    WHERE SessionDate >= DATEADD(day, -29, CAST(GETDATE() AS DATE))
                    GROUP BY SessionDate
                ) s ON d.Date = s.SessionDate
                ORDER BY d.Date";
            SqlCommand cmd = new SqlCommand(query, conn);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    // ========== DIARY FEATURE ==========
    public static DataTable GetDiaryEntry(int userId, DateTime date)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = "SELECT * FROM DiaryEntries WHERE UserId = @uid AND EntryDate = @date";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@date", date.Date);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public static void SaveDiaryEntry(int userId, DateTime entryDate, string content)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            conn.Open();
            SqlCommand checkCmd = new SqlCommand("SELECT EntryId FROM DiaryEntries WHERE UserId = @uid AND EntryDate = @date", conn);
            checkCmd.Parameters.AddWithValue("@uid", userId);
            checkCmd.Parameters.AddWithValue("@date", entryDate.Date);
            object existingId = checkCmd.ExecuteScalar();
            if (existingId != null)
            {
                SqlCommand updateCmd = new SqlCommand("UPDATE DiaryEntries SET Content = @content, CreatedDate = GETDATE() WHERE EntryId = @id", conn);
                updateCmd.Parameters.AddWithValue("@content", content);
                updateCmd.Parameters.AddWithValue("@id", (int)existingId);
                updateCmd.ExecuteNonQuery();
            }
            else
            {
                SqlCommand insertCmd = new SqlCommand("INSERT INTO DiaryEntries (UserId, EntryDate, Content) VALUES (@uid, @date, @content)", conn);
                insertCmd.Parameters.AddWithValue("@uid", userId);
                insertCmd.Parameters.AddWithValue("@date", entryDate.Date);
                insertCmd.Parameters.AddWithValue("@content", content);
                insertCmd.ExecuteNonQuery();
            }
        }
    }

    public static void DeleteDiaryEntry(int userId, DateTime entryDate)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("DELETE FROM DiaryEntries WHERE UserId = @uid AND EntryDate = @date", conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@date", entryDate.Date);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public static DataTable SearchDiaryEntries(int userId, DateTime? startDate, DateTime? endDate)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = "SELECT EntryId, EntryDate, LEFT(Content, 100) as Preview, CreatedDate FROM DiaryEntries WHERE UserId = @uid";
            if (startDate.HasValue)
                query += " AND EntryDate >= @start";
            if (endDate.HasValue)
                query += " AND EntryDate <= @end";
            query += " ORDER BY EntryDate DESC";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            if (startDate.HasValue)
                cmd.Parameters.AddWithValue("@start", startDate.Value.Date);
            if (endDate.HasValue)
                cmd.Parameters.AddWithValue("@end", endDate.Value.Date);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    // ========== HABITS FEATURE ==========
    public static DataTable GetUserHabits(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                SELECT 
                    h.HabitId,
                    h.HabitName,
                    h.Level,
                    h.TargetDays,
                    (SELECT COUNT(*) FROM HabitCompletions WHERE HabitId = h.HabitId) AS TotalCompletions,
                    CASE WHEN EXISTS (
                        SELECT 1 FROM HabitCompletions 
                        WHERE HabitId = h.HabitId AND CompletionDate = CAST(GETDATE() AS DATE)
                    ) THEN 1 ELSE 0 END AS CompletedToday
                FROM Habits h
                WHERE h.UserId = @uid AND h.IsActive = 1
                ORDER BY h.CreatedDate DESC";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public static void AddHabit(int userId, string habitName, string level)
    {
        int targetDays = level == "Easy" ? 21 : (level == "Medium" ? 30 : 45);
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand(
                "INSERT INTO Habits (UserId, HabitName, TargetDays, Level) VALUES (@uid, @name, @target, @level)", conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@name", habitName);
            cmd.Parameters.AddWithValue("@target", targetDays);
            cmd.Parameters.AddWithValue("@level", level);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public static void MarkHabitCompleted(int habitId, DateTime date)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            conn.Open();

            SqlCommand checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM HabitCompletions WHERE HabitId = @hid AND CompletionDate = @date", conn);
            checkCmd.Parameters.AddWithValue("@hid", habitId);
            checkCmd.Parameters.AddWithValue("@date", date.Date);
            int exists = (int)checkCmd.ExecuteScalar();

            if (exists == 0)
            {
                SqlCommand getHabitCmd = new SqlCommand(
                    "SELECT HabitName, UserId FROM Habits WHERE HabitId = @hid", conn);
                getHabitCmd.Parameters.AddWithValue("@hid", habitId);
                SqlDataReader reader = getHabitCmd.ExecuteReader();
                string habitName = "";
                int userId = 0;
                if (reader.Read())
                {
                    habitName = reader["HabitName"].ToString();
                    userId = Convert.ToInt32(reader["UserId"]);
                }
                reader.Close();

                SqlCommand insertCmd = new SqlCommand(
                    "INSERT INTO HabitCompletions (HabitId, CompletionDate) VALUES (@hid, @date)", conn);
                insertCmd.Parameters.AddWithValue("@hid", habitId);
                insertCmd.Parameters.AddWithValue("@date", date.Date);
                insertCmd.ExecuteNonQuery();

                UpdateHabitStreak(habitId, date.Date);
                UpdateUserStreak(userId);
                AddRecentActivity(userId, "Habit", habitName, "Completed for " + date.ToString("dd MMM yyyy"));
                CheckAndAwardBatches(habitId);
            }
        }
    }

    private static void CheckAndAwardBatches(int habitId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            conn.Open();

            SqlCommand getHabitCmd = new SqlCommand(
                "SELECT Level FROM Habits WHERE HabitId = @hid", conn);
            getHabitCmd.Parameters.AddWithValue("@hid", habitId);
            object levelObj = getHabitCmd.ExecuteScalar();
            string level = levelObj != null ? levelObj.ToString() : "";

            SqlCommand countCmd = new SqlCommand(
                "SELECT COUNT(*) FROM HabitCompletions WHERE HabitId = @hid", conn);
            countCmd.Parameters.AddWithValue("@hid", habitId);
            int total = (int)countCmd.ExecuteScalar();

            if (level == "Easy" && total >= 21)
            {
                AwardBatchIfNotExists(habitId, "Bronze");
            }
            else if (level == "Medium" && total >= 30)
            {
                AwardBatchIfNotExists(habitId, "Silver");
            }
            else if (level == "Hard" && total >= 45)
            {
                AwardBatchIfNotExists(habitId, "Gold");
            }
        }
    }

    private static void AwardBatchIfNotExists(int habitId, string batchLevel)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM HabitBatches WHERE HabitId = @hid AND BatchLevel = @level", conn);
            checkCmd.Parameters.AddWithValue("@hid", habitId);
            checkCmd.Parameters.AddWithValue("@level", batchLevel);
            conn.Open();
            int exists = (int)checkCmd.ExecuteScalar();

            if (exists == 0)
            {
                SqlCommand insertCmd = new SqlCommand(
                    "INSERT INTO HabitBatches (HabitId, BatchLevel) VALUES (@hid, @level)", conn);
                insertCmd.Parameters.AddWithValue("@hid", habitId);
                insertCmd.Parameters.AddWithValue("@level", batchLevel);
                insertCmd.ExecuteNonQuery();
            }
        }
    }

    public static bool HasBatch(int habitId, string batchLevel)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM HabitBatches WHERE HabitId = @hid AND BatchLevel = @level", conn);
            cmd.Parameters.AddWithValue("@hid", habitId);
            cmd.Parameters.AddWithValue("@level", batchLevel);
            conn.Open();
            return (int)cmd.ExecuteScalar() > 0;
        }
    }

    public static int GetUserBatchCount(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                SELECT COUNT(*) 
                FROM HabitBatches b
                INNER JOIN Habits h ON b.HabitId = h.HabitId
                WHERE h.UserId = @uid AND h.IsActive = 1";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    // ========== BATCH COUNT METHODS ==========
    public static int GetBronzeBatchCount(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                SELECT COUNT(*) 
                FROM HabitBatches b
                INNER JOIN Habits h ON b.HabitId = h.HabitId
                WHERE h.UserId = @uid AND b.BatchLevel = 'Bronze' AND h.IsActive = 1";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    public static int GetSilverBatchCount(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                SELECT COUNT(*) 
                FROM HabitBatches b
                INNER JOIN Habits h ON b.HabitId = h.HabitId
                WHERE h.UserId = @uid AND b.BatchLevel = 'Silver' AND h.IsActive = 1";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    public static int GetGoldBatchCount(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                SELECT COUNT(*) 
                FROM HabitBatches b
                INNER JOIN Habits h ON b.HabitId = h.HabitId
                WHERE h.UserId = @uid AND b.BatchLevel = 'Gold' AND h.IsActive = 1";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    public static DataTable GetHabitsByBatchLevel(int userId, string batchLevel)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                SELECT h.HabitId, h.HabitName, h.Level, h.TargetDays, 
                       (SELECT COUNT(*) FROM HabitCompletions WHERE HabitId = h.HabitId) as TotalCompletions,
                       b.AwardedDate
                FROM Habits h
                INNER JOIN HabitBatches b ON h.HabitId = b.HabitId
                WHERE h.UserId = @uid AND b.BatchLevel = @level AND h.IsActive = 1
                ORDER BY b.AwardedDate DESC";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@level", batchLevel);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public static void DeleteHabit(int habitId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            conn.Open();
            SqlCommand delComp = new SqlCommand("DELETE FROM HabitCompletions WHERE HabitId = @hid", conn);
            delComp.Parameters.AddWithValue("@hid", habitId);
            delComp.ExecuteNonQuery();

            SqlCommand delBatch = new SqlCommand("DELETE FROM HabitBatches WHERE HabitId = @hid", conn);
            delBatch.Parameters.AddWithValue("@hid", habitId);
            delBatch.ExecuteNonQuery();

            SqlCommand delStreak = new SqlCommand("DELETE FROM HabitStreaks WHERE HabitId = @hid", conn);
            delStreak.Parameters.AddWithValue("@hid", habitId);
            delStreak.ExecuteNonQuery();

            SqlCommand delHabit = new SqlCommand("DELETE FROM Habits WHERE HabitId = @hid", conn);
            delHabit.Parameters.AddWithValue("@hid", habitId);
            delHabit.ExecuteNonQuery();
        }
    }

    // ========== HABIT STREAKS ==========
    public static void UpdateHabitStreak(int habitId, DateTime completedDate)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            conn.Open();

            SqlCommand checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM HabitStreaks WHERE HabitId = @hid", conn);
            checkCmd.Parameters.AddWithValue("@hid", habitId);
            int exists = (int)checkCmd.ExecuteScalar();

            if (exists == 0)
            {
                SqlCommand insertCmd = new SqlCommand(
                    "INSERT INTO HabitStreaks (HabitId, CurrentStreak, LongestStreak, LastCompletedDate) VALUES (@hid, 1, 1, @date)", conn);
                insertCmd.Parameters.AddWithValue("@hid", habitId);
                insertCmd.Parameters.AddWithValue("@date", completedDate);
                insertCmd.ExecuteNonQuery();
            }
            else
            {
                SqlCommand getLastCmd = new SqlCommand(
                    "SELECT LastCompletedDate, CurrentStreak, LongestStreak FROM HabitStreaks WHERE HabitId = @hid", conn);
                getLastCmd.Parameters.AddWithValue("@hid", habitId);
                SqlDataReader reader = getLastCmd.ExecuteReader();
                DateTime? lastDate = null;
                int currentStreak = 0;
                int longestStreak = 0;
                if (reader.Read())
                {
                    if (reader["LastCompletedDate"] != DBNull.Value)
                        lastDate = Convert.ToDateTime(reader["LastCompletedDate"]);
                    currentStreak = Convert.ToInt32(reader["CurrentStreak"]);
                    longestStreak = Convert.ToInt32(reader["LongestStreak"]);
                }
                reader.Close();

                if (lastDate.HasValue)
                {
                    TimeSpan diff = completedDate - lastDate.Value;

                    if (diff.Days == 1)
                    {
                        currentStreak++;
                    }
                    else if (diff.Days > 1)
                    {
                        currentStreak = 1;
                    }
                    else if (diff.Days == 0)
                    {
                        return;
                    }
                }
                else
                {
                    currentStreak = 1;
                }

                if (currentStreak > longestStreak)
                    longestStreak = currentStreak;

                SqlCommand updateCmd = new SqlCommand(
                    "UPDATE HabitStreaks SET CurrentStreak = @streak, LongestStreak = @longest, LastCompletedDate = @date WHERE HabitId = @hid", conn);
                updateCmd.Parameters.AddWithValue("@streak", currentStreak);
                updateCmd.Parameters.AddWithValue("@longest", longestStreak);
                updateCmd.Parameters.AddWithValue("@date", completedDate);
                updateCmd.Parameters.AddWithValue("@hid", habitId);
                updateCmd.ExecuteNonQuery();
            }
        }
    }

    public static int GetHabitStreak(int habitId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand(
                "SELECT ISNULL(CurrentStreak, 0) FROM HabitStreaks WHERE HabitId = @hid", conn);
            cmd.Parameters.AddWithValue("@hid", habitId);
            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }

    public static bool HabitExists(int userId, string habitName)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            // Check active habits only (IsActive = 1)
            SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Habits WHERE UserId = @uid AND HabitName = @name AND IsActive = 1",
                conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@name", habitName);
            conn.Open();
            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }
    }

    public static int GetLongestHabitStreak(int habitId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand(
                "SELECT ISNULL(LongestStreak, 0) FROM HabitStreaks WHERE HabitId = @hid", conn);
            cmd.Parameters.AddWithValue("@hid", habitId);
            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }

    // ========== USER STREAKS (Based on Habit Completion) ==========
    public static void UpdateUserStreak(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            conn.Open();
            DateTime today = DateTime.Today;

            SqlCommand checkHabitCmd = new SqlCommand(
                "SELECT COUNT(*) FROM HabitCompletions hc " +
                "INNER JOIN Habits h ON hc.HabitId = h.HabitId " +
                "WHERE h.UserId = @uid AND hc.CompletionDate = @date", conn);
            checkHabitCmd.Parameters.AddWithValue("@uid", userId);
            checkHabitCmd.Parameters.AddWithValue("@date", today);
            int habitsToday = (int)checkHabitCmd.ExecuteScalar();

            if (habitsToday == 0) return;

            SqlCommand checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM UserStreaks WHERE UserId = @uid", conn);
            checkCmd.Parameters.AddWithValue("@uid", userId);
            int exists = (int)checkCmd.ExecuteScalar();

            if (exists == 0)
            {
                SqlCommand insertCmd = new SqlCommand(
                    "INSERT INTO UserStreaks (UserId, CurrentStreak, LongestStreak, LastActiveDate) VALUES (@uid, 1, 1, @date)", conn);
                insertCmd.Parameters.AddWithValue("@uid", userId);
                insertCmd.Parameters.AddWithValue("@date", today);
                insertCmd.ExecuteNonQuery();
            }
            else
            {
                SqlCommand getLastCmd = new SqlCommand(
                    "SELECT LastActiveDate, CurrentStreak, LongestStreak FROM UserStreaks WHERE UserId = @uid", conn);
                getLastCmd.Parameters.AddWithValue("@uid", userId);
                SqlDataReader reader = getLastCmd.ExecuteReader();
                DateTime? lastDate = null;
                int currentStreak = 0;
                int longestStreak = 0;
                if (reader.Read())
                {
                    if (reader["LastActiveDate"] != DBNull.Value)
                        lastDate = Convert.ToDateTime(reader["LastActiveDate"]);
                    currentStreak = Convert.ToInt32(reader["CurrentStreak"]);
                    longestStreak = Convert.ToInt32(reader["LongestStreak"]);
                }
                reader.Close();

                int newCurrentStreak = currentStreak;

                if (lastDate.HasValue)
                {
                    TimeSpan diff = today - lastDate.Value;

                    if (diff.Days == 1)
                    {
                        newCurrentStreak = currentStreak + 1;
                        if (newCurrentStreak > longestStreak)
                            longestStreak = newCurrentStreak;
                    }
                    else if (diff.Days > 1)
                    {
                        newCurrentStreak = 1;
                    }
                    else if (diff.Days == 0)
                    {
                        return;
                    }
                }
                else
                {
                    newCurrentStreak = 1;
                }

                SqlCommand updateCmd = new SqlCommand(
                    "UPDATE UserStreaks SET CurrentStreak = @streak, LongestStreak = @longest, LastActiveDate = @date WHERE UserId = @uid", conn);
                updateCmd.Parameters.AddWithValue("@streak", newCurrentStreak);
                updateCmd.Parameters.AddWithValue("@longest", longestStreak);
                updateCmd.Parameters.AddWithValue("@date", today);
                updateCmd.Parameters.AddWithValue("@uid", userId);
                updateCmd.ExecuteNonQuery();
            }
        }
    }

    public static int GetCurrentUserStreak(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand(
                "SELECT ISNULL(CurrentStreak, 0) FROM UserStreaks WHERE UserId = @uid", conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }

    public static int GetLongestUserStreak(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand(
                "SELECT ISNULL(LongestStreak, 0) FROM UserStreaks WHERE UserId = @uid", conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }

    public static bool HasCompletedHabitToday(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                SELECT COUNT(*) FROM HabitCompletions hc 
                INNER JOIN Habits h ON hc.HabitId = h.HabitId 
                WHERE h.UserId = @uid AND hc.CompletionDate = CAST(GETDATE() AS DATE)";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }
    }

    public static int GetTodayHabitCount(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                SELECT COUNT(*) FROM HabitCompletions hc 
                INNER JOIN Habits h ON hc.HabitId = h.HabitId 
                WHERE h.UserId = @uid AND hc.CompletionDate = CAST(GETDATE() AS DATE)";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    public static DateTime? GetLastActiveDate(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand(
                "SELECT LastActiveDate FROM UserStreaks WHERE UserId = @uid", conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            object result = cmd.ExecuteScalar();
            return result != DBNull.Value ? (DateTime?)result : null;
        }
    }

    // ========== RECENT ACTIVITIES ==========
    public static void AddRecentActivity(int userId, string activityType, string activityTitle, string details = null)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand(
                "INSERT INTO RecentActivities (UserId, ActivityType, ActivityTitle, ActivityDate, Details) VALUES (@uid, @type, @title, GETDATE(), @details)", conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            cmd.Parameters.AddWithValue("@type", activityType);
            cmd.Parameters.AddWithValue("@title", activityTitle);
            cmd.Parameters.AddWithValue("@details", (object)details ?? DBNull.Value);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public static DataTable GetRecentActivities(int userId, int count = 10)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                SELECT TOP (@count) ActivityType, ActivityTitle, ActivityDate, Details
                FROM RecentActivities
                WHERE UserId = @uid
                ORDER BY ActivityDate DESC";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@count", count);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    // ========== DASHBOARD STATS (Direct from Source Tables) ==========
    public static DataTable GetWeeklyReport(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                WITH Dates AS (
                    SELECT DATEADD(day, -6, CAST(GETDATE() AS DATE)) AS Date
                    UNION ALL
                    SELECT DATEADD(day, 1, Date) FROM Dates WHERE Date < CAST(GETDATE() AS DATE)
                )
                SELECT 
                    d.Date,
                    DATENAME(WEEKDAY, d.Date) as DayOfWeek,
                    ISNULL(h.HabitsCount, 0) as HabitsCompleted,
                    ISNULL(t.TasksCount, 0) as TasksCompleted,
                    ISNULL(s.SessionsCount, 0) as StudySessions
                FROM Dates d
                LEFT JOIN (
                    SELECT CompletionDate, COUNT(*) as HabitsCount
                    FROM HabitCompletions hc
                    INNER JOIN Habits h ON hc.HabitId = h.HabitId
                    WHERE h.UserId = @uid AND hc.CompletionDate >= DATEADD(day, -6, CAST(GETDATE() AS DATE))
                    GROUP BY CompletionDate
                ) h ON d.Date = h.CompletionDate
                LEFT JOIN (
                    SELECT TaskDate, COUNT(*) as TasksCount
                    FROM DailyTasks
                    WHERE UserId = @uid AND IsCompleted = 1 AND TaskDate >= DATEADD(day, -6, CAST(GETDATE() AS DATE))
                    GROUP BY TaskDate
                ) t ON d.Date = t.TaskDate
                LEFT JOIN (
                    SELECT SessionDate, COUNT(*) as SessionsCount
                    FROM StudySessions
                    WHERE UserId = @uid AND SessionDate >= DATEADD(day, -6, CAST(GETDATE() AS DATE))
                    GROUP BY SessionDate
                ) s ON d.Date = s.SessionDate
                ORDER BY d.Date";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public static DataTable GetMonthlyReport(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                WITH Dates AS (
                    SELECT DATEADD(day, -29, CAST(GETDATE() AS DATE)) AS Date
                    UNION ALL
                    SELECT DATEADD(day, 1, Date) FROM Dates WHERE Date < CAST(GETDATE() AS DATE)
                )
                SELECT 
                    d.Date,
                    DAY(d.Date) as DayOfMonth,
                    DATENAME(MONTH, d.Date) as Month,
                    ISNULL(h.HabitsCount, 0) as HabitsCompleted,
                    ISNULL(t.TasksCount, 0) as TasksCompleted,
                    ISNULL(s.SessionsCount, 0) as StudySessions
                FROM Dates d
                LEFT JOIN (
                    SELECT CompletionDate, COUNT(*) as HabitsCount
                    FROM HabitCompletions hc
                    INNER JOIN Habits h ON hc.HabitId = h.HabitId
                    WHERE h.UserId = @uid AND hc.CompletionDate >= DATEADD(day, -29, CAST(GETDATE() AS DATE))
                    GROUP BY CompletionDate
                ) h ON d.Date = h.CompletionDate
                LEFT JOIN (
                    SELECT TaskDate, COUNT(*) as TasksCount
                    FROM DailyTasks
                    WHERE UserId = @uid AND IsCompleted = 1 AND TaskDate >= DATEADD(day, -29, CAST(GETDATE() AS DATE))
                    GROUP BY TaskDate
                ) t ON d.Date = t.TaskDate
                LEFT JOIN (
                    SELECT SessionDate, COUNT(*) as SessionsCount
                    FROM StudySessions
                    WHERE UserId = @uid AND SessionDate >= DATEADD(day, -29, CAST(GETDATE() AS DATE))
                    GROUP BY SessionDate
                ) s ON d.Date = s.SessionDate
                ORDER BY d.Date";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    // ========== USER DETAILS FOR ADMIN ==========
    public static DataTable GetUserFullDetails(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                SELECT 
                    u.Username,
                    u.Email,
                    u.Role,
                    u.CreatedDate,
                    u.LastLogin,
                    ISNULL(us.CurrentStreak, 0) as CurrentStreak,
                    ISNULL(us.LongestStreak, 0) as LongestStreak,
                    (SELECT COUNT(*) FROM HabitCompletions hc 
                     INNER JOIN Habits h ON hc.HabitId = h.HabitId 
                     WHERE h.UserId = @uid AND hc.CompletionDate = CAST(GETDATE() AS DATE)) as TodayHabits,
                    (SELECT COUNT(*) FROM DailyTasks WHERE UserId = @uid) as TotalTasks,
                    (SELECT COUNT(*) FROM DailyTasks WHERE UserId = @uid AND IsCompleted = 0) as PendingTasks,
                    (SELECT COUNT(*) FROM DailyTasks WHERE UserId = @uid AND IsCompleted = 1) as CompletedTasks,
                    (SELECT COUNT(*) FROM Habits WHERE UserId = @uid AND IsActive = 1) as TotalHabits,
                    (SELECT COUNT(*) FROM HabitCompletions hc 
                     INNER JOIN Habits h ON hc.HabitId = h.HabitId 
                     WHERE h.UserId = @uid) as TotalCompletions,
                    (SELECT STRING_AGG(BatchLevel, ',') FROM HabitBatches hb 
                     INNER JOIN Habits h ON hb.HabitId = h.HabitId 
                     WHERE h.UserId = @uid) as BatchesEarned,
                    (SELECT COUNT(*) FROM UserNotes WHERE UserId = @uid) as TotalNotes,
                    (SELECT COUNT(*) FROM UserNotes WHERE UserId = @uid AND CreatedDate >= DATEADD(day, -30, GETDATE())) as RecentNotes,
                    (SELECT COUNT(*) FROM DiaryEntries WHERE UserId = @uid) as TotalDiaryEntries,
                    (SELECT COUNT(*) FROM DiaryEntries WHERE UserId = @uid AND CreatedDate >= DATEADD(day, -30, GETDATE())) as RecentDiaryEntries,
                    (SELECT COUNT(*) FROM TimetableEntries WHERE UserId = @uid) as TotalTimetableEntries,
                    (SELECT COUNT(*) FROM StudySessions WHERE UserId = @uid) as TotalStudySessions,
                    (SELECT ISNULL(SUM(TotalWorkTime), 0) / 60 FROM StudySessions WHERE UserId = @uid) as TotalStudyTime,
                    (SELECT STRING_AGG(HabitName + ' (' + CAST(TotalCompletions AS VARCHAR) + '/' + CAST(TargetDays AS VARCHAR) + ' days)', ' | ') 
                     FROM (SELECT HabitName, TargetDays, (SELECT COUNT(*) FROM HabitCompletions WHERE HabitId = h.HabitId) as TotalCompletions 
                           FROM Habits h WHERE UserId = @uid AND IsActive = 1) as sub) as HabitDetails
                FROM Users u
                LEFT JOIN UserStreaks us ON u.UserId = us.UserId
                WHERE u.UserId = @uid";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public static DataTable GetUserStudySessions(int userId)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"
                SELECT SessionId, SessionDate, WorkDuration, BreakDuration, SessionsCompleted, TotalWorkTime, TotalBreakTime
                FROM StudySessions
                WHERE UserId = @uid
                ORDER BY SessionDate DESC";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    // ========== HELPER METHODS ==========
    public static DataTable ExecuteDataTable(string query, SqlParameter[] parameters)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand(query, conn);
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }

    public static int ExecuteNonQuery(string query, SqlParameter[] parameters)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand(query, conn);
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            conn.Open();
            return cmd.ExecuteNonQuery();
        }
    }

    // ========== ADMIN STATS METHODS (Direct from Source Tables) ==========
    public static DataTable GetAdminStats(DateTime? date = null)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            DateTime targetDate = date ?? DateTime.Today;
            string query = @"
            SELECT 
                (SELECT COUNT(*) FROM Users WHERE CAST(LastLogin AS DATE) = @date) as TodayLogins,
                (SELECT COUNT(*) FROM Users WHERE CAST(CreatedDate AS DATE) = @date) as NewUsers,
                (SELECT COUNT(*) FROM DailyTasks WHERE CAST(TaskDate AS DATE) = @date AND IsCompleted = 1) as TasksCompletedToday,
                (SELECT COUNT(*) FROM UserNotes WHERE CAST(CreatedDate AS DATE) = @date) as NotesToday,
                (SELECT COUNT(*) FROM HabitCompletions hc 
                 INNER JOIN Habits h ON hc.HabitId = h.HabitId 
                 WHERE hc.CompletionDate = @date) as HabitsCompletedToday,
                (SELECT COUNT(*) FROM StudySessions WHERE SessionDate = @date) as TotalSessions,
                (SELECT COUNT(*) FROM DailyTasks WHERE CAST(ReminderTime AS DATE) = @date AND ReminderSent = 1) as RemindersSentToday";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@date", targetDate);
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }
    public static int GetTotalLogins()
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE LastLogin IS NOT NULL", conn);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    public static int GetTotalUsers()
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users", conn);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    public static int GetTotalReminders()
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM DailyTasks WHERE ReminderSent = 1", conn);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    public static int GetTotalHabits()
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Habits WHERE IsActive = 1", conn);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    public static int GetTotalTasks()
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM DailyTasks", conn);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    public static int GetTotalNotes()
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM UserNotes", conn);
            conn.Open();
            return (int)cmd.ExecuteScalar();
        }
    }

    public static DataTable GetStatsDetails(string type, DateTime? date)
    {
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = "";
            bool requiresDate = true;

            switch (type)
            {
                case "logins":
                    query = @"SELECT u.UserId, u.Username, u.Email, CONVERT(VARCHAR, u.LastLogin, 120) as Details
                          FROM Users u WHERE CAST(u.LastLogin AS DATE) = @date ORDER BY u.LastLogin DESC";
                    break;
                case "newusers":
                    query = @"SELECT u.UserId, u.Username, u.Email, CONVERT(VARCHAR, u.CreatedDate, 120) as Details
                          FROM Users u WHERE CAST(u.CreatedDate AS DATE) = @date ORDER BY u.CreatedDate DESC";
                    break;
                case "reminders":
                    query = @"SELECT u.UserId, u.Username, u.Email, t.TaskName as Details
                          FROM DailyTasks t
                          INNER JOIN Users u ON t.UserId = u.UserId
                          WHERE CAST(t.ReminderTime AS DATE) = @date AND t.ReminderSent = 1
                          ORDER BY t.TaskName";
                    break;
                case "habits":
                    query = @"SELECT u.UserId, u.Username, u.Email, h.HabitName as Details
                          FROM HabitCompletions hc
                          INNER JOIN Habits h ON hc.HabitId = h.HabitId
                          INNER JOIN Users u ON h.UserId = u.UserId
                          WHERE hc.CompletionDate = @date
                          ORDER BY u.Username, h.HabitName";
                    break;
                case "tasks":
                    query = @"SELECT u.UserId, u.Username, u.Email, t.TaskName as Details
                          FROM DailyTasks t
                          INNER JOIN Users u ON t.UserId = u.UserId
                          WHERE t.IsCompleted = 1 AND CAST(t.TaskDate AS DATE) = @date
                          ORDER BY u.Username, t.TaskName";
                    break;
                case "notes":
                    query = @"SELECT u.UserId, u.Username, u.Email, n.NoteTitle as Details
                          FROM UserNotes n
                          INNER JOIN Users u ON n.UserId = u.UserId
                          WHERE CAST(n.CreatedDate AS DATE) = @date
                          ORDER BY u.Username, n.NoteTitle";
                    break;
                case "totalhabits":
                    query = @"SELECT u.UserId, u.Username, u.Email, h.HabitName as Details
                          FROM Habits h
                          INNER JOIN Users u ON h.UserId = u.UserId
                          WHERE h.IsActive = 1
                          ORDER BY u.Username, h.HabitName";
                    requiresDate = false;
                    break;
                case "totalnotes":
                    query = @"SELECT u.UserId, u.Username, u.Email, n.NoteTitle as Details
                          FROM UserNotes n
                          INNER JOIN Users u ON n.UserId = u.UserId
                          ORDER BY u.Username, n.NoteTitle";
                    requiresDate = false;
                    break;
                case "totaltasks":
                    query = @"SELECT u.UserId, u.Username, u.Email, t.TaskName as Details
                          FROM DailyTasks t
                          INNER JOIN Users u ON t.UserId = u.UserId
                          ORDER BY u.Username, t.TaskName";
                    requiresDate = false;
                    break;
                case "totalsessions":
                    query = @"SELECT u.UserId, u.Username, u.Email, 
                          CONVERT(VARCHAR, s.SessionDate, 103) + ' - ' + CAST(s.TotalWorkTime/60 AS VARCHAR) + ' mins' as Details
                          FROM StudySessions s
                          INNER JOIN Users u ON s.UserId = u.UserId
                          ORDER BY s.SessionDate DESC, u.Username";
                    requiresDate = false;
                    break;
                default:
                    return new DataTable();
            }

            SqlCommand cmd = new SqlCommand(query, conn);
            if (requiresDate)
            {
                // If date is not provided, use today's date
                DateTime targetDate = date ?? DateTime.Today;
                cmd.Parameters.AddWithValue("@date", targetDate.Date);
            }
            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }
}