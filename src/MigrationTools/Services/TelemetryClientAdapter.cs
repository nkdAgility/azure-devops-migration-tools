using System;
using System.Collections.Generic;
using System.Security.Principal;
using Elmah.Io.Client;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace MigrationTools
{
    public class TelemetryClientAdapter : ITelemetryLogger
    {
        private TelemetryClient _telemetryClient;
        private static IElmahioAPI elmahIoClient;

        public TelemetryClientAdapter(TelemetryClient telemetryClient)
        {
            telemetryClient.InstrumentationKey = "2d666f84-b3fb-4dcf-9aad-65de038d2772";
            telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
            telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            if (!(System.Reflection.Assembly.GetEntryAssembly() is null))
            {
                telemetryClient.Context.Component.Version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            }
            _telemetryClient = telemetryClient;

            elmahIoClient = ElmahioAPI.Create("7589821e832a4ae1a1170f8201def634", new ElmahIoOptions
            {
                Timeout = TimeSpan.FromSeconds(30),
                UserAgent = "Azure-DevOps-Migration-Tools",
            });
            elmahIoClient.Messages.OnMessage += (sender, args) => args.Message.Version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();

        }

        public string SessionId
        {
            get
            {
                return _telemetryClient.Context.Session.Id;
            }
        }

        public void CloseAndFlush()
        {
            _telemetryClient.Flush();
        }

        public void TrackDependency(DependencyTelemetry dependencyTelemetry)
        {
            _telemetryClient.TrackDependency(dependencyTelemetry);
        }

        public void TrackEvent(EventTelemetry eventTelemetry)
        {
            _telemetryClient.TrackEvent(eventTelemetry);
        }

        public void TrackEvent(string name)
        {
            _telemetryClient.TrackEvent(name);
        }

        public void TrackEvent(string name, IDictionary<string, string> properties, IDictionary<string, double> measurements)
        {
            _telemetryClient.TrackEvent(name, properties, measurements);
        }

        public void TrackException(Exception ex, IDictionary<string, string> properties, IDictionary<string, double> measurements)
        {
            _telemetryClient.TrackException(ex, properties, measurements);

            var baseException = ex.GetBaseException();
            var createMessage = new CreateMessage
            {
                DateTime = DateTime.UtcNow,
                Detail = ex.ToString(),
                Type = baseException.GetType().FullName,
                Title = baseException.Message ?? "An error occurred",
                Severity = "Error",
                Source = baseException.Source,
                User = Environment.UserName,
                Hostname = System.Environment.GetEnvironmentVariable("COMPUTERNAME"),
                Application = "Azure-DevOps-Migration-Tools",
                ServerVariables = new List<Item>
                    {
                        new Item("User-Agent", $"X-ELMAHIO-APPLICATION; OS={Environment.OSVersion.Platform}; OSVERSION={Environment.OSVersion.Version}; ENGINE=Azure-DevOps-Migration-Tools"),
                    }
            };
            foreach (var property in properties)
            {
                createMessage.Data.Add(new Item(property.Key, property.Value));
            }
            foreach (var measurement in measurements)
            {
                createMessage.Data.Add(new Item(measurement.Key, measurement.Value.ToString()));
            }

            elmahIoClient.Messages.CreateAndNotify(new Guid("24086b6d-4f58-47f4-8ac7-68d8bc05ca9e"), createMessage);

        }

        public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
        {
            _telemetryClient.TrackRequest(name, startTime, duration, responseCode, success);
        }
    }
}