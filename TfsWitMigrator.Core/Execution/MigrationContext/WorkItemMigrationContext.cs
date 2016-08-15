using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VSTS.DataBulkEditor.Engine.Configuration.Processing;

namespace VSTS.DataBulkEditor.Engine
{
    public class WorkItemMigrationContext : MigrationContextBase
    {

        WorkItemMigrationConfig _config;
        MigrationEngine _me;

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
            Trace.WriteLine(string.Format("Migrate {0} work items?", sourceWIS.Count));
            //////////////////////////////////////////////////
            WorkItemStoreContext targetStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            Project destProject = targetStore.GetProject();
            Trace.WriteLine(string.Format("Found target project as {0}", destProject.Name));

            int current = sourceWIS.Count;
            int count = 0;
            long elapsedms = 0;
            foreach (WorkItem sourceWI in sourceWIS)
            {
                Stopwatch witstopwatch = new Stopwatch();
                witstopwatch.Start();
                WorkItem targetFound;
                targetFound = targetStore.FindReflectedWorkItem(sourceWI, me.ReflectedWorkItemIdFieldName);
                Trace.WriteLine(string.Format("{0} - Migrating: {1}-{2}", current, sourceWI.Id, sourceWI.Type.Name));
                if (targetFound == null)
                {
                    WorkItem newwit = null;
                    // Deside on WIT
                    if (me.WorkItemTypeDefinitions.ContainsKey(sourceWI.Type.Name))
                    {
                        newwit = CreateAndPopulateWorkItem(sourceWI, destProject, me.WorkItemTypeDefinitions[sourceWI.Type.Name].Map(sourceWI));
                        if (newwit.Fields.Contains(me.ReflectedWorkItemIdFieldName))
                        {
                            newwit.Fields[me.ReflectedWorkItemIdFieldName].Value = sourceStore.CreateReflectedWorkItemId(sourceWI);
                        }
                        me.ApplyFieldMappings(sourceWI, newwit);
                        ArrayList fails = newwit.Validate();
                        foreach (Field f in fails)
                        {
                            Trace.WriteLine(string.Format("{0} - Invalid: {1}-{2}-{3}", current, sourceWI.Id, sourceWI.Type.Name, f.ReferenceName));
                        }
                    }
                    else
                    {
                        Trace.WriteLine("...not supported");
                    }

                    if (newwit != null)
                    {

                        try
                        {
                            newwit.Save();
                            Trace.WriteLine(string.Format("...Saved as {0}", newwit.Id));
                            newwit.Fields["System.CreatedDate"].Value = sourceWI.Fields["System.CreatedDate"].Value;
                            Trace.WriteLine(string.Format("...And Date Created Updated"));
                            if (sourceWI.Fields.Contains(me.ReflectedWorkItemIdFieldName))
                            {
                                sourceWI.Fields[me.ReflectedWorkItemIdFieldName].Value = targetStore.CreateReflectedWorkItemId(newwit);
                            }
                            newwit.Save();

                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("...FAILED to Save");
                            foreach (Field f in newwit.Fields)
                            {
                                Trace.WriteLine(string.Format("{0} | {1}", f.ReferenceName, f.Value));
                            }
                            Trace.WriteLine(ex.ToString());
                        }
                    }
                }
                else
                {
                    Console.WriteLine("...Exists");

                    //  sourceWI.Open();
                    //  sourceWI.SyncToLatest();
                    //  sourceWI.Fields["TfsMigrationTool.ReflectedWorkItemId"].Value = destWIFound[0].Id;
                    //sourceWI.Save();
                }
                witstopwatch.Stop();
                elapsedms = elapsedms + witstopwatch.ElapsedMilliseconds;
                current--;
                count++;
                TimeSpan average = new TimeSpan(0, 0, 0, 0, (int)(elapsedms / count));
                TimeSpan remaining = new TimeSpan(0, 0, 0, 0, (int)(average.TotalMilliseconds * current));
                Trace.WriteLine(string.Format("Average time of {0} per work item and {1} estimated to completion", string.Format(@"{0:s\:fff} seconds", average), string.Format(@"{0:%h} hours {0:%m} minutes {0:s\:fff} seconds", remaining)));
            }
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Console.WriteLine(@"DONE in {0:%h} hours {0:%m} minutes {0:s\:fff} seconds", stopwatch.Elapsed);
        }


        private static bool HasChildPBI(WorkItem sourceWI)
        {
            return sourceWI.Title.ToLower().StartsWith("epic") || sourceWI.Title.ToLower().StartsWith("theme");
        }

