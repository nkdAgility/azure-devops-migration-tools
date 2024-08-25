using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Clients;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Exceptions;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    public class TfsWorkItemLinkTool : Tool<TfsWorkItemLinkToolOptions>
    {
        private IMigrationEngine Engine;

        public TfsWorkItemLinkTool(IOptions<TfsWorkItemLinkToolOptions> options, IServiceProvider services, ILogger<TfsWorkItemLinkTool> logger, ITelemetryLogger telemetryLogger)
            : base(options, services, logger, telemetryLogger)
        {

        }

        public  int Enrich(TfsProcessor processor, WorkItemData sourceWorkItemLinkStart, WorkItemData targetWorkItemLinkStart)
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
                Log.LogWarning("TfsWorkItemLinkTool::Enrich: Target work item must be saved before you can add a link: exiting Link Migration");
                return 0;
            }

            if (ShouldCopyLinks(sourceWorkItemLinkStart, targetWorkItemLinkStart))
            {
                Log.LogTrace("Links = '{@sourceWorkItemLinkStartLinks}", sourceWorkItemLinkStart.Links);
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
                    catch (UnexpectedErrorException ex)
                    {
                        sourceWorkItemLinkStart.ToWorkItem().Reset();
                        targetWorkItemLinkStart.ToWorkItem().Reset();
                        Log.LogError(ex, "[UnexpectedErrorException] Adding Link for wiSourceL={sourceWorkItemLinkStartId}", sourceWorkItemLinkStart.Id);
                    }
                }
            }
            if (sourceWorkItemLinkStart.Type == "Test Case")
            {
                MigrateSharedSteps(sourceWorkItemLinkStart, targetWorkItemLinkStart);
                MigrateSharedParameters(sourceWorkItemLinkStart, targetWorkItemLinkStart);
            }
            return 0;
        }

        public void MigrateSharedSteps(TfsProcessor processor, WorkItemData wiSourceL, WorkItemData wiTargetL)
        {
            const string microsoftVstsTcmSteps = "Microsoft.VSTS.TCM.Steps";
            var oldSteps = wiTargetL.ToWorkItem().Fields[microsoftVstsTcmSteps].Value.ToString();
            var newSteps = oldSteps;

            var sourceSharedStepLinks = wiSourceL.ToWorkItem().Links.OfType<RelatedLink>()
                .Where(x => x.LinkTypeEnd.Name == "Shared Steps").ToList();
            var sourceSharedSteps =
                sourceSharedStepLinks.Select(x => processor.Source.WorkItems.GetWorkItem(x.RelatedWorkItemId.ToString()));

            foreach (WorkItemData sourceSharedStep in sourceSharedSteps)
            {
                WorkItemData matchingTargetSharedStep =
                    processor.Target.WorkItems.FindReflectedWorkItemByReflectedWorkItemId(sourceSharedStep);

                if (matchingTargetSharedStep != null)
                {
                    newSteps = newSteps.Replace($"ref=\"{sourceSharedStep.Id}\"",
                        $"ref=\"{matchingTargetSharedStep.Id}\"");
                    wiTargetL.ToWorkItem().Fields[microsoftVstsTcmSteps].Value = newSteps;

                    // DevOps doesn't seem to take impersonation very nicely here - as of 13.05.22 the following line would get you an error:
                    // System.FormatException: The string 'Microsoft.TeamFoundation.WorkItemTracking.Common.ServerDefaultFieldValue' is not a valid AllXsd value.
                    // target.ToWorkItem().Fields["System.ModifiedBy"].Value = "Migration";
                }
            }

            if (wiTargetL.ToWorkItem().IsDirty && Options.SaveAfterEachLinkIsAdded)
            {
                wiTargetL.SaveToAzureDevOps();
            }
        }

        public void MigrateSharedParameters(TfsProcessor processor, WorkItemData wiSourceL, WorkItemData wiTargetL)
        {
            const string microsoftVstsTcmLocalDataSource = "Microsoft.VSTS.TCM.LocalDataSource";
            var oldSteps = wiTargetL.ToWorkItem().Fields[microsoftVstsTcmLocalDataSource].Value.ToString();
            var newSteps = oldSteps;

            var sourceSharedParametersLinks = wiSourceL.ToWorkItem().Links.OfType<RelatedLink>()
                .Where(x => x.LinkTypeEnd.ImmutableName == "Microsoft.VSTS.TestCase.SharedParameterReferencedBy-Reverse").ToList();
            var sourceSharedParameters =
                sourceSharedParametersLinks.Select(x => processor.Source.WorkItems.GetWorkItem(x.RelatedWorkItemId.ToString()));

            foreach (WorkItemData sourceSharedParameter in sourceSharedParameters)
            {
                WorkItemData matchingTargetSharedParameter =
                    processor.Target.WorkItems.FindReflectedWorkItemByReflectedWorkItemId(sourceSharedParameter);

                if (matchingTargetSharedParameter != null)
                {
                    newSteps = newSteps.Replace($"sharedParameterDataSetId\":{sourceSharedParameter.Id}",
                        $"sharedParameterDataSetId\":{matchingTargetSharedParameter.Id}");
                    newSteps = newSteps.Replace($"sharedParameterDataSetIds\":[{sourceSharedParameter.Id}]",
                        $"sharedParameterDataSetIds\":[{matchingTargetSharedParameter.Id}]");
                    wiTargetL.ToWorkItem().Fields[microsoftVstsTcmLocalDataSource].Value = newSteps;
                }
            }

            if (wiTargetL.ToWorkItem().IsDirty && Options.SaveAfterEachLinkIsAdded)
            {
                wiTargetL.SaveToAzureDevOps();
            }
        }

        private void CreateExternalLink(ExternalLink sourceLink, WorkItemData target)
        {
            var exist = (from Link l in target.ToWorkItem().Links
                         where l is ExternalLink && ((ExternalLink)l).LinkedArtifactUri == sourceLink.LinkedArtifactUri
                         select (ExternalLink)l).SingleOrDefault();
            if (exist == null)
            {
                Log.LogInformation("Creating new {SourceLinkType} on {TargetId}", sourceLink.GetType().Name, target.Id);
                ExternalLink el = new ExternalLink(sourceLink.ArtifactLinkType, sourceLink.LinkedArtifactUri)
                {
                    Comment = sourceLink.Comment
                };
                target.ToWorkItem().Links.Add(el);

                // DevOps doesn't seem to take impersonation very nicely here - as of 13.05.22 the following line would get you an error:
                // System.FormatException: The string 'Microsoft.TeamFoundation.WorkItemTracking.Common.ServerDefaultFieldValue' is not a valid AllXsd value.
                // target.ToWorkItem().Fields["System.ModifiedBy"].Value = "Migration";
                if (Options.SaveAfterEachLinkIsAdded)
                {
                    try
                    {
                        target.SaveToAzureDevOps();
                    }
                    catch (Exception ex)
                    {
                        Telemetry.TrackException(ex, null, null);
                        // Ignore this link because the TFS server didn't recognize its type (There's no point in crashing the rest of the migration due to a link)
                        if (ex.Message.Contains("Unrecognized Resource link"))
                        {

                            Log.LogError(ex, "[{ExceptionType}] Failed to save link {SourceLinkType} on {TargetId}", ex.GetType().Name, sourceLink.GetType().Name, target.Id);
                            // Remove the link from the target so it doesn't cause problems downstream
                            target.ToWorkItem().Links.Remove(el);
                        }
                        else
                        {
                            //pass along the exception since we don't know what went wrong
                            throw;
                        }
                    }
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

        private void CreateRelatedLink(TfsProcessor processor, WorkItemData wiSourceL, RelatedLink item, WorkItemData wiTargetL)
        {
            RelatedLink rl = item;
            WorkItemData wiSourceR = null;
            WorkItemData wiTargetR = null;

            Log.LogDebug("RelatedLink is of ArtifactLinkType='{ArtifactLinkType}':LinkTypeEnd='{LinkTypeEndImmutableName}' on WorkItemId s:{ids} t:{idt}", rl.ArtifactLinkType.Name, rl.LinkTypeEnd == null ? "null" : rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiTargetL.Id);

            if (rl.LinkTypeEnd != null) // On a registered link type these will for sure fail as target is not in the system.
            {
                try
                {
                    wiSourceR = processor.Source.WorkItems.GetWorkItem(rl.RelatedWorkItemId.ToString());
                }
                catch (Exception ex)
                {
                    Telemetry.TrackException(ex, null, null);
                    Log.LogError(ex, "  [FIND-FAIL] Adding Link of type {0} where wiSourceL={1}, wiTargetL={2} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiTargetL.Id);
                    return;
                }
                try
                {
                    wiTargetR = GetRightHandSideTargetWi(processor, wiSourceR, wiTargetL);
                }
                catch (Exception ex)
                {
                    Telemetry.TrackException(ex, null, null);
                    Log.LogError(ex, "  [FIND-FAIL] Adding Link of type {0} where wiSourceL={1}, wiTargetL={2} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiTargetL.Id);
                    return;
                }
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
                    IsExisting = exist != null;
                }
                catch (Exception ex)
                {
                    Log.LogWarning(ex, "  [SKIP] Unable to migrate links where wiSourceL={0}, wiSourceR={1}, wiTargetL={2}", wiSourceL != null ? wiSourceL.Id.ToString() : "NotFound", wiSourceR != null ? wiSourceR.Id.ToString() : "NotFound", wiTargetL != null ? wiTargetL.Id.ToString() : "NotFound");
                    return;
                }

                if (!IsExisting && !wiTargetR.ToWorkItem().IsAccessDenied)
                {
                    if (wiSourceR.Id != wiTargetR.Id)
                    {
                        Log.LogInformation("  [CREATE-START] Adding Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id);
                        var client = (TfsWorkItemMigrationClient)processor.Target.WorkItems;
                        if (!client.Store.WorkItemLinkTypes.LinkTypeEnds.Contains(rl.LinkTypeEnd.ImmutableName))
                        {
                            Log.LogWarning($"  [SKIP] Unable to migrate Link because type {rl.LinkTypeEnd.ImmutableName} does not exist in the target project.");
                            return;
                        }

                        WorkItemLinkTypeEnd linkTypeEnd = client.Store.WorkItemLinkTypes.LinkTypeEnds[rl.LinkTypeEnd.ImmutableName];
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
                            linkTypeEnd = ((TfsWorkItemMigrationClient)processor.Target.WorkItems).Store.WorkItemLinkTypes.LinkTypeEnds["System.LinkTypes.Hierarchy-Reverse"];
                            RelatedLink newLl = new RelatedLink(linkTypeEnd, int.Parse(wiTargetL.Id));
                            wiTargetR.ToWorkItem().Links.Add(newLl);

                            // DevOps doesn't seem to take impersonation very nicely here - as of 13.05.22 the following line would get you an error:
                            // System.FormatException: The string 'Microsoft.TeamFoundation.WorkItemTracking.Common.ServerDefaultFieldValue' is not a valid AllXsd value.
                            // wiTargetR.ToWorkItem().Fields["System.ModifiedBy"].Value = "Migration";
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

                            // DevOps doesn't seem to take impersonation very nicely here - as of 13.05.22 the following line would get you an error:
                            // System.FormatException: The string 'Microsoft.TeamFoundation.WorkItemTracking.Common.ServerDefaultFieldValue' is not a valid AllXsd value.
                            // wiTargetL.ToWorkItem().Fields["System.ModifiedBy"].Value = "Migration";
                            if (Options.SaveAfterEachLinkIsAdded)
                            {
                                wiTargetL.SaveToAzureDevOps();
                            }
                        }
                        Log.LogInformation(
                                "  [CREATE-SUCCESS] Adding Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ",
                                rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id);
                    }
                    else
                    {
                        Log.LogWarning(
                                  "  [SKIP] Unable to migrate link where Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} as target WI has not been migrated",
                                  rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id);
                    }
                }
                else
                {
                    if (IsExisting)
                    {
                        Log.LogWarning("  [SKIP] Already Exists a Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id);
                    }
                    if (wiTargetR.ToWorkItem().IsAccessDenied)
                    {
                        Log.LogInformation("  [AccessDenied] The Target  work item is inaccessable to create a Link of type {0} where wiSourceL={1}, wiSourceR={2}, wiTargetL={3}, wiTargetR={4} ", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id);
                    }
                }
            }
            else
            {
                Log.LogWarning("[SKIP] [LINK_CAPTURE_RELATED] [{RegisteredLinkType}] target not found. wiSourceL={wiSourceL}, wiSourceR={wiSourceR}, wiTargetL={wiTargetL}", rl.ArtifactLinkType.GetType().Name, wiSourceL == null ? "null" : wiSourceL.Id, wiSourceR == null ? "null" : wiSourceR.Id, wiTargetL == null ? "null" : wiTargetL.Id);
            }
        }

        private WorkItemData GetRightHandSideTargetWi(TfsProcessor processor, WorkItemData wiSourceR, WorkItemData wiTargetL)
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
                wiTargetR = processor.Target.WorkItems.FindReflectedWorkItem(wiSourceR, true);
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
            var sourceLinkAbsoluteUri = GetAbsoluteUri(sourceLink);
            if (string.IsNullOrEmpty(sourceLinkAbsoluteUri))
            {
                Log.LogWarning("  [SKIP] Unable to create a hyperlink to [{0}]", sourceLink.Location);
                return;
            }

            var exist = (from hyperlink in target.ToWorkItem().Links.OfType<Hyperlink>()
                         let absoluteUri = GetAbsoluteUri(hyperlink)
                         where string.Equals(sourceLinkAbsoluteUri, absoluteUri, StringComparison.OrdinalIgnoreCase)
                         select hyperlink).Any();

            if (exist)
            {
                return;
            }

            var hl = new Hyperlink(sourceLinkAbsoluteUri) // Use AbsoluteUri here as a possible \\UNC\Path\Link will be converted to file://UNC/Path/Link this way
            {
                Comment = sourceLink.Comment
            };

            target.ToWorkItem().Links.Add(hl);

            // DevOps doesn't seem to take impersonation very nicely here - as of 13.05.22 the following line would get you an error:
            // System.FormatException: The string 'Microsoft.TeamFoundation.WorkItemTracking.Common.ServerDefaultFieldValue' is not a valid AllXsd value.
            // target.ToWorkItem().Fields["System.ModifiedBy"].Value = "Migration";
            if (Options.SaveAfterEachLinkIsAdded)
            {
                target.SaveToAzureDevOps();
            }
        }

        private string GetAbsoluteUri(Hyperlink hyperlink)
        {
            try
            {
                return new Uri(hyperlink.Location.Trim('"')).AbsoluteUri;
            }
            catch (UriFormatException e)
            {
                Log.LogError("Unable to get AbsoluteUri of [{0}]: {1}", hyperlink.Location, e.Message);
                return null;
            }
        }

        private bool ShouldCopyLinks(WorkItemData sourceWorkItemLinkStart, WorkItemData targetWorkItemLinkStart)
        {
            if (Options.FilterIfLinkCountMatches)
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