using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Processors;

namespace MigrationTools.Tests.Processors
{
    [TestClass]
    public class SourceStateStampedValuesTests
    {
        private static readonly DateTime Changed = new DateTime(2026, 11, 19, 14, 53, 54, 700, DateTimeKind.Utc);
        private const string StateChangeDate = StateStampedFields.CommonStateChangeDate;
        private const string ClosedBy = StateStampedFields.CommonClosedBy;

        private static SourceStateStampedValues Values(params (int Rev, IDictionary<string, object> Fields)[] revisions)
            => SourceStateStampedValues.From(42, revisions);

        [TestMethod, TestCategory("L0")]
        public void Reconcile_ObjectModelReadEmpty_ButSourceHasValue_UsesSourceValueAndReportsIt()
        {
            var value = SourceStateStampedValues.Reconcile(null, Changed, false, out var problem);

            Assert.AreEqual(Changed, value);
            StringAssert.Contains(problem, "read it as empty");
        }

        [TestMethod, TestCategory("L0")]
        public void Reconcile_ObjectModelReadDBNull_ButSourceHasValue_UsesSourceValue()
        {
            Assert.AreEqual(Changed, SourceStateStampedValues.Reconcile(DBNull.Value, Changed, false, out var problem));
            Assert.IsNotNull(problem);
        }

        [TestMethod, TestCategory("L0")]
        public void Reconcile_BothEmpty_IsEmptyWithoutProblem()
        {
            Assert.IsNull(SourceStateStampedValues.Reconcile("", null, true, out var problem));
            Assert.IsNull(problem);
        }

        [TestMethod, TestCategory("L0")]
        public void Reconcile_SameInstantInAnotherKind_KeepsObjectModelValue()
        {
            var local = Changed.ToLocalTime();

            Assert.AreEqual(local, SourceStateStampedValues.Reconcile(local, Changed, false, out var problem));
            Assert.IsNull(problem);
        }

        [TestMethod, TestCategory("L0")]
        public void Reconcile_DifferentDate_UsesSourceValueAndReportsIt()
        {
            Assert.AreEqual(Changed, SourceStateStampedValues.Reconcile(Changed.AddDays(1), Changed, false, out var problem));
            StringAssert.Contains(problem, "but Azure DevOps has");
        }

        [TestMethod, TestCategory("L0")]
        public void Reconcile_ObjectModelHasValue_SourceHasNone_IsEmpty()
        {
            Assert.IsNull(SourceStateStampedValues.Reconcile(Changed, null, false, out var problem));
            StringAssert.Contains(problem, "no value");
        }

        [TestMethod, TestCategory("L0")]
        public void Reconcile_BareDisplayNameOfTheSameIdentity_KeepsObjectModelForm()
        {
            var value = SourceStateStampedValues.Reconcile("Ghost Person", "Ghost Person <ghost@example.invalid>", true, out var problem);

            Assert.AreEqual("Ghost Person", value);
            Assert.IsNull(problem);
        }

        [TestMethod, TestCategory("L0")]
        public void Reconcile_DifferentIdentity_UsesSourceValue()
        {
            var value = SourceStateStampedValues.Reconcile("Migration Account <migration@example.invalid>", "Ghost Person <ghost@example.invalid>", true, out var problem);

            Assert.AreEqual("Ghost Person <ghost@example.invalid>", value);
            Assert.IsNotNull(problem);
        }

        [TestMethod, TestCategory("L0")]
        public void Value_ReadsTheRevisionsField_AndNullWhenRestLeavesItOut()
        {
            var values = Values((1, new Dictionary<string, object> { [StateChangeDate] = Changed }), (2, new Dictionary<string, object>()));

            Assert.AreEqual(Changed, values.Value(1, StateChangeDate));
            Assert.IsNull(values.Value(2, StateChangeDate));
        }

        [TestMethod, TestCategory("L0")]
        public void Value_RevisionNotReturned_FailsClearly()
        {
            var values = Values((1, new Dictionary<string, object>()));

            var ex = Assert.ThrowsExactly<InvalidOperationException>(() => values.Value(3, StateChangeDate));
            StringAssert.Contains(ex.Message, "revision 3");
        }

        [TestMethod, TestCategory("L0")]
        public void Value_Identity_IsInTheObjectModelForm()
        {
            var values = Values((1, new Dictionary<string, object> { [ClosedBy] = new IdentityRef { DisplayName = "Ghost Person", UniqueName = "ghost@example.invalid" } }));

            Assert.AreEqual("Ghost Person <ghost@example.invalid>", values.Value(1, ClosedBy));
        }

        [TestMethod, TestCategory("L0")]
        public void Normalise_ConvertsRestForms()
        {
            Assert.AreEqual(Changed, SourceStateStampedValues.Normalise(new DateTimeOffset(Changed)));
            Assert.AreEqual(DateTimeKind.Utc, ((DateTime)SourceStateStampedValues.Normalise(new DateTime(2026, 1, 1))).Kind);
            Assert.IsNull(SourceStateStampedValues.Normalise(""));
            Assert.AreEqual("Active", SourceStateStampedValues.Normalise("Active"));
        }

        [TestMethod, TestCategory("L0")]
        public void SameStoredValue_ValueSavedButStoredEmpty_IsNotTheSame()
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

        [TestMethod, TestCategory("L0")]
        public void FormatIdentity_UsesWhatIsSet()
        {
            Assert.AreEqual("Ghost Person <ghost@example.invalid>", SourceStateStampedValues.FormatIdentity("Ghost Person", "ghost@example.invalid"));
            Assert.AreEqual("Ghost Person", SourceStateStampedValues.FormatIdentity("Ghost Person", null));
            Assert.AreEqual("ghost@example.invalid", SourceStateStampedValues.FormatIdentity("", "ghost@example.invalid"));
            Assert.AreEqual("ghost@example.invalid", SourceStateStampedValues.FormatIdentity("ghost@example.invalid", "ghost@example.invalid"));
            Assert.IsNull(SourceStateStampedValues.FormatIdentity(null, null));
        }
    }
}
