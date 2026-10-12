using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MigrationTools.Tools.Tests
{
    [TestClass]
    public class EmbededImagesPlaceholderTrackerTests
    {
        private readonly List<int> _destroyed = new List<int>();
        private readonly HashSet<int> _failing = new HashSet<int>();

        private EmbededImagesPlaceholderTracker CreateTracker(int capacity = 3)
        {
            return new EmbededImagesPlaceholderTracker(capacity, id =>
            {
                if (_failing.Contains(id)) throw new UnauthorizedAccessException("no destroy permission");
                _destroyed.Add(id);
            }, NullLogger.Instance);
        }

        // mirrors the tool: replace when at capacity, create when there is none, then count the use
        private static void Upload(EmbededImagesPlaceholderTracker tracker, ref int nextId)
        {
            if (tracker.IsAtCapacity) tracker.Retire();
            if (tracker.Current == null) tracker.Created(nextId++);
            tracker.Used();
        }

        [TestMethod, TestCategory("L0")]
        public void NoUploads_CreatesNothingAndDestroysNothing()
        {
            var tracker = CreateTracker();

            var remaining = tracker.DestroyAll();

            Assert.IsNull(tracker.Current);
            Assert.AreEqual(0, remaining.Count);
            Assert.AreEqual(0, _destroyed.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void UploadsWithinCapacity_UseOnePlaceholder_DestroyedAtEnd()
        {
            var tracker = CreateTracker(capacity: 3);
            int nextId = 100;

            for (int i = 0; i < 4; i++) Upload(tracker, ref nextId);

            Assert.AreEqual(101, nextId, "only one placeholder should be created");
            Assert.AreEqual(0, _destroyed.Count, "nothing is destroyed before the end of the run");

            var remaining = tracker.DestroyAll();

            CollectionAssert.AreEqual(new[] { 100 }, _destroyed);
            Assert.AreEqual(0, remaining.Count);
            Assert.AreEqual(0, tracker.Pending.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void PastCapacity_RetiresAndDestroysOldPlaceholderBeforeCreatingNext()
        {
            var tracker = CreateTracker(capacity: 3);
            int nextId = 100;

            for (int i = 0; i < 5; i++) Upload(tracker, ref nextId);

            CollectionAssert.AreEqual(new[] { 100 }, _destroyed, "the full placeholder is destroyed when replaced");
            Assert.AreEqual(101, tracker.Current);
            CollectionAssert.AreEqual(new[] { 101 }, (System.Collections.ICollection)tracker.Pending);

            tracker.DestroyAll();

            CollectionAssert.AreEqual(new[] { 100, 101 }, _destroyed);
        }

        [TestMethod, TestCategory("L0")]
        public void ManyRollovers_CreateOnePlaceholderPerCapacityBlock()
        {
            var tracker = CreateTracker(capacity: 900);
            int nextId = 1;

            for (int i = 0; i < 2000; i++) Upload(tracker, ref nextId);
            tracker.DestroyAll();

            // 901 uses fit in each placeholder, so 2000 uploads need 3
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, _destroyed);
        }

        [TestMethod, TestCategory("L0")]
        public void RetireFails_IdStaysPendingAndIsRetriedAtEnd()
        {
            var tracker = CreateTracker(capacity: 3);
            int nextId = 100;
            _failing.Add(100);

            for (int i = 0; i < 5; i++) Upload(tracker, ref nextId);

            Assert.AreEqual(101, tracker.Current, "a new placeholder is still created");
            CollectionAssert.AreEqual(new[] { 100, 101 }, (System.Collections.ICollection)tracker.Pending);

            _failing.Clear();
            var remaining = tracker.DestroyAll();

            CollectionAssert.AreEqual(new[] { 100, 101 }, _destroyed);
            Assert.AreEqual(0, remaining.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void DestroyAll_ContinuesPastFailuresAndReturnsTheFailedIds()
        {
            var tracker = CreateTracker(capacity: 3);
            int nextId = 100;
            _failing.Add(100);
            _failing.Add(101);
            _failing.Add(102);
            for (int i = 0; i < 9; i++) Upload(tracker, ref nextId);

            var remaining = tracker.DestroyAll();

            CollectionAssert.AreEqual(new[] { 100, 101, 102 }, remaining);
            Assert.AreEqual(0, tracker.Pending.Count, "ids are reported once, not retried on the next run");
            Assert.IsNull(tracker.Current);
        }

        [TestMethod, TestCategory("L0")]
        public void DestroyAll_ThenMoreUploads_StartsAFreshPlaceholder()
        {
            var tracker = CreateTracker(capacity: 3);
            int nextId = 100;
            Upload(tracker, ref nextId);
            tracker.DestroyAll();

            Upload(tracker, ref nextId);

            Assert.AreEqual(101, tracker.Current, "a second processor in the same run gets its own placeholder");
        }
    }
}
