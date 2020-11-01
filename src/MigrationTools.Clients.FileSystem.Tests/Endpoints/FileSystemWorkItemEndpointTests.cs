using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;

namespace MigrationTools.Endpoints.Tests
{
    [TestClass()]
    public class FileSystemWorkItemEndpointTests
    {
        [TestMethod]
        public void ConfiguredTest()
        {
            SetupStore(@".\Store\Source\", 10);
            FileSystemWorkItemEndpoint e = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Source, @".\Store\Source\");
            Assert.AreEqual(10, e.Count);
        }

        [TestMethod]
        public void EmptyTest()
        {
            FileSystemWorkItemEndpoint e = CreateInMemoryWorkItemEndpoint(EndpointDirection.Source);
            Assert.AreEqual(0, e.Count);
        }

        [TestMethod]
        public void FilterAllTest()
        {
            SetupStore(@".\Store\Source\", 10);
            SetupStore(@".\Store\target\", 10);
            FileSystemWorkItemEndpoint e1 = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Source, @".\Store\Source\");
            FileSystemWorkItemEndpoint e2 = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Source, @".\Store\target\");
            e1.Filter(e2.WorkItems);
            Assert.AreEqual(0, e1.Count);
        }

        [TestMethod]
        public void FilterHalfTest()
        {
            SetupStore(@".\Store\Source\", 20);
            SetupStore(@".\Store\target\", 10);
            FileSystemWorkItemEndpoint e1 = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Source, @".\Store\Source\");
            FileSystemWorkItemEndpoint e2 = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Source, @".\Store\target\");
            e1.Filter(e2.WorkItems);
            Assert.AreEqual(10, e1.Count);
        }

        [TestMethod]
        public void PersistWorkItemExistsTest()
        {
            SetupStore(@".\Store\Source\", 20);
            SetupStore(@".\Store\target\", 10);
            FileSystemWorkItemEndpoint e1 = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Source, @".\Store\Source\");
            FileSystemWorkItemEndpoint e2 = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Source, @".\Store\target\");
            foreach (WorkItemData2 item in e1.WorkItems)
            {
                e2.PersistWorkItem(item);
            }
            Assert.AreEqual(20, e2.Count);
        }

        [TestMethod]
        public void PersistWorkItemWithFilterTest()
        {
            SetupStore(@".\Store\Source\", 20);
            SetupStore(@".\Store\target\", 10);
            FileSystemWorkItemEndpoint e1 = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Source, @".\Store\Source\");
            FileSystemWorkItemEndpoint e2 = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Source, @".\Store\target\");
            e1.Filter(e2.WorkItems);
            Assert.AreEqual(10, e1.Count);
            foreach (WorkItemData2 item in e1.WorkItems)
            {
                e2.PersistWorkItem(item);
            }
            Assert.AreEqual(20, e2.Count);
        }

        public void SetupStore(string path, int count)
        {
            if (System.IO.Directory.Exists(path))
            {
                System.IO.Directory.Delete(path, true);
            }
            FileSystemWorkItemEndpoint e = CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection.Source, path);
            for (int i = 0; i < count; i++)
            {
                e.PersistWorkItem(new WorkItemData2() { Id = i.ToString() });
            }
        }

        private static FileSystemWorkItemEndpoint CreateAndConfigureInMemoryWorkItemEndpoint(EndpointDirection direction, string queryString)
        {
            FileSystemWorkItemEndpoint e = CreateInMemoryWorkItemEndpoint(EndpointDirection.Source);
            FileSystemWorkItemQuery query = new FileSystemWorkItemQuery();
            query.Configure(null, queryString, null);
            e.Configure(query, null);
            return e;
        }

        private static FileSystemWorkItemEndpoint CreateInMemoryWorkItemEndpoint(EndpointDirection direction)
        {
            var options = new FileSystemWorkItemEndpointOptions() { Direction = direction };
            FileSystemWorkItemEndpoint e = new FileSystemWorkItemEndpoint(options);
            return e;
        }
    }
}