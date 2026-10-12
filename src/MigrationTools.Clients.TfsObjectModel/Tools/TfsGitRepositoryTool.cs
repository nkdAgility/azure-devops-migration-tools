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
                }
                catch (Exception)
                {
                    sourceRepoService = null;
                }

            }
        }


        public  int Enrich(TfsProcessor processor, WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
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
            var changeSetMappings = Services.GetRequiredService<TfsChangeSetMappingTool>();
            List<ExternalLink> newEL = new List<ExternalLink>();
            List<ExternalLink> removeEL = new List<ExternalLink>();
            int count = 0;
            SetupRepoBits();
            if (sourceRepoService == null)
            {
                Log.LogWarning("Unable to configure connection to git!");
                return -1;
            }
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
                    else
                    {
                        TfsGitRepositoryInfo sourceRepoInfo = TfsGitRepositoryInfo.Create(el, sourceRepos, changeSetMappings, sourceWorkItem?.ProjectName);

                        // if sourceRepo is null ignore this link and keep processing further links
                        if (sourceRepoInfo == null)
                        {
                            continue;
                        }

                        // if repo was not found in source project, try to find it by repoId in the whole project collection
                        if (sourceRepoInfo.GitRepo == null)
                        {
                            var anyProjectSourceRepoInfo = TfsGitRepositoryInfo.Create(el, allSourceRepos, changeSetMappings, sourceWorkItem?.ProjectName);
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
                                ReportUnresolved(anyProjectSourceRepoInfo ?? sourceRepoInfo);
                            }
                        }

                        if (sourceRepoInfo.GitRepo != null)
                        {
                            string targetRepoName = GetTargetRepoName(Options.Mappings, sourceRepoInfo);
                            string sourceProjectName = sourceRepoInfo?.GitRepo?.ProjectReference?.Name ?? _processor.Target.Options.Project;
                            string targetProjectName = _processor.Target.Options.Project;

                            TfsGitRepositoryInfo targetRepoInfo = TfsGitRepositoryInfo.Create(targetRepoName, sourceRepoInfo, targetRepos);
                            // if repo was not found in the target project, try to find it in the whole target project collection
                            if (targetRepoInfo.GitRepo == null)
                            {
                                if (Options.Mappings.Values.Contains(targetRepoName))
                                {
                                    var anyTargetRepoInCollectionInfo = TfsGitRepositoryInfo.Create(targetRepoName, sourceRepoInfo, allTargetRepos);
                                    if (anyTargetRepoInCollectionInfo.GitRepo != null)
                                    {
                                        targetRepoInfo = anyTargetRepoInCollectionInfo;
                                    }
                                }
                            }

                            // Fix commit links if target repo has been found
                            if (targetRepoInfo.GitRepo != null)
                            {
                                Log.LogDebug("GitRepositoryEnricher: Fixing {SourceRepoUrl} to {TargetRepoUrl}?", sourceRepoInfo.GitRepo.RemoteUrl, targetRepoInfo.GitRepo.RemoteUrl);

                                // Create External Link object
                                ExternalLink newLink = null;
                                switch (l.ArtifactLinkType.Name)
                                {
                                    case "Branch":
                                        newLink = new ExternalLink(((TfsWorkItemMigrationClient)_processor.Target.WorkItems).Store.RegisteredLinkTypes[ArtifactLinkIds.Branch],
                                            $"vstfs:///git/ref/{targetRepoInfo.GitRepo.ProjectReference.Id}%2f{targetRepoInfo.GitRepo.Id}%2f{sourceRepoInfo.CommitID}");
                                        break;

                                    case "Fixed in Changeset":  // TFVC
                                    case "Fixed in Commit":
                                        newLink = new ExternalLink(((TfsWorkItemMigrationClient)_processor.Target.WorkItems).Store.RegisteredLinkTypes[ArtifactLinkIds.Commit],
                                            $"vstfs:///git/commit/{targetRepoInfo.GitRepo.ProjectReference.Id}%2f{targetRepoInfo.GitRepo.Id}%2f{sourceRepoInfo.CommitID}");
                                        newLink.Comment = el.Comment;
                                        break;

                                    case "Pull Request":
                                        //newLink = new ExternalLink(targetStore.Store.RegisteredLinkTypes[ArtifactLinkIds.PullRequest],
                                        //    $"vstfs:///Git/PullRequestId/{targetRepoInfo.GitRepo.ProjectReference.Id}%2f{targetRepoInfo.GitRepo.Id}%2f{sourceRepoInfo.CommitID}");
                                        removeEL.Add(el);
                                        break;

                                    default:
                                        Log.LogWarning("Skipping unsupported link type {0}", l.ArtifactLinkType.Name);
                                        break;
                                }

                                if (newLink != null)
                                {
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
                            else
                            {
                                ReportUnresolved(sourceRepoInfo, $"target repo \"{targetRepoName}\" not found",
                                    TryGetMapping(Options.Mappings, sourceRepoInfo.GitRepo, out _)
                                        ? $"Create \"{targetRepoName}\" in the target or fix the Mapping"
                                        : null);
                            }
                        }
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

            if (targetWorkItem.ToWorkItem().IsDirty && _save)
            {
                Log.LogDebug("GitRepositoryEnricher: Saving {TargetWorkItemId}", targetWorkItem.Id);
                targetWorkItem.SaveToAzureDevOps();
            }
            return count;
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

        public static string GetTargetRepoName(Dictionary<string, string> gitRepoMappings, TfsGitRepositoryInfo repoInfo)
        {
            return TryGetMapping(gitRepoMappings, repoInfo.GitRepo, out var mapped) ? mapped : repoInfo.GitRepo.Name;
        }

    }
}
