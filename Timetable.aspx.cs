using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace HabiTrackers
{
    public partial class Timetable : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null)
                Response.Redirect("Default.aspx");

            if (!IsPostBack)
            {
                LoadTimetable();
            }
        }

        private void LoadTimetable()
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetUserTimetable(userId);

            if (dt.Rows.Count == 0)
            {
                rptTimetable.Visible = false;
                lblNoData.Visible = true;
                return;
            }

            rptTimetable.Visible = true;
            lblNoData.Visible = false;

            // Group by time slot
            var timeSlots = dt.AsEnumerable()
                .Select(r => new
                {
                    Start = r["StartTime"].ToString(),
                    End = r["EndTime"].ToString(),
                    DisplayStart = FormatTimeForDisplay(r["StartTime"].ToString()),
                    DisplayEnd = FormatTimeForDisplay(r["EndTime"].ToString())
                })
                .Distinct()
                .OrderBy(t => t.Start)
                .ToList();

            // Create a structured DataTable for the repeater
            DataTable displayDt = new DataTable();
            displayDt.Columns.Add("TimeSlot", typeof(string));
            displayDt.Columns.Add("Sunday", typeof(string));
            displayDt.Columns.Add("Monday", typeof(string));
            displayDt.Columns.Add("Tuesday", typeof(string));
            displayDt.Columns.Add("Wednesday", typeof(string));
            displayDt.Columns.Add("Thursday", typeof(string));
            displayDt.Columns.Add("Friday", typeof(string));
            displayDt.Columns.Add("Saturday", typeof(string));
            displayDt.Columns.Add("RowData", typeof(string));

            string[] days = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

            foreach (var slot in timeSlots)
            {
                DataRow row = displayDt.NewRow();
                row["TimeSlot"] = slot.DisplayStart + " - " + slot.DisplayEnd;

                // Find the entry for this time slot to get its ID
                var firstEntry = dt.AsEnumerable()
                    .FirstOrDefault(r => r["StartTime"].ToString() == slot.Start && r["EndTime"].ToString() == slot.End);

                string rowData = "";
                if (firstEntry != null)
                {
                    int entryId = Convert.ToInt32(firstEntry["EntryId"]);
                    string day = firstEntry["Day"].ToString();
                    rowData = entryId + "|" + day + "|" + slot.Start + "|" + slot.End;
                }

                foreach (string day in days)
                {
                    var entry = dt.AsEnumerable()
                        .FirstOrDefault(r => r["Day"].ToString() == day &&
                                            r["StartTime"].ToString() == slot.Start &&
                                            r["EndTime"].ToString() == slot.End);

                    if (entry != null)
                    {
                        string task = entry["Task"].ToString();
                        row[day] = task;
                    }
                    else
                    {
                        row[day] = "-";
                    }
                }
                row["RowData"] = rowData;
                displayDt.Rows.Add(row);
            }

            rptTimetable.DataSource = displayDt;
            rptTimetable.DataBind();
        }

        protected void rptTimetable_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView row = (DataRowView)e.Item.DataItem;
                string[] days = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
                string[] dayCellIds = { "cellSunday", "cellMonday", "cellTuesday", "cellWednesday", "cellThursday", "cellFriday", "cellSaturday" };

                // Get the original DataTable with break information
                int userId = (int)Session["UserId"];
                DataTable dt = DBHelper.GetUserTimetable(userId);

                string timeSlot = row["TimeSlot"].ToString();
                string[] times = timeSlot.Split(new string[] { " - " }, StringSplitOptions.None);
                string startTime = ConvertToDbTimeFormat(times[0]);
                string endTime = ConvertToDbTimeFormat(times[1]);

                for (int i = 0; i < days.Length; i++)
                {
                    // Find the cell in the repeater item - use HtmlTableCell
                    System.Web.UI.HtmlControls.HtmlTableCell cell = (System.Web.UI.HtmlControls.HtmlTableCell)e.Item.FindControl(dayCellIds[i]);

                    if (cell != null)
                    {
                        var entry = dt.AsEnumerable()
                            .FirstOrDefault(r => r["Day"].ToString() == days[i] &&
                                                r["StartTime"].ToString() == startTime &&
                                                r["EndTime"].ToString() == endTime);

                        if (entry != null && Convert.ToBoolean(entry["IsBreak"]))
                        {
                            cell.BgColor = "#fff3cd"; // Light yellow background
                            cell.Attributes["title"] = "Break Time"; // Tooltip on hover
                        }
                    }
                }
            }
        }

        private string ConvertToDbTimeFormat(string displayTime)
        {
            // Convert from "HH:mm" to "HH:mm:00"
            if (!string.IsNullOrEmpty(displayTime) && !displayTime.Contains(":"))
                return displayTime;
            return displayTime + ":00";
        }

        private string FormatTimeForDisplay(string time)
        {
            if (string.IsNullOrEmpty(time)) return "";
            string[] parts = time.Split(':');
            if (parts.Length >= 2)
            {
                return parts[0] + ":" + parts[1];
            }
            return time;
        }

        private bool IsValidTime(string time)
        {
            if (string.IsNullOrEmpty(time)) return false;
            string[] parts = time.Split(':');
            if (parts.Length != 2) return false;

            int hour, minute;
            if (!int.TryParse(parts[0], out hour) || !int.TryParse(parts[1], out minute))
                return false;

            return hour >= 0 && hour < 24 && minute >= 0 && minute < 60;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            int userId = (int)Session["UserId"];
            string day = ddlDay.SelectedValue;
            string startTime = txtStartTime.Text + ":00";
            string endTime = txtEndTime.Text + ":00";
            string task = txtTask.Text.Trim();
            bool isBreak = chkIsBreak.Checked;

            try
            {
                DBHelper.SaveTimetableEntry(userId, day, startTime, endTime, task, isBreak);
                ClearForm();
                LoadTimetable();
                ShowMessage("Entry added successfully!", System.Drawing.ColorTranslator.FromHtml("#01889f"));
            }
            catch (Exception ex)
            {
                ShowMessage("Error adding entry: " + ex.Message, System.Drawing.Color.Red);
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            int entryId = Convert.ToInt32(hfEntryId.Value);
            if (entryId == 0)
            {
                ShowMessage("No entry selected for update.", System.Drawing.Color.Red);
                return;
            }

            string day = ddlDay.SelectedValue;
            string startTime = txtStartTime.Text + ":00";
            string endTime = txtEndTime.Text + ":00";
            string task = txtTask.Text.Trim();
            bool isBreak = chkIsBreak.Checked;

            try
            {
                DBHelper.UpdateTimetableEntry(entryId, day, startTime, endTime, task, isBreak);
                ClearForm();
                LoadTimetable();
                ShowMessage("Entry updated successfully!", System.Drawing.ColorTranslator.FromHtml("#01889f"));
            }
            catch (Exception ex)
            {
                ShowMessage("Error updating entry: " + ex.Message, System.Drawing.Color.Red);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtStartTime.Text) ||
                string.IsNullOrWhiteSpace(txtEndTime.Text) ||
                string.IsNullOrWhiteSpace(txtTask.Text))
            {
                ShowMessage("Please fill in all fields.", System.Drawing.Color.Red);
                return false;
            }

            if (!IsValidTime(txtStartTime.Text) || !IsValidTime(txtEndTime.Text))
            {
                ShowMessage("Please enter valid times.", System.Drawing.Color.Red);
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            hfEntryId.Value = "0";
            ddlDay.SelectedIndex = 0;
            txtStartTime.Text = "";
            txtEndTime.Text = "";
            txtTask.Text = "";
            chkIsBreak.Checked = false;

            btnSave.Visible = true;
            btnUpdate.Visible = false;
            btnCancel.Visible = false;
            formTitle.InnerText = "Add New Entry";
        }

        private void ShowMessage(string message, System.Drawing.Color color)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = color;
        }

        protected void rptTimetable_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "EditEntry")
            {
                string rowData = e.CommandArgument.ToString();
                if (!string.IsNullOrEmpty(rowData))
                {
                    string[] data = rowData.Split('|');
                    if (data.Length >= 4)
                    {
                        int entryId = Convert.ToInt32(data[0]);
                        string day = data[1];
                        string startTime = data[2];
                        string endTime = data[3];

                        // Load entry for editing
                        hfEntryId.Value = entryId.ToString();
                        ddlDay.SelectedValue = day;
                        txtStartTime.Text = FormatTimeForDisplay(startTime);
                        txtEndTime.Text = FormatTimeForDisplay(endTime);

                        // Get task and isBreak from database
                        int userId = (int)Session["UserId"];
                        DataTable dt = DBHelper.GetUserTimetable(userId);
                        var entry = dt.AsEnumerable()
                            .FirstOrDefault(r => Convert.ToInt32(r["EntryId"]) == entryId);

                        if (entry != null)
                        {
                            txtTask.Text = entry["Task"].ToString();
                            chkIsBreak.Checked = Convert.ToBoolean(entry["IsBreak"]);
                        }

                        // Show update buttons and hide save button
                        btnSave.Visible = false;
                        btnUpdate.Visible = true;
                        btnCancel.Visible = true;
                        formTitle.InnerText = "Edit Entry";

                        // Scroll to form
                        ScriptManager.RegisterStartupScript(this, GetType(), "scroll",
                            "document.getElementById('" + formTitle.ClientID + "').scrollIntoView({behavior: 'smooth'});", true);
                    }
                }
            }
            else if (e.CommandName == "DeleteEntry")
            {
                string rowData = e.CommandArgument.ToString();
                if (!string.IsNullOrEmpty(rowData))
                {
                    string[] data = rowData.Split('|');
                    if (data.Length >= 1)
                    {
                        int entryId = Convert.ToInt32(data[0]);

                        try
                        {
                            DBHelper.DeleteTimetableEntry(entryId);
                            LoadTimetable();
                            ShowMessage("Entry deleted successfully!", System.Drawing.Color.Green);

                            // If we were editing this entry, clear the form
                            if (hfEntryId.Value == entryId.ToString())
                            {
                                ClearForm();
                            }
                        }
                        catch (Exception ex)
                        {
                            ShowMessage("Error deleting entry: " + ex.Message, System.Drawing.Color.Red);
                        }
                    }
                }
            }
        }

        protected void btnExportPDF_Click(object sender, EventArgs e)
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetUserTimetable(userId);

            if (dt.Rows.Count == 0)
            {
                ShowMessage("No data to export.", System.Drawing.Color.Red);
                return;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4.Rotate());
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Add title
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("My Timetable", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                document.Add(title);

                // Add generation date
                Font dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                Paragraph date = new Paragraph("Generated on: " + DateTime.Now.ToString("dd MMM yyyy HH:mm"), dateFont);
                date.Alignment = Element.ALIGN_RIGHT;
                date.SpacingAfter = 20;
                document.Add(date);

                // Create PDF table
                PdfPTable pdfTable = new PdfPTable(9);
                pdfTable.WidthPercentage = 100;
                pdfTable.SetWidths(new float[] { 1.5f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f });

                // Add headers
                string[] headers = { "Time", "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Actions" };
                foreach (string header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)));
                    cell.BackgroundColor = new BaseColor(1, 136, 159);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 5;
                    pdfTable.AddCell(cell);
                }

                // Group by time slot
                var timeSlots = dt.AsEnumerable()
                    .Select(r => new
                    {
                        Start = r["StartTime"].ToString(),
                        End = r["EndTime"].ToString(),
                        DisplayStart = FormatTimeForDisplay(r["StartTime"].ToString()),
                        DisplayEnd = FormatTimeForDisplay(r["EndTime"].ToString())
                    })
                    .Distinct()
                    .OrderBy(t => t.Start)
                    .ToList();

                string[] days = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

                foreach (var slot in timeSlots)
                {
                    // Time cell
                    PdfPCell timeCell = new PdfPCell(new Phrase(slot.DisplayStart + " - " + slot.DisplayEnd));
                    timeCell.Padding = 5;
                    pdfTable.AddCell(timeCell);

                    // Day cells
                    foreach (string day in days)
                    {
                        var entry = dt.AsEnumerable()
                            .FirstOrDefault(r => r["Day"].ToString() == day &&
                                                r["StartTime"].ToString() == slot.Start &&
                                                r["EndTime"].ToString() == slot.End);

                        string cellText = "-";
                        if (entry != null)
                        {
                            cellText = entry["Task"].ToString();
                        }

                        PdfPCell dayCell = new PdfPCell(new Phrase(cellText));
                        dayCell.Padding = 5;
                        if (entry != null && Convert.ToBoolean(entry["IsBreak"]))
                        {
                            dayCell.BackgroundColor = new BaseColor(255, 243, 205);
                        }
                        pdfTable.AddCell(dayCell);
                    }

                    // Actions cell
                    PdfPCell actionCell = new PdfPCell(new Phrase(""));
                    actionCell.Padding = 5;
                    pdfTable.AddCell(actionCell);
                }

                document.Add(pdfTable);
                document.Close();

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment; filename=Timetable_" + DateTime.Now.ToString("yyyyMMdd") + ".pdf");
                Response.BinaryWrite(ms.ToArray());
                Response.End();
            }
        }
    }
}