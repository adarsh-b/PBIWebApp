﻿using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Collections.Specialized;
using PBIWebApp.Properties;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using System.Net;

namespace PBIWebApp
{
    public partial class EmbedReport : System.Web.UI.Page
    {
        string baseUri = Settings.Default.PowerBiDataset;

        protected void Page_Load(object sender, EventArgs e)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
               | SecurityProtocolType.Tls11
               | SecurityProtocolType.Tls12
               | SecurityProtocolType.Ssl3;

            if (!Page.IsPostBack)
            {
                //Need an Authorization Code from Azure AD before you can get an access token to be able to call Power BI operations
                //You get the Authorization Code when you click Get Report (see below).
                //After you call AcquireAuthorizationCode(), Azure AD redirects back to this page with an Authorization Code.
                if (Session[Utils.authResultString] != null)
                {
                    //Session[Utils.authResultString] does not equal null, which means you have an
                    //Access Token. With the Acccess Token, you can call the Power BI Get Reports operation. Get Reports returns information
                    //about a Report, not the actual Report visual. You get the Report visual later with some JavaScript. See postActionLoadReport()
                    //in Default.aspx.
                    accessToken.Value = Utils.authResult.AccessToken;

                    //After you get an AccessToken, you can call Power BI API operations such as Get Report
                    //In this sample, you get the first Report. In a production app, you would create a more robost
                    //solution

                    //Gets the corresponding report to the setting's ReportId and WorkspaceId.
                    //If ReportId or WorkspaceId are empty, it will get the first user's report.
                    GetReport();
                }
                else
                {
                    //You need an Authorization Code from Azure AD so that you can get an Access Token
                    //Values are hard-coded for sample purposes.
                    Utils.EmbedType = "EmbedReport";
                    var urlToRedirect = Utils.GetAuthorizationCode();

                    //Redirect to Azure AD to get an authorization code
                    Response.Redirect(urlToRedirect);
                }
            }
        }
        
        // Gets a report based on the setting's ReportId and WorkspaceId.
        // If reportId or WorkspaceId are empty, it will get the first user's report.
        protected void GetReport()
        {
            var powerBiApiUrl = Settings.Default.PowerBiApiUrl;

            using (var client = new PowerBIClient(new Uri(powerBiApiUrl), new TokenCredentials(accessToken.Value, "Bearer")))
            {
                Report report = null;
                Utils.ReportsList = client.Reports.GetReports().Value.ToList();

                //var dashboards = client.Dashboards.GetDashboards().Value.ToList();

                ddlReports.DataSource = Utils.ReportsList;
                ddlReports.DataValueField = "Id";
                ddlReports.DataTextField = "Name";
                ddlReports.DataBind();

                if(ddlReports.Items.Count > 0)
                {
                    ddlReports.SelectedIndex = 0;
                    report = Utils.ReportsList.Where(r => r.Id.ToString() == ddlReports.SelectedItem.Value).FirstOrDefault();

                    txtEmbedUrl.Text = report.EmbedUrl;
                    txtReportId.Text = report.Id.ToString();
                }

                AppendErrorIfReportNull(report, "No reports found. Please specify the target report ID and workspace in the applications settings.");
            }
        }

        // Gets the report with the specified ID from the workspace. If report ID is emty it will retrieve the first report from the workspace.
        private Report GetReportFromWorkspace(PowerBIClient client, Guid WorkspaceId, Guid reportId)
        {
            // Gets the workspace by WorkspaceId.
            var workspaces = client.Groups.GetGroups();
            var sourceWorkspace = workspaces.Value.FirstOrDefault(g => g.Id == WorkspaceId);

            // No workspace with the workspace ID was found.
            if (sourceWorkspace == null)
            {
                errorLabel.Text = $"Workspace with id: '{WorkspaceId}' not found. Please validate the provided workspace ID.";
                return null;
            }

            Report report = null;
            if (reportId == Guid.Empty)
            {
                // Get the first report in the workspace.
                report = client.Reports.GetReportsInGroup(sourceWorkspace.Id).Value.FirstOrDefault();
                AppendErrorIfReportNull(report, "Workspace doesn't contain any reports.");
            }

            else
            {
                try
                {
                    // retrieve a report by the workspace ID and report ID.
                    report = client.Reports.GetReportInGroup(WorkspaceId, reportId);
                }

                catch (HttpOperationException)
                {
                    errorLabel.Text = $"Report with ID: '{reportId}' not found in the workspace with ID: '{WorkspaceId}', Please check the report ID.";

                }
            }

            return report;
        }

        private void AppendErrorIfReportNull(Report report, string errorMessage)
        {
            if (report == null)
            {
                errorLabel.Text = errorMessage;
            }
        }

        private static Guid GetParamGuid(string param)
        {
            Guid paramGuid = Guid.Empty;
            Guid.TryParse(param, out paramGuid);
            return paramGuid;
        }

        protected void ddlReports_SelectedIndexChanged(object sender, EventArgs e)
        {
            var report = Utils.ReportsList.Where(r => r.Id.ToString() == ddlReports.SelectedItem.Value).FirstOrDefault();

            txtEmbedUrl.Text = report.EmbedUrl;
            txtReportId.Text = report.Id.ToString();
        }
    }
}