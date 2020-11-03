using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Tests;

namespace MigrationTools.Endpoints.Tests
{
    [TestClass()]
    public class InMemoryWorkItemEndpointTests
    {
        public ServiceProvider Services { get; private set; }

        [TestInitialize]
        public void Setup()
        {
            Services = ServiceProviderHelper.GetWorkItemMigrationProcessor();
        }

        [TestMethod]
        public void ConfiguredTest()
        {
            InMemoryWorkItemEndpoint e = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Source, "10");
            Assert.AreEqual(10, e.Count);
        }

        [TestMethod()]
        public void EmptyTest()
        {
            var targetOptions = new InMemoryWorkItemEndpointOptions() { Direction = EndpointDirection.Source };
            InMemoryWorkItemEndpoint e = Services.GetRequiredService<InMemoryWorkItemEndpoint>();
            e.Configure(targetOptions);
            Assert.AreEqual(0, e.Count);
        }

        [TestMethod]
        public void FilterAllTest()
        {
            InMemoryWorkItemEndpoint e1 = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Source, "10");
            InMemoryWorkItemEndpoint e2 = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Target, "10");
            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(0, e1.Count);
        }

        [TestMethod]
        public void FilterHalfTest()
        {
            InMemoryWorkItemEndpoint e1 = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Source, "20");
            InMemoryWorkItemEndpoint e2 = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Target, "10");

            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(10, e1.Count);
        }

        [TestMethod]
        public void PersistWorkItemExistsTest()
        {
            InMemoryWorkItemEndpoint e1 = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Source, "20");
            InMemoryWorkItemEndpoint e2 = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Target, "10");
            foreach (WorkItemData2 item in e1.GetWorkItems())
            {
                e2.PersistWorkItem(item);
            }
            Assert.AreEqual(20, e2.Count);
        }

        [TestMethod]
        public void PersistWorkItemWithFilterTest()
        {
            InMemoryWorkItemEndpoint e1 = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Source, "20");
            InMemoryWorkItemEndpoint e2 = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Target, "10");
            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(10, e1.Count);
            foreach (WorkItemData2 item in e1.GetWorkItems())
            {
                e2.PersistWorkItem(item);
            }
            Assert.AreEqual(20, e2.Count);
        }

        private InMemoryWorkItemEndpoint CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection direction, string queryString)
        {
            InMemoryWorkItemEndpoint e = CreateInMemoryWorkItemEndpoint(EndpointDirection.Source);
            InMemoryWorkItemQuery query = new InMemoryWorkItemQuery();
            query.Configure(null, queryString, null);
            //e.Configure(query, null);
            return e;
        }

        private InMemoryWorkItemEndpoint CreateInMemoryWorkItemEndpoint(EndpointDirection direction)
        {
            var options = new InMemoryWorkItemEndpointOptions() { Direction = direction };
            InMemoryWorkItemEndpoint e = Services.GetRequiredService<InMemoryWorkItemEndpoint>();
            e.Configure(options);
            return e;
        }
    }
}