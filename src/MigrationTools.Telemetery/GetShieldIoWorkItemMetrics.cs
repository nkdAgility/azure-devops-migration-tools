using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MigrationTools.Telemetery
{
    public class GetShieldIoWorkItemMetrics
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly ILogger<GetShieldIoWorkItemMetrics> _logger;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10); // Cache duration

        public GetShieldIoWorkItemMetrics(ILogger<GetShieldIoWorkItemMetrics> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [Function("GetShieldIoWorkItemMetrics_WorkItemTotals")]
        public async Task<IActionResult> GetShieldIoWorkItemMetrics_WorkItemTotals(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("Processing request for Application Insights data.");

            string appId = Environment.GetEnvironmentVariable("APP_INSIGHTS_APP_ID");
            string apiKey = Environment.GetEnvironmentVariable("APP_INSIGHTS_API_KEY");

            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(apiKey))
            {
                return new BadRequestObjectResult("Application Insights environment variables not set");
            }

            string versionPrefix = req.Query["version"]; // Get the 'version' query parameter if provided

            // Check if cached result exists
            if (!_cache.TryGetValue("ai_work_items", out AppInsightsResponse cachedData))
            {
                _logger.LogInformation("Cache miss. Fetching data from Application Insights.");

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
                    cachedData = JsonConvert.DeserializeObject<AppInsightsResponse>(result);

                    // Cache the result with a defined expiration
                    _cache.Set("ai_work_items", cachedData, CacheDuration);
                }
                else
                {
                    return new BadRequestObjectResult($"Error fetching data from Application Insights: {response.StatusCode}");
                }
            }
            else
            {
                _logger.LogInformation("Cache hit. Returning cached data.");
            }

            // Check if the user requested a specific version prefix or if we should return the sum
            double totalWorkItems = 0;
            string formattedVersionLabel = "All Versions"; // Default label if no version is provided
            if (!string.IsNullOrEmpty(versionPrefix))
            {
                // Sum totals for all versions that start with the provided version prefix
                totalWorkItems = GetWorkItemTotalForVersionPrefix(cachedData, versionPrefix);
                if (totalWorkItems == -1)
                {
                    // No matching versions found, return "No Data" message
                    return new JsonResult(new
                    {
                        schemaVersion = 1,
                        label = $"Total Work Items ({FormatVersionLabel(versionPrefix)})",
                        message = "No Data",
                        color = "orange"
                    });
                }

                // Format the version label as X.Y.Z (3 digits)
                formattedVersionLabel = FormatVersionLabel(versionPrefix);
            }
            else
            {
                // Sum all work items for all versions
                totalWorkItems = GetTotalWorkItems(cachedData);
                if (totalWorkItems == 0)
                {
                    // No data found at all, return "No Data"
                    return new JsonResult(new
                    {
                        schemaVersion = 1,
                        label = "Total Work Items (All Versions)",
                        message = "No Data",
                        color = "orange"
                    });
                }
            }

            // Create the response for Shields.io with total work items
            var shieldsIoResponse = new
            {
                schemaVersion = 1,
                label = $"Total Work Items ({formattedVersionLabel})",
                message = totalWorkItems.ToString(),
                color = "orange"
            };

            return new JsonResult(shieldsIoResponse);
        }

        // Method to calculate the total work items for all versions
        private double GetTotalWorkItems(AppInsightsResponse appInsightsData)
        {
            double total = 0;
            foreach (var row in appInsightsData.Tables[0].Rows)
            {
                total += Convert.ToDouble(row[1]); // Sum the "Total" column
            }
            return total;
        }

        // Method to get the work item total for all versions that start with the provided version prefix
        private double GetWorkItemTotalForVersionPrefix(AppInsightsResponse appInsightsData, string versionPrefix)
        {
            double total = 0;
            bool foundAnyMatchingVersion = false;

            foreach (var row in appInsightsData.Tables[0].Rows)
            {
                string version = row[0].ToString(); // "application_Version" column
                if (version.StartsWith(versionPrefix))
                {
                    total += Convert.ToDouble(row[1]); // Sum the "Total" column for matching versions
                    foundAnyMatchingVersion = true;
                }
            }

            return foundAnyMatchingVersion ? total : -1; // Return -1 if no matching versions were found
        }

        // Helper method to format the version label to always have three parts (e.g., X.Y.Z)
        private string FormatVersionLabel(string version)
        {
            var versionParts = version.Split('.');
            if (versionParts.Length == 1)
            {
                return $"{versionParts[0]}.0.0"; // Pad with .0.0
            }
            else if (versionParts.Length == 2)
            {
                return $"{versionParts[0]}.{versionParts[1]}.0"; // Pad with .0
            }
            return version; // Already in X.Y.Z format
        }
    }

    // Classes to represent the structure of the Application Insights response
    public class AppInsightsResponse
    {
        public List<Table> Tables { get; set; }
    }

    public class Table
    {
        public string Name { get; set; }
        public List<Column> Columns { get; set; }
        public List<List<object>> Rows { get; set; }
    }

    public class Column
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
