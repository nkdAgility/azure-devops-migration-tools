using System;
using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.DataContracts;
using MigrationTools.DataContracts;

namespace MigrationTools._EngineV1.Clients
{
    public abstract class WorkItemMigrationClientBase : IWorkItemMigrationClient
    {
        private Dictionary<string, WorkItemData> _Cache = new Dictionary<string, WorkItemData>();

        public WorkItemMigrationClientBase(ITelemetryLogger telemetry)
        {
            Telemetry = telemetry;
        }

        public abstract IMigrationClientConfig Config { get; }
        public abstract ProjectData Project { get; }
        protected IMigrationClient MigrationClient { get; private set; }
        protected ITelemetryLogger Telemetry { get; }

        public void Configure(IMigrationClient migrationClient, bool bypassRules = true)
        {
            if (migrationClient is null)
            {
                throw new ArgumentNullException(nameof(migrationClient));
            }
            MigrationClient = migrationClient;
            InnerConfigure(migrationClient, bypassRules);
        }

        public abstract ReflectedWorkItemId CreateReflectedWorkItemId(WorkItemData workItem);

        public abstract WorkItemData FindReflectedWorkItem(WorkItemData reflectedWorkItem, bool cache);

        public abstract WorkItemData FindReflectedWorkItemByReflectedWorkItemId(string reflectedWorkItemId);

        public abstract WorkItemData FindReflectedWorkItemByReflectedWorkItemId(WorkItemData reflectedWorkItem);

        public abstract ProjectData GetProject();

        public abstract ReflectedWorkItemId GetReflectedWorkItemId(WorkItemData workItem);

        public abstract WorkItemData GetRevision(WorkItemData workItem, int revision);

        public abstract WorkItemData GetWorkItem(string id, bool stopOnError = true);

        public abstract WorkItemData GetWorkItem(int id, bool stopOnError = true);

        public abstract List<WorkItemData> GetWorkItems();

        public abstract List<int> GetWorkItemIds(string WIQLQuery);

        public abstract List<WorkItemData> GetWorkItems(string WIQLQuery);

        public abstract List<WorkItemData> GetWorkItems(IWorkItemQueryBuilder queryBuilder);

        protected abstract void InnerConfigure(IMigrationClient migrationClient, bool bypassRules = true);

        public abstract WorkItemData PersistWorkItem(WorkItemData workItem);

        protected void AddToCache(WorkItemData workItem)
        {
            string key = GetReflectedWorkItemId(workItem).ToString();
            if (!_Cache.ContainsKey(key))
            {
                _Cache.Add(key, workItem);
            }
        }

        protected void AddToCache(List<WorkItemData> workItems)
        {
            foreach (WorkItemData workItem in workItems)
            {
                AddToCache(workItem);
            }
        }

        protected WorkItemData GetFromCache(ReflectedWorkItemId reflectedWorkItemId)
        {
            if (_Cache.ContainsKey(reflectedWorkItemId.ToString()))
            {
                return _Cache[reflectedWorkItemId.ToString()];
            }
            return null;
        }
    }
}