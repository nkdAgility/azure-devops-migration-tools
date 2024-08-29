﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Serilog;

namespace MigrationTools.Tools
{
    public class TfsGitRepositoryInfo
    {
        public string CommitID { get; }
        public string RepoID { get; }
        public GitRepository GitRepo { get; }

        public TfsGitRepositoryInfo(string CommitID, string RepoID, GitRepository GitRepo)
        {
            this.CommitID = CommitID;
            this.RepoID = RepoID;
            this.GitRepo = GitRepo;
        }

        public static TfsGitRepositoryInfo Create(ExternalLink gitExternalLink, IList<GitRepository> possibleRepos, TfsChangeSetMappingTool tfsChangeSetMappingTool, string workItemSourceProjectName)
        {
            var repoType = DetermineFromLink(gitExternalLink.LinkedArtifactUri);
            switch (repoType)
            {
                case RepistoryType.Git:
                    return CreateFromGit(gitExternalLink, possibleRepos);

                case RepistoryType.TFVC:
                    return CreateFromTFVC(gitExternalLink, possibleRepos, tfsChangeSetMappingTool.Items, workItemSourceProjectName);
            }

            return null;
        }

        private static TfsGitRepositoryInfo CreateFromTFVC(ExternalLink gitExternalLink, IList<GitRepository> possibleRepos, ReadOnlyDictionary<int, string> changesetMapping, string sourceProjectName)
        {
            //vstfs:///VersionControl/Changeset/{id}
            var changeSetIdPart = gitExternalLink.LinkedArtifactUri.Substring(gitExternalLink.LinkedArtifactUri.LastIndexOf('/') + 1);
            if (!int.TryParse(changeSetIdPart, out int changeSetId))
            {
                return null;
            }

            var commitIDKvPair = changesetMapping.FirstOrDefault(item => item.Key == changeSetId);
            if (string.IsNullOrEmpty(commitIDKvPair.Value))
            {
                Log.Debug("GitRepositoryInfo: Commit Id not found from Changeset Id {changeSetIdPart}.");
                return null;
            }

            //assume the GitRepository source name is the work items project name, which changeset links needs to be fixed
            return new TfsGitRepositoryInfo(commitIDKvPair.Value, null, new GitRepository() { Name = sourceProjectName });
        }

        private enum RepistoryType
        {
            Unknown,
            TFVC,
            Git
        }

        private static RepistoryType DetermineFromLink(string link)
        {
            if (string.IsNullOrEmpty(link))
                throw new ArgumentNullException("link");

            //vstfs:///Git/Commit/25f94570-e3e7-4b79-ad19-4b434787fd5a%2f50477259-3058-4dff-ba4c-e8c179ec5327%2f41dd2754058348d72a6417c0615c2543b9b55535
            //vstfs:///Git/PullRequestId/
            //ToDo: check only for "git" ?
            if (link.ToLowerInvariant().Contains("git/commit")
                || link.ToLowerInvariant().Contains("git/pullrequestid")
                || link.ToLowerInvariant().Contains("git/ref"))
                return RepistoryType.Git;

            //vstfs:///VersionControl/Changeset/{id}
            if (link.ToLowerInvariant().Contains("versioncontrol/changeset"))
                return RepistoryType.TFVC;

            Log.Debug("GitRepositoryInfo: Cannot determine repository type from external link: {link}");

            return RepistoryType.Unknown;
        }

        public static TfsGitRepositoryInfo CreateFromGit(ExternalLink gitExternalLink, IList<GitRepository> possibleRepos)
        {
            string commitID;
            string repoID;
            GitRepository gitRepo;
            //vstfs:///Git/Commit/25f94570-e3e7-4b79-ad19-4b434787fd5a%2f50477259-3058-4dff-ba4c-e8c179ec5327%2f41dd2754058348d72a6417c0615c2543b9b55535
            string guidbits = gitExternalLink.LinkedArtifactUri.Substring(gitExternalLink.LinkedArtifactUri.LastIndexOf('/') + 1);
            string[] bits = Regex.Split(guidbits, "%2f", RegexOptions.IgnoreCase);
            repoID = bits[1];
            if (bits.Count() >= 3)
            {
                commitID = $"{bits[2]}";
                for (int i = 3; i < bits.Count(); i++)
                {
                    commitID += $"%2f{bits[i]}";
                }
            }
            else
            {
                commitID = bits[2];
            }
            gitRepo =
                (from g in possibleRepos where string.Equals(g.Id.ToString(), repoID, StringComparison.OrdinalIgnoreCase) select g)
                .SingleOrDefault();
            return new TfsGitRepositoryInfo(commitID, repoID, gitRepo);
        }

        internal static TfsGitRepositoryInfo Create(string targetRepoName, TfsGitRepositoryInfo sourceRepoInfo, IList<GitRepository> targetRepos)
        {
            var gitRepo = (from g in targetRepos
                           where
                               g.Name == targetRepoName
                           select g).SingleOrDefault();
            return new TfsGitRepositoryInfo(sourceRepoInfo.CommitID, gitRepo?.Id.ToString(), gitRepo);
        }
    }
}