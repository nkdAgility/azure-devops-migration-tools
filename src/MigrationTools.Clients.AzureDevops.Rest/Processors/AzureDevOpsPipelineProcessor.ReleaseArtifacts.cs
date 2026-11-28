using System;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.DataContracts;
using MigrationTools.DataContracts.Pipelines;

namespace MigrationTools.Processors
{
    /// <summary>
    /// What the target already holds that a release definition's artifacts can be pointed at.
    /// </summary>
    public class ReleaseArtifactTargets
    {
        public string SourceProjectName { get; set; }

        public ArtifactSourceDefinitionUrl TargetProject { get; set; }

        public IEnumerable<Mapping> BuildDefinitionMappings { get; set; } = Enumerable.Empty<Mapping>();

        public IEnumerable<GitRepository> SourceRepositories { get; set; } = Enumerable.Empty<GitRepository>();

        public IEnumerable<GitRepository> TargetRepositories { get; set; } = Enumerable.Empty<GitRepository>();

        public IDictionary<string, string> RepositoryNameMaps { get; set; } = new Dictionary<string, string>();

        public IEnumerable<Mapping> ServiceConnectionMappings { get; set; } = Enumerable.Empty<Mapping>();
    }

    /// <summary>
    /// Points the artifacts of a release definition at the target: Build and Azure Repos Git artifacts of the migrated
    /// project at the target project's build definition or repository, and artifacts that come through a service
    /// connection at the target service connection.
    /// </summary>
    public static class ReleaseArtifactMap
    {
        public const string BuildType = "Build";
        public const string GitType = "Git";
        public const string AzureArtifactsType = "PackageManagement";

        /// <summary>
        /// Maps every artifact of the definition, stopping at the first one that cannot be mapped.
        /// </summary>
        /// <returns>The artifact that could not be mapped and why, or <c>null</c> when every artifact was mapped.</returns>
        public static (Artifact Artifact, string Reason)? Apply(ReleaseDefinition definition, ReleaseArtifactTargets targets)
        {
            foreach (var artifact in definition.Artifacts ?? Array.Empty<Artifact>())
            {
                var reason = Apply(artifact, targets);
                if (reason is not null)
                {
                    return (artifact, reason);
                }
            }
            return null;
        }

        /// <returns>Why the artifact cannot be mapped, or <c>null</c> when it was mapped.</returns>
        public static string Apply(Artifact artifact, ReleaseArtifactTargets targets)
        {
            var reference = artifact.DefinitionReference;
            if (reference is null)
            {
                return "it has no definition reference";
            }
            if (string.Equals(artifact.Type, AzureArtifactsType, StringComparison.OrdinalIgnoreCase))
            {
                return "it reads an Azure Artifacts feed, and feeds are not migrated";
            }
            if (string.Equals(artifact.Type, BuildType, StringComparison.OrdinalIgnoreCase))
            {
                return MapBuild(artifact, reference, targets);
            }
            if (string.Equals(artifact.Type, GitType, StringComparison.OrdinalIgnoreCase))
            {
                return MapGit(artifact, reference, targets);
            }
            if (reference.Connection is not null)
            {
                return MapConnection(artifact, reference, targets);
            }
            return $"artifacts of type {artifact.Type} are not mapped";
        }

        private static string MapBuild(Artifact artifact, DefinitionReference reference, ReleaseArtifactTargets targets)
        {
            var notInProject = CheckFromMigratedProject(reference, targets);
            if (notInProject is not null)
            {
                return notInProject;
            }
            var mapping = targets.BuildDefinitionMappings.FirstOrDefault(m => m.SourceId == reference.Definition?.Id);
            if (mapping is null)
            {
                return $"its build definition \"{reference.Definition?.Name}\" is not in the target project; migrate it with MigrateBuildPipelines";
            }
            PointAtTarget(artifact, reference, targets, mapping.TargetId, mapping.Name);
            return null;
        }

        private static string MapGit(Artifact artifact, DefinitionReference reference, ReleaseArtifactTargets targets)
        {
            var notInProject = CheckFromMigratedProject(reference, targets);
            if (notInProject is not null)
            {
                return notInProject;
            }
            var sourceName = targets.SourceRepositories.FirstOrDefault(r => r.Id == reference.Definition?.Id)?.Name
                ?? reference.Definition?.Name
                ?? string.Empty;
            var targetName = targets.RepositoryNameMaps != null && targets.RepositoryNameMaps.TryGetValue(sourceName, out var mappedName)
                ? mappedName
                : sourceName;
            var targetRepository = targets.TargetRepositories.FirstOrDefault(r => r.Name == targetName);
            if (targetRepository is null)
            {
                return $"its repository \"{targetName}\" is not in the target project; migrate it first, or add a RepositoryNameMaps entry";
            }
            PointAtTarget(artifact, reference, targets, targetRepository.Id, targetRepository.Name);
            return null;
        }

        private static string MapConnection(Artifact artifact, DefinitionReference reference, ReleaseArtifactTargets targets)
        {
            var sourceConnectionId = reference.Connection.Id;
            var mapping = targets.ServiceConnectionMappings.FirstOrDefault(m => m.SourceId == sourceConnectionId);
            if (mapping is null)
            {
                return $"its service connection \"{reference.Connection.Name}\" is not in the target; migrate it with MigrateServiceConnections";
            }
            reference.Connection = new ArtifactSourceDefinitionUrl { Id = mapping.TargetId, Name = mapping.Name };
            if (artifact.SourceId is not null && sourceConnectionId is not null && artifact.SourceId.StartsWith(sourceConnectionId + ":", StringComparison.OrdinalIgnoreCase))
            {
                artifact.SourceId = mapping.TargetId + artifact.SourceId.Substring(sourceConnectionId.Length);
            }
            return null;
        }

        private static string CheckFromMigratedProject(DefinitionReference reference, ReleaseArtifactTargets targets)
        {
            var projectName = reference.Project?.Name;
            if (!string.Equals(projectName, targets.SourceProjectName, StringComparison.OrdinalIgnoreCase))
            {
                return $"it comes from project \"{projectName}\", and only artifacts of the migrated project \"{targets.SourceProjectName}\" are mapped";
            }
            if (targets.TargetProject?.Id is null)
            {
                return "the target project's id could not be read";
            }
            return null;
        }

        private static void PointAtTarget(Artifact artifact, DefinitionReference reference, ReleaseArtifactTargets targets, string targetId, string targetName)
        {
            reference.Definition = new ArtifactSourceDefinitionUrl { Id = targetId, Name = targetName };
            reference.Project = new ArtifactSourceDefinitionUrl { Id = targets.TargetProject.Id, Name = targets.TargetProject.Name };
            artifact.SourceId = $"{targets.TargetProject.Id}:{targetId}";
        }
    }
}
