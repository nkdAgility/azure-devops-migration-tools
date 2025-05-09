using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.DataContracts;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.Services;
using Serilog;

namespace MigrationTools.Clients
{
    public class TfsWorkItemMigrationClient : WorkItemMigrationClientBase
    {
        private ITelemetryLogger _telemetry;
        private readonly IWorkItemQueryBuilderFactory _workItemQueryBuilderFactory;
        private WorkItemStoreFlags _bypassRules;
        private Lazy<ProjectData> _project;
        private Lazy<WorkItemStore> _wistore;

        public TfsWorkItemMigrationClient(IOptions<TfsTeamProjectEndpointOptions> options, IMigrationClient migrationClient, IWorkItemQueryBuilderFactory workItemQueryBuilderFactory, ITelemetryLogger telemetry)
            : base(options, migrationClient, telemetry)
        {
            _telemetry = telemetry;
            _workItemQueryBuilderFactory = workItemQueryBuilderFactory;
            _bypassRules = WorkItemStoreFlags.BypassRules;

            _wistore = new Lazy<WorkItemStore>(() =>
            {
                Console.WriteLine("Initializing expensive WorkItemStore...");
                return GetWorkItemStore();
            });
            _project = new Lazy<ProjectData>(() =>
            {
                Console.WriteLine("Initializing expensive ProjectData from WorkItemStore...");
                return GetProject();
            });
        }

        new TfsTeamProjectEndpointOptions Options => (TfsTeamProjectEndpointOptions)base.Options;
        public override ProjectData Project { get { return _project.Value; } }
        public WorkItemStore Store { get { return _wistore.Value; } }

        public List<WorkItemData> FilterExistingWorkItems(
            List<WorkItemData> sourceWorkItems,
            string query,
            TfsWorkItemMigrationClient sourceWorkItemMigrationClient)
        {
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("FilterExistingWorkItems"))
            {
                Log.Debug("FilterByTarget: Query Execute...");
                var targetFoundItems = GetWorkItems(query);
                Log.Debug("FilterByTarget: ... query complete.");
                Log.Debug("FilterByTarget: Found {TargetWorkItemCount} based on the WIQLQuery in the target system.", targetFoundItems.Count);
                var targetFoundIds = (from WorkItemData twi in targetFoundItems select GetReflectedWorkItemId(twi))
                    //exclude null IDs
                    .Where(x => x != null)
                    .ToList();
                //////////////////////////////////////////////////////////
                var sourceWorkItems2 = sourceWorkItems.Where(p => targetFoundIds.All(p2 => p2.ToString() != sourceWorkItemMigrationClient.CreateReflectedWorkItemId(p).ToString())).ToList();
                Log.Debug("FilterByTarget: After removing all found work items there are {SourceWorkItemCount} remaining to be migrated.", sourceWorkItems.Count);
                Log.Debug("FilterByTarget: END");

                //_telemetry.TrackEvent("FilterExistingWorkItems", new Dictionary<string, string> { { "Project", Options.Project }, { "CollectionName", Options.CollectionName } }, new Dictionary<string, double> { { "sourceWorkItems", sourceWorkItems.Count }, { "targetWorkItems", targetFoundItems.Count }, { "resultWorkItems", sourceWorkItems2.Count } });
                return sourceWorkItems2;
            }
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
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("GetProject", ActivityKind.Client))
            {
                activity?.SetTagsFromOptions(Options);
                activity?.SetTag("url.full", Store.TeamProjectCollection.Uri);
                activity?.SetTag("server.address", Store.TeamProjectCollection.Uri);
                activity?.SetTag("http.request.method", "GET");
                activity?.SetTag("http.response.status_code", "500");
                activity?.SetTag("migrationtools.client", "TfsObjectModel");
                activity?.SetEndTime(activity.StartTimeUtc.AddSeconds(10));

                Project y = null;
                try
                {
                    y = (from Project x in Store.Projects where string.Equals(x.Name, Options.Project, StringComparison.OrdinalIgnoreCase) select x).SingleOrDefault(); // Use Single instead of SingleOrDefault to force an exception here
                    if (y == null)
                    {
                        Log.Fatal("The project `{ConfiguredProjectName}` does not exist in the collection. Please fix to continue.", Options.Project);
                        Log.Error("Valid options are: @{projects}", Store.Projects.Cast<Project>().Select(x => x.Name).ToList());
                        Environment.Exit(-1);
                    }
                    activity?.Stop();
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                catch (Exception ex)
                {
                    activity?.Stop();
                    activity?.SetStatus(ActivityStatusCode.Error);
                    Log.Error(ex, "The project `{ConfiguredProjectName}` does not exist in the collection. Please fix to continue.", Options.Project);
                    ActivitySourceProvider.FlushTelemetery();
                    Environment.Exit(-1);
                }
                return y?.ToProjectData();
            }
        }

        public override ReflectedWorkItemId CreateReflectedWorkItemId(WorkItemData workItem)
        {
            return new TfsReflectedWorkItemId(workItem);
        }