        private static WorkItem CreateAndPopulateWorkItem(WorkItem oldWi, Project destProject, String destType)
        {
            bool except = false;
            Trace.Write("... Building");
            List<String> ignore = new List<string>();
            ignore.Add("System.CreatedDate");
            ignore.Add("System.CreatedBy");
            ignore.Add("System.Rev");
            ignore.Add("System.AreaId");
            ignore.Add("System.IterationId");
            ignore.Add("System.Id");
            ignore.Add("System.ChangedDate");
            ignore.Add("System.ChangedBy");
            ignore.Add("System.RevisedDate");
            ignore.Add("System.AttachedFileCount");
            ignore.Add("System.TeamProject");
            ignore.Add("System.NodeName");
            ignore.Add("System.RelatedLinkCount");
            ignore.Add("System.WorkItemType");
            ignore.Add("Microsoft.VSTS.Common.ActivatedDate");
            ignore.Add("Microsoft.VSTS.Common.StateChangeDate");
            ignore.Add("System.ExternalLinkCount");
            ignore.Add("System.HyperLinkCount");
            ignore.Add("System.Watermark");
            ignore.Add("System.AuthorizedDate");
            ignore.Add("System.BoardColumn");
            ignore.Add("System.BoardColumnDone");
            ignore.Add("System.BoardLane");
            ignore.Add("SLB.SWT.DateOfClientFeedback");


            // WorkItem newwit = oldWi.Copy(destProject.WorkItemTypes[destType]);
            var NewWorkItemstartTime = DateTime.UtcNow;
            Stopwatch NewWorkItemTimer = new Stopwatch();
            WorkItem newwit = destProject.WorkItemTypes[destType].NewWorkItem();
            NewWorkItemTimer.Stop();
            Telemetry.Current.TrackDependency("TeamService", "NewWorkItem", NewWorkItemstartTime, NewWorkItemTimer.Elapsed, true);
            Trace.WriteLine(string.Format("Dependnacy: {0} - {1} - {2} - {3} - {4}", "TeamService", "NewWorkItem", NewWorkItemstartTime, NewWorkItemTimer.Elapsed, true));


            newwit.Title = oldWi.Title;
            newwit.State = oldWi.State;
            switch (newwit.State)
            {
                case "Done":
                    newwit.Fields["Microsoft.VSTS.Common.ClosedDate"].Value = DateTime.Now;
                    break;
                case "Closed":
                    newwit.Fields["Microsoft.VSTS.Common.ClosedDate"].Value = DateTime.Now;
                    break;
                default:
                    break;
            }
            newwit.Reason = oldWi.Reason;
            foreach (Field f in oldWi.Fields)
            {
                if (newwit.Fields.Contains(f.ReferenceName) && !ignore.Contains(f.ReferenceName))
                {
                    newwit.Fields[f.ReferenceName].Value = oldWi.Fields[f.ReferenceName].Value;
                }
            }
            newwit.AreaPath = string.Format(@"{0}\{1}", newwit.Project.Name, oldWi.AreaPath);
            newwit.IterationPath = string.Format(@"{0}\{1}", newwit.Project.Name, oldWi.IterationPath);
            newwit.Fields["System.ChangedDate"].Value = oldWi.Fields["System.ChangedDate"].Value;

            switch (destType)
            {
                case "Test Case":
                    newwit.Fields["Microsoft.VSTS.TCM.Steps"].Value = oldWi.Fields["Microsoft.VSTS.TCM.Steps"].Value;
                    newwit.Fields["Microsoft.VSTS.Common.Priority"].Value = oldWi.Fields["Microsoft.VSTS.Common.Priority"].Value;
                    break;
                //case "User Story":
                //newwit.Fields["COMPANY.DEVISION.Analysis"].Value = oldWi.Fields["COMPANY.PRODUCT.AcceptanceCriteria"].Value;
                //break;
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
            description.AppendLine();
            description.AppendLine();
            description.AppendFormat("##REF##{0}##", oldWi.Id);
            newwit.Description = description.ToString();

            StringBuilder history = new StringBuilder();
            BuildCommentTable(oldWi, history);
            BuildFieldTable(oldWi, history);
            history.Append("<p>Migrated by <a href='http://nkdagility.com'>naked Agility Limited's</a> open source <a href='https://github.com/nkdAgility/VstsMigrator'>VSTS/TFS Migrator</a>.</p>");
            newwit.History = history.ToString();

            if (except)
            {
                Trace.Write("...buildErrors");
                System.Threading.Thread.Sleep(1000);

            }
            else
            {
                Trace.Write("...buildComplete");
            }

            return newwit;
        }

        private static string ReplaceFirstOccurence(string wordToReplace, string replaceWith, string input)
        {
            Regex r = new Regex(wordToReplace, RegexOptions.IgnoreCase);
            return r.Replace(input, replaceWith, 1);
        }


        private static void BuildFieldTable(WorkItem oldWi, StringBuilder history, bool useHTML = false)
        {
            history.Append("<p>&nbsp;</p>");
            if (useHTML) {
                history.Append("<table border='1' cellpadding='2' style='width:100%;border-color:#C0C0C0;'><tr><td><b>Field</b></td><td><b>Value</b></td></tr>");
            }
            foreach (Field f in oldWi.Fields)
            {
                if (f.Value == null)
                {
                    if (useHTML)
                    {
                        history.AppendFormat("<tr><td style='text-align:right;white-space:nowrap;'><b>{0}</b></td><td>n/a</td></tr>", f.Name);
                    } else
                    {
                        history.AppendLine(string.Format("{0}: null<br />", f.Name));
                    }
                }
                else
                {
                    if (useHTML)
                    {
                        history.AppendFormat("<tr><td style='text-align:right;white-space:nowrap;'><b>{0}</b></td><td style='width:100%'>{1}</td></tr>", f.Name, f.Value.ToString());

                    }else
                    {
                        history.AppendLine(string.Format("{0}: {1}<br />", f.Name, f.Value.ToString()));
                    }
                }

            }
            if (useHTML)
            {
                history.Append("</table>");
            }
            history.Append("<p>&nbsp;</p>");
        }

        private static void BuildCommentTable(WorkItem oldWi, StringBuilder history)
        {
            history.Append("<p>&nbsp;</p>");
            history.Append("<table border='1' style='width:100%;border-color:#C0C0C0;'>");
            foreach (Revision r in oldWi.Revisions)
            {
                if (r.Fields["System.History"].Value != "" && r.Fields["System.ChangedBy"].Value != "Martin Hinshelwood (Adm)")
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