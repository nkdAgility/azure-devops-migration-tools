using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;

namespace MigrationTools.Endpoints
{
    public class TfsWorkItemEndPoint : WorkItemEndpoint
    {
        private TfsWorkItemEndPointOptions _Options;
        private TfsTeamProjectCollection _tfsTeamProjectCollection;

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
            Log.LogDebug("TfsWorkItemEndPoint::Filter");
            RefreshData();
        }

        public override IEnumerable<WorkItemData> GetWorkItems()
        {
            Log.LogDebug("TfsWorkItemEndPoint::GetWorkItems");
            RefreshData();
            return new List<WorkItemData>();
        }

        public override IEnumerable<WorkItemData> GetWorkItems(IWorkItemQuery query)
        {
            Log.LogDebug("TfsWorkItemEndPoint::GetWorkItems(query)");
            RefreshData();
            return new List<WorkItemData>();
        }

        public override void PersistWorkItem(WorkItemData source)
        {
            Log.LogDebug("TfsWorkItemEndPoint::PersistWorkItem");
            RefreshData();
        }

        private void RefreshData()
        {
            Log.LogDebug("TfsWorkItemEndPoint::RefreshData");
            if (string.IsNullOrEmpty(_Options.Organisation))
            {
                throw new ArgumentNullException(nameof(_Options.Organisation));
            }

            TfsTeamProjectCollection tfs = GetTfsCollection();
            WorkItemStore store = GetWorkItemStore(tfs, WorkItemStoreFlags.BypassRules);
        }

        private WorkItemStore GetWorkItemStore(TfsTeamProjectCollection tfs, WorkItemStoreFlags bypassRules)
        {
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            WorkItemStore store;
            try
            {
                store = new WorkItemStore(tfs, bypassRules);
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
            return store;
        }

        private TfsTeamProjectCollection GetTfsCollection()
        {
            if (_tfsTeamProjectCollection is null)
            {
                var startTime = DateTime.UtcNow;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                VssCredentials vssCredentials;
                try
                {
                    if (!string.IsNullOrEmpty(_Options.AccessToken))
                    {
                        Log.LogDebug("TfsWorkItemEndPoint::EnsureDataSource: Using PAT Authentication ", _Options.Organisation);
                        vssCredentials = new VssBasicCredential(string.Empty, _Options.AccessToken);
                    }
                    else
                    {
                        Log.LogDebug("TfsWorkItemEndPoint::EnsureDataSource: Using Interactive Authentication ", _Options.Organisation);
                        vssCredentials = new VssCredentials();
                    }
                    Log.LogDebug(Microsoft.TeamFoundation.Framework.Common.LocationServiceConstants.ApplicationLocationServiceIdentifier.ToString());
                    Log.LogDebug("TfsWorkItemEndPoint::EnsureDataSource: Connecting to {CollectionUrl} ", _Options.Organisation);
                    _tfsTeamProjectCollection = new TfsTeamProjectCollection(new Uri(_Options.Organisation), vssCredentials);
                    Log.LogDebug("TfsWorkItemEndPoint::EnsureDataSource: Connected ");
                    Log.LogDebug("TfsWorkItemEndPoint::EnsureDataSource: validating security for {@AuthorizedIdentity} ", _tfsTeamProjectCollection.AuthorizedIdentity);
                    _tfsTeamProjectCollection.EnsureAuthenticated();
                    timer.Stop();
                    Log.LogInformation("TfsWorkItemEndPoint::EnsureDataSource: Access granted to {CollectionUrl} for {Name} ({Account})", _Options.Organisation, _tfsTeamProjectCollection.AuthorizedIdentity.DisplayName, _tfsTeamProjectCollection.AuthorizedIdentity.UniqueName);
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
            return _tfsTeamProjectCollection;
        }
    }
}