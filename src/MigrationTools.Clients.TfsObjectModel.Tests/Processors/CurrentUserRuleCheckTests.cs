using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Processors;

namespace MigrationTools.Tests.Processors
{
    [TestClass]
    public class CurrentUserRuleCheckTests
    {
        private static readonly HashSet<string> Identity = new HashSet<string> { "System.AssignedTo", "Microsoft.VSTS.Common.ResolvedBy" };

        private static ProcessRuleInfo Rule(string action, string field, bool disabled = false, bool custom = true, string wit = "Bug", string name = "r")
            => new ProcessRuleInfo
            {
                WorkItemType = wit,
                RuleName = name,
                IsDisabled = disabled,
                IsCustom = custom,
                Actions = new List<ProcessRuleActionInfo> { new ProcessRuleActionInfo { ActionType = action, TargetField = field } }
            };

        [TestMethod, TestCategory("L0")]
        public void Find_CopyFromCurrentUserOnIdentityField_Reported()
        {
            var found = CurrentUserRuleCheck.Find(new[] { Rule("copyFromCurrentUser", "System.AssignedTo") }, Identity.Contains);
            Assert.AreEqual(1, found.Count);
            Assert.AreEqual("System.AssignedTo", found[0].Field);
            Assert.AreEqual("Bug", found[0].WorkItemType);
        }

        [TestMethod, TestCategory("L0")]
        public void Find_ServerCurrentUser_Reported()
        {
            Assert.AreEqual(1, CurrentUserRuleCheck.Find(new[] { Rule("copyFromServerCurrentUser", "Microsoft.VSTS.Common.ResolvedBy") }, Identity.Contains).Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Find_DisabledRule_Ignored()
        {
            Assert.AreEqual(0, CurrentUserRuleCheck.Find(new[] { Rule("copyFromCurrentUser", "System.AssignedTo", disabled: true) }, Identity.Contains).Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Find_SystemRule_Ignored()
        {
            Assert.AreEqual(0, CurrentUserRuleCheck.Find(new[] { Rule("copyFromCurrentUser", "System.AssignedTo", custom: false) }, Identity.Contains).Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Find_DefaultFromCurrentUser_Ignored()
        {
            // A default only applies to an empty field on create, which the replay sets explicitly.
            Assert.AreEqual(0, CurrentUserRuleCheck.Find(new[] { Rule("setDefaultFromCurrentUser", "System.AssignedTo") }, Identity.Contains).Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Find_NonIdentityField_Ignored()
        {
            Assert.AreEqual(0, CurrentUserRuleCheck.Find(new[] { Rule("copyFromCurrentUser", "Custom.Text") }, Identity.Contains).Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Find_Duplicates_ReportedOnceAndSorted()
        {
            var found = CurrentUserRuleCheck.Find(new[]
            {
                Rule("copyFromCurrentUser", "System.AssignedTo", wit: "Task"),
                Rule("copyFromCurrentUser", "System.AssignedTo", wit: "Bug"),
                Rule("copyFromCurrentUser", "System.AssignedTo", wit: "Bug"),
            }, Identity.Contains);
            Assert.AreEqual(2, found.Count);
            Assert.AreEqual("Bug", found[0].WorkItemType);
            Assert.AreEqual("Task", found[1].WorkItemType);
        }

        [TestMethod, TestCategory("L0")]
        public void Find_NullRules_Empty()
        {
            Assert.AreEqual(0, CurrentUserRuleCheck.Find(null, Identity.Contains).Count);
        }
    }
}
