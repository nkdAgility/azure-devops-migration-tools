using System;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.Generic;

namespace TfsWitMigrator.Core
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
            this.targetTfs = targetTfs;
            this.bypassRules = bypassRules;
            wistore = new WorkItemStore(targetTfs.Collection, bypassRules);
            foundWis = new Dictionary<int, WorkItem>();
        }

        public Project GetProject()
        {
            return (from Project x in wistore.Projects where x.Name == targetTfs.Name select x).SingleOrDefault();
        }

        

        public WorkItem FindReflectedWorkItem(WorkItem workItemToFind)
        {
            WorkItem found = null;
            if (foundWis.ContainsKey(workItemToFind.Id))
            {
                return foundWis[workItemToFind.Id];
            }
            if (workItemToFind.Fields.Contains("TfsMigrationTool.ReflectedWorkItemId") && !string.IsNullOrEmpty( workItemToFind.Fields["TfsMigrationTool.ReflectedWorkItemId"].Value.ToString()))
            {
                found = Store.GetWorkItem(int.Parse(workItemToFind.Fields["TfsMigrationTool.ReflectedWorkItemId"].Value.ToString()));
            }
            if (found == null) { found = FindReflectedWorkItemByReflectedWorkItemId(workItemToFind.Id); }
            if (found == null) { found = FindReflectedWorkItemByMigrationRef(workItemToFind.Id); }
            //if (found == null) { found = FindReflectedWorkItemByTitle(workItemToFind.Title); }
            if (found != null) 
            {
                foundWis.Add(workItemToFind.Id, found);
            }

            return found;
        }

        public WorkItem FindReflectedWorkItemByReflectedWorkItemId(int refId)
        {
            TfsQueryContext query = new TfsQueryContext(this);
            query.Query = @"SELECT [System.Id] FROM WorkItems  WHERE [System.TeamProject]=@TeamProject AND [TfsMigrationTool.ReflectedWorkItemId] = @idToFind";
            query.AddParameter("idToFind", refId.ToString());
            query.AddParameter("TeamProject", this.targetTfs.Name);
            return FindWorkItemByQuery(query);
        }

        public WorkItem FindReflectedWorkItemByMigrationRef(int refId)
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


    }
}