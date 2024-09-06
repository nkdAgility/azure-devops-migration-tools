using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MigrationTools.Telemetery
{

    public class GetShieldIoWorkItemMetrics
    {

        private static readonly HttpClient client = new HttpClient();

        private readonly ILogger<GetShieldIoWorkItemMetrics> _logger;

        public GetShieldIoWorkItemMetrics(ILogger<GetShieldIoWorkItemMetrics> logger)
        {
            _logger = logger;
        }

        [Function("GetShieldIoWorkItemMetrics_WorkItemTotals")]
        public async Task<IActionResult> GetShieldIoWorkItemMetrics_WorkItemTotals([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("Processing request for Application Insights data.");

            string appId = Environment.GetEnvironmentVariable("APP_INSIGHTS_APP_ID");
            string apiKey = Environment.GetEnvironmentVariable("APP_INSIGHTS_API_KEY");
            string query = @"
            customMetrics
            | where name == 'work_items_processed_total'
            | summarize Total = sum(value) by application_Version";

            var payload = new
            {
                query = query
            };

            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync($"https://api.applicationinsights.io/v1/apps/{appId}/query", content);

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                return new OkObjectResult(result);
            }
            else
            {
                return new BadRequestObjectResult("Error fetching data from Application Insights");
            }
        }
    }
}
