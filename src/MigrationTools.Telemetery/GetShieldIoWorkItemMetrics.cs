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
        // Make HttpClient static to ensure it's only instantiated once
        private static readonly HttpClient _client = new HttpClient();
        private readonly ILogger<GetShieldIoWorkItemMetrics> _logger;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10); // Cache duration

        static GetShieldIoWorkItemMetrics()
        {
            // You can set other HttpClient defaults here if needed
            _client.Timeout = TimeSpan.FromSeconds(30);  // Example of setting a timeout
        }

        public GetShieldIoWorkItemMetrics(ILogger<GetShieldIoWorkItemMetrics> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        // Method to fetch total work items processed (counter, uses sum)
        [Function("GetShieldIoWorkItemMetrics_WorkItemTotals")]
        public async Task<IActionResult> GetShieldIoWorkItemMetrics_WorkItemTotals(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            string query = @"
                customMetrics
                | where name == 'work_items_processed_total'
                | summarize Total = sum(value) by application_Version";

            return await FetchAndReturnMetric(req, query, "Total Work Items", "sum");
        }

        // Method to fetch average work item processing duration (histogram, uses avg)
        [Function("GetShieldIoWorkItemMetrics_WorkItemProcessingDuration")]
        public async Task<IActionResult> GetShieldIoWorkItemMetrics_WorkItemProcessingDuration(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            string query = @"
        customMetrics
        | where name == 'work_item_processing_duration'
        | summarize Average = avg(value) by application_Version";

            // Fetch the raw result (in milliseconds)
            var result = await FetchAndReturnMetric(req, query, "Work Item Processing Duration (milliseconds)", "avg");

            // Check if the result is a BadRequest or NoData response, return it directly
            if (result is BadRequestObjectResult)
            {
                return result;
            }

            if (result is JsonResult json)
            {
                var responseData = json.Value as dynamic;

                // Check if the message field indicates "No Data"
                if (responseData?.message == "No Data")
                {
                    return result;
                }

                // Convert the result from milliseconds to seconds
                if (double.TryParse(responseData.message.ToString(), out double valueInMilliseconds))
                {
                    // Convert to seconds
                    double valueInSeconds = valueInMilliseconds / 1000;

                    // Create a new object with the updated message value (in seconds)
                    var updatedResponse = new
                    {
                        schemaVersion = responseData.schemaVersion,
                        label = responseData.label,
                        message = valueInSeconds.ToString("F2"), // formatted to 2 decimal places
                        color = responseData.color
                    };

                    // Return the updated JsonResult
                    return new JsonResult(updatedResponse);
                }
            }

            return result;
        }


        // Method to fetch average work item revisions (histogram, uses avg)
        [Function("GetShieldIoWorkItemMetrics_WorkItemRevisions")]
        public async Task<IActionResult> GetShieldIoWorkItemMetrics_WorkItemRevisions(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            string query = @"
                customMetrics
                | where name == 'work_item_revisions'
                | summarize Average = avg(value) by application_Version";

            return await FetchAndReturnMetric(req, query, "Work Item Revisions", "avg");
        }

        // Method to fetch total work item revisions (counter, uses sum)
        [Function("GetShieldIoWorkItemMetrics_WorkItemRevisionsTotal")]
        public async Task<IActionResult> GetShieldIoWorkItemMetrics_WorkItemRevisionsTotal(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            string query = @"
                customMetrics
                | where name == 'work_item_revisions_total'
                | summarize Total = sum(value) by application_Version";

            return await FetchAndReturnMetric(req, query, "Total Work Item Revisions", "sum");
        }

        // Generic method to fetch and return a metric from Application Insights
        private async Task<IActionResult> FetchAndReturnMetric(HttpRequest req, string query, string metricLabel, string aggregationType)
        {
            _logger.LogInformation($"Processing request for Application Insights data for metric {metricLabel}.");

            string appId = Environment.GetEnvironmentVariable("APP_INSIGHTS_APP_ID");
            string apiKey = Environment.GetEnvironmentVariable("APP_INSIGHTS_API_KEY");

            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(apiKey))
            {
                return new BadRequestObjectResult("Application Insights environment variables not set");
            }

            string versionPrefix = req.Query["version"]; // Get the 'version' query parameter if provided

            string cacheKey = $"ai_{metricLabel}_{aggregationType}";
            if (!_cache.TryGetValue(cacheKey, out AppInsightsResponse cachedData))
            {
                _logger.LogInformation("Cache miss. Fetching data from Application Insights.");

                var payload = new
                {
                    query = query
                };

                // Always clear headers and set them before making a request
                _client.DefaultRequestHeaders.Clear();
                _client.DefaultRequestHeaders.Add("x-api-key", apiKey);

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync($"https://api.applicationinsights.io/v1/apps/{appId}/query", content);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    cachedData = JsonConvert.DeserializeObject<AppInsightsResponse>(result);

                    // Cache the result with a defined expiration
                    _cache.Set(cacheKey, cachedData, CacheDuration);
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

            // Check if the user requested a specific version prefix or if we should return the aggregated value
            double aggregatedMetricValue = 0;
            string formattedVersionLabel = "All Versions"; // Default label if no version is provided
            if (!string.IsNullOrEmpty(versionPrefix))
            {
                // Sum/average totals for all versions that start with the provided version prefix
                aggregatedMetricValue = GetWorkItemTotalForVersionPrefix(cachedData, versionPrefix);
                if (aggregatedMetricValue == -1)
                {
                    // No matching versions found, return "No Data" message
                    return new JsonResult(new
                    {
                        schemaVersion = 1,
                        label = $"{metricLabel} ({FormatVersionLabel(versionPrefix)})",
                        message = "No Data",
                        color = "orange"
                    });
                }

                // Format the version label as X.Y.Z (3 digits)
                formattedVersionLabel = FormatVersionLabel(versionPrefix);
            }
            else
            {
                // Aggregate all items for all versions
                aggregatedMetricValue = GetTotalWorkItems(cachedData);
                if (aggregatedMetricValue == 0)
                {
                    // No data found at all, return "No Data"
                    return new JsonResult(new
                    {
                        schemaVersion = 1,
                        label = $"{metricLabel} (All Versions)",
                        message = "No Data",
                        color = "orange"
                    });
                }
            }

            // Create the response for Shields.io with the aggregated metric value
            var shieldsIoResponse = new
            {
                schemaVersion = 1,
                label = $"{metricLabel} ({formattedVersionLabel})",
                message = aggregatedMetricValue.ToString(),
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
                total += Convert.ToDouble(row[1]); // Sum the "Total" or "Average" column
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
                    total += Convert.ToDouble(row[1]); // Sum/average the "Total" or "Average" column for matching versions
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
