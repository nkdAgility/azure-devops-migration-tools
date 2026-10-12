using System;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.DataContracts.Pipelines;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Decides which versions of which source task groups can be migrated.
    /// </summary>
    /// <remarks>
    /// Azure DevOps returns one entry per major version of a task group, all sharing the task group id.
    /// A version can only be created in the target on top of the versions below it, so the versions of
    /// a task group are treated as a unit: the lowest version is created as the root, each following
    /// version is added on top of it, and once a version cannot be migrated (for example because one of
    /// its tasks is not installed in the target) neither it nor any higher version is migrated.
    /// </remarks>
    public class TaskGroupMigrationPlan
    {
        /// <summary>Lowest version of each task group to migrate; created first.</summary>
        public List<TaskGroup> RootVersions { get; } = new List<TaskGroup>();

        /// <summary>Higher versions to add on top of the root versions, in ascending version order.</summary>
        public List<TaskGroup> UpdatedVersions { get; } = new List<TaskGroup>();

        /// <summary>Number of distinct source task groups.</summary>
        public int TotalTaskGroups { get; private set; }

        /// <summary>Number of source task groups that already exist in the target, by name.</summary>
        public int ExistingTaskGroups { get; private set; }

        /// <summary>Number of task groups with at least one version to migrate.</summary>
        public int TaskGroupsToMigrate => RootVersions.Count;

        /// <summary>One message per task group, or task group version, that will not be migrated.</summary>
        public List<string> Warnings { get; } = new List<string>();

        public static TaskGroupMigrationPlan Create(IEnumerable<TaskGroup> sourceDefinitions, IEnumerable<TaskGroup> targetDefinitions, IEnumerable<TaskDefinition> availableTasks)
        {
            var plan = new TaskGroupMigrationPlan();
            var targetNames = new HashSet<string>(targetDefinitions.Where(t => t.Name != null).Select(t => t.Name));
            var availableMajorsByTaskId = availableTasks
                .Where(a => a.Id != null)
                .GroupBy(a => a.Id, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => new HashSet<long>(g.Where(a => a.Version != null).Select(a => a.Version.Major)), StringComparer.OrdinalIgnoreCase);
            bool IsAvailable(TaskTask task)
            {
                if (task?.Id == null || !availableMajorsByTaskId.TryGetValue(task.Id, out var majors))
                {
                    return false;
                }
                // A spec without a fixed major ("*") or a target that doesn't report versions accepts any version.
                var requiredMajor = RequiredMajor(task.VersionSpec);
                return requiredMajor is null || majors.Count == 0 || majors.Contains(requiredMajor.Value);
            }

            var taskGroups = sourceDefinitions.GroupBy(d => string.IsNullOrEmpty(d.Id) ? d.Name : d.Id).ToList();
            plan.TotalTaskGroups = taskGroups.Count;

            foreach (var taskGroup in taskGroups)
            {
                var versions = taskGroup.OrderBy(d => d.Version?.Major ?? 0).ToList();
                var name = versions[0].Name;
                if (targetNames.Contains(name))
                {
                    plan.ExistingTaskGroups++;
                    continue;
                }

                var migratable = new List<TaskGroup>();
                foreach (var version in versions)
                {
                    var missingTaskNames = (version.Tasks ?? Array.Empty<TaskElement>())
                        .Where(t => !IsAvailable(t.Task))
                        .Select(t => RequiredMajor(t.Task?.VersionSpec) is long major ? $"{t.DisplayName} ({major}.*)" : t.DisplayName)
                        .ToList();
                    if (missingTaskNames.Count == 0)
                    {
                        migratable.Add(version);
                        continue;
                    }

                    var skippedVersions = versions.Skip(migratable.Count).Select(v => $"{v.Version?.Major}.*");
                    plan.Warnings.Add(migratable.Count == 0
                        ? $@"TaskGroup ""{name}"" cannot be migrated because version {version.Version?.Major}.* uses the Task(s) ""{string.Join(",", missingTaskNames)}"", which are not available in the target. This usually happens if the extension for the task is not installed. Higher versions are not migrated either, because a version can only be created on top of the versions below it. Skipped versions: {string.Join(", ", skippedVersions)}."
                        : $@"TaskGroup ""{name}"" version {version.Version?.Major}.* cannot be migrated because it uses the Task(s) ""{string.Join(",", missingTaskNames)}"", which are not available in the target. This usually happens if the extension for the task is not installed. Skipped versions: {string.Join(", ", skippedVersions)}.");
                    break;
                }

                if (migratable.Count == 0)
                {
                    continue;
                }
                plan.RootVersions.Add(migratable[0]);
                plan.UpdatedVersions.AddRange(migratable.Skip(1));
            }

            plan.UpdatedVersions.Sort((a, b) => (a.Version?.Major ?? 0).CompareTo(b.Version?.Major ?? 0));
            return plan;
        }

        private static long? RequiredMajor(string versionSpec)
        {
            var major = versionSpec?.Split('.')[0];
            return long.TryParse(major, out var value) ? value : null;
        }
    }
}
