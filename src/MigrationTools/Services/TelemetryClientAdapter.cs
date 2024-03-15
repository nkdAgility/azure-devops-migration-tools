using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace MigrationTools
{
    public class TelemetryClientAdapter : ITelemetryLogger
    {
        private TelemetryClient _telemetryClient;

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
        }

        public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
        {
            _telemetryClient.TrackRequest(name, startTime, duration, responseCode, success);
        }
    }
}