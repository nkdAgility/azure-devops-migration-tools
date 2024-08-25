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
            Services = ServiceProviderHelper.GetServices();
        }

        [TestMethod, TestCategory("L3")]
        public void ConfiguredTest()
        {
            SetupStore("Source", 10);
            FileSystemWorkItemEndpoint e = CreateEndpoint("Source");
            Assert.AreEqual(10, e.Count);
        }

        [TestMethod, TestCategory("L3")]
        public void EmptyTest()
        {
            SetupStore("Source", 0);
            FileSystemWorkItemEndpoint e = CreateEndpoint("Source");
            Assert.AreEqual(0, e.Count);
        }

        [TestMethod, TestCategory("L3")]
        public void FilterAllTest()
        {
            SetupStore("Source", 10);
            FileSystemWorkItemEndpoint e1 = CreateEndpoint("Source");
            SetupStore("Target", 10);
            FileSystemWorkItemEndpoint e2 = CreateEndpoint("Target");
            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(0, e1.Count);
        }

        [TestMethod, TestCategory("L3")]
        public void FilterHalfTest()
        {
            SetupStore("Source", 20);
            FileSystemWorkItemEndpoint e1 = CreateEndpoint("Source");
            SetupStore("Target", 10);
            FileSystemWorkItemEndpoint e2 = CreateEndpoint("Target");
            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(10, e1.Count);
        }

        [TestMethod, TestCategory("L3")]
        public void PersistWorkItemExistsTest()
        {
            SetupStore("Source", 20);
            FileSystemWorkItemEndpoint e1 = CreateEndpoint("Source");
            SetupStore("Target", 10);
            FileSystemWorkItemEndpoint e2 = CreateEndpoint("Target");
            foreach (WorkItemData item in e1.GetWorkItems())
            {
                e2.PersistWorkItem(item);
            }
            Assert.AreEqual(20, e2.Count);
        }

        [TestMethod, TestCategory("L3")]
        public void PersistWorkItemWithFilterTest()
        {
            SetupStore("Source", 20);
            FileSystemWorkItemEndpoint e1 = CreateEndpoint("Source");
            SetupStore("Target", 10);
            FileSystemWorkItemEndpoint e2 = CreateEndpoint("Target");
            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(10, e1.Count);
            foreach (WorkItemData item in e1.GetWorkItems())
            {
                e2.PersistWorkItem(item);
            }
            Assert.AreEqual(20, e2.Count);
        }

        public void SetupStore(string direction, int count)
        {
            string path = string.Format(@".\Store\{0}\", direction.ToString());
            if (System.IO.Directory.Exists(path))
            {
                System.IO.Directory.Delete(path, true);
            }
            FileSystemWorkItemEndpoint e = CreateEndpoint(direction);
            for (int i = 0; i < count; i++)
            {
                e.PersistWorkItem(new WorkItemData() { Id = i.ToString() });
            }
        }

        private FileSystemWorkItemEndpoint CreateEndpoint(string direction)
        {
            var options = new FileSystemWorkItemEndpointOptions() { FileStore = string.Format(@".\Store\{0}\", direction) };
            FileSystemWorkItemEndpoint e = ActivatorUtilities.CreateInstance<FileSystemWorkItemEndpoint>(Services, options);
            return e;
        }
    }
}