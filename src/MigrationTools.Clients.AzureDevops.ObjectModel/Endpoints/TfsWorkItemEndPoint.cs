using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Options;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem;

namespace MigrationTools.Endpoints
{
    public class TfsWorkItemEndpoint : TfsEndpoint, IWorkItemSourceEndpoint, IWorkItemTargetEndpoint, ITfsWorkItemEndpointOptions
    {
        private TfsWorkItemEndpointOptions _Options;

        public QueryOptions Query => _Options.Query;

        public TfsWorkItemEndpoint(EndpointEnricherContainer endpointEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Endpoint> logger) : base(endpointEnrichers, services, telemetry, logger)
        {
        }

        public override void Configure(IEndpointOptions options)
        {
            base.Configure(options);
            Log.LogDebug("TfsWorkItemEndPoint::Configure");
            _Options = (TfsWorkItemEndpointOptions)options;
            if (string.IsNullOrEmpty(_Options.Query?.Query))
            {
                throw new ArgumentNullException(nameof(_Options.Query));
            }
        }

        public void Filter(IEnumerable<WorkItemData> workItems)
        {
            Log.LogDebug("TfsWorkItemEndPoint::Filter");
        }

        public IEnumerable<WorkItemData> GetWorkItems()
        {
            Log.LogDebug("TfsWorkItemEndPoint::GetWorkItems");
            if (string.IsNullOrEmpty(_Options.Query?.Query))
            {
                throw new ArgumentNullException(nameof(_Options.Query));
            }
            return GetWorkItems(_Options.Query);
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
            foreach (WorkItem wi in collection)
            {
                list.Add(ConvertToWorkItemData(wi));
            }
            return list;
        }

        private WorkItemData ConvertToWorkItemData(WorkItem wi)
        {
            WorkItemData wid = new WorkItemData
            {
                Id = wi.Id.ToString(),
                Direction = _Options.Direction,
                Type = wi.Type.ToString()
            };
            PopulateRevisions(wi, wid);
            return wid;
        }

        private void PopulateRevisions(WorkItem wi, WorkItemData wid)
        {
            wid.Revisions = new List<RevisionItem>();
            foreach (Revision revision in wi.Revisions)
            {
                RevisionItem revi = new RevisionItem
                {
                    Number = revision.Index,
                    Index = revision.Index
                };
                RunSourceEnrichers(revision, revi);
                wid.Revisions.Add(revi);
            }
        }

        private void RunSourceEnrichers(Revision wi, RevisionItem wid)
        {
            Log.LogDebug("TfsWorkItemEndPoint::RunSourceEnrichers::{SourceEnrichersCount}", SourceEnrichers.Count());
            foreach (IWorkItemEndpointSourceEnricher enricher in SourceEnrichers)
            {
                enricher.EnrichWorkItemData(this, wi, wid); // HELP:: is this Right
            }
        }

        public void PersistWorkItem(WorkItemData source)
        {
            Log.LogDebug("TfsWorkItemEndPoint::PersistWorkItem");
        }
    }
}