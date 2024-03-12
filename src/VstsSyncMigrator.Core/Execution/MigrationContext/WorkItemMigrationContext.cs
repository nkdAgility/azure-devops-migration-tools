using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.TestManagement;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Proxy;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using MigrationTools;
using MigrationTools._EngineV1.Clients;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools._EngineV1.Containers;
using MigrationTools._EngineV1.DataContracts;
using MigrationTools._EngineV1.Enrichers;
using MigrationTools._EngineV1.Processors;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.ProcessorEnrichers;
using MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers;
using Newtonsoft.Json.Linq;
using Serilog.Context;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace VstsSyncMigrator.Engine
{
    /// <summary>
    /// WorkItemMigrationConfig is the main processor used to Migrate Work Items, Links, and Attachments.
    /// Use `WorkItemMigrationConfig` to configure.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Items</processingtarget>
    public class WorkItemMigrationContext : MigrationProcessorBase
    {

        private static int _count = 0;
        private static int _current = 0;
        private static long _elapsedms = 0;
        private static int _totalWorkItem = 0;
        private static string workItemLogTemplate = "[{sourceWorkItemTypeName,20}][Complete:{currentWorkItem,6}/{totalWorkItems}][sid:{sourceWorkItemId,6}|Rev:{sourceRevisionInt,3}][tid:{targetWorkItemId,6} | ";
        private WorkItemMigrationConfig _config;
        private List<string> _ignore;

        private ILogger contextLog;
        private TfsAttachmentEnricher _attachmentEnricher;
        private IWorkItemProcessorEnricher embededImagesEnricher;
        private IWorkItemProcessorEnricher _workItemEmbededLinkEnricher;
        private StringManipulatorEnricher _stringManipulatorEnricher;
        private TfsUserMappingEnricher _userMappingEnricher;
        private TfsGitRepositoryEnricher gitRepositoryEnricher;
        private TfsNodeStructure _nodeStructureEnricher;
        private ITelemetryLogger _telemetry;
        private readonly EngineConfiguration _engineConfig;
        private TfsRevisionManager _revisionManager;
        private TfsValidateRequiredField _validateConfig;
        private IDictionary<string, double> processWorkItemMetrics = null;
        private IDictionary<string, string> processWorkItemParamiters = null;
        private TfsWorkItemLinkEnricher _workItemLinkEnricher;
        private ILogger workItemLog;
        private List<string> _itemsInError;

        public WorkItemMigrationContext(IMigrationEngine engine,
                                        IServiceProvider services,
                                        ITelemetryLogger telemetry,
                                        ILogger<WorkItemMigrationContext> logger,
                                        TfsUserMappingEnricher userMappingEnricher,
                                        TfsAttachmentEnricher attachmentEnricher,
                                        TfsNodeStructure nodeStructureEnricher,
                                        TfsRevisionManager revisionManager,
                                        TfsWorkItemLinkEnricher workItemLinkEnricher,
                                        StringManipulatorEnricher stringManipulatorEnricher,
                                        TfsWorkItemEmbededLinkEnricher workItemEmbeddedLinkEnricher,
                                        TfsValidateRequiredField requiredFieldValidator,
                                        IOptions<EngineConfiguration> engineConfig)
            : base(engine, services, telemetry, logger)
        {
            _telemetry = telemetry;
            _engineConfig = engineConfig.Value;
            contextLog = Serilog.Log.ForContext<WorkItemMigrationContext>();
            _attachmentEnricher = attachmentEnricher;
            _nodeStructureEnricher = nodeStructureEnricher;
            _userMappingEnricher = userMappingEnricher;
            _revisionManager = revisionManager;
            _workItemLinkEnricher = workItemLinkEnricher;
            _workItemEmbededLinkEnricher = workItemEmbeddedLinkEnricher;
            _stringManipulatorEnricher = stringManipulatorEnricher;
            _validateConfig = requiredFieldValidator;
        }

        public override string Name => "WorkItemMigration";

        public override void Configure(IProcessorConfig config)
        {
            _config = (WorkItemMigrationConfig)config;

            ImportCommonEnricherConfigs();
        }

        private void ImportCommonEnricherConfigs()
        {
            /// setup _engineConfig.CommonEnrichersConfig
            if (_engineConfig.CommonEnrichersConfig == null)
            {
                Log.LogError("CommonEnrichersConfig cant be Null! it must be a minimum of `[]`");
                Environment.Exit(-1);
            }
            PullCommonEnrichersConfig<TfsNodeStructure, TfsNodeStructureOptions>(_engineConfig.CommonEnrichersConfig, _nodeStructureEnricher);
            PullCommonEnrichersConfig<TfsRevisionManager, TfsRevisionManagerOptions>(_engineConfig.CommonEnrichersConfig, _revisionManager);
            PullCommonEnrichersConfig<TfsWorkItemLinkEnricher, TfsWorkItemLinkEnricherOptions>(_engineConfig.CommonEnrichersConfig, _workItemLinkEnricher);
            PullCommonEnrichersConfig<StringManipulatorEnricher, StringManipulatorEnricherOptions>(_engineConfig.CommonEnrichersConfig, _stringManipulatorEnricher);
            PullCommonEnrichersConfig<TfsAttachmentEnricher, TfsAttachmentEnricherOptions>(_engineConfig.CommonEnrichersConfig, _attachmentEnricher);
            PullCommonEnrichersConfig<TfsUserMappingEnricher, TfsUserMappingEnricherOptions>(_engineConfig.CommonEnrichersConfig, _userMappingEnricher);
        }

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
            if (_config == null)
            {
                throw new Exception("You must call Configure() first");
            }
            //////////////////////////////////////////////////
            ValidatePatTokenRequirement();
            //////////////////////////////////////////////////
     
            embededImagesEnricher = Services.GetRequiredService<TfsEmbededImagesEnricher>();
            gitRepositoryEnricher = Services.GetRequiredService<TfsGitRepositoryEnricher>();


            _nodeStructureEnricher.ProcessorExecutionBegin(null);

            var stopwatch = Stopwatch.StartNew();
            _itemsInError = new List<string>();

            try
            {


                PopulateIgnoreList();

                // Inform the user that he maybe has to be patient now
                contextLog.Information("Querying items to be migrated: {SourceQuery} ...", _config.WIQLQuery);
                var sourceWorkItems = Engine.Source.WorkItems.GetWorkItems(_config.WIQLQuery);
                contextLog.Information("Replay all revisions of {sourceWorkItemsCount} work items?",
                    sourceWorkItems.Count);

                //////////////////////////////////////////////////
                ValidateAllWorkItemTypesHaveReflectedWorkItemIdField(sourceWorkItems);
                ValiddateWorkItemTypesExistInTarget(sourceWorkItems);
                ValidateAllNodesExistOrAreMapped(sourceWorkItems);
                ValidateAllUsersExistOrAreMapped(sourceWorkItems);
                //////////////////////////////////////////////////

                contextLog.Information("Found target project as {@destProject}", Engine.Target.WorkItems.Project.Name);

                //////////////////////////////////////////////////////////FilterCompletedByQuery

                if (_config.FilterWorkItemsThatAlreadyExistInTarget)
                {
                    contextLog.Information(
                        "[FilterWorkItemsThatAlreadyExistInTarget] is enabled. Searching for work items that have already been migrated to the target...",
                        sourceWorkItems.Count());

                    string targetWIQLQuery = _nodeStructureEnricher.FixAreaPathAndIterationPathForTargetQuery(_config.WIQLQuery,
                        Engine.Source.WorkItems.Project.Name, Engine.Target.WorkItems.Project.Name, contextLog);

                    sourceWorkItems = ((TfsWorkItemMigrationClient)Engine.Target.WorkItems).FilterExistingWorkItems(
                        sourceWorkItems, _config.WIQLQuery,
                        (TfsWorkItemMigrationClient)Engine.Source.WorkItems);
                    contextLog.Information(
                        "!! After removing all found work items there are {SourceWorkItemCount} remaining to be migrated.",
                        sourceWorkItems.Count());
                }

                //////////////////////////////////////////////////


                _current = 1;
                _count = sourceWorkItems.Count;
                _elapsedms = 0;
                _totalWorkItem = sourceWorkItems.Count;
                foreach (WorkItemData sourceWorkItemData in sourceWorkItems)
                {

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
                            ProcessWorkItemAsync(sourceWorkItemData, _config.WorkItemCreateRetryLimit).Wait();
                            if (_config.PauseAfterEachWorkItem)
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
                            _itemsInError.Add(sourceWorkItem.Id.ToString());
                            workItemLog.Error(e, "Could not save migrated work item {WorkItemId}, an exception occurred.", sourceWorkItem.Id);

                            if (_config.MaxGracefulFailures == 0)
                            {
                                throw;
                            }

                            if (_itemsInError.Count > _config.MaxGracefulFailures)
                            {
                                throw new Exception($"Too many errors: more than {_config.MaxGracefulFailures} errors occurred, aborting migration.");
                            }
                        }
                    }
                }
            }
            finally
            {
                if (_config.FixHtmlAttachmentLinks)
                {
                    embededImagesEnricher?.ProcessorExecutionEnd(null);
                }

                stopwatch.Stop();

                if (_itemsInError.Count > 0)
                {
                    contextLog.Warning("The following items could not be migrated: {ItemIds}", string.Join(", ", _itemsInError));
                }
                contextLog.Information("DONE in {Elapsed}", stopwatch.Elapsed.ToString("c"));
            }
        }

        private void ValidateAllUsersExistOrAreMapped(List<WorkItemData> sourceWorkItems)
        {

            contextLog.Information("Validating::Check that all users in the source exist in the target or are mapped!");
            List<IdentityMapData> usersToMap = new List<IdentityMapData>();
            usersToMap = _userMappingEnricher.GetUsersInSourceMappedToTargetForWorkItems(sourceWorkItems);
            if (usersToMap.Count > 0)
            {
                Log.LogWarning("Validating Failed! There are {usersToMap} users that exist in the source that do not exist in the target. This will not cause any errors, but may result in disconnected users that could have been mapped. Use the ExportUsersForMapping processor to create a list of mappable users. Then Import using ", usersToMap.Count);
            }
           
        }

        private void ValidateAllNodesExistOrAreMapped(List<WorkItemData> sourceWorkItems)
        {
            contextLog.Information("Validating::Check that all Area & Iteration paths from Source have a valid mapping on Target");
            List<NodeStructureItem> nodeStructureMissingItems = _nodeStructureEnricher.GetMissingRevisionNodes(sourceWorkItems);
            if (_nodeStructureEnricher.ValidateTargetNodesExist(nodeStructureMissingItems))
            {
                throw new Exception("Missing Iterations in Target preventing progress, check log for list. To continue you MUST configure IterationMaps or AreaMaps that matches the missing paths..");
            }
        }

        private void ValidateAllWorkItemTypesHaveReflectedWorkItemIdField(List<WorkItemData> sourceWorkItems)
        {
            contextLog.Information("Validating::Check all Target Work Items have the RefectedWorkItemId field");

            var result = _validateConfig.ValidatingRequiredField(
                Engine.Target.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName, sourceWorkItems);
            if (!result)
            {
                var ex = new InvalidFieldValueException(
                    "Not all work items in scope contain a valid ReflectedWorkItemId Field!");
                Log.LogError(ex, "Not all work items in scope contain a valid ReflectedWorkItemId Field!");
                throw ex;
            }
        }

        private void ValiddateWorkItemTypesExistInTarget(List<WorkItemData> sourceWorkItems)
        {
            contextLog.Information("Validating::Check that all work item types needed in the Target exist or are mapped");
            // get list of all work item types
            List<String> sourceWorkItemTypes = sourceWorkItems.SelectMany(x => x.Revisions.Values)
            //.Where(x => x.Fields[fieldName].Value.ToString().Contains("\\"))
            .Select(x => x.Type)
            .Distinct()
            .ToList();

            Log.LogDebug("Validating::WorkItemTypes::sourceWorkItemTypes: {count} WorkItemTypes in the full source history {sourceWorkItemTypesString}", sourceWorkItemTypes.Count(), string.Join(",", sourceWorkItemTypes));

            var targetWorkItemTypes = Engine.Target.WorkItems.Project.ToProject().WorkItemTypes.Cast<WorkItemType>().Select(x => x.Name);
            Log.LogDebug("Validating::WorkItemTypes::targetWorkItemTypes::{count} WorkItemTypes in Target process: {targetWorkItemTypesString}", targetWorkItemTypes.Count(), string.Join(",", targetWorkItemTypes));

            var missingWorkItemTypes = sourceWorkItemTypes.Where(sourceWit => !targetWorkItemTypes.Contains(sourceWit)); // the real one
            if (missingWorkItemTypes.Count() > 0)
            {
                Log.LogWarning("Validating::WorkItemTypes::targetWorkItemTypes::There are {count} WorkItemTypes that are used in the history of the Source and that do not exist in the Target. These will all need mapped using `WorkItemTypeDefinition` in the config. ", missingWorkItemTypes.Count());

                bool allTypesMapped = true;
                foreach (var missingWorkItemType in missingWorkItemTypes)
                {
                    bool thisTypeMapped = true;
                    if (!Engine.TypeDefinitionMaps.Items.ContainsKey(missingWorkItemType))
                    {
                        thisTypeMapped = false;
                    }
                    Log.LogWarning("Validating::WorkItemTypes::targetWorkItemTypes::{missingWorkItemType}::Mapped? {thisTypeMapped}", missingWorkItemType, thisTypeMapped.ToString());
                    allTypesMapped &= thisTypeMapped;
                }
                if (!allTypesMapped)
                {
                    var ex = new Exception(
                       "Not all WorkItemTypes present in the Source are present in the Target or mapped! Filter them from the query, or map the to target types.");
                    Log.LogError(ex, "Not all WorkItemTypes present in the Source are present in the Target or mapped using `WorkItemTypeDefinition` in the config.");
                    throw ex;
                }
            }
        }

        private void ValidatePatTokenRequirement()
        {
            string collUrl = Engine.Target.Config.AsTeamProjectConfig().Collection.ToString();
            if (collUrl.Contains("dev.azure.com") || collUrl.Contains(".visualstudio.com"))
            {
                // Test that
                if (Engine.Target.Config.AsTeamProjectConfig().PersonalAccessToken.IsNullOrEmpty())
                {
                    var ex = new InvalidOperationException("Missing PersonalAccessToken from Target");
                    Log.LogError(ex, "When you are migrating to Azure DevOps you MUST provide an PAT so that we can call the REST API for certain actions. For example we would be unable to deal with a Work item Type change.");
                    throw ex;
                }
            }
        }

        private static bool IsNumeric(string val, NumberStyles numberStyle)
        {
            double result;
            return double.TryParse(val, numberStyle,
                CultureInfo.CurrentCulture, out result);
        }

        private WorkItemData CreateWorkItem_Shell(ProjectData destProject, WorkItemData currentRevisionWorkItem, string destType)
        {
            WorkItem newwit;
            var newWorkItemstartTime = DateTime.UtcNow;
            var newWorkItemTimer = Stopwatch.StartNew();
            if (destProject.ToProject().WorkItemTypes.Contains(destType))
            {
                newwit = destProject.ToProject().WorkItemTypes[destType].NewWorkItem();
            }
            else
            {
                throw new Exception(string.Format("WARNING: Unable to find '{0}' in the target project. Most likley this is due to a typo in the .json configuration under WorkItemTypeDefinition! ", destType));
            }
            newWorkItemTimer.Stop();
            Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", Engine.Target.Config.AsTeamProjectConfig().Collection.ToString(), "NewWorkItem", null, newWorkItemstartTime, newWorkItemTimer.Elapsed, "200", true));
            if (_config.UpdateCreatedBy)
            {
                newwit.Fields["System.CreatedBy"].Value = currentRevisionWorkItem.ToWorkItem().Revisions[0].Fields["System.CreatedBy"].Value;
                workItemLog.Debug("Setting 'System.CreatedBy'={SystemCreatedBy}", currentRevisionWorkItem.ToWorkItem().Revisions[0].Fields["System.CreatedBy"].Value);
            }
            if (_config.UpdateCreatedDate)
            {
                newwit.Fields["System.CreatedDate"].Value = currentRevisionWorkItem.ToWorkItem().Revisions[0].Fields["System.CreatedDate"].Value;
                workItemLog.Debug("Setting 'System.CreatedDate'={SystemCreatedDate}", currentRevisionWorkItem.ToWorkItem().Revisions[0].Fields["System.CreatedDate"].Value);
            }
            return newwit.AsWorkItemData();
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
            newWorkItem.Reason = oldWorkItem.Reason;

            foreach (Field f in oldWorkItem.Fields)
            {
                _userMappingEnricher.MapUserIdentityField(f);
                if (newWorkItem.Fields.Contains(f.ReferenceName) == false)
                {
                    var missedMigratedValue = oldWorkItem.Fields[f.ReferenceName].Value;
                    if (missedMigratedValue != null && !string.Empty.Equals(missedMigratedValue))
                    {
                        Log.LogWarning("PopulateWorkItem:FieldUpdate: Missing field in target workitem, Source WorkItemId: {WorkitemId}, Field: {MissingField}, Value: {SourceValue}", oldWorkItemData.Id, f.ReferenceName, missedMigratedValue);
                    }
                    continue;
                }
                if (!_ignore.Contains(f.ReferenceName) &&
                    (!newWorkItem.Fields[f.ReferenceName].IsChangedInRevision || newWorkItem.Fields[f.ReferenceName].IsEditable)
                    && oldWorkItem.Fields[f.ReferenceName].Value != newWorkItem.Fields[f.ReferenceName].Value)
                {
                    Log.LogDebug("PopulateWorkItem:FieldUpdate: {ReferenceName} | Source:{OldReferenceValue} Target:{NewReferenceValue}", f.ReferenceName, oldWorkItem.Fields[f.ReferenceName].Value, newWorkItem.Fields[f.ReferenceName].Value);

                    switch (f.FieldDefinition.FieldType)
                    {
                        case FieldType.String:
                            _stringManipulatorEnricher.ProcessorExecutionWithFieldItem(null, oldWorkItemData.Fields[f.ReferenceName]);
                            newWorkItem.Fields[f.ReferenceName].Value = oldWorkItemData.Fields[f.ReferenceName].Value;
                            break;
                        default:
                            newWorkItem.Fields[f.ReferenceName].Value = oldWorkItem.Fields[f.ReferenceName].Value;
                            break;
                    }

                } 
            }

            if (_nodeStructureEnricher.Options.Enabled)
            {
                newWorkItem.AreaPath = _nodeStructureEnricher.GetNewNodeName(oldWorkItem.AreaPath, TfsNodeStructureType.Area);
                newWorkItem.IterationPath = _nodeStructureEnricher.GetNewNodeName(oldWorkItem.IterationPath, TfsNodeStructureType.Iteration);
            }

            switch (destType)
            {
                case "Test Case":
                    newWorkItem.Fields["Microsoft.VSTS.TCM.Steps"].Value = oldWorkItem.Fields["Microsoft.VSTS.TCM.Steps"].Value;
                    newWorkItem.Fields["Microsoft.VSTS.Common.Priority"].Value =
                        oldWorkItem.Fields["Microsoft.VSTS.Common.Priority"].Value;
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
            if (targetWorkItem != null && _config.FixHtmlAttachmentLinks)
            {
                embededImagesEnricher.Enrich(null, targetWorkItem);
            }
        }

        private void ProcessWorkItemEmbeddedLinks(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            if (sourceWorkItem != null && targetWorkItem != null && _config.FixHtmlAttachmentLinks)
            {
                _workItemEmbededLinkEnricher.Enrich(sourceWorkItem, targetWorkItem);
            }
        }

        private async Task ProcessWorkItemAsync(WorkItemData sourceWorkItem, int retryLimit = 5, int retries = 0)
        {
            var witStopWatch = Stopwatch.StartNew();
            var startTime = DateTime.Now;
            processWorkItemMetrics = new Dictionary<string, double>();
            processWorkItemParamiters = new Dictionary<string, string>();
            AddParameter("SourceURL", processWorkItemParamiters, Engine.Source.WorkItems.Config.AsTeamProjectConfig().Collection.ToString());
            AddParameter("SourceWorkItem", processWorkItemParamiters, sourceWorkItem.Id);
            AddParameter("TargetURL", processWorkItemParamiters, Engine.Target.WorkItems.Config.AsTeamProjectConfig().Collection.ToString());
            AddParameter("TargetProject", processWorkItemParamiters, Engine.Target.WorkItems.Project.Name);
            AddParameter("RetryLimit", processWorkItemParamiters, retryLimit.ToString());
            AddParameter("RetryNumber", processWorkItemParamiters, retries.ToString());
            Log.LogDebug("######################################################################################");
            Log.LogDebug("ProcessWorkItem: {sourceWorkItemId}", sourceWorkItem.Id);
            Log.LogDebug("######################################################################################");
            try
            {
                if (sourceWorkItem.Type != "Test Plan" && sourceWorkItem.Type != "Test Suite")
                {
                    var targetWorkItem = Engine.Target.WorkItems.FindReflectedWorkItem(sourceWorkItem, false);
                    ///////////////////////////////////////////////
                    TraceWriteLine(LogEventLevel.Information, "Work Item has {sourceWorkItemRev} revisions and revision migration is set to {ReplayRevisions}",
                        new Dictionary<string, object>(){
                            { "sourceWorkItemRev", sourceWorkItem.Rev },
                            { "ReplayRevisions", _revisionManager.Options.ReplayRevisions }}
                        );
                    List<RevisionItem> revisionsToMigrate = _revisionManager.GetRevisionsToMigrate(sourceWorkItem, targetWorkItem);
                    if (targetWorkItem == null)
                    {
                        targetWorkItem = ReplayRevisions(revisionsToMigrate, sourceWorkItem, null);
                        AddMetric("Revisions", processWorkItemMetrics, revisionsToMigrate.Count);
                    }
                    else
                    {
                        if (revisionsToMigrate.Count == 0)
                        {
                            ProcessWorkItemAttachments(sourceWorkItem, targetWorkItem, false);
                            ProcessWorkItemLinks(Engine.Source.WorkItems, Engine.Target.WorkItems, sourceWorkItem, targetWorkItem);
                            ProcessHTMLFieldAttachements(targetWorkItem);
                            ProcessWorkItemEmbeddedLinks(sourceWorkItem, targetWorkItem);
                            TraceWriteLine(LogEventLevel.Information, "Skipping as work item exists and no revisions to sync detected");
                            processWorkItemMetrics.Add("Revisions", 0);
                        }
                        else
                        {
                            TraceWriteLine(LogEventLevel.Information, "Syncing as there are {revisionsToMigrateCount} revisions detected",
                                new Dictionary<string, object>(){
                                    { "revisionsToMigrateCount", revisionsToMigrate.Count }
                                });

                            targetWorkItem = ReplayRevisions(revisionsToMigrate, sourceWorkItem, targetWorkItem);

                            AddMetric("Revisions", processWorkItemMetrics, revisionsToMigrate.Count);
                            AddMetric("SyncRev", processWorkItemMetrics, revisionsToMigrate.Count);
                        }
                    }
                    AddParameter("TargetWorkItem", processWorkItemParamiters, targetWorkItem.ToWorkItem().Revisions.Count.ToString());

                    if (targetWorkItem != null && targetWorkItem.ToWorkItem().IsDirty)
                    {
                        targetWorkItem.SaveToAzureDevOps();
                    }
                    if (targetWorkItem != null)
                    {
                        targetWorkItem.ToWorkItem().Close();
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
                Log.LogError(ex, "Some kind of internet pipe blockage");
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
                    await ProcessWorkItemAsync(sourceWorkItem, retryLimit, retries);
                }
                else
                {
                    TraceWriteLine(LogEventLevel.Error, "ERROR: Failed to create work item. Retry Limit reached ");
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex, ex.ToString());
                Telemetry.TrackRequest("ProcessWorkItem", startTime, witStopWatch.Elapsed, "502", false);
                throw ex;
            }
            witStopWatch.Stop();
            _elapsedms += witStopWatch.ElapsedMilliseconds;
            processWorkItemMetrics.Add("ElapsedTimeMS", _elapsedms);

            var average = new TimeSpan(0, 0, 0, 0, (int)(_elapsedms / _current));
            var remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * _count));
            TraceWriteLine(LogEventLevel.Information,
                "Average time of {average:%s}.{average:%fff} per work item and {remaining:%h} hours {remaining:%m} minutes {remaining:%s}.{remaining:%fff} seconds estimated to completion",
                new Dictionary<string, object>() {
                    {"average", average},
                    {"remaining", remaining}
                });
            Telemetry.TrackEvent("WorkItemMigrated", processWorkItemParamiters, processWorkItemMetrics);
            Telemetry.TrackRequest("ProcessWorkItem", startTime, witStopWatch.Elapsed, "200", true);

            _current++;
            _count--;
        }

        private void ProcessWorkItemAttachments(WorkItemData sourceWorkItem, WorkItemData targetWorkItem, bool save = true)
        {
            if (targetWorkItem != null && _attachmentEnricher.Options.Enabled && sourceWorkItem.ToWorkItem().Attachments.Count > 0)
            {
                TraceWriteLine(LogEventLevel.Information, "Attachemnts {SourceWorkItemAttachmentCount} | LinkMigrator:{AttachmentMigration}", new Dictionary<string, object>() { { "SourceWorkItemAttachmentCount", sourceWorkItem.ToWorkItem().Attachments.Count }, { "AttachmentMigration", _attachmentEnricher.Options.Enabled } });
                _attachmentEnricher.ProcessAttachemnts(sourceWorkItem, targetWorkItem, save);
                AddMetric("Attachments", processWorkItemMetrics, targetWorkItem.ToWorkItem().AttachedFileCount);
            }
        }

        private void ProcessWorkItemLinks(IWorkItemMigrationClient sourceStore, IWorkItemMigrationClient targetStore, WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            if (targetWorkItem != null && _workItemLinkEnricher.Options.Enabled && sourceWorkItem.ToWorkItem().Links.Count > 0)
            {
                TraceWriteLine(LogEventLevel.Information, "Links {SourceWorkItemLinkCount} | LinkMigrator:{LinkMigration}", new Dictionary<string, object>() { { "SourceWorkItemLinkCount", sourceWorkItem.ToWorkItem().Links.Count }, { "LinkMigration", _workItemLinkEnricher.Options.Enabled } });
                _workItemLinkEnricher.Enrich(sourceWorkItem, targetWorkItem);
                AddMetric("RelatedLinkCount", processWorkItemMetrics, targetWorkItem.ToWorkItem().Links.Count);
                int fixedLinkCount = gitRepositoryEnricher.Enrich(sourceWorkItem, targetWorkItem);
                AddMetric("FixedGitLinkCount", processWorkItemMetrics, fixedLinkCount);
            }
            else if (targetWorkItem != null && sourceWorkItem.ToWorkItem().Links.Count > 0 && sourceWorkItem.Type == "Test Case" )
            {
                _workItemLinkEnricher.MigrateSharedSteps(sourceWorkItem, targetWorkItem);
                _workItemLinkEnricher.MigrateSharedParameters(sourceWorkItem, targetWorkItem);
            }
        }

        private WorkItemData ReplayRevisions(List<RevisionItem> revisionsToMigrate, WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            try
            {
                //If work item hasn't been created yet, create a shell
                if (targetWorkItem == null)
                {
                    var finalDestType = revisionsToMigrate.Last().Type;
                    var targetType = revisionsToMigrate.First().Type;

                    if (targetType != finalDestType)
                    {
                        TraceWriteLine(LogEventLevel.Information, $"WorkItem has changed type at one of the revisions, from {targetType} to {finalDestType}");
                    }

                    if (Engine.TypeDefinitionMaps.Items.ContainsKey(targetType))
                    {
                        targetType = Engine.TypeDefinitionMaps.Items[targetType].Map();
                    }
                    targetWorkItem = CreateWorkItem_Shell(Engine.Target.WorkItems.Project, sourceWorkItem, targetType);
                }

                if (_config.AttachRevisionHistory)
                {
                    _revisionManager.AttachSourceRevisionHistoryJsonToTarget(sourceWorkItem, targetWorkItem);
                }

                foreach (var revision in revisionsToMigrate)
                {
                    var currentRevisionWorkItem = sourceWorkItem.GetRevision(revision.Number);

                    TraceWriteLine(LogEventLevel.Information, " Processing Revision [{RevisionNumber}]",
                        new Dictionary<string, object>() {
                            {"RevisionNumber", revision.Number }
                        });

                    // Decide on WIT
                    var destType = currentRevisionWorkItem.Type;
                    if (Engine.TypeDefinitionMaps.Items.ContainsKey(destType))
                    {
                        destType = Engine.TypeDefinitionMaps.Items[destType].Map();
                    }
                    bool typeChange = (destType != targetWorkItem.Type);

                    int workItemId = Int32.Parse(targetWorkItem.Id);

                    if (typeChange && workItemId > 0)
                    {
                        ValidatePatTokenRequirement();
                        Uri collectionUri = Engine.Target.Config.AsTeamProjectConfig().Collection;
                        string token = Engine.Target.Config.AsTeamProjectConfig().PersonalAccessToken;
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
                        var result = workItemTrackingClient.UpdateWorkItemAsync(patchDocument, workItemId, bypassRules: true).Result;
                        targetWorkItem = Engine.Target.WorkItems.GetWorkItem(workItemId);
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
                    targetWorkItem.ToWorkItem().Fields["System.ChangedDate"].Value = revision.Fields["System.ChangedDate"].Value;
                    targetWorkItem.ToWorkItem().Fields["System.ChangedBy"].Value = revision.Fields["System.ChangedBy"].Value.ToString();
                    targetWorkItem.ToWorkItem().Fields["System.History"].Value = revision.Fields["System.History"].Value;

                    // Todo: Ensure all field maps use WorkItemData.Fields to apply a correct mapping
                    Engine.FieldMaps.ApplyFieldMappings(currentRevisionWorkItem, targetWorkItem);

                    // Todo: Think about an "UpdateChangedBy" flag as this is expensive! (2s/WI instead of 1,5s when writing "Migration")

                    var reflectedUri = (TfsReflectedWorkItemId)Engine.Source.WorkItems.CreateReflectedWorkItemId(sourceWorkItem);
                    if (!targetWorkItem.ToWorkItem().Fields.Contains(Engine.Target.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName))
                    {
                        var ex = new InvalidOperationException("ReflectedWorkItemIDField Field Missing");
                        Log.LogError(ex,
                            " The WorkItemType {WorkItemType} does not have a Field called {ReflectedWorkItemID}",
                            targetWorkItem.Type,
                            Engine.Target.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName);
                        throw ex;
                    }
                    targetWorkItem.ToWorkItem().Fields[Engine.Target.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName].Value = reflectedUri.ToString();

                    ProcessHTMLFieldAttachements(targetWorkItem);
                    ProcessWorkItemEmbeddedLinks(sourceWorkItem, targetWorkItem);

                    var skipIterationRevision = SkipRevisionWithInvalidIterationPath(targetWorkItem);
                    var skipAreaRevision = SkipRevisionWithInvalidAreaPath(targetWorkItem);

                    CheckClosedDateIsValid(sourceWorkItem, targetWorkItem);

                    if (!skipIterationRevision && !skipAreaRevision)
                    {

                        targetWorkItem.SaveToAzureDevOps();
                    }
                    TraceWriteLine(LogEventLevel.Information,
                        " Saved TargetWorkItem {TargetWorkItemId}. Replayed revision {RevisionNumber} of {RevisionsToMigrateCount}",
                       new Dictionary<string, object>() {
                               {"TargetWorkItemId", targetWorkItem.Id },
                               {"RevisionNumber", revision.Number },
                               {"RevisionsToMigrateCount",  revisionsToMigrate.Count}
                           });
                }

                // Until here we impersonate the maker of the revisions. From here we act as ourselves to push the attachments and add the comment
                targetWorkItem.ToWorkItem().Fields["System.ChangedBy"].Value = "Migration";

                if (targetWorkItem != null)
                {
                    ProcessWorkItemAttachments(sourceWorkItem, targetWorkItem, false);
                    if (!string.IsNullOrEmpty(targetWorkItem.Id))
                    {
                        ProcessWorkItemLinks(Engine.Source.WorkItems, Engine.Target.WorkItems, sourceWorkItem, targetWorkItem);
                        // The TFS client seems to plainly ignore the ChangedBy field when saving a link, so we need to put this back in place
                        targetWorkItem.ToWorkItem().Fields["System.ChangedBy"].Value = "Migration";
                    }

                    if (_config.GenerateMigrationComment)
                    {
                        var reflectedUri = targetWorkItem.ToWorkItem().Fields[Engine.Target.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName].Value;
                        var history = new StringBuilder();
                        history.Append(
                            $"This work item was migrated from a different project or organization. You can find the old version at <a href=\"{reflectedUri}\">{reflectedUri}</a>.");
                        targetWorkItem.ToWorkItem().History = history.ToString();
                    }
                    targetWorkItem.SaveToAzureDevOps();

                    _attachmentEnricher.CleanUpAfterSave();
                    TraceWriteLine(LogEventLevel.Information, "...Saved as {TargetWorkItemId}", new Dictionary<string, object> { { "TargetWorkItemId", targetWorkItem.Id } });
                }
            }
            catch (Exception ex)
            {
                TraceWriteLine(LogEventLevel.Information, "...FAILED to Save");
                Log.LogInformation("===============================================================");
                if (targetWorkItem != null)
                {
                    foreach (Field f in targetWorkItem.ToWorkItem().Fields)
                        TraceWriteLine(LogEventLevel.Information, "{FieldReferenceName} ({FieldName}) | {FieldValue}",
                            new Dictionary<string, object>()
                            {
                                { "FieldReferenceName", f.ReferenceName }, { "FieldName", f.Name },
                                { "FieldValue", f.Value }
                            });
                }
                Log.LogInformation("===============================================================");
                Log.LogError(ex.ToString(), ex);
                Log.LogInformation("===============================================================");
            }

            return targetWorkItem;
        }

        private void CheckClosedDateIsValid(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            var closedDateField = "System.ClosedDate";
            if (targetWorkItem.ToWorkItem().Fields.Contains("Microsoft.VSTS.Common.ClosedDate")) {
                closedDateField = "Microsoft.VSTS.Common.ClosedDate";
            }
            Log.LogDebug("CheckClosedDateIsValid::ClosedDate field is {closedDateField}", closedDateField);
            if (targetWorkItem.ToWorkItem().Fields[closedDateField].Value == null && (targetWorkItem.ToWorkItem().Fields["System.State"].Value.ToString() == "Closed" || targetWorkItem.ToWorkItem().Fields["System.State"].Value.ToString() == "Done"))
            {
                Log.LogWarning("The field {closedDateField} is set to Null and will revert to the current date on save! ", closedDateField);
                Log.LogWarning("Source Closed Date [#{sourceId}][Rev{sourceRev}]: {sourceClosedDate} ", sourceWorkItem.ToWorkItem().Id, sourceWorkItem.ToWorkItem().Rev, sourceWorkItem.ToWorkItem().Fields[closedDateField].Value);            
            }
            if (!sourceWorkItem.ToWorkItem().Fields.Contains(closedDateField))
            {
                Log.LogWarning("The ClosedDate field {closedDateField} on the Target does not exist in the source! You can fix this with a mapping!", closedDateField);
                if (sourceWorkItem.ToWorkItem().Fields.Contains("Microsoft.VSTS.Common.ClosedDate"))
                {
                    Log.LogWarning("Source ClosedDate Field: ", "Microsoft.VSTS.Common.ClosedDate");
                }
                if (sourceWorkItem.ToWorkItem().Fields.Contains("System.ClosedDate"))
                {
                    Log.LogWarning("Source ClosedDate Field: ", "System.ClosedDate");
                }
                if (targetWorkItem.ToWorkItem().Fields.Contains("Microsoft.VSTS.Common.ClosedDate"))
                {
                    Log.LogWarning("Target ClosedDate Field: ", "Microsoft.VSTS.Common.ClosedDate");
                }
                if (targetWorkItem.ToWorkItem().Fields.Contains("System.ClosedDate"))
                {
                    Log.LogWarning("Target ClosedDate Field: ", "System.ClosedDate");
                }
            }

        }

        private bool SkipRevisionWithInvalidIterationPath(WorkItemData targetWorkItemData)
        {
            if (!_config.SkipRevisionWithInvalidIterationPath)
            {
                return false;
            }

            return ValidateRevisionField(targetWorkItemData, "System.IterationPath");
        }

        private bool SkipRevisionWithInvalidAreaPath(WorkItemData targetWorkItemData)
        {
            if (!_config.SkipRevisionWithInvalidAreaPath)
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