using System;
using Microsoft.Extensions.Logging;

namespace MigrationTools.EndpointEnrichers
{
    public class WorkItemCreatedEnricher : WorkItemEndpointEnricher
    {
        private WorkItemCreatedEnricherOptions _Options;

        public WorkItemCreatedEnricherOptions Options
        {
            get { return _Options; }
        }

        public WorkItemCreatedEnricher(IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemCreatedEnricher> logger) : base(services, telemetry, logger)
        {
        }

        public override void Configure(IEndpointEnricherOptions options)
        {
            _Options = (WorkItemCreatedEnricherOptions)options;
        }
    }
}