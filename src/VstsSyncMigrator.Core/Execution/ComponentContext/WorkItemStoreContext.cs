using System;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemStoreContext
    {
        private WorkItemStoreFlags bypassRules;
        private ITeamProjectContext targetTfs;
        private WorkItemStore wistore;
        private Dictionary<int, WorkItem> foundWis;

        public WorkItemStore Store { get { return wistore; }}

        public WorkItemStoreContext(ITeamProjectContext targetTfs, WorkItemStoreFlags bypassRules)
        {
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            this.targetTfs = targetTfs;
            this.bypassRules = bypassRules;
            try
            {
                wistore = new WorkItemStore(targetTfs.Collection, bypassRules);
                timer.Stop();
                Telemetry.Current.TrackDependency("TeamService", "GetWorkItemStore", startTime, timer.Elapsed, true);
            }
            catch (Exception ex)
            {
                timer.Stop();
                Telemetry.Current.TrackDependency("TeamService", "GetWorkItemStore", startTime, timer.Elapsed, false);
                Telemetry.Current.TrackException(ex,
                       new Dictionary<string, string> {
                            { "CollectionUrl", targetTfs.Collection.Uri.ToString() }
                       },
                       new Dictionary<string, double> {
                            { "Time",timer.ElapsedMilliseconds }
                       });
                Trace.TraceWarning(string.Format("  [EXCEPTION] {0}", ex.Message));
                Trace.TraceWarning(string.Format("  [EXCEPTION] {0}", ex.StackTrace));
                throw ex;
            }
           
            foundWis = new Dictionary<int, WorkItem>();
        }

        public Project GetProject()
        {
            return (from Project x in wistore.Projects where x.Name == targetTfs.Name select x).SingleOrDefault();
        }

        public string CreateReflectedWorkItemId(WorkItem wi)
        {
            return string.Format("{0}/{1}/{2}", wi.Store.TeamProjectCollection.Uri, wi.Project.Name, wi.Id);
        }
        public int GetReflectedWorkItemId(WorkItem wi, string reflectedWotkItemIdField)
        {
            string rwiid = wi.Fields[reflectedWotkItemIdField].Value.ToString();
            if (Regex.IsMatch(rwiid, @"(http(s)?://)?([\w-]+\.)+[\w-]+(/[\w- ;,./?%&=]*)?"))
            {
                return int.Parse(rwiid.Substring(rwiid.LastIndexOf(@"/") + 1));
            }
            return 0;
        }

        public WorkItem FindReflectedWorkItem(WorkItem workItemToFind, string reflectedWotkItemIdField, bool cache)
        {
            string ReflectedWorkItemId = CreateReflectedWorkItemId(workItemToFind);
            WorkItem found = null;
            if (foundWis.ContainsKey(workItemToFind.Id)) {
                return foundWis[workItemToFind.Id];
            }
            try {
                if (workItemToFind.Fields.Contains(reflectedWotkItemIdField) && !string.IsNullOrEmpty(workItemToFind.Fields[reflectedWotkItemIdField].Value.ToString())) {
                    string rwiid = workItemToFind.Fields[reflectedWotkItemIdField].Value.ToString();
                    int idToFind = GetReflectedWorkItemId(workItemToFind, reflectedWotkItemIdField);
                    if (idToFind == 0) {
                        found = null;
                    } else {
                        found = Store.GetWorkItem(idToFind);
                        if (!(found.Fields[reflectedWotkItemIdField].Value.ToString() == rwiid)) {
                            found = null;
                        }
                    }
                }
                if (found == null) { found = FindReflectedWorkItemByReflectedWorkItemId(ReflectedWorkItemId, reflectedWotkItemIdField); }
                if (!workItemToFind.Fields.Contains(reflectedWotkItemIdField)) {
                    if (found == null) { found = FindReflectedWorkItemByMigrationRef(ReflectedWorkItemId); } // Too slow!
                }
            } catch (Exception) {
                // Theo: Exception if reflectedWotkItemIdField does not exists (shared steps) -> search based in title and created date
                found = FindReflectedWorkItemByTitleAndCreatedDate(workItemToFind.Title, workItemToFind.CreatedDate);
            }
            if (found != null && cache) 
            {
                foundWis.Add(workItemToFind.Id, found); /// TODO MENORY LEEK! LEAK
            }
            return found;
        }

        public WorkItem FindReflectedWorkItemByReflectedWorkItemId(WorkItem refWi, string reflectedWotkItemIdField)
        {
            return FindReflectedWorkItemByReflectedWorkItemId(CreateReflectedWorkItemId(refWi), reflectedWotkItemIdField);
        }

        public WorkItem FindReflectedWorkItemByReflectedWorkItemId(string refId, string reflectedWotkItemIdField)
        {
            TfsQueryContext query = new TfsQueryContext(this);
            query.Query = string.Format(@"SELECT [System.Id] FROM WorkItems  WHERE [System.TeamProject]=@TeamProject AND [{0}] = @idToFind", reflectedWotkItemIdField);
            query.AddParameter("idToFind", refId.ToString());
            query.AddParameter("TeamProject", this.targetTfs.Name);
            return FindWorkItemByQuery(query);
        }

        public WorkItem FindReflectedWorkItemByMigrationRef(string refId)
        {
            TfsQueryContext query = new TfsQueryContext(this);
            query.Query = @"SELECT [System.Id] FROM WorkItems  WHERE [System.TeamProject]=@TeamProject AND [System.Description] Contains @KeyToFind";
            query.AddParameter("KeyToFind", string.Format("##REF##{0}##", refId));
            query.AddParameter("TeamProject", this.targetTfs.Name);
            return FindWorkItemByQuery(query);
        }

        public WorkItem FindReflectedWorkItemByTitle(string title)
        {
            TfsQueryContext query = new TfsQueryContext(this);
            query.Query = @"SELECT [System.Id] FROM WorkItems  WHERE [System.TeamProject]=@TeamProject AND [System.Title] = @TitleToFind";
            query.AddParameter("TitleToFind", title);
            query.AddParameter("TeamProject", this.targetTfs.Name);
            return FindWorkItemByQuery(query);
        }

        public WorkItem FindReflectedWorkItemByTitleAndCreatedDate(string title, DateTime CreatedDate) {
            TfsQueryContext query = new TfsQueryContext(this);
            query.Query = @"SELECT [System.Id] FROM WorkItems  WHERE [System.TeamProject]=@TeamProject AND [System.Title] = @TitleToFind";
            query.AddParameter("TitleToFind", title);
            query.AddParameter("TeamProject", this.targetTfs.Name);
            return FindWorkItemByQueryCreatedDate(query,CreatedDate);
        }
        

        public WorkItem FindWorkItemByQueryCreatedDate(TfsQueryContext query,DateTime CreatedDate)
        {
            WorkItemCollection newFound;
            newFound = query.Execute();
            if (newFound.Count == 0) { return null; }
            foreach(WorkItem wiFound in newFound) {
                if(wiFound.CreatedDate == CreatedDate) {
                    return wiFound;
                }
            }
            return null;
        }

        public WorkItem FindWorkItemByQuery(TfsQueryContext query) {
            WorkItemCollection newFound;
            newFound = query.Execute();
            if (newFound.Count == 0) {
                return null;
            }
            return newFound[0];
        }

    }
}