using System;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.TeamFoundation.Server;
using System.Text;

namespace VstsSyncMigrator.Engine
{
    public class WorkItemStoreContext
    {
        private WorkItemStoreFlags bypassRules;
        private ITeamProjectContext teamProjectContext;
        private WorkItemStore wistore;
        private Dictionary<int, WorkItem> foundWis;

        public WorkItemStore Store { get { return wistore; }}

        public WorkItemStoreContext(ITeamProjectContext teamProjectContext, WorkItemStoreFlags bypassRules)
        {
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            this.teamProjectContext = teamProjectContext;
            this.bypassRules = bypassRules;
            try
            {
                wistore = new WorkItemStore(teamProjectContext.Collection, bypassRules);
                timer.Stop();
                Telemetry.Current.TrackDependency("TeamService", "GetWorkItemStore", startTime, timer.Elapsed, true);
            }
            catch (Exception ex)
            {
                timer.Stop();
                Telemetry.Current.TrackDependency("TeamService", "GetWorkItemStore", startTime, timer.Elapsed, false);
                Telemetry.Current.TrackException(ex,
                       new Dictionary<string, string> {
                            { "CollectionUrl", teamProjectContext.Collection.Uri.ToString() }
                       },
                       new Dictionary<string, double> {
                            { "Time",timer.ElapsedMilliseconds }
                       });
                Trace.TraceWarning($"  [EXCEPTION] {ex}");
                throw;
            }
           
            foundWis = new Dictionary<int, WorkItem>();
		}

        public Project GetProject()
        {
            return (from Project x in wistore.Projects where x.Name.ToUpper() == teamProjectContext.Config.Project.ToUpper() select x).SingleOrDefault();
        }

        public string CreateReflectedWorkItemId(WorkItem wi)
        {
            return string.Format("{0}/{1}/_workitems/edit/{2}", wi.Store.TeamProjectCollection.Uri.ToString().TrimEnd('/'), wi.Project.Name, wi.Id);

        }
        public int GetReflectedWorkItemId(WorkItem wi, string reflectedWotkItemIdField)
        {
            if (!wi.Fields.Contains(reflectedWotkItemIdField))
            {
                return 0;
            }
            string rwiid = wi.Fields[reflectedWotkItemIdField].Value.ToString();
            if (Regex.IsMatch(rwiid, @"(http(s)?://)?([\w-]+\.)+[\w-]+(/[\w- ;,./?%&=]*)?"))
            {
                return int.Parse(rwiid.Substring(rwiid.LastIndexOf(@"/") + 1));
            }
            return 0;
        }

        public WorkItem FindReflectedWorkItem(WorkItem workItemToFind, bool cache)
        {
            string ReflectedWorkItemId = CreateReflectedWorkItemId(workItemToFind);
            WorkItem found = null;
            if (foundWis.ContainsKey(workItemToFind.Id))
            {
                return foundWis[workItemToFind.Id];
            }
            if (workItemToFind.Fields.Contains(teamProjectContext.Config.ReflectedWorkItemIDFieldName) && !string.IsNullOrEmpty( workItemToFind.Fields[teamProjectContext.Config.ReflectedWorkItemIDFieldName]?.Value?.ToString()))
            {
                string rwiid = workItemToFind.Fields[teamProjectContext.Config.ReflectedWorkItemIDFieldName].Value.ToString();
                int idToFind = GetReflectedWorkItemId(workItemToFind, teamProjectContext.Config.ReflectedWorkItemIDFieldName);
                if (idToFind == 0)
                {
                    found = null;
                }
                else
                {
                    try
                    {
                        found = Store.GetWorkItem(idToFind);
                        if (!(found.Fields[teamProjectContext.Config.ReflectedWorkItemIDFieldName].Value.ToString() == rwiid))
                        {
                            found = null;
                        }
                    } 
                    catch (DeniedOrNotExistException)
                    {
                        found = null;
                    }
                }                
            }
            if (found == null) { found = FindReflectedWorkItemByReflectedWorkItemId(ReflectedWorkItemId); }
            if (!workItemToFind.Fields.Contains(teamProjectContext.Config.ReflectedWorkItemIDFieldName))
            {
                if (found == null) { found = FindReflectedWorkItemByMigrationRef(ReflectedWorkItemId); } // Too slow!
                //if (found == null) { found = FindReflectedWorkItemByTitle(workItemToFind.Title); }
            }
            if (found != null && cache) 
            {
                foundWis.Add(workItemToFind.Id, found); /// TODO MEMORY LEAK
            }
            return found;
        }

