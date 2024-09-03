using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Services.Shadows
{
    public class TelemetryLoggerFake : ITelemetryLogger
    {
        public string SessionId { get { return new Guid().ToString(); } }

        public void TrackException(Exception ex, IDictionary<string, string> properties = null)
        {
            throw new NotImplementedException();
        }

        public void TrackException(Exception ex, IEnumerable<KeyValuePair<string, string>> properties = null)
        {
          
        }
    }
}
