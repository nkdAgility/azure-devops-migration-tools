using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.DataContracts;
using Serilog;

namespace MigrationTools._EngineV1.Clients
{
    public class TfsWorkItemMigrationClient : WorkItemMigrationClientBase
    {
        private WorkItemStoreFlags _bypassRules;
        private IMigrationClientConfig _config;
        private IMigrationClient _migrationClient;
        private ProjectData _project;
        private WorkItemStore _wistore;

        public TfsWorkItemMigrationClient(IServiceProvider services, ITelemetryLogger telemetry) : base(services, telemetry)
        {
        }

        public override IMigrationClientConfig Config => _config;
        public override ProjectData Project { get { return _project; } }
        public WorkItemStore Store { get { return _wistore; } }

        public override ReflectedWorkItemId CreateReflectedWorkItemId(WorkItemData workItem)
        {
            return new TfsReflectedWorkItemId(workItem);
        }

        public List<WorkItemData> FilterExistingWorkItems(List<WorkItemData> sourceWorkItems,
            TfsWiqlDefinition wiqlDefinition, TfsWorkItemMigrationClient sourceWorkItemMigrationClient)
        {
            Log.Debug("FilterExistingWorkItems: START | ");

            var targetQuery =
                string.Format(
                    @"SELECT [System.Id], [{0}] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {1} ORDER BY {2}",
                     _config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName,
                    wiqlDefinition.QueryBit,
                    wiqlDefinition.OrderBit
                    );

            Log.Debug("FilterByTarget: Query Execute...");
            var targetFoundItems = GetWorkItems(targetQuery);
            Log.Debug("FilterByTarget: ... query complete.");
            Log.Debug("FilterByTarget: Found {TargetWorkItemCount} based on the WIQLQueryBit in the target system.", targetFoundItems.Count);
            var targetFoundIds = (from WorkItemData twi in targetFoundItems select GetReflectedWorkItemId(twi)).ToList();
            //////////////////////////////////////////////////////////
            sourceWorkItems = sourceWorkItems.Where(p => targetFoundIds.All(p2 => p2.ToString() != sourceWorkItemMigrationClient.CreateReflectedWorkItemId(p).ToString())).ToList();
            Log.Debug("FilterByTarget: After removing all found work items there are {SourceWorkItemCount} remaining to be migrated.", sourceWorkItems.Count);
            Log.Debug("FilterByTarget: END");
            return sourceWorkItems;
        }

        public override WorkItemData FindReflectedWorkItem(WorkItemData workItemToReflect, bool cache)

        {
            TfsReflectedWorkItemId ReflectedWorkItemId = new TfsReflectedWorkItemId(workItemToReflect);

            var workItemToFind = workItemToReflect.ToWorkItem();

            WorkItem found = GetFromCache(ReflectedWorkItemId)?.ToWorkItem();
            // SHOULD NEVER HAVE A rEFLECTEDiD ON SOURCE! IF WE DO ITS IRRELEVENT AS ITS FROM A PREVIOUS MIGRATION
            // If we have a Reflected WorkItem ID field on the source store, assume it is pointing to the desired work item on the target store
            //if (Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName != null && workItemToFind.Fields.Contains(Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName) && !string.IsNullOrEmpty(workItemToFind.Fields[Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName]?.Value?.ToString()))
            //{
            //    string rwiid = workItemToFind.Fields[Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName].Value.ToString();
            //    ReflectedWorkItemId idToFind = GetReflectedWorkItemId(workItemToReflect);
            //    if (int.Parse(idToFind.WorkItemId) == 0)
            //    {
            //        found = null;
            //    }
            //    else
            //    {
            //        try
            //        {
            //            found = Store.GetWorkItem(int.Parse(idToFind.WorkItemId));
            //        }
            //        catch (DeniedOrNotExistException)
            //        {
            //            found = null;
            //        }
            //    }
            //}
            if (found == null) { found = FindReflectedWorkItemByReflectedWorkItemId(ReflectedWorkItemId)?.ToWorkItem(); }
            if (Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName != null && !workItemToFind.Fields.Contains(Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName))
            {
                if (found == null) { found = FindReflectedWorkItemByMigrationRef(ReflectedWorkItemId)?.ToWorkItem(); } // Too slow!
                                                                                                                       //if (found == null) { found = FindReflectedWorkItemByTitle(workItemToFind.Title); }
            }
            if (found != null && cache)
            {
                AddToCache(found.AsWorkItemData());/// TODO MEMORY LEAK
            }
            return found?.AsWorkItemData();
        }

        public override WorkItemData FindReflectedWorkItemByReflectedWorkItemId(WorkItemData workItemToReflect)
        {
            return FindReflectedWorkItemByReflectedWorkItemId(CreateReflectedWorkItemId(workItemToReflect));
        }

