﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Options;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem;

namespace MigrationTools.Endpoints
{
    public class TfsWorkItemEndpoint : GenericTfsEndpoint<TfsWorkItemEndpointOptions>, IWorkItemSourceEndpoint, IWorkItemTargetEndpoint
    {
        public TfsWorkItemEndpoint(IOptions<TfsWorkItemEndpointOptions> options, EndpointEnricherContainer endpointEnrichers, IServiceProvider serviceProvider, ITelemetryLogger telemetry, ILogger<Endpoint<TfsWorkItemEndpointOptions>> logger) : base(options, endpointEnrichers, serviceProvider, telemetry, logger)
        {
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
            var wis = TfsStore.Query(query.Query, query.Parameters);
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