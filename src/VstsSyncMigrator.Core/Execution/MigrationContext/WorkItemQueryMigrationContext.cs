using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools;
using MigrationTools._Enginev1.Processors;
using MigrationTools.Clients;
using MigrationTools.Configuration;
using MigrationTools.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemQueryMigrationContext : MigrationProcessorBase
    {
        /// <summary>
        /// The processor configuration
        /// </summary>
        private WorkItemQueryMigrationConfig config;

        /// <summary>
        /// Counter for folders processed
        /// </summary>
        private int totalFoldersAttempted = 0;

        /// <summary>
        /// Counter for queries attempted
        /// </summary>
        private int totalQueriesAttempted = 0;

        /// <summary>
        /// Counter for queries successfully migrated
        /// </summary>
        private int totalQueriesMigrated = 0;

        /// <summary>
        /// Counter for queries skipped
        /// </summary>
        private int totalQueriesSkipped = 0;

        /// <summary>
        /// Counter for the queries that failed migration
        /// </summary>
        private int totalQueryFailed = 0;

        public WorkItemQueryMigrationContext(IMigrationEngine engine, IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemQueryMigrationContext> logger) : base(engine, services, telemetry, logger)
        {
        }

        public override string Name
        {
            get
            {
                return "WorkItemQueryMigrationProcessorContext";
            }
        }

        public override void Configure(IProcessorConfig config)
        {
            this.config = (WorkItemQueryMigrationConfig)config;
        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////

            var sourceQueryHierarchy = ((TfsWorkItemMigrationClient)Engine.Source.WorkItems).Store.Projects[Engine.Source.Config.AsTeamProjectConfig().Project].QueryHierarchy;
            var targetQueryHierarchy = ((TfsWorkItemMigrationClient)Engine.Target.WorkItems).Store.Projects[Engine.Target.Config.AsTeamProjectConfig().Project].QueryHierarchy;

            Log.LogInformation("Found {0} root level child WIQ folders", sourceQueryHierarchy.Count);
            //////////////////////////////////////////////////

            foreach (QueryFolder query in sourceQueryHierarchy)
            {
                MigrateFolder(targetQueryHierarchy, query, targetQueryHierarchy);
            }

            stopwatch.Stop();
            Log.LogInformation("Folders scanned {totalFoldersAttempted}", totalFoldersAttempted);
            Log.LogInformation("Queries Found:{totalQueriesAttempted}  Skipped:{totalQueriesSkipped}  Migrated:{totalQueriesMigrated}   Failed:{totalQueryFailed}", totalQueriesAttempted, totalQueriesSkipped, totalQueriesMigrated, totalQueryFailed);
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
            }
            else
            {
                this.totalFoldersAttempted++;

                // we need to replace the team project name in folder names as it included in query paths
                var requiredPath = sourceFolder.Path.Replace($"{Engine.Source.Config.AsTeamProjectConfig().Project}/", $"{Engine.Target.Config.AsTeamProjectConfig().Project}/");

                // Is the project name to be used in the migration as an extra folder level?
                if (config.PrefixProjectToNodes == true)
                {
                    // we need to inject the team name as a folder in the structure
                    requiredPath = requiredPath.Replace(config.SharedFolderName, $"{config.SharedFolderName}/{Engine.Source.Config.AsTeamProjectConfig().Project}");

                    // If on the root level we need to check that the extra folder has already been added
                    if (sourceFolder.Path.Count(f => f == '/') == 1)
                    {
                        var targetSharedFolderRoot = (QueryFolder)parentFolder[config.SharedFolderName];
                        QueryFolder extraFolder = (QueryFolder)targetSharedFolderRoot.FirstOrDefault(q => q.Path == requiredPath);
                        if (extraFolder == null)
                        {
                            // we are at the root level on the first pass and need to create the extra folder for the team name
                            Log.LogInformation("Adding a folder '{Project}'", Engine.Source.Config.AsTeamProjectConfig().Project);
                            extraFolder = new QueryFolder(Engine.Source.Config.AsTeamProjectConfig().Project);
                            targetSharedFolderRoot.Add(extraFolder);
                            targetHierarchy.Save(); // moved the save here a more immediate and relavent error message
                        }

                        // adjust the working folder to the newly added one
                        parentFolder = targetSharedFolderRoot;
                    }
                }

                // check if there is a folder of the required name, using the path to make sure it is unique
                QueryFolder targetFolder = (QueryFolder)parentFolder.FirstOrDefault(q => q.Path == requiredPath);
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
                foreach (QueryItem sub_query in sourceFolder)
                {
                    if (sub_query.GetType() == typeof(QueryFolder))
                    {
                        MigrateFolder(targetHierarchy, (QueryFolder)sub_query, (QueryFolder)targetFolder);
                    }
                    else
                    {
                        MigrateQuery(targetHierarchy, (QueryDefinition)sub_query, (QueryFolder)targetFolder);
                    }
                }
            }
        }

        /// <summary>
        /// Add Query Definition under a specific Query Folder.
        /// </summary>
        /// <param name="targetHierarchy">The object that represents the whole of the target query tree</param>
        /// <param name="query">Query Definition - Contains the Query Details</param>
        /// <param name="QueryFolder">Parent Folder</param>
        private void MigrateQuery(QueryHierarchy targetHierarchy, QueryDefinition query, QueryFolder parentFolder)
        {
            if (parentFolder.FirstOrDefault(q => q.Name == query.Name) != null)
            {
                this.totalQueriesSkipped++;
                Log.LogWarning("Skipping query '{queryName}' as already exists", query.Name);
            }
            else
            {
                // Sort out any path issues in the quertText
                var fixedQueryText = query.QueryText.Replace($"'{Engine.Source.Config.AsTeamProjectConfig().Project}", $"'{Engine.Target.Config.AsTeamProjectConfig().Project}"); // the ' should only items at the start of areapath etc.

                if (config.PrefixProjectToNodes)
                {
                    // we need to inject the team name as a folder in the structure too
                    fixedQueryText = fixedQueryText.Replace($"{Engine.Target.Config.AsTeamProjectConfig().Project}\\", $"{Engine.Target.Config.AsTeamProjectConfig().Project}\\{Engine.Source.Config.AsTeamProjectConfig().Project}\\");
                }

                if (config.SourceToTargetFieldMappings != null)
                {
                    foreach (var sourceField in config.SourceToTargetFieldMappings.Keys)
                    {
                        fixedQueryText = query.QueryText.Replace(sourceField, config.SourceToTargetFieldMappings[sourceField]);
                    }
                }

                // you cannot just add an item from one store to another, we need to create a new object
                var queryCopy = new QueryDefinition(query.Name, fixedQueryText);
                this.totalQueriesAttempted++;
                Log.LogInformation("Migrating query '{queryName}'", query.Name);
                parentFolder.Add(queryCopy);
                try
                {
                    targetHierarchy.Save(); // moved the save here for better error message
                    this.totalQueriesMigrated++;
                }
                catch (Exception ex)
                {
                    this.totalQueryFailed++;
                    Log.LogDebug("Source Query: '{query}'");
                    Log.LogDebug("Target Query: '{fixedQueryText}'");
                    Log.LogError(ex, "Error saving query '{queryName}', probably due to invalid area or iteration paths", query.Name);
                    targetHierarchy.Refresh(); // get the tree without the last edit
                }
            }
        }
    }
}