using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.DataContracts;
using MigrationTools.DataContracts;
using Serilog;
using static Microsoft.TeamFoundation.Client.CommandLine.Options;

namespace MigrationTools._EngineV1.Clients
{
    public class TfsWorkItemMigrationClient : WorkItemMigrationClientBase
    {
        private ITelemetryLogger _telemetry;
        private readonly IWorkItemQueryBuilderFactory _workItemQueryBuilderFactory;
        private WorkItemStoreFlags _bypassRules;
        private IMigrationClientConfig _config;
        private ProjectData _project;
        private WorkItemStore _wistore;

        public TfsWorkItemMigrationClient(IWorkItemQueryBuilderFactory workItemQueryBuilderFactory, ITelemetryLogger telemetry)
            : base(telemetry)
        {
            _telemetry = telemetry;
            _workItemQueryBuilderFactory = workItemQueryBuilderFactory;
        }

        public override IMigrationClientConfig Config => _config;
        public override ProjectData Project { get { return _project; } }
        public WorkItemStore Store { get { return _wistore; } }

        public List<WorkItemData> FilterExistingWorkItems(
            List<WorkItemData> sourceWorkItems,
            string query,
            TfsWorkItemMigrationClient sourceWorkItemMigrationClient)
        {
            Log.Debug("FilterExistingWorkItems: START | ");


            Log.Debug("FilterByTarget: Query Execute...");
            var targetFoundItems = GetWorkItems(query);
            Log.Debug("FilterByTarget: ... query complete.");
            Log.Debug("FilterByTarget: Found {TargetWorkItemCount} based on the WIQLQuery in the target system.", targetFoundItems.Count);
            var targetFoundIds = (from WorkItemData twi in targetFoundItems select GetReflectedWorkItemId(twi))
                //exclude null IDs
                .Where(x=> x != null)
                .ToList();
            //////////////////////////////////////////////////////////
           var sourceWorkItems2 = sourceWorkItems.Where(p => targetFoundIds.All(p2 => p2.ToString() != sourceWorkItemMigrationClient.CreateReflectedWorkItemId(p).ToString())).ToList();
            Log.Debug("FilterByTarget: After removing all found work items there are {SourceWorkItemCount} remaining to be migrated.", sourceWorkItems.Count);
            Log.Debug("FilterByTarget: END");
            _telemetry.TrackEvent("FilterExistingWorkItems", new Dictionary<string, string> { { "Project", sourceWorkItemMigrationClient.Config.AsTeamProjectConfig().Project }, { "CollectionName", sourceWorkItemMigrationClient.Config.AsTeamProjectConfig().CollectionName } }, new Dictionary<string, double> { { "sourceWorkItems", sourceWorkItems.Count }, { "targetWorkItems", targetFoundItems.Count }, { "resultWorkItems", sourceWorkItems2.Count } });
            return sourceWorkItems2;
        }

        public override WorkItemData FindReflectedWorkItem(WorkItemData workItemToReflect, bool cache)
        {
            TfsReflectedWorkItemId ReflectedWorkItemId = new TfsReflectedWorkItemId(workItemToReflect);
            WorkItem found = GetFromCache(ReflectedWorkItemId)?.ToWorkItem();
            if (found == null)
            {
                found = FindReflectedWorkItemByReflectedWorkItemId(ReflectedWorkItemId)?.ToWorkItem();
            }
            if (found != null && cache)
            {
                AddToCache(found.AsWorkItemData());// TODO MEMORY LEAK
            }
            return found?.AsWorkItemData();
        }

        public override WorkItemData FindReflectedWorkItemByReflectedWorkItemId(WorkItemData workItemToReflect)
        {
            return FindReflectedWorkItemByReflectedWorkItemId(CreateReflectedWorkItemId(workItemToReflect));
        }

        public override WorkItemData FindReflectedWorkItemByReflectedWorkItemId(string refId)
        {
            var workItemQueryBuilder = CreateReflectedWorkItemQuery(refId);
            return FindWorkItemByQuery(workItemQueryBuilder);
        }

