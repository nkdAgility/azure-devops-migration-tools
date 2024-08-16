using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Tests;


namespace MigrationTools.ProcessorEnrichers.Tests
{
    [TestClass()]
    public class TfsRevisionManagerTests
    {

        private static List<RevisionItem> GetWorkItemWithRevisions(DateTime currentDateTime, int startHours = 1, int endHours = 1, bool dateIncreasing = true)
        {
            var revisions = new System.Collections.Generic.SortedDictionary<int, RevisionItem>();
            for (int i = startHours; i < endHours + startHours; i++)
            {
                DateTime dateTime = dateIncreasing ? currentDateTime.AddHours(i) : currentDateTime;
                revisions.Add(i, new RevisionItem() { Index = i, Number = i, ChangedDate = dateTime, OriginalChangedDate = dateTime });
            }

            return revisions.Values.ToList();
        }


        [TestMethod(), TestCategory("L0")]
        public void TfsRevisionManagerInSync1()
        {
            var processorEnricher = GetTfsRevisionManager();

            var currentDateTime = System.DateTime.Now;
            List<RevisionItem> source = GetWorkItemWithRevisions(currentDateTime, 1, 1);
            List<RevisionItem> target = GetWorkItemWithRevisions(currentDateTime, 1, 1);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.AreEqual(revs.Count, 0);

        }

        [TestMethod(), TestCategory("L0")]
        public void TfsRevisionManagerInSync10()
        {
            var processorEnricher = GetTfsRevisionManager();

            var currentDateTime = System.DateTime.Now;
            List<RevisionItem> source = GetWorkItemWithRevisions(currentDateTime, 1, 10);
            List<RevisionItem> target = GetWorkItemWithRevisions(currentDateTime, 1, 10);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.AreEqual(0, revs.Count);

        }

        [TestMethod(), TestCategory("L0")]
        public void TfsRevisionManagerSync1()
        {
            var processorEnricher = GetTfsRevisionManager();

            var currentDateTime = System.DateTime.Now;
            List<RevisionItem> source = GetWorkItemWithRevisions(currentDateTime, 1, 2);
            List<RevisionItem> target = GetWorkItemWithRevisions(currentDateTime, 1, 1);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.AreEqual(1, revs.Count);
        }

        [TestMethod(), TestCategory("L0")]
        public void TfsRevisionManagerSync10()
        {
            var processorEnricher = GetTfsRevisionManager();

            var currentDateTime = DateTime.Now;
            List<RevisionItem> source = GetWorkItemWithRevisions(currentDateTime, 1, 11);
            List<RevisionItem> target = GetWorkItemWithRevisions(currentDateTime, 1, 1);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.AreEqual(10, revs.Count);
        }

        [TestMethod(), TestCategory("L0")]
        public void TfsRevisionManagerReplayRevisionsOff()
        {
            var processorEnricher = GetTfsRevisionManager(new TfsRevisionManagerOptions()
            {
                Enabled = true,
                MaxRevisions = 0,
                ReplayRevisions = false,
            });

            var currentDateTime = DateTime.Now.AddDays(-100);
            List<RevisionItem> source = GetWorkItemWithRevisions(currentDateTime, 1, 4);
            List<RevisionItem> target = GetWorkItemWithRevisions(currentDateTime, 1, 1);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.AreEqual(1, revs.Count);
        }


        [TestMethod(), TestCategory("L0")]
        public void TfsRevisionManagerMaxRevision51()
        {
            var processorEnricher = GetTfsRevisionManager(new TfsRevisionManagerOptions()
            {
                Enabled = true,
                MaxRevisions = 5,
                ReplayRevisions = true,
            });

            var currentDateTime = DateTime.Now;
            List<RevisionItem> source = GetWorkItemWithRevisions(currentDateTime, 1, 2);
            List<RevisionItem> target = GetWorkItemWithRevisions(currentDateTime, 2, 2);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.AreEqual(0, revs.Count);
        }

        [TestMethod(), TestCategory("L0")]
        public void TfsRevisionManagerMaxRevision56()
        {
            var processorEnricher = GetTfsRevisionManager(new TfsRevisionManagerOptions()
            {
                Enabled = true,
                MaxRevisions = 5,
                ReplayRevisions = true,
            });

            var currentDateTime = DateTime.Now;
            List<RevisionItem> source = GetWorkItemWithRevisions(currentDateTime, 1, 7);
            List<RevisionItem> target = GetWorkItemWithRevisions(currentDateTime, 1, 1);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.AreEqual(5, revs.Count);
        }

        [TestMethod(), TestCategory("L0")]
        public void TfsRevisionManagerMaxRevision59()
        {
            var processorEnricher = GetTfsRevisionManager(new TfsRevisionManagerOptions()
            {
                Enabled = true,
                MaxRevisions = 5,
                ReplayRevisions = true,
            });

            var currentDateTime = DateTime.Now;
            List<RevisionItem> source = GetWorkItemWithRevisions(currentDateTime, 1, 10);

            var revs = processorEnricher.GetRevisionsToMigrate(source, null);

            Assert.AreEqual(5, revs.Count);
        }

        [TestMethod(), TestCategory("L0")]
        public void TfsRevisionManagerDatesMustBeIncreasing()
        {
            var processorEnricher = GetTfsRevisionManager();

            var currentDateTime = DateTime.Now;
            List<RevisionItem> source = GetWorkItemWithRevisions(currentDateTime, 1, 10, false);

            var revs = processorEnricher.GetRevisionsToMigrate(source, null);
            Assert.AreEqual(true, CheckDateIncreasing(revs));
        }

        private static bool CheckDateIncreasing(List<RevisionItem> revs)
        {
            DateTime lastDatetime = DateTime.MinValue;
            bool increasing = true;
            foreach (var rev in revs)
            {
                if (rev.ChangedDate == lastDatetime)
                {
                    increasing = false;
                }
                lastDatetime = rev.ChangedDate;
            }
            return increasing;
        }

        private static TfsRevisionManager GetTfsRevisionManager()
        {
            return GetTfsRevisionManager(new TfsRevisionManagerOptions() { Enabled = true, MaxRevisions = 0, ReplayRevisions = true });
        }

        private static TfsRevisionManager GetTfsRevisionManager(TfsRevisionManagerOptions options)
        {

            var sp = ServiceProviderHelper.GetMigrationToolServicesForUnitTests();
            sp.AddSingleton<TfsRevisionManager>();
            sp.Configure<TfsRevisionManagerOptions>(o =>
            {
                o.Enabled = options.Enabled;
                o.MaxRevisions = options.MaxRevisions;
                o.ReplayRevisions = options.ReplayRevisions;
            });
            return sp.BuildServiceProvider().GetService<TfsRevisionManager>();
        }
    }
}
