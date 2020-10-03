using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;

namespace MigrationTools
{
    public class TelemetryClientAdapter : ITelemetryLogger
    {
        private const string applicationInsightsKey = "4b9bb17b-c7ee-43e5-b220-ec6db2c33373";
        private TelemetryClient telemetryClient;
        private TelemetryConfiguration telemetryConfiguration;
        private bool enableTrace = false;

        public bool EnableTrace { get { return enableTrace; } set { enableTrace = value; } }

        public TelemetryConfiguration Configuration
        {
            get
            {
                return telemetryConfiguration;
            }
        }

        public TelemetryClientAdapter()
        {
            telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.InstrumentationKey = applicationInsightsKey;

            telemetryClient = new TelemetryClient(telemetryConfiguration);
            telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
            telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();

            telemetryClient.Context.Component.Version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();

            AddModules(telemetryConfiguration);
        }

        private void AddModules(TelemetryConfiguration telemetryConfiguration)
        {
            var perfCollectorModule = new PerformanceCollectorModule();
            perfCollectorModule.Counters.Add(new PerformanceCounterCollectionRequest(
              string.Format(@"\.NET CLR Memory({0})\# GC Handles", System.AppDomain.CurrentDomain.FriendlyName), "GC Handles"));
            perfCollectorModule.Initialize(telemetryConfiguration);
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

        public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
        {
            telemetryClient.TrackRequest(name, startTime, duration, responseCode, success);
        }

        public void TrackException(Exception ex, IDictionary<string, string> properties, IDictionary<string, double> measurements)
        {
            telemetryClient.TrackException(ex, properties, measurements);
        }

        public void CloseAndFlush()
        {
            telemetryClient.Flush();
        }
    }
}