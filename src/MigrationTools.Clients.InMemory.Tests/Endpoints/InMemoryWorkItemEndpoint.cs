using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;

namespace MigrationTools.Endpoints.Tests
{
    [TestClass()]
    public class InMemoryWorkItemEndpointTests
    {
        [TestMethod()]
        public void EmptyTest()
        {
            InMemoryWorkItemEndpoint e = new InMemoryWorkItemEndpoint(new InMemoryWorkItemEndpointOptions() { Direction = EndpointDirection.Source });
            Assert.AreEqual(0, e.Count);
        }

        [TestMethod]
        public void ConfiguredTest()
        {
            InMemoryWorkItemEndpoint e = new InMemoryWorkItemEndpoint(new InMemoryWorkItemEndpointOptions() { Direction = EndpointDirection.Source });
            InMemoryWorkItemQuery query = new InMemoryWorkItemQuery();
            query.Configure(null, "10", null);
            e.Configure(query, null);
            Assert.AreEqual(10, e.Count);
        }

        [TestMethod]
        public void FilterAllTest()
        {
            InMemoryWorkItemEndpoint e1 = new InMemoryWorkItemEndpoint(new InMemoryWorkItemEndpointOptions() { Direction = EndpointDirection.Source });
            InMemoryWorkItemQuery query = new InMemoryWorkItemQuery();
            query.Configure(null, "10", null);
            e1.Configure(query, null);
            InMemoryWorkItemEndpoint e2 = new InMemoryWorkItemEndpoint(new InMemoryWorkItemEndpointOptions() { Direction = EndpointDirection.Target });
            e2.Configure(query, null);
            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(0, e1.Count);
        }

        [TestMethod]
        public void FilterHalfTest()
        {
            InMemoryWorkItemEndpoint e1 = new InMemoryWorkItemEndpoint(new InMemoryWorkItemEndpointOptions() { Direction = EndpointDirection.Source });
            InMemoryWorkItemQuery query = new InMemoryWorkItemQuery();
            query.Configure(null, "20", null);
            e1.Configure(query, null);
            InMemoryWorkItemEndpoint e2 = new InMemoryWorkItemEndpoint(new InMemoryWorkItemEndpointOptions() { Direction = EndpointDirection.Target });
            query.Configure(null, "10", null);
            e2.Configure(query, null);
            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(10, e1.Count);
        }

        [TestMethod]
        public void PersistWorkItemWithFilterTest()
        {
            InMemoryWorkItemEndpoint e1 = new InMemoryWorkItemEndpoint(new InMemoryWorkItemEndpointOptions() { Direction = EndpointDirection.Source });
            InMemoryWorkItemQuery query = new InMemoryWorkItemQuery();
            query.Configure(null, "20", null);
            e1.Configure(query, null);
            InMemoryWorkItemEndpoint e2 = new InMemoryWorkItemEndpoint(new InMemoryWorkItemEndpointOptions() { Direction = EndpointDirection.Target });
            query.Configure(null, "10", null);
            e2.Configure(query, null);
            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(10, e1.Count);
            foreach (WorkItemData item in e1.GetWorkItems())
            {
                e2.PersistWorkItem(item);
            }
            Assert.AreEqual(20, e2.Count);
        }

        [TestMethod]
        public void PersistWorkItemExistsTest()
        {
            InMemoryWorkItemEndpoint e1 = new InMemoryWorkItemEndpoint(new InMemoryWorkItemEndpointOptions() { Direction = EndpointDirection.Source });
            InMemoryWorkItemQuery query = new InMemoryWorkItemQuery();
            query.Configure(null, "20", null);
            e1.Configure(query, null);
            InMemoryWorkItemEndpoint e2 = new InMemoryWorkItemEndpoint(new InMemoryWorkItemEndpointOptions() { Direction = EndpointDirection.Target });
            query.Configure(null, "10", null);
            e2.Configure(query, null);
            foreach (WorkItemData item in e1.GetWorkItems())
            {
                e2.PersistWorkItem(item);
            }
            Assert.AreEqual(20, e2.Count);
        }
    }
}