using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Serilog;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.Enrichers
{
    public class GitRepositoryInfo
    {
        public string CommitID { get; }
        public string RepoID { get; }
        public GitRepository GitRepo { get; }

        public GitRepositoryInfo(string CommitID, string RepoID, GitRepository GitRepo)
        {
            this.CommitID = CommitID;
            this.RepoID = RepoID;
            this.GitRepo = GitRepo;
        }

        public static GitRepositoryInfo Create(ExternalLink gitExternalLink, IList<GitRepository> possibleRepos, IMigrationEngine migrationEngine, string workItemSourceProjectName)
        {
            var repoType = DetermineFromLink(gitExternalLink.LinkedArtifactUri);
            switch (repoType)
            {
                case RepistoryType.Git:
                    return CreateFromGit(gitExternalLink, possibleRepos);

                case RepistoryType.TFVC:
                    return CreateFromTFVC(gitExternalLink, possibleRepos, migrationEngine.ChangeSetMapps.Items, migrationEngine.Source.Config.AsTeamProjectConfig().Project, workItemSourceProjectName);
            }

            return null;
        }

        private static GitRepositoryInfo CreateFromTFVC(ExternalLink gitExternalLink, IList<GitRepository> possibleRepos, ReadOnlyDictionary<int, string> changesetMapping, string sourceProjectName, string workItemSourceProjectName)
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
            return new GitRepositoryInfo(commitIDKvPair.Value, null, new GitRepository() { Name = workItemSourceProjectName });
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

        public static GitRepositoryInfo CreateFromGit(ExternalLink gitExternalLink, IList<GitRepository> possibleRepos)
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
                (from g in possibleRepos where g.Id.ToString() == repoID select g)
                .SingleOrDefault();
            return new GitRepositoryInfo(commitID, repoID, gitRepo);
        }

        internal static GitRepositoryInfo Create(string targetRepoName, GitRepositoryInfo sourceRepoInfo, IList<GitRepository> targetRepos)
        {
            var gitRepo = (from g in targetRepos
                           where
                               g.Name == targetRepoName
                           select g).SingleOrDefault();
            return new GitRepositoryInfo(sourceRepoInfo.CommitID, gitRepo?.Id.ToString(), gitRepo);
        }
    }
}