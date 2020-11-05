using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;

namespace MigrationTools.Endpoints
{
    public class TfsWorkItemEndPoint : WorkItemEndpoint
    {
        private TfsWorkItemEndPointOptions _Options;

        public TfsWorkItemEndPoint(EndpointEnricherContainer endpointEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemEndpoint> logger) : base(endpointEnrichers, services, telemetry, logger)
        {
        }

        public override int Count => 0;
        public override EndpointDirection Direction => _Options.Direction;
        public override IEnumerable<IWorkItemProcessorSourceEnricher> SourceEnrichers => throw new NotImplementedException();
        public override IEnumerable<IWorkItemProcessorTargetEnricher> TargetEnrichers => throw new NotImplementedException();

        public override void Configure(IEndpointOptions options)
        {
            Log.LogDebug("TfsWorkItemEndPoint::Configure");
            _Options = (TfsWorkItemEndPointOptions)options;
        }

        public override void Filter(IEnumerable<WorkItemData> workItems)
        {
            Log.LogDebug("TfsWorkItemEndPoint::Configure::Filter");
        }

        public override IEnumerable<WorkItemData> GetWorkItems()
        {
            Log.LogDebug("TfsWorkItemEndPoint::Configure::GetWorkItems");
            return new List<WorkItemData>();
        }

        public override IEnumerable<WorkItemData> GetWorkItems(IWorkItemQuery query)
        {
            Log.LogDebug("TfsWorkItemEndPoint::Configure::GetWorkItems(query)");
            return new List<WorkItemData>();
        }

        public override void PersistWorkItem(WorkItemData source)
        {
            Log.LogDebug("TfsWorkItemEndPoint::Configure::PersistWorkItem");
        }
    }
}