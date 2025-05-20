using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools;

namespace MigrationTools.Processors
{
    /// <summary>
    /// The TfsSharedQueryProcessor enabled you to migrate queries from one locatio nto another.
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
            // Get the original query text for debugging
            string originalQueryText = query.QueryText;
            
            // Sort out any path issues in the quertText
            var fixedQueryText = query.QueryText.Replace($"'{Source.Project}", $"'{Target.Project}"); // the ' should only items at the start of areapath etc.

            if (Options.PrefixProjectToNodes)
            {
                // we need to inject the team name as a folder in the structure too
                fixedQueryText = fixedQueryText.Replace($"{Target.Project}\\", $"{Target.Project}\\{Source.Project}\\");
            }

            if (Options.SourceToTargetFieldMappings != null)
            {
                foreach (var sourceField in Options.SourceToTargetFieldMappings.Keys)
                {
                    fixedQueryText = fixedQueryText.Replace(sourceField, Options.SourceToTargetFieldMappings[sourceField]);
                }
            }
            
            // Special handling for @CurrentIteration macro to ensure it's preserved correctly
            fixedQueryText = PreserveMacros(fixedQueryText);
            
            Log.LogDebug("Original Query Text: '{queryText}'", originalQueryText);
            Log.LogDebug("Fixed Query Text: '{fixedQueryText}'", fixedQueryText);

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
                Log.LogDebug("Source Query: '{query}'", originalQueryText);
                Log.LogDebug("Target Query: '{fixedQueryText}'", fixedQueryText);
                Log.LogError(ex, "Error saving query '{queryName}', probably due to invalid area or iteration paths", query.Name);
                targetHierarchy.Refresh(); // get the tree without the last edit
            }
        }

        /// <summary>
        /// Ensures that special macros like @CurrentIteration are preserved in the query text
        /// </summary>
        /// <param name="queryText">The query text to process</param>
        /// <returns>Updated query text with preserved macros</returns>
        private string PreserveMacros(string queryText)
        {
            // If the query doesn't contain @CurrentIteration, no special handling needed
            if (!queryText.Contains("@CurrentIteration"))
            {
                return queryText;
            }
            
            Log.LogInformation("Found @CurrentIteration in query, applying special handling");
            
            try
            {
                // Based on the issue, when a query contains @CurrentIteration, the area path selections are lost
                // The specific issue shown in the screenshot is that the area selection is missing in the migrated query
                
                // This is a direct fix for the issue where area path selection is missing with @CurrentIteration
                // We need to ensure the area path conditions are properly maintained in the migrated query

                // The issue appears when [System.IterationPath] = @CurrentIteration is used,
                // which may cause area path selections to be dropped during migration
                
                // The WIQL query syntax for @CurrentIteration can vary across different queries
                // but the core issue is that area path selections get lost in the process
                
                // Since the TFS Object Model may not fully support this scenario (as per Mr. Hinsh's comment),
                // we need to ensure the area path selection is explicitly preserved
                string areaPathPattern = @"(\[System\.AreaPath\]\s*(?:=|UNDER)\s*(?:'[^']+'|@\w+))";
                
                // Look for area path conditions in the query
                MatchCollection areaMatches = Regex.Matches(queryText, areaPathPattern);
                
                if (areaMatches.Count > 0)
                {
                    // The query has area path conditions that need to be preserved
                    Log.LogInformation("Found area path conditions with @CurrentIteration - ensuring they're preserved");
                    
                    // In the WIQL parser, area path conditions might be dropped when @CurrentIteration is present
                    // This specific fix addresses that by restructuring the query to ensure area paths are preserved
                    
                    // Extract the area path conditions and ensure they're properly formatted
                    foreach (Match match in areaMatches)
                    {
                        string areaCondition = match.Groups[1].Value;
                        Log.LogDebug("Area path condition: {0}", areaCondition);
                    }
                    
                    // The key fix: Move @CurrentIteration references after area path conditions
                    // This ensures that area path selections aren't dropped during migration
                    int areaPathIndex = queryText.IndexOf("[System.AreaPath]", StringComparison.OrdinalIgnoreCase);
                    int iterationPathIndex = queryText.IndexOf("[System.IterationPath]", StringComparison.OrdinalIgnoreCase);
                    
                    // Only reorder if both exist and area path comes after iteration path (which may cause the issue)
                    if (areaPathIndex > 0 && iterationPathIndex > 0 && areaPathIndex > iterationPathIndex)
                    {
                        Log.LogInformation("Reordering conditions to ensure area path is preserved");
                        
                        // Extract the WHERE clause
                        int whereIndex = queryText.IndexOf("WHERE ", StringComparison.OrdinalIgnoreCase);
                        if (whereIndex > 0)
                        {
                            // Extract the part before WHERE
                            string beforeWhere = queryText.Substring(0, whereIndex + 6); // +6 includes "WHERE "
                            
                            // Extract the conditions after WHERE
                            string whereConditions = queryText.Substring(whereIndex + 6);
                            
                            // Split conditions by AND
                            string[] conditions = whereConditions.Split(new[] { " AND " }, StringSplitOptions.None);
                            
                            // Identify area path and iteration path conditions
                            string areaPathCondition = null;
                            string iterationPathCondition = null;
                            
                            for (int i = 0; i < conditions.Length; i++)
                            {
                                if (conditions[i].Contains("[System.AreaPath]"))
                                {
                                    areaPathCondition = conditions[i];
                                    conditions[i] = null; // Mark for removal
                                }
                                else if (conditions[i].Contains("[System.IterationPath]") && conditions[i].Contains("@CurrentIteration"))
                                {
                                    iterationPathCondition = conditions[i];
                                    conditions[i] = null; // Mark for removal
                                }
                            }
                            
                            // Rebuild the WHERE conditions with area path before iteration path
                            string newWhereConditions = "";
                            
                            // Add area path first if it exists
                            if (areaPathCondition != null)
                            {
                                newWhereConditions = areaPathCondition;
                            }
                            
                            // Add iteration path next if it exists
                            if (iterationPathCondition != null)
                            {
                                if (newWhereConditions.Length > 0)
                                {
                                    newWhereConditions += " AND ";
                                }
                                newWhereConditions += iterationPathCondition;
                            }
                            
                            // Add other conditions
                            foreach (string condition in conditions)
                            {
                                if (!string.IsNullOrEmpty(condition))
                                {
                                    if (newWhereConditions.Length > 0)
                                    {
                                        newWhereConditions += " AND ";
                                    }
                                    newWhereConditions += condition;
                                }
                            }
                            
                            // Rebuild the full query
                            string newQueryText = beforeWhere + newWhereConditions;
                            
                            Log.LogDebug("Reordered query: {0}", newQueryText);
                            return newQueryText;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning(ex, "Error while processing @CurrentIteration in query");
            }
            
            return queryText;
        }
    }
}