        public WorkItem FindReflectedWorkItemByReflectedWorkItemId(WorkItem refWi)
        {
            return FindReflectedWorkItemByReflectedWorkItemId(CreateReflectedWorkItemId(refWi));
        }

        public WorkItem FindReflectedWorkItemByReflectedWorkItemId(int refId, bool cache)
        {
            var sourceIdKey = ~refId;
            if (foundWis.TryGetValue(sourceIdKey, out var workItem)) return workItem;

            IEnumerable<WorkItem> QueryWorkItems()
            {
                TfsQueryContext query = new TfsQueryContext(this);
                query.Query = string.Format(@"SELECT [System.Id] FROM WorkItems  WHERE [System.TeamProject]=@TeamProject AND [{0}] Contains '@idToFind'", teamProjectContext.Config.ReflectedWorkItemIDFieldName);
                query.AddParameter("idToFind", refId.ToString());
                query.AddParameter("TeamProject", this.teamProjectContext.Config.Project);
                foreach(WorkItem wi in query.Execute())
                {
                    yield return wi;
                }
            }

            var foundWorkItem = QueryWorkItems().FirstOrDefault(wi => wi.Fields[teamProjectContext.Config.ReflectedWorkItemIDFieldName].Value.ToString().EndsWith("/" + refId));
            if (cache && foundWorkItem != null) foundWis[sourceIdKey] = foundWorkItem;
            return foundWorkItem;
        }

        private StringBuilder FindReflectedWorkItemQueryBase(TfsQueryContext query)
        {
            StringBuilder s = new StringBuilder();
            s.Append("SELECT [System.Id] FROM WorkItems");
            s.Append("WHERE ");
            if (teamProjectContext.Config.AllowCrossProjectLinking)
            {
                s.Append("[System.TeamProject]=@TeamProject AND ");
                query.AddParameter("TeamProject", this.teamProjectContext.Config.Project);
            }
            return s;
        }

        public WorkItem FindReflectedWorkItemByReflectedWorkItemId(string refId)
        {
            TfsQueryContext query = new TfsQueryContext(this);
            StringBuilder queryBuilder = FindReflectedWorkItemQueryBase(query);
            queryBuilder.AppendFormat("[{0}] = @idToFind", teamProjectContext.Config.ReflectedWorkItemIDFieldName);
            query.AddParameter("idToFind", refId.ToString());
            query.Query = queryBuilder.ToString();
            return FindWorkItemByQuery(query);
        }

        public WorkItem FindReflectedWorkItemByMigrationRef(string refId)
        {
            TfsQueryContext query = new TfsQueryContext(this);
            StringBuilder queryBuilder  = FindReflectedWorkItemQueryBase(query);
            queryBuilder.Append(" [System.Description] Contains @KeyToFind");
            query.AddParameter("KeyToFind", string.Format("##REF##{0}##", refId));
            query.Query = queryBuilder.ToString();
            return FindWorkItemByQuery(query);
        }

        public WorkItem FindReflectedWorkItemByTitle(string title)
        {
            TfsQueryContext query = new TfsQueryContext(this);
            query.Query = @"SELECT [System.Id] FROM WorkItems  WHERE [System.TeamProject]=@TeamProject AND [System.Title] = @TitleToFind";
            query.AddParameter("TitleToFind", title);
            query.AddParameter("TeamProject", this.teamProjectContext.Config.Project);
            return FindWorkItemByQuery(query);
        }

        public WorkItem FindWorkItemByQuery(TfsQueryContext query)
        {
            WorkItemCollection newFound;
            newFound = query.Execute();
            if (newFound.Count == 0)
            {
                return null;
            }
            return newFound[0];
        }

        public WorkItem GetRevision(WorkItem workItem, int revision)
        {
            return Store.GetWorkItem(workItem.Id, revision);
        }

    }
}
