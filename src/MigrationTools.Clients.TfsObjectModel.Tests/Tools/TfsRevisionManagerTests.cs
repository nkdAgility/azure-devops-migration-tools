using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Tools;


namespace MigrationTools.Tests.Tools
{
    [TestClass()]
    public class TfsRevisionManagerToolTests
    {

        private static List<RevisionItem> GetWorkItemWithRevisions(DateTime currentDateTime, int startHours = 1, int endHours = 1, bool dateIncreasing = true)
        {
            var revisions = new SortedDictionary<int, RevisionItem>();
            for (int i = startHours; i < endHours + startHours; i++)
            {
                DateTime dateTime = dateIncreasing ? currentDateTime.AddHours(i) : currentDateTime;
                revisions.Add(i, new RevisionItem() { Index = i, Number = i, ChangedDate = dateTime, OriginalChangedDate = dateTime });
            }

            return revisions.Values.ToList();
        }


        [TestMethod(), TestCategory("L0")]
        public void TfsRevisionManagerToolInSync1()
        {
            var processorEnricher = GetTfsRevisionManagerTool();

            var currentDateTime = DateTime.Now;
            List<RevisionItem> source = GetWorkItemWithRevisions(currentDateTime, 1, 1);
            List<RevisionItem> target = GetWorkItemWithRevisions(currentDateTime, 1, 1);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.IsEmpty(revs);
        }

        [TestMethod(), TestCategory("L0")]
        public void TfsRevisionManagerToolInSync10()
        {
            var processorEnricher = GetTfsRevisionManagerTool();

            var currentDateTime = DateTime.Now;
            List<RevisionItem> source = GetWorkItemWithRevisions(currentDateTime, 1, 10);
            List<RevisionItem> target = GetWorkItemWithRevisions(currentDateTime, 1, 10);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.IsEmpty(revs);
        }

        [TestMethod(), TestCategory("L0")]
        public void TfsRevisionManagerToolSync1()
        {
            var processorEnricher = GetTfsRevisionManagerTool();

            var currentDateTime = DateTime.Now;
            List<RevisionItem> source = GetWorkItemWithRevisions(currentDateTime, 1, 2);
            List<RevisionItem> target = GetWorkItemWithRevisions(currentDateTime, 1, 1);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.AreEqual(1, revs.Count);
        }

        [TestMethod(), TestCategory("L0")]
        public void TfsRevisionManagerToolSync10()
        {
            var processorEnricher = GetTfsRevisionManagerTool();

            var currentDateTime = DateTime.Now;
            List<RevisionItem> source = GetWorkItemWithRevisions(currentDateTime, 1, 11);
            List<RevisionItem> target = GetWorkItemWithRevisions(currentDateTime, 1, 1);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.AreEqual(10, revs.Count);
        }

        [TestMethod(), TestCategory("L0")]
        public void TfsRevisionManagerToolReplayRevisionsOff()
        {
            var processorEnricher = GetTfsRevisionManagerTool(new TfsRevisionManagerToolOptions()
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
        public void TfsRevisionManagerToolMaxRevision51()
        {
            var processorEnricher = GetTfsRevisionManagerTool(new TfsRevisionManagerToolOptions()
            {
                Enabled = true,
                MaxRevisions = 5,
                ReplayRevisions = true,
            });

            var currentDateTime = DateTime.Now;
            List<RevisionItem> source = GetWorkItemWithRevisions(currentDateTime, 1, 2);
            List<RevisionItem> target = GetWorkItemWithRevisions(currentDateTime, 2, 2);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.IsEmpty(revs);
        }

        [TestMethod(), TestCategory("L0")]
        public void TfsRevisionManagerToolMaxRevision56()
        {
            var processorEnricher = GetTfsRevisionManagerTool(new TfsRevisionManagerToolOptions()
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
        public void TfsRevisionManagerToolMaxRevision59()
        {
            var processorEnricher = GetTfsRevisionManagerTool(new TfsRevisionManagerToolOptions()
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
        public void TfsRevisionManagerToolDatesMustBeIncreasing()
        {
            var processorEnricher = GetTfsRevisionManagerTool();

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

        private static TfsRevisionManagerTool GetTfsRevisionManagerTool()
        {
            return GetTfsRevisionManagerTool(new TfsRevisionManagerToolOptions() { Enabled = true, MaxRevisions = 0, ReplayRevisions = true });
        }

        private static TfsRevisionManagerTool GetTfsRevisionManagerTool(TfsRevisionManagerToolOptions options)
        {

            var sp = ServiceProviderHelper.GetMigrationToolServicesForUnitTests();
            sp.AddSingleton<TfsRevisionManagerTool>();
            sp.Configure<TfsRevisionManagerToolOptions>(o =>
            {
                o.Enabled = options.Enabled;
                o.MaxRevisions = options.MaxRevisions;
                o.ReplayRevisions = options.ReplayRevisions;
            });
            return sp.BuildServiceProvider().GetService<TfsRevisionManagerTool>();
        }
    }
}
