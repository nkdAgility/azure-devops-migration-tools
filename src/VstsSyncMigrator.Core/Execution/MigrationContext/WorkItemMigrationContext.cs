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
using System.Collections;
using VstsSyncMigrator.Core.Execution.OMatics;
using Microsoft.TeamFoundation.WorkItemTracking.Proxy;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemMigrationContext : MigrationContextBase
    {
        private readonly WorkItemMigrationConfig _config;
        List<String> _ignore;
        WorkItemTrackingHttpClient _witClient;
        WorkItemLinkOMatic workItemLinkOMatic = new WorkItemLinkOMatic();
        AttachmentOMatic attachmentOMatic;
        EmbededImagesRepairOMatic embededImagesRepairOMatic = new EmbededImagesRepairOMatic();
        int _current = 0;
        int _count = 0;
        int _failures = 0;
        int _imported = 0;
        int _skipped = 0;
        long _elapsedms = 0;

        public WorkItemMigrationContext(MigrationEngine me, WorkItemMigrationConfig config)
            : base(me, config)
        {
            _config = config;
            PopulateIgnoreList();

            VssClientCredentials adoCreds = new VssClientCredentials();
            _witClient = new WorkItemTrackingHttpClient(me.Target.Collection.Uri, adoCreds);

            var workItemServer = me.Source.Collection.GetService<WorkItemServer>();
            attachmentOMatic = new AttachmentOMatic(workItemServer, config.AttachmentWorkingPath);

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
            tfsqc.AddParameter("TeamProject", me.Source.Config.Name);
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
            //////////////////////////////////////////////////////////
            
            //////////////////////////////////////////////////
            _current = sourceWorkItems.Count;
            _count = 0;
            _elapsedms = 0;

            //Validation: make sure that the ReflectedWorkItemId field name specified in the config exists in the target process, preferably on each work item type.
            ConfigValidation();

            foreach (WorkItem sourceWorkItem in sourceWorkItems)
            {
                ProcessWorkItem(sourceStore, targetStore, destProject, sourceWorkItem, _config.WorkItemCreateRetryLimit);
            }
            //////////////////////////////////////////////////
            stopwatch.Stop();

            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }

        private void ProcessWorkItem(WorkItemStoreContext sourceStore, WorkItemStoreContext targetStore, Project destProject, WorkItem sourceWorkItem, int retryLimit = 5, int retrys = 0)
        {
            var witstopwatch = Stopwatch.StartNew();
            try
            {
                var targetWorkItem = targetStore.FindReflectedWorkItem(sourceWorkItem, false);
                Trace.WriteLine($"{_current} - Migrating: {sourceWorkItem.Id} - {sourceWorkItem.Type.Name}", Name);
                ///////////////////////////////////////////////
                Console.WriteLine(string.Format("STATUS: Work Item has {0} revisions and revision migration is set to {1}", sourceWorkItem.Rev, _config.ReplayRevisions));
                if (targetWorkItem == null)
                {
                    if (_config.ReplayRevisions)
                    {
                        targetWorkItem = CreateWorkItem_ReplayRevisions(sourceWorkItem, destProject, sourceStore, _current, targetStore);
                    }
                    else
                    {
                        targetWorkItem = CreateWorkItem_TipOnly(sourceWorkItem, destProject, sourceStore, _current, targetStore);
                    }

                }
                else
                {
                    Console.WriteLine("...Exists");

                }
                ///////////////////////////////////////////////
                Console.WriteLine(string.Format("...Source Work Item has {0} attachements and Attachment migration is set to {1}", sourceWorkItem.Attachments.Count, _config.AttachmentMigration));
                if (targetWorkItem != null && _config.AttachmentMigration && sourceWorkItem.Attachments.Count > 0)
                {
                    attachmentOMatic.ProcessAttachemnts(sourceWorkItem, targetWorkItem);
                }
                ///////////////////////////////////////////////
                Console.WriteLine(string.Format("...Source Work Item has {0} links and Link migration is set to {1}", sourceWorkItem.Links.Count, _config.LinkMigration));
                if (targetWorkItem != null && _config.LinkMigration && sourceWorkItem.Links.Count > 0)
                {
                    Console.WriteLine("...Processing Links");
                    workItemLinkOMatic.MigrateLinks(sourceWorkItem, sourceStore, targetWorkItem, targetStore);
                }
                ///////////////////////////////////////////////
                Console.WriteLine(string.Format("...Target Work Item may need its HTML field images fixed."));
                if (targetWorkItem != null && _config.FixHtmlAttachmentLinks)
                {
                    embededImagesRepairOMatic.FixHtmlAttachmentLinks(targetWorkItem, me.Source.Collection.Uri.ToString(), me.Target.Collection.Uri.ToString());
                }
                ///////////////////////////////////////////////
                if (targetWorkItem != null && targetWorkItem.IsDirty)
                {
                    targetWorkItem.Save();
                }
                if (targetWorkItem != null)
                {
                    targetWorkItem.Close();
                }
                if (sourceWorkItem != null)
                {
                    sourceWorkItem.Close();
                }
               
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ERROR: Failed to create work item. Will retry untill ");
                Telemetry.Current.TrackException(ex);
                Trace.WriteLine(ex.ToString());

                System.Threading.Thread.Sleep(new TimeSpan(0, 0, retrys));
                if (retrys > retryLimit)
                {
                    return;
                } else
                {
                    ProcessWorkItem(sourceStore, targetStore, destProject, sourceWorkItem);
                }
                
            }
            witstopwatch.Stop();
            _elapsedms += witstopwatch.ElapsedMilliseconds;
            _current--;
            _count++;
            var average = new TimeSpan(0, 0, 0, 0, (int)(_elapsedms / _count));
            var remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * _current));
            Trace.WriteLine(
                string.Format("Average time of {0} per work item and {1} estimated to completion",
                    string.Format(@"{0:s\:fff} seconds", average),
                    string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining)), Name);
            Trace.Flush();
        }

        /// <summary>
        /// Validate the current configuration of the both the migrator and the target project
        /// </summary>
        private void ConfigValidation()
        {
            //Make sure that the ReflectedWorkItemId field name specified in the config exists in the target process, preferably on each work item type
            var fields = _witClient.GetFieldsAsync(me.Target.Config.Name).Result;
            bool rwiidFieldExists = fields.Any(x => x.ReferenceName == me.Target.Config.ReflectedWorkItemIDFieldName || x.Name == me.Target.Config.ReflectedWorkItemIDFieldName);
            Trace.WriteLine($"Found {fields.Count.ToString("n0")} work item fields.");
            if (rwiidFieldExists)
                Trace.WriteLine($"Found '{me.Target.Config.ReflectedWorkItemIDFieldName}' in this project, proceeding.");
            else
            {
                Trace.WriteLine($"Config file specifies '{me.Target.Config.ReflectedWorkItemIDFieldName}', which wasn't found.");
                Trace.WriteLine("Instead, found:");
                foreach (var field in fields.OrderBy(x => x.Name))
                    Trace.WriteLine($"{field.Type.ToString().PadLeft(15)} - {field.Name.PadRight(20)} {field.Description ?? ""}");
                throw new Exception("Running a replay migration requires a ReflectedWorkItemId field to be defined in the target project's process.");
            }
        }

        private WorkItem CreateWorkItem_ReplayRevisions(WorkItem sourceWorkItem, Project destProject, WorkItemStoreContext sourceStore,
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
                            Number = Convert.ToInt32(x.Fields["System.Rev"].Value)
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
                            newwit = CreateWorkItem_Shell(destProject, currentRevisionWorkItem, destType);
                        }
                        //If the work item already exists and its type has changed, update its type. Done this way because there doesn't appear to be a way to do this through the store.
                        else if (newwit.Type.Name != destType)
                        {
                            Debug.WriteLine($"Work Item type change! '{newwit.Title}': From {newwit.Type.Name} to {destType}");
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
                            _witClient.UpdateWorkItemAsync(patchDoc, newwit.Id, bypassRules: true).Wait();
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
                    if (newwit.Fields.Contains(me.Target.Config.ReflectedWorkItemIDFieldName))
                    {
                        newwit.Fields["System.ChangedBy"].Value = "Migration";
                        newwit.Fields[me.Target.Config.ReflectedWorkItemIDFieldName].Value = reflectedUri;
                    }
                    var history = new StringBuilder();
                    history.Append(
                        $"This work item was migrated from a different project or organization. You can find the old version at <a href=\"{reflectedUri}\">{reflectedUri}</a>.");
                    newwit.History = history.ToString();

                    newwit.Save();
               
                    Trace.WriteLine($"...Saved as {newwit.Id}", Name);

                    if (_config.UpdateSourceReflectedId && sourceWorkItem.Fields.Contains(me.Source.Config.ReflectedWorkItemIDFieldName))
                    {
                        sourceWorkItem.Fields[me.Source.Config.ReflectedWorkItemIDFieldName].Value =
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
            return newwit;
        }

        private WorkItem CreateWorkItem_TipOnly(WorkItem sourceWorkItem, Project destProject, WorkItemStoreContext sourceStore,
            int current,
            WorkItemStoreContext targetStore)
        {
            WorkItem newwit = null;
            // Deside on WIT
            if (me.WorkItemTypeDefinitions.ContainsKey(sourceWorkItem.Type.Name))
            {
                var destType = me.WorkItemTypeDefinitions[sourceWorkItem.Type.Name].Map(sourceWorkItem);
                newwit = CreateWorkItem_Shell(destProject, sourceWorkItem, destType);
                PopulateWorkItem(sourceWorkItem, newwit, destType);

                if (newwit.Fields.Contains(me.Target.Config.ReflectedWorkItemIDFieldName))
                {
                    newwit.Fields[me.Target.Config.ReflectedWorkItemIDFieldName].Value = sourceStore.CreateReflectedWorkItemId(sourceWorkItem);
                }
                me.ApplyFieldMappings(sourceWorkItem, newwit);

                StringBuilder history = new StringBuilder();

                BuildCommentTable(sourceWorkItem, history);

                string reflectedUri = sourceStore.CreateReflectedWorkItemId(sourceWorkItem);
                history.Append(
                    $"This work item was migrated from a different project or organization. You can find the old version at <a href=\"{reflectedUri}\">{reflectedUri}</a>.");
                newwit.History = history.ToString();

                if (_config.BuildFieldTable)
                {
                    BuildFieldTable(sourceWorkItem, history);
                }

                if (_config.AppendMigrationToolSignatureFooter)
                {
                    AppendMigratedByFooter(history);
                }

                newwit.History = history.ToString();



                ArrayList fails = newwit.Validate();
                foreach (Field f in fails)
                {
                    Trace.WriteLine(string.Format("{0} - Invalid: {1}-{2}-{3}-{4} Status: {5} Value: {6}", current, sourceWorkItem.Id, sourceWorkItem.Type.Name, f.ReferenceName, sourceWorkItem.Title, f.Status, f.Value), this.Name);
                }
            }
            else
            {
                Trace.WriteLine(string.Format("...the WITD named {0} is not in the list provided in the configuration.json under WorkItemTypeDefinitions. Add it to the list to enable migration of this work item type.", sourceWorkItem.Type.Name), Name);
            }

            if (newwit != null)
            {

                try
                {
                   

                    var invalidFields = newwit.Validate();
                    if (invalidFields.Count > 0)
                    {
                        Trace.WriteLine("...FAILED validation. Invalid fields:");
                        foreach (Field invalidField in invalidFields)
                        {
                            Trace.WriteLine($" -NAME: {invalidField.Name} STATUS: {invalidField.Status} VALUE: {invalidField.Value}");
                        }

                        _failures++;
                        return null;
                    }

                    newwit.Save();
                    Trace.WriteLine(string.Format("...Saved as {0}", newwit.Id), this.Name);

                    if (me.Source.Config.ReflectedWorkItemIDFieldName != null)
                    {
                        if (sourceWorkItem.Fields.Contains(me.Source.Config.ReflectedWorkItemIDFieldName) && _config.UpdateSourceReflectedId)
                        {
                            sourceWorkItem.Fields[me.Source.Config.ReflectedWorkItemIDFieldName].Value = targetStore.CreateReflectedWorkItemId(newwit);
                        }
                        sourceWorkItem.Save();
                    }

                    Trace.WriteLine(string.Format("...and Source Updated {0}", sourceWorkItem.Id), this.Name);
                    _imported++;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("...FAILED to Save", this.Name);
                    _failures++;
                    foreach (Field f in newwit.Fields)
                    {
                        Trace.WriteLine(string.Format("{0} | {1}", f.ReferenceName, f.Value), this.Name);
                    }
                    Trace.WriteLine(ex.ToString(), this.Name);
                }
            }
            return newwit;
        }

        private WorkItem CreateWorkItem_Shell(Project destProject, WorkItem currentRevisionWorkItem, string destType)
        {
            WorkItem newwit;
            var newWorkItemstartTime = DateTime.UtcNow;
            var newWorkItemTimer = Stopwatch.StartNew();
            if (destProject.WorkItemTypes.Contains(destType))
            {
                newwit = destProject.WorkItemTypes[destType].NewWorkItem();
            } else
            {
                throw new Exception(string.Format("WARNING: Unable to find '{0}' in the target project. Most likley this is due to a typo in the .json configuration under WorkItemTypeDefinition! ", destType));
            }
            newWorkItemTimer.Stop();
            Telemetry.Current.TrackDependency("TeamService", "NewWorkItem", newWorkItemstartTime, newWorkItemTimer.Elapsed, true);
            Trace.WriteLine(
                string.Format("Dependency: {0} - {1} - {2} - {3} - {4}", "TeamService", "NewWorkItem",
                    newWorkItemstartTime, newWorkItemTimer.Elapsed, true), Name);

            if (_config.UpdateCreatedBy){  newwit.Fields["System.CreatedBy"].Value = currentRevisionWorkItem.Revisions[0].Fields["System.CreatedBy"].Value; }
            if (_config.UpdateCreatedDate) {newwit.Fields["System.CreatedDate"].Value = currentRevisionWorkItem.Revisions[0].Fields["System.CreatedDate"].Value; }
            
            return newwit;
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
                if (newwit.Fields.Contains(f.ReferenceName) && !_ignore.Contains(f.ReferenceName) && (!newwit.Fields[f.ReferenceName].IsChangedInRevision || newwit.Fields[f.ReferenceName].IsEditable))
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

        private static void AppendMigratedByFooter(StringBuilder history)
        {
            history.Append("<p>Migrated by <a href='https://dev.azure.com/nkdagility/migration-tools/'>Azure DevOps Migration Tools</a> open source.</p>");
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
