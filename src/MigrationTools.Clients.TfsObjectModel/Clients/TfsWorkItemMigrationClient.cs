using MigrationTools.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.DataContracts;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.Processors;
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

        private ReflectedWorkItemIdMap _reflectedIdMap;

        /// <summary>The map loaded by <see cref="LoadReflectedWorkItemIdMap"/>, or null when it has not been loaded.</summary>
        public ReflectedWorkItemIdMap ReflectedIdMap => _reflectedIdMap;

        /// <summary>
        /// Reads the reflected work item ID of every target work item with one WIQL query per field (#3299). From then on a
        /// reflected item is loaded by id and an item missing from the map is known not to be in the target, with no query.
        /// </summary>
        public ReflectedWorkItemIdMap LoadReflectedWorkItemIdMap()
        {
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("LoadReflectedWorkItemIdMap", ActivityKind.Client))
            {
                var map = new ReflectedWorkItemIdMap();
                try
                {
                    if (CollectionHasConfiguredField)
                    {
                        AddToMap(map, Options.ReflectedWorkItemIdField, null);
                    }
                    if (!string.Equals(Options.ReflectedWorkItemIdField, NonCustomisableWorkItemTypes.ReflectedWorkItemIdField, StringComparison.OrdinalIgnoreCase))
                    {
                        var types = string.Join(", ", NonCustomisableWorkItemTypes.Types.Select(t => "'" + t.Replace("'", "''") + "'"));
                        AddToMap(map, NonCustomisableWorkItemTypes.ReflectedWorkItemIdField, $"[System.WorkItemType] IN ({types})");
                    }
                }
                catch (Exception ex) when (ex is not MigrationRunStoppedException)
                {
                    // The map only saves queries, so a failure to load it must not stop the run.
                    map.MarkIncomplete();
                    Log.Warning("The map of migrated work items could not be loaded, so every work item is looked up one by one: {Reason}", WorkItemMigrationFailedException.ServerReason(ex));
                    Log.Debug(ex, "LoadReflectedWorkItemIdMap failed");
                }
                _reflectedIdMap = map;
                activity?.SetTag("ReflectedWorkItemIds", map.Count);
                return map;
            }
        }

        private void AddToMap(ReflectedWorkItemIdMap map, string field, string extraCondition)
        {
            var query = new StringBuilder($"SELECT [System.Id], [{field}] FROM WorkItems WHERE [{field}] <> ''");
            var parameters = new Dictionary<string, string>();
            if (!Options.AllowCrossProjectLinking)
            {
                query.Append(" AND [System.TeamProject] = @TeamProject");
                parameters["TeamProject"] = Options.Project;
            }
            if (extraCondition != null)
            {
                query.Append(" AND ").Append(extraCondition);
            }
            // Azure DevOps truncates a query at 20,000 items without an error, so the load is split into id ranges that
            // can never hold that many. The highest id comes first in a descending query, even a truncated one.
            Log.Debug("LoadReflectedWorkItemIdMap: {Query}", query);
            var newest = Store.Query(query + " ORDER BY [System.Id] DESC", parameters);
            if (newest.Count == 0)
            {
                return;
            }
            foreach (var (from, to) in ReflectedWorkItemIdMap.IdRanges(newest[0].Id, IdRangeWidth))
            {
                var items = Store.Query($"{query} AND [System.Id] >= {from} AND [System.Id] <= {to}", parameters);
                if (items.Count >= IncompleteMapThreshold)
                {
                    map.MarkIncomplete();
                    Log.Warning("{Count} target work items with a value in {Field} between ids {From} and {To}, close to the 20,000 item query limit: every work item is looked up one by one.", items.Count, field, from, to);
                }
                foreach (WorkItem wi in items)
                {
                    try
                    {
                        if (wi.Fields.Contains(field))
                        {
                            map.Add(wi.Fields[field].Value?.ToString(), wi.Id);
                        }
                    }
                    catch (Exception ex) when (ex is not MigrationRunStoppedException)
                    {
                        // For example TF201077 for an item in a project that is being deleted: the map cannot vouch for it.
                        if (map.IsComplete)
                        {
                            Log.Warning("Target work item {Id} could not be read while loading the map of migrated work items, so every work item is looked up one by one: {Reason}", wi.Id, WorkItemMigrationFailedException.ServerReason(ex));
                        }
                        map.MarkIncomplete();
                    }
                }
            }
        }

        /// <summary>Ids per map load query: fewer than the 20,000 item query limit, since ids are unique in a collection.</summary>
        public const int IdRangeWidth = 15000;

        /// <summary>A load query returning this many items may have been truncated at the 20,000 item limit.</summary>
        public const int IncompleteMapThreshold = 19000;

        /// <summary>
        /// When false, a source item missing from the map is queried instead of being taken as not in the target. Set
        /// while retrying a work item, whose earlier attempt may have saved a target item before failing.
        /// </summary>
        public bool MapMissesAreAuthoritative { get; set; } = true;

        /// <summary>Adds a target item created or found during the run to the map.</summary>
        public void RecordReflectedWorkItem(string reflectedWorkItemId, int targetId)
        {
            _reflectedIdMap?.Add(reflectedWorkItemId, targetId);
        }

        /// <summary>
        /// Answers a lookup from the map: the target item loaded by id, or null when the map shows it is not in the target.
        /// Returns false when there is no map or it cannot say, so the caller queries as before.
        /// </summary>
        private bool TryFindInMap(string refId, out WorkItemData found)
        {
            found = null;
            if (_reflectedIdMap is null || _reflectedIdMap.MustQuery(refId))
            {
                return false;
            }
            if (!_reflectedIdMap.TryGetTargetId(refId, out _) && !MapMissesAreAuthoritative)
            {
                return false;
            }
            if (_reflectedIdMap.TryGetTargetId(refId, out var targetId))
            {
                found = GetWorkItemIfExists(targetId);
                return found is not null;
            }
            return true;
        }

        public override WorkItemData FindReflectedWorkItem(WorkItemData workItemToReflect, bool cache)
        {
            TfsReflectedWorkItemId ReflectedWorkItemId = new TfsReflectedWorkItemId(workItemToReflect);
            if (TryFindInMap(ReflectedWorkItemId.ToString(), out var mapped))
            {
                return mapped;
            }
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
            if (TryFindInMap(refId, out var mapped))
            {
                return mapped;
            }
            if (!CollectionHasConfiguredField)
            {
                return FindInNonCustomisableTypes(refId);
            }
            var workItemQueryBuilder = CreateReflectedWorkItemQuery(refId);
            return FindWorkItemByQuery(workItemQueryBuilder) ?? FindInNonCustomisableTypes(refId);
        }

        /// <summary>
        /// A WIQL query on a field the collection does not define fails instead of returning nothing.
        /// </summary>
        private bool CollectionHasConfiguredField => Store.FieldDefinitions.Contains(Options.ReflectedWorkItemIdField);

        /// <summary>
        /// Finds a target work item of a type that cannot be customised, whose reflected work item ID is kept in
        /// <see cref="NonCustomisableWorkItemTypes.ReflectedWorkItemIdField"/>.
        /// </summary>
        private WorkItemData FindInNonCustomisableTypes(string refId)
        {
            if (string.Equals(Options.ReflectedWorkItemIdField, NonCustomisableWorkItemTypes.ReflectedWorkItemIdField, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            var workItemQueryBuilder = _workItemQueryBuilderFactory.Create();
            var query = new StringBuilder("SELECT [System.Id] FROM WorkItems WHERE ");
            if (!Options.AllowCrossProjectLinking)
            {
                query.Append("[System.TeamProject]=@TeamProject AND ");
                workItemQueryBuilder.AddParameter("TeamProject", Options.Project);
            }
            query.Append(NonCustomisableWorkItemTypes.BuildLookupQuery());
            workItemQueryBuilder.AddParameter("idToFind", refId);
            workItemQueryBuilder.Query = query.ToString();
            var found = FindWorkItemByQuery(workItemQueryBuilder);
            if (found is not null)
            {
                Log.Debug("FindInNonCustomisableTypes: found {TargetId} for {ReflectedWorkItemId} in {Field}", found.Id, refId, NonCustomisableWorkItemTypes.ReflectedWorkItemIdField);
            }
            return found;
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

                Project y = null;
                try
                {
                    y = (from Project x in Store.Projects where string.Equals(x.Name, Options.Project, StringComparison.OrdinalIgnoreCase) select x).SingleOrDefault(); // Use Single instead of SingleOrDefault to force an exception here
                    if (y == null)
                    {
                        Log.Fatal("The project `{ConfiguredProjectName}` does not exist in the collection. Please fix to continue.", Options.Project);
                        Log.Error("Valid options are: @{projects}", Store.Projects.Cast<Project>().Select(x => x.Name).ToList());
                        throw new MigrationRunStoppedException("The configured project does not exist in the collection.");
                    }
                    activity?.Stop();
                    activity?.SetStatus(ActivityStatusCode.Ok);
                }
                catch (MigrationRunStoppedException)
                {
                    activity?.Stop();
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
                catch (Exception ex)
                {
                    activity?.Stop();
                    activity?.SetStatus(ActivityStatusCode.Error);
                    Log.Error(ex, "The project `{ConfiguredProjectName}` does not exist in the collection. Please fix to continue.", Options.Project);
                    throw new MigrationRunStoppedException("The configured project does not exist in the collection.", ex);
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

            var field = NonCustomisableWorkItemTypes.ResolveField(local.Type.Name, Options.ReflectedWorkItemIdField, local.Fields.Contains);
            if (field is null)
            {
                Log.Debug("GetReflectedWorkItemId: END - no reflected work item id on work item");
                return null;
            }
            string rwiid = local.Fields[field].Value?.ToString();
            if (!string.IsNullOrEmpty(rwiid))
            {
                // The fallback field is a built-in field, so it may hold a value the migration did not write.
                if (field != Options.ReflectedWorkItemIdField && !TfsReflectedWorkItemId.IsReflectedWorkItemId(rwiid))
                {
                    Log.Debug("GetReflectedWorkItemId: END - {Field} holds a value that is not a reflected work item id", field);
                    return null;
                }
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
                    Telemetry.TrackException(ex, activity?.Tags);
                    Log.Error("Could not read work item {id} in {Collection}: {Reason} Check that it exists and that the migration identity can read its project.", id, Options.Collection, ex.Message);
                    Log.Debug(ex, "TfsWorkItemMigrationClient::GetWorkItem({id}) failed", id);
                    if (stopOnError)
                    {
                        // Rethrown as is: the caller decides whether this is an item failure or must stop the run.
                        throw;
                    }
                }
                return y?.AsWorkItemData();
            }
        }

        /// <summary>
        /// Gets a work item, or returns null when the server reports that it does not exist or that the
        /// migration identity cannot read it (TF26198, <see cref="DeniedOrNotExistException"/>).
        /// Any other error is thrown, so it is not mistaken for a missing work item.
        /// </summary>
        public WorkItemData GetWorkItemIfExists(int id)
        {
            return GetIfExists(id, i => Store.GetWorkItem(i)?.AsWorkItemData());
        }

        internal static WorkItemData GetIfExists(int id, Func<int, WorkItemData> load)
        {
            try
            {
                return load(id);
            }
            catch (DeniedOrNotExistException ex)
            {
                Log.Debug(ex, "TfsWorkItemMigrationClient::GetWorkItemIfExists({id}): denied or does not exist", id);
                return null;
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
                if (TryFindInMap(refId.ToString(), out var mapped))
                {
                    return mapped;
                }
                var foundWorkItem = GetFromCache(refId);
                if (foundWorkItem is null)
                {
                    if (CollectionHasConfiguredField)
                    {
                        var workItemQueryBuilder = CreateReflectedWorkItemQuery(refId.ToString());
                        var query = workItemQueryBuilder.BuildWIQLQuery(MigrationClient);
                        var items = query.GetWorkItems();
                        var reflectedFielName = Options.ReflectedWorkItemIdField;
                        foundWorkItem = items.FirstOrDefault(wi => wi.ToWorkItem().Fields[reflectedFielName].Value.ToString() == refId.ToString());
                    }
                    foundWorkItem ??= FindInNonCustomisableTypes(refId.ToString());
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
                catch (MigrationRunStoppedException)
                {
                    // Already a readable reason (for example a sign-in failure); wrapping it would bury it under a stack trace.
                    activity?.Stop();
                    activity?.SetStatus(ActivityStatusCode.Error);
                    throw;
                }
                catch (Exception ex)
                {
                    activity?.Stop();
                    activity?.SetStatus(ActivityStatusCode.Error);
                    Telemetry.TrackException(ex, activity?.Tags);
                    Log.Fatal(ex, "Unable to configure store");
                    throw new MigrationRunStoppedException("Unable to configure work item store.", ex);
                }
                return store;
            }
        }

        public override List<ProjectData> GetProjects()
        {
            List<ProjectData> projects = new List<ProjectData>();
            foreach (Project p in Store.Projects)
            {
                projects.Add(p?.ToProjectData());
            }
            return projects;
        }
    }
}
