using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Answers whether a commit or branch exists in a target Git repository, caching each answer per repository so a
    /// migration with thousands of links asks the server once per commit or branch.
    /// </summary>
    public class TfsGitTargetArtifactCache
    {
        private readonly Func<Guid, string, bool?> _commitExists;
        private readonly Func<Guid, string, bool?> _branchExists;
        private readonly Dictionary<string, bool> _cache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        /// <param name="commitExists">Returns true or false, or null when the server could not say (the answer is not cached).</param>
        /// <param name="branchExists">Takes the branch name without "refs/heads/"; null means unknown, as for commits.</param>
        public TfsGitTargetArtifactCache(Func<Guid, string, bool?> commitExists, Func<Guid, string, bool?> branchExists)
        {
            _commitExists = commitExists ?? throw new ArgumentNullException(nameof(commitExists));
            _branchExists = branchExists ?? throw new ArgumentNullException(nameof(branchExists));
        }

        public static TfsGitTargetArtifactCache ForClient(GitHttpClient client)
        {
            return new TfsGitTargetArtifactCache(
                (repoId, commitId) =>
                {
                    try
                    {
                        return client.GetCommitAsync(commitId, repoId).GetAwaiter().GetResult() != null;
                    }
                    catch (VssServiceException)
                    {
                        return false;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                },
                (repoId, branch) =>
                {
                    try
                    {
                        var refs = client.GetRefsAsync(repoId, filter: $"heads/{branch}").GetAwaiter().GetResult();
                        return refs.Any(r => string.Equals(r.Name, $"refs/heads/{branch}", StringComparison.OrdinalIgnoreCase));
                    }
                    catch (VssServiceException)
                    {
                        return false;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                });
        }

        public bool CommitExists(Guid repoId, string commitId) => Lookup($"{repoId}/commit/{commitId}", () => _commitExists(repoId, commitId));

        /// <param name="repoId">The target repository.</param>
        /// <param name="refId">The ref as it appears in a branch link, e.g. "GBfeature%2Fx".</param>
        public bool BranchExists(Guid repoId, string refId)
        {
            string branch = BranchNameFromRefId(refId);
            return branch != null && Lookup($"{repoId}/branch/{branch}", () => _branchExists(repoId, branch));
        }

        /// <summary>Converts a branch link ref ("GBfeature%2Fx") to a branch name ("feature/x"); null for tags or commits.</summary>
        public static string BranchNameFromRefId(string refId)
        {
            string decoded = Uri.UnescapeDataString(refId ?? string.Empty);
            return decoded.StartsWith("GB", StringComparison.Ordinal) && decoded.Length > 2 ? decoded.Substring(2) : null;
        }

        private bool Lookup(string key, Func<bool?> query)
        {
            if (_cache.TryGetValue(key, out bool cached))
            {
                return cached;
            }
            bool? exists = query();
            if (exists.HasValue)
            {
                _cache[key] = exists.Value;
            }
            return exists ?? false;
        }
    }
}
