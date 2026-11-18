using System;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Processors;

namespace MigrationTools.Tests.Processors
{
    [TestClass]
    public class StateStampedFieldsTests
    {
        private static readonly DateTime Closed = new DateTime(2023, 5, 17, 9, 30, 0, DateTimeKind.Utc);

        [TestMethod, TestCategory("L0")]
        public void Fields_IncludeEveryStateStampedDateAndByField()
        {
            CollectionAssert.AreEquivalent(new[]
            {
                StateStampedFields.CommonActivatedDate, StateStampedFields.CommonResolvedDate,
                StateStampedFields.CommonActivatedBy, StateStampedFields.CommonResolvedBy, StateStampedFields.CommonClosedBy
            }, StateStampedFields.Fields.ToList());
        }

        [TestMethod, TestCategory("L0")]
        public void SameValue_SameInstantInAnotherKind_IsSame()
        {
            Assert.IsTrue(StateStampedFields.SameValue(Closed, Closed.ToLocalTime()));
        }

        [TestMethod, TestCategory("L0")]
        public void SameValue_TargetStampedWithMigrationTime_IsNotSame()
        {
            Assert.IsFalse(StateStampedFields.SameValue(Closed, Closed.AddDays(400)));
        }

        [TestMethod, TestCategory("L0")]
        public void SameValue_SourceEmptyTargetStamped_IsNotSame()
        {
            // Parity both ways: a value the target stamped where the source has none is wrong (#3270, fault C).
            Assert.IsFalse(StateStampedFields.SameValue(null, "Migration Account <migration@example.invalid>"));
            Assert.IsFalse(StateStampedFields.SameValue(DBNull.Value, Closed));
        }

        [TestMethod, TestCategory("L0")]
        public void SameValue_EmptyForms_AreSame()
        {
            Assert.IsTrue(StateStampedFields.SameValue(null, DBNull.Value));
            Assert.IsTrue(StateStampedFields.SameValue("", null));
        }

        [TestMethod, TestCategory("L0")]
        public void SameValue_IdentityStrings_CompareOrdinal()
        {
            Assert.IsTrue(StateStampedFields.SameValue("Ghost Person <ghost@example.invalid>", "Ghost Person <ghost@example.invalid>"));
            Assert.IsFalse(StateStampedFields.SameValue("Ghost Person <ghost@example.invalid>", "Migration Account <migration@example.invalid>"));
            Assert.IsFalse(StateStampedFields.SameValue("ghost person", "Ghost Person"));
        }

        [TestMethod, TestCategory("L0")]
        public void IsFieldCannotBeChangedError_TF26194AnywhereInChain_ReturnsTrue()
        {
            var ex = new Exception("save failed", new Exception("TF26194: The value for the field 'Closed Date' cannot be changed."));
            Assert.IsTrue(StateStampedFields.IsFieldCannotBeChangedError(ex));
        }

        [TestMethod, TestCategory("L0")]
        public void IsFieldCannotBeChangedError_OtherErrors_ReturnFalse()
        {
            Assert.IsFalse(StateStampedFields.IsFieldCannotBeChangedError(new Exception("TF237124: Work Item is not ready to save")));
            Assert.IsFalse(StateStampedFields.IsFieldCannotBeChangedError(null));
        }

        [TestMethod, TestCategory("L0")]
        public void ResolveClosedDateField_PrefersCommon()
        {
            Assert.AreEqual(StateStampedFields.CommonClosedDate, StateStampedFields.ResolveClosedDateField(true, true));
            Assert.AreEqual(StateStampedFields.SystemClosedDate, StateStampedFields.ResolveClosedDateField(false, true));
            Assert.IsNull(StateStampedFields.ResolveClosedDateField(false, false));
        }

        [TestMethod, TestCategory("L0")]
        public void ToInternalDate_LocalAndUtc_GiveTheSameUtcInstant()
        {
            var local = Closed.ToLocalTime();
            Assert.AreEqual(Closed, ObjectModelFieldWriter.ToInternalDate(local));
            Assert.AreEqual(DateTimeKind.Utc, ObjectModelFieldWriter.ToInternalDate(local).Kind);
            Assert.AreEqual(Closed, ObjectModelFieldWriter.ToInternalDate(Closed));
        }

        [TestMethod, TestCategory("L0")]
        public void ObjectModelFieldWriter_InternalSetterExistsInShippedObjectModel()
        {
            // The migration stops before migrating if this setter is missing; this catches an object model upgrade that removes it.
            Assert.IsTrue(ObjectModelFieldWriter.IsAvailable, $"WorkItem.SetFieldValue(int, object, FieldFlags) not found in {typeof(WorkItem).Assembly.FullName}");
        }
    }
}
