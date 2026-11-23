using System;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.DataContracts.Pipelines;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Points a build definition's Azure Repos Git repository at the repository with the same name, or the name
    /// configured in <c>RepositoryNameMaps</c>, in the target project.
    /// </summary>
    /// <remarks>
    /// Repositories hosted outside the project (GitHub, Bitbucket, TFVC and so on) are left as they are.
    /// </remarks>
    public static class BuildDefinitionRepositoryMap
    {
        public const string AzureReposGitType = "TfsGit";

        public static bool UsesAzureReposGit(BuildDefinition definition)
        {
            return string.Equals(definition.Repository?.Type, AzureReposGitType, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Sets the definition's repository id to the target repository's id.
        /// </summary>
        /// <returns>The name of the target repository that was looked for, and whether it was found.</returns>
        public static (string TargetRepositoryName, bool Mapped) Apply(
            BuildDefinition definition,
            IEnumerable<GitRepository> sourceRepositories,
            IEnumerable<GitRepository> targetRepositories,
            IDictionary<string, string> repositoryNameMaps)
        {
            var sourceRepositoryName = sourceRepositories.FirstOrDefault(s => s.Id == definition.Repository.Id)?.Name
                ?? definition.Repository.Name
                ?? string.Empty;
            var targetRepositoryName = repositoryNameMaps != null && repositoryNameMaps.TryGetValue(sourceRepositoryName, out var mappedName)
                ? mappedName
                : sourceRepositoryName;
            var targetRepository = targetRepositories.FirstOrDefault(r => r.Name == targetRepositoryName);
            if (targetRepository is null)
            {
                return (targetRepositoryName, false);
            }
            definition.Repository.Id = targetRepository.Id;
            definition.Repository.Name = targetRepository.Name;
            return (targetRepositoryName, true);
        }
    }
}
