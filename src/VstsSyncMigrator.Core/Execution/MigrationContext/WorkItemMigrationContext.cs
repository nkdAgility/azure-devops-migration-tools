using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemMigrationContext : MigrationContextBase
    {

        WorkItemMigrationConfig _config;
        MigrationEngine _me;
        List<String> _ignore;

        public override string Name
        {
            get
            {
                return "WorkItemMigrationContext";
            }
        }

        public WorkItemMigrationContext(MigrationEngine me, WorkItemMigrationConfig config) : base(me, config)
        {
            _me = me;
            _config = config;
            PopulateIgnoreList();
        }

        private void PopulateIgnoreList()
        {
           _ignore = new List<string>();
            //ignore.Add("System.CreatedDate");
            //ignore.Add("System.CreatedBy");
            _ignore.Add("System.Rev");
            _ignore.Add("System.AreaId");
            _ignore.Add("System.IterationId");
            _ignore.Add("System.Id");
            //ignore.Add("System.ChangedDate");
            //ignore.Add("System.ChangedBy");
            _ignore.Add("System.RevisedDate");
            _ignore.Add("System.AttachedFileCount");
            _ignore.Add("System.TeamProject");
            _ignore.Add("System.NodeName");
            _ignore.Add("System.RelatedLinkCount");
            _ignore.Add("System.WorkItemType");
            _ignore.Add("Microsoft.VSTS.Common.ActivatedDate");
            _ignore.Add("Microsoft.VSTS.Common.StateChangeDate");
            _ignore.Add("System.ExternalLinkCount");
            _ignore.Add("System.HyperLinkCount");
            _ignore.Add("System.Watermark");
            _ignore.Add("System.AuthorizedDate");
            _ignore.Add("System.BoardColumn");
            _ignore.Add("System.BoardColumnDone");
            _ignore.Add("System.BoardLane");
            _ignore.Add("SLB.SWT.DateOfClientFeedback");
        }

        internal override void InternalExecute()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //////////////////////////////////////////////////
            WorkItemStoreContext sourceStore = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.BypassRules);
            TfsQueryContext tfsqc = new TfsQueryContext(sourceStore);
            tfsqc.AddParameter("TeamProject", me.Source.Name);
            tfsqc.Query = string.Format(@"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.ChangedDate] desc", _config.QueryBit);
            WorkItemCollection sourceWIS = tfsqc.Execute();
            Trace.WriteLine(string.Format("Migrate {0} work items?", sourceWIS.Count),this.Name);
            //////////////////////////////////////////////////
            WorkItemStoreContext targetStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            Project destProject = targetStore.GetProject();
            Trace.WriteLine(string.Format("Found target project as {0}", destProject.Name), this.Name);

            int current = sourceWIS.Count;
            int count = 0;
            int failures = 0;
            int imported = 0;
            int skipped = 0;
            long elapsedms = 0;
            foreach (WorkItem sourceWI in sourceWIS)
            {
                Stopwatch witstopwatch = new Stopwatch();
                witstopwatch.Start();
                WorkItem targetFound;
                targetFound = targetStore.FindReflectedWorkItem(sourceWI, me.ReflectedWorkItemIdFieldName, false);
                Trace.WriteLine(string.Format("{0} - Migrating: {1}-{2}", current, sourceWI.Id, sourceWI.Type.Name), this.Name);
                if (targetFound == null)
                {
                    WorkItem newwit = null;
                    // Deside on WIT
                    if (me.WorkItemTypeDefinitions.ContainsKey(sourceWI.Type.Name))
                    {
                        newwit = CreateAndPopulateWorkItem(_config,sourceWI, destProject, me.WorkItemTypeDefinitions[sourceWI.Type.Name].Map(sourceWI));
                        if (newwit.Fields.Contains(me.ReflectedWorkItemIdFieldName))
                        {
                            newwit.Fields[me.ReflectedWorkItemIdFieldName].Value = sourceStore.CreateReflectedWorkItemId(sourceWI);
                        }
                        me.ApplyFieldMappings(sourceWI, newwit);
                        ArrayList fails = newwit.Validate();
                        foreach (Field f in fails)
                        {
                            Trace.WriteLine(string.Format("{0} - Invalid: {1}-{2}-{3}", current, sourceWI.Id, sourceWI.Type.Name, f.ReferenceName), this.Name);
                        }
                    }
                    else
                    {
                        Trace.WriteLine("...not supported", this.Name);
                        skipped++;
                    }

                    if (newwit != null)
                    {

                        try
                        {
                            if (_config.UpdateCreatedDate) { newwit.Fields["System.CreatedDate"].Value = sourceWI.Fields["System.CreatedDate"].Value; }
                            if (_config.UpdateCreatedBy) { newwit.Fields["System.CreatedBy"].Value = sourceWI.Fields["System.CreatedBy"].Value; }
                            newwit.Save();
                            newwit.Close();
                            Trace.WriteLine(string.Format("...Saved as {0}", newwit.Id), this.Name);
                            if (sourceWI.Fields.Contains(me.ReflectedWorkItemIdFieldName) && _config.UpdateSoureReflectedId)
                            {
                                sourceWI.Fields[me.ReflectedWorkItemIdFieldName].Value = targetStore.CreateReflectedWorkItemId(newwit);
                            }
                            sourceWI.Save();
                            Trace.WriteLine(string.Format("...and Source Updated {0}", sourceWI.Id), this.Name);
                            imported++;
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("...FAILED to Save", this.Name);
                            failures++;
                            foreach (Field f in newwit.Fields)
                            {
                                Trace.WriteLine(string.Format("{0} | {1}", f.ReferenceName, f.Value), this.Name);
                            }
                            Trace.WriteLine(ex.ToString(), this.Name);
                        }
                    }
                }
                else
                {
                    Trace.WriteLine("...Exists", this.Name);
                    skipped++;
                    //  sourceWI.Open();
                    //  sourceWI.SyncToLatest();
                    //  sourceWI.Fields["TfsMigrationTool.ReflectedWorkItemId"].Value = destWIFound[0].Id;
                    //sourceWI.Save();
                }
                sourceWI.Close();
                witstopwatch.Stop();
                elapsedms = elapsedms + witstopwatch.ElapsedMilliseconds;
                current--;
                count++;
                TimeSpan average = new TimeSpan(0, 0, 0, 0, (int)(elapsedms / count));
                TimeSpan remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * current));
                Trace.WriteLine(string.Format("Average time of {0} per work item and {1} estimated to completion", string.Format(@"{0:s\:fff} seconds", average), string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining)), this.Name);
                Trace.Flush();
            }
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Trace.WriteLine(string.Format(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds - {1} Items, {2} Imported, {3} Skipped, {4} Failures", stopwatch.Elapsed, sourceWIS.Count, imported, skipped, failures), this.Name);
        }


        private static bool HasChildPBI(WorkItem sourceWI)
        {
            return sourceWI.Title.ToLower().StartsWith("epic") || sourceWI.Title.ToLower().StartsWith("theme");
        }

        private WorkItem CreateAndPopulateWorkItem(WorkItemMigrationConfig config , WorkItem oldWi, Project destProject, String destType)
        {
            Stopwatch fieldMappingTimer = new Stopwatch();

            bool except = false;
            Trace.Write("... Building", "WorkItemMigrationContext");
          
            var NewWorkItemstartTime = DateTime.UtcNow;
            Stopwatch NewWorkItemTimer = new Stopwatch();
            WorkItem newwit = destProject.WorkItemTypes[destType].NewWorkItem();
            NewWorkItemTimer.Stop();
            Telemetry.Current.TrackDependency("TeamService", "NewWorkItem", NewWorkItemstartTime, NewWorkItemTimer.Elapsed, true);
            Trace.WriteLine(string.Format("Dependancy: {0} - {1} - {2} - {3} - {4}", "TeamService", "NewWorkItem", NewWorkItemstartTime, NewWorkItemTimer.Elapsed, true), "WorkItemMigrationContext");
            newwit.Title = oldWi.Title;
            newwit.State = oldWi.State;
            newwit.Reason = oldWi.Reason;
            
            foreach (Field f in oldWi.Fields)
            {
                if (newwit.Fields.Contains(f.ReferenceName) && !_ignore.Contains(f.ReferenceName) && newwit.Fields[f.ReferenceName].IsEditable)
                {
                    newwit.Fields[f.ReferenceName].Value = oldWi.Fields[f.ReferenceName].Value;
                }
            }

            if (config.PrefixProjectToNodes)
            {
                newwit.AreaPath = string.Format(@"{0}\{1}", newwit.Project.Name, oldWi.AreaPath);
                newwit.IterationPath = string.Format(@"{0}\{1}", newwit.Project.Name, oldWi.IterationPath);
            }
            else
            {
                var regex = new Regex(Regex.Escape(oldWi.Project.Name));
                newwit.AreaPath = regex.Replace(oldWi.AreaPath, newwit.Project.Name, 1);
                newwit.IterationPath = regex.Replace(oldWi.IterationPath, newwit.Project.Name, 1);
            }
            
            newwit.Fields["System.ChangedDate"].Value = oldWi.Fields["System.ChangedDate"].Value;


            switch (destType)
            {
                case "Test Case":
                    newwit.Fields["Microsoft.VSTS.TCM.Steps"].Value = oldWi.Fields["Microsoft.VSTS.TCM.Steps"].Value;
                    newwit.Fields["Microsoft.VSTS.Common.Priority"].Value = oldWi.Fields["Microsoft.VSTS.Common.Priority"].Value;
                    break;
                default:
                    break;
            }



            if (newwit.Fields.Contains("Microsoft.VSTS.Common.BacklogPriority")
                && newwit.Fields["Microsoft.VSTS.Common.BacklogPriority"].Value != null
                && !isNumeric(newwit.Fields["Microsoft.VSTS.Common.BacklogPriority"].Value.ToString(),
                NumberStyles.Any))
                {
                    newwit.Fields["Microsoft.VSTS.Common.BacklogPriority"].Value = 10;
                }

            StringBuilder description = new StringBuilder();
            description.Append(oldWi.Description);
            newwit.Description = description.ToString();

            StringBuilder history = new StringBuilder();
            BuildCommentTable(oldWi, history);
            BuildFieldTable(oldWi, history);
            history.Append("<p>Migrated by <a href='http://nkdagility.com'>naked Agility Limited's</a> open source <a href='https://github.com/nkdAgility/VstsMigrator'>VSTS/TFS Migrator</a>.</p>");
            newwit.History = history.ToString();

            if (except)
            {
                Trace.WriteLine("...buildErrors", "WorkItemMigrationContext");
                System.Threading.Thread.Sleep(1000);

            }
            else
            {
                Trace.WriteLine("...buildComplete", "WorkItemMigrationContext");
            }
            fieldMappingTimer.Stop();
            Telemetry.Current.TrackMetric( "FieldMappingTime", fieldMappingTimer.ElapsedMilliseconds);
            Trace.WriteLine(string.Format("FieldMapOnNewWorkItem: {0} - {1}", NewWorkItemstartTime, fieldMappingTimer.Elapsed.ToString("c")), "WorkItemMigrationContext");
            return newwit;
        }

        private static string ReplaceFirstOccurence(string wordToReplace, string replaceWith, string input)
        {
            Regex r = new Regex(wordToReplace, RegexOptions.IgnoreCase);
            return r.Replace(input, replaceWith, 1);
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

        static bool isNumeric(string val, NumberStyles NumberStyle)
        {
            Double result;
            return Double.TryParse(val, NumberStyle,
                System.Globalization.CultureInfo.CurrentCulture, out result);
        }


    }
}