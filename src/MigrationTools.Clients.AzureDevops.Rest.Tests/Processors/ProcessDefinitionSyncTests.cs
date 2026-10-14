using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts.Process;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class ProcessDefinitionSyncTests
    {
        private static WorkItemType Wit(string referenceName, string customization) => new WorkItemType { ReferenceName = referenceName, Name = "Bug", Customization = customization };

        private static WorkItemControl Control(string id) => new WorkItemControl { Id = id, ControlType = "FieldControl" };

        [TestMethod(), TestCategory("L0")]
        public void TargetTypeMissing_IsCreated()
        {
            Assert.IsTrue(ProcessDefinitionSync.NeedsTargetWorkItemTypeCreated(Wit("src.Bug", "inherited"), null));
        }

        [TestMethod(), TestCategory("L0")]
        public void TargetOnlyHasLockedSystemType_InheritedCopyIsCreated()
        {
            Assert.IsTrue(ProcessDefinitionSync.NeedsTargetWorkItemTypeCreated(
                Wit("src.Bug", "inherited"), Wit("Microsoft.VSTS.WorkItemTypes.Bug", "system")));
        }

        [TestMethod(), TestCategory("L0")]
        public void TargetAlreadyInherited_IsUpdated()
        {
            Assert.IsFalse(ProcessDefinitionSync.NeedsTargetWorkItemTypeCreated(Wit("src.Bug", "inherited"), Wit("tgt.Bug", "inherited")));
        }

        [TestMethod(), TestCategory("L0")]
        public void BothSystem_NothingIsCreated()
        {
            Assert.IsFalse(ProcessDefinitionSync.NeedsTargetWorkItemTypeCreated(
                Wit("Microsoft.VSTS.WorkItemTypes.Bug", "system"), Wit("Microsoft.VSTS.WorkItemTypes.Bug", "system")));
        }

        [TestMethod(), TestCategory("L0")]
        public void InheritedBehavior_MatchesByReferenceName()
        {
            var stories = new WorkItemBehavior { ReferenceName = "System.RequirementBacklogBehavior", Name = "Stories", Customization = "inherited" };
            var target = new List<WorkItemBehavior>
            {
                new WorkItemBehavior { ReferenceName = "System.TaskBacklogBehavior", Name = "Tasks" },
                new WorkItemBehavior { ReferenceName = "System.RequirementBacklogBehavior", Name = "Stories" },
            };

            Assert.AreSame(target[1], ProcessDefinitionSync.FindTargetBehavior(target, stories));
        }

        [TestMethod(), TestCategory("L0")]
        public void CustomBehavior_FallsBackToName()
        {
            var source = new WorkItemBehavior { ReferenceName = "Custom.source-guid", Name = "Initiatives", Customization = "custom" };
            var target = new List<WorkItemBehavior> { new WorkItemBehavior { ReferenceName = "Custom.target-guid", Name = "initiatives" } };

            Assert.AreSame(target[0], ProcessDefinitionSync.FindTargetBehavior(target, source));
        }

        [TestMethod(), TestCategory("L0")]
        public void MissingBehavior_ReturnsNull()
        {
            var source = new WorkItemBehavior { ReferenceName = "Custom.x", Name = "Initiatives" };

            Assert.IsNull(ProcessDefinitionSync.FindTargetBehavior(null, source));
            Assert.IsNull(ProcessDefinitionSync.FindTargetBehavior(new List<WorkItemBehavior>(), source));
        }

        [TestMethod(), TestCategory("L0")]
        public void ControlInAnotherGroup_IsFoundForMove()
        {
            var g = new WorkItemGroup { Id = "g", Label = "G", Controls = new List<WorkItemControl> { Control("Custom.Other") } };
            var empty = new WorkItemGroup { Id = "e", Label = "E" };
            var o = new WorkItemGroup { Id = "o", Label = "O", Controls = new List<WorkItemControl> { Control("Custom.F") } };

            Assert.AreSame(o, ProcessDefinitionSync.FindGroupContainingControl(new[] { g, empty, o }, "custom.f", g));
        }

        [TestMethod(), TestCategory("L0")]
        public void ControlOnlyInDestinationGroup_IsNotAMove()
        {
            var g = new WorkItemGroup { Id = "g", Label = "G", Controls = new List<WorkItemControl> { Control("Custom.F") } };
            var gCopy = new WorkItemGroup { Id = "g", Label = "G", Controls = new List<WorkItemControl> { Control("Custom.F") } };

            Assert.IsNull(ProcessDefinitionSync.FindGroupContainingControl(new[] { gCopy }, "Custom.F", g));
        }

        [TestMethod(), TestCategory("L0")]
        public void NewControl_HasNoCurrentGroup()
        {
            Assert.IsNull(ProcessDefinitionSync.FindGroupContainingControl(new[] { new WorkItemGroup { Id = "e" }, null }, "Custom.F"));
            Assert.IsNull(ProcessDefinitionSync.FindGroupContainingControl(null, "Custom.F"));
        }

        [TestMethod(), TestCategory("L0")]
        public void NewProcess_UsesMappedName()
        {
            var maps = new Dictionary<string, string> { { "amtpd-src", "amtpd-tgt" } };

            Assert.AreEqual("amtpd-tgt", ProcessDefinitionSync.ResolveTargetProcessName(maps, "amtpd-src"));
            Assert.AreEqual("other", ProcessDefinitionSync.ResolveTargetProcessName(maps, "other"));
            Assert.AreEqual("other", ProcessDefinitionSync.ResolveTargetProcessName(null, "other"));
        }
    }
}
