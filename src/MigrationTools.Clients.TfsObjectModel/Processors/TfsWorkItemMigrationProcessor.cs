using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
using MigrationTools.Clients;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools._EngineV1.Containers;
using MigrationTools._EngineV1.DataContracts;

using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Services;
using MigrationTools.Tools;
using Newtonsoft.Json.Linq;
using Serilog.Context;
using Serilog.Events;
using ILogger = Serilog.ILogger;
using MigrationTools.Tools.Interfaces;

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

        private static int _count = 0;
        private static int _current = 0;
        private static int _totalWorkItem = 0;
        private static string workItemLogTemplate = "[{sourceWorkItemTypeName,20}][Complete:{currentWorkItem,6}/{totalWorkItems}][sid:{sourceWorkItemId,6}|Rev:{sourceRevisionInt,3}][tid:{targetWorkItemId,6} | ";
        private List<string> _ignore;

        private ILogger contextLog;
        private ILogger workItemLog;
        private List<string> _itemsInError;

        public WorkItemMetrics workItemMetrics { get; private set; }

        public TfsWorkItemMigrationProcessor(IOptions<TfsWorkItemMigrationProcessorOptions> options, TfsCommonTools tfsCommonTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<TfsWorkItemMigrationProcessor> logger) : base(options, tfsCommonTools, processorEnrichers, services, telemetry, logger)
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
                ValidateAllWorkItemTypesHaveReflectedWorkItemIdField(sourceWorkItems);
                ValiddateWorkItemTypesExistInTarget(sourceWorkItems);
                CommonTools.NodeStructure.ValidateAllNodesExistOrAreMapped(this, sourceWorkItems, Source.WorkItems.Project.Name, Target.WorkItems.Project.Name);
                ValidateAllUsersExistOrAreMapped(sourceWorkItems);
                //////////////////////////////////////////////////

                contextLog.Information("Found target project as {@destProject}", Target.WorkItems.Project.Name);

                //////////////////////////////////////////////////////////FilterCompletedByQuery

                if (Options.FilterWorkItemsThatAlreadyExistInTarget)
                {
                    contextLog.Information(
                        "[FilterWorkItemsThatAlreadyExistInTarget] is enabled. Searching for {sourceWorkItems} work items that may have already been migrated to the target...",
                        sourceWorkItems.Count());

                    string targetWIQLQuery = CommonTools.NodeStructure.FixAreaPathAndIterationPathForTargetQuery(Options.WIQLQuery,
                        Source.WorkItems.Project.Name, Target.WorkItems.Project.Name, contextLog);
                    // Also replace Project Name
                    targetWIQLQuery = targetWIQLQuery.Replace(Source.WorkItems.Project.Name, Target.WorkItems.Project.Name);
                    //Then run query
                    sourceWorkItems = ((TfsWorkItemMigrationClient)Target.WorkItems).FilterExistingWorkItems(
                        sourceWorkItems, targetWIQLQuery,
                        (TfsWorkItemMigrationClient)Source.WorkItems);
                    contextLog.Information(
                        "!! After removing all found work items there are {SourceWorkItemCount} remaining to be migrated.",
                        sourceWorkItems.Count());
                }

                //////////////////////////////////////////////////


                _current = 1;
                _count = sourceWorkItems.Count;
                _totalWorkItem = sourceWorkItems.Count;
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
                            ProcessWorkItemAsync(sourceWorkItemData, Options.WorkItemCreateRetryLimit).Wait();

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
                            _itemsInError.Add(sourceWorkItem.Id.ToString());
                            workItemLog.Error(e, "Could not save migrated work item {WorkItemId}, an exception occurred.", sourceWorkItem.Id);

                            if (Options.MaxGracefulFailures == 0)
                            {
                                throw;
                            }

                            if (_itemsInError.Count > Options.MaxGracefulFailures)
                            {
                                throw new Exception($"Too many errors: more than {Options.MaxGracefulFailures} errors occurred, aborting migration.");
                            }
                        }
                    }
                }
            }
            finally
            {
                if (Options.FixHtmlAttachmentLinks)
                {
                    CommonTools.EmbededImages?.ProcessorExecutionEnd(null);
                }


                if (_itemsInError.Count > 0)
                {
                    contextLog.Warning("The following items could not be migrated: {ItemIds}", string.Join(", ", _itemsInError));
                }

            }
        }

        private void ValidateAllUsersExistOrAreMapped(List<WorkItemData> sourceWorkItems)
        {
            contextLog.Information("Validating::Check that all users in the source exist in the target or are mapped!");
            IdentityMapResult usersToMap = CommonTools.UserMapping.GetUsersInSourceMappedToTargetForWorkItems(this, sourceWorkItems);
            if (usersToMap != null && usersToMap.IdentityMap != null && usersToMap.IdentityMap.Count > 0)
            {
                Log.LogWarning("Validating Failed! There are {usersToMap} users that exist in the source that do not exist "
                    + "in the target. This will not cause any errors, but may result in disconnected users that could have "
                    + "been mapped. Use the ExportUsersForMapping processor to create a list of mappable users.",
                    usersToMap.IdentityMap.Count);
            }
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

        private void ValidateAllWorkItemTypesHaveReflectedWorkItemIdField(List<WorkItemData> sourceWorkItems)
        {
            contextLog.Information("Validating::Check all Target Work Items have the RefectedWorkItemId field");

            var result = CommonTools.ValidateRequiredField.ValidatingRequiredField(this,
                Target.Options.ReflectedWorkItemIdField, sourceWorkItems);
            if (!result)
            {
                var ex = new InvalidFieldValueException(
                    "Not all work items in scope contain a valid ReflectedWorkItemId Field!");
                Log.LogError(ex, "Not all work items in scope contain a valid ReflectedWorkItemId Field!");
                Environment.Exit(-1);
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

            var targetWorkItemTypes = Target.WorkItems.Project.ToProject().WorkItemTypes.Cast<WorkItemType>().Select(x => x.Name);
            Log.LogDebug("Validating::WorkItemTypes::targetWorkItemTypes::{count} WorkItemTypes in Target process: {targetWorkItemTypesString}", targetWorkItemTypes.Count(), string.Join(",", targetWorkItemTypes));

            var missingWorkItemTypes = sourceWorkItemTypes.Where(sourceWit => !targetWorkItemTypes.Contains(sourceWit)); // the real one
            if (missingWorkItemTypes.Count() > 0)
            {
                Log.LogWarning("Validating::WorkItemTypes::targetWorkItemTypes::There are {count} WorkItemTypes that are used in the history of the Source and that do not exist in the Target. These will all need mapped using `WorkItemTypeDefinition` in the config. ", missingWorkItemTypes.Count());

                bool allTypesMapped = true;
                foreach (var missingWorkItemType in missingWorkItemTypes)
                {
                    bool thisTypeMapped = true;
                    if (!CommonTools.WorkItemTypeMapping.Mappings.ContainsKey(missingWorkItemType))
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
                    Environment.Exit(-1);
                }
            }
        }

        private void ValidatePatTokenRequirement()
        {
            string collUrl = Target.Options.Collection.ToString();
            if (collUrl.Contains("dev.azure.com") || collUrl.Contains(".visualstudio.com"))
            {
                var token = Target.Options.Authentication.AccessToken;
                // Test that
                if (token.IsNullOrEmpty())
                {
                    var ex = new InvalidOperationException("Missing PersonalAccessToken from Target");
                    Log.LogError(ex, "When you are migrating to Azure DevOps you MUST provide an PAT so that we can call the REST API for certain actions. For example we would be unable to deal with a Work item Type change.");
                    Environment.Exit(-1);
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
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("CreateWorkItem_Shell", ActivityKind.Client))
            {
                activity?.SetTagsFromOptions(Options);
                activity?.SetTag("http.request.method", "GET");
                activity?.SetTag("migrationtools.client", "TfsObjectModel");
                activity?.SetEndTime(activity.StartTimeUtc.AddSeconds(10));

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
                if (Options.UpdateCreatedBy)
                {
                    newwit.Fields["System.CreatedBy"].Value = currentRevisionWorkItem.ToWorkItem().Revisions[0].Fields["System.CreatedBy"].Value;
                    workItemLog.Debug("Setting 'System.CreatedBy'={SystemCreatedBy}", currentRevisionWorkItem.ToWorkItem().Revisions[0].Fields["System.CreatedBy"].Value);
                }
                if (Options.UpdateCreatedDate)
                {
                    newwit.Fields["System.CreatedDate"].Value = currentRevisionWorkItem.ToWorkItem().Revisions[0].Fields["System.CreatedDate"].Value;
                    workItemLog.Debug("Setting 'System.CreatedDate'={SystemCreatedDate}", currentRevisionWorkItem.ToWorkItem().Revisions[0].Fields["System.CreatedDate"].Value);
                }
                return newwit.AsWorkItemData();
            }
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
            try
            {
                if (newWorkItem.Fields.Contains("Microsoft.VSTS.Common.ClosedDate") && newWorkItem.Fields["Microsoft.VSTS.Common.ClosedDate"].IsEditable)
                {
                    newWorkItem.Fields["Microsoft.VSTS.Common.ClosedDate"].Value = oldWorkItem.Fields["Microsoft.VSTS.Common.ClosedDate"].Value;
                }
            }
            catch (FieldDefinitionNotExistException ex)
            {
                // Eat exception coz the TFS API Sucks
            }
            newWorkItem.Reason = oldWorkItem.Reason;

            foreach (Field f in oldWorkItem.Fields)
            {
                CommonTools.UserMapping.MapUserIdentityField(f);
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
                            string oldValue = oldWorkItem.Fields[f.ReferenceName].Value.ToString();
                            string newValue = CommonTools.StringManipulator.ProcessString(oldValue);
                            newWorkItem.Fields[f.ReferenceName].Value = newValue;
                            break;
                        default:
                            newWorkItem.Fields[f.ReferenceName].Value = oldWorkItem.Fields[f.ReferenceName].Value;
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
            if (targetWorkItem != null && Options.FixHtmlAttachmentLinks)
            {
                CommonTools.EmbededImages.FixEmbededImages(this, null, targetWorkItem);
            }
        }

        private void ProcessWorkItemEmbeddedLinks(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            if (sourceWorkItem != null && targetWorkItem != null && Options.FixHtmlAttachmentLinks)
            {
                CommonTools.WorkItemEmbededLink.Enrich(this, sourceWorkItem, targetWorkItem);
            }
        }

        private async Task ProcessWorkItemAsync(WorkItemData sourceWorkItem, int retryLimit = 5, int retries = 0)
        {
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("ProcessWorkItemAsync", ActivityKind.Client))
            {
                activity?.SetTagsFromOptions(Options);
                activity?.SetTag("http.request.method", "GET");
                activity?.SetTag("migrationtools.client", "TfsObjectModel");
                activity?.SetEndTime(activity.StartTimeUtc.AddSeconds(10));
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
                        List<RevisionItem> revisionsToMigrate = CommonTools.RevisionManager.GetRevisionsToMigrate(sourceWorkItem.Revisions.Values.ToList(), targetWorkItem?.Revisions.Values.ToList());
                        if (targetWorkItem == null)
                        {
                            targetWorkItem = ReplayRevisions(revisionsToMigrate, sourceWorkItem, null);
                            activity?.SetTag("Revisions", revisionsToMigrate.Count);

                        }
                        else
                        {
                            if (revisionsToMigrate.Count == 0)
                            {
                                ProcessWorkItemAttachments(sourceWorkItem, targetWorkItem, false);
                                ProcessWorkItemLinks(sourceWorkItem, targetWorkItem);
                                ProcessHTMLFieldAttachements(targetWorkItem);
                                ProcessWorkItemEmbeddedLinks(sourceWorkItem, targetWorkItem);
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
                    activity?.Stop();
                    activity?.SetStatus(ActivityStatusCode.Error);
                    activity?.SetTag("http.response.status_code", "502");
                    Log.LogError(ex, ex.ToString());
                    Telemetry.TrackException(ex, activity.Tags);
                    throw ex;
                }
                var average = new TimeSpan(0, 0, 0, 0, (int)(activity.Duration.TotalMilliseconds / _current));
                var remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * _count));
                TraceWriteLine(LogEventLevel.Information,
                    "Average time of {average:%s}.{average:%fff} per work item and {remaining:%h} hours {remaining:%m} minutes {remaining:%s}.{remaining:%fff} seconds estimated to completion",
                    new Dictionary<string, object>() {
                    {"average", average},
                    {"remaining", remaining}
                    });
                activity?.Stop();
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

        private void ProcessWorkItemLinks(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            if (targetWorkItem != null && CommonTools.WorkItemLink.Enabled && sourceWorkItem.ToWorkItem().Links.Count > 0)
            {
                TraceWriteLine(LogEventLevel.Information, "Links {SourceWorkItemLinkCount} | LinkMigrator:{LinkMigration}", new Dictionary<string, object>() { { "SourceWorkItemLinkCount", sourceWorkItem.ToWorkItem().Links.Count }, { "LinkMigration", CommonTools.WorkItemLink.Enabled } });
                CommonTools.WorkItemLink.Enrich(this, sourceWorkItem, targetWorkItem);
                //AddMetric("RelatedLinkCount", processWorkItemMetrics, targetWorkItem.ToWorkItem().Links.Count);
                int fixedLinkCount = CommonTools.GitRepository.Enrich(this, sourceWorkItem, targetWorkItem);
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

                    if (CommonTools.WorkItemTypeMapping.Mappings.ContainsKey(targetType))
                    {
                        targetType = CommonTools.WorkItemTypeMapping.Mappings[targetType];
                    }
                    targetWorkItem = CreateWorkItem_Shell(Target.WorkItems.Project, sourceWorkItem, targetType);
                }

                if (Options.AttachRevisionHistory)
                {
                    CommonTools.RevisionManager.AttachSourceRevisionHistoryJsonToTarget(sourceWorkItem, targetWorkItem);
                }

                foreach (var revision in revisionsToMigrate)
                {
                    workItemMetrics.RevisionsProcessedCount.Add(1);
                    var currentRevisionWorkItem = sourceWorkItem.GetRevision(revision.Number);

                    TraceWriteLine(LogEventLevel.Information, " Processing Revision [{RevisionNumber}]",
                        new Dictionary<string, object>() {
                            {"RevisionNumber", revision.Number }
                        });

                    // Decide on WIT
                    var destType = currentRevisionWorkItem.Type;
                    if (CommonTools.WorkItemTypeMapping.Mappings.ContainsKey(destType))
                    {
                        destType = CommonTools.WorkItemTypeMapping.Mappings[destType];
                    }
                    bool typeChange = (destType != targetWorkItem.Type);

                    int workItemId = Int32.Parse(targetWorkItem.Id);

                    if (typeChange && workItemId > 0)
                    {
                        ValidatePatTokenRequirement();
                        Uri collectionUri = Target.Options.Collection;
                        string token = Target.Options.Authentication.AccessToken;
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
                        targetWorkItem = Target.WorkItems.GetWorkItem(workItemId);
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

                    // Todo: Ensure all field maps use WorkItemData.Fields to apply a correct mapping
                    CommonTools.FieldMappingTool.ApplyFieldMappings(currentRevisionWorkItem, targetWorkItem);

                    // Todo: Think about an "UpdateChangedBy" flag as this is expensive! (2s/WI instead of 1,5s when writing "Migration")

                    var reflectedUri = (TfsReflectedWorkItemId)Source.WorkItems.CreateReflectedWorkItemId(sourceWorkItem);
                    if (!targetWorkItem.ToWorkItem().Fields.Contains(Target.Options.ReflectedWorkItemIdField))
                    {
                        var ex = new InvalidOperationException("ReflectedWorkItemIdField Field Missing");
                        Log.LogError(ex,
                            " The WorkItemType {WorkItemType} does not have a Field called {ReflectedWorkItemID}",
                            targetWorkItem.Type,
                            Target.Options.ReflectedWorkItemIdField);
                        throw ex;
                    }
                    targetWorkItem.ToWorkItem().Fields[Target.Options.ReflectedWorkItemIdField].Value = reflectedUri.ToString();

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
                        ProcessWorkItemLinks(sourceWorkItem, targetWorkItem);
                        // The TFS client seems to plainly ignore the ChangedBy field when saving a link, so we need to put this back in place
                        targetWorkItem.ToWorkItem().Fields["System.ChangedBy"].Value = "Migration";
                    }

                    if (Options.GenerateMigrationComment)
                    {
                        var reflectedUri = targetWorkItem.ToWorkItem().Fields[Target.Options.ReflectedWorkItemIdField].Value;
                        var history = new StringBuilder();
                        history.Append(
                            $"This work item was migrated from a different project or organization. You can find the old version at <a href=\"{reflectedUri}\">{reflectedUri}</a>.");
                        targetWorkItem.ToWorkItem().History = history.ToString();
                    }
                    targetWorkItem.SaveToAzureDevOps();

                    CommonTools.Attachment.CleanUpAfterSave();
                    TraceWriteLine(LogEventLevel.Information, "...Saved as {TargetWorkItemId}", new Dictionary<string, object> { { "TargetWorkItemId", targetWorkItem.Id } });
                }
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
            if (targetWorkItem.ToWorkItem().Fields.Contains("Microsoft.VSTS.Common.ClosedDate"))
            {
                closedDateField = "Microsoft.VSTS.Common.ClosedDate";
            }
            else if (!targetWorkItem.ToWorkItem().Fields.Contains("System.ClosedDate"))
            {
                Log.LogDebug("CheckClosedDateIsValid::ClosedDate field doesn't exist in targetWorkItem: {targetWorkItem} - nothing to validate.", targetWorkItem);
                return;
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
