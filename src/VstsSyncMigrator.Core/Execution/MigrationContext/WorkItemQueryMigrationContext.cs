using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MigrationTools.Core.Configuration.Processing;
using Microsoft.Extensions.Hosting;
using MigrationTools.Core.Configuration;
using MigrationTools;
using MigrationTools.Core.Engine.Processors;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemQueryMigrationContext : MigrationProcessorBase
    {
        /// <summary>
        /// Counter for folders processed
        /// </summary>
        private int totalFoldersAttempted = 0;

        /// <summary>
        /// Counter for queries skipped
        /// </summary>
        private int totalQueriesSkipped = 0;

        /// <summary>
        /// Counter for queries attempted
        /// </summary>
        private int totalQueriesAttempted = 0;

        /// <summary>
        /// Counter for queries successfully migrated
        /// </summary>
        private int totalQueriesMigrated = 0;

        /// <summary>
        /// Counter for the queries that failed migration
        /// </summary>
        private int totalQueryFailed = 0;

        /// <summary>
        /// The processor configuration
        /// </summary>
        private WorkItemQueryMigrationConfig config;

        public override string Name
        {
            get
            {
                return "WorkItemQueryMigrationProcessorContext";
            }
        }

        public WorkItemQueryMigrationContext(IServiceProvider services, ITelemetryLogger telemetry) : base(services, telemetry)
        {
        }

        public override void Configure(IProcessorConfig config)
        {
            this.config = (WorkItemQueryMigrationConfig)config;
        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
			//////////////////////////////////////////////////
			var sourceStore = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.None, Telemetry);
            var targetStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.None, Telemetry);

            var sourceQueryHierarchy = sourceStore.Store.Projects[me.Source.Config.Project].QueryHierarchy;
            var targetQueryHierarchy = targetStore.Store.Projects[me.Target.Config.Project].QueryHierarchy;

            Trace.WriteLine(string.Format("Found {0} root level child WIQ folders", sourceQueryHierarchy.Count));
            //////////////////////////////////////////////////

            foreach (QueryFolder query in sourceQueryHierarchy)
            {
                MigrateFolder(targetQueryHierarchy, query, targetQueryHierarchy);
            }

            stopwatch.Stop();
            Console.WriteLine($"Folders scanned {this.totalFoldersAttempted}");
            Console.WriteLine($"Queries Found:{this.totalQueriesAttempted}  Skipped:{this.totalQueriesSkipped}  Migrated:{this.totalQueriesMigrated}   Failed:{this.totalQueryFailed}");
            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
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
                Trace.WriteLine($"Found a personal folder {sourceFolder.Name}. Migration only available for shared Team Query folders");
            }
            else
            {
                this.totalFoldersAttempted++;

                // we need to replace the team project name in folder names as it included in query paths
                var requiredPath = sourceFolder.Path.Replace($"{me.Source.Config.Project}/", $"{me.Target.Config.Project}/");

                // Is the project name to be used in the migration as an extra folder level?
                if (config.PrefixProjectToNodes == true)
                {
                    // we need to inject the team name as a folder in the structure
                    requiredPath = requiredPath.Replace(config.SharedFolderName, $"{config.SharedFolderName}/{me.Source.Config.Project}");

                    // If on the root level we need to check that the extra folder has already been added
                    if (sourceFolder.Path.Count(f => f == '/') == 1)
                    {
                        var targetSharedFolderRoot = (QueryFolder)parentFolder[config.SharedFolderName];
                        QueryFolder extraFolder = (QueryFolder)targetSharedFolderRoot.FirstOrDefault(q => q.Path == requiredPath);
                        if (extraFolder == null)
                        {
                            // we are at the root level on the first pass and need to create the extra folder for the team name
                            Trace.WriteLine($"Adding a folder '{me.Source.Config.Project}'");
                            extraFolder = new QueryFolder(me.Source.Config.Project);
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
                    Trace.WriteLine($"Skipping folder '{sourceFolder.Name}' as already exists");
                }
                else
                {
                    Trace.WriteLine($"Migrating a folder '{sourceFolder.Name}'");
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
        void MigrateQuery(QueryHierarchy targetHierarchy, QueryDefinition query, QueryFolder parentFolder)
        {
            if (parentFolder.FirstOrDefault(q => q.Name == query.Name) != null)
            {
                this.totalQueriesSkipped++;
                Trace.WriteLine($"Skipping query '{query.Name}' as already exists");
            }
            else
            {
                // Sort out any path issues in the quertText
                var fixedQueryText = query.QueryText.Replace($"'{me.Source.Config.Project}", $"'{me.Target.Config.Project}"); // the ' should only items at the start of areapath etc.

                if (config.PrefixProjectToNodes)
                {
                    // we need to inject the team name as a folder in the structure too
                    fixedQueryText = fixedQueryText.Replace($"{me.Target.Config.Project}\\", $"{me.Target.Config.Project}\\{me.Source.Config.Project}\\");
                }

                if (config.SourceToTargetFieldMappings != null)
                {
                    foreach (var sourceField in config.SourceToTargetFieldMappings.Keys)
                    {
                        fixedQueryText = query.QueryText.Replace($"{sourceField}", $"'{config.SourceToTargetFieldMappings[sourceField]}");
                    }
                }

                // you cannot just add an item from one store to another, we need to create a new object
                var queryCopy = new QueryDefinition(query.Name, fixedQueryText);
                this.totalQueriesAttempted++;
                Trace.WriteLine($"Migrating query '{query.Name}'");
                parentFolder.Add(queryCopy);
                try
                {
                    targetHierarchy.Save(); // moved the save here for better error message
                    this.totalQueriesMigrated++;
                }
                catch (Exception ex)
                {
                    this.totalQueryFailed++;
                    Trace.WriteLine($"Error saving query '{query.Name}', probably due to invalid area or iteration paths");
                    Trace.WriteLine($"Source Query: '{query}'");
                    Trace.WriteLine($"Target Query: '{fixedQueryText}'");
                    Trace.WriteLine(ex.Message);
                    targetHierarchy.Refresh(); // get the tree without the last edit
                }
            }
        }

    }
}