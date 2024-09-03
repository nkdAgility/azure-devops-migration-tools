using System;
using System.Collections.Generic;

namespace MigrationTools
{
    public interface ITelemetryLogger
    {
        string SessionId { get; }

        void TrackException(Exception ex, IDictionary<string, string> properties = null);
        void TrackException(Exception ex, IEnumerable<KeyValuePair<string, string>> properties = null);
    }
}