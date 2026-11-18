using MigrationTools.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using MigrationTools._EngineV1.DataContracts;
using MigrationTools.Clients;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Services;
using MigrationTools.Tools;
using Serilog.Context;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace MigrationTools.Processors
{
    /// <summary>
    /// WorkItemMigrationConfig is the main processor used to Migrate Work Items, Links, and Attachments.
    /// Use `WorkItemMigrationConfig` to configure.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Items</processingtarget>
    public class TfsWorkItemMigrationProcessor : TfsProcessor
    {
        private class ProgressTimer
        {
            private TimeSpan _totalProcessedTime = TimeSpan.Zero;

            public ProgressTimer(int totalCount)
            {
                TotalCount = totalCount;
            }

            public int TotalCount { get; private set; }
            public int ProcessedCount { get; private set; }
            public TimeSpan TotalProcessedTime => _totalProcessedTime;

            public void AddProcessedItem(TimeSpan processDuration, bool addToTotal)
            {
                if (addToTotal)
                {
                    TotalCount++;
                }
                ProcessedCount++;
                _totalProcessedTime += processDuration;
            }

            public TimeSpan AverageTime => ProcessedCount > 0
                ? TimeSpan.FromSeconds(TotalProcessedTime.TotalSeconds / ProcessedCount)
                : TotalProcessedTime;

            public TimeSpan RemainingTime => TimeSpan.FromTicks(AverageTime.Ticks * (TotalCount - ProcessedCount));
        }

        private static int _count = 0;
        private static int _current = 0;
        private static int _totalWorkItem = 0;
        private static string workItemLogTemplate = "[{sourceWorkItemTypeName,20}][Complete:{currentWorkItem,6}/{totalWorkItems}][sid:{sourceWorkItemId,6}|Rev:{sourceRevisionInt,3}][tid:{targetWorkItemId,6} | ";
        private List<string> _ignore;

        private ILogger contextLog;
        private ILogger workItemLog;
        private List<string> _itemsInError;

        public WorkItemMetrics workItemMetrics { get; private set; }

        public TfsWorkItemMigrationProcessor(
            IOptions<TfsWorkItemMigrationProcessorOptions> options,
            TfsCommonTools tfsCommonTools,
            ProcessorEnricherContainer processorEnrichers,
            IServiceProvider services,
            ITelemetryLogger telemetry,
            ILogger<TfsWorkItemMigrationProcessor> logger)
            : base(options, tfsCommonTools, processorEnrichers, services, telemetry, logger)
        {
            contextLog = Serilog.Log.ForContext<TfsWorkItemMigrationProcessor>();
            workItemMetrics = services.GetRequiredService<WorkItemMetrics>();
        }

        new TfsWorkItemMigrationProcessorOptions Options => (TfsWorkItemMigrationProcessorOptions)base.Options;

        new TfsTeamProjectEndpoint Source => (TfsTeamProjectEndpoint)base.Source;

        new TfsTeamProjectEndpoint Target => (TfsTeamProjectEndpoint)base.Target;

        internal void TraceWriteLine(LogEventLevel level, string message, Dictionary<string, object> properties = null)
        {
            if (properties != null)
            {
                foreach (var item in properties)
                {
                    workItemLog = workItemLog.ForContext(item.Key, item.Value);
                }
            }
            workItemLog.Write(level, workItemLogTemplate + message);
        }

        protected override void InternalExecute()
        {
            Log.LogDebug("WorkItemMigrationContext::InternalExecute ");
            if (Options == null)
            {
                throw new Exception("You must call Configure() first");
            }
            //////////////////////////////////////////////////
            ValidatePatTokenRequirement();
            //////////////////////////////////////////////////

            ValidateWorkItemTypes();

            WarnOnCurrentUserRules();

            ValidateStateStampedFieldsCanBeSet();

            ValidateProject();

            CommonTools.NodeStructure.ProcessorExecutionBegin(this);
            if (CommonTools.TeamSettings.Enabled)
            {
                CommonTools.TeamSettings.ProcessorExecutionBegin(this);
            }
            else
            {
                Log.LogWarning("WorkItemMigrationContext::InternalExecute: teamSettingsEnricher is disabled!");
            }

            _itemsInError = new List<string>();

            try
            {
                PopulateIgnoreList();

                // Inform the user that he maybe has to be patient now
                contextLog.Information("Querying items to be migrated: {SourceQuery} ...", Options.WIQLQuery);
                var sourceWorkItems = Source.WorkItems.GetWorkItems(Options.WIQLQuery);
                contextLog.Information("Replay all revisions of {sourceWorkItemsCount} work items?",
                    sourceWorkItems.Count);

                //////////////////////////////////////////////////
                CommonTools.NodeStructure.ValidateAllNodesExistOrAreMapped(this, sourceWorkItems, Source.WorkItems.Project.Name, Target.WorkItems.Project.Name);
                ValidateAllUsersExistOrAreMapped(sourceWorkItems);
                //////////////////////////////////////////////////

                contextLog.Information("Found target project as {@destProject}", Target.WorkItems.Project.Name);

                //////////////////////////////////////////////////////////FilterCompletedByQuery

                if (Options.FilterWorkItemsThatAlreadyExistInTarget)
                {
                    // Martin's decision (#3270): items already in the target are not skipped, so a rerun checks every
                    // migrated revision against the source and reports any that were saved with wrong values.
                    Log.LogWarning("FilterWorkItemsThatAlreadyExistInTarget is set to true but is ignored: work items already in the target are checked against the source on every run, so wrongly migrated revisions are reported. Remove the option or set it to false.");
                }

                //////////////////////////////////////////////////

                sourceWorkItems = SharedItemsFirst(sourceWorkItems);

                // After filtering, so the scan only covers (and only stops for) items that will be migrated.
                ValidateRevisionHistoryFields(sourceWorkItems);

                _current = 1;
                _count = sourceWorkItems.Count;
                _totalWorkItem = sourceWorkItems.Count;
                ProgressTimer progressTimer = new ProgressTimer(_totalWorkItem);

                ProcessorActivity.SetTag("source_workitems_to_process", sourceWorkItems.Count);
                foreach (WorkItemData sourceWorkItemData in sourceWorkItems)
                {

                    var stopwatch = Stopwatch.StartNew();
                    var sourceWorkItem = TfsExtensions.ToWorkItem(sourceWorkItemData);
                    workItemLog = contextLog.ForContext("SourceWorkItemId", sourceWorkItem.Id);
                    using (LogContext.PushProperty("sourceWorkItemTypeName", sourceWorkItem.Type.Name))
                    using (LogContext.PushProperty("currentWorkItem", _current))
                    using (LogContext.PushProperty("totalWorkItems", _totalWorkItem))
                    using (LogContext.PushProperty("sourceWorkItemId", sourceWorkItem.Id))
                    using (LogContext.PushProperty("sourceRevisionInt", sourceWorkItem.Revision))
                    using (LogContext.PushProperty("targetWorkItemId", null))
                    {
                        try
                        {
                            ProcessWorkItemAsync(sourceWorkItemData, progressTimer, Options.WorkItemCreateRetryLimit).Wait();

                            stopwatch.Stop();
                            var processingTime = stopwatch.Elapsed.TotalMilliseconds;
                            workItemMetrics.WorkItemsProcessedCount.Add(1, new KeyValuePair<string, object?>("workItemType", sourceWorkItemData.Type));
                            workItemMetrics.ProcessingDuration.Record(processingTime, new KeyValuePair<string, object?>("workItemType", sourceWorkItemData.Type));
                            if (Options.PauseAfterEachWorkItem)
                            {
                                Console.WriteLine("Do you want to continue? (y/n)");
                                if (Console.ReadKey().Key != ConsoleKey.Y)
                                {
                                    workItemLog.Warning("USER ABORTED");
                                    break;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            // .Wait() wraps failures in AggregateException; a stop request must end the run, not count as an item failure.
                            var stop = e as MigrationRunStoppedException
                                ?? (e as AggregateException)?.Flatten().InnerExceptions.OfType<MigrationRunStoppedException>().FirstOrDefault();
                            if (stop != null)
                            {
                                ExceptionDispatchInfo.Capture(stop).Throw();
                            }
                            _itemsInError.Add(sourceWorkItem.Id.ToString());
                            var failure = (e as AggregateException)?.Flatten().InnerExceptions.OfType<WorkItemMigrationFailedException>().FirstOrDefault()
                                ?? e as WorkItemMigrationFailedException;
                            var message = failure?.Message ?? WorkItemMigrationFailedException.Describe(sourceWorkItem.Id.ToString(), null, null, sourceWorkItem.Type?.Name, e);
                            workItemLog.Error("{FailureMessage}", message);
                            Log.LogDebug(e, "Details for source work item {WorkItemId}", sourceWorkItem.Id);

                            if (Options.StopOnFirstError)
                            {
                                throw new MigrationRunStoppedException($"StopOnFirstError is true and source work item {sourceWorkItem.Id} failed: {message}", e);
                            }

                            if (ShouldAbortAfterFailure(Options.StopOnFirstError, Options.MaxGracefulFailures, _itemsInError.Count))
                            {
                                throw new MigrationRunStoppedException($"Too many errors: more than {Options.MaxGracefulFailures} errors occurred, aborting migration.");
                            }
                            workItemLog.Warning("Continuing with the remaining work items ({FailedCount} failed so far). Set StopOnFirstError to stop on the first failure.", _itemsInError.Count);
                        }
                    }
                }
            }
            finally
            {
                LogMissingTargetFieldSummary();
                CommonTools.ExportWorkItemMapping.SaveMappings();
                CommonTools.EmbededImages?.ProcessorExecutionEnd(this);
                CommonTools.GitRepository?.LogUnresolvedSummary();

                if (_itemsInError.Count > 0)
                {
                    CompletedWithErrors = true;
                    contextLog.Error("{FailedCount} work item(s) could not be migrated: {ItemIds}", _itemsInError.Count, string.Join(", ", _itemsInError));
                }

            }
        }

        public static bool ShouldAbortAfterFailure(bool stopOnFirstError, int maxGracefulFailures, int failureCount)
        {
            if (stopOnFirstError)
            {
                return failureCount > 0;
            }
            return maxGracefulFailures > 0 && failureCount > maxGracefulFailures;
        }

        private void ValidateProject()
        {
            Log.LogInformation("[ValidateProject]");
            Log.LogInformation("------------------------------------");
            Log.LogInformation("Source Project Check");
            Log.LogInformation("------------------------------------");
            var sourceProjectName = Source.Options.Project;
            var sourceProjects = Source.WorkItems.GetProjects();
            var foundSourceProject = sourceProjects.FirstOrDefault(p => p.Name.Equals(sourceProjectName, StringComparison.OrdinalIgnoreCase));
            if (foundSourceProject == null)
            {
                Log.LogError("Source project '{ProjectName}' not found!", sourceProjectName);
                Log.LogInformation("Available projects are: {@sourceProjects}", string.Join(", ", sourceProjects.Select(p => p.Name)));
                throw new MigrationRunStoppedException("Source project not found.");
            }
            else
            {
                Log.LogInformation("Found source project as {@sourceProject}", foundSourceProject.Name);
            }
            Log.LogInformation("------------------------------------");
            Log.LogInformation("Target Project Check");
            Log.LogInformation("------------------------------------");
            var targetProjectName = Target.Options.Project;
            var targetProjects = Target.WorkItems.GetProjects();
            var foundTargetProject = targetProjects.FirstOrDefault(p => p.Name.Equals(targetProjectName, StringComparison.OrdinalIgnoreCase));
            if (foundTargetProject == null)
            {
                Log.LogError("Target project '{ProjectName}' not found!", targetProjectName);
                Log.LogInformation("Available projects are: {@targetProjects}", string.Join(", ", targetProjects.Select(p => p.Name)));
                throw new MigrationRunStoppedException("Target project not found.");
            }
            else
            {
                Log.LogInformation("Found target project as {@targetProject}", foundTargetProject.Name);
            }

            Log.LogInformation("[/ValidateProject]");
        }

        private void ValidateWorkItemTypes()
        {
            Log.LogInformation("[ValidateWorkItemTypes]");
            Log.LogInformation("------------------------------------");
            Log.LogInformation("Starting Pre-Validation: Validating work item types and fields with `WorkItemTypeValidatorTool`.");
            Log.LogInformation("Refer to https://devopsmigration.io/TfsWorkItemTypeValidatorTool/ for configuration.");
            Log.LogInformation("------------------------------------");

            var sourceWits = Source.WorkItems.Project
                .ToProject()
                .WorkItemTypes
                .Cast<WorkItemType>()
                .OrderBy(wit => wit.Name)
                .ToList();
            var targetWits = Target.WorkItems.Project
                .ToProject()
                .WorkItemTypes
                .Cast<WorkItemType>()
                .OrderBy(wit => wit.Name)
                .ToList();

            // Reflected work item ID field is mandatory for migration, so it is validated even if the validator tool is disabled.
            bool containsReflectedWorkItemId = CommonTools.WorkItemTypeValidatorTool.ValidateReflectedWorkItemIdField(
                sourceWits, targetWits, Target.Options.ReflectedWorkItemIdField);
            bool validationResult = true;
            if (CommonTools.WorkItemTypeValidatorTool.Enabled)
            {
                validationResult = CommonTools.WorkItemTypeValidatorTool.ValidateWorkItemTypes(sourceWits, targetWits);
            }
            else
            {
                const string msg = $"Validation of work item types ({nameof(TfsWorkItemTypeValidatorTool)}) is disabled."
                    + " We suggest to enable this tool, so work item types in target system are properly validated."
                    + " If they are not validated, some data in source system may not be migrated,"
                    + " or migration may not work at all.";
                Log.LogWarning(msg);
            }
            if (!containsReflectedWorkItemId || !validationResult)
            {
                Log.LogError("Validation of work item types failed.");
                throw new MigrationRunStoppedException("Validation of work item types failed.");
            }
            Log.LogInformation("------------------------------------");
            Log.LogInformation("[/ValidateWorkItemTypes]");
        }

        // When the scan is off, the run-time MissingTargetFieldTracker summary still reports these fields (#3194).
        private void ValidateRevisionHistoryFields(List<WorkItemData> sourceWorkItems)
        {
            var validator = CommonTools.WorkItemTypeValidatorTool;
            if (!validator.Enabled || !validator.ScanRevisionHistoryForFields)
            {
                return;
            }
            Log.LogInformation("[ValidateRevisionHistoryFields]");
            var sourceWits = Source.WorkItems.Project.ToProject().WorkItemTypes.Cast<WorkItemType>().ToList();
            var targetWits = Target.WorkItems.Project.ToProject().WorkItemTypes.Cast<WorkItemType>().ToList();
            bool isValid = validator.ValidateRevisionHistoryFields(sourceWorkItems, sourceWits, targetWits, IsWorkItemTypeUsableInTarget);
            if (!isValid && validator.StopIfHistoryOnlyFieldsFound)
            {
                Log.LogError("History-only fields were found and '{option}' is true, so the migration will stop now.",
                    nameof(TfsWorkItemTypeValidatorToolOptions.StopIfHistoryOnlyFieldsFound));
                throw new MigrationRunStoppedException("Validation of revision history fields failed.");
            }
            Log.LogInformation("[/ValidateRevisionHistoryFields]");
        }

        /// <summary>
        /// Warns about target process rules that copy the current user into an identity field. Azure DevOps applies them
        /// on every save, even with BypassRules, so replayed revisions would show the migration account (#2869).
        /// Only inherited processes are checked; any failure is logged and the migration continues.
        /// </summary>
        private void WarnOnCurrentUserRules()
        {
            var token = Target.Options.Authentication?.ResolveAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                Log.LogDebug("WarnOnCurrentUserRules: no target PAT, skipping the process rule check.");
                return;
            }
            try
            {
                var connection = new VssConnection(Target.Options.Collection, new VssBasicCredential(string.Empty, token));
                var project = connection.GetClient<Microsoft.TeamFoundation.Core.WebApi.ProjectHttpClient>()
                    .GetProject(Target.Options.Project).Result;
                var properties = connection.GetClient<Microsoft.TeamFoundation.Core.WebApi.ProjectHttpClient>()
                    .GetProjectPropertiesAsync(project.Id, new[] { "System.ProcessTemplateType" }).Result;
                var processIdValue = properties.FirstOrDefault()?.Value?.ToString();
                if (!Guid.TryParse(processIdValue, out var processId))
                {
                    Log.LogDebug("WarnOnCurrentUserRules: could not read the target process id, skipping.");
                    return;
                }
                var processClient = connection.GetClient<Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi.WorkItemTrackingProcessHttpClient>();
                var rules = new List<ProcessRuleInfo>();
                foreach (var wit in processClient.GetProcessWorkItemTypesAsync(processId).Result)
                {
                    foreach (var rule in processClient.GetProcessWorkItemTypeRulesAsync(processId, wit.ReferenceName).Result)
                    {
                        rules.Add(new ProcessRuleInfo
                        {
                            WorkItemType = wit.Name,
                            RuleName = rule.Name,
                            IsDisabled = rule.IsDisabled,
                            IsCustom = rule.CustomizationType != Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi.Models.CustomizationType.System,
                            Actions = (rule.Actions ?? Enumerable.Empty<Microsoft.TeamFoundation.WorkItemTracking.Process.WebApi.Models.RuleAction>())
                                .Select(a => new ProcessRuleActionInfo { ActionType = a.ActionType.ToString(), TargetField = a.TargetField })
                                .ToList()
                        });
                    }
                }
                var fieldDefinitions = ((TfsWorkItemMigrationClient)Target.WorkItems).Store.FieldDefinitions;
                var findings = CurrentUserRuleCheck.Find(rules,
                    field => fieldDefinitions.Contains(field) && fieldDefinitions[field].IsIdentity);
                Log.LogDebug("WarnOnCurrentUserRules: checked {RuleCount} rules in process {ProcessId}; {FindingCount} copy the current user into an identity field.", rules.Count, processId, findings.Count);
                foreach (var f in findings)
                {
                    Log.LogWarning("Target process rule '{RuleName}' on '{WorkItemType}' copies the current user into {Field}. Azure DevOps applies it on every save, even with BypassRules, so migrated revisions will show the migration account in that field. Disable the rule in the target process while migrating, then re-enable it.",
                        f.RuleName, f.WorkItemType, f.Field);
                }
            }
            catch (Exception ex)
            {
                Log.LogDebug(ex, "WarnOnCurrentUserRules: unable to read the target process rules; skipping the check.");
            }
        }

        /// <summary>
        /// Stops the run before anything is migrated when the target cannot take the source's state-stamped dates and
        /// By fields in the save that changes state (#3270). Without BypassRules the service stamps its own clock and the
        /// migration identity, so every activated, resolved or closed item would be saved with wrong values.
        /// </summary>
        private void ValidateStateStampedFieldsCanBeSet()
        {
            var store = ((TfsWorkItemMigrationClient)Target.WorkItems).Store;
            if (!store.BypassRules)
            {
                Log.LogError("The target connection does not bypass rules, so Activated/Resolved/Closed Date and By would be saved as the migration time and identity instead of the source values. Give the migration identity 'Bypass rules on work item updates' in the target project, then run again.");
                throw new MigrationRunStoppedException("Target does not bypass rules: state-stamped dates and By fields cannot keep their source values.");
            }
            if (!ObjectModelFieldWriter.IsAvailable)
            {
                Log.LogError("The TFS object model in use cannot set state-stamped dates in a revision that changes state, so they would be saved as the migration time.");
                throw new MigrationRunStoppedException("State-stamped dates cannot be set from the source with this object model.");
            }
        }

        private void ValidateAllUsersExistOrAreMapped(List<WorkItemData> sourceWorkItems)
        {
            IdentityMapResult usersToMap = null;
            if (CommonTools.UserMapping.Options.SkipValidateAllUsersExistOrAreMapped)
            {
                contextLog.Information("Skipped: Validating::Check that all users in the source exist in the target or are mapped!");
            }
            else
            {
                contextLog.Information("Validating::Check that all users in the source exist in the target or are mapped!");
                usersToMap = CommonTools.UserMapping.GetUsersInSourceMappedToTargetForWorkItems(this, sourceWorkItems);
                if (usersToMap != null && usersToMap.IdentityMap != null && usersToMap.IdentityMap.Count > 0)
                {
                    Log.LogWarning("Validating Failed! There are {usersToMap} users that exist in the source that do not exist "
                        + "in the target. This will not cause any errors, but may result in disconnected users that could have "
                        + "been mapped. Use the ExportUsersForMapping processor to create a list of mappable users.",
                        usersToMap.IdentityMap.Count);
                }
            }
            var userMapping = CommonTools.UserMapping;
            var referencedUsers = userMapping.Options.Enabled && userMapping.Options.ValidateMappingTargets == MappingTargetValidationScope.Referenced
                ? TfsUserMappingTool.GetUsersFromWorkItems(sourceWorkItems, userMapping.IdentityFieldsToCheck)
                : null;
            userMapping.ValidateMappingTargets(this, usersToMap?.TargetUsers, referencedUsers);
        }

        //private void ValidateAllNodesExistOrAreMapped(List<WorkItemData> sourceWorkItems)
        //{
        //    contextLog.Information("Validating::Check that all Area & Iteration paths from Source have a valid mapping on Target");
        //    if (!TfsCommonTools.NodeStructure.Options.Enabled && Target.Options.Project != Source.Config.AsTeamProjectConfig().Project)
        //    {
        //        Log.LogError("Source and Target projects have different names, but  NodeStructureEnricher is not enabled. Cant continue... please enable nodeStructureEnricher in the config and restart.");
        //        Environment.Exit(-1);
        //    }
        //    if ( TfsCommonTools.NodeStructure.Options.Enabled)
        //    {
        //        List<NodeStructureItem> nodeStructureMissingItems = TfsCommonTools.NodeStructure.GetMissingRevisionNodes(sourceWorkItems);
        //        if (TfsCommonTools.NodeStructure.ValidateTargetNodesExist(nodeStructureMissingItems))
        //        {
        //            Log.LogError("Missing Iterations in Target preventing progress, check log for list. To continue you MUST configure IterationMaps or AreaMaps that matches the missing paths..");
        //            Environment.Exit(-1);
        //        }
        //    } else
        //    {
        //        contextLog.Error("nodeStructureEnricher is disabled! Please enable it in the config.");
        //    }
        //}

        private void ValidatePatTokenRequirement()
        {
            string collUrl = Target.Options.Collection.ToString();
            if (collUrl.Contains("dev.azure.com") || collUrl.Contains(".visualstudio.com"))
            {
                var token = Target.Options.Authentication.ResolveAccessToken();
                // Test that
                if (token.IsNullOrEmpty())
                {
                    Log.LogError("When you are migrating to Azure DevOps you MUST provide an PAT so that we can call the REST API for certain actions. For example we would be unable to deal with a Work item Type change.");
                    throw new MigrationRunStoppedException("Missing PersonalAccessToken from Target.");
                }
            }
        }

        private static bool IsNumeric(string val, NumberStyles numberStyle)
        {
            double result;
            return double.TryParse(val, numberStyle,
                CultureInfo.CurrentCulture, out result);
        }

        private readonly Dictionary<string, bool> _targetTypeUsableCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// True when the type exists in the target project and is not disabled. Disabled state is read over REST
        /// (the object model does not expose it); if that lookup fails the type is assumed to be enabled.
        /// </summary>
        private bool IsWorkItemTypeUsableInTarget(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return false;
            if (_targetTypeUsableCache.TryGetValue(typeName, out var cached)) return cached;
            bool usable = Target.WorkItems.Project.ToProject().WorkItemTypes.Contains(typeName);
            bool lookupSucceeded = true;
            if (usable)
            {
                try
                {
                    string token = Target.Options.Authentication?.ResolveAccessToken();
                    if (!string.IsNullOrEmpty(token))
                    {
                        using (var connection = new VssConnection(Target.Options.Collection, new VssBasicCredential(string.Empty, token)))
                        using (var client = connection.GetClient<WorkItemTrackingHttpClient>())
                        {
                            var wit = client.GetWorkItemTypeAsync(Target.Options.Project, typeName).GetAwaiter().GetResult();
                            if (wit?.IsDisabled == true) usable = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Do not cache: a later work item can retry once REST recovers.
                    lookupSucceeded = false;
                    Log.LogWarning(ex, "Unable to determine whether work item type '{TypeName}' is disabled in the target; assuming it is enabled.", typeName);
                }
            }
            if (lookupSucceeded)
            {
                _targetTypeUsableCache[typeName] = usable;
            }
            return usable;
        }

        private WorkItemData CreateWorkItem_Shell(ProjectData destProject, WorkItemData currentRevisionWorkItem, string destType)
        {
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("CreateWorkItem_Shell", ActivityKind.Client))
            {
                activity?.SetTagsFromOptions(Options);
                activity?.SetTag("http.request.method", "GET");
                activity?.SetTag("migrationtools.client", "TfsObjectModel");

                WorkItem newwit;
                if (destProject.ToProject().WorkItemTypes.Contains(destType))
                {
                    newwit = destProject.ToProject().WorkItemTypes[destType].NewWorkItem();
                }
                else
                {
                    throw new Exception($"WARNING: Unable to find '{destType}' in the target project. Either the work item specific is from the source, or its being specified in the {nameof(WorkItemTypeMappingTool)} definition in your configuration file! ");
                }
                activity?.Stop();
                activity?.SetStatus(ActivityStatusCode.Ok);
                activity?.SetTag("http.response.status_code", "200");
                if (Target.Options.ProductVersion != Endpoints.TfsProductVersion.OnPremisesClassic)
                {
                    newwit.Fields["System.CreatedBy"].Value = currentRevisionWorkItem.ToWorkItem().Revisions[0].Fields["System.CreatedBy"].Value;
                    workItemLog.Debug("Setting 'System.CreatedBy'={SystemCreatedBy}", currentRevisionWorkItem.ToWorkItem().Revisions[0].Fields["System.CreatedBy"].Value);
                    newwit.Fields["System.CreatedDate"].Value = currentRevisionWorkItem.ToWorkItem().Revisions[0].Fields["System.CreatedDate"].Value;
                    workItemLog.Debug("Setting 'System.CreatedDate'={SystemCreatedDate}", currentRevisionWorkItem.ToWorkItem().Revisions[0].Fields["System.CreatedDate"].Value);
                }
                return newwit.AsWorkItemData();
            }
        }

        private static readonly string[] SharedItemTypes = { "Shared Steps", "Shared Parameter" };

        // Test Case steps reference Shared Steps and Shared Parameters by id, and those references are only remapped
        // when the target item already exists, so migrate them first; otherwise keep the query's order.
        internal static List<WorkItemData> SharedItemsFirst(List<WorkItemData> workItems)
        {
            return workItems
                .OrderBy(wi => SharedItemTypes.Contains(wi.Type, StringComparer.OrdinalIgnoreCase) ? 0 : 1)
                .ToList();
        }

        private void PopulateIgnoreList()
        {
            _ignore = new List<string>
            {
                "System.Rev",
                "System.AreaId",
                "System.IterationId",
                "System.Id",
                "System.Parent",
                "System.RevisedDate",
                "System.AuthorizedAs",
                "System.AttachedFileCount",
                "System.TeamProject",
                "System.NodeName",
                "System.RelatedLinkCount",
                "System.WorkItemType",
                "Microsoft.VSTS.Common.StateChangeDate",
                "System.ExternalLinkCount",
                "System.HyperLinkCount",
                "System.Watermark",
                "System.AuthorizedDate",
                "System.BoardColumn",
                "System.BoardColumnDone",
                "System.BoardLane",
                "SLB.SWT.DateOfClientFeedback",
                "System.CommentCount",
                "System.RemoteLinkCount"
            };
        }

        private readonly MissingTargetFieldTracker _missingTargetFields = new MissingTargetFieldTracker();

        /// <summary>
        /// Records that a source revision carried a value for a field that does not exist on the target work item type.
        /// The work item type validator only inspects the current source type definitions, so fields that only appear
        /// in historical revisions (e.g. items moved from another project or process) are not caught up front (#3173).
        /// The first occurrence is logged prominently and a summary is produced at the end of the run.
        /// </summary>
        private void RecordMissingTargetField(string sourceType, string targetType, string fieldReferenceName, string sourceWorkItemId, int sourceRevision)
        {
            // Field maps are applied later in the revision replay; a field they consume is not dropped.
            if (!MissingFieldPolicy.IsDropped(sourceType, fieldReferenceName))
            {
                return;
            }
            if (_missingTargetFields.Record(sourceType, targetType, fieldReferenceName, sourceWorkItemId, sourceRevision))
            {
                Log.LogWarning("Field '{MissingField}' has data in source revisions of type '{SourceType}' but does not exist on target type '{TargetType}'. "
                    + "The value will NOT be migrated. If the field is not on the current source type it only exists in work item history "
                    + "(e.g. the item was moved from another project/process), so it is not reported by TfsWorkItemTypeValidatorTool "
                    + "and field maps cannot read it. Add the field to the target type, or for a field on the source type configure a "
                    + "field map for the source type. A summary is logged at the end of the run.",
                    fieldReferenceName, sourceType, targetType);
            }
        }

        private MissingTargetFieldPolicy _missingFieldPolicy;

        // Created on first use, because the source endpoint is only connected once the processor runs.
        private MissingTargetFieldPolicy MissingFieldPolicy => _missingFieldPolicy ??= new MissingTargetFieldPolicy(
            sourceType =>
            {
                var witTypes = Source.WorkItems.Project.ToProject().WorkItemTypes;
                return witTypes.Contains(sourceType)
                    ? witTypes[sourceType].FieldDefinitions.Cast<FieldDefinition>().Select(fd => fd.ReferenceName)
                    : Enumerable.Empty<string>();
            },
            CommonTools.FieldMappingTool);

        private void LogMissingTargetFieldSummary()
        {
            if (_missingTargetFields.Count == 0)
            {
                return;
            }
            Log.LogWarning("{Count} source field(s) had values that were not migrated because the field does not exist on the target work item type "
                + "and no applicable field map handles them. These are not detected by work item type validation, which only checks current "
                + "type definitions and not fields present in revision history:", _missingTargetFields.Count);
            foreach (var line in _missingTargetFields.GetSummary())
            {
                Log.LogWarning("  Missing target field: {MissingFieldSummary}", line);
            }
        }

        // TODO : Make this into the Work Item mapping tool
        private void PopulateWorkItem(WorkItemData oldWorkItemData, WorkItemData newWorkItemData, string destType)
        {
            var oldWorkItem = oldWorkItemData.ToWorkItem();
            var newWorkItem = newWorkItemData.ToWorkItem();
            var fieldMappingTimer = Stopwatch.StartNew();

            if (newWorkItem.IsPartialOpen || !newWorkItem.IsOpen)
            {
                newWorkItem.Open();
            }

            newWorkItem.Title = oldWorkItem.Title;
            newWorkItem.State = oldWorkItem.State;
            try
            {
                if (newWorkItem.Fields.Contains("Microsoft.VSTS.Common.ClosedDate") && newWorkItem.Fields["Microsoft.VSTS.Common.ClosedDate"].IsEditable)
                {
                    newWorkItem.Fields["Microsoft.VSTS.Common.ClosedDate"].Value = oldWorkItem.Fields["Microsoft.VSTS.Common.ClosedDate"].Value;
                }
            }
            catch (FieldDefinitionNotExistException)
            {
                // Eat exception coz the TFS API Sucks
            }
            newWorkItem.Reason = oldWorkItem.Reason;

            foreach (Field f in oldWorkItem.Fields)
            {
                if (newWorkItem.Fields.Contains(f.ReferenceName) == false)
                {
                    var missedMigratedValue = oldWorkItem.Fields[f.ReferenceName].Value;
                    if (missedMigratedValue != null && !string.Empty.Equals(missedMigratedValue))
                    {
                        RecordMissingTargetField(oldWorkItem.Type.Name, destType, f.ReferenceName, oldWorkItemData.Id, oldWorkItem.Revision);
                        Log.LogWarning("PopulateWorkItem:FieldUpdate: Missing field in target workitem, Source WorkItemId: {WorkitemId}, Field: {MissingField}, Value: {SourceValue}", oldWorkItemData.Id, f.ReferenceName, missedMigratedValue);
                    }
                    continue;
                }
                // Fields pre-populated on the shell (e.g. System.CreatedBy) may be read-only below, so map them here.
                if (!_ignore.Contains(f.ReferenceName))
                {
                    CommonTools.UserMapping.MapUserIdentityField(newWorkItem.Fields[f.ReferenceName]);
                }
                var sourceFieldValue = CommonTools.UserMapping.MapUserIdentityValue(f.ReferenceName, oldWorkItem.Fields[f.ReferenceName].Value);
                if (!_ignore.Contains(f.ReferenceName) &&
                    (!newWorkItem.Fields[f.ReferenceName].IsChangedInRevision || newWorkItem.Fields[f.ReferenceName].IsEditable)
                    && sourceFieldValue != newWorkItem.Fields[f.ReferenceName].Value)
                {
                    Log.LogDebug("PopulateWorkItem:FieldUpdate: {ReferenceName} | Source:{OldReferenceValue} Target:{NewReferenceValue}", f.ReferenceName, sourceFieldValue, newWorkItem.Fields[f.ReferenceName].Value);

                    switch (f.FieldDefinition.FieldType)
                    {
                        case FieldType.String:
                            string oldValue = sourceFieldValue?.ToString();
                            string newValue = CommonTools.StringManipulator.ProcessString(oldValue);
                            newWorkItem.Fields[f.ReferenceName].Value = newValue;
                            break;
                        default:
                            newWorkItem.Fields[f.ReferenceName].Value = sourceFieldValue;
                            break;
                    }

                }
            }

            if (CommonTools.NodeStructure.Enabled)
            {

                newWorkItem.AreaPath = CommonTools.NodeStructure.GetNewNodeName(oldWorkItem.AreaPath, TfsNodeStructureType.Area);
                newWorkItem.IterationPath = CommonTools.NodeStructure.GetNewNodeName(oldWorkItem.IterationPath, TfsNodeStructureType.Iteration);
            }
            else
            {
                Log.LogWarning("WorkItemMigrationContext::PopulateWorkItem::nodeStructureEnricher::Disabled! This needs to be set to true!");
            }

            switch (destType)
            {
                case "Test Case":
                    try
                    {
                        newWorkItem.Fields["Microsoft.VSTS.TCM.Steps"].Value = oldWorkItem.Fields["Microsoft.VSTS.TCM.Steps"].Value;
                    }
                    catch (FieldDefinitionNotExistException ex)
                    {
                        Log.LogWarning($"Microsoft.VSTS.TCM.Steps does not exist on Source Work Item. This field will be skipped, but the all other fields on the revision will be populated. Exception details: {ex.Message}");
                    }
                    // The source revision may be an earlier, different type (shell fell back to the final type).
                    if (oldWorkItem.Fields.Contains("Microsoft.VSTS.Common.Priority"))
                    {
                        newWorkItem.Fields["Microsoft.VSTS.Common.Priority"].Value =
                            oldWorkItem.Fields["Microsoft.VSTS.Common.Priority"].Value;
                    }
                    break;
            }

            if (newWorkItem.Fields.Contains("Microsoft.VSTS.Common.BacklogPriority")
                && newWorkItem.Fields["Microsoft.VSTS.Common.BacklogPriority"].Value != null
                && !IsNumeric(newWorkItem.Fields["Microsoft.VSTS.Common.BacklogPriority"].Value.ToString(),
                    NumberStyles.Any))
                newWorkItem.Fields["Microsoft.VSTS.Common.BacklogPriority"].Value = 10;

            var description = new StringBuilder();
            description.Append(oldWorkItem.Description);
            newWorkItem.Description = description.ToString();
            fieldMappingTimer.Stop();
        }

        private void ProcessHTMLFieldAttachements(WorkItemData targetWorkItem)
        {
            if (targetWorkItem != null && Options.FixHtmlAttachmentLinks)
            {
                CommonTools.EmbededImages.FixEmbededImages(this, targetWorkItem);
            }
        }

        private void ProcessWorkItemEmbeddedLinks(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            if (sourceWorkItem != null && targetWorkItem != null && Options.FixHtmlAttachmentLinks)
            {
                CommonTools.WorkItemEmbededLink.Enrich(this, sourceWorkItem, targetWorkItem);
            }
        }

        private async Task ProcessWorkItemAsync(WorkItemData sourceWorkItem, ProgressTimer progressTimer, int retryLimit = 5, int retries = 0)
        {
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("ProcessWorkItemAsync", ActivityKind.Client))
            {
                activity?.SetTagsFromOptions(Options);
                activity?.SetTag("http.request.method", "GET");
                activity?.SetTag("migrationtools.client", "TfsObjectModel");
                activity?.SetTag("SourceURL", Source.Options.Collection.ToString());
                activity?.SetTag("SourceWorkItem", sourceWorkItem.Id);
                activity?.SetTag("TargetURL", Target.Options.Collection.ToString());
                activity?.SetTag("TargetProject", Target.WorkItems.Project.Name);
                activity?.SetTag("RetryLimit", retryLimit.ToString());
                activity?.SetTag("RetryNumber", retries.ToString());
                Log.LogDebug("######################################################################################");
                Log.LogDebug("ProcessWorkItem: {sourceWorkItemId}", sourceWorkItem.Id);
                Log.LogDebug("######################################################################################");
                try
                {
                    if (sourceWorkItem.Type != "Test Plan" && sourceWorkItem.Type != "Test Suite")
                    {
                        workItemMetrics.RevisionsPerWorkItem.Record(sourceWorkItem.Rev);
                        var targetWorkItem = Target.WorkItems.FindReflectedWorkItem(sourceWorkItem, false);
                        ///////////////////////////////////////////////
                        TraceWriteLine(LogEventLevel.Information, "Work Item has {sourceWorkItemRev} revisions and revision migration is set to {ReplayRevisions}",
                            new Dictionary<string, object>(){
                            { "sourceWorkItemRev", sourceWorkItem.Rev },
                            { "ReplayRevisions", CommonTools.RevisionManager.ReplayRevisions }}
                            );
                        List<RevisionItem> revisionsToMigrate =CommonTools.RevisionManager.GetRevisionsToMigrate(sourceWorkItem.Revisions.Values.ToList(), targetWorkItem?.Revisions.Values.ToList());
                        if (targetWorkItem == null)
                        {
                            targetWorkItem = ReplayRevisions(revisionsToMigrate, sourceWorkItem, null);
                            activity?.SetTag("Revisions", revisionsToMigrate.Count);

                        }
                        else
                        {
                            // A rerun never treats a wrongly saved revision as migrated (#3270).
                            VerifyMigratedRevisions(sourceWorkItem, targetWorkItem);
                            if (revisionsToMigrate.Count == 0)
                            {
                                ProcessWorkItemAttachments(sourceWorkItem, targetWorkItem, false);
                                ProcessWorkItemLinks(sourceWorkItem, targetWorkItem, true);
                                ProcessHTMLFieldAttachements(targetWorkItem);
                                ProcessWorkItemEmbeddedLinks(sourceWorkItem, targetWorkItem);
                                CommonTools.FieldMappingTool.ApplyFieldMappings(sourceWorkItem, targetWorkItem);
                                // A field map must not change the reflected id, or the next run cannot find this item (#2998).
                                SetReflectedWorkItemId(targetWorkItem, Source.WorkItems.CreateReflectedWorkItemId(sourceWorkItem).ToString());
                                StampCatchUpSave(targetWorkItem);
                                TraceWriteLine(LogEventLevel.Information, "Skipping as work item exists and no revisions to sync detected");
                                activity?.SetTag("Revisions", 0);
                            }
                            else
                            {
                                TraceWriteLine(LogEventLevel.Information, "Syncing as there are {revisionsToMigrateCount} revisions detected",
                                    new Dictionary<string, object>(){
                                    { "revisionsToMigrateCount", revisionsToMigrate.Count }
                                    });

                                targetWorkItem = ReplayRevisions(revisionsToMigrate, sourceWorkItem, targetWorkItem);
                            }
                        }
                        if (targetWorkItem != null && targetWorkItem.ToWorkItem().IsDirty)
                        {
                            // Only in a save that happens anyway: the migration hyperlink never causes a revision of its own.
                            if (Options.GenerateMigrationHyperlink)
                            {
                                EnsureMigrationHyperlink(sourceWorkItem, targetWorkItem, Source.WorkItems.CreateReflectedWorkItemId(sourceWorkItem).ToString());
                            }
                            targetWorkItem.SaveToAzureDevOps();
                        }
                        CommonTools.Attachment.CleanUpAfterSave();
                        if (targetWorkItem != null)
                        {
                            CommonTools.ExportWorkItemMapping.AddMapping(sourceWorkItem.Id, targetWorkItem.Id);                            targetWorkItem.ToWorkItem().Close();
                        }
                        if (sourceWorkItem != null)
                        {
                            sourceWorkItem.ToWorkItem().Close();
                        }
                    }
                    else
                    {
                        TraceWriteLine(LogEventLevel.Warning, "SKIP: Unable to migrate {sourceWorkItemTypeName}/{sourceWorkItemId}. Use the TestPlansAndSuitesMigrationContext after you have migrated all Test Cases. ",
                            new Dictionary<string, object>() {
                            {"sourceWorkItemTypeName", sourceWorkItem.Type },
                            {"sourceWorkItemId", sourceWorkItem.Id }
                            });
                    }
                }
                catch (WebException ex)
                {
                    Log.LogWarning("Network error while migrating source work item {SourceId}: {Reason}", sourceWorkItem?.Id, WorkItemMigrationFailedException.ServerReason(ex));
                    Log.LogDebug(ex, "Network error details for source work item {SourceId}", sourceWorkItem?.Id);
                    if (retries < retryLimit)
                    {
                        TraceWriteLine(LogEventLevel.Warning, "WebException: Will retry in {retrys}s ",
                            new Dictionary<string, object>() {
                            {"retrys", retries }
                            });
                        System.Threading.Thread.Sleep(new TimeSpan(0, 0, retries));
                        retries++;
                        TraceWriteLine(LogEventLevel.Warning, "RETRY {Retrys}/{RetryLimit} ",
                            new Dictionary<string, object>() {
                            {"Retrys", retries },
                            {"RetryLimit", retryLimit }
                            });
                        await ProcessWorkItemAsync(sourceWorkItem, progressTimer, retryLimit, retries);
                    }
                    else
                    {
                        TraceWriteLine(LogEventLevel.Error, "ERROR: Failed to create work item. Retry Limit reached ");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    activity?.Stop();
                    activity?.SetStatus(ActivityStatusCode.Error);
                    activity?.SetTag("http.response.status_code", "502");
                    Log.LogDebug(ex, "Migrating source work item {SourceId} failed.", sourceWorkItem?.Id);
                    Telemetry.TrackException(ex, activity.Tags);
                    throw;
                }
                activity?.Stop();
                progressTimer.AddProcessedItem(activity.Duration, retries > 0);
                TraceWriteLine(LogEventLevel.Information,
                    "Average time of {average:%s}.{average:%fff} seconds per work item and {remaining:%h} hours {remaining:%m} minutes {remaining:%s} seconds estimated to completion.",
                    new Dictionary<string, object>() {
                        { "average", progressTimer.AverageTime },
                        { "remaining", progressTimer.RemainingTime }
                    });

                activity?.SetStatus(ActivityStatusCode.Error);
                activity?.SetTag("http.response.status_code", "200");

                _current++;
                _count--;
            }
        }

        private void ProcessWorkItemAttachments(WorkItemData sourceWorkItem, WorkItemData targetWorkItem, bool save = true)
        {
            if (targetWorkItem != null && CommonTools.Attachment.Enabled && sourceWorkItem.ToWorkItem().Attachments.Count > 0)
            {
                TraceWriteLine(LogEventLevel.Information, "Attachemnts {SourceWorkItemAttachmentCount} | LinkMigrator:{AttachmentMigration}", new Dictionary<string, object>() { { "SourceWorkItemAttachmentCount", sourceWorkItem.ToWorkItem().Attachments.Count }, { "AttachmentMigration", CommonTools.Attachment.Enabled } });
                CommonTools.Attachment.ProcessAttachemnts(this, sourceWorkItem, targetWorkItem, save);
                //AddMetric("Attachments", processWorkItemMetrics, targetWorkItem.ToWorkItem().AttachedFileCount);
            }
        }

        /// <summary>
        /// Applies the attachment and link changes made in one source revision to the target before that revision is saved (#3192),
        /// so they are saved with the revision's ChangedBy/ChangedDate instead of in a later "Migration" save.
        /// </summary>
        /// <param name="previousSourceRevision">The source revision replayed before this one, or null if none has been replayed.</param>
        /// <param name="sourceRevision">The source work item opened at the revision being replayed.</param>
        /// <param name="targetWorkItem">The target work item the revision is replayed onto; not saved here.</param>
        /// <param name="isLastRevision">On the last revision all links are re-checked, to catch any skipped earlier.</param>
        private void ApplyRevisionAttachmentsAndLinks(WorkItemData previousSourceRevision, WorkItemData sourceRevision, WorkItemData targetWorkItem, bool isLastRevision)
        {
            var previousWi = previousSourceRevision?.ToWorkItem();
            var currentWi = sourceRevision.ToWorkItem();

            if (CommonTools.Attachment.Enabled)
            {
                var attachments = RevisionArtifactDiff.Compare(
                    previousWi?.Attachments.Cast<Attachment>(),
                    currentWi.Attachments.Cast<Attachment>(),
                    a => RevisionArtifactDiff.AttachmentKey(a.Id));
                if (attachments.Removed.Count > 0)
                {
                    CommonTools.Attachment.RemoveAttachments(attachments.Removed, targetWorkItem);
                }
                // On the last revision re-check all attachments (duplicate-safe), to catch any missing on a resumed target.
                if (attachments.Added.Count > 0 || (isLastRevision && currentWi.Attachments.Count > 0))
                {
                    ProcessWorkItemAttachments(sourceRevision, targetWorkItem, false);
                }
            }

            var links = RevisionArtifactDiff.Compare(
                previousWi?.Links.Cast<Link>(),
                currentWi.Links.Cast<Link>(),
                LinkKey);
            if (CommonTools.WorkItemLink.Enabled && links.Removed.Count > 0)
            {
                CommonTools.WorkItemLink.RemoveLinks(this, sourceRevision, links.Removed, targetWorkItem);
            }
            if (links.Added.Count > 0 || (isLastRevision && currentWi.Links.Count > 0))
            {
                ProcessWorkItemLinks(sourceRevision, targetWorkItem, true);
            }
        }

        /// <summary>
        /// When an already migrated item with no new source revisions still needs a save (e.g. a missing link),
        /// keep the last revision's author and date it just after it, instead of "Migration" now (#3192).
        /// </summary>
        private static void StampCatchUpSave(WorkItemData targetWorkItem)
        {
            var wi = targetWorkItem.ToWorkItem();
            if (!wi.IsDirty)
            {
                return;
            }
            var lastChangedBy = wi.Fields["System.ChangedBy"].OriginalValue?.ToString();
            var lastChangedDate = (DateTime)wi.Fields["System.ChangedDate"].OriginalValue;
            if (!string.IsNullOrEmpty(lastChangedBy))
            {
                wi.Fields["System.ChangedBy"].Value = lastChangedBy;
            }
            wi.Fields["System.ChangedDate"].Value = lastChangedDate.AddSeconds(1);
        }

        private void AppendMigrationComment(WorkItemData targetWorkItem, string reflectedUri)
        {
            var wi = targetWorkItem.ToWorkItem();
            wi.History = MigrationCommentHelper.AppendTo(wi.History, reflectedUri);
        }

        /// <summary>
        /// Keeps exactly one hyperlink from the target to the source work item's web page (GenerateMigrationHyperlink).
        /// Changes the work item in memory only; the caller saves it with a revision it is saving anyway.
        /// </summary>
        private void EnsureMigrationHyperlink(WorkItemData sourceWorkItem, WorkItemData targetWorkItem, string sourceUrl)
        {
            var wi = targetWorkItem.ToWorkItem();
            var hyperlinks = wi.Links.OfType<Hyperlink>().ToList();
            var sourceLocations = sourceWorkItem.ToWorkItem().Links.OfType<Hyperlink>().Select(h => h.Location);
            var plan = MigrationHyperlinkHelper.Plan(hyperlinks.Select(h => (h.Location, h.Comment)).ToList(), sourceLocations, sourceUrl);
            foreach (var index in plan.Remove)
            {
                Log.LogInformation("Replacing migration hyperlink '{OldUrl}' on target work item {Id} with '{NewUrl}'.", hyperlinks[index].Location, wi.Id, sourceUrl);
                wi.Links.Remove(hyperlinks[index]);
            }
            if (plan.Add)
            {
                wi.Links.Add(new Hyperlink(sourceUrl) { Comment = MigrationHyperlinkHelper.Comment });
            }
        }

        private readonly HashSet<string> _typesUsingFallbackField = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Writes the reflected work item ID to the configured field or, for a type that cannot be customised
        /// (<see cref="NonCustomisableWorkItemTypes"/>) and lacks it, to the fallback field.
        /// </summary>
        private void SetReflectedWorkItemId(WorkItemData targetWorkItem, string reflectedUri)
        {
            var wi = targetWorkItem.ToWorkItem();
            var field = NonCustomisableWorkItemTypes.ResolveField(wi.Type.Name, Target.Options.ReflectedWorkItemIdField, wi.Fields.Contains);
            if (field is null)
            {
                // The caller logs this as the work item's one-line failure message (#3275).
                throw new InvalidOperationException($"the target work item type '{wi.Type.Name}' has no field '{Target.Options.ReflectedWorkItemIdField}' to hold the reflected work item ID. Add the field to '{wi.Type.Name}' in the target process, or set ReflectedWorkItemIdField to a field that exists.");
            }
            if (field != Target.Options.ReflectedWorkItemIdField && _typesUsingFallbackField.Add(wi.Type.Name))
            {
                Log.LogInformation("Work item type '{WorkItemType}' cannot have field '{ReflectedWorkItemIdField}'; the reflected work item ID is stored in '{FallbackField}' instead.",
                    wi.Type.Name, Target.Options.ReflectedWorkItemIdField, field);
            }
            var existing = wi.Fields[field].Value?.ToString();
            if (!string.IsNullOrEmpty(existing) && existing != reflectedUri && field != Target.Options.ReflectedWorkItemIdField)
            {
                Log.LogWarning("Overwriting '{Field}' value '{Existing}' on target work item {Id} with the reflected work item ID.", field, existing, wi.Id);
            }
            wi.Fields[field].Value = reflectedUri;
        }

        private static string LinkKey(Link link)
        {
            switch (link)
            {
                case Hyperlink hl:
                    return RevisionArtifactDiff.HyperlinkKey(hl.Location);
                case ExternalLink el:
                    return RevisionArtifactDiff.ExternalLinkKey(el.LinkedArtifactUri);
                case RelatedLink rl:
                    return RevisionArtifactDiff.RelatedLinkKey(rl.LinkTypeEnd?.ImmutableName ?? rl.ArtifactLinkType?.Name, rl.RelatedWorkItemId);
                default:
                    return link.GetType().Name + "|" + link.ArtifactLinkType?.Name;
            }
        }

        private void ProcessWorkItemLinks(WorkItemData sourceWorkItem, WorkItemData targetWorkItem, bool inline = false)
        {
            if (targetWorkItem != null && CommonTools.WorkItemLink.Enabled && sourceWorkItem.ToWorkItem().Links.Count > 0)
            {
                TraceWriteLine(LogEventLevel.Information, "Links {SourceWorkItemLinkCount} | LinkMigrator:{LinkMigration}", new Dictionary<string, object>() { { "SourceWorkItemLinkCount", sourceWorkItem.ToWorkItem().Links.Count }, { "LinkMigration", CommonTools.WorkItemLink.Enabled } });
                CommonTools.WorkItemLink.Enrich(this, sourceWorkItem, targetWorkItem, inline);
                //AddMetric("RelatedLinkCount", processWorkItemMetrics, targetWorkItem.ToWorkItem().Links.Count);
                int fixedLinkCount = CommonTools.GitRepository.Enrich(this, sourceWorkItem, targetWorkItem, !inline);
                // AddMetric("FixedGitLinkCount", processWorkItemMetrics, fixedLinkCount);
            }
            else if (targetWorkItem != null && sourceWorkItem.ToWorkItem().Links.Count > 0 && sourceWorkItem.Type == "Test Case")
            {
                CommonTools.WorkItemLink.MigrateSharedSteps(this, sourceWorkItem, targetWorkItem);
                CommonTools.WorkItemLink.MigrateSharedParameters(this, sourceWorkItem, targetWorkItem);
            }
        }

        private WorkItemData ReplayRevisions(List<RevisionItem> revisionsToMigrate, WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            RevisionItem replaying = null;
            try
            {
                //If work item hasn't been created yet, create a shell
                if (targetWorkItem == null)
                {
                    var firstSourceType = revisionsToMigrate.First().Type;
                    var finalSourceType = revisionsToMigrate.Last().Type;

                    if (firstSourceType != finalSourceType)
                    {
                        TraceWriteLine(LogEventLevel.Information, $"WorkItem has changed type at one of the revisions, from {firstSourceType} to {finalSourceType}");
                    }

                    var selection = WorkItemShellTypeSelector.Select(firstSourceType, finalSourceType, CommonTools.WorkItemTypeMapping.Mappings, IsWorkItemTypeUsableInTarget);
                    if (selection.FellBackToFinalType)
                    {
                        Log.LogWarning("Source work item {SourceId} changed type from '{FirstType}' to '{FinalType}'. Target type '{FirstTargetType}' does not exist or is disabled in the target project, so the work item will be created as '{ShellType}' instead.",
                            sourceWorkItem.Id, firstSourceType, finalSourceType, selection.FirstType, selection.ShellType);
                    }
                    targetWorkItem = CreateWorkItem_Shell(Target.WorkItems.Project, sourceWorkItem, selection.ShellType);
                }

                if (Options.AttachRevisionHistory)
                {
                    CommonTools.RevisionManager.AttachSourceRevisionHistoryJsonToTarget(sourceWorkItem, targetWorkItem);
                }

                // Baseline for attachment/link changes: when resuming an existing target, the source revision before
                // the first one we replay. A new target has no baseline, so everything in its first replayed revision
                // is added, even when revisions were skipped (ReplayRevisions=false or MaxRevisions).
                WorkItemData previousSourceRevision = null;
                var previousRevisionNumber = RevisionArtifactDiff.BaselineRevisionNumber(
                    targetWorkItem.Id != "0", revisionsToMigrate.First().Number, sourceWorkItem.Revisions.Keys);
                if (previousRevisionNumber.HasValue)
                {
                    previousSourceRevision = sourceWorkItem.GetRevision(previousRevisionNumber.Value);
                }
                var lastRevision = revisionsToMigrate.Last();

                foreach (var revision in revisionsToMigrate)
                {
                    replaying = revision;
                    workItemMetrics.RevisionsProcessedCount.Add(1);
                    var currentRevisionWorkItem = sourceWorkItem.GetRevision(revision.Number);

                    TraceWriteLine(LogEventLevel.Information, " Processing Revision [{RevisionNumber}]",
                        new Dictionary<string, object>() {
                            {"RevisionNumber", revision.Number }
                        });

                    // Decide on WIT
                    var revisionType = WorkItemShellTypeSelector.MapType(currentRevisionWorkItem.Type, CommonTools.WorkItemTypeMapping.Mappings);
                    var destType = WorkItemShellTypeSelector.SelectRevisionType(currentRevisionWorkItem.Type, targetWorkItem.Type, CommonTools.WorkItemTypeMapping.Mappings, IsWorkItemTypeUsableInTarget);
                    if (destType != revisionType)
                    {
                        Log.LogWarning("Source work item {SourceId} revision {RevisionNumber} has type '{RevisionType}', which does not exist or is disabled in the target; keeping '{CurrentType}'.",
                            sourceWorkItem.Id, revision.Number, revisionType, targetWorkItem.Type);
                    }
                    bool typeChange = (destType != targetWorkItem.Type);

                    int workItemId = Int32.Parse(targetWorkItem.Id);

                    if (typeChange && workItemId > 0)
                    {
                        ValidatePatTokenRequirement();
                        Uri collectionUri = Target.Options.Collection;
                        string token = Target.Options.Authentication.ResolveAccessToken();
                        VssConnection connection = new VssConnection(collectionUri, new VssBasicCredential(string.Empty, token));
                        WorkItemTrackingHttpClient workItemTrackingClient = connection.GetClient<WorkItemTrackingHttpClient>();
                        JsonPatchDocument patchDocument = new JsonPatchDocument();
                        DateTime changedDate = ((DateTime)currentRevisionWorkItem.Fields["System.ChangedDate"].Value).AddMilliseconds(-3);

                        patchDocument.Add(
                            new JsonPatchOperation()
                            {
                                Operation = Operation.Add,
                                Path = "/fields/System.WorkItemType",
                                Value = destType
                            }
                        );
                        patchDocument.Add(
                            new JsonPatchOperation()
                            {
                                Operation = Operation.Add,
                                Path = "/fields/System.State",
                                Value = (string)currentRevisionWorkItem.Fields["System.State"].Value
                            }
                        );
                        patchDocument.Add(
                            new JsonPatchOperation()
                            {
                                Operation = Operation.Add,
                                Path = "/fields/System.Reason",
                                Value = (string)currentRevisionWorkItem.Fields["System.Reason"].Value
                            }
                        );
                        patchDocument.Add(
                            new JsonPatchOperation()
                            {
                                Operation = Operation.Add,
                                Path = "/fields/System.ChangedDate",
                                Value = changedDate
                            }
                        );
                        patchDocument.Add(
                        new JsonPatchOperation()
                        {
                            Operation = Operation.Add,
                            Path = "/fields/System.ChangedBy",
                            Value = currentRevisionWorkItem.Fields["System.ChangedBy"].Value.ToString()
                        }
                        );
                        // The type change commits the new state, so it must carry the source's state-stamped values too (#3270).
                        var typeChangeStamps = GetStateStampedTargetValues(currentRevisionWorkItem.ToWorkItem(), targetWorkItem.ToWorkItem());
                        foreach (var (field, value, _) in typeChangeStamps)
                        {
                            patchDocument.Add(new JsonPatchOperation()
                            {
                                Operation = Operation.Add,
                                Path = "/fields/" + field,
                                // An empty string clears the field; the client refuses a null value.
                                Value = value is DateTime d ? (object)ObjectModelFieldWriter.ToInternalDate(d) : value ?? ""
                            });
                        }
                        var result = workItemTrackingClient.UpdateWorkItemAsync(patchDocument, workItemId, bypassRules: true).Result;
                        targetWorkItem = Target.WorkItems.GetWorkItem(workItemId);
                        // Check the committed revision as the object model reads it, the same form as every other check:
                        // REST qualifies every identity, the object model only display names that several accounts share.
                        var typeChanged = targetWorkItem.ToWorkItem();
                        var committed = typeChanged.Revisions[typeChanged.Revisions.Count - 1];
                        VerifyStateStampedFieldsSaved(result.Rev ?? typeChanged.Rev,
                            f => committed.Fields.Contains(f) ? committed.Fields[f].Value : null, typeChangeStamps);
                    }
                    PopulateWorkItem(currentRevisionWorkItem, targetWorkItem, destType);

                    var fails = ((WorkItem)targetWorkItem.internalObject).Validate();
                    foreach (Field f in fails)
                    {
                        if (f.Name == "Reason")
                        {
                            if (f.AllowedValues.Count > 0)
                            {
                                targetWorkItem.ToWorkItem().Fields[f.Name].Value = f.AllowedValues[0];
                            }
                            else if (f.FieldDefinition.AllowedValues.Count > 0)
                            {
                                targetWorkItem.ToWorkItem().Fields[f.Name].Value = f.FieldDefinition.AllowedValues[0];
                            }
                        }
                    }
                    // Impersonate revision author. Mapping will apply later and may change this.
                    targetWorkItem.ToWorkItem().Fields["System.ChangedDate"].Value = revision.ChangedDate;
                    targetWorkItem.ToWorkItem().Fields["System.ChangedBy"].Value = revision.Fields["System.ChangedBy"].Value.ToString();
                    targetWorkItem.ToWorkItem().Fields["System.History"].Value = revision.Fields["System.History"].Value;

                    var reflectedUri = (TfsReflectedWorkItemId)Source.WorkItems.CreateReflectedWorkItemId(sourceWorkItem);
                    // Set the reflected id before the field maps so a TargetToTarget map can copy it to another
                    // field on the first revision (#2998), and again after so no map can overwrite it.
                    SetReflectedWorkItemId(targetWorkItem, reflectedUri.ToString());

                    // Todo: Ensure all field maps use WorkItemData.Fields to apply a correct mapping
                    CommonTools.FieldMappingTool.ApplyFieldMappings(currentRevisionWorkItem, targetWorkItem);

                    // Todo: Think about an "UpdateChangedBy" flag as this is expensive! (2s/WI instead of 1,5s when writing "Migration")

                    SetReflectedWorkItemId(targetWorkItem, reflectedUri.ToString());

                    ProcessHTMLFieldAttachements(targetWorkItem);
                    ProcessWorkItemEmbeddedLinks(sourceWorkItem, targetWorkItem);
                    // The last replayed revision is the source's latest, so use the work item's current attachments and
                    // links for it. The per-revision view can miss removals when the source history is back-dated (for
                    // example a source that was itself migrated), and this keeps the final state matching the source.
                    var artifactSource = revision == lastRevision ? sourceWorkItem : currentRevisionWorkItem;
                    ApplyRevisionAttachmentsAndLinks(previousSourceRevision, artifactSource, targetWorkItem, revision == lastRevision);
                    previousSourceRevision = currentRevisionWorkItem;

                    if (revision == lastRevision && Options.GenerateMigrationComment)
                    {
                        AppendMigrationComment(targetWorkItem, reflectedUri.ToString());
                    }
                    if (revision == lastRevision && Options.GenerateMigrationHyperlink)
                    {
                        EnsureMigrationHyperlink(sourceWorkItem, targetWorkItem, reflectedUri.ToString());
                    }

                    var skipIterationRevision = SkipRevisionWithInvalidIterationPath(targetWorkItem);
                    var skipAreaRevision = SkipRevisionWithInvalidAreaPath(targetWorkItem);

                    if (!skipIterationRevision && !skipAreaRevision)
                    {
                        // Last before the save, after field maps: the save that changes state must carry the source values (#3270).
                        var stampedFields = ApplyStateStampedFieldsFromSource(currentRevisionWorkItem, targetWorkItem);
                        targetWorkItem.SaveToAzureDevOps();
                        var saved = targetWorkItem.ToWorkItem();
                        VerifyStateStampedFieldsSaved(saved.Rev, f => saved.Fields[f].Value, stampedFields);
                        CommonTools.Attachment.CleanUpAfterSave();
                        if (CommonTools.WorkItemLink.Enabled && CommonTools.WorkItemLink.HasDeferredLinks)
                        {
                            // A child link skipped because this item was not saved yet; now it has an id, add it.
                            // The child's conflicting parent is removed by saving the child, as before this change.
                            ProcessWorkItemLinks(currentRevisionWorkItem, targetWorkItem, false);
                        }
                    }
                    TraceWriteLine(LogEventLevel.Information,
                        " Saved TargetWorkItem {TargetWorkItemId}. Replayed revision {RevisionNumber} of {RevisionsToMigrateCount}",
                       new Dictionary<string, object>() {
                               {"TargetWorkItemId", targetWorkItem.Id },
                               {"RevisionNumber", revision.Number },
                               {"RevisionsToMigrateCount",  revisionsToMigrate.Count}
                           });
                }

                // Attachments, links and the migration comment were saved with the replayed revisions (#3192),
                // so there is no extra save here with a ChangedBy/ChangedDate later than the source history.
                CommonTools.Attachment.CleanUpAfterSave();
                TraceWriteLine(LogEventLevel.Information, "...Saved as {TargetWorkItemId}", new Dictionary<string, object> { { "TargetWorkItemId", targetWorkItem.Id } });
            }
            catch (Exception ex)
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                if (targetWorkItem != null)
                {
                    foreach (Field f in targetWorkItem.ToWorkItem().Fields)
                        parameters.Add($"{f.ReferenceName} ({f.Name})", f.Value?.ToString());
                }
                Telemetry.TrackException(ex, parameters);
                // A failed save or check still removes the attachment files downloaded for this item.
                try { CommonTools.Attachment.CleanUpAfterSave(); }
                catch (Exception cleanUp) { Log.LogDebug(cleanUp, "Attachment clean-up after the failure of source work item {SourceId} failed.", sourceWorkItem.Id); }
                // The full exception and the field values are for diagnosis only (#3275); the clear message is logged by the caller.
                Log.LogDebug(ex, "Saving source work item {SourceId} failed. Target field values follow.", sourceWorkItem.Id);
                if (targetWorkItem != null)
                {
                    foreach (Field f in targetWorkItem.ToWorkItem().Fields)
                        Log.LogDebug("{FieldReferenceName} ({FieldName}) | {FieldValue}", f.ReferenceName, f.Name, f.Value);
                }
                // A run stop must still stop the run, and a network error must still reach the caller's retry, which
                // catches a WebException itself, so one inside an AggregateException (from .Result) is unwrapped.
                var passThrough = PassThroughReplay(ex);
                if (passThrough != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(passThrough).Throw();
                var type = replaying?.Type ?? sourceWorkItem.Type;
                throw new WorkItemMigrationFailedException(
                    WorkItemMigrationFailedException.Describe(sourceWorkItem.Id, replaying?.Number, targetWorkItem?.Id, type, ex), ex);
            }

            return targetWorkItem;
        }

        /// <summary>
        /// The exception the replay must rethrow unwrapped, or null: a run stop, which ends the run, or a network error,
        /// which <see cref="ProcessWorkItemAsync"/> retries; taken out of an <see cref="AggregateException"/> (from <c>.Result</c>).
        /// </summary>
        internal static Exception PassThroughReplay(Exception ex)
        {
            if (ex is MigrationRunStoppedException || ex is WebException) return ex;
            if (ex is AggregateException agg)
            {
                var inner = agg.Flatten().InnerExceptions;
                return (Exception)inner.OfType<MigrationRunStoppedException>().FirstOrDefault() ?? inner.OfType<WebException>().FirstOrDefault();
            }
            return null;
        }

        private static string GetClosedDateField(WorkItem workItem)
        {
            return StateStampedFields.ResolveClosedDateField(
                workItem.Fields.Contains(StateStampedFields.CommonClosedDate),
                workItem.Fields.Contains(StateStampedFields.SystemClosedDate));
        }

        /// <summary>
        /// The value each state-stamped field on the target must have after saving <paramref name="source"/>: the source
        /// value (By fields through the user mapping), or empty when the source has none. When the source work item type
        /// has no such field, a value a field map set in this revision is kept; otherwise the field must be empty, so the
        /// service cannot leave its own stamp there unnoticed (#3270).
        /// </summary>
        private List<(string Field, object Expected, bool IsIdentity)> GetStateStampedTargetValues(WorkItem source, WorkItem target)
        {
            var targetFields = new List<string>();
            var closedTarget = GetClosedDateField(target);
            if (closedTarget != null) targetFields.Add(closedTarget);
            targetFields.AddRange(StateStampedFields.Fields.Where(f => target.Fields.Contains(f)));

            var values = new List<(string Field, object Expected, bool IsIdentity)>();
            foreach (var targetField in targetFields)
            {
                var field = target.Fields[targetField];
                var isIdentity = field.FieldDefinition.IsIdentity;
                var sourceField = source.Fields.Contains(targetField) ? targetField
                    : targetField == closedTarget ? GetClosedDateField(source) : null;
                object value;
                if (sourceField == null)
                {
                    value = field.IsChangedByUser ? field.Value : null;
                    Log.LogDebug("State-stamped field {Field} is not on source type {SourceType}; the target keeps {Value} ({Origin}).",
                        targetField, source.Type.Name, value, field.IsChangedByUser ? "set by a field map" : "empty");
                }
                else
                {
                    value = source.Fields[sourceField].Value;
                    if (isIdentity) value = CommonTools.UserMapping.MapUserIdentityValue(targetField, value);
                }
                values.Add((targetField, StateStampedFields.IsEmpty(value) ? null : value, isIdentity));
            }
            return values;
        }

        /// <summary>
        /// Sets every state-stamped date and By field on the target to the value it must have (see
        /// <see cref="GetStateStampedTargetValues"/>), immediately before the save (#3270). In a revision that changes
        /// state the service would otherwise stamp the server clock and the migration identity, and nothing can correct
        /// them after the save. A field the object model has made read-only for the state change is set below its
        /// client-side check (<see cref="ObjectModelFieldWriter"/>); the service keeps the value because the store
        /// bypasses rules. Returns the values, checked again after the save. Throws, so the work item fails clearly, if a
        /// value cannot be set.
        /// </summary>
        private List<(string Field, object Expected, bool IsIdentity)> ApplyStateStampedFieldsFromSource(WorkItemData sourceRevision, WorkItemData targetWorkItem)
        {
            var source = sourceRevision.ToWorkItem();
            var target = targetWorkItem.ToWorkItem();
            var expected = GetStateStampedTargetValues(source, target);
            foreach (var (targetField, value, isIdentity) in expected)
            {
                var field = target.Fields[targetField];
                var before = field.Value;
                try
                {
                    if (field.IsEditable)
                    {
                        if (StateStampedFields.SameValue(value, field.Value, isIdentity)) continue;
                        try
                        {
                            field.Value = value;
                        }
                        catch (Exception ex) when (StateStampedFields.IsFieldCannotBeChangedError(ex))
                        {
                            ObjectModelFieldWriter.SetBelowClientRules(target, targetField, value);
                        }
                    }
                    else
                    {
                        // Read-only here means a state change made the object model leave the field to the service, which
                        // then stamps it even when it looks empty in memory. Setting it explicitly, empty included, stops that.
                        ObjectModelFieldWriter.SetBelowClientRules(target, targetField, value);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"the state-stamped field {targetField} could not be set to the source value '{value}' before the save: {WorkItemMigrationFailedException.ServerReason(ex)}", ex);
                }
                if (!StateStampedFields.SameValue(value, target.Fields[targetField].Value, isIdentity))
                {
                    throw new InvalidOperationException($"the state-stamped field {targetField} could not be set to the source value '{value}' before the save (the object model kept '{target.Fields[targetField].Value}'), so the revision was not saved.");
                }
                Log.LogDebug("State-stamped field {Field} on target #{TargetId} set to source [#{SourceId}][Rev{SourceRev}] value {Value} (was {Before})",
                    targetField, target.Id, source.Id, source.Rev, value, before);
            }
            return expected;
        }

        /// <summary>
        /// Checks the saved revision kept every state-stamped value (#3270). A mismatch means the service stamped its own
        /// value; the work item fails with the field, the expected and the saved value, rather than carrying on.
        /// </summary>
        /// <summary>
        /// On a rerun, compares every target revision with the source revision it replays (#3270): State, and every
        /// state-stamped date and By field (By through the user mapping). A target revision replays the source revision
        /// dated within 10 ms after it (the REST type-change save is dated 3 ms early) or, failing that, the latest source
        /// revision dated at or before it (a fix-up, a catch-up save, or a revision replayed with an adjusted date). A
        /// target revision dated before the first source revision is reported. A target migrated wrongly, by an earlier
        /// run or an earlier version, fails clearly here instead of being skipped as migrated. Fields the source type
        /// does not have are not compared: a field map may own them.
        /// </summary>
        private void VerifyMigratedRevisions(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            var source = sourceWorkItem.ToWorkItem();
            var target = targetWorkItem.ToWorkItem();
            // Source revisions are matched on the dates replay used: EnforceDatesMustBeIncreasing (already applied to
            // sourceWorkItem.Revisions by GetRevisionsToMigrate) moves revisions that share a ChangedDate 1 s apart.
            var replayDates = sourceWorkItem.Revisions.Values.ToDictionary(r => r.Index, r => r.ChangedDate.ToUniversalTime());
            DateTime When(Revision r) => r.WorkItem.Id == source.Id && replayDates.TryGetValue(r.Index, out var d)
                ? d : ((DateTime)r.Fields["System.ChangedDate"].Value).ToUniversalTime();
            var sourceRevisions = source.Revisions.Cast<Revision>().OrderBy(When).ToList();
            var fields = new List<string>();
            var closedTarget = GetClosedDateField(target);
            if (closedTarget != null) fields.Add(closedTarget);
            fields.AddRange(StateStampedFields.Fields.Where(f => target.Fields.Contains(f)));

            var wrong = new List<string>();
            foreach (Revision tr in target.Revisions)
            {
                var at = When(tr);
                var sr = sourceRevisions.FirstOrDefault(r => When(r) >= at && (When(r) - at).TotalMilliseconds <= 10)
                    ?? sourceRevisions.LastOrDefault(r => When(r) <= at);
                if (sr == null)
                {
                    wrong.Add($"revision {tr.Index + 1} (dated {at:o}) is earlier than every source revision");
                    continue;
                }
                var sourceState = sr.Fields["System.State"].Value as string;
                var targetState = tr.Fields["System.State"].Value as string;
                if (!string.Equals(sourceState, targetState, StringComparison.Ordinal))
                {
                    wrong.Add($"revision {tr.Index + 1} State is '{targetState}' but source revision {sr.Index + 1} is '{sourceState}'");
                }
                foreach (var field in fields)
                {
                    var sourceField = sr.Fields.Contains(field) ? field : field == closedTarget ? GetClosedDateField(source) : null;
                    if (sourceField == null || !sr.Fields.Contains(sourceField)) continue;
                    var isIdentity = target.Fields[field].FieldDefinition.IsIdentity;
                    var expected = sr.Fields[sourceField].Value;
                    if (isIdentity) expected = CommonTools.UserMapping.MapUserIdentityValue(field, expected);
                    var saved = tr.Fields.Contains(field) ? tr.Fields[field].Value : null;
                    if (!StateStampedFields.SameValue(StateStampedFields.IsEmpty(expected) ? null : expected, saved, isIdentity))
                    {
                        wrong.Add($"revision {tr.Index + 1} {field} is '{saved}' but source revision {sr.Index + 1} is '{expected}'");
                    }
                }
            }
            if (wrong.Count > 0)
            {
                throw new InvalidOperationException($"the already migrated target work item does not match the source, stamped instead of the source values: {string.Join("; ", wrong.Take(10))}{(wrong.Count > 10 ? $"; and {wrong.Count - 10} more" : "")}.");
            }
        }


        private static void VerifyStateStampedFieldsSaved(int revision, Func<string, object> savedValue, List<(string Field, object Expected, bool IsIdentity)> expected)
        {
            var wrong = expected.Where(e => !StateStampedFields.SameValue(e.Expected, savedValue(e.Field), e.IsIdentity))
                .Select(e => $"{e.Field} saved as '{savedValue(e.Field)}' but the source is '{e.Expected}'")
                .ToList();
            if (wrong.Count > 0)
            {
                throw new InvalidOperationException($"target revision {revision} was saved with values the service stamped instead of the source values: {string.Join("; ", wrong)}.");
            }

        }
        private bool SkipRevisionWithInvalidIterationPath(WorkItemData targetWorkItemData)
        {
            if (!Options.SkipRevisionWithInvalidIterationPath)
            {
                return false;
            }

            return ValidateRevisionField(targetWorkItemData, "System.IterationPath");
        }

        private bool SkipRevisionWithInvalidAreaPath(WorkItemData targetWorkItemData)
        {
            if (!Options.SkipRevisionWithInvalidAreaPath)
            {
                return false;
            }

            return ValidateRevisionField(targetWorkItemData, "System.AreaPath"); ;
        }

        private bool ValidateRevisionField(WorkItemData targetWorkItemData, string fieldReferenceName)
        {
            var workItem = targetWorkItemData.ToWorkItem();
            var invalidFields = workItem.Validate();

            if (invalidFields.Count == 0)
            {
                return false;
            }

            foreach (Field invalidField in invalidFields)
            {
                // We cannot save a revision when it has no IterationPath and/or AreaPath
                if (invalidField.ReferenceName == fieldReferenceName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
