
using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Collections.Specialized;
using Newtonsoft.Json;
using PBIWebApp.Properties;
using Microsoft.PowerBI.Api;
using Microsoft.Rest;
using Microsoft.PowerBI.Api.Models;
using System.Collections.Generic;
using System.Net;

namespace PBIWebApp
{
    public partial class EmbedDashboard : System.Web.UI.Page
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
                //Test for AuthenticationResult
                if (Session[Utils.authResultString] != null)
                {
                    accessToken.Value = Utils.authResult.AccessToken;

                    IList<Dashboard> dashboards = null;
                    var powerBiApiUrl = Settings.Default.PowerBiApiUrl;

                    using (var client = new PowerBIClient(new Uri(powerBiApiUrl), new TokenCredentials(Utils.authResult.AccessToken, "Bearer")))
                    {
                        dashboards = client.Dashboards.GetDashboards().Value.ToList();
                    }

                    ddlReports.DataSource = dashboards;
                    ddlReports.DataValueField = "EmbedUrl";
                    ddlReports.DataTextField = "DisplayName";
                    ddlReports.DataBind();

                    if (ddlReports.Items.Count > 0)
                    {
                        ddlReports.SelectedIndex = 0;
                        txtEmbedUrl.Text = ddlReports.SelectedItem.Value;
                    }
                }
                else
                {
                    Utils.EmbedType = "EmbedDashboard";
                    var urlToRedirect = Utils.GetAuthorizationCode();
                    Response.Redirect(urlToRedirect);
                }
            }
        }

        protected void ddlReports_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtEmbedUrl.Text = ddlReports.SelectedItem.Value;
        }
    }
}