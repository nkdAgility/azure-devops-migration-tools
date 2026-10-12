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
                        string targetUri = MapGitLinkToTargetUri(el, sourceWorkItem?.ProjectName, out bool dropLink, out string linkTypeId);
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
            return MapGitLinkToTargetUri(sourceLink, sourceWorkItem?.ProjectName, out _, out _);
        }

        private string MapGitLinkToTargetUri(ExternalLink el, string sourceProjectName, out bool dropLink, out string linkTypeId)
        {
            dropLink = false;
            linkTypeId = null;
            var changeSetMappings = Services.GetRequiredService<TfsChangeSetMappingTool>();

            // During revision replay (#3192) this runs on every revision that adds links, so links already rewritten to
            // a target repository (and not also a source repository) are left alone instead of being re-mapped.
            var targetRepoOfLink = TfsGitRepositoryInfo.Create(el, targetRepos, changeSetMappings, _processor.Target.Options.Project);
            if (targetRepoOfLink?.GitRepo != null && !sourceRepos.Any(r => r.Id == targetRepoOfLink.GitRepo.Id))
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
                // if repo is found in a different project and the repo Name is listed in repo mappings, use it
                if (anyProjectSourceRepoInfo.GitRepo != null && Options.Mappings.ContainsKey(anyProjectSourceRepoInfo.GitRepo.Name))
                {
                    sourceRepoInfo = anyProjectSourceRepoInfo;
                }
                else
                {
                    Log.LogWarning("GitRepositoryEnricher: Could not find source git repo - repo referenced: {SourceRepoName}/{TargetRepoName}", anyProjectSourceRepoInfo?.GitRepo?.ProjectReference?.Name, anyProjectSourceRepoInfo?.GitRepo?.Name);
                    return null;
                }
            }

            string targetRepoName = GetTargetRepoName(Options.Mappings, sourceRepoInfo);
            TfsGitRepositoryInfo targetRepoInfo = TfsGitRepositoryInfo.Create(targetRepoName, sourceRepoInfo, targetRepos);
            // if repo was not found in the target project, try to find it in the whole target project collection
            if (targetRepoInfo.GitRepo == null && Options.Mappings.Values.Contains(targetRepoName))
            {
                var anyTargetRepoInCollectionInfo = TfsGitRepositoryInfo.Create(targetRepoName, sourceRepoInfo, allTargetRepos);
                if (anyTargetRepoInCollectionInfo.GitRepo != null)
                {
                    targetRepoInfo = anyTargetRepoInCollectionInfo;
                }
            }

            if (targetRepoInfo.GitRepo == null)
            {
                Log.LogWarning("GitRepositoryEnricher: Target Repo not found. Cannot map {SourceRepoUrl} to ???", sourceRepoInfo.GitRepo.RemoteUrl);
                return null;
            }

            Log.LogDebug("GitRepositoryEnricher: Fixing {SourceRepoUrl} to {TargetRepoUrl}?", sourceRepoInfo.GitRepo.RemoteUrl, targetRepoInfo.GitRepo.RemoteUrl);
            switch (el.ArtifactLinkType.Name)
            {
                case "Branch":
                    linkTypeId = ArtifactLinkIds.Branch;
                    return $"vstfs:///git/ref/{targetRepoInfo.GitRepo.ProjectReference.Id}%2f{targetRepoInfo.GitRepo.Id}%2f{sourceRepoInfo.CommitID}";

                case "Fixed in Changeset":  // TFVC
                case "Fixed in Commit":
                    linkTypeId = ArtifactLinkIds.Commit;
                    return $"vstfs:///git/commit/{targetRepoInfo.GitRepo.ProjectReference.Id}%2f{targetRepoInfo.GitRepo.Id}%2f{sourceRepoInfo.CommitID}";

                case "Pull Request":
                    dropLink = true;
                    return null;

                default:
                    Log.LogWarning("Skipping unsupported link type {0}", el.ArtifactLinkType.Name);
                    return null;
            }
        }

        private string GetTargetRepoName(Dictionary<string, string> gitRepoMappings, TfsGitRepositoryInfo repoInfo)
        {
            if (gitRepoMappings.ContainsKey(repoInfo.GitRepo.Name))
            {
                return gitRepoMappings[repoInfo.GitRepo.Name];
            }
            else
            {
                return repoInfo.GitRepo.Name;
            }
        }

    }
}
