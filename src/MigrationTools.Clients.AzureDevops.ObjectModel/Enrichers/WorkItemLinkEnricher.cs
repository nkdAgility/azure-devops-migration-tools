using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Clients.AzureDevops.ObjectModel.Clients;
using MigrationTools.Clients;
using MigrationTools.DataContracts;
using MigrationTools.Exceptions;
using VstsSyncMigrator.Engine;
using MigrationTools.Enrichers;
using Microsoft.Extensions.Logging;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.Enrichers
{
    public class WorkItemLinkEnricher : WorkItemEnricher
    {

        private bool _save = true;
        private bool _filterWorkItemsThatAlreadyExistInTarget = true;

        public WorkItemLinkEnricher(IMigrationEngine engine, ILogger<WorkItemLinkEnricher> logger) :base(engine, logger)
        {

        }

        public override void Configure(
            bool save = true,
            bool filterWorkItemsThatAlreadyExistInTarget = true)
        {
            _save = save;
            _filterWorkItemsThatAlreadyExistInTarget = filterWorkItemsThatAlreadyExistInTarget;
        }

        public override int Enrich(WorkItemData sourceWorkItemLinkStart, WorkItemData targetWorkItemLinkStart)
        {
            if (sourceWorkItemLinkStart is null)
            {
                throw new ArgumentNullException(nameof(sourceWorkItemLinkStart));
            }
            if (targetWorkItemLinkStart is null)
            {
                throw new ArgumentNullException(nameof(targetWorkItemLinkStart));
            }
            if (targetWorkItemLinkStart.Id == "0")
            {
                throw new IndexOutOfRangeException("Target work item must be saved before you can add a link");
            }

            if (ShouldCopyLinks(sourceWorkItemLinkStart, targetWorkItemLinkStart))
            {
                Trace.Indent();
                foreach (Link item in sourceWorkItemLinkStart.ToWorkItem().Links)
                {
                    try
                    {
                        Log.LogInformation("Migrating link for {sourceWorkItemLinkStartId} of type {ItemGetTypeName}", sourceWorkItemLinkStart.Id, item.GetType().Name);
                        if (IsHyperlink(item))
                        {
                            CreateHyperlink((Hyperlink)item, targetWorkItemLinkStart);
                        }
                        else if (IsRelatedLink(item))
                        {
                            RelatedLink rl = (RelatedLink)item;
                            CreateRelatedLink(sourceWorkItemLinkStart, rl, targetWorkItemLinkStart);
                        }
                        else if (IsExternalLink(item))
                        {
                            var el = (ExternalLink)item;
                            if (!IsBuildLink(el))
                            {
                                CreateExternalLink(el, targetWorkItemLinkStart);
                            }
                        }
                        else
                        {
                            UnknownLinkTypeException ex = new UnknownLinkTypeException(string.Format("  [UnknownLinkType] Unable to {0}", item.GetType().Name));
                            Log.LogError(ex, "LinkMigrationContext");
                            throw ex;
                        }
                    }
                    catch (WorkItemLinkValidationException ex)
                    {
                        sourceWorkItemLinkStart.ToWorkItem().Reset();
                        targetWorkItemLinkStart.ToWorkItem().Reset();
                        Log.LogError(ex, "[WorkItemLinkValidationException] Adding link for wiSourceL={sourceWorkItemLinkStartId}", sourceWorkItemLinkStart.Id);
                    }
                    catch (FormatException ex)
                    {
                        sourceWorkItemLinkStart.ToWorkItem().Reset();
                        targetWorkItemLinkStart.ToWorkItem().Reset();
                        Log.LogError(ex, "[CREATE-FAIL] Adding Link for wiSourceL={sourceWorkItemLinkStartId}", sourceWorkItemLinkStart.Id);
                    }
                }
            }
            if (sourceWorkItemLinkStart.Type == "Test Case")
            {
                MigrateSharedSteps(sourceWorkItemLinkStart, targetWorkItemLinkStart);
            }
            return 0;
        }

        private void MigrateSharedSteps(WorkItemData wiSourceL, WorkItemData wiTargetL)
        {
            const string microsoftVstsTcmSteps = "Microsoft.VSTS.TCM.Steps";
            var oldSteps = wiTargetL.ToWorkItem().Fields[microsoftVstsTcmSteps].Value.ToString();
            var newSteps = oldSteps;

            var sourceSharedStepLinks = wiSourceL.ToWorkItem().Links.OfType<RelatedLink>()
                .Where(x => x.LinkTypeEnd.Name == "Shared Steps").ToList();
            var sourceSharedSteps =
                sourceSharedStepLinks.Select(x => Engine.Source.WorkItems.GetWorkItem(x.RelatedWorkItemId.ToString()));

            foreach (WorkItemData sourceSharedStep in sourceSharedSteps)
            {
                WorkItemData matchingTargetSharedStep =
                    Engine.Target.WorkItems.FindReflectedWorkItemByReflectedWorkItemId(sourceSharedStep);

                if (matchingTargetSharedStep != null)
                {
                    newSteps = newSteps.Replace($"ref=\"{sourceSharedStep.Id}\"",
                        $"ref=\"{matchingTargetSharedStep.Id}\"");
                    wiTargetL.ToWorkItem().Fields[microsoftVstsTcmSteps].Value = newSteps;
                }
            }

            if (wiTargetL.ToWorkItem().IsDirty && _save)
            {
                wiTargetL.ToWorkItem().Fields["System.ChangedBy"].Value = "Migration";
                wiTargetL.SaveToAzureDevOps();
            }
        }

        private void CreateExternalLink(ExternalLink sourceLink, WorkItemData target)
        {
            var exist = (from Link l in target.ToWorkItem().Links
                         where l is ExternalLink && ((ExternalLink)l).LinkedArtifactUri == ((ExternalLink)sourceLink).LinkedArtifactUri
                         select (ExternalLink)l).SingleOrDefault();
            if (exist == null)
            {
                Log.LogInformation("Creating new {SourceLinkType} on {TargetId}", sourceLink.GetType().Name, target.Id);
                ExternalLink el = new ExternalLink(sourceLink.ArtifactLinkType, sourceLink.LinkedArtifactUri);
                el.Comment = sourceLink.Comment;
                target.ToWorkItem().Links.Add(el);
                if (_save)
                {
                    target.SaveToAzureDevOps();
                }
            }
            else
            {
                Log.LogInformation("Link {SourceLinkType} on {TargetId} already exists",
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

        private void CreateRelatedLink(WorkItemData wiSourceL, RelatedLink item, WorkItemData wiTargetL)
        {
            RelatedLink rl = (RelatedLink)item;
            WorkItemData wiSourceR = null;
            WorkItemData wiTargetR = null;
            try
            {
                wiSourceR = Engine.Source.WorkItems.GetWorkItem(rl.RelatedWorkItemId.ToString());
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "  [FIND-FAIL] Adding Link of type {0} where wiSourceL={1}, wiTargetL={2} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiTargetL.Id);
                return;
            }
            try
            {
                wiTargetR = GetRightHandSideTargetWi(wiSourceL, wiSourceR, wiTargetL);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "  [FIND-FAIL] Adding Link of type {0} where wiSourceL={1}, wiTargetL={2} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiTargetL.Id);
                return;

            }
            if (wiTargetR != null)
            {
                bool IsExisting = false;
                try
                {
                    var exist = (
                        from Link l in wiTargetL.ToWorkItem().Links
                        where l is RelatedLink
                            && ((RelatedLink)l).RelatedWorkItemId.ToString() == wiTargetR.Id
                            && ((RelatedLink)l).LinkTypeEnd.ImmutableName == item.LinkTypeEnd.ImmutableName
                        select (RelatedLink)l).SingleOrDefault();
                    IsExisting = (exist != null);
                }
                catch (Exception ex)
                {

                    Log.LogError(ex, "  [SKIP] Unable to migrate links where wiSourceL={0}, wiSourceR={1}, wiTargetL={2}", ((wiSourceL != null) ? wiSourceL.Id.ToString() : "NotFound"), ((wiSourceR != null) ? wiSourceR.Id.ToString() : "NotFound"), ((wiTargetL != null) ? wiTargetL.Id.ToString() : "NotFound"));
                    return;
                }

                if (!IsExisting && !wiTargetR.ToWorkItem().IsAccessDenied)
                {
                    if (wiSourceR.Id != wiTargetR.Id)
                    {
                        Log.LogInformation("  [CREATE-START] Adding Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id);
                        WorkItemLinkTypeEnd linkTypeEnd = ((WorkItemMigrationClient)Engine.Target.WorkItems).Store.WorkItemLinkTypes.LinkTypeEnds[rl.LinkTypeEnd.ImmutableName];
                        RelatedLink newRl = new RelatedLink(linkTypeEnd, int.Parse(wiTargetR.Id));
                        if (linkTypeEnd.ImmutableName == "System.LinkTypes.Hierarchy-Forward")
                        {
                            var potentialParentConflictLink = ( // TF201036: You cannot add a Child link between work items xxx and xxx because a work item can have only one Parent link.
                                    from Link l in wiTargetR.ToWorkItem().Links
                                    where l is RelatedLink
                                        && ((RelatedLink)l).LinkTypeEnd.ImmutableName == "System.LinkTypes.Hierarchy-Reverse"
                                    select (RelatedLink)l).SingleOrDefault();
                            if (potentialParentConflictLink != null)
                            {
                                wiTargetR.ToWorkItem().Links.Remove(potentialParentConflictLink);
                            }
                            linkTypeEnd = ((WorkItemMigrationClient)Engine.Target.WorkItems).Store.WorkItemLinkTypes.LinkTypeEnds["System.LinkTypes.Hierarchy-Reverse"];
                            RelatedLink newLl = new RelatedLink(linkTypeEnd, int.Parse(wiTargetL.Id));
                            wiTargetR.ToWorkItem().Links.Add(newLl);
                            wiTargetR.ToWorkItem().Fields["System.ChangedBy"].Value = "Migration";
                            wiTargetR.SaveToAzureDevOps();
                        }
                        else
                        {
                            if (linkTypeEnd.ImmutableName == "System.LinkTypes.Hierarchy-Reverse")
                            {
                                var potentialParentConflictLink = ( // TF201065: You can not add a Parent link to this work item because a work item can have only one link of this type.
                                    from Link l in wiTargetL.ToWorkItem().Links
                                    where l is RelatedLink
                                        && ((RelatedLink)l).LinkTypeEnd.ImmutableName == "System.LinkTypes.Hierarchy-Reverse"
                                    select (RelatedLink)l).SingleOrDefault();
                                if (potentialParentConflictLink != null)
                                {
                                    wiTargetL.ToWorkItem().Links.Remove(potentialParentConflictLink);
                                }
                            }
                            wiTargetL.ToWorkItem().Links.Add(newRl);
                            if (_save)
                            {
                                wiTargetL.ToWorkItem().Fields["System.ChangedBy"].Value = "Migration";
                                wiTargetL.SaveToAzureDevOps();
                            }
                        }
                        Log.LogInformation(
                                "  [CREATE-SUCCESS] Adding Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ",
                                rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id);

                    }
                    else
                    {
                        Log.LogInformation(
                                  "  [SKIP] Unable to migrate link where Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} as target WI has not been migrated",
                                  rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id);
                    }
                }
                else
                {
                    if (IsExisting)
                    {
                        Log.LogInformation("  [SKIP] Already Exists a Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id);
                    }
                    if (wiTargetR.ToWorkItem().IsAccessDenied)
                    {
                        Log.LogInformation("  [AccessDenied] The Target  work item is inaccessable to create a Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id);
                    }
                }
            }
            else
            {
                Log.LogInformation("  [SKIP] Cant find wiTargetR where wiSourceL={0}, wiSourceR={1}, wiTargetL={2}", wiSourceL.Id, wiSourceR.Id, wiTargetL.Id);
            }
        }

        private WorkItemData GetRightHandSideTargetWi(WorkItemData wiSourceL, WorkItemData wiSourceR, WorkItemData wiTargetL)
        {
            WorkItemData wiTargetR;
            if (!(wiTargetL == null)
                && wiSourceR.ToWorkItem().Project.Name == wiTargetL.ToWorkItem().Project.Name
                && wiSourceR.ToWorkItem().Project.Store.TeamProjectCollection.Uri.ToString().Replace("/", "") == wiTargetL.ToWorkItem().Project.Store.TeamProjectCollection.Uri.ToString().Replace("/", ""))
            {
                // Moving to same team project as SourceR
                wiTargetR = wiSourceR;
            }
            else
            {
                // Moving to Other Team Project from Source
                wiTargetR = Engine.Target.WorkItems.FindReflectedWorkItem(wiSourceR, true);
                if (wiTargetR == null) // Assume source only (other team project)
                {
                    wiTargetR = wiSourceR;
                    if (wiTargetR.ToWorkItem().Project.Store.TeamProjectCollection.Uri.ToString().Replace("/", "") != wiSourceR.ToWorkItem().Project.Store.TeamProjectCollection.Uri.ToString().Replace("/", ""))
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

        private void CreateHyperlink(Hyperlink sourceLink, WorkItemData target)
        {
            var exist = (from Link l in target.ToWorkItem().Links where l is Hyperlink && ((Hyperlink)l).Location == ((Hyperlink)sourceLink).Location select (Hyperlink)l).SingleOrDefault();
            if (exist == null)
            {
                Hyperlink hl = new Hyperlink(sourceLink.Location);
                hl.Comment = sourceLink.Comment;
                target.ToWorkItem().Links.Add(hl);
                if (_save)
                {
                    target.SaveToAzureDevOps();
                }
            }
        }

        private bool ShouldCopyLinks(WorkItemData sourceWorkItemLinkStart, WorkItemData targetWorkItemLinkStart)
        {
            if (_filterWorkItemsThatAlreadyExistInTarget)
            {
                if (targetWorkItemLinkStart.ToWorkItem().Links.Count == sourceWorkItemLinkStart.ToWorkItem().Links.Count) // we should never have this as the target should not have existed in this path
                {
                    Log.LogInformation("[SKIP] Source and Target have same number of links  {sourceWorkItemLinkStartId} - {sourceWorkItemLinkStartType}", sourceWorkItemLinkStart.Id, sourceWorkItemLinkStart.Type.ToString());
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
