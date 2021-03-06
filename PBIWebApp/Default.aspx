﻿<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="PBIWebApp._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <header>
        <h1>
            Power BI Report & Dashboard Viewer
        </h1>
    </header>

    <div>
        <asp:Button ID="embedReport" runat="server" OnClick="embedReportButton_Click" Text="View Report" />
    </div>
    <div>
        <asp:Button ID="embedDashboard" runat="server" OnClick="embedDashboardButton_Click" Text="View Dashboard" />
    </div>
    <div hidden ="hidden">
        <asp:Button ID="embedTile" runat="server" OnClick="embedTileButton_Click" Text="Embed Tiles" />
    </div>
</asp:Content>
