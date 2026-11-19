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
                StateStampedFields.CommonActivatedDate, StateStampedFields.CommonResolvedDate, StateStampedFields.CommonStateChangeDate,
                StateStampedFields.CommonActivatedBy, StateStampedFields.CommonResolvedBy, StateStampedFields.CommonClosedBy
            }, StateStampedFields.Fields.ToList());
        }

        private const string Mapped = "Person A <TENANT\\person-a@example.invalid>";

        private static System.Collections.Generic.IReadOnlyList<System.Collections.Generic.IReadOnlyCollection<string>> Accounts(params string[][] uniqueNames) => uniqueNames;

        [TestMethod, TestCategory("L0")]
        public void CompareIdentity_MappedValueAgainstBareDisplayNameOfTheSameAccount_IsSame()
        {
            // #3290: the mapping file gives 'Display <tenant\upn>'; the object model reads the target back as 'Display'.
            var match = StateStampedFields.CompareIdentity(Mapped, "Person A",
                name => name == "Person A" ? Accounts(new[] { "person-a@example.invalid" }) : null);
            Assert.AreEqual(IdentityMatch.Same, match);
        }

        [TestMethod, TestCategory("L0")]
        public void CompareIdentity_BareDisplayNameOfADifferentAccount_IsDifferent()
        {
            var match = StateStampedFields.CompareIdentity(Mapped, "Person B",
                name => Accounts(new[] { "person-b@example.invalid", "TENANT\\person-b@example.invalid" }));
            Assert.AreEqual(IdentityMatch.Different, match);
        }

        [TestMethod, TestCategory("L0")]
        public void CompareIdentity_SameDisplayNameButAnotherAccount_IsDifferent()
        {
            var match = StateStampedFields.CompareIdentity(Mapped, "Person A", name => Accounts(new[] { "someone-else@example.invalid" }));
            Assert.AreEqual(IdentityMatch.Different, match);
        }

        [TestMethod, TestCategory("L0")]
        public void CompareIdentity_AccountCannotBeRead_IsUnproven()
        {
            Assert.AreEqual(IdentityMatch.Unproven, StateStampedFields.CompareIdentity(Mapped, "Person A", name => null));
            Assert.AreEqual(IdentityMatch.Unproven, StateStampedFields.CompareIdentity(Mapped, "Person A", name => Accounts()));
        }

        [TestMethod, TestCategory("L0")]
        public void CompareIdentity_DisplayNameSharedByTheMappedAccountAndAnother_IsUnproven()
        {
            var match = StateStampedFields.CompareIdentity(Mapped, "Person A",
                name => Accounts(new[] { "person-a@example.invalid" }, new[] { "other@example.invalid" }));
            Assert.AreEqual(IdentityMatch.Unproven, match);
        }

        [TestMethod, TestCategory("L0")]
        public void CompareIdentity_BothQualifiedWithDifferentAccounts_IsDifferentWithoutALookup()
        {
            var match = StateStampedFields.CompareIdentity(Mapped, "Person A <TENANT\\other@example.invalid>",
                name => throw new AssertFailedException("no lookup expected"));
            Assert.AreEqual(IdentityMatch.Different, match);
        }

        [TestMethod, TestCategory("L0")]
        public void CompareIdentity_SourceEmptyTargetSet_IsDifferent()
        {
            Assert.AreEqual(IdentityMatch.Different, StateStampedFields.CompareIdentity(null, "Person A", name => Accounts(new[] { "x@example.invalid" })));
        }

        [TestMethod, TestCategory("L0")]
        public void SameUniqueName_DomainQualifiedUpnMatchesBareUpn_ButNotBareAccounts()
        {
            Assert.IsTrue(StateStampedFields.SameUniqueName("TENANT\\person-a@example.invalid", "PERSON-A@example.invalid"));
            Assert.IsFalse(StateStampedFields.SameUniqueName("DOMAINA\\person", "DOMAINB\\person"));
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
