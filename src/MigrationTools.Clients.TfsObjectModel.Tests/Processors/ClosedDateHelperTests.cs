using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Processors;

namespace MigrationTools.Tests.Processors
{
    [TestClass]
    public class ClosedDateHelperTests
    {
        private static readonly DateTime Closed = new DateTime(2023, 5, 17, 9, 30, 0, DateTimeKind.Utc);

        [TestMethod, TestCategory("L0")]
        public void NeedsUpdate_TargetNull_SourceClosed_ReturnsTrue()
        {
            Assert.IsTrue(ClosedDateHelper.NeedsUpdate(Closed, null));
        }

        [TestMethod, TestCategory("L0")]
        public void NeedsUpdate_TargetIsMigrationTime_ReturnsTrue()
        {
            Assert.IsTrue(ClosedDateHelper.NeedsUpdate(Closed, DateTime.UtcNow));
        }

        [TestMethod, TestCategory("L0")]
        public void NeedsUpdate_SameInstantDifferentKind_ReturnsFalse()
        {
            Assert.IsFalse(ClosedDateHelper.NeedsUpdate(Closed, Closed.ToLocalTime()));
        }

        [TestMethod, TestCategory("L0")]
        public void NeedsUpdate_SourceNotClosed_NeverUpdates()
        {
            Assert.IsFalse(ClosedDateHelper.NeedsUpdate(null, null));
            Assert.IsFalse(ClosedDateHelper.NeedsUpdate(null, Closed));
            Assert.IsFalse(ClosedDateHelper.NeedsUpdate(DBNull.Value, Closed));
            Assert.IsFalse(ClosedDateHelper.NeedsUpdate("", Closed));
        }

        [TestMethod, TestCategory("L0")]
        public void StateStampedDateFields_IncludesActivatedDate()
        {
            CollectionAssert.Contains(ClosedDateHelper.StateStampedDateFields.ToList(), ClosedDateHelper.CommonActivatedDate);
        }

        [TestMethod, TestCategory("L0")]
        public void FieldsNeedingFixUp_ReturnsOnlyFieldsTheServerRestamped()
        {
            var activated = new DateTime(2020, 10, 7, 14, 50, 44, DateTimeKind.Utc);
            var fixUps = ClosedDateHelper.FieldsNeedingFixUp(new (string, object, object)[]
            {
                // Reactivation: the server stamped the migration time over the source activation date.
                (ClosedDateHelper.CommonActivatedDate, activated, DateTime.UtcNow),
                // Already matches.
                (ClosedDateHelper.CommonClosedDate, Closed, Closed.ToLocalTime()),
            });

            CollectionAssert.AreEqual(new List<string> { ClosedDateHelper.CommonActivatedDate }, fixUps);
        }

        [TestMethod, TestCategory("L0")]
        public void FieldsNeedingFixUp_SourceNotActivated_LeavesTargetAlone()
        {
            var fixUps = ClosedDateHelper.FieldsNeedingFixUp(new (string, object, object)[]
            {
                (ClosedDateHelper.CommonActivatedDate, null, DateTime.UtcNow),
            });

            Assert.AreEqual(0, fixUps.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void ResolveField_PrefersCommonClosedDate()
        {
            Assert.AreEqual(ClosedDateHelper.CommonClosedDate, ClosedDateHelper.ResolveField(true, true));
            Assert.AreEqual(ClosedDateHelper.SystemClosedDate, ClosedDateHelper.ResolveField(false, true));
            Assert.IsNull(ClosedDateHelper.ResolveField(false, false));
        }
    }
}
