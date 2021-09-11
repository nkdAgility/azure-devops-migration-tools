using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Options;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem;

namespace MigrationTools.Endpoints
{
    public class TfsWorkItemEndpoint : GenericTfsEndpoint<TfsWorkItemEndpointOptions>, IWorkItemSourceEndpoint, IWorkItemTargetEndpoint
    {
        public TfsWorkItemEndpoint(EndpointEnricherContainer endpointEnrichers, ITelemetryLogger telemetry, ILogger<TfsWorkItemEndpoint> logger)
            : base(endpointEnrichers, telemetry, logger)
        {
        }

        public override void Configure(TfsWorkItemEndpointOptions options)
        {
            base.Configure(options);
            Log.LogDebug("TfsWorkItemEndPoint::Configure");
            if (string.IsNullOrEmpty(Options.Query?.Query))
            {
                throw new ArgumentNullException(nameof(Options.Query));
            }
        }

        public void Filter(IEnumerable<WorkItemData> workItems)
        {
            Log.LogDebug("TfsWorkItemEndPoint::Filter");
        }

        public IEnumerable<WorkItemData> GetWorkItems()
        {
            Log.LogDebug("TfsWorkItemEndPoint::GetWorkItems");
            if (string.IsNullOrEmpty(Options.Query?.Query))
            {
                throw new ArgumentNullException(nameof(Options.Query));
            }
            return GetWorkItems(Options.Query);
        }

        public IEnumerable<WorkItemData> GetWorkItems(QueryOptions query)
        {
            Log.LogDebug("TfsWorkItemEndPoint::GetWorkItems(query)");
            var wis = TfsStore.Query(query.Query, query.Paramiters);
            return ToWorkItemDataList(wis);
        }

        private List<WorkItemData> ToWorkItemDataList(WorkItemCollection collection)
        {
            List<WorkItemData> list = new List<WorkItemData>();
            TfsWorkItemConvertor tfswic = new TfsWorkItemConvertor();

            foreach (WorkItem wi in collection)
            {
                WorkItemData wid = new WorkItemData { internalObject = wi };
                tfswic.MapWorkItemtoWorkItemData(wid, wi, null);
                list.Add(wid);
            }
            return list;
        }

        public void PersistWorkItem(WorkItemData source)
        {
            Log.LogDebug("TfsWorkItemEndPoint::PersistWorkItem");
        }
    }
}