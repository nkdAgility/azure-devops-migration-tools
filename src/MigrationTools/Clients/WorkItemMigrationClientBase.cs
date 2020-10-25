using System;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.Configuration;
using MigrationTools.DataContracts;

namespace MigrationTools.Clients
{
    public abstract class WorkItemMigrationClientBase : IWorkItemMigrationClient
    {
        private Dictionary<ReflectedWorkItemId, WorkItemData> _Cache = new Dictionary<ReflectedWorkItemId, WorkItemData>();
        private IMigrationClient _migrationClient;

        public WorkItemMigrationClientBase(IServiceProvider services, ITelemetryLogger telemetry)
        {
            Services = services;
            Telemetry = telemetry;
        }

        public abstract IMigrationClientConfig Config { get; }
        public abstract ProjectData Project { get; }
        protected IMigrationClient MigrationClient { get { return _migrationClient; } }
        protected IServiceProvider Services { get; }
        protected ITelemetryLogger Telemetry { get; }

        public void Configure(IMigrationClient migrationClient, bool bypassRules = true)
        {
            if (migrationClient is null)
            {
                throw new ArgumentNullException(nameof(migrationClient));
            }
            _migrationClient = migrationClient;
            InnerConfigure(migrationClient, bypassRules);
        }

        public abstract ReflectedWorkItemId CreateReflectedWorkItemId(WorkItemData workItem);

        public abstract WorkItemData FindReflectedWorkItem(WorkItemData reflectedWorkItem, bool cache);

        public abstract WorkItemData FindReflectedWorkItemByReflectedWorkItemId(string reflectedWorkItemId);

        public abstract WorkItemData FindReflectedWorkItemByReflectedWorkItemId(WorkItemData reflectedWorkItem);

        public abstract ProjectData GetProject();

        public abstract ReflectedWorkItemId GetReflectedWorkItemId(WorkItemData workItem);

        public abstract WorkItemData GetRevision(WorkItemData workItem, int revision);

        public abstract WorkItemData GetWorkItem(string id);

        public abstract WorkItemData GetWorkItem(int id);

        public abstract List<WorkItemData> GetWorkItems();

        public abstract List<WorkItemData> GetWorkItems(string WIQLQuery);

        public abstract List<WorkItemData> GetWorkItems(IWorkItemQueryBuilder queryBuilder);

        public abstract void InnerConfigure(IMigrationClient migrationClient, bool bypassRules = true);

        public abstract WorkItemData PersistWorkItem(WorkItemData workItem);

        protected void AddToCache(WorkItemData workItem)
        {
            _Cache.Add(GetReflectedWorkItemId(workItem), workItem);
        }

        protected void AddToCache(List<WorkItemData> workItems)
        {
            foreach (WorkItemData workItem in workItems)
            {
                _Cache.Add(GetReflectedWorkItemId(workItem), workItem);
            }
        }

        protected WorkItemData GetFromCache(ReflectedWorkItemId reflectedWorkItemId)
        {
            if (_Cache.ContainsKey(reflectedWorkItemId))
            {
                return _Cache[reflectedWorkItemId];
            }
            var found = (from ReflectedWorkItemId x in _Cache.Keys where x.ToString() == reflectedWorkItemId.ToString() select x).SingleOrDefault();
            if (found != null)
            {
                return _Cache[found];
            }
            return null;
        }
    }
}