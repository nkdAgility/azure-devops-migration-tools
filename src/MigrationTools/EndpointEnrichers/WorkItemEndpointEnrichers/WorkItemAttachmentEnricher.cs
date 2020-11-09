using System;
using Microsoft.Extensions.Logging;

namespace MigrationTools.EndpointEnrichers
{
    public class WorkItemAttachmentEnricher : WorkItemEndpointEnricher
    {
        private WorkItemAttachmentEnricherOptions _Options;

        public WorkItemAttachmentEnricherOptions Options
        {
            get { return _Options; }
        }

        public WorkItemAttachmentEnricher(IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemAttachmentEnricher> logger) : base(services, telemetry, logger)
        {
        }

        public override void Configure(IEndpointEnricherOptions options)
        {
            _Options = (WorkItemAttachmentEnricherOptions)options;
        }
    }
}