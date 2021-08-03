﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Proxy;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using MigrationTools;
using MigrationTools._EngineV1.Clients;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools._EngineV1.DataContracts;
using MigrationTools._EngineV1.Enrichers;
using MigrationTools._EngineV1.Processors;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.ProcessorEnrichers;
using Newtonsoft.Json;
using Serilog.Context;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemMigrationContext : MigrationProcessorBase
    {
        private static int _count = 0;
        private static int _current = 0;
        private static long _elapsedms = 0;
        private static int _totalWorkItem = 0;
        private static string workItemLogTeamplate = "[{sourceWorkItemTypeName,20}][Complete:{currentWorkItem,6}/{totalWorkItems}][sid:{sourceWorkItemId,6}|Rev:{sourceRevisionInt,3}][tid:{targetWorkItemId,6} | ";
        private WorkItemMigrationConfig _config;
        private List<String> _ignore;
        private WorkItemTrackingHttpClient _witClient;

        private ILogger contextLog;
        private IAttachmentMigrationEnricher attachmentEnricher;
        private IWorkItemProcessorEnricher embededImagesEnricher;
        private TfsGitRepositoryEnricher gitRepositoryEnricher;
        private TfsNodeStructure nodeStructureEnricher;
        private TfsRevisionManager revisionManager;
        private TfsValidateRequiredField validateConfig;
        private IDictionary<string, double> processWorkItemMetrics = null;
        private IDictionary<string, string> processWorkItemParamiters = null;
        private TfsWorkItemLinkEnricher workItemLinkEnricher;
        private ILogger workItemLog;

        public WorkItemMigrationContext(IMigrationEngine engine, IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemMigrationContext> logger) : base(engine, services, telemetry, logger)
        {
            contextLog = Serilog.Log.ForContext<WorkItemMigrationContext>();
        }

        public override string Name => "WorkItemMigration";

        public override void Configure(IProcessorConfig config)
        {
            _config = (WorkItemMigrationConfig)config;
            validateConfig = Services.GetRequiredService<TfsValidateRequiredField>();
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
            workItemLog.Write(level, workItemLogTeamplate + message);
        }

        protected override void InternalExecute()
        {
            Log.LogInformation("WorkItemMigrationContext::InternalExecute ");
            if (_config == null)
            {
                throw new Exception("You must call Configure() first");
            }
            var workItemServer = Engine.Source.GetService<WorkItemServer>();
            attachmentEnricher = new TfsAttachmentEnricher(workItemServer, _config.AttachmentWorkingPath, _config.AttachmentMaxSize);
            workItemLinkEnricher = Services.GetRequiredService<TfsWorkItemLinkEnricher>();
            embededImagesEnricher = Services.GetRequiredService<TfsEmbededImagesEnricher>();
            gitRepositoryEnricher = Services.GetRequiredService<TfsGitRepositoryEnricher>();
            nodeStructureEnricher = Services.GetRequiredService<TfsNodeStructure>();
            nodeStructureEnricher.Configure(new TfsNodeStructureOptions() { Enabled = true, NodeBasePaths = _config.NodeBasePaths, PrefixProjectToNodes = _config.PrefixProjectToNodes });
            nodeStructureEnricher.ProcessorExecutionBegin(null);
            revisionManager = Services.GetRequiredService<TfsRevisionManager>();
            revisionManager.Configure(new TfsRevisionManagerOptions() { Enabled = true, MaxRevisions = _config.MaxRevisions, ReplayRevisions = _config.ReplayRevisions });


            _witClient = new WorkItemTrackingHttpClient(Engine.Target.Config.AsTeamProjectConfig().Collection, Engine.Target.Credentials);
            //Validation: make sure that the ReflectedWorkItemId field name specified in the config exists in the target process, preferably on each work item type.
            PopulateIgnoreList();

            var stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            string sourceQuery =
                string.Format(
                    @"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY {1}",
                    _config.WIQLQueryBit, _config.WIQLOrderBit);

            // Inform the user that he maybe has to be patient now
            contextLog.Information("Querying items to be migrated: {SourceQuery} ...", sourceQuery);
            var sourceWorkItems = Engine.Source.WorkItems.GetWorkItems(sourceQuery);
            contextLog.Information("Replay all revisions of {sourceWorkItemsCount} work items?", sourceWorkItems.Count);
            //////////////////////////////////////////////////
            contextLog.Information("Found target project as {@destProject}", Engine.Target.WorkItems.Project.Name);
            //////////////////////////////////////////////////////////FilterCompletedByQuery
            if (_config.FilterWorkItemsThatAlreadyExistInTarget)
            {
                contextLog.Information("[FilterWorkItemsThatAlreadyExistInTarget] is enabled. Searching for work items that have already been migrated to the target...", sourceWorkItems.Count());
                                               
                string targetWIQLQueryBit = FixAreaPathInTargetQuery(_config.WIQLQueryBit, Engine.Source.WorkItems.Project.Name, Engine.Target.WorkItems.Project.Name, _config.NodeBasePaths, contextLog);
                sourceWorkItems = ((TfsWorkItemMigrationClient)Engine.Target.WorkItems).FilterExistingWorkItems(sourceWorkItems, new TfsWiqlDefinition() { OrderBit = _config.WIQLOrderBit, QueryBit = targetWIQLQueryBit }, (TfsWorkItemMigrationClient)Engine.Source.WorkItems);
                contextLog.Information("!! After removing all found work items there are {SourceWorkItemCount} remaining to be migrated.", sourceWorkItems.Count());
            }
            //////////////////////////////////////////////////

            var result = validateConfig.ValidatingRequiredField(Engine.Target.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName, sourceWorkItems);
            if (!result)
            {
                var ex = new InvalidFieldValueException("Not all work items in scope contain a valid ReflectedWorkItemId Field!");
                Log.LogError(ex, "Not all work items in scope contain a valid ReflectedWorkItemId Field!");
                throw ex;
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
                    ProcessWorkItem(sourceWorkItemData, _config.WorkItemCreateRetryLimit);
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
            }
            //////////////////////////////////////////////////
            stopwatch.Stop();

            contextLog.Information("DONE in {Elapsed}", stopwatch.Elapsed.ToString("c"));
        }

        internal static string FixAreaPathInTargetQuery(string sourceWIQLQueryBit, string sourceProject, string targetProject, string[] nodeBasePaths, ILogger? contextLog)
        {
            string targetWIQLQueryBit = sourceWIQLQueryBit;
            if (nodeBasePaths != null && nodeBasePaths.Any() && targetWIQLQueryBit.Contains("[System.AreaPath]"))
            {               
                if (sourceProject != targetProject)
                {
                    //Switch out source Area Path with destination
                    foreach (var nodeBasePath in nodeBasePaths)
                    {
                        if (sourceWIQLQueryBit.Contains(nodeBasePath))
                        {
                            contextLog?.Information("[NodeBasePaths] has been set and QueryBit contains [System.AreaPath].  Since {nodeBasePath} was found in the query, updating the projectName from {source} to {target}", nodeBasePath, sourceProject, targetProject);
                            StringBuilder myStringBuilder = new StringBuilder(sourceWIQLQueryBit);
                            int locationOfAreaPath = sourceWIQLQueryBit.IndexOf("[System.AreaPath]");
                            int querySegmentSize = (sourceWIQLQueryBit.IndexOf(nodeBasePath) + nodeBasePath.Length) - locationOfAreaPath;
                            myStringBuilder.Replace(sourceProject, targetProject, locationOfAreaPath, querySegmentSize);
                            targetWIQLQueryBit = myStringBuilder.ToString();
                        }
                    }
                }
            }

            return targetWIQLQueryBit;
        }

        private static void AppendMigratedByFooter(StringBuilder history)
        {
            history.Append("<p>Migrated by <a href='https://dev.azure.com/nkdagility/migration-tools/'>Azure DevOps Migration Tools</a> open source.</p>");
        }

        private static void BuildCommentTable(WorkItem oldWi, StringBuilder history)
        {
            if (oldWi.Revisions != null && oldWi.Revisions.Count > 0)
            {
                history.Append("<p>Comments from previous work item:</p>");
                history.Append("<table border='1' style='width:100%;border-color:#C0C0C0;'>");
                foreach (Revision r in oldWi.Revisions)
                {
                    if ((string)r.Fields["System.History"].Value != "" && (string)r.Fields["System.ChangedBy"].Value != "Martin Hinshelwood (Adm)")
                    {
                        r.WorkItem.Open();
                        history.AppendFormat("<tr><td style='align:right;width:100%'><p><b>{0} on {1}:</b></p><p>{2}</p></td></tr>", r.Fields["System.ChangedBy"].Value, DateTime.Parse(r.Fields["System.ChangedDate"].Value.ToString()).ToLongDateString(), r.Fields["System.History"].Value);
                    }
                }
                history.Append("</table>");
                history.Append("<p>&nbsp;</p>");
            }
        }

        private static void BuildFieldTable(WorkItem oldWi, StringBuilder history, bool useHTML = false)
        {
            history.Append("<p>Fields from previous Work Item:</p>");
            foreach (Field f in oldWi.Fields)
            {
                if (f.Value == null)
                {
                    history.AppendLine(string.Format("{0}: null<br />", f.Name));
                }
                else
                {
                    history.AppendLine(string.Format("{0}: {1}<br />", f.Name, f.Value.ToString()));
                }
            }
            history.Append("<p>&nbsp;</p>");
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
            if (_config.UpdateCreatedBy) { newwit.Fields["System.CreatedBy"].Value = currentRevisionWorkItem.ToWorkItem().Revisions[0].Fields["System.CreatedBy"].Value; }
            if (_config.UpdateCreatedDate) { newwit.Fields["System.CreatedDate"].Value = currentRevisionWorkItem.ToWorkItem().Revisions[0].Fields["System.CreatedDate"].Value; }
            return newwit.AsWorkItemData();
        }

        [Obsolete("Replaced be TfsWorkItemMigrationClient.FilterExistingWorkItems ", true)]
        private List<WorkItemData> FilterByTarget(List<WorkItemData> sourceWorkItems)
        {
            contextLog.Debug("FilterByTarget: START");
            var targetQuery =
                string.Format(
                    @"SELECT [System.Id], [{0}] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {1} ORDER BY {2}",
                     Engine.Target.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName,
                    _config.WIQLQueryBit,
                    _config.WIQLOrderBit
                    );
            contextLog.Debug("FilterByTarget: Query Execute...");
            var targetFoundItems = Engine.Target.WorkItems.GetWorkItems(targetQuery);
            contextLog.Debug("FilterByTarget: ... query complete.");
            contextLog.Debug("FilterByTarget: Found {TargetWorkItemCount} based on the WIQLQueryBit in the target system.", targetFoundItems.Count());
            var targetFoundIds = (from WorkItemData twi in targetFoundItems select Engine.Target.WorkItems.GetReflectedWorkItemId(twi)).ToList();
            //////////////////////////////////////////////////////////
            sourceWorkItems = sourceWorkItems.Where(p => !targetFoundIds.Any(p2 => p2.ToString() == p.Id)).ToList();
            contextLog.Debug("FilterByTarget: After removing all found work items there are {SourceWorkItemCount} remaining to be migrated.", sourceWorkItems.Count());
            contextLog.Debug("FilterByTarget: END");
            return sourceWorkItems;
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
        private void PopulateWorkItem(WorkItemData oldWi, WorkItemData newwit, string destType)
        {
            var oldWorkItem = oldWi.ToWorkItem();
            var newWorkItem = newwit.ToWorkItem();
            var newWorkItemstartTime = DateTime.UtcNow;
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
                if (newWorkItem.Fields.Contains(f.ReferenceName) && !_ignore.Contains(f.ReferenceName) && (!newWorkItem.Fields[f.ReferenceName].IsChangedInRevision || newWorkItem.Fields[f.ReferenceName].IsEditable) && oldWorkItem.Fields[f.ReferenceName].Value != newWorkItem.Fields[f.ReferenceName].Value)
                {
                    Log.LogDebug("PopulateWorkItem:FieldUpdate: {ReferenceName} | Old:{OldReferenceValue} New:{NewReferenceValue}", f.ReferenceName, oldWorkItem.Fields[f.ReferenceName].Value, newWorkItem.Fields[f.ReferenceName].Value);
                    newWorkItem.Fields[f.ReferenceName].Value = oldWorkItem.Fields[f.ReferenceName].Value;
                }
            }

            newWorkItem.AreaPath = nodeStructureEnricher.GetNewNodeName(oldWorkItem.AreaPath, TfsNodeStructureType.Area);
            newWorkItem.IterationPath = nodeStructureEnricher.GetNewNodeName(oldWorkItem.IterationPath, TfsNodeStructureType.Iteration);
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

        private void ProcessWorkItem(WorkItemData sourceWorkItem, int retryLimit = 5, int retrys = 0)
        {
            var witstopwatch = Stopwatch.StartNew();
            var starttime = DateTime.Now;
            processWorkItemMetrics = new Dictionary<string, double>();
            processWorkItemParamiters = new Dictionary<string, string>();
            AddParameter("SourceURL", processWorkItemParamiters, Engine.Source.WorkItems.Config.AsTeamProjectConfig().Collection.ToString());
            AddParameter("SourceWorkItem", processWorkItemParamiters, sourceWorkItem.Id.ToString());
            AddParameter("TargetURL", processWorkItemParamiters, Engine.Target.WorkItems.Config.AsTeamProjectConfig().Collection.ToString());
            AddParameter("TargetProject", processWorkItemParamiters, Engine.Target.WorkItems.Project.Name);
            AddParameter("RetryLimit", processWorkItemParamiters, retryLimit.ToString());
            AddParameter("RetryNumber", processWorkItemParamiters, retrys.ToString());
            Log.LogDebug("######################################################################################");
            Log.LogDebug("ProcessWorkItem: {sourceWorkItemId}", sourceWorkItem.Id);
            Log.LogDebug("######################################################################################");
            try
            {
                if (sourceWorkItem.Type != "Test Plan" || sourceWorkItem.Type != "Test Suite")
                {
                    var targetWorkItem = Engine.Target.WorkItems.FindReflectedWorkItem(sourceWorkItem, false);
                    ///////////////////////////////////////////////
                    TraceWriteLine(LogEventLevel.Information, "Work Item has {sourceWorkItemRev} revisions and revision migration is set to {ReplayRevisions}",
                        new Dictionary<string, object>(){
                            { "sourceWorkItemRev", sourceWorkItem.Rev },
                            { "ReplayRevisions", _config.ReplayRevisions }}
                        );
                    List<RevisionItem> revisionsToMigrate = revisionManager.GetRevisionsToMigrate(sourceWorkItem, targetWorkItem);
                    if (targetWorkItem == null)
                    {
                        targetWorkItem = ReplayRevisions(revisionsToMigrate, sourceWorkItem, null, _current);
                        AddMetric("Revisions", processWorkItemMetrics, revisionsToMigrate.Count);
                    }
                    else
                    {
                        if (revisionsToMigrate.Count == 0)
                        {
                            ProcessWorkItemAttachments(sourceWorkItem, targetWorkItem, false);
                            ProcessWorkItemLinks(Engine.Source.WorkItems, Engine.Target.WorkItems, sourceWorkItem, targetWorkItem);
                            TraceWriteLine(LogEventLevel.Information, "Skipping as work item exists and no revisions to sync detected");
                            processWorkItemMetrics.Add("Revisions", 0);
                        }
                        else
                        {
                            TraceWriteLine(LogEventLevel.Information, "Syncing as there are {revisionsToMigrateCount} revisons detected",
                                new Dictionary<string, object>(){
                                    { "revisionsToMigrateCount", revisionsToMigrate.Count }
                                });

                            targetWorkItem = ReplayRevisions(revisionsToMigrate, sourceWorkItem, targetWorkItem, _current);

                            AddMetric("Revisions", processWorkItemMetrics, revisionsToMigrate.Count);
                            AddMetric("SyncRev", processWorkItemMetrics, revisionsToMigrate.Count);
                        }
                    }
                    AddParameter("TargetWorkItem", processWorkItemParamiters, targetWorkItem.ToWorkItem().Revisions.Count.ToString());
                    ///////////////////////////////////////////////
                    ProcessHTMLFieldAttachements(targetWorkItem);
                    ///////////////////////////////////////////////
                    ///////////////////////////////////////////////////////
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
                if (retrys < retryLimit)
                {
                    TraceWriteLine(LogEventLevel.Warning, "WebException: Will retry in {retrys}s ",
                        new Dictionary<string, object>() {
                            {"retrys", retrys }
                        });
                    System.Threading.Thread.Sleep(new TimeSpan(0, 0, retrys));
                    retrys++;
                    TraceWriteLine(LogEventLevel.Warning, "RETRY {Retrys}/{RetryLimit} ",
                        new Dictionary<string, object>() {
                            {"Retrys", retrys },
                            {"RetryLimit", retryLimit }
                        });
                    ProcessWorkItem(sourceWorkItem, retryLimit, retrys);
                }
                else
                {
                    TraceWriteLine(LogEventLevel.Error, "ERROR: Failed to create work item. Retry Limit reached ");
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex, ex.ToString());
                Telemetry.TrackRequest("ProcessWorkItem", starttime, witstopwatch.Elapsed, "502", false);
                throw ex;
            }
            witstopwatch.Stop();
            _elapsedms += witstopwatch.ElapsedMilliseconds;
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
            Telemetry.TrackRequest("ProcessWorkItem", starttime, witstopwatch.Elapsed, "200", true);

            _current++;
            _count--;
        }

        private void ProcessWorkItemAttachments(WorkItemData sourceWorkItem, WorkItemData targetWorkItem, bool save = true)
        {
            if (targetWorkItem != null && _config.AttachmentMigration && sourceWorkItem.ToWorkItem().Attachments.Count > 0)
            {
                TraceWriteLine(LogEventLevel.Information, "Attachemnts {SourceWorkItemAttachmentCount} | LinkMigrator:{AttachmentMigration}", new Dictionary<string, object>() { { "SourceWorkItemAttachmentCount", sourceWorkItem.ToWorkItem().Attachments.Count }, { "AttachmentMigration", _config.AttachmentMigration } });
                attachmentEnricher.ProcessAttachemnts(sourceWorkItem, targetWorkItem, save);
                AddMetric("Attachments", processWorkItemMetrics, targetWorkItem.ToWorkItem().AttachedFileCount);
            }
        }

        private void ProcessWorkItemLinks(IWorkItemMigrationClient sourceStore, IWorkItemMigrationClient targetStore, WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            if (targetWorkItem != null && _config.LinkMigration && sourceWorkItem.ToWorkItem().Links.Count > 0)
            {
                TraceWriteLine(LogEventLevel.Information, "Links {SourceWorkItemLinkCount} | LinkMigrator:{LinkMigration}", new Dictionary<string, object>() { { "SourceWorkItemLinkCount", sourceWorkItem.ToWorkItem().Links.Count }, { "LinkMigration", _config.LinkMigration } });
                workItemLinkEnricher.Enrich(sourceWorkItem, targetWorkItem);
                AddMetric("RelatedLinkCount", processWorkItemMetrics, targetWorkItem.ToWorkItem().Links.Count);
                int fixedLinkCount = gitRepositoryEnricher.Enrich(sourceWorkItem, targetWorkItem);
                AddMetric("FixedGitLinkCount", processWorkItemMetrics, fixedLinkCount);
            }
        }

        private WorkItemData ReplayRevisions(List<RevisionItem> revisionsToMigrate, WorkItemData sourceWorkItem, WorkItemData targetWorkItem, int current)
        {
            try
            {
                var skipToFinalRevisedWorkItemType = _config.SkipToFinalRevisedWorkItemType;

                string finalDestType = revisionsToMigrate.Last().Type;

                if (skipToFinalRevisedWorkItemType && Engine.TypeDefinitionMaps.Items.ContainsKey(finalDestType))
                {
                    finalDestType =
                       Engine.TypeDefinitionMaps.Items[finalDestType].Map();
                }

                //If work item hasn't been created yet, create a shell
                if (targetWorkItem == null)
                {
                    string targetType = revisionsToMigrate.First().Type;
                    if (Engine.TypeDefinitionMaps.Items.ContainsKey(targetType))
                    {
                        targetType = Engine.TypeDefinitionMaps.Items[targetType].Map();
                    }
                    targetWorkItem = CreateWorkItem_Shell(Engine.Target.WorkItems.Project, sourceWorkItem, skipToFinalRevisedWorkItemType ? finalDestType : targetType);
                }
                
                if (_config.AttachRevisionHistory)
                {
                    revisionManager.AttachSourceRevisionHistroyJsonToTarget(sourceWorkItem, targetWorkItem);
                } 

                foreach (var revision in revisionsToMigrate)
                {
                    var currentRevisionWorkItem = sourceWorkItem.GetRevision(revision.Number);

                    TraceWriteLine(LogEventLevel.Information, " Processing Revision [{RevisionNumber}]",
                        new Dictionary<string, object>() {
                            {"RevisionNumber", revision.Number }
                        });

                    // Decide on WIT
                    string destType = currentRevisionWorkItem.Type;
                    if (Engine.TypeDefinitionMaps.Items.ContainsKey(destType))
                    {
                        destType =
                           Engine.TypeDefinitionMaps.Items[destType].Map();
                    }

                    WorkItemTypeChange(targetWorkItem, skipToFinalRevisedWorkItemType, finalDestType, revision, currentRevisionWorkItem, destType);

                    PopulateWorkItem(currentRevisionWorkItem, targetWorkItem, destType);

                    // Todo: Ensure all field maps use WorkItemData.Fields to apply a correct mapping
                    Engine.FieldMaps.ApplyFieldMappings(currentRevisionWorkItem, targetWorkItem);

                    // Todo: Think about an "UpdateChangedBy" flag as this is expensive! (2s/WI instead of 1,5s when writing "Migration")
                    targetWorkItem.ToWorkItem().Fields["System.ChangedBy"].Value = currentRevisionWorkItem.Fields["System.ChangedBy"];

                    targetWorkItem.ToWorkItem().Fields["System.History"].Value = currentRevisionWorkItem.Fields["System.History"];
                    //Debug.WriteLine("Discussion:" + currentRevisionWorkItem.Revisions[revision.Index].Fields["System.History"].Value);

                    TfsReflectedWorkItemId reflectedUri = (TfsReflectedWorkItemId)Engine.Source.WorkItems.CreateReflectedWorkItemId(sourceWorkItem);
                    if (!targetWorkItem.ToWorkItem().Fields.Contains(Engine.Target.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName))
                    {
                        var ex = new InvalidOperationException("ReflectedWorkItemIDField Field Missing");
                        Log.LogError(ex, " The WorkItemType {WorkItemType} does not have a Field called {ReflectedWorkItemID}", targetWorkItem.Type, Engine.Target.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName);
                        throw ex;
                    }
                    targetWorkItem.ToWorkItem().Fields[Engine.Target.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName].Value = reflectedUri.ToString();

                    targetWorkItem.SaveToAzureDevOps();
                    TraceWriteLine(LogEventLevel.Information,
                        " Saved TargetWorkItem {TargetWorkItemId}. Replayed revision {RevisionNumber} of {RevisionsToMigrateCount}",
                       new Dictionary<string, object>() {
                               {"TargetWorkItemId", targetWorkItem.Id },
                               {"RevisionNumber", revision.Number },
                               {"RevisionsToMigrateCount",  revisionsToMigrate.Count}
                           });
                }

                if (targetWorkItem != null)
                {
                    ProcessWorkItemAttachments(sourceWorkItem, targetWorkItem, false);
                    if (!string.IsNullOrEmpty(targetWorkItem.Id))
                    { ProcessWorkItemLinks(Engine.Source.WorkItems, Engine.Target.WorkItems, sourceWorkItem, targetWorkItem); }

                    if (_config.GenerateMigrationComment)
                    {
                        var reflectedUri = targetWorkItem.ToWorkItem().Fields[Engine.Target.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName].Value;
                        var history = new StringBuilder();
                        history.Append(
                            $"This work item was migrated from a different project or organization. You can find the old version at <a href=\"{reflectedUri}\">{reflectedUri}</a>.");
                        targetWorkItem.ToWorkItem().History = history.ToString();
                    }
                    targetWorkItem.SaveToAzureDevOps();

                    attachmentEnricher.CleanUpAfterSave();
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
                        TraceWriteLine(LogEventLevel.Information, "{FieldReferenceName} ({FieldName}) | {FieldValue}", new Dictionary<string, object>() { { "FieldReferenceName", f.ReferenceName }, { "FieldName", f.Name }, { "FieldValue", f.Value } });
                }
                Log.LogInformation("===============================================================");
                Log.LogError(ex.ToString(), ex);
                Log.LogInformation("===============================================================");
            }

            return targetWorkItem;
        }

       

        private void WorkItemTypeChange(WorkItemData targetWorkItem, bool skipToFinalRevisedWorkItemType, string finalDestType, RevisionItem revision, WorkItemData currentRevisionWorkItem, string destType)
        {
            //If the work item already exists and its type has changed, update its type. Done this way because there doesn't appear to be a way to do this through the store.
            if (!skipToFinalRevisedWorkItemType && targetWorkItem.Type != finalDestType)

            {
                Debug.WriteLine($"Work Item type change! '{targetWorkItem.Title}': From {targetWorkItem.Type} to {destType}");
                var typePatch = new JsonPatchOperation()
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                    Path = "/fields/System.WorkItemType",
                    Value = destType
                };
                var datePatch = new JsonPatchOperation()
                {
                    Operation = Microsoft.VisualStudio.Services.WebApi.Patch.Operation.Add,
                    Path = "/fields/System.ChangedDate",
                    Value = currentRevisionWorkItem.ToWorkItem().Revisions[revision.Index].Fields["System.ChangedDate"].Value
                };

                var patchDoc = new JsonPatchDocument
                {
                    typePatch,
                    datePatch
                };
                _witClient.UpdateWorkItemAsync(patchDoc, int.Parse(targetWorkItem.Id), bypassRules: true).Wait();
            }
        }
    }
}