        public override ReflectedWorkItemId GetReflectedWorkItemId(WorkItemData workItem)
        {
            Log.Debug("GetReflectedWorkItemId: START");
            var local = workItem.ToWorkItem();

            if (!local.Fields.Contains(Options.ReflectedWorkItemIdField))
            {
                Log.Debug("GetReflectedWorkItemId: END - no reflected work item id on work item");
                return null;
            }
            string rwiid = local.Fields[Options.ReflectedWorkItemIdField].Value.ToString();
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

        public override WorkItemData GetWorkItem(string id, bool stopOnError = true)
        {
            return GetWorkItem(int.Parse(id), stopOnError);
        }

        public override WorkItemData GetWorkItem(int id, bool stopOnError = true)
        {
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("GetWorkItem", ActivityKind.Client))
            {
                activity?.SetTag("url.full", Store.TeamProjectCollection.Uri);
                activity?.SetTag("server.address", Store.TeamProjectCollection.Uri);
                activity?.SetTag("http.request.method", "GET");
                activity?.SetTag("http.response.status_code", "500");
                activity?.SetTag("migrationtools.client", "TfsObjectModel");
                activity?.SetEndTime(activity.StartTimeUtc.AddSeconds(10));
                if (id == 0)
                {
                    throw new ArgumentOutOfRangeException("id", id, "id cant be empty.");
                }
                WorkItem y = null;
                try
                {
                    Log.Debug("TfsWorkItemMigrationClient::GetWorkItem({id})", id);
                    y = Store.GetWorkItem(id);
                    activity?.Stop();
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                catch (Exception ex)
                {
                    activity?.Stop();
                    activity?.SetStatus(ActivityStatusCode.Error);
                    Telemetry.TrackException(ex, activity.Tags);
                    Log.Error(ex, "Unable to GetWorkItem with id[{id}]", id);
                    if (stopOnError)
                    {
                        ActivitySourceProvider.FlushTelemetery();
                        Environment.Exit(-1);
                    }
                }
                return y?.AsWorkItemData();
            }
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
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("GetWorkItems"))
            {
                activity?.SetTagsFromOptions(Options);
                activity?.SetTag("WIQLQuery", WIQLQuery);
                var query = GetWorkItemQuery(WIQLQuery);
                return query.GetWorkItems();
            }
        }

        public override List<WorkItemData> GetWorkItems(IWorkItemQueryBuilder queryBuilder)
        {
            queryBuilder.AddParameter("TeamProject", Options.Project);
            return queryBuilder.BuildWIQLQuery(MigrationClient).GetWorkItems();
        }

        private Endpoints.IWorkItemQuery GetWorkItemQuery(string WIQLQuery)
        {
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("GetWorkItemQuery"))
            {
                activity?.SetTagsFromOptions(Options);
                activity?.SetTag("WIQLQuery", WIQLQuery);
                var wiqb = _workItemQueryBuilderFactory.Create();
                wiqb.Query = WIQLQuery;
                wiqb.AddParameter("TeamProject", Options.Project);
                wiqb.AddParameter("ReflectedWorkItemIdField", Options.ReflectedWorkItemIdField);
                return wiqb.BuildWIQLQuery(MigrationClient);
            }
        }



        public override WorkItemData PersistWorkItem(WorkItemData workItem)
        {
            throw new NotImplementedException();
        }

        protected WorkItemData FindReflectedWorkItemByReflectedWorkItemId(ReflectedWorkItemId refId, bool cache = true)
        {
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("GetWorkItemQuery"))
            {
                activity?.SetTagsFromOptions(Options);
                activity?.SetTag("ReflectedWorkItemId", refId);
                var foundWorkItem = GetFromCache(refId);
                if (foundWorkItem is null)
                {
                    var workItemQueryBuilder = CreateReflectedWorkItemQuery(refId.ToString());
                    var query = workItemQueryBuilder.BuildWIQLQuery(MigrationClient);
                    var items = query.GetWorkItems();
                    var reflectedFielName = Options.ReflectedWorkItemIdField;
                    foundWorkItem = items.FirstOrDefault(wi => wi.ToWorkItem().Fields[reflectedFielName].Value.ToString() == refId.ToString());
                    if (cache && foundWorkItem is not null)
                    {
                        AddToCache(foundWorkItem);
                    }
                }
                return foundWorkItem;
            }
        }

        private IWorkItemQueryBuilder CreateReflectedWorkItemQuery(string refId)
        {
            var workItemQueryBuilder = _workItemQueryBuilderFactory.Create();
            StringBuilder queryBuilder = new StringBuilder();
            queryBuilder.Append("SELECT [System.Id] FROM WorkItems");
            queryBuilder.Append(" WHERE ");
            if (!Options.AllowCrossProjectLinking)
            {
                queryBuilder.Append("[System.TeamProject]=@TeamProject AND ");
                workItemQueryBuilder.AddParameter("TeamProject", Options.Project);
            }

            queryBuilder.AppendFormat("[{0}] = @idToFind", Options.ReflectedWorkItemIdField);
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
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("GetWorkItemStore", ActivityKind.Client))
            {
                activity?.SetTag("url.full", Options.Collection);
                activity?.SetTag("server.address", Options.Collection);
                activity?.SetTag("http.request.method", "GET");
                activity?.SetTag("http.response.status_code", "500");
                activity?.SetTag("migrationtools.client", "TfsObjectModel");
                activity?.SetTag("migrationtools.WorkItemStoreFlags", _bypassRules.ToString());
                activity?.SetTagsFromOptions(Options);
                activity?.SetEndTime(activity.StartTimeUtc.AddSeconds(10));
                WorkItemStore store = null;
                try
                {
                    Log.Debug("TfsWorkItemMigrationClient::GetWorkItemStore({InternalCollection}, {bypassRules})", Options.Collection, _bypassRules);
                    store = new WorkItemStore((TfsTeamProjectCollection)MigrationClient.InternalCollection, _bypassRules);
                    activity?.Stop();
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    Log.Information("Work Item Store connected to {InternalCollection} with BypassRules set to {bypassRules}", Options.Collection, store.BypassRules);
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
                    activity?.Stop();
                    activity?.SetStatus(ActivityStatusCode.Error);
                    Telemetry.TrackException(ex, activity.Tags);
                    Log.Fatal(ex, "Unable to configure store");
                    ActivitySourceProvider.FlushTelemetery();
                    Environment.Exit(-1);
                }
                return store;
            }
        }
    }
}
