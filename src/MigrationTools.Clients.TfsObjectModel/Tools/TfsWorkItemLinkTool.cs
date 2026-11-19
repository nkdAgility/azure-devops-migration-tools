using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Clients;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Exceptions;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Tool for migrating work item links and relationships between work items, including shared steps and parameters for test cases.
    /// </summary>
    public class TfsWorkItemLinkTool : Tool<TfsWorkItemLinkToolOptions>
    {
        private bool _inline;
        private readonly HashSet<string> _skippedChildLinks = new HashSet<string>();

        private bool SaveAfterEachLink => Options.SaveAfterEachLinkIsAdded && !_inline;

        /// <summary>
        /// True when the last inline <see cref="Enrich"/> skipped a child link because the target was not saved yet and the
        /// child already has another parent. Call <see cref="Enrich"/> again (not inline) after saving the target (#3192).
        /// </summary>
        public bool HasDeferredLinks { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TfsWorkItemLinkTool class.
        /// </summary>
        /// <param name="options">Configuration options for the work item link tool</param>
        /// <param name="services">Service provider for dependency injection</param>
        /// <param name="logger">Logger for the tool operations</param>
        /// <param name="telemetryLogger">Telemetry logger for tracking operations</param>
        public TfsWorkItemLinkTool(IOptions<TfsWorkItemLinkToolOptions> options, IServiceProvider services, ILogger<TfsWorkItemLinkTool> logger, ITelemetryLogger telemetryLogger)
            : base(options, services, logger, telemetryLogger)
        {

        }

        /// <summary>
        /// Enriches the target work item with links from the source work item, processing all link types and relationships.
        /// </summary>
        /// <param name="processor">The TFS processor performing the migration</param>
        /// <param name="sourceWorkItemLinkStart">The source work item containing links to migrate</param>
        /// <param name="targetWorkItemLinkStart">The target work item to receive the links</param>
        /// <param name="inline">
        /// True when called during revision replay (#3192): the links are saved as part of the revision being replayed,
        /// so nothing is saved here (SaveAfterEachLinkIsAdded is ignored), the target may not have been saved yet,
        /// and a child link is added on the target itself rather than by saving the child.
        /// </param>
        /// <returns>The number of links processed</returns>
        /// <exception cref="ArgumentNullException">Thrown when source or target work items are null</exception>
        public  int Enrich(TfsProcessor processor, WorkItemData sourceWorkItemLinkStart, WorkItemData targetWorkItemLinkStart, bool inline = false)
        {
            if (sourceWorkItemLinkStart is null)
            {
                throw new ArgumentNullException(nameof(sourceWorkItemLinkStart));
            }
            if (targetWorkItemLinkStart is null)
            {
                throw new ArgumentNullException(nameof(targetWorkItemLinkStart));
            }
            _inline = inline;
            HasDeferredLinks = false;
            if (targetWorkItemLinkStart.Id == "0" && !inline)
            {
                Log.LogWarning("TfsWorkItemLinkTool::Enrich: Target work item must be saved before you can add a link: exiting Link Migration");
                return 0;
            }

            if (ShouldCopyLinks(processor, sourceWorkItemLinkStart, targetWorkItemLinkStart))
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
                            CreateRelatedLink(processor, sourceWorkItemLinkStart, rl, targetWorkItemLinkStart);
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
                    catch (Exception ex) when (ex is WorkItemLinkValidationException || ex is FormatException || ex is UnexpectedErrorException || ex is InvalidOperationException || ex is ValidationException)
                    {
                        // The target is not reset: that would also drop the links added before this one.
                        sourceWorkItemLinkStart.ToWorkItem().Reset();
                        Log.LogWarning("  [SKIP] A {LinkType} on source {SourceId} could not be added to target {TargetId} and was skipped: {Reason} Check the link in the source; the rest of the work item carries on migrating.", item.GetType().Name, sourceWorkItemLinkStart.Id, targetWorkItemLinkStart.Id, ex.Message);
                        Log.LogDebug(ex, "Adding a link for source {SourceId}", sourceWorkItemLinkStart.Id);
                    }
                }
            }
            if (sourceWorkItemLinkStart.Type == "Test Case")
            {
                MigrateSharedSteps(processor, sourceWorkItemLinkStart, targetWorkItemLinkStart);
                MigrateSharedParameters(processor, sourceWorkItemLinkStart, targetWorkItemLinkStart);
            }
            return 0;
        }

        /// <summary>
        /// Says, from the loaded items and the source → target id map only, which link of the source the target lacks (#3299).
        /// A link to a work item that is not in the target is not counted: the link migration could not add it either.
        /// </summary>
        /// <returns>A description of the first missing link, or null when the target has every link of the source.</returns>
        public string FindMissingLink(TfsProcessor processor, WorkItemData sourceWorkItem, WorkItemData targetWorkItem, ReflectedWorkItemIdMap map)
        {
            var source = sourceWorkItem.ToWorkItem();
            var targetLinks = targetWorkItem.ToWorkItem().Links.Cast<Link>().ToList();
            var targetRelated = new HashSet<string>(
                targetLinks.OfType<RelatedLink>().Where(t => t.LinkTypeEnd != null)
                    .Select(t => RevisionArtifactDiff.RelatedLinkKey(t.LinkTypeEnd.ImmutableName, t.RelatedWorkItemId)),
                StringComparer.OrdinalIgnoreCase);
            var targetHyperlinks = new HashSet<string>(
                targetLinks.OfType<Hyperlink>().Select(t => RevisionArtifactDiff.HyperlinkKey(GetAbsoluteUri(t) ?? t.Location)),
                StringComparer.OrdinalIgnoreCase);
            var targetExternal = new HashSet<string>(
                targetLinks.OfType<ExternalLink>().Select(t => RevisionArtifactDiff.ExternalLinkKey(t.LinkedArtifactUri)),
                StringComparer.OrdinalIgnoreCase);
            var sourceCollection = source.Store.TeamProjectCollection.Uri;

            foreach (Link sourceLink in source.Links)
            {
                switch (sourceLink)
                {
                    case Hyperlink hl:
                        var uri = GetAbsoluteUri(hl);
                        if (!string.IsNullOrEmpty(uri) && IsValidHyperlink(uri) && !targetHyperlinks.Contains(RevisionArtifactDiff.HyperlinkKey(uri)))
                        {
                            return $"hyperlink {uri}";
                        }
                        break;
                    case RelatedLink rl when rl.LinkTypeEnd != null:
                        if (map.TryGetTargetId(sourceCollection, rl.RelatedWorkItemId, out var relatedTargetId)
                            && !targetRelated.Contains(RevisionArtifactDiff.RelatedLinkKey(rl.LinkTypeEnd.ImmutableName, relatedTargetId)))
                        {
                            return $"{rl.LinkTypeEnd.ImmutableName} link to source {rl.RelatedWorkItemId} (target {relatedTargetId})";
                        }
                        break;
                    case ExternalLink el when !IsBuildLink(el):
                        if (!HasExternalLink(processor, sourceWorkItem, el, targetExternal, targetHyperlinks))
                        {
                            return $"external link {el.LinkedArtifactUri}";
                        }
                        break;
                }
            }
            return null;
        }

        private bool HasExternalLink(TfsProcessor processor, WorkItemData sourceWorkItem, ExternalLink el, HashSet<string> targetExternal, HashSet<string> targetHyperlinks)
        {
            if (targetExternal.Contains(RevisionArtifactDiff.ExternalLinkKey(el.LinkedArtifactUri)))
            {
                return true;
            }
            var git = Services.GetRequiredService<TfsGitRepositoryTool>();
            var targetUri = git.GetTargetUri(processor, sourceWorkItem, el);
            if (targetUri != null && targetExternal.Contains(RevisionArtifactDiff.ExternalLinkKey(targetUri)))
            {
                return true;
            }
            var url = git.GetPullRequestHyperlinkUrl(processor, el);
            return url != null && targetHyperlinks.Contains(RevisionArtifactDiff.HyperlinkKey(url));
        }

        /// <summary>
        /// Migrates shared steps for test case work items, updating step references in the target work item.
        /// </summary>
        /// <param name="processor">The TFS processor performing the migration</param>
        /// <param name="wiSourceL">The source test case work item</param>
        /// <param name="wiTargetL">The target test case work item</param>
        public void MigrateSharedSteps(TfsProcessor processor, WorkItemData wiSourceL, WorkItemData wiTargetL)
        {
            const string microsoftVstsTcmSteps = "Microsoft.VSTS.TCM.Steps";
            var oldSteps = wiTargetL.ToWorkItem().Fields[microsoftVstsTcmSteps].Value.ToString();
            var newSteps = oldSteps;

            var sourceSharedStepLinks = wiSourceL.ToWorkItem().Links.OfType<RelatedLink>()
                .Where(x => x.LinkTypeEnd.Name == "Shared Steps").ToList();
            var sourceSharedSteps = ReadableSourceItems(processor, sourceSharedStepLinks, wiSourceL, wiTargetL, "Shared Steps",
                "its step reference keeps the source id");

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

            if (wiTargetL.ToWorkItem().IsDirty && SaveAfterEachLink)
            {
                wiTargetL.SaveToAzureDevOps();
            }
        }

        private List<WorkItemData> ReadableSourceItems(TfsProcessor processor, List<RelatedLink> links, WorkItemData wiSourceL, WorkItemData wiTargetL, string kind, string consequence)
        {
            var client = (TfsWorkItemMigrationClient)processor.Source.WorkItems;
            var items = new List<WorkItemData>();
            foreach (var link in links)
            {
                var item = client.GetWorkItemIfExists(link.RelatedWorkItemId);
                if (item == null)
                {
                    Log.LogWarning("  [SKIP] {Kind} {RelatedId} used by Test Case {SourceId} (target {TargetId}) not remapped: it does not exist in the source or the migration identity cannot read it (TF26198), so {Consequence}. Grant the identity read access to its project, or migrate it, and re-run.", kind, link.RelatedWorkItemId, wiSourceL.Id, wiTargetL.Id, consequence);
                    continue;
                }
                items.Add(item);
            }
            return items;
        }

        /// <summary>
        /// Migrates shared parameters for test case work items, updating parameter references in the target work item.
        /// </summary>
        /// <param name="processor">The TFS processor performing the migration</param>
        /// <param name="wiSourceL">The source test case work item</param>
        /// <param name="wiTargetL">The target test case work item</param>
        public void MigrateSharedParameters(TfsProcessor processor, WorkItemData wiSourceL, WorkItemData wiTargetL)
        {
            const string microsoftVstsTcmLocalDataSource = "Microsoft.VSTS.TCM.LocalDataSource";
            var sourceSharedParametersLinks = wiSourceL.ToWorkItem().Links.OfType<RelatedLink>()
                .Where(x => x.LinkTypeEnd.ImmutableName == "Microsoft.VSTS.TestCase.SharedParameterReferencedBy-Reverse").ToList();
            var sourceSharedParameters = ReadableSourceItems(processor, sourceSharedParametersLinks, wiSourceL, wiTargetL, "Shared Parameter",
                "its data source reference keeps the source id");

            var sourceToTarget = new Dictionary<string, string>();
            foreach (WorkItemData sourceSharedParameter in sourceSharedParameters)
            {
                WorkItemData matchingTargetSharedParameter =
                    processor.Target.WorkItems.FindReflectedWorkItemByReflectedWorkItemId(sourceSharedParameter);
                if (matchingTargetSharedParameter != null)
                {
                    sourceToTarget[sourceSharedParameter.Id] = matchingTargetSharedParameter.Id;
                }
            }

            if (sourceToTarget.Count > 0)
            {
                // Remap from the source value so a re-run never translates an ID that is already a target ID.
                var sourceDataSource = wiSourceL.ToWorkItem().Fields[microsoftVstsTcmLocalDataSource].Value?.ToString();
                var newDataSource = SharedParameterDataSource.Remap(sourceDataSource, sourceToTarget);
                var targetField = wiTargetL.ToWorkItem().Fields[microsoftVstsTcmLocalDataSource];
                if (newDataSource != targetField.Value?.ToString())
                {
                    targetField.Value = newDataSource;
                }
            }

            if (wiTargetL.ToWorkItem().IsDirty && SaveAfterEachLink)
            {
                wiTargetL.SaveToAzureDevOps();
            }
        }

        private void CreateExternalLink(ExternalLink sourceLink, WorkItemData target)
        {
            var exist = (from Link l in target.ToWorkItem().Links
                         where l is ExternalLink && ((ExternalLink)l).LinkedArtifactUri == sourceLink.LinkedArtifactUri
                         select (ExternalLink)l).FirstOrDefault();
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
                if (SaveAfterEachLink)
                {
                    try
                    {
                        target.SaveToAzureDevOps();
                    }
                    catch (Exception ex)
                    {
                        Telemetry.TrackException(ex, null);
                        // A rejected link must not stay pending, or every later save of this target fails with it.
                        target.ToWorkItem().Links.Remove(el);
                        if (!ex.Message.Contains("Unrecognized Resource link"))
                        {
                            throw;
                        }
                        Log.LogWarning("  [SKIP] {SourceLinkType} on target {TargetId} not migrated: the target does not recognise its link type ({Reason}).", sourceLink.GetType().Name, target.Id, ex.Message);
                        Log.LogDebug(ex, "Saving {SourceLinkType} on {TargetId}", sourceLink.GetType().Name, target.Id);
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
                    wiSourceR = ((TfsWorkItemMigrationClient)processor.Source.WorkItems).GetWorkItemIfExists(rl.RelatedWorkItemId);
                }
                catch (MigrationRunStoppedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Telemetry.TrackException(ex, null);
                    Log.LogWarning("  [SKIP] Link of type {LinkType} on source {SourceId} not added to target {TargetId}: the linked work item {RelatedId} could not be found ({Reason}).", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiTargetL.Id, rl.RelatedWorkItemId, ex.Message);
                    Log.LogDebug(ex, "Finding linked work item {RelatedId}", rl.RelatedWorkItemId);
                    return;
                }
                if (wiSourceR == null)
                {
                    Log.LogWarning("  [SKIP] Link of type {LinkType} from source {SourceId} to {RelatedId} not migrated: work item {RelatedId} does not exist in the source or the migration identity cannot read it (TF26198).", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, rl.RelatedWorkItemId, rl.RelatedWorkItemId);
                    return;
                }
                try
                {
                    wiTargetR = GetRightHandSideTargetWi(processor, wiSourceR, wiTargetL);
                }
                catch (MigrationRunStoppedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Telemetry.TrackException(ex, null);
                    Log.LogWarning("  [SKIP] Link of type {LinkType} on source {SourceId} not added to target {TargetId}: the linked work item {RelatedId} could not be found ({Reason}).", rl.LinkTypeEnd.ImmutableName, wiSourceL.Id, wiTargetL.Id, rl.RelatedWorkItemId, ex.Message);
                    Log.LogDebug(ex, "Finding linked work item {RelatedId}", rl.RelatedWorkItemId);
                    return;
                }
            }

            if (wiTargetR != null)
            {
                bool IsExisting = false;
                try
                {
                    IsExisting = wiTargetL.ToWorkItem().Links.OfType<RelatedLink>().Any(l =>
                        l.RelatedWorkItemId.ToString() == wiTargetR.Id
                        && l.LinkTypeEnd != null
                        && l.LinkTypeEnd.ImmutableName == item.LinkTypeEnd.ImmutableName);
                }
                catch (Exception ex)
                {
                    Log.LogWarning("  [SKIP] Link of type {LinkType} from source {SourceId} to {RelatedId} not migrated: the existing links on target {TargetId} could not be read ({Reason}).", item.LinkTypeEnd?.ImmutableName, wiSourceL?.Id, wiSourceR?.Id, wiTargetL?.Id, ex.Message);
                    Log.LogDebug(ex, "Reading the links on target {TargetId}", wiTargetL?.Id);
                    return;
                }

                if (!IsExisting && !wiTargetR.ToWorkItem().IsAccessDenied)
                {
                    // GetRightHandSideTargetWi returns the source item itself when no target was found. Ids can't show that:
                    // the target item can have the same id in another collection.
                    if (!ReferenceEquals(wiSourceR, wiTargetR))
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
                        List<RelatedLink> childParentLinks = new List<RelatedLink>();
                        if (linkTypeEnd.ImmutableName == HierarchyForward)
                        {
                            RefreshLinks(wiTargetR);
                            childParentLinks = ParentLinks(wiTargetR);
                            var action = ChildLinkDecision.Decide(
                                int.Parse(wiTargetL.Id),
                                childParentLinks.Select(l => l.RelatedWorkItemId),
                                int.Parse(wiSourceL.Id),
                                ParentLinks(wiSourceR).Select(l => l.RelatedWorkItemId),
                                _inline);
                            switch (action)
                            {
                                case ChildLinkAction.AlreadyLinked:
                                    Log.LogInformation("  [SKIP] Child {ChildTargetId} is already under {ParentTargetId}.", wiTargetR.Id, wiTargetL.Id);
                                    return;
                                case ChildLinkAction.SkipChildHasFinalParentElsewhere:
                                    // Every replayed revision that still holds the link meets it again: warn once.
                                    var level = _skippedChildLinks.Add($"{wiSourceL.Id}>{wiSourceR.Id}") ? LogLevel.Warning : LogLevel.Debug;
                                    Log.Log(level, "  [SKIP] Child link from source {SourceId} to {ChildSourceId} not added to target {TargetId}: the child has since moved to another parent in the source, and a work item can have only one parent. Target child {ChildTargetId} keeps its parent {CurrentParentIds}.",
                                        wiSourceL.Id, wiSourceR.Id, wiTargetL.Id, wiTargetR.Id, string.Join(", ", childParentLinks.Select(l => l.RelatedWorkItemId)));
                                    return;
                                case ChildLinkAction.DeferUntilParentSaved:
                                    Log.LogInformation("  [DEFER] Child {wiTargetR} already has another parent and {wiSourceL} is not saved yet; the link will be added after this revision is saved.", wiTargetR.Id, wiSourceL.Id);
                                    HasDeferredLinks = true;
                                    return;
                            }
                        }
                        if (linkTypeEnd.ImmutableName == HierarchyForward && (!_inline || childParentLinks.Count > 0))
                        {
                            // Saving the child creates a revision on it dated now, so during replay this is only
                            // done when the child must move here from a parent it no longer has in the source.
                            foreach (var conflict in childParentLinks)
                            {
                                wiTargetR.ToWorkItem().Links.Remove(conflict);
                            }
                            linkTypeEnd = ((TfsWorkItemMigrationClient)processor.Target.WorkItems).Store.WorkItemLinkTypes.LinkTypeEnds[HierarchyReverse];
                            RelatedLink newLl = new RelatedLink(linkTypeEnd, int.Parse(wiTargetL.Id));
                            wiTargetR.ToWorkItem().Links.Add(newLl);

                            // DevOps doesn't seem to take impersonation very nicely here - as of 13.05.22 the following line would get you an error:
                            // System.FormatException: The string 'Microsoft.TeamFoundation.WorkItemTracking.Common.ServerDefaultFieldValue' is not a valid AllXsd value.
                            // wiTargetR.ToWorkItem().Fields["System.ModifiedBy"].Value = "Migration";
                            try
                            {
                                wiTargetR.SaveToAzureDevOps();
                            }
                            catch
                            {
                                // Keep only the child's saved parent, so a later read does not see the unsaved move.
                                wiTargetR.ToWorkItem().Reset();
                                throw;
                            }
                        }
                        else
                        {
                            var removedParents = new List<RelatedLink>();
                            if (linkTypeEnd.ImmutableName == HierarchyReverse)
                            {
                                // TF201065: a work item can have only one parent, so this item's own history decides: drop every earlier one.
                                removedParents = ParentLinks(wiTargetL);
                                foreach (var conflict in removedParents)
                                {
                                    wiTargetL.ToWorkItem().Links.Remove(conflict);
                                }
                            }
                            wiTargetL.ToWorkItem().Links.Add(newRl);

                            // DevOps doesn't seem to take impersonation very nicely here - as of 13.05.22 the following line would get you an error:
                            // System.FormatException: The string 'Microsoft.TeamFoundation.WorkItemTracking.Common.ServerDefaultFieldValue' is not a valid AllXsd value.
                            // wiTargetL.ToWorkItem().Fields["System.ModifiedBy"].Value = "Migration";
                            if (SaveAfterEachLink)
                            {
                                SaveOrUndo(wiTargetL, () =>
                                {
                                    var links = wiTargetL.ToWorkItem().Links;
                                    links.Remove(newRl);
                                    foreach (var parent in removedParents)
                                    {
                                        links.Add(new RelatedLink(parent.LinkTypeEnd, parent.RelatedWorkItemId));
                                    }
                                });
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

        /// <summary>
        /// Removes from the target the links that correspond to the given source links.
        /// Used during revision replay (#3192) to remove a link at the revision where it was removed in the source.
        /// Does not save the target. Git links rewritten by the git repository tool are matched by their rewritten URI.
        /// </summary>
        public void RemoveLinks(TfsProcessor processor, WorkItemData sourceWorkItem, IEnumerable<Link> removedSourceLinks, WorkItemData target)
        {
            if (target is null) throw new ArgumentNullException(nameof(target));
            var targetLinks = target.ToWorkItem().Links;
            foreach (Link sourceLink in removedSourceLinks)
            {
                Link match = null;
                try
                {
                    if (sourceLink is Hyperlink hl)
                    {
                        var key = RevisionArtifactDiff.HyperlinkKey(GetAbsoluteUri(hl) ?? hl.Location);
                        match = targetLinks.OfType<Hyperlink>().FirstOrDefault(t => RevisionArtifactDiff.HyperlinkKey(GetAbsoluteUri(t) ?? t.Location) == key);
                    }
                    else if (sourceLink is ExternalLink el)
                    {
                        var key = RevisionArtifactDiff.ExternalLinkKey(el.LinkedArtifactUri);
                        match = targetLinks.OfType<ExternalLink>().FirstOrDefault(t => RevisionArtifactDiff.ExternalLinkKey(t.LinkedArtifactUri) == key);
                        if (match == null)
                        {
                            // Git links are rewritten to the target repository, so look for the rewritten URI.
                            var targetUri = Services.GetRequiredService<TfsGitRepositoryTool>().GetTargetUri(processor, sourceWorkItem, el);
                            if (targetUri != null)
                            {
                                var mappedKey = RevisionArtifactDiff.ExternalLinkKey(targetUri);
                                match = targetLinks.OfType<ExternalLink>().FirstOrDefault(t => RevisionArtifactDiff.ExternalLinkKey(t.LinkedArtifactUri) == mappedKey);
                            }
                        }
                        if (match == null)
                        {
                            // Pull request links are converted to a hyperlink to the source pull request (#3286).
                            var url = Services.GetRequiredService<TfsGitRepositoryTool>().GetPullRequestHyperlinkUrl(processor, el);
                            if (url != null)
                            {
                                var hlKey = RevisionArtifactDiff.HyperlinkKey(url);
                                match = targetLinks.OfType<Hyperlink>().FirstOrDefault(t => RevisionArtifactDiff.HyperlinkKey(t.Location) == hlKey);
                            }
                        }
                    }
                    else if (sourceLink is RelatedLink rl && rl.LinkTypeEnd != null)
                    {
                        var wiSourceR = ((TfsWorkItemMigrationClient)processor.Source.WorkItems).GetWorkItemIfExists(rl.RelatedWorkItemId);
                        if (wiSourceR == null)
                        {
                            Log.LogWarning("  [SKIP] Link of type {LinkType} from source {SourceId} to {RelatedId} not removed from target {TargetId}: work item {RelatedId} does not exist in the source or the migration identity cannot read it (TF26198).", rl.LinkTypeEnd.ImmutableName, sourceWorkItem?.Id, rl.RelatedWorkItemId, target.Id, rl.RelatedWorkItemId);
                            continue;
                        }
                        var wiTargetR = GetRightHandSideTargetWi(processor, wiSourceR, target);
                        if (wiTargetR != null)
                        {
                            match = targetLinks.OfType<RelatedLink>().FirstOrDefault(t =>
                                t.RelatedWorkItemId.ToString() == wiTargetR.Id
                                && t.LinkTypeEnd != null
                                && t.LinkTypeEnd.ImmutableName == rl.LinkTypeEnd.ImmutableName);
                        }
                    }
                }
                catch (MigrationRunStoppedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Log.LogWarning(ex, "  [SKIP] Unable to find the target link to remove for {SourceLinkType} on {TargetId}", sourceLink.GetType().Name, target.Id);
                    continue;
                }
                if (match != null)
                {
                    Log.LogInformation("  [REMOVE] Removing {SourceLinkType} from {TargetId} as it was removed in the source", sourceLink.GetType().Name, target.Id);
                    targetLinks.Remove(match);
                }
                else
                {
                    Log.LogDebug("  [REMOVE-SKIP] {SourceLinkType} removed in the source was not found on {TargetId}", sourceLink.GetType().Name, target.Id);
                }
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

        // A link the server rejects must not stay pending on the target, or every later save of it fails too.
        private static void SaveOrUndo(WorkItemData target, Action undo)
        {
            try
            {
                target.SaveToAzureDevOps();
            }
            catch
            {
                undo();
                throw;
            }
        }

        private const string HierarchyForward ="System.LinkTypes.Hierarchy-Forward";
        private const string HierarchyReverse = "System.LinkTypes.Hierarchy-Reverse";

        // The target child usually comes from the reflected-item cache, so a parent link added since (from the parent's
        // side) is missing until it is synced. An item with unsaved changes is left as it is: those links count too.
        private void RefreshLinks(WorkItemData workItem)
        {
            var wi = workItem.ToWorkItem();
            if (wi.Id == 0 || wi.IsDirty) return;
            try
            {
                wi.SyncToLatest();
            }
            catch (Exception ex)
            {
                Log.LogDebug(ex, "Refreshing the links on {TargetId}", workItem.Id);
            }
        }

        private static List<RelatedLink> ParentLinks(WorkItemData workItem)
        {
            return workItem.ToWorkItem().Links.OfType<RelatedLink>()
                .Where(l => l.LinkTypeEnd != null && l.LinkTypeEnd.ImmutableName == HierarchyReverse)
                .ToList();
        }

        private bool IsRelatedLink(Link item)
        {
            return item is RelatedLink;
        }

        private void CreateHyperlink(Hyperlink sourceLink, WorkItemData target)
        {
            string sourceLinkAbsoluteUri = GetAbsoluteUri(sourceLink);
            if (string.IsNullOrEmpty(sourceLinkAbsoluteUri))
            {
                Log.LogWarning("  [SKIP] Unable to create a hyperlink to [{0}]", sourceLink.Location);
                return;
            }
            if (!IsValidHyperlink(sourceLinkAbsoluteUri))
            {
                Log.LogWarning("  [SKIP] Unable to create a hyperlink to [{0}] <-- protocall no longer allowed or url format is invalid", sourceLink.Location);
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
            if (SaveAfterEachLink)
            {
                SaveOrUndo(target, () => target.ToWorkItem().Links.Remove(hl));
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

        private bool ShouldCopyLinks(TfsProcessor processor, WorkItemData sourceWorkItemLinkStart, WorkItemData targetWorkItemLinkStart)
        {
            if (Options.FilterIfLinkCountMatches)
            {
                // The generated backlink (GenerateMigrationHyperlink) exists only on the target, so it is not counted.
                // Copied hyperlinks that carry the same comment but point elsewhere are counted like any other link.
                var sourceUrl = processor?.Source?.WorkItems?.CreateReflectedWorkItemId(sourceWorkItemLinkStart)?.ToString();
                var targetLinks = targetWorkItemLinkStart.ToWorkItem().Links;
                var targetLinkCount = targetLinks.Count - targetLinks.OfType<Hyperlink>().Count(h => MigrationHyperlinkHelper.IsGeneratedBacklink(h.Location, h.Comment, sourceUrl));
                if (targetLinkCount == sourceWorkItemLinkStart.ToWorkItem().Links.Count) // we should never have this as the target should not have existed in this path
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

        private bool IsValidHyperlink(string sourceLinkAbsoluteUri)
        {
            if (string.IsNullOrEmpty(sourceLinkAbsoluteUri))
            {
                return false;
            }

            try
            {
                var uri = new Uri(sourceLinkAbsoluteUri);
                var allowedSchemes = new[]
                {
                    "http", "https", "ftp", "gopher", "mailto", "news", "telnet", "wais",
                    "vstfs", "tfs", "alm", "mtm", "mtms", "mtr", "mtrs", "mfbclient", "mfbclients",
                    "test-runner", "x-mvwit", "onenote", "codeflow", "file", "tel", "skype"
                };
                return allowedSchemes.Contains(uri.Scheme.ToLowerInvariant());
            }
            catch (UriFormatException)
            {
                return false;
            }
        }
    }
}
