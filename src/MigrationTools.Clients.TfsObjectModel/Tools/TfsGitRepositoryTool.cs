using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Git.Client;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Clients;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Services;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    public class TfsGitRepositoryTool : Tool<TfsGitRepositoryToolOptions>
    {

        private bool _save = true;
        private GitRepositoryService sourceRepoService;
        private IList<GitRepository> sourceRepos;
        private IList<GitRepository> allSourceRepos;
        private GitRepositoryService targetRepoService;
        private IList<GitRepository> targetRepos;
        private IList<GitRepository> allTargetRepos;
        private List<string> gitWits;
        private TfsProcessor _processor;

        public TfsGitRepositoryTool(IOptions<TfsGitRepositoryToolOptions> options, IServiceProvider services, ILogger<TfsGitRepositoryTool> logger, ITelemetryLogger telemetryLogger) : base(options, services, logger, telemetryLogger)
        {
            gitWits = new List<string>
                {
                    "Branch",
                    "Fixed in Commit",
                    "Pull Request",
                    "Fixed in Changeset"    //TFVC
                };
        }

        private void SetupRepoBits()
        {
            if (sourceRepoService == null)
            {
                try
                {
                    sourceRepoService = _processor.Source.GetService<GitRepositoryService>();
                    sourceRepos = sourceRepoService.QueryRepositories(_processor.Source.Options.Project);
                    allSourceRepos = sourceRepoService.QueryRepositories("");
                    //////////////////////////////////////////////////
                    targetRepoService = _processor.Target.GetService<GitRepositoryService>();
                    targetRepos = targetRepoService.QueryRepositories(_processor.Target.Options.Project);
                    allTargetRepos = targetRepoService.QueryRepositories("");
                    foreach (var kv in FindAmbiguousNameMappings(Options.Mappings, allSourceRepos))
                    {
                        Log.LogWarning("GitRepositoryEnricher: Mapping key \"{Key}\" matches repositories in more than one source project ({Repos}); it applies to all of them. Use \"Project/Repo\" keys to map them separately.",
                            kv.Key, string.Join(", ", kv.Value));
                    }
                }
                catch (Exception)
                {
                    sourceRepoService = null;
                }

            }
        }


        /// <summary>Rewrites the target's Git links (commit, branch, changeset) to the mapped target repositories.</summary>
        /// <param name="processor">The processor running the migration.</param>
        /// <param name="sourceWorkItem">The source work item, used for its project name.</param>
        /// <param name="targetWorkItem">The target work item whose links are rewritten.</param>
        /// <param name="save">False during revision replay (#3192): the rewritten links are saved with the revision being replayed.</param>
        public  int Enrich(TfsProcessor processor, WorkItemData sourceWorkItem, WorkItemData targetWorkItem, bool save = true)
        {
            _processor = processor;
            if (sourceWorkItem is null)
            {
                throw new ArgumentNullException(nameof(sourceWorkItem));
            }

            if (targetWorkItem is null)
            {
                throw new ArgumentNullException(nameof(targetWorkItem));
            }
            if (!Options.Enabled)
            {
                Log.LogWarning("TfsGitRepositoryEnricher is not enabled! We will not fix any git commit links in Work items and they will be ignored.");
                return 0;
            }

            Log.LogInformation("GitRepositoryEnricher: Enriching {Id} To fix Git Repo Links", targetWorkItem.Id);
            List<ExternalLink> newEL = new List<ExternalLink>();
            List<ExternalLink> removeEL = new List<ExternalLink>();
            List<Hyperlink> newHL = new List<Hyperlink>();
            int count = 0;
            SetupRepoBits();
            if (sourceRepoService == null)
            {
                Log.LogWarning("Unable to configure connection to git!");
                Services.GetService<WorkItemReviewLog>()?.ClearChecked(WorkItemReviewCategory.LinkLeftOnSource);
                return -1;
            }
            Services.GetService<WorkItemReviewLog>()?.MarkChecked(WorkItemReviewCategory.LinkLeftOnSource);
            foreach (Link l in targetWorkItem.ToWorkItem().Links)
            {
                if (l is ExternalLink && gitWits.Contains(l.ArtifactLinkType.Name))
                {
                    ExternalLink el = (ExternalLink)l;
                    if (Options.ShouldDropChangedSetLinks)
                    {
                        Log.LogDebug("Dropping changeset {Id}", el.LinkedArtifactUri);
                        removeEL.Add(el);
                    }
                    else if (el.ArtifactLinkType.Name == "Pull Request")
                    {
                        if (IsTargetRepositoryLink(el.LinkedArtifactUri, allSourceRepos, allTargetRepos))
                        {
                            continue;
                        }
                        var action = DecidePullRequestLink(el.LinkedArtifactUri, sourceWorkItem.Id, _processor.Source.Options.Collection, _processor.Target.Options.Collection, allSourceRepos, out string webUrl, out string comment);
                        if (action == PullRequestLinkAction.Drop)
                        {
                            removeEL.Add(el);
                        }
                        else if (action == PullRequestLinkAction.ConvertToHyperlink)
                        {
                            bool exists = targetWorkItem.ToWorkItem().Links.OfType<Hyperlink>().Any(h => string.Equals(h.Location, webUrl, StringComparison.OrdinalIgnoreCase))
                                || newHL.Any(h => string.Equals(h.Location, webUrl, StringComparison.OrdinalIgnoreCase));
                            if (!exists)
                            {
                                newHL.Add(new Hyperlink(webUrl) { Comment = comment });
                            }
                            removeEL.Add(el);
                        }
                    }
                    else
                    {
                        string targetUri = MapGitLinkToTargetUri(el, sourceWorkItem?.ProjectName, out bool dropLink, out string linkTypeId, sourceWorkItemId: sourceWorkItem.Id);
                        if (dropLink)
                        {
                            removeEL.Add(el);
                            continue;
                        }
                        if (targetUri == null)
                        {
                            continue;
                        }
                        ExternalLink newLink = new ExternalLink(((TfsWorkItemMigrationClient)_processor.Target.WorkItems).Store.RegisteredLinkTypes[linkTypeId], targetUri);
                        if (linkTypeId == ArtifactLinkIds.Commit)
                        {
                            newLink.Comment = el.Comment;
                        }
                        var elinks = from Link lq in targetWorkItem.ToWorkItem().Links
                                     where gitWits.Contains(lq.ArtifactLinkType.Name)
                                     select (ExternalLink)lq;
                        var found =
                        (from Link lq in elinks
                         where ((ExternalLink)lq).LinkedArtifactUri.ToLower() == newLink.LinkedArtifactUri.ToLower()
                         select lq).SingleOrDefault();
                        if (found == null)
                        {
                            newEL.Add(newLink);
                        }
                        removeEL.Add(el);
                    }
                }
            }
            // add and remove
            foreach (ExternalLink eln in newEL)
            {
                try
                {
                    Log.LogDebug("GitRepositoryEnricher: Adding {LinkedArtifactUri} ", eln.LinkedArtifactUri);
                    targetWorkItem.ToWorkItem().Links.Add(eln);
                }
                catch (Exception)
                {
                    // eat exception as sometimes TFS thinks this is an attachment
                }
            }
            foreach (Hyperlink hl in newHL)
            {
                Log.LogDebug("GitRepositoryEnricher: Adding hyperlink {Location}", hl.Location);
                targetWorkItem.ToWorkItem().Links.Add(hl);
            }
            foreach (ExternalLink elr in removeEL)
            {
                if (targetWorkItem.ToWorkItem().Links.Contains(elr))
                {
                    try
                    {
                        Log.LogDebug("GitRepositoryEnricher: Removing {LinkedArtifactUri} ", elr.LinkedArtifactUri);
                        targetWorkItem.ToWorkItem().Links.Remove(elr);
                        count++;
                    }
                    catch (Exception)
                    {
                        // eat exception as sometimes TFS thinks this is an attachment
                    }
                }
            }

            if (targetWorkItem.ToWorkItem().IsDirty && _save && save)
            {
                Log.LogDebug("GitRepositoryEnricher: Saving {TargetWorkItemId}", targetWorkItem.Id);
                targetWorkItem.SaveToAzureDevOps();
            }
            return count;
        }

        /// <summary>
        /// Returns the URI that <see cref="Enrich"/> writes on the target for a source Git link, or null when the link
        /// is not rewritten (tool disabled, links dropped, repo not mapped). Used during revision replay (#3192) to find
        /// the target link to remove when the source removes a Git link.
        /// </summary>
        public string GetTargetUri(TfsProcessor processor, WorkItemData sourceWorkItem, ExternalLink sourceLink)
        {
            if (!Options.Enabled || Options.ShouldDropChangedSetLinks || sourceLink?.ArtifactLinkType == null || !gitWits.Contains(sourceLink.ArtifactLinkType.Name))
            {
                return null;
            }
            _processor = processor;
            SetupRepoBits();
            if (sourceRepoService == null)
            {
                return null;
            }
            return MapGitLinkToTargetUri(sourceLink, sourceWorkItem?.ProjectName, out _, out _, report: false);
        }

        /// <summary>
        /// True when <see cref="Enrich"/> would leave the source link in place because the target repository does not
        /// have the commit or branch that <paramref name="targetUri"/> (from <see cref="GetTargetUri"/>) names. A re-run
        /// then counts the raw source link on the target as present, instead of repairing it on every run (#3326).
        /// Reports nothing: Enrich reports a missing artifact when it runs.
        /// </summary>
        public bool TargetLacksArtifact(string targetUri)
        {
            if (!Options.VerifyTargetExists || !TryParseTargetUri(targetUri, out bool isBranch, out Guid repoId, out string commitOrRefId))
            {
                return false;
            }
            return isBranch ? !TargetArtifacts.BranchExists(repoId, commitOrRefId) : !TargetArtifacts.CommitExists(repoId, commitOrRefId);
        }

        /// <summary>Reads the repository and commit or ref from a link written by <see cref="MapGitLinkToTargetUri"/>.</summary>
        public static bool TryParseTargetUri(string targetUri, out bool isBranch, out Guid repoId, out string commitOrRefId)
        {
            isBranch = false;
            repoId = Guid.Empty;
            commitOrRefId = null;
            const string commitPrefix = "vstfs:///git/commit/";
            const string branchPrefix = "vstfs:///git/ref/";
            string rest;
            if (targetUri != null && targetUri.StartsWith(commitPrefix, StringComparison.OrdinalIgnoreCase))
            {
                rest = targetUri.Substring(commitPrefix.Length);
            }
            else if (targetUri != null && targetUri.StartsWith(branchPrefix, StringComparison.OrdinalIgnoreCase))
            {
                isBranch = true;
                rest = targetUri.Substring(branchPrefix.Length);
            }
            else
            {
                return false;
            }
            var parts = rest.Split(new[] { "%2f", "%2F" }, 3, StringSplitOptions.None);
            if (parts.Length != 3 || !Guid.TryParse(parts[1], out repoId) || parts[2].Length == 0)
            {
                return false;
            }
            commitOrRefId = parts[2];
            return true;
        }

        private TfsGitTargetArtifactCache _targetArtifacts;

        /// <summary>Replaces the target existence lookups; for tests.</summary>
        public TfsGitTargetArtifactCache TargetArtifacts
        {
            get => _targetArtifacts ??= TfsGitTargetArtifactCache.ForClient(_processor.Target.GetClient<GitHttpClient>());
            set => _targetArtifacts = value;
        }

        private readonly Dictionary<string, int> _linksLeftOnSource = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _reportedLeftOnSource = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Target repositories missing linked commits or branches, with the number of links left pointing at the source.</summary>
        public IReadOnlyDictionary<string, int> LinksLeftOnSource => _linksLeftOnSource;

        /// <summary>
        /// Checks the target repo has the linked commit or branch. When it does not, logs a warning and records it: the
        /// caller then leaves the source link in place rather than creating a link to something that does not exist.
        /// </summary>
        public bool TargetHasArtifact(bool isBranch, GitRepository targetRepo, string commitOrRefId, string sourceLinkUri, string sourceWorkItemId)
        {
            bool exists = isBranch
                ? TargetArtifacts.BranchExists(targetRepo.Id, commitOrRefId)
                : TargetArtifacts.CommitExists(targetRepo.Id, commitOrRefId);
            // Revision replay enriches every revision, so each missing link is reported once per work item.
            if (!exists && _reportedLeftOnSource.Add($"{sourceWorkItemId}|{sourceLinkUri}"))
            {
                string repoName = $"{targetRepo.ProjectReference?.Name}/{targetRepo.Name}";
                string artifact = isBranch ? $"branch {TfsGitTargetArtifactCache.BranchNameFromRefId(commitOrRefId) ?? commitOrRefId}" : $"commit {commitOrRefId}";
                Log.LogWarning("GitRepositoryEnricher: Source work item {SourceWorkItemId}: {Artifact} not found in target repo {TargetRepo}; leaving link {SourceLink} pointing at the source.",
                    sourceWorkItemId, artifact, repoName, sourceLinkUri);
                Services.GetService<WorkItemReviewLog>()?.Record(WorkItemReviewCategory.LinkLeftOnSource,
                    $"{artifact} not found in target repo {repoName}; the link {sourceLinkUri} still points at the source.",
                    "Push the commit or branch to the target repo, then re-run to rewrite the link; or fix the link by hand.",
                    sourceId: sourceWorkItemId?.ToString());
                _linksLeftOnSource.TryGetValue(repoName, out int seen);
                _linksLeftOnSource[repoName] = seen + 1;
            }
            return exists;
        }

        private readonly HashSet<string> _reportedPullRequests = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<int, string> _pullRequestTitles = new Dictionary<int, string>();

        /// <summary>Pull request links kept as artifact links because the target is the source organisation.</summary>
        public int PullRequestLinksKept { get; private set; }

        /// <summary>Pull request links replaced by a hyperlink to the source pull request.</summary>
        public int PullRequestLinksConverted { get; private set; }

        /// <summary>Pull request links that could not be read and were left unchanged.</summary>
        public int PullRequestLinksUnreadable { get; private set; }

        /// <summary>Reads a source pull request's title by id; replaceable for tests.</summary>
        public Func<int, string> PullRequestTitleReader { get; set; }

        /// <summary>
        /// Decides what happens to a pull request link (#3286). A vstfs pull request link resolves only in the organisation
        /// that holds the repository, so on any other target it becomes a hyperlink to the source pull request's web page.
        /// Each link is logged and counted once per work item, as revision replay enriches every revision.
        /// </summary>
        public PullRequestLinkAction DecidePullRequestLink(string sourceLinkUri, string sourceWorkItemId, Uri sourceCollection, Uri targetCollection,
            IList<GitRepository> sourceRepositories, out string webUrl, out string comment)
        {
            webUrl = null;
            comment = null;
            if (Options.DropPullRequestLinks)
            {
                Log.LogDebug("GitRepositoryEnricher: Source work item {SourceWorkItemId}: dropping pull request link {SourceLink}", sourceWorkItemId, sourceLinkUri);
                return PullRequestLinkAction.Drop;
            }
            bool firstTime = _reportedPullRequests.Add($"{sourceWorkItemId}|{sourceLinkUri}");
            if (TfsPullRequestLink.SameCollection(sourceCollection, targetCollection))
            {
                if (firstTime)
                {
                    PullRequestLinksKept++;
                }
                return PullRequestLinkAction.Keep;
            }
            if (!TfsPullRequestLink.TryParse(sourceLinkUri, out var link))
            {
                if (firstTime)
                {
                    Log.LogWarning("GitRepositoryEnricher: Source work item {SourceWorkItemId}: pull request link {SourceLink} is not in the expected vstfs:///Git/PullRequestId/project/repository/id form, so it was left unchanged and will not open on the target. Fix or remove the link in the source and run again, or set DropPullRequestLinks to remove pull request links.",
                        sourceWorkItemId, sourceLinkUri);
                    PullRequestLinksUnreadable++;
                }
                return PullRequestLinkAction.LeaveUnchanged;
            }
            webUrl = PullRequestWebUrl(link, sourceCollection, sourceRepositories);
            comment = TfsPullRequestLink.Comment(link.PullRequestId, ReadPullRequestTitle(link.PullRequestId));
            if (firstTime)
            {
                Log.LogInformation("GitRepositoryEnricher: Source work item {SourceWorkItemId}: pull request link {SourceLink} replaced by a hyperlink to {WebUrl}, as the target organisation cannot open the source pull request.",
                    sourceWorkItemId, sourceLinkUri, webUrl);
                PullRequestLinksConverted++;
            }
            return PullRequestLinkAction.ConvertToHyperlink;
        }

        private static GitRepository FindRepository(TfsPullRequestLink link, IList<GitRepository> repositories)
        {
            return repositories?.FirstOrDefault(r => string.Equals(r.Id.ToString(), link.Repository, StringComparison.OrdinalIgnoreCase)
                                                    || string.Equals(r.Name, link.Repository, StringComparison.OrdinalIgnoreCase)
                                                       && (string.Equals(r.ProjectReference?.Name, link.Project, StringComparison.OrdinalIgnoreCase) || string.Equals(r.ProjectReference?.Id.ToString(), link.Project, StringComparison.OrdinalIgnoreCase)));
        }

        private static string PullRequestWebUrl(TfsPullRequestLink link, Uri sourceCollection, IList<GitRepository> sourceRepositories)
        {
            var repo = FindRepository(link, sourceRepositories);
            // Azure DevOps also accepts project and repository ids in the web address, so a link to a deleted or unreadable repository still gets a hyperlink.
            return TfsPullRequestLink.WebUrl(sourceCollection, repo?.ProjectReference?.Name ?? link.Project, repo?.Name ?? link.Repository, link.PullRequestId);
        }

        /// <summary>
        /// True when a pull request link on the target belongs to a target repository (added on the target after an earlier run),
        /// so it already opens there and must not be converted.
        /// </summary>
        public static bool IsTargetRepositoryLink(string artifactUri, IList<GitRepository> sourceRepositories, IList<GitRepository> targetRepositories)
        {
            if (!TfsPullRequestLink.TryParse(artifactUri, out var link))
            {
                return false;
            }
            return FindRepository(link, targetRepositories) != null && FindRepository(link, sourceRepositories) == null;
        }

        /// <summary>
        /// The hyperlink address that <see cref="Enrich"/> writes on the target for a source pull request link, or null when the
        /// link is not converted. Used during revision replay to find the hyperlink to remove when the source removes the link.
        /// </summary>
        public string GetPullRequestHyperlinkUrl(TfsProcessor processor, ExternalLink sourceLink)
        {
            if (!Options.Enabled || Options.ShouldDropChangedSetLinks || Options.DropPullRequestLinks || sourceLink?.ArtifactLinkType?.Name != "Pull Request")
            {
                return null;
            }
            _processor = processor;
            SetupRepoBits();
            if (sourceRepoService == null
                || TfsPullRequestLink.SameCollection(processor.Source.Options.Collection, processor.Target.Options.Collection)
                || !TfsPullRequestLink.TryParse(sourceLink.LinkedArtifactUri, out var link))
            {
                return null;
            }
            return PullRequestWebUrl(link, processor.Source.Options.Collection, allSourceRepos);
        }

        private string ReadPullRequestTitle(int pullRequestId)
        {
            if (_pullRequestTitles.TryGetValue(pullRequestId, out string title))
            {
                return title;
            }
            try
            {
                var reader = PullRequestTitleReader ?? (id => _processor.Source.GetClient<GitHttpClient>().GetPullRequestByIdAsync(id).GetAwaiter().GetResult().Title);
                title = reader(pullRequestId);
            }
            catch (Exception ex)
            {
                Log.LogWarning("GitRepositoryEnricher: Could not read the title of source pull request {PullRequestId} ({Reason}); its hyperlink is named by number only. Give the source identity read access to the repository to include the title.",
                    pullRequestId, (ex.InnerException ?? ex).Message);
                Log.LogDebug(ex, "GitRepositoryEnricher: reading source pull request {PullRequestId} failed", pullRequestId);
                title = null;
            }
            _pullRequestTitles[pullRequestId] = title;
            return title;
        }

        private string MapGitLinkToTargetUri(ExternalLink el, string sourceProjectName, out bool dropLink, out string linkTypeId, bool report = true, string sourceWorkItemId = null)
        {
            bool verifyTarget = report && Options.VerifyTargetExists;
            dropLink = false;
            linkTypeId = null;
            // Pull request ids are not preserved by a git push, so there is no target artifact to rewrite to; Enrich handles them (#3286).
            if (el.ArtifactLinkType.Name == "Pull Request")
            {
                return null;
            }
            var changeSetMappings = Services.GetRequiredService<TfsChangeSetMappingTool>();

            // During revision replay (#3192) this runs on every revision that adds links, so links already rewritten to
            // a target repository (and not also a source repository) are left alone instead of being re-mapped.
            // TFVC changeset links resolve to a placeholder repo with no Id (#3226), so they are never treated as already rewritten.
            var targetRepoOfLink = TfsGitRepositoryInfo.Create(el, targetRepos, changeSetMappings, _processor.Target.Options.Project);
            if (targetRepoOfLink?.GitRepo != null && targetRepoOfLink.GitRepo.Id != Guid.Empty && !sourceRepos.Any(r => r.Id == targetRepoOfLink.GitRepo.Id))
            {
                return null;
            }

            TfsGitRepositoryInfo sourceRepoInfo = TfsGitRepositoryInfo.Create(el, sourceRepos, changeSetMappings, sourceProjectName);

            // if sourceRepo is null ignore this link and keep processing further links
            if (sourceRepoInfo == null)
            {
                return null;
            }

            // if repo was not found in source project, try to find it by repoId in the whole project collection
            if (sourceRepoInfo.GitRepo == null)
            {
                var anyProjectSourceRepoInfo = TfsGitRepositoryInfo.Create(el, allSourceRepos, changeSetMappings, sourceProjectName);
                // Legacy links carry the repo GUID, so a hit there is exact even when the repo lives in (or was referenced
                // under) another / former project name. Newer "Project/Repo/id" links match on project and repo name, falling
                // back to a repo name that is unique in the collection (needed when the project was renamed). Accept the repo
                // when it is mapped (by "Repo" or "Project/Repo") or when the target project has a repo of the same name.
                if (ShouldUseCrossProjectRepo(anyProjectSourceRepoInfo?.GitRepo, Options.Mappings, targetRepos))
                {
                    sourceRepoInfo = anyProjectSourceRepoInfo;
                }
                else
                {
                    if (report)
                    {
                        ReportUnresolved(anyProjectSourceRepoInfo ?? sourceRepoInfo);
                    }
                    return null;
                }
            }

            string targetRepoName = GetTargetRepoName(Options.Mappings, sourceRepoInfo);
            TfsGitRepositoryInfo targetRepoInfo = TfsGitRepositoryInfo.Create(targetRepoName, sourceRepoInfo, targetRepos);
            // if repo was not found in the target project, try to find it in the whole target project collection
            if (targetRepoInfo.GitRepo == null && Options.Mappings.Values.Contains(targetRepoName, StringComparer.OrdinalIgnoreCase))
            {
                var anyTargetRepoInCollectionInfo = TfsGitRepositoryInfo.Create(targetRepoName, sourceRepoInfo, allTargetRepos);
                if (anyTargetRepoInCollectionInfo.GitRepo != null)
                {
                    targetRepoInfo = anyTargetRepoInCollectionInfo;
                }
            }

            if (targetRepoInfo.GitRepo == null)
            {
                if (report)
                {
                    ReportUnresolved(sourceRepoInfo, $"target repo \"{targetRepoName}\" not found",
                        TryGetMapping(Options.Mappings, sourceRepoInfo.GitRepo, out _)
                            ? $"Create \"{targetRepoName}\" in the target or fix the Mapping"
                            : null);
                }
                return null;
            }

            Log.LogDebug("GitRepositoryEnricher: Fixing {SourceRepoUrl} to {TargetRepoUrl}?", sourceRepoInfo.GitRepo.RemoteUrl, targetRepoInfo.GitRepo.RemoteUrl);
            switch (el.ArtifactLinkType.Name)
            {
                case "Branch":
                    if (verifyTarget && !TargetHasArtifact(isBranch: true, targetRepoInfo.GitRepo, sourceRepoInfo.CommitID, el.LinkedArtifactUri, sourceWorkItemId))
                    {
                        return null;
                    }
                    linkTypeId = ArtifactLinkIds.Branch;
                    return $"vstfs:///git/ref/{targetRepoInfo.GitRepo.ProjectReference.Id}%2f{targetRepoInfo.GitRepo.Id}%2f{sourceRepoInfo.CommitID}";

                case "Fixed in Changeset":  // TFVC
                case "Fixed in Commit":
                    if (verifyTarget && !TargetHasArtifact(isBranch: false, targetRepoInfo.GitRepo, sourceRepoInfo.CommitID, el.LinkedArtifactUri, sourceWorkItemId))
                    {
                        return null;
                    }
                    linkTypeId = ArtifactLinkIds.Commit;
                    return $"vstfs:///git/commit/{targetRepoInfo.GitRepo.ProjectReference.Id}%2f{targetRepoInfo.GitRepo.Id}%2f{sourceRepoInfo.CommitID}";

                default:
                    Log.LogWarning("Skipping unsupported link type {0}", el.ArtifactLinkType.Name);
                    return null;
            }
        }

        private readonly Dictionary<string, int> _unresolvedRepos = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Repositories that could not be resolved, with the number of links referencing each.</summary>
        public IReadOnlyDictionary<string, int> UnresolvedRepos => _unresolvedRepos;

        private void ReportUnresolved(TfsGitRepositoryInfo info, string reason = "source repo not resolved", string advice = null)
        {
            advice ??= "Add a Mapping for the repo name (or \"Project/Repo\")";
            string key = info?.GitRepo != null
                ? $"{info.GitRepo.ProjectReference?.Name}/{info.GitRepo.Name}"
                : $"unknown repo id {info?.RepoID ?? "(none)"}";
            key = $"{key} ({reason})";
            _unresolvedRepos.TryGetValue(key, out int seen);
            _unresolvedRepos[key] = seen + 1;
            Services.GetService<WorkItemReviewLog>()?.Record(WorkItemReviewCategory.LinkLeftOnSource,
                $"Could not rewrite a git link - {key}; the link still points at the source.",
                $"{advice} to rewrite the link, then re-run.");
            if (seen == 0)
            {
                Log.LogWarning("GitRepositoryEnricher: Could not rewrite git links - {RepoReference}. {Advice} to rewrite these links. Further links to this repo are logged at Debug.", key, advice);
            }
            else
            {
                Log.LogDebug("GitRepositoryEnricher: Could not rewrite git links - {RepoReference} ({Count} links so far)", key, seen + 1);
            }
        }

        /// <summary>Logs one line per unresolved repository with its link count. Call once at the end of a run.</summary>
        public void LogUnresolvedSummary()
        {
            if (PullRequestLinksConverted > 0)
            {
                Log.LogInformation("GitRepositoryEnricher: {LinkCount} pull request link(s) replaced by hyperlinks to the source pull request; set DropPullRequestLinks to remove them instead.", PullRequestLinksConverted);
            }
            if (PullRequestLinksKept > 0)
            {
                Log.LogInformation("GitRepositoryEnricher: {LinkCount} pull request link(s) kept, as the target is the source organisation.", PullRequestLinksKept);
            }
            if (PullRequestLinksUnreadable > 0)
            {
                Log.LogWarning("GitRepositoryEnricher: {LinkCount} pull request link(s) could not be read and were left unchanged; see the warnings above.", PullRequestLinksUnreadable);
            }
            if (_linksLeftOnSource.Count > 0)
            {
                Log.LogWarning("GitRepositoryEnricher: {RepoCount} target repo(s) are missing linked commits or branches; those links were left pointing at the source:", _linksLeftOnSource.Count);
                foreach (var kv in _linksLeftOnSource.OrderByDescending(kv => kv.Value))
                {
                    Log.LogWarning("  {TargetRepo}: {LinkCount} link(s)", kv.Key, kv.Value);
                }
            }
            if (_unresolvedRepos.Count == 0)
            {
                return;
            }
            Log.LogWarning("GitRepositoryEnricher: {RepoCount} git repo reference(s) could not be resolved; their links were left unchanged:", _unresolvedRepos.Count);
            foreach (var kv in _unresolvedRepos.OrderByDescending(kv => kv.Value))
            {
                Log.LogWarning("  {RepoReference}: {LinkCount} link(s)", kv.Key, kv.Value);
            }
        }

        /// <summary>
        /// Looks up a mapping for a repo by "Project/Repo" first, then by repo name alone.
        /// </summary>
        public static bool TryGetMapping(Dictionary<string, string> gitRepoMappings, GitRepository repo, out string targetName)
        {
            targetName = null;
            if (gitRepoMappings == null || repo?.Name == null)
            {
                return false;
            }
            var projectName = repo.ProjectReference?.Name;
            if (!string.IsNullOrEmpty(projectName))
            {
                var qualified = $"{projectName}/{repo.Name}";
                var hit = gitRepoMappings.FirstOrDefault(kv => string.Equals(kv.Key, qualified, StringComparison.OrdinalIgnoreCase));
                if (hit.Key != null)
                {
                    targetName = hit.Value;
                    return true;
                }
            }
            if (gitRepoMappings.TryGetValue(repo.Name, out targetName))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Decides whether a repo found by GUID outside the source project should be used to rewrite a link.
        /// </summary>
        public static bool ShouldUseCrossProjectRepo(GitRepository repo, Dictionary<string, string> gitRepoMappings, IList<GitRepository> targetProjectRepos)
        {
            if (repo == null)
            {
                return false;
            }
            if (TryGetMapping(gitRepoMappings, repo, out _))
            {
                return true;
            }
            return targetProjectRepos != null && targetProjectRepos.Any(r => string.Equals(r.Name, repo.Name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Finds bare "Repo" mapping keys that match repositories in more than one source project, with the matching
        /// "Project/Repo" names. Repositories that also have a "Project/Repo" key are excluded, as that key wins for them.
        /// </summary>
        public static Dictionary<string, List<string>> FindAmbiguousNameMappings(Dictionary<string, string> gitRepoMappings, IList<GitRepository> sourceRepos)
        {
            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            if (gitRepoMappings == null || sourceRepos == null)
            {
                return result;
            }
            foreach (var key in gitRepoMappings.Keys.Where(k => !k.Contains('/')))
            {
                var matches = sourceRepos
                    .Where(r => string.Equals(r.Name, key, StringComparison.OrdinalIgnoreCase))
                    .Select(r => $"{r.ProjectReference?.Name}/{r.Name}")
                    .Where(q => !gitRepoMappings.Keys.Any(k => string.Equals(k, q, StringComparison.OrdinalIgnoreCase)))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(q => q, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                if (matches.Count > 1)
                {
                    result[key] = matches;
                }
            }
            return result;
        }

        public static string GetTargetRepoName(Dictionary<string, string> gitRepoMappings, TfsGitRepositoryInfo repoInfo)
        {
            return TryGetMapping(gitRepoMappings, repoInfo.GitRepo, out var mapped) ? mapped : repoInfo.GitRepo.Name;
        }

    }
}