        public override ProjectData GetProject()
        {
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            Project y;
            try
            {
                y = (from Project x in Store.Projects where string.Equals(x.Name, MigrationClient.Config.AsTeamProjectConfig().Project, StringComparison.OrdinalIgnoreCase) select x).Single(); // Use Single instead of SingleOrDefault to force an exception here
                timer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", MigrationClient.Config.AsTeamProjectConfig().Collection.ToString(), "GetProject", null, startTime, timer.Elapsed, "200", true));
            }
            catch (Exception ex)
            {
                timer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", MigrationClient.Config.AsTeamProjectConfig().Collection.ToString(), "GetProject", null, startTime, timer.Elapsed, "500", false));
                Telemetry.TrackException(ex,
                       new Dictionary<string, string> {
                            { "CollectionUrl", MigrationClient.Config.AsTeamProjectConfig().Collection.ToString() }
                       },
                       new Dictionary<string, double> {
                            { "Time",timer.ElapsedMilliseconds }
                       });
                Log.Error(ex, "Unable to get project with name {ConfiguredProjectName}", MigrationClient.Config.AsTeamProjectConfig().Project);
                throw;
            }
            return y.ToProjectData(); // With SingleOrDefault earlier this would result in a NullReferenceException which is hard to debug
        }

        public override ReflectedWorkItemId CreateReflectedWorkItemId(WorkItemData workItem)
        {
            return new TfsReflectedWorkItemId(workItem);
        }

        public override ReflectedWorkItemId GetReflectedWorkItemId(WorkItemData workItem)
        {
            Log.Debug("GetReflectedWorkItemId: START");
            var local = workItem.ToWorkItem();

            if (!local.Fields.Contains(Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName))
            {
                Log.Debug("GetReflectedWorkItemId: END - no reflected work item id on work item");
                return null;
            }
            string rwiid = local.Fields[Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName].Value.ToString();
            if (!string.IsNullOrEmpty(rwiid))
            {
                Log.Debug("GetReflectedWorkItemId: END - Has ReflectedWorkItemIdField and has value");
                return new TfsReflectedWorkItemId(rwiid);
            }
            Log.Debug("GetReflectedWorkItemId: END - Has ReflectedWorkItemIdField but has no value");
            return null;
        }

        public override WorkItemData GetRevision(WorkItemData workItem, int revision)
        {
            throw new NotImplementedException("GetRevision in combination with WorkItemData is buggy");
        }

        public override WorkItemData GetWorkItem(string id)
        {
            return GetWorkItem(int.Parse(id));
        }

        public override WorkItemData GetWorkItem(int id)
        {
            if (id == 0)
            {
                throw new ArgumentOutOfRangeException("id", id, "id cant be empty.");
            }
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            WorkItem y = null ;
            try
            {
                Log.Debug("TfsWorkItemMigrationClient::GetWorkItem({id})", id);
                y = Store.GetWorkItem(id);
                timer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", MigrationClient.Config.AsTeamProjectConfig().Collection.ToString(), "GetWorkItem", null, startTime, timer.Elapsed, "200", true));
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex,
                       new Dictionary<string, string> {
                            { "CollectionUrl", MigrationClient.Config.AsTeamProjectConfig().Collection.ToString() },
                             { "Project", MigrationClient.Config.AsTeamProjectConfig().Project.ToString() },
                             { "WorkItem", id.ToString() }
                       },
                       new Dictionary<string, double> {
                            { "Time",timer.ElapsedMilliseconds }
                       });
                Log.Error(ex, "Unable to GetWorkItem with id[{id}]", id);
                Environment.Exit(-1);
            } finally
            {
                timer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", MigrationClient.Config.AsTeamProjectConfig().Collection.ToString(), "GetWorkItem", null, startTime, timer.Elapsed, "500", false));
                
            }
            return y?.AsWorkItemData();
        }

        public override List<WorkItemData> GetWorkItems()
        {
            throw new NotImplementedException();
        }

        public override List<int> GetWorkItemIds(string WIQLQuery)
        {
            var query = GetWorkItemQuery(WIQLQuery);
            return query.GetWorkItemIds();
        }

        public override List<WorkItemData> GetWorkItems(string WIQLQuery)
        {
            var query = GetWorkItemQuery(WIQLQuery);
            return query.GetWorkItems();
        }

        public override List<WorkItemData> GetWorkItems(IWorkItemQueryBuilder queryBuilder)
{
            queryBuilder.AddParameter("TeamProject", MigrationClient.Config.AsTeamProjectConfig().Project);
            return queryBuilder.BuildWIQLQuery(MigrationClient).GetWorkItems();
        }

