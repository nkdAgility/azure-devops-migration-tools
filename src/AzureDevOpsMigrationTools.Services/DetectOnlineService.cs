using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace AzureDevOpsMigrationTools.Services
{
    public class DetectOnlineService : IDetectOnlineService
    {
        private readonly ILogger<DetectOnlineService> _Log;
        private readonly IConfiguration _Config;
        private readonly TelemetryClient _Telemetry;

        public DetectOnlineService(ILogger<DetectOnlineService> log, IConfiguration config, TelemetryClient telemetry)
        {
            _Log = log;
            _Config = config;
            _Telemetry = telemetry;
        }

        public bool IsOnline()
        {
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
            }
            catch (Exception ex)
            {
                // Likley no network is even available
                _Log.LogError(ex, "Error checking if we are online.");
                responce = "error";
                isOnline = false;
            }
            /////////////////
            mainTimer.Stop();
            _Telemetry.TrackDependency(new DependencyTelemetry("Ping", "GoogleDNS", "IsOnline", null, startTime, mainTimer.Elapsed, responce, true));
            return isOnline;
        }
    }
}