namespace MigrationTools.Services.Shadows
{
    public class TelemetryLoggerFake : ITelemetryLogger
    {
        public string SessionId { get { return new Guid().ToString(); } }

        public void TrackException(Exception ex, IDictionary<string, string> properties = null)
        {
        }

        public void TrackException(Exception ex, IEnumerable<KeyValuePair<string, string>> properties = null)
        {
        }
    }
}
