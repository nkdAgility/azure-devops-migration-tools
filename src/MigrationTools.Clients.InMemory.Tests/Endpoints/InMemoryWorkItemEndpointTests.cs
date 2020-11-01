using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;

namespace MigrationTools.Endpoints.Tests
{
    [TestClass()]
    public class InMemoryWorkItemEndpointTests
    {
        [TestMethod]
        public void ConfiguredTest()
        {
            InMemoryWorkItemEndpoint e = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Source, "10");
            Assert.AreEqual(10, e.Count);
        }

        [TestMethod()]
        public void EmptyTest()
        {
            var targetOptions = Microsoft.Extensions.Options.Options.Create<InMemoryWorkItemEndpointOptions>(new InMemoryWorkItemEndpointOptions() { Direction = EndpointDirection.Source });
            InMemoryWorkItemEndpoint e = new InMemoryWorkItemEndpoint(targetOptions);
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

        private static InMemoryWorkItemEndpoint CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection direction, string queryString)
        {
            InMemoryWorkItemEndpoint e = CreateInMemoryWorkItemEndpoint(EndpointDirection.Source);
            InMemoryWorkItemQuery query = new InMemoryWorkItemQuery();
            query.Configure(null, queryString, null);
            e.Configure(query, null);
            return e;
        }

        private static InMemoryWorkItemEndpoint CreateInMemoryWorkItemEndpoint(EndpointDirection direction)
        {
            var options = Microsoft.Extensions.Options.Options.Create<InMemoryWorkItemEndpointOptions>(new InMemoryWorkItemEndpointOptions() { Direction = direction });
            InMemoryWorkItemEndpoint e = new InMemoryWorkItemEndpoint(options);
            return e;
        }
    }
}