using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using VstsSyncMigrator.Engine.Configuration.Processing;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemRevisionReplayMigrationContext : MigrationContextBase
    {
        private readonly WorkItemRevisionReplayMigrationConfig _config;
        List<String> _ignore;
		WorkItemTrackingHttpClient witClient;

        public WorkItemRevisionReplayMigrationContext(MigrationEngine me, WorkItemRevisionReplayMigrationConfig config)
            : base(me, config)
        {
            _config = config;
            PopulateIgnoreList();

			VssClientCredentials adoCreds = new VssClientCredentials();
			witClient = new WorkItemTrackingHttpClient(me.Target.Collection.Uri, adoCreds);
        }

        private void PopulateIgnoreList()
        {
            _ignore = new List<string>
            {
                "System.Rev",
                "System.AreaId",
                "System.IterationId",
                "System.Id",
                "System.RevisedDate",
                "System.AuthorizedAs",
                "System.AttachedFileCount",
                "System.TeamProject",
                "System.NodeName",
                "System.RelatedLinkCount",
                "System.WorkItemType",
                "Microsoft.VSTS.Common.ActivatedDate",
                "Microsoft.VSTS.Common.ActivatedBy",
                "Microsoft.VSTS.Common.ResolvedDate",
                "Microsoft.VSTS.Common.ResolvedBy",
                "Microsoft.VSTS.Common.ClosedDate",
                "Microsoft.VSTS.Common.ClosedBy",
                "Microsoft.VSTS.Common.StateChangeDate",
                "System.ExternalLinkCount",
                "System.HyperLinkCount",
                "System.Watermark",
                "System.AuthorizedDate",
                "System.BoardColumn",
                "System.BoardColumnDone",
                "System.BoardLane",
                "SLB.SWT.DateOfClientFeedback",
                "System.CommentCount"
            };
        }

        public override string Name => "WorkItemRevisionReplayMigrationContext";

        internal override void InternalExecute()
        {
			var stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            var sourceStore = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.BypassRules);
            var tfsqc = new TfsQueryContext(sourceStore);
            tfsqc.AddParameter("TeamProject", me.Source.Name);
            tfsqc.Query =
                string.Format(
                    @"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.ChangedDate] desc",
                    _config.QueryBit);
            var sourceWorkItems = tfsqc.Execute();
            Trace.WriteLine($"Replay all revisions of {sourceWorkItems.Count} work items?", Name);

            //////////////////////////////////////////////////
            var targetStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            var destProject = targetStore.GetProject();
            Trace.WriteLine($"Found target project as {destProject.Name}", Name);

            var current = sourceWorkItems.Count;
            var count = 0;
            long elapsedms = 0;

            foreach (WorkItem sourceWorkItem in sourceWorkItems)
            {
                var witstopwatch = Stopwatch.StartNew();
                var targetFound = targetStore.FindReflectedWorkItem(sourceWorkItem, me.ReflectedWorkItemIdFieldName, false);
                Trace.WriteLine($"{current} - Migrating: {sourceWorkItem.Id} - {sourceWorkItem.Type.Name}", Name);

                if (targetFound == null)
                {
                    ReplayRevisions(sourceWorkItem, destProject, sourceStore, current, targetStore);
                }
                else
                {
                    Console.WriteLine("...Exists");
                }

                sourceWorkItem.Close();
                witstopwatch.Stop();
                elapsedms += witstopwatch.ElapsedMilliseconds;
                current--;
                count++;
                var average = new TimeSpan(0, 0, 0, 0, (int) (elapsedms / count));
                var remaining = new TimeSpan(0, 0, 0, 0, (int) (average.TotalMilliseconds * current));
                Trace.WriteLine(
                    string.Format("Average time of {0} per work item and {1} estimated to completion",
                        string.Format(@"{0:s\:fff} seconds", average),
                        string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining)), Name);
                Trace.Flush();
            }
            //////////////////////////////////////////////////
            stopwatch.Stop();

            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }

        private void ReplayRevisions(WorkItem sourceWorkItem, Project destProject, WorkItemStoreContext sourceStore,
            int current,
            WorkItemStoreContext targetStore)
        {
            WorkItem newwit = null;
            try
            {
                // just to make sure, we replay the events in the same order as they appeared
                // maybe, the Revisions collection is not sorted according to the actual Revision number
                var sortedRevisions = sourceWorkItem.Revisions.Cast<Revision>().Select(x =>
                        new
                        {
                            x.Index,
                            Number =  Convert.ToInt32(x.Fields["System.Rev"].Value)
                        }
                    )
                    .OrderBy(x => x.Number)
                    .ToList();

                Trace.WriteLine($"...Replaying {sourceWorkItem.Revisions.Count} revisions of work item {sourceWorkItem.Id}", Name);

                foreach (var revision in sortedRevisions)
                {
                    var currentRevisionWorkItem = sourceStore.GetRevision(sourceWorkItem, revision.Number);

                    // Decide on WIT
                    if (me.WorkItemTypeDefinitions.ContainsKey(currentRevisionWorkItem.Type.Name))
                    {
                        var destType =
                            me.WorkItemTypeDefinitions[currentRevisionWorkItem.Type.Name].Map(currentRevisionWorkItem);
						//If work item hasn't been created yet, create a shell
						if (newwit == null)
                        {
                            var newWorkItemstartTime = DateTime.UtcNow;
                            var newWorkItemTimer = Stopwatch.StartNew();
                            newwit = destProject.WorkItemTypes[destType].NewWorkItem();
                            newWorkItemTimer.Stop();
                            Telemetry.Current.TrackDependency("TeamService", "NewWorkItem", newWorkItemstartTime, newWorkItemTimer.Elapsed, true);
                            Trace.WriteLine(
                                string.Format("Dependency: {0} - {1} - {2} - {3} - {4}", "TeamService", "NewWorkItem",
                                    newWorkItemstartTime, newWorkItemTimer.Elapsed, true), Name);

                            newwit.Fields["System.CreatedBy"].Value = currentRevisionWorkItem.Revisions[0].Fields["System.CreatedBy"].Value;
                            newwit.Fields["System.CreatedDate"].Value = currentRevisionWorkItem.Revisions[0].Fields["System.CreatedDate"].Value;
                        }
						
						//If the work item already exists and its type has changed, update its type. Done this way because there doesn't appear to be a way to do this through the store.
						else if (newwit.Type.Name != destType)
						{
							Debug.WriteLine($"TYPE CHANGE: {newwit.Type.Name} to {destType}");
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
								Value = currentRevisionWorkItem.Revisions[revision.Index].Fields["System.ChangedDate"].Value
							};

							var patchDoc = new JsonPatchDocument();
							patchDoc.Add(typePatch);
							patchDoc.Add(datePatch);
							witClient.UpdateWorkItemAsync(patchDoc, newwit.Id,bypassRules:true).Wait();
						}

						PopulateWorkItem(currentRevisionWorkItem, newwit, destType);
                        me.ApplyFieldMappings(currentRevisionWorkItem, newwit);

                        newwit.Fields["System.ChangedBy"].Value =
                            currentRevisionWorkItem.Revisions[revision.Index].Fields["System.ChangedBy"].Value;

                        newwit.Fields["System.History"].Value =
                            currentRevisionWorkItem.Revisions[revision.Index].Fields["System.History"].Value;
						Debug.WriteLine("Discussion:" + currentRevisionWorkItem.Revisions[revision.Index].Fields["System.History"].Value);


                        var fails = newwit.Validate();

                        foreach (Field f in fails)
                        {
                            Trace.WriteLine(
                                $"{current} - Invalid: {currentRevisionWorkItem.Id}-{currentRevisionWorkItem.Type.Name}-{f.ReferenceName}-{sourceWorkItem.Title} Value: {f.Value}", Name);
                        }
						
                        newwit.Save();
                        Trace.WriteLine(
                            $" ...Saved as {newwit.Id}. Replayed revision {revision.Number} of {sourceWorkItem.Revisions.Count}",
                            Name);
					}
                    else
                    {
                        Trace.WriteLine(string.Format("...the WITD named {0} is not in the list provided in the configuration.json under WorkItemTypeDefinitions. Add it to the list to enable migration of this work item type.", currentRevisionWorkItem.Type.Name), Name);
                        break;
                    }
                }

                if (newwit != null)
                {
					string reflectedUri = sourceStore.CreateReflectedWorkItemId(sourceWorkItem);
					if (newwit.Fields.Contains(me.ReflectedWorkItemIdFieldName))
                    {
                        newwit.Fields["System.ChangedBy"].Value = "Migration";
                        newwit.Fields[me.ReflectedWorkItemIdFieldName].Value = reflectedUri;
                    }
                    var history = new StringBuilder();
                    history.Append(
						$"This work item was migrated from a different project or organization. You can find the old version at <a href=\"{reflectedUri}\">{reflectedUri}</a>.");
                    newwit.History = history.ToString();

                    newwit.Save();
                    newwit.Close();
                    Trace.WriteLine($"...Saved as {newwit.Id}", Name);

                    if (_config.UpdateSourceReflectedId && sourceWorkItem.Fields.Contains(me.SourceReflectedWorkItemIdFieldName))
                    {
                        sourceWorkItem.Fields[me.SourceReflectedWorkItemIdFieldName].Value =
                            targetStore.CreateReflectedWorkItemId(newwit);
                        sourceWorkItem.Save();
                        Trace.WriteLine($"...and Source Updated {sourceWorkItem.Id}", Name);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("...FAILED to Save", Name);

                if (newwit != null)
                {
                    foreach (Field f in newwit.Fields)
                        Trace.WriteLine($"{f.ReferenceName} ({f.Name}) | {f.Value}", Name);
                }
                Trace.WriteLine(ex.ToString(), Name);
            }
        }

        private void PopulateWorkItem(WorkItem oldWi, WorkItem newwit, string destType)
        {
            var newWorkItemstartTime = DateTime.UtcNow;
            var fieldMappingTimer = Stopwatch.StartNew();
            
            Trace.Write("... Building ", Name);
            
            newwit.Title = oldWi.Title;
            newwit.State = oldWi.State;
            newwit.Reason = oldWi.Reason;

            foreach (Field f in oldWi.Fields)
            {
                if (newwit.Fields.Contains(f.ReferenceName) && !_ignore.Contains(f.ReferenceName))
                {
                    newwit.Fields[f.ReferenceName].Value = oldWi.Fields[f.ReferenceName].Value;
                }
            }

            newwit.AreaPath = GetNewNodeName(oldWi.AreaPath, oldWi.Project.Name, newwit.Project.Name, newwit.Store, "Area");
            newwit.IterationPath = GetNewNodeName(oldWi.IterationPath, oldWi.Project.Name, newwit.Project.Name, newwit.Store, "Iteration");
            switch (destType)
            {
                case "Test Case":
                    newwit.Fields["Microsoft.VSTS.TCM.Steps"].Value = oldWi.Fields["Microsoft.VSTS.TCM.Steps"].Value;
                    newwit.Fields["Microsoft.VSTS.Common.Priority"].Value =
                        oldWi.Fields["Microsoft.VSTS.Common.Priority"].Value;
                    break;
            }
            
            if (newwit.Fields.Contains("Microsoft.VSTS.Common.BacklogPriority")
                && newwit.Fields["Microsoft.VSTS.Common.BacklogPriority"].Value != null
                && !IsNumeric(newwit.Fields["Microsoft.VSTS.Common.BacklogPriority"].Value.ToString(),
                    NumberStyles.Any))
                newwit.Fields["Microsoft.VSTS.Common.BacklogPriority"].Value = 10;

            var description = new StringBuilder();
            description.Append(oldWi.Description);
            newwit.Description = description.ToString();

            Trace.WriteLine("...build complete", Name);
            fieldMappingTimer.Stop();
            Telemetry.Current.TrackMetric("FieldMappingTime", fieldMappingTimer.ElapsedMilliseconds);
            Trace.WriteLine(
                $"FieldMapOnNewWorkItem: {newWorkItemstartTime} - {fieldMappingTimer.Elapsed.ToString("c")}", Name);
        }

        NodeDetecomatic _nodeOMatic;

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
            } else
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
                Trace.WriteLine(string.Format("The Node '{0}' does not exist, leaving as '{1}'. This may be because it has been renamed or moved and no longer exists, or that you have not migrateed the Node Structure yet.", newNodeName, newProjectName));
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


        private static bool IsNumeric(string val, NumberStyles numberStyle)
        {
            double result;
            return double.TryParse(val, numberStyle,
                CultureInfo.CurrentCulture, out result);
        }

    }

    public class NodeDetecomatic
    {
        ICommonStructureService _commonStructure;
        List<string> _foundNodes = new List<string>();
        WorkItemStore _store;

        public NodeDetecomatic(WorkItemStore store)
        {
            _store = store;
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
}
