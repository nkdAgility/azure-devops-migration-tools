using System;
using Microsoft.Extensions.Logging;

namespace MigrationTools.Enrichers
{
    public class WorkItemEmbedEnricher : WorkItemEndpointEnricher
    {
        private WorkItemEmbedEnricherOptions _Options;

        public WorkItemEmbedEnricherOptions Options
        {
            get { return _Options; }
        }

        public WorkItemEmbedEnricher(IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemEndpointEnricher> logger) : base(services, telemetry, logger)
        {
        }

        public override void Configure(IEndpointEnricherOptions options)
        {
            _Options = (WorkItemEmbedEnricherOptions)options;
        }
    }
}