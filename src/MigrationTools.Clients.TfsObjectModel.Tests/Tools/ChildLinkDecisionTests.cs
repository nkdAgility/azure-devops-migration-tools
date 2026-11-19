using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tools;

namespace MigrationTools.Tests.Tools
{
    [TestClass]
    public class ChildLinkDecisionTests
    {
        private const int SourceA = 10, SourceB = 20, SourceC = 30;
        private const int TargetA = 110, TargetB = 120, TargetC = 130;

        [TestMethod, TestCategory("L0")]
        public void Decide_ChildWithoutParent_Adds()
        {
            Assert.AreEqual(ChildLinkAction.Add,
                ChildLinkDecision.Decide(TargetA, new int[0], SourceA, new[] { SourceA }, inline: true));
        }

        [TestMethod, TestCategory("L0")]
        public void Decide_ChildReparentedAToB_MigratedBeforeA_SkipsAndKeepsB()
        {
            Assert.AreEqual(ChildLinkAction.SkipChildHasFinalParentElsewhere,
                ChildLinkDecision.Decide(TargetA, new[] { TargetB }, SourceA, new[] { SourceB }, inline: true));
        }

        [TestMethod, TestCategory("L0")]
        public void Decide_ChildReparentedAToBToC_ReplayingB_Skips()
        {
            Assert.AreEqual(ChildLinkAction.SkipChildHasFinalParentElsewhere,
                ChildLinkDecision.Decide(TargetB, new[] { TargetC }, SourceB, new[] { SourceC }, inline: true));
        }

        [TestMethod, TestCategory("L0")]
        public void Decide_HistoricalParentNotYetSaved_SkipsInsteadOfDeferring()
        {
            Assert.AreEqual(ChildLinkAction.SkipChildHasFinalParentElsewhere,
                ChildLinkDecision.Decide(0, new[] { TargetB }, SourceA, new[] { SourceB }, inline: true));
        }

        [TestMethod, TestCategory("L0")]
        public void Decide_ParentAddedAndRemovedWithinRevision_ChildHasNoFinalParent_Skips()
        {
            Assert.AreEqual(ChildLinkAction.SkipChildHasFinalParentElsewhere,
                ChildLinkDecision.Decide(TargetA, new[] { TargetB }, SourceA, new int[0], inline: true));
        }

        [TestMethod, TestCategory("L0")]
        public void Decide_FinalParent_ChildStillUnderOldParent_ReplacesParent()
        {
            Assert.AreEqual(ChildLinkAction.ReplaceParent,
                ChildLinkDecision.Decide(TargetB, new[] { TargetA }, SourceB, new[] { SourceB }, inline: true));
        }

        [TestMethod, TestCategory("L0")]
        public void Decide_FinalParentNotYetSaved_Defers()
        {
            Assert.AreEqual(ChildLinkAction.DeferUntilParentSaved,
                ChildLinkDecision.Decide(0, new[] { TargetA }, SourceB, new[] { SourceB }, inline: true));
        }

        [TestMethod, TestCategory("L0")]
        public void Decide_DuplicateParentLinksIncludingUnsaved_DoesNotThrowAndReplacesAll()
        {
            Assert.AreEqual(ChildLinkAction.ReplaceParent,
                ChildLinkDecision.Decide(TargetB, new[] { TargetA, TargetC, TargetA }, SourceB, new[] { SourceB, SourceB }, inline: false));
        }

        [TestMethod, TestCategory("L0")]
        public void Decide_ChildAlreadyUnderThisParentAmongDuplicates_AlreadyLinked()
        {
            Assert.AreEqual(ChildLinkAction.AlreadyLinked,
                ChildLinkDecision.Decide(TargetB, new[] { TargetB, TargetB }, SourceB, new[] { SourceB }, inline: true));
        }
    }
}
