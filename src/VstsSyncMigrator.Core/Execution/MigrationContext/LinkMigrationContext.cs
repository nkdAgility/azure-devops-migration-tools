using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VstsSyncMigrator.Engine.Configuration.Processing;
using VstsSyncMigrator.Engine.Execution.Exceptions;

namespace VstsSyncMigrator.Engine
{
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
            tfsqc.AddParameter("TeamProject", me.Source.Name);
            tfsqc.Query = string.Format(@"SELECT [System.Id] FROM WorkItems WHERE  [System.TeamProject] = @TeamProject {0} ORDER BY [System.ChangedDate] desc ", config.QueryBit); // AND  [Microsoft.VSTS.Common.ClosedDate] = ''
            WorkItemCollection sourceWIS = tfsqc.Execute();
            //////////////////////////////////////////////////

            Trace.WriteLine(string.Format("Migrate {0} work items links?", sourceWIS.Count), "LinkMigrationContext");
            int current = sourceWIS.Count;
            //////////////////////////////////////////////////
            WorkItemStoreContext targetWitsc = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            Project targetProj = targetWitsc.GetProject();
            //////////////////////////////////////////////////
            foreach (WorkItem wiSourceL in sourceWIS)
            {
                Trace.WriteLine(string.Format("Migrating Links for wiSourceL={0}", wiSourceL.Id), "LinkMigrationContext");
                WorkItem wiTargetL = null;
                try
                {
                    wiTargetL = targetWitsc.FindReflectedWorkItem(wiSourceL, me.ReflectedWorkItemIdFieldName, true);
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
                if (wiTargetL.Links.Count == wiSourceL.Links.Count)
                {
                    Trace.WriteLine(string.Format("[SKIP] SOurce and Target have same number of links  {0} - {1}", wiSourceL.Id, wiSourceL.Type.ToString()), "LinkMigrationContext");
                }
                else
                {
                    try
                    {
                        Trace.Indent();
                        foreach (Link item in wiSourceL.Links)
                        {
                            Trace.WriteLine(string.Format("Migrating link for {0} of type {1}",
                                wiSourceL.Id, item.GetType().Name), "LinkMigrationContext");
                            if (IsHyperlink(item))
                            {
                                CreateHyperlink((Hyperlink)item, wiTargetL);
                            } else if (IsRelatedLink(item)) {
                                RelatedLink rl = (RelatedLink)item;
                                CreateRelatedLink(wiSourceL, rl, wiTargetL, sourceStore, targetWitsc);
                            }
                            else if (IsExternalLink(item))
                            {
                                ExternalLink rl = (ExternalLink)item;
                                CreateExternalLink((ExternalLink)item, wiTargetL);
                            } else {
                                UnknownLinkTypeException ex = new UnknownLinkTypeException(string.Format("  [UnknownLinkType] Unable to {0}",item.GetType().Name));
                                Telemetry.Current.TrackException(ex);
                                Trace.WriteLine(ex.ToString(), "LinkMigrationContext");
                                throw ex;
                            }
                        }
                    }
                    catch (WorkItemLinkValidationException ex)
                    {
                        wiSourceL.Reset();
                        wiTargetL.Reset();
                        Telemetry.Current.TrackException(ex);
                        Trace.WriteLine(string.Format("  [WorkItemLinkValidationException] Adding link for wiSourceL={0}", wiSourceL.Id), "LinkMigrationContext");
                        Trace.WriteLine(ex.ToString(), "LinkMigrationContext");
                    }
                    catch (Exception ex)
                    {
                        Telemetry.Current.TrackException(ex);
                        Trace.WriteLine(string.Format("  [CREATE-FAIL] Adding Link for wiSourceL={0}", wiSourceL.Id), "LinkMigrationContext");
                        Trace.WriteLine(ex.ToString(), "LinkMigrationContext");
                    }
                }
                if (wiSourceL.Type.Name == "Test Case")
                {
                    MigrateSharedSteps(wiSourceL, wiTargetL, sourceStore, targetWitsc);
                }

                current--;
            }
        }

        private void MigrateSharedSteps(WorkItem wiSourceL, WorkItem wiTargetL, WorkItemStoreContext sourceStore,
            WorkItemStoreContext targetStore)
        {
            const string microsoftVstsTcmSteps = "Microsoft.VSTS.TCM.Steps";
            var oldSteps = wiTargetL.Fields[microsoftVstsTcmSteps].Value.ToString();
            var newSteps = oldSteps;

            var sourceSharedStepLinks = wiSourceL.Links.OfType<RelatedLink>()
                .Where(x => x.LinkTypeEnd.Name == "Shared Steps").ToList();
            var sourceSharedSteps =
                sourceSharedStepLinks.Select(x => sourceStore.Store.GetWorkItem(x.RelatedWorkItemId));

            foreach (WorkItem sourceSharedStep in sourceSharedSteps)
            {
                WorkItem matchingTargetSharedStep =
                    targetStore.FindReflectedWorkItemByReflectedWorkItemId(sourceSharedStep,
                        me.ReflectedWorkItemIdFieldName);

                if (matchingTargetSharedStep != null)
                {
                    newSteps = newSteps.Replace($"ref=\"{sourceSharedStep.Id}\"",
                        $"ref=\"{matchingTargetSharedStep.Id}\"");
                    wiTargetL.Fields[microsoftVstsTcmSteps].Value = newSteps;
                }
            }

            if (wiTargetL.IsDirty)
                wiTargetL.Save();
        }