        private Endpoints.IWorkItemQuery GetWorkItemQuery(string WIQLQuery)
        {
            var wiqb = _workItemQueryBuilderFactory.Create();
            wiqb.Query = WIQLQuery;
            wiqb.AddParameter("TeamProject", MigrationClient.Config.AsTeamProjectConfig().Project);
            wiqb.AddParameter("ReflectedWorkItemIdFieldName", MigrationClient.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName);
            return wiqb.BuildWIQLQuery(MigrationClient);
        }

        protected override void InnerConfigure(IMigrationClient migrationClient, bool bypassRules = true)
        {
            _config = MigrationClient.Config;
            _bypassRules = bypassRules ? WorkItemStoreFlags.BypassRules : WorkItemStoreFlags.None;
            _wistore = GetWorkItemStore();
            _project = migrationClient.WorkItems.GetProject();
        }

        public override WorkItemData PersistWorkItem(WorkItemData workItem)
        {
            throw new NotImplementedException();
        }

        protected WorkItemData FindReflectedWorkItemByReflectedWorkItemId(ReflectedWorkItemId refId, bool cache = true)
        {
            var foundWorkItem = GetFromCache(refId);
            if (foundWorkItem is null)
            {
                var workItemQueryBuilder = CreateReflectedWorkItemQuery(refId.ToString());
                var query = workItemQueryBuilder.BuildWIQLQuery(MigrationClient);
                var items = query.GetWorkItems();
                var reflectedFielName = MigrationClient.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName;
                foundWorkItem = items.FirstOrDefault(wi => wi.ToWorkItem().Fields[reflectedFielName].Value.ToString() == refId.ToString());
                if (cache && foundWorkItem is not null)
                {
                    AddToCache(foundWorkItem);
                }
            }
            return foundWorkItem;
        }

        private IWorkItemQueryBuilder CreateReflectedWorkItemQuery(string refId)
        {
            var workItemQueryBuilder = _workItemQueryBuilderFactory.Create();
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append("SELECT [System.Id] FROM WorkItems");
            queryBuilder.Append(" WHERE ");
            if (!MigrationClient.Config.AsTeamProjectConfig().AllowCrossProjectLinking)
            {
                queryBuilder.Append("[System.TeamProject]=@TeamProject AND ");
                workItemQueryBuilder.AddParameter("TeamProject", MigrationClient.Config.AsTeamProjectConfig().Project);
            }

            queryBuilder.AppendFormat("[{0}] = @idToFind", MigrationClient.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName);
            workItemQueryBuilder.AddParameter("idToFind", refId);
            workItemQueryBuilder.Query = queryBuilder.ToString();
            return workItemQueryBuilder;
        }

        private WorkItemData FindWorkItemByQuery(IWorkItemQueryBuilder query)
        {
            var newFound = query.BuildWIQLQuery(MigrationClient).GetWorkItems();
            if (newFound.Count == 0)
            {
                return null;
            }
            return newFound[0];
        }

        private WorkItemStore GetWorkItemStore()
        {
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            WorkItemStore store;
            try
            {
                Log.Debug("TfsWorkItemMigrationClient::GetWorkItemStore({InternalCollection}, {bypassRules})", _config.AsTeamProjectConfig().Collection, _bypassRules);
                store = new WorkItemStore((TfsTeamProjectCollection)MigrationClient.InternalCollection, _bypassRules);
                timer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", MigrationClient.Config.AsTeamProjectConfig().Collection.ToString(), "GetWorkItemStore", null, startTime, timer.Elapsed, "200", true));
                Log.Information("Work Item Store connected to {InternalCollection} with BypassRules set to {bypassRules}", _config.AsTeamProjectConfig().Collection, store.BypassRules);
                if (_bypassRules == WorkItemStoreFlags.BypassRules)
                {
                    if (store.BypassRules == false)
                    {
                        Log.Warning("TfsWorkItemMigrationClient::BypassRules Is not Enabled. Check your permissions on the server!");
                    }
                }

            }
            catch (Exception ex)
            {
                timer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", MigrationClient.Config.AsTeamProjectConfig().Collection.ToString(), "GetWorkItemStore", null, startTime, timer.Elapsed, "500", false));
                Telemetry.TrackException(ex,
                       new Dictionary<string, string> {
                            { "CollectionUrl", MigrationClient.Config.AsTeamProjectConfig().Collection.ToString() }
                       },
                       new Dictionary<string, double> {
                            { "Time",timer.ElapsedMilliseconds }
                       });
                Log.Error(ex, "Unable to configure store");
                throw;
            }
            return store;
        }
    }
}