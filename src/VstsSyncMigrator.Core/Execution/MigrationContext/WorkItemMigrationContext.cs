using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Proxy;
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
using Serilog.Context;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemMigrationContext : MigrationProcessorBase
    {
        private const string RegexPatternForAreaAndIterationPathsFix = "\\[?(?<key>System.AreaPath|System.IterationPath)+\\]?[^']*'(?<value>[^']*(?:''.[^']*)*)'";

        private static int _count = 0;
        private static int _current = 0;
        private static long _elapsedms = 0;
        private static int _totalWorkItem = 0;
        private static string workItemLogTemplate = "[{sourceWorkItemTypeName,20}][Complete:{currentWorkItem,6}/{totalWorkItems}][sid:{sourceWorkItemId,6}|Rev:{sourceRevisionInt,3}][tid:{targetWorkItemId,6} | ";
        private WorkItemMigrationConfig _config;
        private List<string> _ignore;
        private Dictionary<string, Tuple<WorkItemData, WorkItemData>> _processedWorkItems = new Dictionary<string, Tuple<WorkItemData, WorkItemData>>();  

        private ILogger contextLog;
        private IAttachmentMigrationEnricher attachmentEnricher;
        private IWorkItemProcessorEnricher embededImagesEnricher;
        private IWorkItemProcessorEnricher _workItemEmbededLinkEnricher;
        private TfsGitRepositoryEnricher gitRepositoryEnricher;
        private TfsNodeStructure _nodeStructureEnricher;
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
                                        TfsNodeStructure nodeStructureEnricher,
                                        TfsRevisionManager revisionManager,
                                        TfsWorkItemLinkEnricher workItemLinkEnricher,
                                        TfsWorkItemEmbededLinkEnricher workItemEmbeddedLinkEnricher,
                                        TfsValidateRequiredField requiredFieldValidator,
                                        IOptions<EngineConfiguration> engineConfig)
            : base(engine, services, telemetry, logger)
        {
            _engineConfig = engineConfig.Value;
            contextLog = Serilog.Log.ForContext<WorkItemMigrationContext>();
            _nodeStructureEnricher = nodeStructureEnricher;
            _revisionManager = revisionManager;
            _workItemLinkEnricher = workItemLinkEnricher;
            _workItemEmbededLinkEnricher = workItemEmbeddedLinkEnricher;
            _validateConfig = requiredFieldValidator;
        }

        public override string Name => "WorkItemMigration";

        public int _delayBetweenWI = 0;

        public override void Configure(IProcessorConfig config)
        {
            _config = (WorkItemMigrationConfig)config;

            if (_config.UseCommonNodeStructureEnricherConfig)
            {
                var nseConfig = _engineConfig.CommonEnrichersConfig.OfType<TfsNodeStructureOptions>()
                                    .FirstOrDefault() ??
                                throw new InvalidOperationException(
                                    "Cannot use common node structure because it is not found.");
                _nodeStructureEnricher.Configure(nseConfig);
            }
            else
            {
                _nodeStructureEnricher.Configure(new TfsNodeStructureOptions
                {
                    Enabled = _config.NodeStructureEnricherEnabled ?? true,
                    NodeBasePaths = _config.NodeBasePaths,
                    PrefixProjectToNodes = _config.PrefixProjectToNodes,
                    AreaMaps = _config.AreaMaps ?? new Dictionary<string, string>(),
                    IterationMaps = _config.IterationMaps ?? new Dictionary<string, string>(),
                });
            }

            _revisionManager.Configure(new TfsRevisionManagerOptions() { Enabled = true, MaxRevisions = _config.MaxRevisions, ReplayRevisions = _config.ReplayRevisions });

            _workItemLinkEnricher.Configure(_config.LinkMigrationSaveEachAsAdded, _config.FilterWorkItemsThatAlreadyExistInTarget);
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
            Log.LogInformation("WorkItemMigrationContext::InternalExecute ");
            if (_config == null)
            {
                throw new Exception("You must call Configure() first");
            }

            var workItemServer = Engine.Source.GetService<WorkItemServer>();
            attachmentEnricher = new TfsAttachmentEnricher(workItemServer, _config.AttachmentWorkingPath, _config.AttachmentMaxSize);
            embededImagesEnricher = Services.GetRequiredService<TfsEmbededImagesEnricher>();
            gitRepositoryEnricher = Services.GetRequiredService<TfsGitRepositoryEnricher>();

            _nodeStructureEnricher.ProcessorExecutionBegin(null);

            var stopwatch = Stopwatch.StartNew();
            _itemsInError = new List<string>();

            try
            {
                //Validation: make sure that the ReflectedWorkItemId field name specified in the config exists in the target process, preferably on each work item type.
                PopulateIgnoreList();

                string sourceQuery =
                    string.Format(
                        @"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY {1}",
                        _config.WIQLQueryBit, _config.WIQLOrderBit);

                // Inform the user that he maybe has to be patient now
                contextLog.Information("Querying items to be migrated: {SourceQuery} ...", sourceQuery);
                var sourceWorkItems = Engine.Source.WorkItems.GetWorkItems(sourceQuery);
                contextLog.Information("Replay all revisions of {sourceWorkItemsCount} work items?",
                    sourceWorkItems.Count);

                //////////////////////////////////////////////////
                if (!_nodeStructureEnricher.ValidateTargetNodesExist(sourceWorkItems))
                {
                    if (_config.StopMigrationOnMissingAreaIterationNodes)
                    {
                        throw new Exception("Missing Iterations in Target preventing progress, check log for list. If you resolve with a FieldMap set StopMigrationOnMissingAreaIterationNodes = false in the config to continue.");
                    }                    
                }
                //////////////////////////////////////////////////
                contextLog.Information("Found target project as {@destProject}", Engine.Target.WorkItems.Project.Name);
                //////////////////////////////////////////////////////////FilterCompletedByQuery
                if (_config.FilterWorkItemsThatAlreadyExistInTarget)
                {
                    contextLog.Information(
                        "[FilterWorkItemsThatAlreadyExistInTarget] is enabled. Searching for work items that have already been migrated to the target...",
                        sourceWorkItems.Count());

                    string targetWIQLQueryBit = FixAreaPathAndIterationPathForTargetQuery(_config.WIQLQueryBit,
                        Engine.Source.WorkItems.Project.Name, Engine.Target.WorkItems.Project.Name, contextLog);
                    sourceWorkItems = ((TfsWorkItemMigrationClient)Engine.Target.WorkItems).FilterExistingWorkItems(
                        sourceWorkItems,
                        new TfsWiqlDefinition() { OrderBit = _config.WIQLOrderBit, QueryBit = targetWIQLQueryBit },
                        (TfsWorkItemMigrationClient)Engine.Source.WorkItems);
                    contextLog.Information(
                        "!! After removing all found work items there are {SourceWorkItemCount} remaining to be migrated.",
                        sourceWorkItems.Count());
                }
                //////////////////////////////////////////////////

                var result = _validateConfig.ValidatingRequiredField(
                    Engine.Target.Config.AsTeamProjectConfig().ReflectedWorkItemIDFieldName, sourceWorkItems);
                if (!result)
                {
                    var ex = new InvalidFieldValueException(
                        "Not all work items in scope contain a valid ReflectedWorkItemId Field!");
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

					if (_current % 30 == 0) sendemail(Engine.Source.WorkItems.Project.Name, Engine.Target.Config.AsTeamProjectConfig().WhoToEmail);

					Log.LogInformation("Sleeping for " + _delayBetweenWI);
					Thread.Sleep(_delayBetweenWI);
					Log.LogInformation("Sleeping over!");
				}

				// fix embedded links in html fields
				foreach (var k in _processedWorkItems.Keys)
				{
					try
					{
						_processedWorkItems[k].Item2.ToWorkItem().Open();
						_processedWorkItems[k].Item1.ToWorkItem().Open();
						ProcessWorkItemEmbeddedLinks(_processedWorkItems[k].Item1, _processedWorkItems[k].Item2);

						if (_processedWorkItems[k].Item2.ToWorkItem().IsDirty)
						{
							_processedWorkItems[k].Item2.SaveToAzureDevOps();
							Thread.Sleep(150);
						}

						_processedWorkItems[k].Item2.ToWorkItem().Close();
						_processedWorkItems[k].Item1.ToWorkItem().Close();
						//MigrateInlineLinks.MigrateFor(_targetWorkItems.Values.ToArray());
					}
					catch (Exception ex)
					{
						contextLog.Error("Error for " + k + ". Will continue with next WI.", ex);
					}
				}

				var creds = new Microsoft.VisualStudio.Services.Common.VssBasicCredential(string.Empty, Engine.Target.Config.AsTeamProjectConfig().PersonalAccessToken);
				var connection = new Microsoft.VisualStudio.Services.WebApi.VssConnection(Engine.Target.Config.AsTeamProjectConfig().Collection, creds);
				var targetGuid = ((Project)Engine.Target.WorkItems.GetProject().internalObject).Guid;
				using (var witClient = connection.GetClient<Microsoft.TeamFoundation.WorkItemTracking.WebApi.WorkItemTrackingHttpClient>())
				{
					// fix embedded links in History
					foreach (var k in _processedWorkItems.Keys)
					{
						var wid = int.Parse(_processedWorkItems[k].Item2.Id);

						try
						{
							var res = witClient.GetCommentsAsync(targetGuid, wid).Result;

							foreach (var c in res.Comments)
							{
								var text = c.Text;
								var newText = workItemEmbededLinkEnricher.FixLinks(text);

								if (newText != text)
								{
									contextLog.Information($"Fixing comment for wi {wid}");
									var commentUpdate = new Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.CommentUpdate { Text = newText };
									var ddd = witClient.UpdateCommentAsync(commentUpdate, targetGuid, wid, c.Id).Result;
								}
							}
						}
						catch (Exception ex)
						{
							contextLog.Error("Error for " + wid + ". Will continue with next WI.", ex);
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

        internal void sendemail(string projName, string who)
        {
            try
            {
                var req = WebRequest.Create($"https://andyemailer.azurewebsites.net/api/Emailer?code=2gQ24cKqsicIcrjBAP7n8zz8EhEwnJ4ucTQtBdjBRjKZAzFue8QLcw==&to={who}&subject=Done {projName} - {_current} of {_totalWorkItem}&body=Nothing");
                WebResponse myWebResponse = req.GetResponse();
                myWebResponse.Close();
            }
            catch (Exception ex)
            {
                contextLog.Error("Error sending email..", ex);
            }
        }

        internal static string FixAreaPathAndIterationPathForTargetQuery(string sourceWIQLQueryBit, string sourceProject, string targetProject, ILogger? contextLog)
        {

            string targetWIQLQueryBit = sourceWIQLQueryBit;

            if (string.IsNullOrWhiteSpace(targetWIQLQueryBit))
            {
                return targetWIQLQueryBit;
            }

            var matches = Regex.Matches(targetWIQLQueryBit, RegexPatternForAreaAndIterationPathsFix);


            if (string.IsNullOrWhiteSpace(sourceProject)
                || string.IsNullOrWhiteSpace(targetProject)
                || sourceProject == targetProject)
            {
                return targetWIQLQueryBit;
            }

            foreach (Match match in matches)
            {
                var value = match.Groups["value"].Value;
                if (string.IsNullOrWhiteSpace(value) || !value.StartsWith(sourceProject))
                    continue;

                var fieldType = match.Groups["key"].Value;
                TfsNodeStructureType structureType;
                switch (fieldType)
                {
                    case "System.AreaPath":
                        structureType = TfsNodeStructureType.Area;
                        break;
                    case "System.IterationPath":
                        structureType = TfsNodeStructureType.Iteration;
                        break;
                    default:
                        throw new InvalidOperationException($"Field type {fieldType} is not supported for query remapping.");
                }

                var remappedPath = _nodeStructureEnricher.GetNewNodeName(value, structureType);
                targetWIQLQueryBit = targetWIQLQueryBit.Replace(value, remappedPath);
            }

            contextLog?.Information("[FilterWorkItemsThatAlreadyExistInTarget] is enabled. Source project {sourceProject} is replaced with target project {targetProject} on the WIQLQueryBit which resulted into this target WIQLQueryBit \"{targetWIQLQueryBit}\" .", sourceProject, targetProject, targetWIQLQueryBit);

            return targetWIQLQueryBit;
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
        private void PopulateWorkItem(WorkItemData oldWi, WorkItemData newwit, string destType)
        {
            var oldWorkItem = oldWi.ToWorkItem();
            var newWorkItem = newwit.ToWorkItem();
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
                if (newWorkItem.Fields.Contains(f.ReferenceName) == false)
                {
                    var missedMigratedValue = oldWorkItem.Fields[f.ReferenceName].Value;
                    if (missedMigratedValue != null && !string.Empty.Equals(missedMigratedValue))
                    {
                        Log.LogDebug("PopulateWorkItem:FieldUpdate: Missing field in target workitem, Source WorkItemId: {WorkitemId}, Field: {MissingField}, Value: {SourceValue}", oldWi.Id, f.ReferenceName, missedMigratedValue);
                    }
                    continue;
                }
                if (!_ignore.Contains(f.ReferenceName) &&
                    (!newWorkItem.Fields[f.ReferenceName].IsChangedInRevision || newWorkItem.Fields[f.ReferenceName].IsEditable)
                    && oldWorkItem.Fields[f.ReferenceName].Value != newWorkItem.Fields[f.ReferenceName].Value)
                {
                    Log.LogDebug("PopulateWorkItem:FieldUpdate: {ReferenceName} | Old:{OldReferenceValue} New:{NewReferenceValue}", f.ReferenceName, oldWorkItem.Fields[f.ReferenceName].Value, newWorkItem.Fields[f.ReferenceName].Value);
                    newWorkItem.Fields[f.ReferenceName].Value = oldWorkItem.Fields[f.ReferenceName].Value;
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
                            { "ReplayRevisions", _config.ReplayRevisions }}
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

                        if (!_processedWorkItems.ContainsKey(targetWorkItem.Id))
                        {
                            _processedWorkItems.Add(targetWorkItem.Id, new Tuple<WorkItemData, WorkItemData>(sourceWorkItem,targetWorkItem));
                        }
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
                _workItemLinkEnricher.Enrich(sourceWorkItem, targetWorkItem);
                AddMetric("RelatedLinkCount", processWorkItemMetrics, targetWorkItem.ToWorkItem().Links.Count);
                int fixedLinkCount = gitRepositoryEnricher.Enrich(sourceWorkItem, targetWorkItem);
                AddMetric("FixedGitLinkCount", processWorkItemMetrics, fixedLinkCount);
            }
        }

        private WorkItemData ReplayRevisions(List<RevisionItem> revisionsToMigrate, WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            try
            {
                //If work item hasn't been created yet, create a shell
                if (targetWorkItem == null)
                {
                    var skipToFinalRevisedWorkItemType = _config.SkipToFinalRevisedWorkItemType;
                    var finalDestType = revisionsToMigrate.Last().Type;
                    var targetType = revisionsToMigrate.First().Type;

                    if (targetType != finalDestType)
                    {
                        TraceWriteLine(LogEventLevel.Information, $"WorkItem has changed type at one of the revisions, from {targetType} to {finalDestType}");
                    }

                    if (skipToFinalRevisedWorkItemType && Engine.TypeDefinitionMaps.Items.ContainsKey(finalDestType))
                    {
                        finalDestType = Engine.TypeDefinitionMaps.Items[finalDestType].Map();
                    }

                    if (Engine.TypeDefinitionMaps.Items.ContainsKey(targetType))
                    {
                        targetType = Engine.TypeDefinitionMaps.Items[targetType].Map();
                    }
                    targetWorkItem = CreateWorkItem_Shell(Engine.Target.WorkItems.Project, sourceWorkItem, skipToFinalRevisedWorkItemType ? finalDestType : targetType);
                }

                if (_config.AttachRevisionHistory)
                {
                    _revisionManager.AttachSourceRevisionHistoryJsonToTarget(sourceWorkItem, targetWorkItem);
                }

                foreach (var revision in revisionsToMigrate)
                {
                    Log.LogInformation("Sleeping for " + _delayBetweenWI/2);
                    Thread.Sleep(_delayBetweenWI/2);
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

                    PopulateWorkItem(currentRevisionWorkItem, targetWorkItem, destType);

                    // Impersonate revision author. Mapping will apply later and may change this.
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

                    var skipRevision = SkipRevisionWithInvalidIterationPath(targetWorkItem);

                    if (!skipRevision)
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

        private bool SkipRevisionWithInvalidIterationPath(WorkItemData targetWorkItemData)
        {
            if (!_config.SkipRevisionWithInvalidIterationPath)
            {
                return false;
            }

            var workItem = targetWorkItemData.ToWorkItem();
            var invalidFields = workItem.Validate();

            if (invalidFields.Count == 0)
            {
                return false;
            }

            foreach (Field invalidField in invalidFields)
            {
                // We cannot save a revision when it has no IterationPath
                if (invalidField.ReferenceName == "System.IterationPath")
                {
                    return true;
                }
            }

            return false;
        }
    }
}