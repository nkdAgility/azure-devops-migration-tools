using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Proxy;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using MigrationTools;
using MigrationTools.Clients;
using MigrationTools.Clients.AzureDevops.ObjectModel;
using MigrationTools.Clients.AzureDevops.ObjectModel.Enrichers;
using MigrationTools.Configuration;
using MigrationTools.Configuration.Processing;
using MigrationTools.DataContracts;
using MigrationTools.Engine.Processors;
using MigrationTools.Enrichers;
using Newtonsoft.Json;
using Serilog.Context;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace VstsSyncMigrator.Engine
{
    public class NodeDetecomatic
    {
        private ICommonStructureService _commonStructure;
        private List<string> _foundNodes = new List<string>();

        public NodeDetecomatic(WorkItemStore store)
        {
            if (_commonStructure == null)
            {
                _commonStructure = (ICommonStructureService4)store.TeamProjectCollection.GetService(typeof(ICommonStructureService4));
            }
        }

        public bool NodeExists(string nodePath)
        {
            if (!_foundNodes.Contains(nodePath))
            {
                NodeInfo node = null;
                try
                {
                    node = _commonStructure.GetNodeFromPath(nodePath);
                }
                catch
                {
                    return false;
                }
                _foundNodes.Add(nodePath);
            }
            return true;
        }
    }

    public class WorkItemMigrationContext : MigrationProcessorBase
    {
        private static int _count = 0;
        private static int _current = 0;
        private static long _elapsedms = 0;
        private static int _totalWorkItem = 0;
        private static string workItemLogTeamplate = "[{sourceWorkItemTypeName,20}][Complete:{currentWorkItem,6}/{totalWorkItems}][sid:{sourceWorkItemId,6}|Rev:{sourceRevisionInt,3}][tid:{targetWorkItemId,6} | ";
        private WorkItemMigrationConfig _config;
        private List<String> _ignore;
        private NodeDetecomatic _nodeOMatic;
        private WorkItemTrackingHttpClient _witClient;
        private IAttachmentMigrationEnricher attachmentEnricher;
        private ILogger contextLog;
        private IWorkItemEnricher embededImagesEnricher;
        private GitRepositoryEnricher gitRepositoryEnricher;
        private NodeStructureEnricher nodeStructureEnricher;
        private IDictionary<string, double> processWorkItemMetrics = null;
        private IDictionary<string, string> processWorkItemParamiters = null;
        private WorkItemLinkEnricher workItemLinkEnricher;
        private ILogger workItemLog;

        public WorkItemMigrationContext(IMigrationEngine engine, IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemMigrationContext> logger) : base(engine, services, telemetry, logger)
        {
            contextLog = Serilog.Log.ForContext<WorkItemMigrationContext>();
        }

        public override string Name => "WorkItemMigration";

        public override void Configure(IProcessorConfig config)
        {
            _config = (WorkItemMigrationConfig)config;
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
            Log.LogInformation("Starting ");
            if (_config == null)
            {
                throw new Exception("You must call Configure() first");
            }
            var workItemServer = Engine.Source.GetService<WorkItemServer>();
            workItemLinkEnricher = Services.GetRequiredService<WorkItemLinkEnricher>();
            attachmentEnricher = new AttachmentMigrationEnricher(workItemServer, _config.AttachmentWorkingPath, _config.AttachmentMaxSize);
            embededImagesEnricher = Services.GetRequiredService<EmbededImagesRepairEnricher>();
            gitRepositoryEnricher = Services.GetRequiredService<GitRepositoryEnricher>();
            nodeStructureEnricher = Services.GetRequiredService<NodeStructureEnricher>();
            VssClientCredentials adoCreds = new VssClientCredentials();
            _witClient = new WorkItemTrackingHttpClient(Engine.Target.Config.Collection, adoCreds);
            //Validation: make sure that the ReflectedWorkItemId field name specified in the config exists in the target process, preferably on each work item type.
            ConfigValidation();
            PopulateIgnoreList();

            nodeStructureEnricher.MigrateAllNodeStructures(_config.PrefixProjectToNodes, _config.NodeBasePaths);

            var stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            string sourceQuery =
                string.Format(
                    @"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY {1}",
                    _config.WIQLQueryBit, _config.WIQLOrderBit);
            var sourceWorkItems = Engine.Source.WorkItems.GetWorkItems(sourceQuery);
            contextLog.Information("Replay all revisions of {sourceWorkItemsCount} work items?", sourceWorkItems.Count);
            //////////////////////////////////////////////////
            contextLog.Information("Found target project as {@destProject}", Engine.Target.WorkItems.Project.Name);
            //////////////////////////////////////////////////////////FilterCompletedByQuery
            if (_config.FilterWorkItemsThatAlreadyExistInTarget)
            {
                contextLog.Information("[FilterWorkItemsThatAlreadyExistInTarget] is enabled. Searching for work items that have already been migrated to the target...", sourceWorkItems.Count());
                sourceWorkItems = FilterByTarget(sourceWorkItems);
                contextLog.Information("!! After removing all found work items there are {SourceWorkItemCount} remaining to be migrated.", sourceWorkItems.Count());
            }
            //////////////////////////////////////////////////
            _current = 1;
            _count = sourceWorkItems.Count;
            _elapsedms = 0;
            _totalWorkItem = sourceWorkItems.Count;
            foreach (WorkItemData sourceWorkItemData in sourceWorkItems)
            {
                var sourceWorkItem = sourceWorkItemData.ToWorkItem();
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

        /// <summary>
        /// Validate the current configuration of the both the migrator and the target project
        /// </summary>
        private void ConfigValidation()
        {
            //Make sure that the ReflectedWorkItemId field name specified in the config exists in the target process, preferably on each work item type
            var fields = _witClient.GetFieldsAsync(Engine.Target.Config.Project).Result;
            bool rwiidFieldExists = fields.Any(x => x.ReferenceName == Engine.Target.Config.ReflectedWorkItemIDFieldName || x.Name == Engine.Target.Config.ReflectedWorkItemIDFieldName);
            contextLog.Information("Found {FieldsFoundCount} work item fields.", fields.Count.ToString("n0"));
            if (rwiidFieldExists)
                contextLog.Information("Found '{ReflectedWorkItemIDFieldName}' in this project, proceeding.", Engine.Target.Config.ReflectedWorkItemIDFieldName);
            else
            {
                contextLog.Information("Config file specifies '{ReflectedWorkItemIDFieldName}', which wasn't found.", Engine.Target.Config.ReflectedWorkItemIDFieldName);
                contextLog.Information("Instead, found:");
                foreach (var field in fields.OrderBy(x => x.Name))
                    contextLog.Information("{FieldType} - {FieldName} - {FieldRefName}", field.Type.ToString().PadLeft(15), field.Name.PadRight(20), field.ReferenceName ?? "");
                throw new Exception("Running a replay migration requires a ReflectedWorkItemId field to be defined in the target project's process.");
            }
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
            Telemetry.TrackDependency(new DependencyTelemetry("TeamService", Engine.Target.Config.Collection.ToString(), "NewWorkItem", null, newWorkItemstartTime, newWorkItemTimer.Elapsed, "200", true));
            if (_config.UpdateCreatedBy) { newwit.Fields["System.CreatedBy"].Value = currentRevisionWorkItem.ToWorkItem().Revisions[0].Fields["System.CreatedBy"].Value; }
            if (_config.UpdateCreatedDate) { newwit.Fields["System.CreatedDate"].Value = currentRevisionWorkItem.ToWorkItem().Revisions[0].Fields["System.CreatedDate"].Value; }

            return newwit.AsWorkItemData();
        }

        private List<WorkItemData> FilterByTarget(List<WorkItemData> sourceWorkItems)
        {
            contextLog.Debug("FilterByTarget: START");
            var targetQuery =
                string.Format(
                    @"SELECT [System.Id], [{0}] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {1} ORDER BY {2}",
                     Engine.Target.Config.ReflectedWorkItemIDFieldName,
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

        private string GetNewNodeName(string oldNodeName, string oldProjectName, string newProjectName, WorkItemStore newStore, string nodePath)
        {
            if (_nodeOMatic == null)
            {
                _nodeOMatic = new NodeDetecomatic(newStore);
            }

            // Replace project name with new name (if necessary) and inject nodePath (Area or Iteration) into path for node validation
            string newNodeName = "";
            if (_config.PrefixProjectToNodes)
            {
                newNodeName = $@"{newProjectName}\{nodePath}\{oldNodeName}";
            }
            else
            {
                var regex = new Regex(Regex.Escape(oldProjectName));
                if (oldNodeName.StartsWith($@"{oldProjectName}\{nodePath}\"))
                {
                    newNodeName = regex.Replace(oldNodeName, newProjectName, 1);
                }
                else
                {
                    newNodeName = regex.Replace(oldNodeName, $@"{newProjectName}\{nodePath}", 1);
                }
            }

            // Validate the node exists
            if (!_nodeOMatic.NodeExists(newNodeName))
            {
                contextLog.Warning("The Node '{newNodeName}' does not exist, leaving as '{newProjectName}'. This may be because it has been renamed or moved and no longer exists, or that you have not migrateed the Node Structure yet.", newNodeName, newProjectName);
                newNodeName = newProjectName;
            }

            // Remove nodePath (Area or Iteration) from path for correct population in work item
            if (newNodeName.StartsWith(newProjectName + '\\' + nodePath + '\\'))
            {
                return newNodeName.Remove(newNodeName.IndexOf($@"{nodePath}\"), $@"{nodePath}\".Length);
            }
            else if (newNodeName.StartsWith(newProjectName + '\\' + nodePath))
            {
                return newNodeName.Remove(newNodeName.IndexOf($@"{nodePath}"), $@"{nodePath}".Length);
            }
            else
            {
                return newNodeName;
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
                if (newWorkItem.Fields.Contains(f.ReferenceName) && !_ignore.Contains(f.ReferenceName) && (!newWorkItem.Fields[f.ReferenceName].IsChangedInRevision || newWorkItem.Fields[f.ReferenceName].IsEditable))
                {
                    newWorkItem.Fields[f.ReferenceName].Value = oldWorkItem.Fields[f.ReferenceName].Value;
                }
            }

            newWorkItem.AreaPath = GetNewNodeName(oldWorkItem.AreaPath, oldWorkItem.Project.Name, newWorkItem.Project.Name, newWorkItem.Store, "Area");
            newWorkItem.IterationPath = GetNewNodeName(oldWorkItem.IterationPath, oldWorkItem.Project.Name, newWorkItem.Project.Name, newWorkItem.Store, "Iteration");
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
            // Trace.WriteLine(
            //    $"FieldMapOnNewWorkItem: {newWorkItemstartTime} - {fieldMappingTimer.Elapsed.ToString("c")}", Name);
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
            AddParameter("SourceURL", processWorkItemParamiters, Engine.Source.WorkItems.Config.Collection.ToString());
            AddParameter("SourceWorkItem", processWorkItemParamiters, sourceWorkItem.Id.ToString());
            AddParameter("TargetURL", processWorkItemParamiters, Engine.Target.WorkItems.Config.Collection.ToString());
            AddParameter("TargetProject", processWorkItemParamiters, Engine.Target.WorkItems.Project.Name);
            AddParameter("RetryLimit", processWorkItemParamiters, retryLimit.ToString());
            AddParameter("RetryNumber", processWorkItemParamiters, retrys.ToString());

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
                    List<RevisionItem> revisionsToMigrate = RevisionsToMigrate(sourceWorkItem, targetWorkItem);
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
            Trace.Flush();
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

                var last = Engine.Source.WorkItems.GetRevision(sourceWorkItem, revisionsToMigrate.Last().Number);

                string finalDestType = last.Type;

                if (skipToFinalRevisedWorkItemType && Engine.TypeDefinitionMaps.Items.ContainsKey(finalDestType))
                {
                    finalDestType =
                       Engine.TypeDefinitionMaps.Items[finalDestType].Map();
                }

                //If work item hasn't been created yet, create a shell
                if (targetWorkItem == null)
                {
                    targetWorkItem = CreateWorkItem_Shell(Engine.Target.WorkItems.Project, sourceWorkItem, skipToFinalRevisedWorkItemType ? finalDestType : Engine.Source.WorkItems.GetRevision(sourceWorkItem, revisionsToMigrate.First().Number).Type);
                }

                if (_config.CollapseRevisions)
                {
                    var data = revisionsToMigrate.Select(rev => Engine.Source.WorkItems.GetRevision(sourceWorkItem, rev.Number)).Select(rev => new
                    {
                        rev.Id,
                        rev.Rev,
                        rev.RevisedDate,
                        Fields = rev.ToWorkItem().Fields.AsDictionary()
                    });

                    var fileData = JsonConvert.SerializeObject(data, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.None });
                    var filePath = Path.Combine(Path.GetTempPath(), $"{sourceWorkItem.Id}_PreMigrationHistory.json");

                    File.WriteAllText(filePath, fileData);
                    targetWorkItem.ToWorkItem().Attachments.Add(new Attachment(filePath, "History has been consolidated into the attached file."));

                    revisionsToMigrate = revisionsToMigrate.GetRange(revisionsToMigrate.Count - 1, 1);

                    TraceWriteLine(LogEventLevel.Information, " Attached a consolidated set of {RevisionCount} revisions.",
                        new Dictionary<string, object>() {
                            {"RevisionCount", data.Count() }
                        });
                }

                foreach (var revision in revisionsToMigrate)
                {
                    var currentRevisionWorkItem = Engine.Source.WorkItems.GetRevision(sourceWorkItem, revision.Number);

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
                    Engine.FieldMaps.ApplyFieldMappings(currentRevisionWorkItem, targetWorkItem);

                    targetWorkItem.ToWorkItem().Fields["System.ChangedBy"].Value =
                        currentRevisionWorkItem.ToWorkItem().Revisions[revision.Index].Fields["System.ChangedBy"].Value;

                    targetWorkItem.ToWorkItem().Fields["System.History"].Value =
                        currentRevisionWorkItem.ToWorkItem().Revisions[revision.Index].Fields["System.History"].Value;
                    //Debug.WriteLine("Discussion:" + currentRevisionWorkItem.Revisions[revision.Index].Fields["System.History"].Value);

                    var fails = targetWorkItem.ToWorkItem().Validate();

                    foreach (Field f in fails)
                    {
                        TraceWriteLine(LogEventLevel.Information,
                            "{Current} - Invalid: {CurrentRevisionWorkItemId}-{CurrentRevisionWorkItemTypeName}-{FieldReferenceName}-{SourceWorkItemTitle} Value: {FieldValue}",
                           new Dictionary<string, object>() {
                               {"Current", current },
                               {"CurrentRevisionWorkItemId", currentRevisionWorkItem.Id },
                               {"CurrentRevisionWorkItemTypeName",  currentRevisionWorkItem.Type},
                               {"FieldReferenceName", f.ReferenceName },
                               {"SourceWorkItemTitle", sourceWorkItem.Title },
                               {"FieldValue", f.Value }
                           });
                    }
                    //if (fails.Count > 0) // TODO Do we want to stop or continue?
                    //{
                    //    contextLog.Error("Unable to save revission due to the error in validation");
                    //    break;
                    //}
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
                    ProcessWorkItemLinks(Engine.Source.WorkItems, Engine.Target.WorkItems, sourceWorkItem, targetWorkItem);
                    string reflectedUri = Engine.Source.WorkItems.CreateReflectedWorkItemId(sourceWorkItem);
                    if (targetWorkItem.ToWorkItem().Fields.Contains(Engine.Target.Config.ReflectedWorkItemIDFieldName))
                    {
                        targetWorkItem.ToWorkItem().Fields[Engine.Target.Config.ReflectedWorkItemIDFieldName].Value = reflectedUri;
                    }
                    var history = new StringBuilder();
                    history.Append(
                        $"This work item was migrated from a different project or organization. You can find the old version at <a href=\"{reflectedUri}\">{reflectedUri}</a>.");
                    targetWorkItem.ToWorkItem().History = history.ToString();
                    targetWorkItem.SaveToAzureDevOps();

                    attachmentEnricher.CleanUpAfterSave();
                    TraceWriteLine(LogEventLevel.Information, "...Saved as {TargetWorkItemId}", new Dictionary<string, object> { { "TargetWorkItemId", targetWorkItem.Id } });
                }
            }
            catch (Exception ex)
            {
                TraceWriteLine(LogEventLevel.Information, "...FAILED to Save");

                if (targetWorkItem != null)
                {
                    foreach (Field f in targetWorkItem.ToWorkItem().Fields)
                        TraceWriteLine(LogEventLevel.Information, "{FieldReferenceName} ({FieldName}) | {FieldValue}", new Dictionary<string, object>() { { "FieldReferenceName", f.ReferenceName }, { "FieldName", f.Name }, { "FieldValue", f.Value } });
                }
                Log.LogError(ex.ToString(), ex);
            }

            return targetWorkItem;
        }

        private List<RevisionItem> RevisionsToMigrate(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            // just to make sure, we replay the events in the same order as they appeared
            // maybe, the Revisions collection is not sorted according to the actual Revision number
            List<RevisionItem> sortedRevisions = null;
            sortedRevisions = sourceWorkItem.ToWorkItem().Revisions.Cast<Revision>()
                    .Select(x => new RevisionItem
                    {
                        Index = x.Index,
                        Number = Convert.ToInt32(x.Fields["System.Rev"].Value),
                        ChangedDate = Convert.ToDateTime(x.Fields["System.ChangedDate"].Value)
                    })
                    .ToList();

            if (targetWorkItem != null)
            {
                // Target exists so remove any Changed Date matches bwtween them
                var targetChangedDates = (from Revision x in targetWorkItem.ToWorkItem().Revisions select Convert.ToDateTime(x.Fields["System.ChangedDate"].Value)).ToList();
                if (_config.ReplayRevisions)
                {
                    sortedRevisions = sortedRevisions.Where(x => !targetChangedDates.Contains(x.ChangedDate)).ToList();
                }
                // Find Max target date and remove all source revisions that are newer
                var targetLatestDate = targetChangedDates.Max();
                sortedRevisions = sortedRevisions.Where(x => x.ChangedDate > targetLatestDate).ToList();
            }

            sortedRevisions = sortedRevisions.OrderBy(x => x.Number).ToList();
            if (!_config.ReplayRevisions && sortedRevisions.Count > 0)
            {
                // Remove all but the latest revision if we are not replaying reviss=ions
                sortedRevisions.RemoveRange(0, sortedRevisions.Count - 1);
            }

            TraceWriteLine(LogEventLevel.Information, "Found {RevisionsCount} revisions to migrate on  Work item:{sourceWorkItemId}",
                new Dictionary<string, object>() {
                    {"RevisionsCount", sortedRevisions.Count},
                    {"sourceWorkItemId", sourceWorkItem.Id}
                });
            return sortedRevisions;
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