using System;
using System.Data;
using System.IO;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace HabiTrackers
{
    public partial class Notes : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserId"] == null) Response.Redirect("Default.aspx");
            if (!IsPostBack)
            {
                ViewState["SearchTerm"] = "";
                LoadNotes();
            }
        }

        private void LoadNotes()
        {
            int userId = (int)Session["UserId"];
            DataTable dt = DBHelper.GetUserNotes(userId);

            string search = ViewState["SearchTerm"]?.ToString();
            if (!string.IsNullOrEmpty(search))
            {
                dt.DefaultView.RowFilter = string.Format("NoteTitle LIKE '%{0}%' OR NoteContent LIKE '%{0}%'", search.Replace("'", "''"));
                rptNotes.DataSource = dt.DefaultView;
            }
            else
            {
                rptNotes.DataSource = dt;
            }
            rptNotes.DataBind();
        }

        private string SaveAttachment(FileUpload fu)
        {
            if (fu.HasFile && fu.FileContent.Length > 0)
            {
                string folder = Server.MapPath("~/Uploads/Notes/");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(fu.FileName);
                string filePath = Path.Combine(folder, fileName);
                fu.SaveAs(filePath);
                return "~/Uploads/Notes/" + fileName;
            }
            return null;
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            ViewState["SearchTerm"] = txtSearch.Text.Trim();
            LoadNotes();
        }

        protected void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ViewState["SearchTerm"] = "";
            LoadNotes();
        }

        protected void btnSaveNote_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNoteTitle.Text) || string.IsNullOrWhiteSpace(txtNoteContent.Text))
            {
                lblMessage.Text = "Title and content are required.";
                return;
            }

            int userId = (int)Session["UserId"];
            string attachmentPath = SaveAttachment(fuAttachment);

            DBHelper.SaveNote(userId, txtNoteTitle.Text, txtNoteContent.Text, attachmentPath);
            ClearForm();
            LoadNotes();
            lblMessage.Text = "Note saved successfully.";
        }

        protected void btnUpdateNote_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNoteTitle.Text) || string.IsNullOrWhiteSpace(txtNoteContent.Text))
            {
                lblMessage.Text = "Title and content are required.";
                return;
            }

            int noteId = Convert.ToInt32(hfNoteId.Value);
            string attachmentPath = SaveAttachment(fuAttachment);
            if (attachmentPath == null && !string.IsNullOrEmpty(lblCurrentAttachment.Text))
            {
                // keep existing attachment
                attachmentPath = lblCurrentAttachment.Text;
            }

            DBHelper.UpdateNote(noteId, txtNoteTitle.Text, txtNoteContent.Text, attachmentPath);
            ClearForm();
            LoadNotes();
            lblMessage.Text = "Note updated successfully.";
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            hfNoteId.Value = "0";
            txtNoteTitle.Text = "";
            txtNoteContent.Text = "";
            lblCurrentAttachment.Text = "";
            fuAttachment.Dispose();
            btnSaveNote.Visible = true;
            btnUpdateNote.Visible = false;
            btnCancelEdit.Visible = false;
            formTitle.InnerText = "Add New Note";
        }

        protected void rptNotes_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int noteId = Convert.ToInt32(e.CommandArgument);
            int userId = (int)Session["UserId"];

            if (e.CommandName == "DeleteNote")
            {
                // Get note title before deleting for activity
                DataTable noteDt = DBHelper.GetNoteById(noteId);
                string noteTitle = "";
                if (noteDt.Rows.Count > 0)
                    noteTitle = noteDt.Rows[0]["NoteTitle"].ToString();

                DBHelper.DeleteNote(noteId);
                DBHelper.AddRecentActivity(userId, "Note", noteTitle, "Deleted");
                LoadNotes();
                lblMessage.Text = "Note deleted.";
            }
            else if (e.CommandName == "EditNote")
            {
                DataTable dt = DBHelper.GetNoteById(noteId);
                if (dt.Rows.Count > 0)
                {
                    hfNoteId.Value = noteId.ToString();
                    txtNoteTitle.Text = dt.Rows[0]["NoteTitle"].ToString();
                    txtNoteContent.Text = dt.Rows[0]["NoteContent"].ToString();
                    lblCurrentAttachment.Text = dt.Rows[0]["AttachmentPath"]?.ToString() ?? "";
                    btnSaveNote.Visible = false;
                    btnUpdateNote.Visible = true;
                    btnCancelEdit.Visible = true;
                    formTitle.InnerText = "Edit Note";
                }
            }
            else if (e.CommandName == "ExportPDF")
            {
                ExportNoteToPDF(noteId);
            }
        }

        private void ExportNoteToPDF(int noteId)
        {
            DataTable dt = DBHelper.GetNoteById(noteId);
            if (dt.Rows.Count == 0) return;

            string title = dt.Rows[0]["NoteTitle"].ToString();
            string content = dt.Rows[0]["NoteContent"].ToString();
            DateTime created = Convert.ToDateTime(dt.Rows[0]["CreatedDate"]);

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4);
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // Add content
                document.Add(new Paragraph("Note: " + title, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18)));
                document.Add(new Paragraph("Date: " + created.ToString("dd MMM yyyy HH:mm")));
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(content, FontFactory.GetFont(FontFactory.HELVETICA, 12)));

                document.Close();

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment; filename=Note_" + noteId + ".pdf");
                Response.BinaryWrite(ms.ToArray());
                Response.End();
            }
        }
    }
}