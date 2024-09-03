using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using MigrationTools.Services;
using Serilog;

namespace MigrationTools.Host.Services
{
    public class DetectOnlineService : IDetectOnlineService
    {
        private readonly ITelemetryLogger _Telemetry;
        private ILogger<DetectOnlineService> _logger;

        public DetectOnlineService(ITelemetryLogger telemetry, ILogger<DetectOnlineService> logger)
        {
            _Telemetry = telemetry;
            _logger = logger;
        }

        public bool IsOnline()
        {
            _logger.LogDebug("DetectOnlineService::IsOnline");
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("DetectOnlineService:IsOnline", ActivityKind.Client))
            {
                activity?.SetTag("url.full", "8.8.4.4");
                activity?.SetTag("server.address", "8.8.4.4");
                activity?.SetTag("http.request.method", "GET");
                
                activity?.SetTag("migrationtools.client", "TfsObjectModel");
                activity?.SetEndTime(activity.StartTimeUtc.AddSeconds(1));

                DateTime startTime = DateTime.Now;
                Stopwatch mainTimer = Stopwatch.StartNew();
                //////////////////////////////////
                bool isOnline = false;
                string responce = "none";
                try
                {
                    Ping myPing = new Ping();
                    String host = "8.8.4.4";
                    byte[] buffer = new byte[32];
                    int timeout = 1000;
                    PingOptions pingOptions = new PingOptions();
                    PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                    responce = reply.Status.ToString();
                    if (reply.Status == IPStatus.Success)
                    {
                        isOnline = true;
                    }
                    mainTimer.Stop();
                    activity.SetStatus(ActivityStatusCode.Ok);
                    activity?.SetTag("http.response.status_code", responce);
                }
                catch (Exception ex)
                {
                    mainTimer.Stop();
                    // Likley no network is even available
                    Log.Error(ex, "Error checking if we are online.");
                    responce = "error";
                    isOnline = false;
                    activity.SetStatus(ActivityStatusCode.Error);
                    activity?.SetTag("http.response.status_code", "500");
                }
                /////////////////
                mainTimer.Stop();
                return isOnline;
            }
        }
    }
}