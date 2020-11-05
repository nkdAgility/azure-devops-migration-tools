using System;
using Microsoft.Extensions.Logging;

namespace MigrationTools.Enrichers
{
    public class WorkItemFieldTableEnricher : WorkItemEndpointEnricher
    {
        private WorkItemFieldTableEnricherOptions _Options;

        public WorkItemFieldTableEnricherOptions Options
        {
            get { return _Options; }
        }

        public WorkItemFieldTableEnricher(IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemFieldTableEnricher> logger) : base(services, telemetry, logger)
        {
        }

        public override void Configure(IEndpointEnricherOptions options)
        {
            _Options = (WorkItemFieldTableEnricherOptions)options;
        }
    }
}