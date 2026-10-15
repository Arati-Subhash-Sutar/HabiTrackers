<%@ Page Title="Diary" Language="C#" MasterPageFile="~/MasterUser.master" AutoEventWireup="true" CodeBehind="Diary.aspx.cs" Inherits="HabiTrackers.Diary" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .notebook-textarea {
            width: 100%;
            padding: 10px;
            font-family: 'Courier New', monospace;
            font-size: 16px;
            line-height: 1.8;
            background: repeating-linear-gradient(
                white 0px,
                white 29px,
                #e6e6e6 30px
            );
            border: 1px solid #ccc;
            resize: vertical;
            outline: none;
            box-sizing: border-box;
        }
        .diary-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 1rem;
        }
        .diary-date {
            font-size: 1.2rem;
            font-weight: bold;
            color: #333;
        }
        .search-box {
            background: #f9f9f9;
            padding: 1rem;
            border-radius: 8px;
            margin-bottom: 1rem;
        }
        .entry-list {
            max-height: 300px;
            overflow-y: auto;
            border: 1px solid #ddd;
            padding: 0.5rem;
            border-radius: 5px;
        }
        .entry-item {
            padding: 0.5rem;
            border-bottom: 1px solid #eee;
            cursor: pointer;
        }
        .entry-item:hover {
            background-color: #f0f0f0;
        }
        .entry-date {
            font-weight: bold;
            color: #01889f;
        }
        .btn-danger {
            background-color: #dc3545;
            color: white;
            border: none;
            padding: 10px 20px;
            border-radius: 5px;
            cursor: pointer;
        }
        .btn-danger:hover {
            background-color: #c82333;
        }
        .btn-secondary {
            background-color: #6c757d;
            color: white;
            border: none;
            padding: 10px 20px;
            border-radius: 5px;
            cursor: pointer;
        }
        .btn-secondary:hover {
            background-color: #5a6268;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div class="page-container">
        <div class="card">
            <h2>My Diary</h2>

            <!-- Search / Filter Section - Simplified to single date -->
            <div class="search-box">
                <div style="display: flex; gap: 10px; align-items: center; flex-wrap: wrap;">
                    <div>
                        <label>Search by Date:</label>
                        <asp:TextBox ID="txtSearchDate" runat="server" TextMode="Date" CssClass="form-control" style="width:200px;" />
                    </div>
                    <div>
                        <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" CssClass="btn-primary" style="padding:10px 20px;" />
                        <asp:Button ID="btnClearSearch" runat="server" Text="Clear" OnClick="btnClearSearch_Click" CssClass="btn-secondary" style="padding:10px 20px;" />
                    </div>
                </div>
                <div style="margin-top: 1rem;">
                    <asp:Label ID="lblSearchMessage" runat="server" ForeColor="Red" />
                </div>
            </div>

            <!-- Diary Entry Editor - Removed date selector, only showing current date -->
            <div class="diary-header">
                <div class="diary-date">
                    <asp:Label ID="lblDisplayDate" runat="server" Text="" />
                </div>
            </div>

            <!-- Notebook-style textarea -->
            <asp:TextBox ID="txtContent" runat="server" TextMode="MultiLine" CssClass="notebook-textarea" placeholder="Write your thoughts for the day..." style="height:250px;" />

            <div style="margin-top: 1rem; display: flex; gap: 1rem;">
                <asp:Button ID="btnSave" runat="server" Text="Save Today's Entry" OnClick="btnSave_Click" CssClass="btn-primary" style="padding:10px 20px;" />
            </div>

            <asp:Label ID="lblMessage" runat="server" ForeColor="#01889f" style="margin-top:0.5rem; display:block;" />

            <!-- List of recent entries -->
            <div style="margin-top: 2rem;">
                <h3>Previous Entries</h3>
                <div class="entry-list">
                    <asp:Repeater ID="rptEntries" runat="server" OnItemCommand="rptEntries_ItemCommand">
                        <ItemTemplate>
                            <div class="entry-item" style="display: flex; justify-content: space-between; align-items: center;">
                                <asp:LinkButton ID="lnkSelect" runat="server" CommandName="ViewEntry" CommandArgument='<%# Eval("EntryDate", "{0:yyyy-MM-dd}") %>' style="text-decoration:none; color:inherit; flex-grow:1;">
                                    <span class="entry-date"><%# Eval("EntryDate", "{0:dd MMM yyyy}") %></span>
                                    <span style="margin-left: 10px;"><%# Eval("Preview") %></span>
                                </asp:LinkButton>
                                <asp:LinkButton ID="lnkDelete" runat="server" CommandName="DeleteEntry" CommandArgument='<%# Eval("EntryId") + "|" + Eval("EntryDate", "{0:yyyy-MM-dd}") %>' 
                                    OnClientClick="return confirm('Delete this diary entry?');" style="color:#dc3545; margin-left:10px;" Text="Delete" />
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:Label ID="lblNoEntries" runat="server" Text="No diary entries found." Visible="false" style="color:#999; text-align:center; display:block;" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>