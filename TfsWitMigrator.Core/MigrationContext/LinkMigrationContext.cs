using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TfsWitMigrator.Core
{
    public class LinkMigrationContext : MigrationContextBase
    {
        public override string Name
        {
            get
            {
                return "LinkMigrationContext";
            }
        }

        public LinkMigrationContext(MigrationEngine me) : base(me)
        {
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
            WorkItemStoreContext sourceStore = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.None);
            TfsQueryContext tfsqc = new TfsQueryContext(sourceStore);
            tfsqc.AddParameter("TeamProject", me.Source.Name);
            tfsqc.Query = @"SELECT [System.Id] FROM WorkItems WHERE  [System.TeamProject] = @TeamProject AND [System.RelatedLinkCount] > 0  AND  [Microsoft.VSTS.Common.ClosedDate] = '' ORDER BY [System.ChangedDate] desc  ";
            WorkItemCollection sourceWIS = tfsqc.Execute();
            //////////////////////////////////////////////////

            Trace.WriteLine(string.Format("Migrate {0} work items links?", sourceWIS.Count));
            int current = sourceWIS.Count;
            //////////////////////////////////////////////////
            WorkItemStoreContext targetWitsc = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            Project targetProj = targetWitsc.GetProject();
            //////////////////////////////////////////////////
            foreach (WorkItem wiSourceL in sourceWIS)
            {
                WorkItem wiTargetL = targetWitsc.FindReflectedWorkItem(wiSourceL);
                if (wiTargetL == null)
                {
                    //wiSourceL was not migrated, or the migrated work item has been deleted. 
                    Trace.WriteLine(string.Format("[SKIP] Unable to migrate links where wiSourceL={0}, wiTargetL=NotFound",
                                                   wiSourceL.Id));
                    continue;
                }

                ///
                if (wiTargetL.Links.Count == wiSourceL.Links.Count)
                {
                    Trace.WriteLine(string.Format("[SKIP] SOurce and Target have same number of links  {0} - {1}", wiSourceL.Id, wiSourceL.Type.ToString()));
                }
                else
                {
                    foreach (Link item in wiSourceL.Links)
                    {
                        if (IsHyperlink(item))
                        {
                            CreateHyperlink((Hyperlink)item, wiTargetL);
                        }
                        if (IsRelatedLink(item))
                        {
                            CreateRelatedLink(wiSourceL, (RelatedLink)item, wiTargetL, sourceStore, targetWitsc);
                        }
                    }
                }
                current--;
            }

        }

        private void CreateRelatedLink(WorkItem wiSourceL, RelatedLink item, WorkItem wiTargetL, WorkItemStoreContext sourceStore, WorkItemStoreContext targetStore)
        {
            RelatedLink rl = (RelatedLink)item;
            WorkItem wiSourceR = null;
            try
            {
                wiSourceR = sourceStore.Store.GetWorkItem(rl.RelatedWorkItemId);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("  [FIND-FAIL] Adding Link of type {0} where wiSourceL={1}, wiTargetL={2} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiTargetL.Id));
                Trace.TraceError(ex.ToString());
                return;
            }
            WorkItem wiTargetR = GetRightHandSideTargitWi(wiSourceL, wiSourceR, wiTargetL, targetStore);
            if (wiTargetR != null)
            {
                bool IsExisting = false;
                try
                {
                    var exist = (
                        from Link l in wiTargetL.Links
                        where l is RelatedLink
                            && ((RelatedLink)l).RelatedWorkItemId == wiTargetR.Id
                            && ((RelatedLink)l).LinkTypeEnd.ImmutableName == item.LinkTypeEnd.ImmutableName
                        select (RelatedLink)l).SingleOrDefault();
                    IsExisting = (exist != null);
                }
                catch (Exception ex)
                {

                    Trace.WriteLine(string.Format("  [SKIP] Unable to migrate links where wiSourceL={0}, wiSourceR={1}, wiTargetL={2}", ((wiSourceL != null) ? wiSourceL.Id.ToString() : "NotFound"), ((wiSourceR != null) ? wiSourceR.Id.ToString() : "NotFound"), ((wiTargetL != null) ? wiTargetL.Id.ToString() : "NotFound")));
                    Trace.TraceError(ex.ToString());
                    return;
                }

                if (!IsExisting)
                {
                    Trace.WriteLine(string.Format("  [CREATE-START] Adding Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id));
                    WorkItemLinkTypeEnd linkTypeEnd = targetStore.Store.WorkItemLinkTypes.LinkTypeEnds[rl.LinkTypeEnd.ImmutableName];
                    RelatedLink newRl = new RelatedLink(linkTypeEnd, wiTargetR.Id);
                    wiTargetL.Links.Add(newRl);
                    try
                    {
                        wiTargetL.Save();
                        Trace.WriteLine(string.Format("  [CREATE-SUCESS] Adding Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id));

                    }
                    catch (WorkItemLinkValidationException ex)
                    {
                        Trace.WriteLine(string.Format("  [CREATE-FAIL] Adding Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id));
                        Trace.TraceError(ex.ToString());
                        return;
                    }
                }
                else
                {
                    // Yes Link
                    Trace.WriteLine(string.Format("  [SKIP] Already Exists a Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id));
                }

            }
            else
            {
                Trace.WriteLine(string.Format("  [SKIP] Cant find wiTargetR where wiSourceL={0}, wiSourceR={1}, wiTargetL={2}", wiSourceL.Id, wiSourceR.Id, wiTargetL.Id));
            }


        }

        private WorkItem GetRightHandSideTargitWi(WorkItem wiSourceL, WorkItem wiSourceR, WorkItem wiTargetL, WorkItemStoreContext targetStore)
        {
            WorkItem wiTargetR;
            if (!(wiTargetL == null) && wiSourceR.Project.Name == wiTargetL.Project.Name)
            {
                // Moving to same team project as SourceR
                wiTargetR = wiSourceR;
            }
            else
            {
                // Moving to Other Team Project from SOurceR
                wiTargetR = targetStore.FindReflectedWorkItem(wiSourceR);
                if (wiTargetR == null) // Assume source only (other team projkect)
                {
                    wiTargetR = wiSourceR;
                    if (wiTargetR.Project.Store.TeamProjectCollection.Uri != wiSourceR.Project.Store.TeamProjectCollection.Uri)
                    {
                        wiTargetR = null; // Totally bogus break! as not same team collection
                    }
                }
            }
            return wiTargetR;
        }

        private bool IsRelatedLink(Link item)
        {
            return item is RelatedLink;
        }

        private void CreateHyperlink(Hyperlink sourceLink, WorkItem target)
        {
            var exist = from Link l in target.Links where l is Hyperlink && ((Hyperlink)l).Location == ((Hyperlink)sourceLink).Location select (Hyperlink)l;
            if (exist == null)
            {
                Hyperlink hl = new Hyperlink(sourceLink.Location);
                hl.Comment = sourceLink.Comment;
                target.Links.Add(hl);
                target.Save();
            }
        }

        private bool IsHyperlink(Link item)
        {
            return item is Hyperlink;
        }
    }
}