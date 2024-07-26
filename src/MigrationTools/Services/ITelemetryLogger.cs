using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.DataContracts;

namespace MigrationTools
{
    public interface ITelemetryLogger
    {
        string SessionId { get; }

        void TrackDependency(DependencyTelemetry dependencyTelemetry);

        void TrackEvent(EventTelemetry eventTelemetry);

        void TrackEvent(string name);

        void TrackEvent(string name, IDictionary<string, string> properties, IDictionary<string, double> measurements);

        //void TrackRequest(RequestTelemetry requestTelemetry);
        void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success);

        void TrackException(Exception ex, IDictionary<string, string> properties = null, IDictionary<string, double> measurements = null);

        void CloseAndFlush();
    }
}