        public override WorkItemData FindReflectedWorkItemByReflectedWorkItemId(string refId)
        {
            IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
            StringBuilder queryBuilder = FindReflectedWorkItemQueryBase(wiqb);
            queryBuilder.AppendFormat("[{0}] = @idToFind", MigrationClient.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName);
            wiqb.AddParameter("idToFind", refId.ToString());
            wiqb.Query = queryBuilder.ToString();
            return FindWorkItemByQuery(wiqb);
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
            WorkItemData result;
            try
            {
                result = Store.GetWorkItem(int.Parse(workItem.Id), revision).AsWorkItemData();
            }
            catch (Exception)
            {
                throw;
            }
            return result;
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
                y = Store.GetWorkItem(id);
                timer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", MigrationClient.Config.AsTeamProjectConfig().Collection.ToString(), "GetWorkItem", null, startTime, timer.Elapsed, "200", true));
            }
            catch (Exception ex)
            {
                timer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", MigrationClient.Config.AsTeamProjectConfig().Collection.ToString(), "GetWorkItem", null, startTime, timer.Elapsed, "500", false));
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
            return y?.AsWorkItemData();
        }

        public override List<WorkItemData> GetWorkItems()
        {
            throw new NotImplementedException();
        }

        public override List<WorkItemData> GetWorkItems(string WIQLQuery)
        {
            IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
            wiqb.Query = WIQLQuery;
            return GetWorkItems(wiqb);
        }

        public override List<WorkItemData> GetWorkItems(IWorkItemQueryBuilder queryBuilder)
        {
            queryBuilder.AddParameter("TeamProject", MigrationClient.Config.AsTeamProjectConfig().Project);
            return queryBuilder.BuildWIQLQuery(MigrationClient).GetWorkItems();
        }

        public override void InnerConfigure(IMigrationClient migrationClient, bool bypassRules = true)
        {
            _migrationClient = migrationClient;
            _config = MigrationClient.Config;
            _bypassRules = bypassRules ? WorkItemStoreFlags.BypassRules : WorkItemStoreFlags.None;
            _wistore = GetWorkItemStore(_bypassRules);
            _project = migrationClient.WorkItems.GetProject();
        }

        public override WorkItemData PersistWorkItem(WorkItemData workItem)
        {
            throw new NotImplementedException();
        }

        protected WorkItemData FindReflectedWorkItemByMigrationRef(ReflectedWorkItemId refId)
        {
            IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
            StringBuilder queryBuilder = FindReflectedWorkItemQueryBase(wiqb);
            queryBuilder.Append(" [System.Description] Contains @KeyToFind");
            wiqb.AddParameter("KeyToFind", string.Format("##REF##{0}##", refId));
            wiqb.Query = queryBuilder.ToString();
            return FindWorkItemByQuery(wiqb);
        }

        protected WorkItemData FindReflectedWorkItemByReflectedWorkItemId(ReflectedWorkItemId refId, bool cache = true)
        {
            var foundWorkItem = GetFromCache(refId);
            if (foundWorkItem is null)
            {
                IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
                wiqb.Query = string.Format(@"SELECT [System.Id] FROM WorkItems  WHERE [System.TeamProject]=@TeamProject AND [{0}] = '@idToFind'", MigrationClient.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName);
                wiqb.AddParameter("idToFind", refId.ToString());
                wiqb.AddParameter("TeamProject", MigrationClient.Config.AsTeamProjectConfig().Project);
                var query = wiqb.BuildWIQLQuery(MigrationClient);
                var items = query.GetWorkItems();

                foundWorkItem = items.FirstOrDefault(wi => wi.ToWorkItem().Fields[MigrationClient.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName].Value.ToString() == refId.ToString());
                if (cache && foundWorkItem != null)
                {
                    AddToCache(foundWorkItem);
                }
            }
            return foundWorkItem;
        }

        private StringBuilder FindReflectedWorkItemQueryBase(IWorkItemQueryBuilder query)
        {
            StringBuilder s = new StringBuilder();
            s.Append("SELECT [System.Id] FROM WorkItems");
            s.Append(" WHERE ");
            if (!MigrationClient.Config.AsTeamProjectConfig().AllowCrossProjectLinking)
            {
                s.Append("[System.TeamProject]=@TeamProject AND ");
                query.AddParameter("TeamProject", MigrationClient.Config.AsTeamProjectConfig().Project);
            }
            return s;
        }

        private WorkItemData FindWorkItemByQuery(IWorkItemQueryBuilder query)
        {
            List<WorkItemData> newFound;
            newFound = query.BuildWIQLQuery(MigrationClient).GetWorkItems();
            if (newFound.Count == 0)
            {
                return null;
            }
            return newFound[0];
        }

        private WorkItemStore GetWorkItemStore(WorkItemStoreFlags bypassRules)
        {
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            WorkItemStore store;
            try
            {
                store = new WorkItemStore((TfsTeamProjectCollection)MigrationClient.InternalCollection, bypassRules);
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