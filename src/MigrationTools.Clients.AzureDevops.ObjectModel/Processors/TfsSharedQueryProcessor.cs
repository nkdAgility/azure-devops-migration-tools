using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    /// <summary>
    /// The TfsSharedQueryProcessor enabled you to migrate queries from one locatio nto another.
    /// </summary>
    public class TfsSharedQueryProcessor : Processor
    {
        private TfsSharedQueryProcessorOptions _options;
        private int _totalFoldersAttempted;
        private int _totalQueriesAttempted;
        private int _totalQueriesSkipped;
        private int _totalQueriesMigrated;
        private int _totalQueryFailed;

        public new TfsEndpoint Source => (TfsEndpoint)base.Source;

        public new TfsEndpoint Target => (TfsEndpoint)base.Target;

        public TfsSharedQueryProcessor(ProcessorEnricherContainer processorEnrichers,
                                       IEndpointFactory endpointFactory,
                                       IServiceProvider services,
                                       ITelemetryLogger telemetry,
                                       ILogger<Processor> logger)
            : base(processorEnrichers, endpointFactory, services, telemetry, logger)
        {
        }

        public override void Configure(IProcessorOptions options)
        {
            base.Configure(options);
            Log.LogInformation("TfsSharedQueryProcessor::Configure");
            _options = (TfsSharedQueryProcessorOptions)options;
        }

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
            if (_options.PrefixProjectToNodes == true)
            {
                // we need to inject the team name as a folder in the structure
                requiredPath = requiredPath.Replace(_options.SharedFolderName, $"{_options.SharedFolderName}/{Source.Project}");

                // If on the root level we need to check that the extra folder has already been added
                if (sourceFolder.Path.Count(f => f == '/') == 1)
                {
                    var targetSharedFolderRoot = (QueryFolder)parentFolder[_options.SharedFolderName];
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
            // Sort out any path issues in the quertText
            var fixedQueryText = query.QueryText.Replace($"'{Source.Project}", $"'{Target.Project}"); // the ' should only items at the start of areapath etc.

            if (_options.PrefixProjectToNodes)
            {
                // we need to inject the team name as a folder in the structure too
                fixedQueryText = fixedQueryText.Replace($"{Target.Project}\\", $"{Target.Project}\\{Source.Project}\\");
            }

            if (_options.SourceToTargetFieldMappings != null)
            {
                foreach (var sourceField in _options.SourceToTargetFieldMappings.Keys)
                {
                    fixedQueryText = fixedQueryText.Replace(sourceField, _options.SourceToTargetFieldMappings[sourceField]);
                }
            }

            // you cannot just add an item from one store to another, we need to create a new object
            var queryCopy = new QueryDefinition(query.Name, fixedQueryText);
            _totalQueriesAttempted++;
            Log.LogInformation("Migrating query '{queryName}'", query.Name);
            parentFolder.Add(queryCopy);
            try
            {
                targetHierarchy.Save(); // moved the save here for better error message
                _totalQueriesMigrated++;
            }
            catch (Exception ex)
            {
                _totalQueryFailed++;
                Log.LogDebug("Source Query: '{query}'");
                Log.LogDebug("Target Query: '{fixedQueryText}'");
                Log.LogError(ex, "Error saving query '{queryName}', probably due to invalid area or iteration paths", query.Name);
                targetHierarchy.Refresh(); // get the tree without the last edit
            }
        }
    }
}
