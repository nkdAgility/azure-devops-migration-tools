using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Clients;
using MigrationTools.Configuration;
using MigrationTools.DataContracts;
using Serilog;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.Clients
{
    public class WorkItemMigrationClient : WorkItemMigrationClientBase
    {
        private WorkItemStoreFlags _bypassRules;
        private WorkItemStore _wistore;
        private TeamProjectConfig _config;
        private ProjectData _project;

        public WorkItemStore Store { get { return _wistore; } }

        public override TeamProjectConfig Config => _config;

        public override ProjectData Project { get { return _project; } }

        public WorkItemMigrationClient(IServiceProvider services, ITelemetryLogger telemetry) : base(services, telemetry)
        {

        }

        public override void InnerConfigure(IMigrationClient migrationClient, bool bypassRules = true)
        {
            _config = MigrationClient.Config;
            _bypassRules = bypassRules ? WorkItemStoreFlags.BypassRules : WorkItemStoreFlags.None;
            _wistore = new WorkItemStore(MigrationClient.Config.Collection.ToString(), _bypassRules);
            _project = migrationClient.WorkItems.GetProject();
        }


        public override List<WorkItemData> GetWorkItems()
        {
            throw new NotImplementedException();
        }

        public override List<WorkItemData> GetWorkItems(string query)
        {
            IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
            wiqb.Query = query;
            return GetWorkItems(wiqb);
        }
        public override List<WorkItemData> GetWorkItems(IWorkItemQueryBuilder queryBuilder)
        {
            queryBuilder.AddParameter("TeamProject", MigrationClient.Config.Project);
            return queryBuilder.BuildWIQLQuery(MigrationClient).GetWorkItems();
        }


        public override WorkItemData PersistWorkItem(WorkItemData workItem)
        {
            throw new NotImplementedException();
        }

        public override ProjectData GetProject()
        {

            Project y = (from Project x in Store.Projects where x.Name.ToUpper() == MigrationClient.Config.Project.ToUpper() select x).SingleOrDefault();
            return y.ToProjectData();
        }

        public override string CreateReflectedWorkItemId(WorkItemData workItem)
        {
            var wi = workItem.ToWorkItem();
            return string.Format("{0}/{1}/_workitems/edit/{2}", wi.Store.TeamProjectCollection.Uri.ToString().TrimEnd('/'), wi.Project.Name, wi.Id);

        }
        public override int GetReflectedWorkItemId(WorkItemData workItem)
        {
            Log.Debug("GetReflectedWorkItemId: START");
            var local = workItem.ToWorkItem();
            if (!local.Fields.Contains(Config.ReflectedWorkItemIDFieldName))
            {
                Log.Debug("GetReflectedWorkItemId: END - no reflected work item id on work item");
                return 0;
            }
            string rwiid = local.Fields[Config.ReflectedWorkItemIDFieldName].Value.ToString();
            if (Regex.IsMatch(rwiid, @"(http(s)?://)?([\w-]+\.)+[\w-]+(/[\w- ;,./?%&=]*)?"))
            {
                Log.Debug("GetReflectedWorkItemId: END - Has ReflectedWorkItemIdField and has value");
                return int.Parse(rwiid.Substring(rwiid.LastIndexOf(@"/") + 1));
            }
            Log.Debug("GetReflectedWorkItemId: END - Has ReflectedWorkItemIdField but has no value");
            return 0;
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
            if (Config.ReflectedWorkItemIDFieldName != null && workItemToFind.Fields.Contains(Config.ReflectedWorkItemIDFieldName) && !string.IsNullOrEmpty(workItemToFind.Fields[Config.ReflectedWorkItemIDFieldName]?.Value?.ToString()))
            {
                string rwiid = workItemToFind.Fields[Config.ReflectedWorkItemIDFieldName].Value.ToString();
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
            if (Config.ReflectedWorkItemIDFieldName != null && !workItemToFind.Fields.Contains(Config.ReflectedWorkItemIDFieldName))
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
                wiqb.Query = string.Format(@"SELECT [System.Id] FROM WorkItems  WHERE [System.TeamProject]=@TeamProject AND [{0}] Contains '@idToFind'", MigrationClient.Config.ReflectedWorkItemIDFieldName);
                wiqb.AddParameter("idToFind", refId.ToString());
                wiqb.AddParameter("TeamProject", MigrationClient.Config.Project);

                foreach (WorkItemData wi in wiqb.BuildWIQLQuery(MigrationClient).GetWorkItems())
                {
                    yield return wi;
                }
            }

            var foundWorkItem = QueryWorkItems().FirstOrDefault(wi => wi.ToWorkItem().Fields[MigrationClient.Config.ReflectedWorkItemIDFieldName].Value.ToString().EndsWith("/" + refId));
            if (cache && foundWorkItem != null)
            {
                AddToCache(IdKey, foundWorkItem);
            }
            return foundWorkItem;
        }

        private StringBuilder FindReflectedWorkItemQueryBase(IWorkItemQueryBuilder query)
        {
            StringBuilder s = new StringBuilder();
            s.Append("SELECT [System.Id] FROM WorkItems");
            s.Append(" WHERE ");
            if (!MigrationClient.Config.AllowCrossProjectLinking)
            {
                s.Append("[System.TeamProject]=@TeamProject AND ");
                query.AddParameter("TeamProject", MigrationClient.Config.Project);
            }
            return s;
        }

        public override WorkItemData FindReflectedWorkItemByReflectedWorkItemId(string refId)
        {
            IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
            StringBuilder queryBuilder = FindReflectedWorkItemQueryBase(wiqb);
            queryBuilder.AppendFormat("[{0}] = @idToFind", MigrationClient.Config.ReflectedWorkItemIDFieldName);
            wiqb.AddParameter("idToFind", refId.ToString());
            wiqb.Query = queryBuilder.ToString();
            return FindWorkItemByQuery(wiqb);
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

        public override WorkItemData FindReflectedWorkItemByTitle(string title)
        {
            IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
            wiqb.Query = @"SELECT [System.Id] FROM WorkItems  WHERE [System.TeamProject]=@TeamProject AND [System.Title] = @TitleToFind";
            wiqb.AddParameter("TitleToFind", title);
            wiqb.AddParameter("TeamProject", MigrationClient.Config.Project);
            return FindWorkItemByQuery(wiqb);
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

        public override WorkItemData GetRevision(WorkItemData workItem, int revision)
        {
            return Store.GetWorkItem(int.Parse(workItem.Id), revision).AsWorkItemData();
        }


        public override WorkItemData GetWorkItem(string id)
        {
            return GetWorkItem(int.Parse(id));
        }

        public override WorkItemData GetWorkItem(int id)
        {
            return Store.GetWorkItem(id)?.AsWorkItemData();
        }
    }
}
