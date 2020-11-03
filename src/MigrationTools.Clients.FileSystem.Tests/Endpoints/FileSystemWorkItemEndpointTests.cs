using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Tests;

namespace MigrationTools.Endpoints.Tests
{
    [TestClass()]
    public class FileSystemWorkItemEndpointTests
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
            SetupStore(EndpointDirection.Source, 10);
            FileSystemWorkItemEndpoint e = CreateInMemoryWorkItemEndpoint(EndpointDirection.Source);
            Assert.AreEqual(10, e.Count);
        }

        [TestMethod]
        public void EmptyTest()
        {
            SetupStore(EndpointDirection.Source, 0);
            FileSystemWorkItemEndpoint e = CreateInMemoryWorkItemEndpoint(EndpointDirection.Source);
            Assert.AreEqual(0, e.Count);
        }

        [TestMethod]
        public void FilterAllTest()
        {
            SetupStore(EndpointDirection.Source, 10);
            FileSystemWorkItemEndpoint e1 = CreateInMemoryWorkItemEndpoint(EndpointDirection.Source);
            SetupStore(EndpointDirection.Target, 10);
            FileSystemWorkItemEndpoint e2 = CreateInMemoryWorkItemEndpoint(EndpointDirection.Target);
            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(0, e1.Count);
        }

        [TestMethod]
        public void FilterHalfTest()
        {
            SetupStore(EndpointDirection.Source, 20);
            FileSystemWorkItemEndpoint e1 = CreateInMemoryWorkItemEndpoint(EndpointDirection.Source);
            SetupStore(EndpointDirection.Target, 10);
            FileSystemWorkItemEndpoint e2 = CreateInMemoryWorkItemEndpoint(EndpointDirection.Target);
            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(10, e1.Count);
        }

        [TestMethod]
        public void PersistWorkItemExistsTest()
        {
            SetupStore(EndpointDirection.Source, 20);
            FileSystemWorkItemEndpoint e1 = CreateInMemoryWorkItemEndpoint(EndpointDirection.Source);
            SetupStore(EndpointDirection.Target, 10);
            FileSystemWorkItemEndpoint e2 = CreateInMemoryWorkItemEndpoint(EndpointDirection.Target);
            foreach (WorkItemData2 item in e1.GetWorkItems())
            {
                e2.PersistWorkItem(item);
            }
            Assert.AreEqual(20, e2.Count);
        }

        [TestMethod]
        public void PersistWorkItemWithFilterTest()
        {
            SetupStore(EndpointDirection.Source, 20);
            FileSystemWorkItemEndpoint e1 = CreateInMemoryWorkItemEndpoint(EndpointDirection.Source);
            SetupStore(EndpointDirection.Target, 10);
            FileSystemWorkItemEndpoint e2 = CreateInMemoryWorkItemEndpoint(EndpointDirection.Target);
            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(10, e1.Count);
            foreach (WorkItemData2 item in e1.GetWorkItems())
            {
                e2.PersistWorkItem(item);
            }
            Assert.AreEqual(20, e2.Count);
        }

        public void SetupStore(EndpointDirection direction, int count)
        {
            string path = string.Format(@".\Store\{0}\", direction.ToString());
            if (System.IO.Directory.Exists(path))
            {
                System.IO.Directory.Delete(path, true);
            }
            FileSystemWorkItemEndpoint e = CreateInMemoryWorkItemEndpoint(EndpointDirection.Source);
            for (int i = 0; i < count; i++)
            {
                e.PersistWorkItem(new WorkItemData2() { Id = i.ToString() });
            }
        }

        private FileSystemWorkItemEndpoint CreateInMemoryWorkItemEndpoint(EndpointDirection direction)
        {
            var options = new FileSystemWorkItemEndpointOptions() { Direction = direction, FileStore = string.Format(@".\Store\{0}\", direction.ToString()) };
            FileSystemWorkItemEndpoint e = Services.GetRequiredService<FileSystemWorkItemEndpoint>();
            e.Configure(options);
            return e;
        }
    }
}