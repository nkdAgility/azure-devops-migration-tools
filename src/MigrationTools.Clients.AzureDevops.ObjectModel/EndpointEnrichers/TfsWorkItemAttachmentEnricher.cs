using System;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;

namespace MigrationTools.EndpointEnrichers
{
    /// <summary>
    /// The `TfsWorkItemAttachmentEnricher` processes the attachements for a specific work item.
    /// </summary>
    /// <processingtarget>WorkItem</processingtarget>
    public class TfsWorkItemAttachmentEnricher : WorkItemAttachmentEnricher
    {
        private WorkItemAttachmentEnricherOptions _Options;

        public TfsWorkItemAttachmentEnricher(IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemAttachmentEnricher> logger) : base(services, telemetry, logger)
        {
        }

        public override void EnrichWorkItemData(IEndpoint endpoint, object dataSource, RevisionItem dataTarget)
        {
            throw new NotImplementedException();
        }
    }
}