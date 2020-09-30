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
using MigrationTools.Core.Clients;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.DataContracts;
using Serilog;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.Clients
{
    public class WorkItemMigrationClient : IWorkItemMigrationClient
    {
        private WorkItemStoreFlags _bypassRules;
        private WorkItemStore _wistore;
        private Dictionary<int, WorkItemData> foundWis;
        private MigrationClient _migrationClient;

        internal WorkItemStore Store { get { return _wistore; } }

        protected MigrationClient MigrationClient { get { return _migrationClient; } }
        protected IServiceProvider Services { get; }
        protected ITelemetryLogger Telemetry { get; }

        public WorkItemMigrationClient( IServiceProvider services, ITelemetryLogger telemetry)
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
            _migrationClient = (MigrationClient)migrationClient;
            foundWis = new Dictionary<int, WorkItemData>();
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            _bypassRules = bypassRules ? WorkItemStoreFlags.BypassRules : WorkItemStoreFlags.None;
            try
            {
                _wistore = new WorkItemStore(MigrationClient.Config.Collection.ToString(), _bypassRules);
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

        public IEnumerable<WorkItemData> GetWorkItems()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WorkItemData> GetWorkItems(string query)
        {
            throw new NotImplementedException();
        }

        public WorkItemData PersistWorkItem(WorkItemData workItem)
        {
            throw new NotImplementedException();
        }

        public Project GetProject()
        {
            return (from Project x in Store.Projects where x.Name.ToUpper() == MigrationClient.Config.Project.ToUpper() select x).SingleOrDefault();
        }

        public string CreateReflectedWorkItemId(WorkItemData workItem)
        {
            var wi = workItem.ToWorkItem();
            return string.Format("{0}/{1}/_workitems/edit/{2}", wi.Store.TeamProjectCollection.Uri.ToString().TrimEnd('/'), wi.Project.Name, wi.Id);

        }
        public int GetReflectedWorkItemId(WorkItemData workItem, string reflectedWotkItemIdField)
        {
            var local = workItem.ToWorkItem();
            if (!local.Fields.Contains(reflectedWotkItemIdField))
            {
                return 0;
            }
            string rwiid = local.Fields[reflectedWotkItemIdField].Value.ToString();
            if (Regex.IsMatch(rwiid, @"(http(s)?://)?([\w-]+\.)+[\w-]+(/[\w- ;,./?%&=]*)?"))
            {
                return int.Parse(rwiid.Substring(rwiid.LastIndexOf(@"/") + 1));
            }
            return 0;
        }

        public WorkItemData FindReflectedWorkItem(WorkItemData workItem, bool cache, string sourceReflectedWIIdField = null)
        {
            string ReflectedWorkItemId = CreateReflectedWorkItemId(workItem);

            var workItemToFind = workItem.ToWorkItem();

            WorkItem found = null;
            if (foundWis.ContainsKey(workItemToFind.Id))
            {
                return foundWis[workItemToFind.Id]; ;
            }

            // If we have a Reflected WorkItem ID field on the source store, assume it is pointing to the desired work item on the target store
            if (sourceReflectedWIIdField != null && workItemToFind.Fields.Contains(sourceReflectedWIIdField) && !string.IsNullOrEmpty(workItemToFind.Fields[sourceReflectedWIIdField]?.Value?.ToString()))
            {
                string rwiid = workItemToFind.Fields[sourceReflectedWIIdField].Value.ToString();
                int idToFind = GetReflectedWorkItemId(workItem, sourceReflectedWIIdField);
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
            if (found == null) { found = FindReflectedWorkItemByReflectedWorkItemId(ReflectedWorkItemId).ToWorkItem(); }
            if (sourceReflectedWIIdField != null && !workItemToFind.Fields.Contains(sourceReflectedWIIdField))
            {
                if (found == null) { found = FindReflectedWorkItemByMigrationRef(ReflectedWorkItemId).ToWorkItem(); } // Too slow!
                //if (found == null) { found = FindReflectedWorkItemByTitle(workItemToFind.Title); }
            }
            if (found != null && cache)
            {
                foundWis.Add(workItemToFind.Id, found.ToWorkItemData()); /// TODO MEMORY LEAK
            }
            return found.ToWorkItemData();
        }

        public WorkItemData FindReflectedWorkItemByReflectedWorkItemId(WorkItemData refWi)
        {
            return FindReflectedWorkItemByReflectedWorkItemId(CreateReflectedWorkItemId(refWi));
        }

        public WorkItemData FindReflectedWorkItemByReflectedWorkItemId(int refId, bool cache)
        {
            var sourceIdKey = ~refId;
            if (foundWis.TryGetValue(sourceIdKey, out var workItem)) return workItem ;

            IEnumerable<WorkItemData> QueryWorkItems()
            {
                IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
                wiqb.Query = string.Format(@"SELECT [System.Id] FROM WorkItems  WHERE [System.TeamProject]=@TeamProject AND [{0}] Contains '@idToFind'", MigrationClient.Config.ReflectedWorkItemIDFieldName);
                wiqb.AddParameter("idToFind", refId.ToString());
                wiqb.AddParameter("TeamProject", MigrationClient.Config.Project);
                
                foreach (WorkItemData wi in wiqb.Build().GetWorkItems())
                {
                    yield return wi;
                }
            }

            var foundWorkItem = QueryWorkItems().FirstOrDefault(wi => wi.ToWorkItem().Fields[MigrationClient.Config.ReflectedWorkItemIDFieldName].Value.ToString().EndsWith("/" + refId));
            if (cache && foundWorkItem != null) foundWis[sourceIdKey] = foundWorkItem;
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

        public WorkItemData FindReflectedWorkItemByReflectedWorkItemId(string refId)
        {
            IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
            StringBuilder queryBuilder = FindReflectedWorkItemQueryBase(wiqb);
            queryBuilder.AppendFormat("[{0}] = @idToFind", MigrationClient.Config.ReflectedWorkItemIDFieldName);
            wiqb.AddParameter("idToFind", refId.ToString());
            wiqb.Query = queryBuilder.ToString();
            return FindWorkItemByQuery(wiqb);
        }

        public WorkItemData FindReflectedWorkItemByMigrationRef(string refId)
        {
            IWorkItemQueryBuilder wiqb = Services.GetRequiredService<IWorkItemQueryBuilder>();
            StringBuilder queryBuilder = FindReflectedWorkItemQueryBase(wiqb);
            queryBuilder.Append(" [System.Description] Contains @KeyToFind");
            wiqb.AddParameter("KeyToFind", string.Format("##REF##{0}##", refId));
            wiqb.Query = queryBuilder.ToString();
            return FindWorkItemByQuery(wiqb);
        }

        public WorkItemData FindReflectedWorkItemByTitle(string title)
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
            newFound = query.Build().GetWorkItems();
            if (newFound.Count == 0)
            {
                return null;
            }
            return newFound[0];
        }

        public WorkItemData GetRevision(WorkItemData workItem, int revision)
        {
            return Store.GetWorkItem(int.Parse(workItem.Id), revision).ToWorkItemData() ;
        }
    }
}
