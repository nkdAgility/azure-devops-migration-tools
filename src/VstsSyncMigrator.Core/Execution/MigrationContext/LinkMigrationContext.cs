using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VstsSyncMigrator.Core.Execution.OMatics;
using VstsSyncMigrator.Engine.Configuration.Processing;
using VstsSyncMigrator.Engine.Execution.Exceptions;

namespace VstsSyncMigrator.Engine
{
    [Obsolete()]
    public class LinkMigrationContext : MigrationContextBase
    {

        LinkMigrationConfig config;
        public override string Name
        {
            get
            {
                return "LinkMigrationContext";
            }
        }

        public LinkMigrationContext(MigrationEngine me, LinkMigrationConfig config) : base(me, config)
        {
            this.config = config;
        }

        public void AddTagToWit(WorkItem w, string tag)
        {
            List<string> tags = w.Tags.Split(char.Parse(@";")).ToList();
            List<string> newTags = tags.Union(new List<string>() { tag }).ToList();
            w.Tags = string.Join(";", newTags.ToArray());
            w.Save();
        }

        internal override void InternalExecute()
        {
            WorkItemStoreContext sourceStore = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.BypassRules);
            TfsQueryContext tfsqc = new TfsQueryContext(sourceStore);
            tfsqc.AddParameter("TeamProject", me.Source.Config.Name);
            tfsqc.Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE  [System.TeamProject] = @TeamProject {0} ORDER BY [System.ChangedDate] desc ", config.QueryBit); // AND  [Microsoft.VSTS.Common.ClosedDate] = ''
            WorkItemCollection sourceWIS = tfsqc.Execute();
            //////////////////////////////////////////////////

            Trace.WriteLine(string.Format("Migrate {0} work items links?", sourceWIS.Count), "LinkMigrationContext");
            int current = sourceWIS.Count;
            //////////////////////////////////////////////////
            WorkItemStoreContext targetWitsc = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            Project targetProj = targetWitsc.GetProject();
            //////////////////////////////////////////////////
            WorkItemLinkOMatic linkomatic = new WorkItemLinkOMatic();
            foreach (WorkItem wiSourceL in sourceWIS)
            {
                Trace.WriteLine(string.Format("Migrating Links for wiSourceL={0}", wiSourceL.Id), "LinkMigrationContext");
                WorkItem wiTargetL = null;
                try
                {
                    wiTargetL = targetWitsc.FindReflectedWorkItem(wiSourceL, true);
                }
                catch (Exception)
                {
                    Trace.WriteLine(string.Format("Cannot find twiTargetL matching wiSourceL={0} probably due to missing ReflectedWorkItemID", wiSourceL.Id), "LinkMigrationContext");
                }

                if (wiTargetL == null)
                {
                    //wiSourceL was not migrated, or the migrated work item has been deleted. 
                    Trace.WriteLine(string.Format("[SKIP] Unable to migrate links where wiSourceL={0}, wiTargetL=NotFound",
                            wiSourceL.Id), "LinkMigrationContext");
                    continue;
                }
                Trace.WriteLine(string.Format("Found Target Left wiSourceL={0}, wiTargetL=NotFound",
                    wiSourceL.Id), "LinkMigrationContext");

                linkomatic.MigrateLinks(wiSourceL, sourceStore, wiTargetL, targetWitsc);

                current--;
            }
        }


 

    }

    
}