using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace MigrationTools
{
    public class TelemetryClientAdapter : ITelemetryLogger
    {
        private TelemetryClient telemetryClient;

        private TelemetryConfiguration telemetryConfiguration;

        public TelemetryClientAdapter()
        {
            telemetryConfiguration = TelemetryConfiguration.CreateFromConfiguration("ApplicationInsights.config");
            telemetryConfiguration.InstrumentationKey = "2d666f84-b3fb-4dcf-9aad-65de038d2772";
            telemetryConfiguration.ConnectionString = "InstrumentationKey=2d666f84-b3fb-4dcf-9aad-65de038d2772;IngestionEndpoint=https://northeurope-0.in.applicationinsights.azure.com/";
            telemetryClient = new TelemetryClient(telemetryConfiguration);
            telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
            telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            telemetryClient.Context.Component.Version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
        }

        public TelemetryConfiguration Configuration
        {
            get
            {
                return telemetryConfiguration;
            }
        }

        public string SessionId
        {
            get
            {
                return telemetryClient.Context.Session.Id;
            }
        }

        public void CloseAndFlush()
        {
            telemetryClient.Flush();
        }

        public void TrackDependency(DependencyTelemetry dependencyTelemetry)
        {
            telemetryClient.TrackDependency(dependencyTelemetry);
        }

        public void TrackEvent(EventTelemetry eventTelemetry)
        {
            telemetryClient.TrackEvent(eventTelemetry);
        }

        public void TrackEvent(string name)
        {
            telemetryClient.TrackEvent(name);
        }

        public void TrackEvent(string name, IDictionary<string, string> properties, IDictionary<string, double> measurements)
        {
            telemetryClient.TrackEvent(name, properties, measurements);
        }

        public void TrackException(Exception ex, IDictionary<string, string> properties, IDictionary<string, double> measurements)
        {
            telemetryClient.TrackException(ex, properties, measurements);
        }

        public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
        {
            telemetryClient.TrackRequest(name, startTime, duration, responseCode, success);
        }
    }
}