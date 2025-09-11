using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MI.CRM.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PowerBIController : ControllerBase
    {
        private const string TenantId = "37ae3169-81cc-4188-9546-467067c51719";
        private const string ClientId = "d705371a-2963-4803-92f5-afc9eee1bbd1";
        private const string ClientSecret = "KkY8Q~-e30pqbUzFYdAEs8lqBHu7j9rzOqcl5aMv";
        private Guid WorkspaceId = new Guid("78551788-f2dc-4ff0-92f9-8b8c6a847cb3"); // Or actual GUID of workspace
        private Guid ReportId = new Guid("53659efa-e7f0-4086-b079-ff1360207ca3");

        private async Task<PowerBIClient> GetPowerBiClient()
        {
            var app = ConfidentialClientApplicationBuilder.Create(ClientId)
                .WithClientSecret(ClientSecret)
                .WithAuthority($"https://login.microsoftonline.com/{TenantId}")
                .Build();

            var scopes = new string[] { "https://analysis.windows.net/powerbi/api/.default" };
            var token = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return new PowerBIClient(
                new Uri("https://api.powerbi.com/"),
                new TokenCredentials(token.AccessToken, "Bearer")
            );
        }

        [HttpGet("embed-config")]
        public async Task<IActionResult> GetEmbedConfig()
        {
            try
            {
                // 1. Authenticate
                var appConfidential = ConfidentialClientApplicationBuilder.Create(ClientId)
                    .WithClientSecret(ClientSecret)
                    .WithAuthority($"https://login.microsoftonline.com/{TenantId}")
                    .Build();

                string[] scopes = { "https://analysis.windows.net/powerbi/api/.default" };
                var authResult = await appConfidential.AcquireTokenForClient(scopes).ExecuteAsync();
                string accessToken = authResult.AccessToken;

                // 2. Power BI client
                using var pbiClient = new PowerBIClient(new Uri("https://api.powerbi.com/"), new TokenCredentials(accessToken, "Bearer"));

                // 3. Get report
                var report = await pbiClient.Reports.GetReportInGroupAsync(WorkspaceId, ReportId);

                if (report == null) return NotFound("Report not found.");

                // 4. Build GenerateTokenRequestV2 with dataset + report + workspace
                var tokenRequest = new GenerateTokenRequestV2(
                    reports: new List<GenerateTokenRequestV2Report>
                    {
                new GenerateTokenRequestV2Report(report.Id)
                    },
                    datasets: new List<GenerateTokenRequestV2Dataset>
                    {
                new GenerateTokenRequestV2Dataset(report.DatasetId)
                    },
                    targetWorkspaces: new List<GenerateTokenRequestV2TargetWorkspace>
                    {
                new GenerateTokenRequestV2TargetWorkspace(WorkspaceId)
                    }
                );

                // 5. Generate embed token
                var embedToken = await pbiClient.EmbedToken.GenerateTokenAsync(tokenRequest);

                // 6. Return config
                return Ok(new
                {
                    reportId = report.Id,
                    datasetId = report.DatasetId,
                    embedUrl = report.EmbedUrl,
                    accessToken = embedToken.Token,
                    expiration = embedToken.Expiration
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }



        [HttpGet("ExportDetails")]
        public async Task<IActionResult> ExportDetails()
        {
            // 1. Authenticate
            var appConfidential = ConfidentialClientApplicationBuilder.Create(ClientId)
                .WithClientSecret(ClientSecret)
                .WithAuthority($"https://login.microsoftonline.com/{TenantId}")
                .Build();

            string[] scopes = { "https://analysis.windows.net/powerbi/api/.default" };
            var authResult = await appConfidential.AcquireTokenForClient(scopes).ExecuteAsync();

            // 2. Create Power BI client
            using var pbiClient = new PowerBIClient(
                new Uri("https://api.powerbi.com/"),
                new TokenCredentials(authResult.AccessToken, "Bearer")
            );

            // 3. Fetch all workspaces
            var groups = await pbiClient.Groups.GetGroupsAsync();
            var result = new List<object>();

            foreach (var g in groups.Value)
            {
                var reports = await pbiClient.Reports.GetReportsInGroupAsync(g.Id);

                result.Add(new
                {
                    WorkspaceName = g.Name,
                    WorkspaceId = g.Id,
                    Reports = reports.Value.Select(r => new
                    {
                        ReportName = r.Name,
                        ReportId = r.Id,
                        EmbedUrl = r.EmbedUrl,
                        DatasetId = r.DatasetId
                    })
                });
            }

            // 4. Save to JSON file
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync("powerbi-data.json", json);

            // 5. Return response
            return Ok(new { message = "Power BI details exported", file = "powerbi-data.json" });
        }

        [HttpGet("list-reports")]
        public async Task<IActionResult> ListReports()
        {
            try
            {
                var authorityUrl = $"https://login.microsoftonline.com/{TenantId}";
                var scopes = new[] { "https://analysis.windows.net/powerbi/api/.default" };

                // 1. Authenticate with Azure AD
                var app = ConfidentialClientApplicationBuilder.Create(ClientId)
                    .WithClientSecret(ClientSecret)
                    .WithAuthority(new Uri(authorityUrl))
                    .Build();

                var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
                var tokenCredentials = new TokenCredentials(result.AccessToken, "Bearer");

                // 2. Create Power BI client
                using var pbiClient = new PowerBIClient(new Uri("https://api.powerbi.com/"), tokenCredentials);

                // 3. List workspaces, reports, datasets
                var groups = await pbiClient.Groups.GetGroupsAsync();
                var response = new List<object>();

                foreach (var g in groups.Value)
                {
                    var workspaceReports = new List<object>();

                    var reports = await pbiClient.Reports.GetReportsInGroupAsync(g.Id);
                    foreach (var r in reports.Value)
                    {
                        workspaceReports.Add(new
                        {
                            ReportName = r.Name,
                            ReportId = r.Id,
                            DatasetId = r.DatasetId,
                            EmbedUrl = r.EmbedUrl
                        });
                    }

                    var datasets = await pbiClient.Datasets.GetDatasetsInGroupAsync(g.Id);

                    response.Add(new
                    {
                        WorkspaceName = g.Name,
                        WorkspaceId = g.Id,
                        Reports = workspaceReports,
                        Datasets = datasets.Value.Select(d => new { d.Name, d.Id })
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching Power BI details", error = ex.Message });
            }
        }
    }
}
       
