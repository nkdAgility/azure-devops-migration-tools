using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class DefinitionMigrationOutcomeTests
    {
        [TestMethod(), TestCategory("L0")]
        public void EverythingCreated_HasNoFailures()
        {
            var outcome = DefinitionMigrationOutcome.Create(selected: 3, notInTarget: 3, planned: 3, created: 3);

            Assert.IsFalse(outcome.HasFailures);
            Assert.AreEqual(0, outcome.Skipped);
            Assert.AreEqual(0, outcome.Failed);
        }

        [TestMethod(), TestCategory("L0")]
        public void AlreadyInTarget_DoesNotCount()
        {
            var outcome = DefinitionMigrationOutcome.Create(selected: 3, notInTarget: 1, planned: 1, created: 1);

            Assert.IsFalse(outcome.HasFailures);
            Assert.AreEqual(2, outcome.Existing);
        }

        [TestMethod(), TestCategory("L0")]
        public void EverythingAlreadyInTarget_HasNoFailures()
        {
            var outcome = DefinitionMigrationOutcome.Create(selected: 2, notInTarget: 0, planned: 0, created: 0);

            Assert.IsFalse(outcome.HasFailures);
        }

        [TestMethod(), TestCategory("L0")]
        public void NothingSelected_HasNoFailures()
        {
            var outcome = DefinitionMigrationOutcome.Create(selected: 0, notInTarget: 0, planned: 0, created: 0);

            Assert.IsFalse(outcome.HasFailures);
        }

        [TestMethod(), TestCategory("L0")]
        public void FilteredOut_IsSkippedAndFails()
        {
            var outcome = DefinitionMigrationOutcome.Create(selected: 4, notInTarget: 3, planned: 1, created: 1);

            Assert.IsTrue(outcome.HasFailures);
            Assert.AreEqual(2, outcome.Skipped);
            Assert.AreEqual(0, outcome.Failed);
        }

        [TestMethod(), TestCategory("L0")]
        public void RejectedByTarget_Fails()
        {
            var outcome = DefinitionMigrationOutcome.Create(selected: 2, notInTarget: 2, planned: 2, created: 1);

            Assert.IsTrue(outcome.HasFailures);
            Assert.AreEqual(0, outcome.Skipped);
            Assert.AreEqual(1, outcome.Failed);
        }

        [TestMethod(), TestCategory("L0")]
        public void SkippedAndRejected_ReportsBoth()
        {
            var outcome = DefinitionMigrationOutcome.Create(selected: 5, notInTarget: 4, planned: 3, created: 1);

            Assert.IsTrue(outcome.HasFailures);
            Assert.AreEqual(1, outcome.Existing);
            Assert.AreEqual(1, outcome.Skipped);
            Assert.AreEqual(2, outcome.Failed);
        }
    }
}
