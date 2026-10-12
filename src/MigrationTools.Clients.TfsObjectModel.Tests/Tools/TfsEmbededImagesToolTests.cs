using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MigrationTools.Tools.Tests
{
    [TestClass]
    public class TfsEmbededImagesToolTests
    {
        [TestMethod, TestCategory("L0")]
        public void DestroyPlaceholders_DestroysEveryId()
        {
            var destroyed = new List<int>();

            var remaining = TfsEmbededImagesTool.DestroyPlaceholders(new[] { 1, 2, 3 }, destroyed.Add, NullLogger.Instance);

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, destroyed);
            Assert.AreEqual(0, remaining.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void DestroyPlaceholders_ContinuesPastFailuresAndReturnsTheFailedIds()
        {
            var destroyed = new List<int>();

            var remaining = TfsEmbededImagesTool.DestroyPlaceholders(new[] { 1, 2, 3 }, id =>
            {
                if (id == 2) throw new UnauthorizedAccessException("no destroy permission");
                destroyed.Add(id);
            }, NullLogger.Instance);

            CollectionAssert.AreEqual(new[] { 1, 3 }, destroyed);
            CollectionAssert.AreEqual(new[] { 2 }, remaining);
        }

        [TestMethod, TestCategory("L0")]
        public void DestroyPlaceholders_NoIds_DoesNothing()
        {
            var remaining = TfsEmbededImagesTool.DestroyPlaceholders(Array.Empty<int>(), _ => Assert.Fail("nothing to destroy"), NullLogger.Instance);

            Assert.AreEqual(0, remaining.Count);
        }
    }
}
