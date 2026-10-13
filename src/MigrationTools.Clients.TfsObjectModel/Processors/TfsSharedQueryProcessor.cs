using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools;

namespace MigrationTools.Processors
{
    /// <summary>
    /// The TfsSharedQueryProcessor migrates shared queries and their folders from one project to another, keeping the team context of @CurrentIteration and @TeamAreas when both servers support the REST Queries API (Azure DevOps Services and TFS 2015 or later).
    /// </summary>
    /// <status>Beta</status>
    /// <processingtarget>Queries</processingtarget>
    public class TfsSharedQueryProcessor : Processor
    {

        private int _totalFoldersAttempted;
        private int _totalQueriesAttempted;
        private int _totalQueriesSkipped;
        private int _totalQueriesMigrated;
        private int _totalQueryFailed;
        private WorkItemTrackingHttpClient _sourceQueryClient;
        private WorkItemTrackingHttpClient _targetQueryClient;
        private bool? _restQueriesAvailable;
        private HashSet<string> _targetTeams;

        public TfsSharedQueryProcessor(IOptions<TfsSharedQueryProcessorOptions> options, CommonTools commonTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(options, commonTools, processorEnrichers, services, telemetry, logger)
        {
        }

        public new TfsSharedQueryProcessorOptions Options => (TfsSharedQueryProcessorOptions)base.Options;

        public new TfsEndpoint Source => (TfsEndpoint)base.Source;

        public new TfsEndpoint Target => (TfsEndpoint)base.Target;



        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////

            var sourceQueryHierarchy = Source.TfsProject.QueryHierarchy;
            var targetQueryHierarchy = Target.TfsProject.QueryHierarchy;

            Log.LogInformation("Found {0} root level child WIQ folders", sourceQueryHierarchy.Count);
            //////////////////////////////////////////////////

            foreach (QueryFolder query in sourceQueryHierarchy)
            {
                MigrateFolder(targetQueryHierarchy, query, targetQueryHierarchy);
            }

            stopwatch.Stop();
            Log.LogInformation("Folders scanned {totalFoldersAttempted}", _totalFoldersAttempted);
            Log.LogInformation("Queries Found:{totalQueriesAttempted}  Skipped:{totalQueriesSkipped}  Migrated:{totalQueriesMigrated}   Failed:{totalQueryFailed}", _totalQueriesAttempted, _totalQueriesSkipped, _totalQueriesMigrated, _totalQueryFailed);
            if (_totalQueryFailed > 0)
            {
                Log.LogWarning("{totalQueryFailed} queries could not be migrated. Check the errors above.", _totalQueryFailed);
            }
            Log.LogInformation("DONE in {Elapsed} seconds", stopwatch.Elapsed.ToString("c"));
        }

        /// <summary>
        /// Define Query Folders under the current parent
        /// </summary>
        /// <param name="targetHierarchy">The object that represents the whole of the target query tree</param>
        /// <param name="sourceFolder">The source folder in tree on source instance</param>
        /// <param name="parentFolder">The target folder in tree on target instance</param>
        private void MigrateFolder(QueryHierarchy targetHierarchy, QueryFolder sourceFolder, QueryFolder parentFolder)
        {
            // We only migrate non-private folders and their contents
            if (sourceFolder.IsPersonal)
            {
                Log.LogInformation("Found a personal folder {sourceFolderName}. Migration only available for shared Team Query folders", sourceFolder.Name);
                return;
            }

            _totalFoldersAttempted++;

            // we need to replace the team project name in folder names as it included in query paths
            var requiredPath = sourceFolder.Path.Replace($"{Source.Project}/", $"{Target.Project}/");

            // Is the project name to be used in the migration as an extra folder level?
            if (Options.PrefixProjectToNodes == true)
            {
                // we need to inject the team name as a folder in the structure
                requiredPath = requiredPath.Replace(Options.SharedFolderName, $"{Options.SharedFolderName}/{Source.Project}");

                // If on the root level we need to check that the extra folder has already been added
                if (sourceFolder.Path.Count(f => f == '/') == 1)
                {
                    var targetSharedFolderRoot = (QueryFolder)parentFolder[Options.SharedFolderName];
                    var extraFolder = (QueryFolder)targetSharedFolderRoot.FirstOrDefault(q => q.Path == requiredPath);
                    if (extraFolder == null)
                    {
                        // we are at the root level on the first pass and need to create the extra folder for the team name
                        Log.LogInformation("Adding a folder '{Project}'", Source.Project);
                        extraFolder = new QueryFolder(Source.Project);
                        targetSharedFolderRoot.Add(extraFolder);
                        targetHierarchy.Save(); // moved the save here a more immediate and relavent error message
                    }

                    // adjust the working folder to the newly added one
                    parentFolder = targetSharedFolderRoot;
                }
            }

            // check if there is a folder of the required name, using the path to make sure it is unique
            var targetFolder = (QueryFolder)parentFolder.FirstOrDefault(q => q.Path == requiredPath);
            if (targetFolder != null)
            {
                Log.LogInformation("Skipping folder '{sourceFolderName}' as already exists", sourceFolder.Name);
            }
            else
            {
                Log.LogInformation("Migrating a folder '{sourceFolderName}'", sourceFolder.Name);
                targetFolder = new QueryFolder(sourceFolder.Name);
                parentFolder.Add(targetFolder);
                targetHierarchy.Save(); // moved the save here a more immediate and relavent error message
            }

            // Process child items
            foreach (QueryItem subQuery in sourceFolder)
            {
                if (subQuery.GetType() == typeof(QueryFolder))
                {
                    MigrateFolder(targetHierarchy, (QueryFolder)subQuery, targetFolder);
                }
                else
                {
                    MigrateQuery(targetHierarchy, (QueryDefinition)subQuery, targetFolder);
                }
            }

        }

