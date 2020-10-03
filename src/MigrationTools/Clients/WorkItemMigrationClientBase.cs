using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.ApplicationInsights.DataContracts;
using MigrationTools.Configuration;
using MigrationTools.DataContracts;
using Serilog;

namespace MigrationTools.Clients
{
    public abstract class WorkItemMigrationClientBase : IWorkItemMigrationClient
    {

        private Dictionary<int, WorkItemData> _Cache = new Dictionary<int, WorkItemData>();
        private IMigrationClient _migrationClient;

        protected IMigrationClient MigrationClient { get { return _migrationClient; } }
        protected IServiceProvider Services { get; }
        protected ITelemetryLogger Telemetry { get; }

        protected ReadOnlyDictionary<int, WorkItemData> Cache { get { return new ReadOnlyDictionary<int, WorkItemData>(_Cache); } }

        public abstract TeamProjectConfig Config { get; }
        public abstract ProjectData Project { get; }

        public WorkItemMigrationClientBase(IServiceProvider services, ITelemetryLogger telemetry)
        {
            Services = services;
            Telemetry = telemetry;
        }

        public void Configure(IMigrationClient migrationClient, bool bypassRules = true)
        {
            if (migrationClient is null)
            {
                throw new ArgumentNullException(nameof(migrationClient));
            }
            _migrationClient = migrationClient;

            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                InnerConfigure(migrationClient, bypassRules);
                timer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TeamService", "GetWorkItemStore", startTime, timer.Elapsed, true));
            }
            catch (Exception ex)
            {
                timer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TeamService", "GetWorkItemStore", startTime, timer.Elapsed, false));
                Telemetry.TrackException(ex,
                       new Dictionary<string, string> {
                            { "CollectionUrl", MigrationClient.Config.Collection.ToString() }
                       },
                       new Dictionary<string, double> {
                            { "Time",timer.ElapsedMilliseconds }
                       });
                Log.Error(ex, "Unable to configure store");
                throw;
            }
        }

        protected void AddToCache(int sourceIdKey, WorkItemData workItem)
        {
            _Cache.Add(sourceIdKey, workItem);
        }

        public abstract void InnerConfigure(IMigrationClient migrationClient, bool bypassRules = true);
        public abstract List<WorkItemData> GetWorkItems();
        public abstract List<WorkItemData> GetWorkItems(string WIQLQuery);
        public abstract List<WorkItemData> GetWorkItems(IWorkItemQueryBuilder queryBuilder);
        public abstract WorkItemData PersistWorkItem(WorkItemData workItem);
        public abstract WorkItemData GetRevision(WorkItemData workItem, int revision);
        public abstract WorkItemData FindReflectedWorkItemByTitle(string title);
        public abstract WorkItemData FindReflectedWorkItemByMigrationRef(string refId);
        public abstract WorkItemData FindReflectedWorkItemByReflectedWorkItemId(string refId);
        public abstract WorkItemData FindReflectedWorkItemByReflectedWorkItemId(int refId, bool cache);
        public abstract WorkItemData FindReflectedWorkItemByReflectedWorkItemId(WorkItemData refWi);
        public abstract string CreateReflectedWorkItemId(WorkItemData workItem);
        public abstract int GetReflectedWorkItemId(WorkItemData workItem);
        public abstract WorkItemData FindReflectedWorkItem(WorkItemData workItem, bool cache);
        public abstract ProjectData GetProject();
        public abstract WorkItemData GetWorkItem(string id);
        public abstract WorkItemData GetWorkItem(int id);

    }
}
