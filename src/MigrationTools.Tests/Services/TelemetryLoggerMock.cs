using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.DataContracts;

namespace MigrationTools.Services
{
    public class TelemetryLoggerMock : ITelemetryLogger
    {
        public bool EnableTrace { get; set; }

        public string SessionId => throw new NotImplementedException();

        public void TrackException(Exception ex, IDictionary<string, string> properties = null)
        {
            throw new NotImplementedException();
        }

        public void TrackException(Exception ex, IEnumerable<KeyValuePair<string, string>> properties = null)
        {
            throw new NotImplementedException();
        }
    }
}