using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Processors;

namespace MigrationTools.Tests.Processors
{
    [TestClass]
    public class SourceStateStampedValuesTests
    {
        private static readonly DateTime Changed = new DateTime(2026, 11, 19, 14, 53, 54, 700, DateTimeKind.Utc);
        private const string StateChangeDate = StateStampedFields.CommonStateChangeDate;

        private static SourceStateStampedValues Values(params (int Rev, IDictionary<string, object> Fields)[] revisions)
            => SourceStateStampedValues.From(42, revisions);

        [TestMethod, TestCategory("L0")]
        public void Reconcile_OneReadEmpty_OtherHasValue_UsesTheValueAndReportsIt()
        {
            Assert.AreEqual(Changed, SourceStateStampedValues.Reconcile(null, Changed, false, out var problem));
            StringAssert.Contains(problem, "empty value");
            Assert.AreEqual(Changed, SourceStateStampedValues.Reconcile(Changed, DBNull.Value, false, out problem));
            Assert.IsNotNull(problem);
        }

        [TestMethod, TestCategory("L0")]
        public void Reconcile_BothEmpty_IsEmptyWithoutProblem()
        {
            Assert.IsNull(SourceStateStampedValues.Reconcile("", null, true, out var problem));
            Assert.IsNull(problem);
        }

        [TestMethod, TestCategory("L0")]
        public void Reconcile_SameInstantInAnotherKind_KeepsTheFirstRead()
        {
            var local = Changed.ToLocalTime();
            Assert.AreEqual(local, SourceStateStampedValues.Reconcile(local, Changed, false, out var problem));
            Assert.IsNull(problem);
        }

        [TestMethod, TestCategory("L0")]
        public void Reconcile_TwoDifferentValues_FailsClearly()
        {
            var ex = Assert.ThrowsExactly<InvalidOperationException>(() => SourceStateStampedValues.Reconcile(Changed.AddDays(1), Changed, false, out _));
            StringAssert.Contains(ex.Message, "cannot be decided");
        }

        [TestMethod, TestCategory("L0")]
        public void Reconcile_BareDisplayNameOfTheSameIdentity_KeepsTheFirstReadsForm()
        {
            Assert.AreEqual("Ghost Person", SourceStateStampedValues.Reconcile("Ghost Person", "Ghost Person <ghost@example.invalid>", true, out var problem));
            Assert.IsNull(problem);
        }

        [TestMethod, TestCategory("L0")]
        public void Reconcile_DifferentIdentity_FailsClearly()
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => SourceStateStampedValues.Reconcile("Migration Account <migration@example.invalid>", "Ghost Person <ghost@example.invalid>", true, out _));
        }

        [TestMethod, TestCategory("L0")]
        public void Value_ReadsTheRevisionsField_AndNullWhenEmptyOrMissing()
        {
            var values = Values((1, new Dictionary<string, object> { [StateChangeDate] = Changed }), (2, new Dictionary<string, object> { [StateChangeDate] = DBNull.Value }), (3, new Dictionary<string, object>()));
            Assert.AreEqual(Changed, values.Value(1, StateChangeDate));
            Assert.IsNull(values.Value(2, StateChangeDate));
            Assert.IsNull(values.Value(3, StateChangeDate));
        }

        [TestMethod, TestCategory("L0")]
        public void Value_RevisionNotRead_FailsClearly()
        {
            var ex = Assert.ThrowsExactly<InvalidOperationException>(() => Values((1, new Dictionary<string, object>())).Value(3, StateChangeDate));
            StringAssert.Contains(ex.Message, "revision 3");
        }

        [TestMethod, TestCategory("L0")]
        public void SameStoredValue_ValueSavedButReadBackEmpty_IsNotTheSame()
        {
            Assert.IsFalse(SourceStateStampedValues.SameStoredValue(Changed, null, false));
            Assert.IsFalse(SourceStateStampedValues.SameStoredValue("Ghost Person", null, true));
        }

        [TestMethod, TestCategory("L0")]
        public void SameStoredValue_SameDateOrQualifiedFormOfABareName_IsTheSame()
        {
            Assert.IsTrue(SourceStateStampedValues.SameStoredValue(Changed.ToLocalTime(), Changed, false));
            Assert.IsTrue(SourceStateStampedValues.SameStoredValue("Ghost Person", "Ghost Person <ghost@example.invalid>", true));
            Assert.IsTrue(SourceStateStampedValues.SameStoredValue(null, null, true));
        }

        [TestMethod, TestCategory("L0")]
        public void SameStoredValue_AnotherIdentityOrDate_IsNotTheSame()
        {
            Assert.IsFalse(SourceStateStampedValues.SameStoredValue("Ghost Person", "Migration Account <migration@example.invalid>", true));
            Assert.IsFalse(SourceStateStampedValues.SameStoredValue(Changed, Changed.AddSeconds(1), false));
            Assert.IsFalse(SourceStateStampedValues.SameStoredValue(null, Changed, false));
        }
    }
}
