using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

public class GetSvcWorkItemMetrics
{
    // Make HttpClient static to ensure it's only instantiated once
    private static readonly HttpClient _client = new HttpClient();
    private readonly ILogger<GetSvcWorkItemMetrics> _logger;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10); // Cache duration

    static GetSvcWorkItemMetrics()
    {
        // Set HttpClient defaults
        _client.Timeout = TimeSpan.FromSeconds(30);  // Example of setting a timeout
    }

    public GetSvcWorkItemMetrics(ILogger<GetSvcWorkItemMetrics> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    // Method to fetch and animate total work items processed
    [Function("GetSvcWorkItemMetrics_WorkItemTotals")]
    public async Task<IActionResult> GetSvcWorkItemMetrics_WorkItemTotals(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        string version = req.Query["version"];

        // Generate a unique cache key based on version and other query parameters
        string cacheKey = $"svg_workitem_totals_{version}";

        // Check if SVG is cached
        if (_cache.TryGetValue(cacheKey, out string cachedSvg))
        {
            _logger.LogInformation("Returning cached SVG for key: " + cacheKey);
            return new ContentResult
            {
                Content = cachedSvg,
                ContentType = "image/svg+xml"
            };
        }

        // Query to get the total number of work items
        string totalQuery = $@"
            customMetrics
            | where name == 'work_items_processed_total'
            {(string.IsNullOrEmpty(version) ? "" : $"| where application_Version startswith '{version}'")}
            | summarize Total = sum(value)";

        // Query to get the average work items per hour for projection
        string averagePerHourQuery = $@"
            customMetrics
            | where name == 'work_items_processed_total'
            | where timestamp >= ago(24h)
            {(string.IsNullOrEmpty(version) ? "" : $"| where application_Version startswith '{version}'")}
            | summarize WorkItemsPerHour = count() by bin(timestamp, 1h)
            | summarize AvgWorkItemsPerHour = avg(WorkItemsPerHour)";

        // Fetch data and generate the SVG
        var svgResult = await FetchAndReturnAnimatedSVG(req, totalQuery, averagePerHourQuery, "Total Work Items", "sum");

        // Cache the generated SVG for future requests
        _cache.Set(cacheKey, ((ContentResult)svgResult).Content, CacheDuration);

        return svgResult;
    }

    // Generic method to fetch data from Application Insights and return an animated SVG
    private async Task<IActionResult> FetchAndReturnAnimatedSVG(HttpRequest req, string totalQuery, string averagePerHourQuery, string metricLabel, string aggregationType)
    {
        _logger.LogInformation($"Processing request for Application Insights data for metric {metricLabel}.");

        string appId = Environment.GetEnvironmentVariable("APP_INSIGHTS_APP_ID");
        string apiKey = Environment.GetEnvironmentVariable("APP_INSIGHTS_API_KEY");

        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(apiKey))
        {
            return new BadRequestObjectResult("Application Insights environment variables not set");
        }

        // Fetch total work items
        double totalWorkItems = await FetchTotalMetricValue(totalQuery, appId, apiKey);
        if (totalWorkItems < 0)
        {
            return new BadRequestObjectResult("Failed to fetch total work items.");
        }

        // Fetch average work items per hour
        double avgWorkItemsPerHour = await FetchAverageMetricValue(averagePerHourQuery, appId, apiKey);
        if (avgWorkItemsPerHour < 0)
        {
            return new BadRequestObjectResult("Failed to fetch average work items per hour.");
        }

        // Project the next hour's value by adding average work items to the total
        double projectedValue = totalWorkItems + avgWorkItemsPerHour;

        // Generate animated SVG with smooth transition over 1 hour
        string svgResponse = GenerateAnimatedSVG(totalWorkItems, projectedValue, 3600);

        return new ContentResult
        {
            Content = svgResponse,
            ContentType = "image/svg+xml"
        };
    }

    // Method to generate animated SVG content
    private string GenerateAnimatedSVG(double startValue, double endValue, int durationInSeconds)
    {
        return $@"
        <svg width=""400"" height=""100"" xmlns=""http://www.w3.org/2000/svg"">
          <text x=""10"" y=""50"" font-size=""40"" font-family=""Arial"" fill=""black"">
            <tspan id=""count"">{startValue}</tspan>
            <animate attributeName=""text"" from=""{startValue}"" to=""{endValue}"" dur=""{durationInSeconds}s"" keySplines=""0.4 0 0.6 1"" repeatCount=""indefinite"" fill=""freeze"" />
          </text>
        </svg>";
    }

    // Method to fetch total metric value from Application Insights
    private async Task<double> FetchTotalMetricValue(string query, string appId, string apiKey)
    {
        var payload = new
        {
            query = query
        };

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("x-api-key", apiKey);

        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync($"https://api.applicationinsights.io/v1/apps/{appId}/query", content);

        if (response.IsSuccessStatusCode)
        {
            string result = await response.Content.ReadAsStringAsync();
            var appInsightsData = JsonConvert.DeserializeObject<AppInsightsResponse>(result);

            if (appInsightsData.Tables.Count == 0 || appInsightsData.Tables[0].Rows.Count == 0)
            {
                _logger.LogWarning("No rows returned from Application Insights query.");
                return -1;
            }

            if (appInsightsData.Tables[0].Rows[0].Count > 0)
            {
                if (double.TryParse(appInsightsData.Tables[0].Rows[0][0]?.ToString(), out double totalValue))
                {
                    return totalValue;
                }
                else
                {
                    _logger.LogError("Failed to convert total metric value to double.");
                    return -1;
                }
            }
            else
            {
                _logger.LogError("The query did not return the expected column structure.");
                return -1;
            }
        }

        _logger.LogError($"Failed to fetch data from Application Insights: {response.StatusCode}");
        return -1;
    }

    // Method to fetch average metric value from Application Insights
    private async Task<double> FetchAverageMetricValue(string query, string appId, string apiKey)
    {
        var payload = new
        {
            query = query
        };

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("x-api-key", apiKey);

        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync($"https://api.applicationinsights.io/v1/apps/{appId}/query", content);

        if (response.IsSuccessStatusCode)
        {
            string result = await response.Content.ReadAsStringAsync();
            var appInsightsData = JsonConvert.DeserializeObject<AppInsightsResponse>(result);

            if (appInsightsData.Tables.Count == 0 || appInsightsData.Tables[0].Rows.Count == 0)
            {
                _logger.LogWarning("No rows returned from Application Insights query for average metric.");
                return -1;
            }

            double totalValue = 0;
            int rowCount = 0;
            foreach (var row in appInsightsData.Tables[0].Rows)
            {
                if (double.TryParse(row[0]?.ToString(), out double hourlyValue))
                {
                    totalValue += hourlyValue;
                    rowCount++;
                }
                else
                {
                    _logger.LogError("Failed to convert hourly metric value to double.");
                }
            }

            if (rowCount > 0)
            {
                return totalValue / rowCount;  // Calculate the average over the rows
            }

            return -1; // No valid rows found
        }

        _logger.LogError($"Failed to fetch data from Application Insights: {response.StatusCode}");
        return -1;
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
