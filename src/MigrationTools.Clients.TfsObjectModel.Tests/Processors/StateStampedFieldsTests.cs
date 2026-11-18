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
        public void SameIdentity_BareDisplayNameAgainstQualified_IsNotProvenSame()
        {
            // The object model qualifies only display names shared by several accounts, so a bare name cannot say which.
            Assert.IsFalse(StateStampedFields.SameValue("Ghost Person", "Ghost Person <ghost@example.invalid>", true));
            Assert.IsFalse(StateStampedFields.SameValue("Ghost Person", "Ghost Person <other@example.invalid>", true));
            Assert.IsFalse(StateStampedFields.SameValue("Ghost Person <ghost@example.invalid>", "Ghost Person", true));
        }

        [TestMethod, TestCategory("L0")]
        public void SameIdentity_SameAccount_MatchesInEveryProvableForm()
        {
            Assert.IsTrue(StateStampedFields.SameValue("Ghost Person", "Ghost Person", true));
            Assert.IsTrue(StateStampedFields.SameValue("Ghost Person <ghost@example.invalid>", "Ghost Person <ghost@example.invalid>", true));
            Assert.IsTrue(StateStampedFields.SameValue("ghost@example.invalid", "Ghost Person <ghost@example.invalid>", true));
        }

        [TestMethod, TestCategory("L0")]
        public void SameIdentity_MappedByEmail_MatchesQualifiedReadBack()
        {
            Assert.IsTrue(StateStampedFields.SameValue("ghost@example.invalid", "Ghost Person <GHOST@example.invalid>", true));
        }

        [TestMethod, TestCategory("L0")]
        public void SameIdentity_SameDisplayNameDifferentAccount_IsNotSame()
        {
            Assert.IsFalse(StateStampedFields.SameValue("Ghost Person <ghost@example.invalid>", "Ghost Person <other@example.invalid>", true));
            Assert.IsFalse(StateStampedFields.SameValue("Ghost Person", "Migration Account <migration@example.invalid>", true));
        }

        [TestMethod, TestCategory("L0")]
        public void SameIdentity_EmptyOnlyMatchesEmpty()
        {
            Assert.IsTrue(StateStampedFields.SameValue(null, "", true));
            Assert.IsFalse(StateStampedFields.SameValue(null, "Migration Account <migration@example.invalid>", true));
        }

        [TestMethod, TestCategory("L0")]
        public void FromRest_IdentityAndDate_ConvertToObjectModelForms()
        {
            var identity = Newtonsoft.Json.Linq.JObject.Parse("{\"displayName\":\"Ghost Person\",\"uniqueName\":\"ghost@example.invalid\"}");
            Assert.AreEqual("Ghost Person <ghost@example.invalid>", StateStampedFields.FromRest(identity));
            var date = (DateTime)StateStampedFields.FromRest("2023-05-17T09:30:00Z");
            Assert.AreEqual(Closed, date);
            Assert.AreEqual(DateTimeKind.Utc, date.Kind);
            Assert.AreEqual("Active", StateStampedFields.FromRest("Active"));
            Assert.IsNull(StateStampedFields.FromRest(null));
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
