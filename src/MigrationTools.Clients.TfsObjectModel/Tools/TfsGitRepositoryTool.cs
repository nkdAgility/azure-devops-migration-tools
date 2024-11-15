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
        private bool _filter = true;
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
            var changeSetMappings = Services.GetService<TfsChangeSetMappingTool>();
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
                        // if repo is found in a different project and the repo Name is listed in repo mappings, use it
                        if (anyProjectSourceRepoInfo.GitRepo != null && Options.Mappings.ContainsKey(anyProjectSourceRepoInfo.GitRepo.Name))
                        {
                            sourceRepoInfo = anyProjectSourceRepoInfo;
                        }
                        else
                        {
                            Log.LogWarning("GitRepositoryEnricher: Could not find source git repo - repo referenced: {SourceRepoName}/{TargetRepoName}", anyProjectSourceRepoInfo?.GitRepo?.ProjectReference?.Name, anyProjectSourceRepoInfo?.GitRepo?.Name);
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
                            Log.LogWarning("GitRepositoryEnricher: Target Repo not found. Cannot map {SourceRepoUrl} to ???", sourceRepoInfo.GitRepo.RemoteUrl);
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
