using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Net.Mail;
using System.Threading;

namespace HabiTrackers.ReminderService
{
    class Program
    {
        private static string connStr = ConfigurationManager.ConnectionStrings["HabiConn"]?.ConnectionString ?? "";
        private static bool isRunning = true;

        static void Main(string[] args)
        {
            if (string.IsNullOrEmpty(connStr))
            {
                Console.WriteLine("ERROR: Database connection string not found in App.config");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("HabiTrackers Reminder Service Started");
            Console.WriteLine($"Checking for due reminders every 60 seconds");
            Console.WriteLine("Press Ctrl+C to stop...");

            // Handle Ctrl+C to gracefully stop
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Stopping service...");
                isRunning = false;
                e.Cancel = true;
            };

            // Main loop
            while (isRunning)
            {
                try
                {
                    CheckAndSendReminders();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                // Wait 60 seconds before next check
                for (int i = 0; i < 60 && isRunning; i++)
                {
                    Thread.Sleep(1000);
                }
            }

            Console.WriteLine("Service stopped.");
        }

        static void CheckAndSendReminders()
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Checking for due reminders...");

            DataTable? tasksDue = GetTasksDueForReminder();

            if (tasksDue == null || tasksDue.Rows.Count == 0)
            {
                Console.WriteLine("No due reminders found.");
                return;
            }

            Console.WriteLine($"Found {tasksDue.Rows.Count} due reminders.");

            foreach (DataRow row in tasksDue.Rows)
            {
                int taskId = Convert.ToInt32(row["TaskId"]);
                string taskName = row["TaskName"]?.ToString() ?? "Untitled Task";
                string description = row["Description"]?.ToString() ?? "";
                DateTime reminderTime = Convert.ToDateTime(row["ReminderTime"]);
                string email = row["Email"]?.ToString() ?? "";
                string username = row["Username"]?.ToString() ?? "User";

                if (string.IsNullOrEmpty(email))
                {
                    Console.WriteLine($"Warning: No email found for task ID {taskId}");
                    MarkReminderSent(taskId); // Mark as sent to avoid repeated failures
                    continue;
                }

                Console.WriteLine($"Sending reminder for task '{taskName}' to {email}");

                bool emailSent = SendEmailReminder(email, username, taskName, description, reminderTime);

                if (emailSent)
                {
                    MarkReminderSent(taskId);
                    Console.WriteLine($"✓ Reminder sent and marked as delivered.");
                }
                else
                {
                    Console.WriteLine($"✗ Failed to send reminder.");
                }
            }
        }

        static DataTable? GetTasksDueForReminder()
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                return null;
            }
        }

        static void MarkReminderSent(int taskId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    SqlCommand cmd = new SqlCommand("UPDATE DailyTasks SET ReminderSent = 1 WHERE TaskId = @id", conn);
                    cmd.Parameters.AddWithValue("@id", taskId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking reminder as sent: {ex.Message}");
            }
        }

        static bool SendEmailReminder(string toEmail, string username, string taskName, string description, DateTime reminderTime)
        {
            try
            {
                string? smtpServer = ConfigurationManager.AppSettings["SmtpServer"];
                string? smtpPortStr = ConfigurationManager.AppSettings["SmtpPort"];
                string? smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
                string? smtpPass = ConfigurationManager.AppSettings["SmtpPass"];
                string? enableSslStr = ConfigurationManager.AppSettings["SmtpEnableSsl"];
                string? fromEmail = ConfigurationManager.AppSettings["FromEmail"];

                // Validate settings
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPortStr) ||
                    string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass) ||
                    string.IsNullOrEmpty(fromEmail))
                {
                    Console.WriteLine("Email settings missing in App.config");
                    return false;
                }

                int smtpPort = Convert.ToInt32(smtpPortStr);
                bool enableSsl = Convert.ToBoolean(enableSslStr ?? "true");

                using (SmtpClient client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = enableSsl;
                    client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass);

                    using (MailMessage message = new MailMessage())
                    {
                        message.From = new MailAddress(fromEmail, "HabiTrackers");
                        message.To.Add(new MailAddress(toEmail));  // This will throw if email invalid
                        message.Subject = $"Task Reminder: {taskName}";
                        message.Body = $@"
                            <html>
                            <body>
                                <h2>Task Reminder from HabiTrackers</h2>
                                <p>Hello {username},</p>
                                <p>This is a reminder for your task:</p>
                                <div style='background:#f8f9fa; padding:15px; border-left:4px solid #01889f; margin:15px 0;'>
                                    <h3 style='color:#01889f; margin:0;'>{taskName}</h3>
                                    <p style='margin:10px 0;'><strong>Description:</strong> {description}</p>
                                    <p style='margin:0;'><strong>Reminder Time:</strong> {reminderTime:dd MMM yyyy HH:mm}</p>
                                </div>
                                <p>Log in to HabiTrackers to mark this task as complete.</p>
                                <hr>
                                <p style='color:#999; font-size:0.9em;'>This is an automated reminder from your HabiTrackers app.</p>
                            </body>
                            </html>";
                        message.IsBodyHtml = true;

                        client.Send(message);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email error: {ex.Message}");
                return false;
            }
        }
    }
}