        private void CreateExternalLink(ExternalLink sourceLink, WorkItem target)
        {
            var exist = (from Link l in target.Links
                         where l is ExternalLink && ((ExternalLink)l).LinkedArtifactUri == ((ExternalLink)sourceLink).LinkedArtifactUri
                         select (ExternalLink)l).SingleOrDefault();
            if (exist == null)
            {

                Trace.WriteLine(string.Format("Creating new {0} on {1}",
                                                   sourceLink.GetType().Name, target.Id), "LinkMigrationContext");
                ExternalLink el = new ExternalLink(sourceLink.ArtifactLinkType, sourceLink.LinkedArtifactUri);
                el.Comment = sourceLink.Comment;
                target.Links.Add(el);
                target.Save();
            } else {
                Trace.WriteLine(string.Format("Link {0} on {1} already exists",
                                                  sourceLink.GetType().Name, target.Id), "LinkMigrationContext");
            }
        }

        private bool IsExternalLink(Link item)
        {
            return item is ExternalLink;
        }

        private void CreateRelatedLink(WorkItem wiSourceL, RelatedLink item, WorkItem wiTargetL, WorkItemStoreContext sourceStore, WorkItemStoreContext targetStore)
        {
            RelatedLink rl = (RelatedLink)item;
            WorkItem wiSourceR = null;
            WorkItem wiTargetR = null;
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
            try
            {
                wiTargetR = GetRightHandSideTargitWi(wiSourceL, wiSourceR, wiTargetL, targetStore);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("  [FIND-FAIL] Adding Link of type {0} where wiSourceL={1}, wiTargetL={2} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiTargetL.Id));
                Trace.TraceError(ex.ToString());
                return;

            }
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

                if (!IsExisting && !wiTargetR.IsAccessDenied)
                {

                    if (wiSourceR.Id != wiTargetR.Id)
                    {
                        Trace.WriteLine(
                            string.Format("  [CREATE-START] Adding Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id));
                        WorkItemLinkTypeEnd linkTypeEnd = targetStore.Store.WorkItemLinkTypes.LinkTypeEnds[rl.LinkTypeEnd.ImmutableName];
                        RelatedLink newRl = new RelatedLink(linkTypeEnd, wiTargetR.Id);

                        wiTargetL.Links.Add(newRl);
                        wiTargetL.Save();
                        Trace.WriteLine(
                            string.Format(
                                "  [CREATE-SUCCESS] Adding Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ",
                                rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id));
                    }
                    else
                    {
                        Trace.WriteLine(
                              string.Format(
                                  "  [SKIP] Unable to migrate link where Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} as target WI has not been migrated",
                                  rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id));
                    }
                }
                else
                {
                    if (IsExisting)
                    {
                        Trace.WriteLine(string.Format("  [SKIP] Already Exists a Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id));
                    }
                    if (wiTargetR.IsAccessDenied)
                    {
                        Trace.WriteLine(string.Format("  [AccessDenied] The Target  work item is inaccessable to create a Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id));
                    }
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
            if (!(wiTargetL == null)
                && wiSourceR.Project.Name == wiTargetL.Project.Name
                && wiSourceR.Project.Store.TeamProjectCollection.Uri.ToString().Replace("/", "") == wiTargetL.Project.Store.TeamProjectCollection.Uri.ToString().Replace("/", ""))
            {
                // Moving to same team project as SourceR
                wiTargetR = wiSourceR;
            }
            else
            {
                // Moving to Other Team Project from SOurceR
                wiTargetR = targetStore.FindReflectedWorkItem(wiSourceR, me.ReflectedWorkItemIdFieldName, true);
                if (wiTargetR == null) // Assume source only (other team projkect)
                {
                    wiTargetR = wiSourceR;
                    if (wiTargetR.Project.Store.TeamProjectCollection.Uri.ToString().Replace("/", "") != wiSourceR.Project.Store.TeamProjectCollection.Uri.ToString().Replace("/", ""))
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
            var exist = (from Link l in target.Links where l is Hyperlink && ((Hyperlink)l).Location == ((Hyperlink)sourceLink).Location select (Hyperlink)l).SingleOrDefault();
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