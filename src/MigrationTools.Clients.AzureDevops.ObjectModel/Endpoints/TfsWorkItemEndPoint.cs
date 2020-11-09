using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using MigrationTools.DataContracts;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Options;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItem;

namespace MigrationTools.Endpoints
{
    public class TfsWorkItemEndPoint : WorkItemEndpoint
    {
        private TfsTeamProjectCollection _Collection;
        private TfsWorkItemEndPointOptions _Options;
        private WorkItemStore _Store;

        public TfsWorkItemEndPoint(EndpointEnricherContainer endpointEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemEndpoint> logger) : base(endpointEnrichers, services, telemetry, logger)
        {
        }

        public override int Count => 0;

        public override void Configure(IEndpointOptions options)
        {
            base.Configure(options);
            Log.LogDebug("TfsWorkItemEndPoint::Configure");
            _Options = (TfsWorkItemEndPointOptions)options;
            ValidateConfiguration(_Options);
        }

        public override void Filter(IEnumerable<WorkItemData> workItems)
        {
            Log.LogDebug("TfsWorkItemEndPoint::Filter");
            EnsureConnection();
        }

        public override IEnumerable<WorkItemData> GetWorkItems()
        {
            Log.LogDebug("TfsWorkItemEndPoint::GetWorkItems");
            if (string.IsNullOrEmpty(_Options.Query?.Query))
            {
                throw new ArgumentNullException(nameof(_Options.Query));
            }
            return GetWorkItems(_Options.Query);
        }

        public override IEnumerable<WorkItemData> GetWorkItems(QueryOptions query)
        {
            Log.LogDebug("TfsWorkItemEndPoint::GetWorkItems(query)");
            EnsureConnection();
            var wis = _Store.Query(query.Query, query.Paramiters);
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

        public override void PersistWorkItem(WorkItemData source)
        {
            Log.LogDebug("TfsWorkItemEndPoint::PersistWorkItem");
            EnsureConnection();
        }

        private void EnsureConnection()
        {
            Log.LogDebug("TfsWorkItemEndPoint::RefreshData");
            ValidateConfiguration(_Options);
            TfsTeamProjectCollection tfs = GetTfsCollection();
            WorkItemStore store = GetWorkItemStore(tfs, WorkItemStoreFlags.BypassRules);
            Log.LogDebug("TfsWorkItemEndPoint::EnsureConnection: Validated connection to {TeamProjectCollection} ", store.TeamProjectCollection.DisplayName);
        }

        private TfsTeamProjectCollection GetTfsCollection()
        {
            if (_Collection is null)
            {
                var startTime = DateTime.UtcNow;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                VssCredentials vssCredentials;
                try
                {
                    if (!string.IsNullOrEmpty(_Options.AccessToken))
                    {
                        Log.LogDebug("TfsWorkItemEndPoint::GetTfsCollection: Using PAT Authentication ", _Options.Organisation);
                        vssCredentials = new VssBasicCredential(string.Empty, _Options.AccessToken);
                    }
                    else
                    {
                        Log.LogDebug("TfsWorkItemEndPoint::EnsureDataSource: Using Interactive Authentication ", _Options.Organisation);
                        vssCredentials = new VssCredentials();
                    }
                    Log.LogDebug(Microsoft.TeamFoundation.Framework.Common.LocationServiceConstants.ApplicationLocationServiceIdentifier.ToString());
                    Log.LogDebug("TfsWorkItemEndPoint::GetTfsCollection: Connecting to {CollectionUrl} ", _Options.Organisation);
                    _Collection = new TfsTeamProjectCollection(new Uri(_Options.Organisation), vssCredentials);
                    Log.LogDebug("TfsWorkItemEndPoint::GetTfsCollection: Connected ");
                    Log.LogDebug("TfsWorkItemEndPoint::GetTfsCollection: validating security for {@AuthorizedIdentity} ", _Collection.AuthorizedIdentity);
                    _Collection.EnsureAuthenticated();
                    timer.Stop();
                    Log.LogInformation("TfsWorkItemEndPoint::GetTfsCollection: Access granted to {CollectionUrl} for {Name} ({Account})", _Options.Organisation, _Collection.AuthorizedIdentity.DisplayName, _Collection.AuthorizedIdentity.UniqueName);
                    Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", _Options.Organisation, "GetTfsCollection", null, startTime, timer.Elapsed, "200", true));
                }
                catch (Exception ex)
                {
                    timer.Stop();
                    Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", _Options.Organisation, "GetTfsCollection", null, startTime, timer.Elapsed, "500", false));
                    Log.LogError(ex, "Unable to connect to {Organisation}", _Options.Organisation);
                    throw;
                }
            }
            return _Collection;
        }

        private WorkItemStore GetWorkItemStore(TfsTeamProjectCollection tfs, WorkItemStoreFlags bypassRules)
        {
            if (_Store is null)
            {
                var startTime = DateTime.UtcNow;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    _Store = new WorkItemStore(tfs, bypassRules);
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

            return _Store;
        }

        private void ValidateConfiguration(TfsWorkItemEndPointOptions options)
        {
            if (string.IsNullOrEmpty(_Options.Organisation))
            {
                throw new ArgumentNullException(nameof(_Options.Organisation));
            }
            if (string.IsNullOrEmpty(_Options.Project))
            {
                throw new ArgumentNullException(nameof(_Options.Project));
            }
            if (string.IsNullOrEmpty(_Options.AccessToken))
            {
                throw new ArgumentNullException(nameof(_Options.AccessToken));
            }
            if (string.IsNullOrEmpty(_Options.Query?.Query))
            {
                throw new ArgumentNullException(nameof(_Options.Query));
            }
        }
    }
}