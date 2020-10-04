using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.DataContracts;

namespace MigrationTools.Services
{
    public class TelemetryLoggerMock : ITelemetryLogger
    {
        public string SessionId => throw new NotImplementedException();

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