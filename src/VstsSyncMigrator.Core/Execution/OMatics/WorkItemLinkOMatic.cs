using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Serilog;
using VstsSyncMigrator.Engine;
using VstsSyncMigrator.Engine.Execution.Exceptions;

namespace VstsSyncMigrator.Core.Execution.OMatics
{
    public class WorkItemLinkOMatic
    {
        public void MigrateLinks(WorkItem sourceWorkItemLinkStart, WorkItemStoreContext sourceWorkItemStore, WorkItem targetWorkItemLinkStart, WorkItemStoreContext targetWorkItemStore, bool save = true, bool filterWorkItemsThatAlreadyExistInTarget = true, string sourceReflectedWIIdField = null)
        {
            if (ShouldCopyLinks(sourceWorkItemLinkStart, targetWorkItemLinkStart, filterWorkItemsThatAlreadyExistInTarget))
            {
                Trace.Indent();
                foreach (Link item in sourceWorkItemLinkStart.Links)
                {
                    try
                    {
                        Log.Information("Migrating link for {sourceWorkItemLinkStartId} of type {ItemGetTypeName}", sourceWorkItemLinkStart.Id, item.GetType().Name);
                        if (IsHyperlink(item))
                        {
                            CreateHyperlink((Hyperlink)item, targetWorkItemLinkStart, save);
                        }
                        else if (IsRelatedLink(item))
                        {
                            RelatedLink rl = (RelatedLink)item;
                            CreateRelatedLink(sourceWorkItemLinkStart, rl, targetWorkItemLinkStart, sourceWorkItemStore, targetWorkItemStore, save, sourceReflectedWIIdField);
                        }
                        else if (IsExternalLink(item))
                        {
                            var el = (ExternalLink)item;
                            if (!IsBuildLink(el))
                            {
                                CreateExternalLink(el, targetWorkItemLinkStart, save);
                            }
                        }
                        else
                        {
                            UnknownLinkTypeException ex = new UnknownLinkTypeException(string.Format("  [UnknownLinkType] Unable to {0}", item.GetType().Name));
                            Log.Error(ex, "LinkMigrationContext");
                            throw ex;
                        }
                    }
                    catch (WorkItemLinkValidationException ex)
                    {
                        sourceWorkItemLinkStart.Reset();
                        targetWorkItemLinkStart.Reset();
                        Log.Error(ex, "[WorkItemLinkValidationException] Adding link for wiSourceL={sourceWorkItemLinkStartId}", sourceWorkItemLinkStart.Id);
                    }
                    catch (FormatException ex)
                    {
                        sourceWorkItemLinkStart.Reset();
                        targetWorkItemLinkStart.Reset();
                        Log.Error(ex, "[CREATE-FAIL] Adding Link for wiSourceL={sourceWorkItemLinkStartId}", sourceWorkItemLinkStart.Id);
                    }
                }
            }
            if (sourceWorkItemLinkStart.Type.Name == "Test Case")
            {
                MigrateSharedSteps(sourceWorkItemLinkStart, targetWorkItemLinkStart, sourceWorkItemStore, targetWorkItemStore, save);
            }
        }

        private void MigrateSharedSteps(WorkItem wiSourceL, WorkItem wiTargetL, WorkItemStoreContext sourceStore,
            WorkItemStoreContext targetStore, bool save)
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
                    targetStore.FindReflectedWorkItemByReflectedWorkItemId(sourceSharedStep);

                if (matchingTargetSharedStep != null)
                {
                    newSteps = newSteps.Replace($"ref=\"{sourceSharedStep.Id}\"",
                        $"ref=\"{matchingTargetSharedStep.Id}\"");
                    wiTargetL.Fields[microsoftVstsTcmSteps].Value = newSteps;
                }
            }