        /// <summary>
        /// Add Query Definition under a specific Query Folder.
        /// </summary>
        /// <param name="targetHierarchy">The object that represents the whole of the target query tree</param>
        /// <param name="query">Query Definition - Contains the Query Details</param>
        /// <param name="parentFolder">Parent Folder</param>
        private void MigrateQuery(QueryHierarchy targetHierarchy, QueryDefinition query, QueryFolder parentFolder)
        {
            if (parentFolder.FirstOrDefault(q => q.Name == query.Name) != null)
            {
                _totalQueriesSkipped++;
                Log.LogWarning("Skipping query '{queryName}' as already exists", query.Name);
                return;
            }
            _totalQueriesAttempted++;
            Log.LogInformation("Migrating query '{queryName}'", query.Name);

            // The object model drops the team context from macros such as @CurrentIteration('[Project]\Team') and
            // rejects @CurrentIteration +/- n, so the WIQL is read and written through the REST API when the servers
            // have it (TFS 2015+), and through the object model otherwise (#2705).
            if (!RestQueriesAvailable)
            {
                MigrateQueryWithObjectModel(targetHierarchy, query, parentFolder);
                return;
            }

            string sourceQueryText;
            try
            {
                sourceQueryText = SourceQueryClient.GetQueryAsync(Source.Project, query.Id.ToString(), QueryExpand.Wiql).GetAwaiter().GetResult().Wiql;
            }
            catch (Exception ex)
            {
                _totalQueryFailed++;
                Log.LogError(ex, "Error reading query '{queryName}' from the source through the REST API", query.Name);
                return;
            }

            var fixedQueryText = SharedQueryWiqlRewriter.Rewrite(sourceQueryText, Source.Project, Target.Project, Options.PrefixProjectToNodes, Options.SourceToTargetFieldMappings);
            fixedQueryText = MapTeams(query.Name, fixedQueryText);

            try
            {
                TargetQueryClient.CreateQueryAsync(new QueryHierarchyItem { Name = query.Name, Wiql = fixedQueryText }, Target.Project, parentFolder.Id.ToString()).GetAwaiter().GetResult();
                _totalQueriesMigrated++;
            }
            catch (Exception ex)
            {
                _totalQueryFailed++;
                Log.LogDebug("Source Query: '{sourceQueryText}'", sourceQueryText);
                Log.LogDebug("Target Query: '{fixedQueryText}'", fixedQueryText);
                Log.LogError(ex, "Error saving query '{queryName}', probably due to invalid area or iteration paths", query.Name);
            }
        }

