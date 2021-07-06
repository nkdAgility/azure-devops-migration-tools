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
                Enabled = true

            };
            return migrationConfig;
        }

        [TestMethod(), TestCategory("L0"), TestCategory("AzureDevOps.ObjectModel")]
        public void TestBasicWorkItem()
        {
            var peOptions = GetTfsRevisionManagerOptions();
            var processorEnricher = Services.GetRequiredService<TfsRevisionManager>();
            processorEnricher.Configure(peOptions);

            var source = new WorkItemData();
            source.Id = "1";
            source.Revisions = new System.Collections.Generic.SortedDictionary<int, RevisionItem>();
            source.Revisions.Add(1, new RevisionItem() { Index = 1, Number = 1, ChangedDate = System.DateTime.Now });


            var revs = processorEnricher.GetRevisionsToMigrate(source, source);

            Assert.AreEqual(revs.Count, 0);

        }

    }
}
