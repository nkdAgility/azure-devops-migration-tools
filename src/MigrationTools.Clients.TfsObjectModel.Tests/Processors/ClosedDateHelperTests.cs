using System;
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
        public void ResolveField_PrefersCommonClosedDate()
        {
            Assert.AreEqual(ClosedDateHelper.CommonClosedDate, ClosedDateHelper.ResolveField(true, true));
            Assert.AreEqual(ClosedDateHelper.SystemClosedDate, ClosedDateHelper.ResolveField(false, true));
            Assert.IsNull(ClosedDateHelper.ResolveField(false, false));
        }
    }
}
