using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Configuration;
using MigrationTools.DataContracts;
using Serilog;

namespace MigrationTools.Clients
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

        public override string CreateReflectedWorkItemId(WorkItemData workItem)
        {
            var wi = workItem.ToWorkItem();
            return string.Format("{0}/{1}/_workitems/edit/{2}", wi.Store.TeamProjectCollection.Uri.ToString().TrimEnd('/'), wi.Project.Name, wi.Id);
        }

        public override WorkItemData FindReflectedWorkItem(WorkItemData workItem, bool cache)
        {
            string ReflectedWorkItemId = CreateReflectedWorkItemId(workItem);

            var workItemToFind = workItem.ToWorkItem();

            WorkItem found = null;
            if (Cache.ContainsKey(workItemToFind.Id))
            {
                return Cache[workItemToFind.Id]; ;
            }

            // If we have a Reflected WorkItem ID field on the source store, assume it is pointing to the desired work item on the target store
            if (Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName != null && workItemToFind.Fields.Contains(Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName) && !string.IsNullOrEmpty(workItemToFind.Fields[Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName]?.Value?.ToString()))
            {
                string rwiid = workItemToFind.Fields[Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName].Value.ToString();
                int idToFind = GetReflectedWorkItemId(workItem);
                if (idToFind == 0)
                {
                    found = null;
                }
                else
                {
                    try
                    {
                        found = Store.GetWorkItem(idToFind);
                    }
                    catch (DeniedOrNotExistException)
                    {
                        found = null;
                    }
                }
            }
            if (found == null) { found = FindReflectedWorkItemByReflectedWorkItemId(ReflectedWorkItemId)?.ToWorkItem(); }
            if (Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName != null && !workItemToFind.Fields.Contains(Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName))
            {
                if (found == null) { found = FindReflectedWorkItemByMigrationRef(ReflectedWorkItemId)?.ToWorkItem(); } // Too slow!
                                                                                                                       //if (found == null) { found = FindReflectedWorkItemByTitle(workItemToFind.Title); }
            }
            if (found != null && cache)
            {
                AddToCache(workItemToFind.Id, found.AsWorkItemData());/// TODO MEMORY LEAK
            }
            return found?.AsWorkItemData();
        }

        public override WorkItemData FindReflectedWorkItemByMigrationRef(string refId)
        {
            IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
            StringBuilder queryBuilder = FindReflectedWorkItemQueryBase(wiqb);
            queryBuilder.Append(" [System.Description] Contains @KeyToFind");
            wiqb.AddParameter("KeyToFind", string.Format("##REF##{0}##", refId));
            wiqb.Query = queryBuilder.ToString();
            return FindWorkItemByQuery(wiqb);
        }

        public override WorkItemData FindReflectedWorkItemByReflectedWorkItemId(WorkItemData refWi)
        {
            return FindReflectedWorkItemByReflectedWorkItemId(CreateReflectedWorkItemId(refWi));
        }

        public override WorkItemData FindReflectedWorkItemByReflectedWorkItemId(int refId, bool cache)
        {
            var IdKey = ~refId;
            if (Cache.TryGetValue(IdKey, out var workItem)) return workItem;

            IEnumerable<WorkItemData> QueryWorkItems()
            {
                IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
                wiqb.Query = string.Format(@"SELECT [System.Id] FROM WorkItems  WHERE [System.TeamProject]=@TeamProject AND [{0}] Contains '@idToFind'", MigrationClient.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName);
                wiqb.AddParameter("idToFind", refId.ToString());
                wiqb.AddParameter("TeamProject", MigrationClient.Config.AsTeamProjectConfig().Project);

                foreach (WorkItemData wi in wiqb.BuildWIQLQuery(MigrationClient).GetWorkItems())
                {
                    yield return wi;
                }
            }

            var foundWorkItem = QueryWorkItems().FirstOrDefault(wi => wi.ToWorkItem().Fields[MigrationClient.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName].Value.ToString().EndsWith("/" + refId));
            if (cache && foundWorkItem != null)
            {
                AddToCache(IdKey, foundWorkItem);
            }
            return foundWorkItem;
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

        public override WorkItemData FindReflectedWorkItemByTitle(string title)
        {
            IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
            wiqb.Query = @"SELECT [System.Id] FROM WorkItems  WHERE [System.TeamProject]=@TeamProject AND [System.Title] = @TitleToFind";
            wiqb.AddParameter("TitleToFind", title);
            wiqb.AddParameter("TeamProject", MigrationClient.Config.AsTeamProjectConfig().Project);
            return FindWorkItemByQuery(wiqb);
        }

        public override ProjectData GetProject()
        {
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            Project y;
            try
            {
                y = (from Project x in Store.Projects where x.Name.ToUpper() == MigrationClient.Config.AsTeamProjectConfig().Project.ToUpper() select x).SingleOrDefault();
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
                Log.Error(ex, "Unable to configure store");
                throw;
            }
            return y.ToProjectData();
        }

        public override int GetReflectedWorkItemId(WorkItemData workItem)
        {
            Log.Debug("GetReflectedWorkItemId: START");
            var local = workItem.ToWorkItem();
            if (!local.Fields.Contains(Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName))
            {
                Log.Debug("GetReflectedWorkItemId: END - no reflected work item id on work item");
                return 0;
            }
            string rwiid = local.Fields[Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName].Value.ToString();
            if (Regex.IsMatch(rwiid, @"(http(s)?://)?([\w-]+\.)+[\w-]+(/[\w- ;,./?%&=]*)?"))
            {
                Log.Debug("GetReflectedWorkItemId: END - Has ReflectedWorkItemIdField and has value");
                return int.Parse(rwiid.Substring(rwiid.LastIndexOf(@"/") + 1));
            }
            Log.Debug("GetReflectedWorkItemId: END - Has ReflectedWorkItemIdField but has no value");
            return 0;
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
                store = new WorkItemStore(MigrationClient.Config.AsTeamProjectConfig().Collection.ToString(), bypassRules);
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