            if (wiTargetL.IsDirty && save)
            {
                wiTargetL.Fields["System.ChangedBy"].Value = "Migration";
                wiTargetL.Save();
            }
        }

        private void CreateExternalLink(ExternalLink sourceLink, WorkItem target, bool save)
        {
            var exist = (from Link l in target.Links
                         where l is ExternalLink && ((ExternalLink)l).LinkedArtifactUri == ((ExternalLink)sourceLink).LinkedArtifactUri
                         select (ExternalLink)l).SingleOrDefault();
            if (exist == null)
            {
                Log.Information("Creating new {SourceLinkType} on {TargetId}", sourceLink.GetType().Name, target.Id);
                ExternalLink el = new ExternalLink(sourceLink.ArtifactLinkType, sourceLink.LinkedArtifactUri);
                el.Comment = sourceLink.Comment;
                target.Links.Add(el);
                if (save)
                {
                    target.Fields["System.ChangedBy"].Value = "Migration";
                    target.Save();
                }
            }
            else
            {
                Log.Information("Link {SourceLinkType} on {TargetId} already exists",
                                                  sourceLink.GetType().Name, target.Id);
            }
        }

        private bool IsExternalLink(Link item)
        {
            return item is ExternalLink;
        }

        private bool IsBuildLink(ExternalLink link)
        {
            return link.LinkedArtifactUri != null &&
                   link.LinkedArtifactUri.StartsWith("vstfs:///Build/Build/", StringComparison.InvariantCultureIgnoreCase);
        }

        private void CreateRelatedLink(WorkItem wiSourceL, RelatedLink item, WorkItem wiTargetL, WorkItemStoreContext sourceStore, WorkItemStoreContext targetStore, bool save, string sourceReflectedWIIdField)
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
                Log.Error(ex, "  [FIND-FAIL] Adding Link of type {0} where wiSourceL={1}, wiTargetL={2} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiTargetL.Id);
                return;
            }
            try
            {
                wiTargetR = GetRightHandSideTargetWi(wiSourceL, wiSourceR, wiTargetL, targetStore, sourceStore, sourceReflectedWIIdField);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "  [FIND-FAIL] Adding Link of type {0} where wiSourceL={1}, wiTargetL={2} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiTargetL.Id);
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

                    Log.Error(ex, "  [SKIP] Unable to migrate links where wiSourceL={0}, wiSourceR={1}, wiTargetL={2}", ((wiSourceL != null) ? wiSourceL.Id.ToString() : "NotFound"), ((wiSourceR != null) ? wiSourceR.Id.ToString() : "NotFound"), ((wiTargetL != null) ? wiTargetL.Id.ToString() : "NotFound"));
                    return;
                }

                if (!IsExisting && !wiTargetR.IsAccessDenied)
                {
                    if (wiSourceR.Id != wiTargetR.Id)
                    {
                        Log.Information("  [CREATE-START] Adding Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id);
                        WorkItemLinkTypeEnd linkTypeEnd = targetStore.Store.WorkItemLinkTypes.LinkTypeEnds[rl.LinkTypeEnd.ImmutableName];
                        RelatedLink newRl = new RelatedLink(linkTypeEnd, wiTargetR.Id);
                        if (linkTypeEnd.ImmutableName == "System.LinkTypes.Hierarchy-Forward")
                        {
                            var potentialParentConflictLink = ( // TF201036: You cannot add a Child link between work items xxx and xxx because a work item can have only one Parent link.
                                    from Link l in wiTargetR.Links
                                    where l is RelatedLink
                                        && ((RelatedLink)l).LinkTypeEnd.ImmutableName == "System.LinkTypes.Hierarchy-Reverse"
                                    select (RelatedLink)l).SingleOrDefault();
                            if (potentialParentConflictLink != null)
                            {
                                wiTargetR.Links.Remove(potentialParentConflictLink);
                            }
                            linkTypeEnd = targetStore.Store.WorkItemLinkTypes.LinkTypeEnds["System.LinkTypes.Hierarchy-Reverse"];
                            RelatedLink newLl = new RelatedLink(linkTypeEnd, wiTargetL.Id);
                            wiTargetR.Links.Add(newLl);
                            wiTargetR.Fields["System.ChangedBy"].Value = "Migration";
                            wiTargetR.Save();
                        }
                        else
                        {
                            wiTargetL.Links.Add(newRl);
                            if (save)
                            {
                                wiTargetL.Fields["System.ChangedBy"].Value = "Migration";
                                wiTargetL.Save();
                            }
                        }
                        Log.Information(
                                "  [CREATE-SUCCESS] Adding Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ",
                                rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id);

                    }
                    else
                    {
                        Log.Information(
                                  "  [SKIP] Unable to migrate link where Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} as target WI has not been migrated",
                                  rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id);
                    }
                }
                else
                {
                    if (IsExisting)
                    {
                        Log.Information("  [SKIP] Already Exists a Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id);
                    }
                    if (wiTargetR.IsAccessDenied)
                    {
                        Log.Information("  [AccessDenied] The Target  work item is inaccessable to create a Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id);
                    }
                }
            }
            else
            {
                Log.Information("  [SKIP] Cant find wiTargetR where wiSourceL={0}, wiSourceR={1}, wiTargetL={2}", wiSourceL.Id, wiSourceR.Id, wiTargetL.Id);
            }
        }

        private WorkItem GetRightHandSideTargetWi(WorkItem wiSourceL, WorkItem wiSourceR, WorkItem wiTargetL, WorkItemStoreContext targetStore, WorkItemStoreContext sourceStore, string sourceReflectedWIIdField)
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
                // Moving to Other Team Project from Source
                wiTargetR = targetStore.FindReflectedWorkItem(wiSourceR, true, sourceReflectedWIIdField);
                if (wiTargetR == null) // Assume source only (other team project)
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

        private void CreateHyperlink(Hyperlink sourceLink, WorkItem target, bool save)
        {
            var exist = (from Link l in target.Links where l is Hyperlink && ((Hyperlink)l).Location == ((Hyperlink)sourceLink).Location select (Hyperlink)l).SingleOrDefault();
            if (exist == null)
            {
                Hyperlink hl = new Hyperlink(sourceLink.Location);
                hl.Comment = sourceLink.Comment;
                target.Links.Add(hl);
                if (save)
                {
                    target.Fields["System.ChangedBy"].Value = "Migration";
                    target.Save();
                }
            }
        }

        private static bool ShouldCopyLinks(WorkItem sourceWorkItemLinkStart, WorkItem targetWorkItemLinkStart, bool filterWorkItemsThatAlreadyExistInTarget)
        {
            if (filterWorkItemsThatAlreadyExistInTarget)
            {
                if (targetWorkItemLinkStart.Links.Count == sourceWorkItemLinkStart.Links.Count) // we should never have this as the target should not have existed in this path
                {
                    Log.Information("[SKIP] Source and Target have same number of links  {sourceWorkItemLinkStartId} - {sourceWorkItemLinkStartType}", sourceWorkItemLinkStart.Id, sourceWorkItemLinkStart.Type.ToString());
                    return false;
                }
            }
            return true;
        }

        private bool IsHyperlink(Link item)
        {
            return item is Hyperlink;
        }
    }
}
