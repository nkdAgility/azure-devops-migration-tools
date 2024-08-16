using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ApplicationInsights.DataContracts;

namespace MigrationTools.TestExtensions
{
    public class TelemetryLoggerFake : ITelemetryLogger
    {
        public string SessionId { get { return new Guid().ToString(); } }

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

        public void TrackException(Exception ex, IDictionary<string, string> properties = null, IDictionary<string, double> measurements = null)
        {
           
        }

        public void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success)
        {
            
        }
    }
}
