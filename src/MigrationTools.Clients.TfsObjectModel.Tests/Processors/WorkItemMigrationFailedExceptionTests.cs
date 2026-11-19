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
                "target revision 3 was not saved with the source's state-stamped values: ClosedDate saved as 'a' but the source is 'b'."));
            StringAssert.Contains(WorkItemMigrationFailedException.Advice(reason, "User Story"), "Bypass rules on work item updates");
        }

        [TestMethod, TestCategory("L0")]
        public void Advice_ReadBackMismatch_SaysAReRunCorrectsEmptyValues()
        {
            var advice = WorkItemMigrationFailedException.Advice("the target was not stored with the source's state-stamped values: revision 3 (source revision 3) Microsoft.VSTS.Common.StateChangeDate reads back from the server as '' but the source is 'x'.", "User Story");
            StringAssert.Contains(advice, "the re-run sets its current values from the source with no delete");
        }

        [TestMethod, TestCategory("L0")]
        public void Advice_SourceRevisionsUnreadable_SaysToRunAgain()
        {
            var reason = WorkItemMigrationFailedException.ServerReason(new InvalidOperationException(
                "the source revisions could not be read from Azure DevOps to check the state-stamped values: VS30063: You are not authorized"));
            var advice = WorkItemMigrationFailedException.Advice(reason, "User Story");
            StringAssert.Contains(advice, "run the migration again");
            Assert.IsFalse(advice.Contains("delete the target"), advice);
        }

        [TestMethod, TestCategory("L0")]
        public void Advice_RerunMismatch_PointsAtTheUserMappingBeforeMigratingAgain()
        {
            // Migrating again replays the same mapping, so the advice must not stop at "delete and migrate again" (#3290).
            var advice = WorkItemMigrationFailedException.Advice("the already migrated target work item does not match the source, stamped instead of the source values: revision 7 Microsoft.VSTS.Common.ActivatedBy is 'A' but source revision 7 is 'B'.", "User Story");
            StringAssert.Contains(advice, "user mapping");
            StringAssert.Contains(advice, "gives the same values until the mapping is right");
        }

        [TestMethod, TestCategory("L0")]
        public void Advice_TF26194_PointsAtBypassRules()
        {
            StringAssert.Contains(WorkItemMigrationFailedException.Advice("TF26194: The value for the field 'Closed Date' cannot be changed.", "Bug"), "Bypass rules on work item updates");
        }

        [TestMethod, TestCategory("L0")]
        public void PassThroughReplay_StopAndNetworkErrors_AreNotWrapped()
        {
            var stop = new MigrationTools.Exceptions.MigrationRunStoppedException("stop");
            var net = new System.Net.WebException("net");
            Assert.AreSame(stop, TfsWorkItemMigrationProcessor.PassThroughReplay(stop));
            Assert.AreSame(net, TfsWorkItemMigrationProcessor.PassThroughReplay(net));
            // The caller's retry catches WebException itself, so an aggregated one must come out unwrapped.
            Assert.AreSame(net, TfsWorkItemMigrationProcessor.PassThroughReplay(new AggregateException(net)));
            Assert.AreSame(stop, TfsWorkItemMigrationProcessor.PassThroughReplay(new AggregateException(new AggregateException(stop))));
            Assert.IsNull(TfsWorkItemMigrationProcessor.PassThroughReplay(new InvalidOperationException("save failed")));
        }

        [TestMethod, TestCategory("L0")]
        public void IsTransientNetworkError_ObjectModelConnectionLoss_IsRetried()
        {
            // The object model reports a dropped connection as TF400324, wrapping the WebException.
            var tf400324 = new Microsoft.TeamFoundation.TeamFoundationServiceUnavailableException("TF400324: Azure DevOps services are not available");
            Assert.IsTrue(TfsWorkItemMigrationProcessor.IsTransientNetworkError(tf400324));
            Assert.IsTrue(TfsWorkItemMigrationProcessor.IsTransientNetworkError(new Exception("outer", new System.Net.WebException("closed"))));
            Assert.IsTrue(TfsWorkItemMigrationProcessor.IsTransientNetworkError(new AggregateException(tf400324)));
            Assert.AreSame(tf400324, TfsWorkItemMigrationProcessor.PassThroughReplay(new AggregateException(tf400324)));
            Assert.IsFalse(TfsWorkItemMigrationProcessor.IsTransientNetworkError(new InvalidOperationException("TF26194")));
        }

        [TestMethod, TestCategory("L0")]
        public void Advice_Unknown_PointsAtDebugLogging()
        {
            StringAssert.Contains(WorkItemMigrationFailedException.Advice("something new", "Bug"), "MinimumLevel 'Debug'");
        }
    }
}
