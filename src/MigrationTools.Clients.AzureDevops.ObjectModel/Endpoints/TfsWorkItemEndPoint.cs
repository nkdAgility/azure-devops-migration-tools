using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Options;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem;

namespace MigrationTools.Endpoints
{
    public class TfsWorkItemEndpoint : TfsEndpoint, IWorkItemSourceEndPoint, IWorkItemTargetEndPoint
    {
        private TfsWorkItemEndpointOptions _Options;
        private WorkItemStore _InnerStore;

        protected WorkItemStore Store
        {
            get
            {
                return GetWorkItemStore(Collection, WorkItemStoreFlags.BypassRules);
            }
        }

        public TfsWorkItemEndpoint(EndpointEnricherContainer endpointEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Endpoint> logger) : base(endpointEnrichers, services, telemetry, logger)
        {
        }

        public override int Count => 0;

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
            var wis = Store.Query(query.Query, query.Paramiters);
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

        private WorkItemStore GetWorkItemStore(TfsTeamProjectCollection tfs, WorkItemStoreFlags bypassRules)
        {
            if (_InnerStore is null)
            {
                var startTime = DateTime.UtcNow;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    _InnerStore = new WorkItemStore(tfs, bypassRules);
                    timer.Stop();
                    Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", _Options.Organisation, "GetWorkItemStore", null, startTime, timer.Elapsed, "200", true));
                }
                catch (Exception ex)
                {
                    timer.Stop();
                    Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", _Options.Organisation, "GetWorkItemStore", null, startTime, timer.Elapsed, "500", false));
                    Log.LogError(ex, "Unable to connect to {Organisation} Store", _Options.Organisation);
                    throw;
                }
            }

            return _InnerStore;
        }
    }
}