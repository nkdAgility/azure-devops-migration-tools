using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;

namespace MigrationTools.Clients.FileSystem.Endpoints.Tests
{
    [TestClass()]
    public class WorkItemEndpointTests
    {
        public void SetupStore(string path, int count)
        {
            if (System.IO.Directory.Exists(path))
            {
                System.IO.Directory.Delete(path, true);
            }
            WorkItemEndpoint e = new WorkItemEndpoint();
            WorkItemQuery query = new WorkItemQuery();
            query.Configure(null, path, null);
            e.Configure(query);
            for (int i = 0; i < count; i++)
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
            SetupStore(@".\Store\Source\", 10);
            WorkItemEndpoint e = new WorkItemEndpoint();
            WorkItemQuery query = new WorkItemQuery();
            query.Configure(null, @".\Store\Source\", null);
            e.Configure(query);
            Assert.AreEqual(10, e.Count);
        }

        [TestMethod]
        public void FilterAllTest()
        {
            SetupStore(@".\Store\Source\", 10);
            SetupStore(@".\Store\target\", 10);
            WorkItemEndpoint e1 = new WorkItemEndpoint();
            WorkItemQuery query = new WorkItemQuery();
            query.Configure(null, @".\Store\Source\", null);
            e1.Configure(query);
            WorkItemEndpoint e2 = new WorkItemEndpoint();
            query.Configure(null, @".\Store\Target\", null);
            e2.Configure(query);
            e1.Filter(e2.WorkItems);
            Assert.AreEqual(0, e1.Count);
        }

        [TestMethod]
        public void FilterHalfTest()
        {
            SetupStore(@".\Store\Source\", 20);
            SetupStore(@".\Store\target\", 10);
            WorkItemEndpoint e1 = new WorkItemEndpoint();
            WorkItemQuery query = new WorkItemQuery();
            query.Configure(null, @".\Store\Source\", null);
            e1.Configure(query);
            WorkItemEndpoint e2 = new WorkItemEndpoint();
            query.Configure(null, @".\Store\target\", null);
            e2.Configure(query);
            e1.Filter(e2.WorkItems);
            Assert.AreEqual(10, e1.Count);
        }

        [TestMethod]
        public void PersistWorkItemWithFilterTest()
        {
            SetupStore(@".\Store\Source\", 20);
            SetupStore(@".\Store\target\", 10);
            WorkItemEndpoint e1 = new WorkItemEndpoint();
            WorkItemQuery query = new WorkItemQuery();
            query.Configure(null, @".\Store\Source\", null);
            e1.Configure(query);
            WorkItemEndpoint e2 = new WorkItemEndpoint();
            query.Configure(null, @".\Store\target\", null);
            e2.Configure(query);
            e1.Filter(e2.WorkItems);
            Assert.AreEqual(10, e1.Count);
            foreach (WorkItemData item in e1.WorkItems)
            {
                e2.PersistWorkItem(item);
            }
            Assert.AreEqual(20, e2.Count);
        }

        [TestMethod]
        public void PersistWorkItemExistsTest()
        {
            SetupStore(@".\Store\Source\", 20);
            SetupStore(@".\Store\target\", 10);
            WorkItemEndpoint e1 = new WorkItemEndpoint();
            WorkItemQuery query = new WorkItemQuery();
            query.Configure(null, @".\Store\Source\", null);
            e1.Configure(query);
            WorkItemEndpoint e2 = new WorkItemEndpoint();
            query.Configure(null, @".\Store\target\", null);
            e2.Configure(query);
            foreach (WorkItemData item in e1.WorkItems)
            {
                e2.PersistWorkItem(item);
            }
            Assert.AreEqual(20, e2.Count);
        }
    }
}