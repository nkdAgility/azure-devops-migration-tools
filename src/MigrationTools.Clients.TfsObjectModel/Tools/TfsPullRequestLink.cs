using System;
using System.Text.RegularExpressions;

namespace MigrationTools.Tools
{
    /// <summary>What happens to a pull request link on the target work item (#3286).</summary>
    public enum PullRequestLinkAction
    {
        /// <summary>The target resolves the artifact link, so it stays as it is.</summary>
        Keep,
        /// <summary>The artifact link is replaced by a hyperlink to the source pull request's web page.</summary>
        ConvertToHyperlink,
        /// <summary>The link is removed (DropPullRequestLinks).</summary>
        Drop,
        /// <summary>The link could not be read, so it is left unchanged.</summary>
        LeaveUnchanged,
    }

    /// <summary>A pull request artifact link, <c>vstfs:///Git/PullRequestId/{project}%2F{repository}%2F{id}</c>.</summary>
    public sealed class TfsPullRequestLink
    {
        private static readonly Regex UriPattern = new Regex(@"^vstfs:///Git/PullRequestId/([^/%]+)(?:%2F|/)([^/%]+)(?:%2F|/)(\d+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public string Project { get; }
        public string Repository { get; }
        public int PullRequestId { get; }

        private TfsPullRequestLink(string project, string repository, int pullRequestId)
        {
            Project = project;
            Repository = repository;
            PullRequestId = pullRequestId;
        }

        public static bool TryParse(string artifactUri, out TfsPullRequestLink link)
        {
            link = null;
            var m = UriPattern.Match(artifactUri?.Trim() ?? string.Empty);
            if (!m.Success || !int.TryParse(m.Groups[3].Value, out int id))
            {
                return false;
            }
            link = new TfsPullRequestLink(Uri.UnescapeDataString(m.Groups[1].Value), Uri.UnescapeDataString(m.Groups[2].Value), id);
            return true;
        }

        /// <summary>A vstfs artifact link resolves only inside the organisation (collection) that holds the repository.</summary>
        public static bool SameCollection(Uri source, Uri target)
        {
            if (source == null || target == null)
            {
                return false;
            }
            return string.Equals(source.AbsoluteUri.TrimEnd('/'), target.AbsoluteUri.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
        }

        public static string WebUrl(Uri sourceCollection, string projectName, string repositoryName, int pullRequestId)
        {
            return $"{sourceCollection.AbsoluteUri.TrimEnd('/')}/{Uri.EscapeDataString(projectName)}/_git/{Uri.EscapeDataString(repositoryName)}/pullrequest/{pullRequestId}";
        }

        public static string Comment(int pullRequestId, string title)
        {
            return string.IsNullOrWhiteSpace(title) ? $"Pull request {pullRequestId}" : $"Pull request {pullRequestId}: {title.Trim()}";
        }
    }
}