        private void MigrateQueryWithObjectModel(QueryHierarchy targetHierarchy, QueryDefinition query, QueryFolder parentFolder)
        {
            var fixedQueryText = SharedQueryWiqlRewriter.Rewrite(query.QueryText, Source.Project, Target.Project, Options.PrefixProjectToNodes, Options.SourceToTargetFieldMappings);
            if (fixedQueryText.IndexOf("@CurrentIteration", StringComparison.OrdinalIgnoreCase) >= 0 || fixedQueryText.IndexOf("@TeamAreas", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Log.LogWarning("Query '{queryName}' uses a team-scoped macro. Without the REST Queries API the team context cannot be read, so the migrated query uses the default team.", query.Name);
            }

            // you cannot just add an item from one store to another, we need to create a new object
            parentFolder.Add(new QueryDefinition(query.Name, fixedQueryText));
            try
            {
                targetHierarchy.Save(); // moved the save here for better error message
                _totalQueriesMigrated++;
            }
            catch (Exception ex)
            {
                _totalQueryFailed++;
                Log.LogDebug("Source Query: '{query}'", query);
                Log.LogDebug("Target Query: '{fixedQueryText}'", fixedQueryText);
                Log.LogError(ex, "Error saving query '{queryName}', probably due to invalid area or iteration paths", query.Name);
                targetHierarchy.Refresh(); // get the tree without the last edit
            }
        }

        private string MapTeams(string queryName, string queryText)
        {
            var sourceDefaultTeam = $"{Source.Project} Team";
            var targetDefaultTeam = $"{Target.Project} Team";
            foreach (var team in SharedQueryWiqlRewriter.GetReferencedTeams(queryText, Target.Project).ToList())
            {
                if (string.Equals(team, sourceDefaultTeam, StringComparison.OrdinalIgnoreCase) && !string.Equals(sourceDefaultTeam, targetDefaultTeam, StringComparison.OrdinalIgnoreCase) && TargetTeamExists(targetDefaultTeam) == true)
                {
                    // the default team is named after the project, so it changes name with it
                    Log.LogInformation("Query '{queryName}' refers to the source default team '{team}'; using the target default team '{targetDefaultTeam}'", queryName, team, targetDefaultTeam);
                    queryText = SharedQueryWiqlRewriter.ReplaceTeam(queryText, Target.Project, team, targetDefaultTeam);
                }
                else if (TargetTeamExists(team) == false)
                {
                    Log.LogWarning("Query '{queryName}' refers to team '{team}', which does not exist in target project '{targetProject}'. The query is migrated but will not return results until the team is created, for example with TfsTeamSettingsProcessor.", queryName, team, Target.Project);
                }
            }
            return queryText;
        }

        private bool RestQueriesAvailable
        {
            get
            {
                if (_restQueriesAvailable == null)
                {
                    try
                    {
                        SourceQueryClient.GetQueriesAsync(Source.Project, depth: 0).GetAwaiter().GetResult();
                        TargetQueryClient.GetQueriesAsync(Target.Project, depth: 0).GetAwaiter().GetResult();
                        _restQueriesAvailable = true;
                    }
                    catch (Exception ex)
                    {
                        Log.LogWarning(ex, "The REST Queries API is not available (it needs TFS 2015 or later). Queries are migrated with the object model, which cannot keep the team context of @CurrentIteration or @TeamAreas.");
                        _restQueriesAvailable = false;
                    }
                }
                return _restQueriesAvailable.Value;
            }
        }

        private WorkItemTrackingHttpClient SourceQueryClient => _sourceQueryClient ??= Source.TfsCollection.GetClient<WorkItemTrackingHttpClient>();

        private WorkItemTrackingHttpClient TargetQueryClient => _targetQueryClient ??= Target.TfsCollection.GetClient<WorkItemTrackingHttpClient>();

        /// <summary>
        /// Whether the team exists in the target project, or null when the teams could not be read.
        /// </summary>
        private bool? TargetTeamExists(string team)
        {
            if (_targetTeams == null)
            {
                try
                {
                    var teamClient = Target.TfsCollection.GetClient<TeamHttpClient>();
                    var teams = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    const int pageSize = 100;
                    for (var skip = 0; ; skip += pageSize)
                    {
                        var page = teamClient.GetTeamsAsync(Target.Project, top: pageSize, skip: skip).GetAwaiter().GetResult();
                        teams.UnionWith(page.Select(t => t.Name));
                        if (page.Count < pageSize) break;
                    }
                    _targetTeams = teams;
                }
                catch (Exception ex)
                {
                    // not cached, so a transient failure does not mark every team as missing for the rest of the run
                    Log.LogWarning(ex, "Could not read the teams of target project '{targetProject}' to check the teams that queries refer to", Target.Project);
                    return null;
                }
            }
            return _targetTeams.Contains(team);
        }
    }
}
