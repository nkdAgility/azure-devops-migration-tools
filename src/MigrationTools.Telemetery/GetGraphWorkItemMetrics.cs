using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.ImageSharp;

namespace MigrationTools.Telemetry
{
    public class GetGraphWorkItemMetrics
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly ILogger<GetGraphWorkItemMetrics> _logger;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(60); // Cache duration

        public GetGraphWorkItemMetrics(ILogger<GetGraphWorkItemMetrics> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [Function("GetGraphWorkItemMetrics_WorkItems")]
        public async Task<IActionResult> GetGraphWorkItemMetrics_WorkItems(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("Processing request for Application Insights data.");

            string appId = Environment.GetEnvironmentVariable("APP_INSIGHTS_APP_ID");
            string apiKey = Environment.GetEnvironmentVariable("APP_INSIGHTS_API_KEY");

            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(apiKey))
            {
                return new BadRequestObjectResult("Application Insights environment variables not set");
            }

            string version = req.Query["version"]; // Get the 'version' query parameter if provided
            bool hasVersion = !string.IsNullOrEmpty(version); // Check if a version was provided
            string cacheKey = hasVersion ? $"ai_workitems_{version}" : "ai_workitems_all";

            // Check cache first
            if (!_cache.TryGetValue(cacheKey, out AppInsightsResponse cachedData))
            {
                _logger.LogInformation("Cache miss. Fetching data from Application Insights.");

                // KQL query to get time-based work item metrics for the last 30 days
                string query = $@"
                    customMetrics
                    | where name == 'work_items_processed_total'
                    | where timestamp > ago(30d)  // Only include data from the last 30 days
                    {(hasVersion ? $"| where application_Version == '{version}'" : "")}  // Optional version filtering
                    | summarize TotalWorkItems = sum(value) by bin(timestamp, 1d)
                    | order by timestamp asc";

                var payload = new
                {
                    query = query
                };

                // Set up the HTTP request to Application Insights API
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync($"https://api.applicationinsights.io/v1/apps/{appId}/query", content);

                if (!response.IsSuccessStatusCode)
                {
                    return new BadRequestObjectResult($"Error fetching data from Application Insights: {response.StatusCode}");
                }

                string result = await response.Content.ReadAsStringAsync();
                cachedData = JsonConvert.DeserializeObject<AppInsightsResponse>(result);

                // Cache the data for 60 minutes
                _cache.Set(cacheKey, cachedData, CacheDuration);
            }
            else
            {
                _logger.LogInformation("Cache hit. Returning cached data.");
            }

            // Use "All Versions" if no specific version is provided
            string versionLabel = hasVersion ? version : "All Versions";

            // Parse response and generate graph using OxyPlot
            MemoryStream imageStream = GenerateGraph(cachedData, versionLabel);

            // Set cache headers for 4 hours (3600 seconds * 4 = 14400 seconds)
            var responseHeaders = new FileStreamResult(imageStream, "image/png")
            {
                FileDownloadName = "workitems_graph.png"
            };
            req.HttpContext.Response.Headers.Add("Cache-Control", "public, max-age=14400");
            req.HttpContext.Response.Headers.Add("Expires", DateTime.UtcNow.AddHours(1).ToString("R"));

            // Return the graph image as an HTTP response
            return responseHeaders;
        }

        // Method to generate the graph using OxyPlot
        private MemoryStream GenerateGraph(AppInsightsResponse data, string versionLabel)
        {
            var plotModel = new PlotModel {
                Title = $"Work Items Processed ({versionLabel})",
                TitleColor = OxyColor.Parse("#2575fc"), // Updated title color as requested
                DefaultColors = new List<OxyColor> { OxyColor.Parse("#2575fc") } // Updated default color as requested

            };

            // Create X and Y axes
            plotModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "MM-dd",
                Title = "Date",
                TitleColor = OxyColor.Parse("#2575fc"), // Updated title color as requested
                AxislineColor = OxyColor.Parse("#2575fc"), // Updated axis line color as requested
                TextColor = OxyColor.Parse("#2575fc"), // Updated text color as requested
                TicklineColor = OxyColor.Parse("#2575fc") // Updated tick line color as requested
            });
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Work Items Processed",
                TitleColor = OxyColor.Parse("#2575fc"), // Updated title color as requested
                AxislineColor = OxyColor.Parse("#2575fc"), // Updated axis line color as requested
                TextColor = OxyColor.Parse("#2575fc"), // Updated text color as requested
                TicklineColor = OxyColor.Parse("#2575fc") // Updated tick line color as requested

            });

            // Create a line series to plot the work item data
            var series = new LineSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.Purple                 
            };

            // Generate list of the last 30 days and pad missing days with zero
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-30);
            var dateRange = Enumerable.Range(0, 31)
                                      .Select(i => startDate.AddDays(i).Date)
                                      .ToList();

            var dataPoints = new Dictionary<DateTime, double>();



            if (data.Tables.Count > 0 && data.Tables[0].Rows.Count > 0)
            {
                // Populate data points from Application Insights data
                foreach (var row in data.Tables[0].Rows)
                {
                    DateTime date = DateTime.Parse(row[0].ToString());
                    double totalWorkItems = Convert.ToDouble(row[1]);

                    dataPoints[date.Date] = totalWorkItems;
                }
            }

            // Add zero for missing days and plot data points
            foreach (var date in dateRange)
            {
                if (!dataPoints.ContainsKey(date))
                {
                    dataPoints[date] = 0;
                }

                // Add data point for each date
                series.Points.Add(new DataPoint(DateTimeAxis.ToDouble(date), dataPoints[date]));
            }

            plotModel.Series.Add(series);

            // Export the plot as a PNG image using OxyPlot's PngExporter
            var imageStream = new MemoryStream();
            var pngExporter = new PngExporter(800, 600); // Updated exporter as requested
            pngExporter.Export(plotModel, imageStream);
            imageStream.Position = 0; // Reset stream position for response

            return imageStream;
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
