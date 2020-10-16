using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;

namespace MigrationTools.Clients.FileSystem.Endpoints.Tests
{
    [TestClass()]
    public class WorkItemEndpointTests
    {
        [TestInitialize]
        public void Setup()
        {
            //System.IO.Directory.Delete(@".\Store\");
            WorkItemEndpoint e = new WorkItemEndpoint();
            WorkItemQuery query = new WorkItemQuery();
            query.Configure(null, @".\Store\", null);
            e.Configure(query);
            for (int i = 0; i < 10; i++)
            {
                e.PersistWorkItem(new WorkItemData() { Id = i.ToString(), Title = string.Format("Title {0}", i) });
            }
        }

        [TestMethod]
        public void EmptyTest()
        {
            WorkItemEndpoint e = new WorkItemEndpoint();
            Assert.AreEqual(0, e.Count);
        }

        [TestMethod]
        public void ConfiguredTest()
        {
            WorkItemEndpoint e = new WorkItemEndpoint();
            WorkItemQuery query = new WorkItemQuery();
            query.Configure(null, @".\Store\", null);
            e.Configure(query);
            Assert.AreEqual(10, e.Count);
        }

        [TestMethod]
        public void FilterAllTest()
        {
            WorkItemEndpoint e1 = new WorkItemEndpoint();
            WorkItemQuery query = new WorkItemQuery();
            query.Configure(null, @".\Store\", null);
            e1.Configure(query);
            WorkItemEndpoint e2 = new WorkItemEndpoint();
            e2.Configure(query);
            e1.Filter(e2.WorkItems);
            Assert.AreEqual(0, e1.Count);
        }

        [TestMethod]
        public void FilterHalfTest()
        {
            WorkItemEndpoint e1 = new WorkItemEndpoint();
            WorkItemQuery query = new WorkItemQuery();
            query.Configure(null, @".\Store\", null);
            e1.Configure(query);
            WorkItemEndpoint e2 = new WorkItemEndpoint();
            query.Configure(null, @".\Store\", null);
            e2.Configure(query);
            e1.Filter(e2.WorkItems);
            Assert.AreEqual(0, e1.Count);
        }

        [TestMethod]
        public void PersistWorkItemWithFilterTest()
        {
            WorkItemEndpoint e1 = new WorkItemEndpoint();
            WorkItemQuery query = new WorkItemQuery();
            query.Configure(null, "20", null);
            e1.Configure(query);
            WorkItemEndpoint e2 = new WorkItemEndpoint();
            query.Configure(null, "10", null);
            e2.Configure(query);
            e1.Filter(e2.WorkItems);
            Assert.AreEqual(0, e1.Count);
            foreach (WorkItemData item in e1.WorkItems)
            {
                e2.PersistWorkItem(item);
            }
            Assert.AreEqual(0, e2.Count);
        }

        [TestMethod]
        public void PersistWorkItemExistsTest()
        {
            WorkItemEndpoint e1 = new WorkItemEndpoint();
            WorkItemQuery query = new WorkItemQuery();
            query.Configure(null, "20", null);
            e1.Configure(query);
            WorkItemEndpoint e2 = new WorkItemEndpoint();
            query.Configure(null, "10", null);
            e2.Configure(query);
            foreach (WorkItemData item in e1.WorkItems)
            {
                e2.PersistWorkItem(item);
            }
            Assert.AreEqual(0, e2.Count);
        }
    }
}