using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly IWorkItemQueryBuilderFactory _workItemQueryBuilderFactory;
        private WorkItemStoreFlags _bypassRules;
        private IMigrationClientConfig _config;
        private ProjectData _project;
        private WorkItemStore _wistore;

        public TfsWorkItemMigrationClient(IWorkItemQueryBuilderFactory workItemQueryBuilderFactory, ITelemetryLogger telemetry)
            : base(telemetry)
        {
            _workItemQueryBuilderFactory = workItemQueryBuilderFactory;
        }

        public override IMigrationClientConfig Config => _config;
        public override ProjectData Project { get { return _project; } }
        public WorkItemStore Store { get { return _wistore; } }

        public List<WorkItemData> FilterExistingWorkItems(
            List<WorkItemData> sourceWorkItems,
            TfsWiqlDefinition wiqlDefinition,
            TfsWorkItemMigrationClient sourceWorkItemMigrationClient)
        {
            Log.Debug("FilterExistingWorkItems: START | ");

            var targetQuery =
                string.Format(
                    @"SELECT [System.Id], [{0}] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {1} ORDER BY {2}",
                     _config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName,
                    wiqlDefinition.QueryBit,
                    wiqlDefinition.OrderBit);

            Log.Debug("FilterByTarget: Query Execute...");
            var targetFoundItems = GetWorkItems(targetQuery);
            Log.Debug("FilterByTarget: ... query complete.");
            Log.Debug("FilterByTarget: Found {TargetWorkItemCount} based on the WIQLQueryBit in the target system.", targetFoundItems.Count);
            var targetFoundIds = (from WorkItemData twi in targetFoundItems select GetReflectedWorkItemId(twi))
                //exclude null IDs
                .Where(x=> x != null)
                .ToList();
            //////////////////////////////////////////////////////////
            sourceWorkItems = sourceWorkItems.Where(p => targetFoundIds.All(p2 => p2.ToString() != sourceWorkItemMigrationClient.CreateReflectedWorkItemId(p).ToString())).ToList();
            Log.Debug("FilterByTarget: After removing all found work items there are {SourceWorkItemCount} remaining to be migrated.", sourceWorkItems.Count);
            Log.Debug("FilterByTarget: END");
            return sourceWorkItems;
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
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            WorkItem y;
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
                            { "CollectionUrl", MigrationClient.Config.AsTeamProjectConfig().Collection.ToString() }
                       },
                       new Dictionary<string, double> {
                            { "Time",timer.ElapsedMilliseconds }
                       });
                Log.Error(ex, "Unable to GetWorkItem with id[{id}]", id);
                throw;
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