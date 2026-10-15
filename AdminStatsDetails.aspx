<%@ Page Title="Stat Details" Language="C#" MasterPageFile="~/MasterAdmin.master" AutoEventWireup="true" CodeBehind="AdminStatsDetails.aspx.cs" Inherits="HabiTrackers.AdminStatsDetails" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .back-btn {
            background: #6c757d;
            color: white;
            padding: 10px 20px;
            border-radius: 8px;
            text-decoration: none;
            display: inline-block;
            margin-top: 20px;
            border: none;
            cursor: pointer;
        }
        .back-btn:hover {
            background: #5a6268;
        }
        .detail-card {
            background: white;
            border-radius: 10px;
            padding: 20px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .page-container {
            max-width: 1200px;
            margin: 2rem auto;
            padding: 0 2rem;
        }
        .table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 1rem;
        }
        .table th, .table td {
            padding: 12px;
            text-align: left;
            border-bottom: 1px solid #e1e5e9;
        }
        .table th {
            background: #f8f9fa;
            color: #01889f;
            font-weight: 600;
        }
        .search-bar {
            margin-bottom: 1rem;
            display: flex;
            gap: 10px;
            align-items: center;
        }
        .search-bar input {
            flex: 1;
            padding: 8px 12px;
            border: 1px solid #ccc;
            border-radius: 4px;
        }
        .search-bar button {
            background: #01889f;
            color: white;
            border: none;
            padding: 8px 16px;
            border-radius: 4px;
            cursor: pointer;
        }
        .search-bar button:hover {
            background: #016c7e;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="page-container">
        <div class="detail-card">
            <h2 id="pageTitle" runat="server">Stat Details</h2>

            <!-- Search Bar -->
            <div class="search-bar">
                <asp:TextBox ID="txtSearch" runat="server" placeholder="Search by user ID, username, email, or details..." CssClass="form-control" style="flex:1;" />
                <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" CssClass="btn-primary" />
                <asp:Button ID="btnClear" runat="server" Text="Clear" OnClick="btnClear_Click" CssClass="btn-secondary" />
            </div>

            <asp:GridView ID="gvDetails" runat="server" AutoGenerateColumns="False" CssClass="table" GridLines="None" EmptyDataText="No records found.">
                <Columns>
                    <asp:BoundField DataField="UserId" HeaderText="User ID" />
                    <asp:BoundField DataField="Username" HeaderText="Username" />
                    <asp:BoundField DataField="Email" HeaderText="Email" />
                    <asp:BoundField DataField="Details" HeaderText="Details" />
                </Columns>
            </asp:GridView>
            <div style="margin-top: 1rem;">
                <asp:Button ID="btnBack" runat="server" Text="← Back to Dashboard" OnClick="btnBack_Click" CssClass="back-btn" />
            </div>
        </div>
    </div>
</asp:Content>