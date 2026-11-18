using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Processors;

namespace MigrationTools.Tests.Processors
{
    [TestClass]
    public class WorkItemMigrationFailedExceptionTests
    {
        private static readonly Exception DisabledType = new Exception("There is a problem on the server.",
            new Exception("VS403417: ChangedBy value cannot be empty when BypassRules is specified\r\nmore"));

        [TestMethod, TestCategory("L0")]
        public void Describe_SaysWhatWhyAndWhatToDo()
        {
            var m = WorkItemMigrationFailedException.Describe("545", 1, null, "Bug", DisabledType);
            StringAssert.StartsWith(m, "Source work item 545 revision 1 (Bug) could not be saved to the target.");
            StringAssert.Contains(m, "Reason: VS403417: ChangedBy value cannot be empty when BypassRules is specified more");
            StringAssert.Contains(m, "What to do: the target would not create or update a 'Bug'");
            Assert.IsFalse(m.Contains("\n"), "one line, no stack trace");
        }

        [TestMethod, TestCategory("L0")]
        public void Describe_NamesTheTargetWhenItExists()
        {
            StringAssert.Contains(WorkItemMigrationFailedException.Describe("497", 3, "568", "User Story", new Exception("x")),
                "could not be saved to target work item 568.");
        }

        [TestMethod, TestCategory("L0")]
        public void Advice_StateStampedMismatch_PointsAtBypassRules()
        {
            var reason = WorkItemMigrationFailedException.ServerReason(new InvalidOperationException(
                "target revision 3 was saved with values the service stamped instead of the source values: ClosedDate saved as 'a' but the source is 'b'."));
            StringAssert.Contains(WorkItemMigrationFailedException.Advice(reason, "User Story"), "Bypass rules on work item updates");
        }

        [TestMethod, TestCategory("L0")]
        public void Advice_TF26194_PointsAtBypassRules()
        {
            StringAssert.Contains(WorkItemMigrationFailedException.Advice("TF26194: The value for the field 'Closed Date' cannot be changed.", "Bug"), "Bypass rules on work item updates");
        }

        [TestMethod, TestCategory("L0")]
        public void Advice_Unknown_PointsAtDebugLogging()
        {
            StringAssert.Contains(WorkItemMigrationFailedException.Advice("something new", "Bug"), "MinimumLevel 'Debug'");
        }
    }
}
