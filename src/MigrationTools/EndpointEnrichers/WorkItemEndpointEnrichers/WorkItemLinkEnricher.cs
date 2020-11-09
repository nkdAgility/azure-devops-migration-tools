using System;
using Microsoft.Extensions.Logging;

namespace MigrationTools.EndpointEnrichers
{
    public class WorkItemLinkEnricher : WorkItemEndpointEnricher
    {
        private WorkItemLinkEnricherOptions _Options;

        public WorkItemLinkEnricherOptions Options
        {
            get { return _Options; }
        }

        public WorkItemLinkEnricher(IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemLinkEnricher> logger) : base(services, telemetry, logger)
        {
        }

        public override void Configure(IEndpointEnricherOptions options)
        {
            _Options = (WorkItemLinkEnricherOptions)options;
        }
    }
}