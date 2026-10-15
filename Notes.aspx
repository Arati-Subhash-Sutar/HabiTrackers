<%@ Page Title="Notes" Language="C#" MasterPageFile="~/MasterUser.master" AutoEventWireup="true" CodeBehind="Notes.aspx.cs" Inherits="HabiTrackers.Notes" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div class="page-container">
        <div class="card">
            <h2>My Notes</h2>

            <!-- Search Bar - Fixed button sizes -->
            <div style="margin-bottom:1.5rem; display:flex; gap:10px; align-items:center;">
                <asp:TextBox ID="txtSearch" runat="server" placeholder="Search notes..." CssClass="form-control" style="width:300px;" />
                <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" CssClass="btn-primary" style="padding:10px 20px;" />
                <asp:Button ID="btnClearSearch" runat="server" Text="Clear" OnClick="btnClearSearch_Click" CssClass="btn-secondary" style="padding:10px 20px;" />
            </div>

            <!-- Add/Edit Form -->
            <div style="margin-bottom:2rem; border:1px solid #ccc; padding:1rem; border-radius:5px;">
                <h3 id="formTitle" runat="server">Add New Note</h3>
                <asp:HiddenField ID="hfNoteId" runat="server" Value="0" />
                <div style="margin-bottom:0.5rem;">
                    <asp:TextBox ID="txtNoteTitle" runat="server" placeholder="Note title" CssClass="form-control" style="width:300px;" />
                </div>
                <div style="margin-bottom:0.5rem;">
                    <asp:TextBox ID="txtNoteContent" runat="server" TextMode="MultiLine" Height="80px" 
                        placeholder="Write your note..." CssClass="form-control" style="width:100%;" />
                </div>
                <div style="margin-bottom:0.5rem;">
                    <label>Attachment (optional):</label>
                    <asp:FileUpload ID="fuAttachment" runat="server" CssClass="form-control" />
                    <asp:Label ID="lblCurrentAttachment" runat="server" ForeColor="Gray" />
                </div>
                <div style="margin-top:1rem;">
                    <asp:Button ID="btnSaveNote" runat="server" Text="Save Note" OnClick="btnSaveNote_Click" CssClass="btn-primary" style="padding:10px 20px;" />
                    <asp:Button ID="btnUpdateNote" runat="server" Text="Update Note" OnClick="btnUpdateNote_Click" CssClass="btn-primary" Visible="false" style="padding:10px 20px;" />
                    <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" OnClick="btnCancelEdit_Click" CssClass="btn-secondary" Visible="false" CausesValidation="false" style="padding:10px 20px;" />
                </div>
            </div>
            
            <!-- Notes List -->
            <asp:Repeater ID="rptNotes" runat="server" OnItemCommand="rptNotes_ItemCommand">
                <ItemTemplate>
                    <div style="background:#f8f9fa; padding:1.5rem; margin:1rem 0; border-radius:10px; border-left:4px solid #01889f;">
                        <h4 style="margin-bottom:0.5rem; color:#01889f;"><%# Eval("NoteTitle") %></h4>
                        <p style="margin-bottom:0.5rem; color:#666;"><%# Eval("NoteContent") %></p>
                        
                        <!-- Attachment link -->
                        <%# !string.IsNullOrEmpty(Eval("AttachmentPath").ToString()) ? 
                            "<a href='" + Eval("AttachmentPath") + "' target='_blank' style='display:inline-block; margin-right:1rem;'>📎 Download Attachment</a>" : "" %>

                        <small style="color:#999;">Added: <%# ((DateTime)Eval("CreatedDate")).ToString("dd MMM yyyy HH:mm") %></small>
                        
                        <div style="float:right;">
                            <asp:LinkButton ID="lnkEdit" runat="server" CommandName="EditNote" CommandArgument='<%# Eval("NoteId") %>' Text="Edit" style="margin-right:10px;" />
                            <asp:LinkButton ID="lnkExport" runat="server" CommandName="ExportPDF" CommandArgument='<%# Eval("NoteId") %>' Text="Export PDF" style="margin-right:10px;" />
                            <asp:LinkButton ID="lnkDelete" runat="server" CommandName="DeleteNote" CommandArgument='<%# Eval("NoteId") %>' Text="Delete" OnClientClick="return confirm('Delete this note?');" style="color:#dc3545;" />
                        </div>
                        <div style="clear:both;"></div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>

            <asp:Label ID="lblMessage" runat="server" ForeColor="#01889f" />
        </div>
    </div>
</asp:Content>