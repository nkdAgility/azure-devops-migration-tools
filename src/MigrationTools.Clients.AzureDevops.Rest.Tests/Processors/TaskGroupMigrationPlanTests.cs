using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts.Pipelines;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class TaskGroupMigrationPlanTests
    {
        private const string AvailableTask = "available-task";
        private const string MissingTask = "missing-task";

        private static TaskGroup Version(string id, string name, long major, params string[] taskIds) => new TaskGroup
        {
            Id = id,
            Name = name,
            Version = new Version { Major = major },
            Tasks = taskIds.Select(t => new TaskElement { DisplayName = t, Task = new TaskTask { Id = t } }).ToArray()
        };

        private static readonly List<TaskDefinition> AvailableTasks = new List<TaskDefinition> { new TaskDefinition { Id = AvailableTask } };

        [TestMethod(), TestCategory("L0")]
        public void AllVersionsCompatible_RootAndUpdatesArePlanned()
        {
            var source = new[] { Version("a", "A", 2, AvailableTask), Version("a", "A", 1, AvailableTask), Version("a", "A", 3, AvailableTask) };

            var plan = TaskGroupMigrationPlan.Create(source, new TaskGroup[0], AvailableTasks);

            Assert.AreEqual(1, plan.TotalTaskGroups);
            Assert.AreEqual(1, plan.TaskGroupsToMigrate);
            Assert.AreEqual(1, plan.RootVersions.Single().Version.Major);
            CollectionAssert.AreEqual(new long[] { 2, 3 }, plan.UpdatedVersions.Select(v => v.Version.Major).ToArray());
            Assert.IsEmpty(plan.Warnings);
        }

        [TestMethod(), TestCategory("L0")]
        public void RootVersionIncompatible_NoVersionIsPlannedAndOneWarningExplainsWhy()
        {
            // Issue #2976: v1 uses a task missing from the target, v2 is fine. Previously v2 was
            // planned as an update to a root that was never created, and the log said "1 of 0".
            var source = new[] { Version("a", "A", 1, MissingTask), Version("a", "A", 2, AvailableTask) };

            var plan = TaskGroupMigrationPlan.Create(source, new TaskGroup[0], AvailableTasks);

            Assert.AreEqual(1, plan.TotalTaskGroups);
            Assert.AreEqual(0, plan.TaskGroupsToMigrate);
            Assert.IsEmpty(plan.UpdatedVersions);
            Assert.HasCount(1, plan.Warnings);
            StringAssert.Contains(plan.Warnings[0], "1.*, 2.*");
        }

        [TestMethod(), TestCategory("L0")]
        public void HigherVersionIncompatible_LowerVersionsArePlannedAndHigherSkipped()
        {
            var source = new[] { Version("a", "A", 1, AvailableTask), Version("a", "A", 2, MissingTask), Version("a", "A", 3, AvailableTask) };

            var plan = TaskGroupMigrationPlan.Create(source, new TaskGroup[0], AvailableTasks);

            Assert.AreEqual(1, plan.RootVersions.Single().Version.Major);
            Assert.IsEmpty(plan.UpdatedVersions);
            StringAssert.Contains(plan.Warnings.Single(), "2.*, 3.*");
        }

        [TestMethod(), TestCategory("L0")]
        public void LowestSourceVersionAboveOne_IsCreatedAsRoot()
        {
            var source = new[] { Version("a", "A", 2, AvailableTask), Version("a", "A", 3, AvailableTask) };

            var plan = TaskGroupMigrationPlan.Create(source, new TaskGroup[0], AvailableTasks);

            Assert.AreEqual(2, plan.RootVersions.Single().Version.Major);
            Assert.AreEqual(3, plan.UpdatedVersions.Single().Version.Major);
        }

        [TestMethod(), TestCategory("L0")]
        public void ExistingInTarget_IsSkippedAndCountedSeparately()
        {
            var source = new[] { Version("a", "A", 1, MissingTask), Version("a", "A", 2, AvailableTask), Version("b", "B", 1, AvailableTask) };
            var target = new[] { Version("x", "A", 1) };

            var plan = TaskGroupMigrationPlan.Create(source, target, AvailableTasks);

            Assert.AreEqual(2, plan.TotalTaskGroups);
            Assert.AreEqual(1, plan.ExistingTaskGroups);
            Assert.AreEqual("B", plan.RootVersions.Single().Name);
            Assert.IsEmpty(plan.UpdatedVersions);
            Assert.IsEmpty(plan.Warnings);
        }
    }
}
