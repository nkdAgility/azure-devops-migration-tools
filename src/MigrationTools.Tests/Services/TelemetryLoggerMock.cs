using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace MigrationTools.Services
{
    public class TelemetryLoggerMock : ITelemetryLogger
    {
        public bool EnableTrace { get; set; }

        public TelemetryConfiguration Configuration { get; }

        public void CloseAndFlush()
        {
        }

        public void TrackDependency(DependencyTelemetry dependencyTelemetry)
        {
        }

        public void TrackEvent(EventTelemetry eventTelemetry)
        {
        }

        public void TrackEvent(string name)
        {
        }

        public void TrackEvent(string name, IDictionary<string, string> properties, IDictionary<string, double> measurements)
        {
        }

        public void TrackException(Exception ex, IDictionary<string, string> properties, IDictionary<string, double> measurements)
        {
        }

        public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
        {
        }
    }
}