using System;
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
        protected ServiceProvider Services = ServiceProviderHelper.GetServices();

        [TestInitialize]
        public void Setup()
        {
        }

        protected static TfsRevisionManagerOptions GetTfsRevisionManagerOptions()
        {
            var migrationConfig = new TfsRevisionManagerOptions()
            {
                Enabled = true,
                MaxRevisions = 0,
                ReplayRevisions = true

            };
            return migrationConfig;
        }

        private static WorkItemData GetWorkItemWithRevisions(System.DateTime currentDateTime, int startHours = 1, int endHours = 1)
        {
            var fakeWorkItem = new WorkItemData();
            fakeWorkItem.Id = Guid.NewGuid().ToString();
            fakeWorkItem.Revisions = new System.Collections.Generic.SortedDictionary<int, RevisionItem>();
            for (int i = startHours; i < endHours + startHours; i++)
            {
                fakeWorkItem.Revisions.Add(i, new RevisionItem() { Index = i, Number = i, ChangedDate = currentDateTime.AddHours(-i) });
            }

            return fakeWorkItem;
        }

        [TestMethod(), TestCategory("L0"), TestCategory("AzureDevOps.ObjectModel")]
        public void TfsRevisionManagerInSync1()
        {
            var peOptions = GetTfsRevisionManagerOptions();
            var processorEnricher = Services.GetRequiredService<TfsRevisionManager>();
            processorEnricher.Configure(peOptions);

            var currentDateTime = System.DateTime.Now;
            WorkItemData source = GetWorkItemWithRevisions(currentDateTime, 1, 1);
            WorkItemData target = GetWorkItemWithRevisions(currentDateTime, 1, 1);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.AreEqual(revs.Count, 0);

        }

        [TestMethod(), TestCategory("L0"), TestCategory("AzureDevOps.ObjectModel")]
        public void TfsRevisionManagerInSync10()
        {
            var peOptions = GetTfsRevisionManagerOptions();
            var processorEnricher = Services.GetRequiredService<TfsRevisionManager>();
            processorEnricher.Configure(peOptions);

            var currentDateTime = System.DateTime.Now;
            WorkItemData source = GetWorkItemWithRevisions(currentDateTime, 1, 10);
            WorkItemData target = GetWorkItemWithRevisions(currentDateTime, 1, 10);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.AreEqual(revs.Count, 0);

        }

        [TestMethod(), TestCategory("L0"), TestCategory("AzureDevOps.ObjectModel")]
        public void TfsRevisionManagerSync1()
        {
            var peOptions = GetTfsRevisionManagerOptions();
            var processorEnricher = Services.GetRequiredService<TfsRevisionManager>();
            processorEnricher.Configure(peOptions);

            var currentDateTime = System.DateTime.Now;
            WorkItemData source = GetWorkItemWithRevisions(currentDateTime, 1, 2);
            WorkItemData target = GetWorkItemWithRevisions(currentDateTime, 2, 1);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.AreEqual(1, revs.Count);
        }

        [TestMethod(), TestCategory("L0"), TestCategory("AzureDevOps.ObjectModel")]
        public void TfsRevisionManagerSync10()
        {
            var peOptions = GetTfsRevisionManagerOptions();
            var processorEnricher = Services.GetRequiredService<TfsRevisionManager>();
            processorEnricher.Configure(peOptions);

            var currentDateTime = System.DateTime.Now;
            WorkItemData source = GetWorkItemWithRevisions(currentDateTime, 1, 11);
            WorkItemData target = GetWorkItemWithRevisions(currentDateTime, 11, 1);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.AreEqual(10, revs.Count);
        }

        [TestMethod(), TestCategory("L0"), TestCategory("AzureDevOps.ObjectModel")]
        public void TfsRevisionManagerReplayRevisionsOff()
        {
            var peOptions = GetTfsRevisionManagerOptions();
            peOptions.ReplayRevisions = false;
            var processorEnricher = Services.GetRequiredService<TfsRevisionManager>();
            processorEnricher.Configure(peOptions);

            var currentDateTime = System.DateTime.Now;
            WorkItemData source = GetWorkItemWithRevisions(currentDateTime, 1, 4);
            WorkItemData target = GetWorkItemWithRevisions(currentDateTime, 4, 1);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.AreEqual(1, revs.Count);
        }


        [TestMethod(), TestCategory("L0"), TestCategory("AzureDevOps.ObjectModel")]
        public void TfsRevisionManagerMaxRevision51()
        {
            var peOptions = GetTfsRevisionManagerOptions();
            peOptions.MaxRevisions = 5;
            var processorEnricher = Services.GetRequiredService<TfsRevisionManager>();
            processorEnricher.Configure(peOptions);

            var currentDateTime = System.DateTime.Now;
            WorkItemData source = GetWorkItemWithRevisions(currentDateTime, 1, 2);
            WorkItemData target = GetWorkItemWithRevisions(currentDateTime, 2, 1);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.AreEqual(1, revs.Count);
        }

        [TestMethod(), TestCategory("L0"), TestCategory("AzureDevOps.ObjectModel")]
        public void TfsRevisionManagerMaxRevision56()
        {
            var peOptions = GetTfsRevisionManagerOptions();
            peOptions.MaxRevisions = 5;
            var processorEnricher = Services.GetRequiredService<TfsRevisionManager>();
            processorEnricher.Configure(peOptions);

            var currentDateTime = System.DateTime.Now;
            WorkItemData source = GetWorkItemWithRevisions(currentDateTime, 1, 7);
            WorkItemData target = GetWorkItemWithRevisions(currentDateTime, 7, 1);

            var revs = processorEnricher.GetRevisionsToMigrate(source, target);

            Assert.AreEqual(5, revs.Count);
        }

        [TestMethod(), TestCategory("L0"), TestCategory("AzureDevOps.ObjectModel")]
        public void TfsRevisionManagerMaxRevision59()
        {
            var peOptions = GetTfsRevisionManagerOptions();
            peOptions.MaxRevisions = 5;
            var processorEnricher = Services.GetRequiredService<TfsRevisionManager>();
            processorEnricher.Configure(peOptions);

            var currentDateTime = System.DateTime.Now;
            WorkItemData source = GetWorkItemWithRevisions(currentDateTime, 1, 10);

            var revs = processorEnricher.GetRevisionsToMigrate(source, null);

            Assert.AreEqual(5, revs.Count);
        }

    }
}
