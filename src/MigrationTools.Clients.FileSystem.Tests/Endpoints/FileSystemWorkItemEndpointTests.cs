using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            FileSystemWorkItemEndpoint e = CreateEndpoint("Source");
            CleanAndAdd(e, 10);
            Assert.AreEqual(10, e.Count);
        }

        [TestMethod, TestCategory("L3")]
        public void EmptyTest()
        {
            FileSystemWorkItemEndpoint e = CreateEndpoint("Source");
            CleanAndAdd(e, 0);
            Assert.AreEqual(0, e.Count);
        }

        [TestMethod, TestCategory("L3")]
        public void FilterAllTest()
        {
            FileSystemWorkItemEndpoint e1 = CreateEndpoint("Source");
            CleanAndAdd(e1, 10);
            FileSystemWorkItemEndpoint e2 = CreateEndpoint("Target");
            CleanAndAdd(e2, 10);
            
            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(0, e1.Count);
        }

        [TestMethod, TestCategory("L3")]
        public void FilterHalfTest()
        {
            FileSystemWorkItemEndpoint e1 = CreateEndpoint("Source");
            CleanAndAdd(e1, 20);
            FileSystemWorkItemEndpoint e2 = CreateEndpoint("Target");
            CleanAndAdd(e2, 10);            
            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(10, e1.Count);
        }

        [TestMethod, TestCategory("L3")]
        public void PersistWorkItemExistsTest()
        {
            FileSystemWorkItemEndpoint e1 = CreateEndpoint("Source");
            CleanAndAdd(e1, 20);
            FileSystemWorkItemEndpoint e2 = CreateEndpoint("Target");
            CleanAndAdd(e2, 10);
            
            foreach (WorkItemData item in e1.GetWorkItems())
            {
                e2.PersistWorkItem(item);
            }
            Assert.AreEqual(20, e2.Count);
        }

        [TestMethod, TestCategory("L3")]
        public void PersistWorkItemWithFilterTest()
        {
            FileSystemWorkItemEndpoint e1 = CreateEndpoint("Source");
            CleanAndAdd(e1, 20);
            FileSystemWorkItemEndpoint e2 = CreateEndpoint("Target");
            CleanAndAdd(e2, 10);
            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(10, e1.Count);
            foreach (WorkItemData item in e1.GetWorkItems())
            {
                e2.PersistWorkItem(item);
            }
            Assert.AreEqual(20, e2.Count);
        }

        public void CleanAndAdd(FileSystemWorkItemEndpoint endpoint, int count)
        {
            if (System.IO.Directory.Exists(endpoint.Options.FileStore))
            {
                System.IO.Directory.Delete(endpoint.Options.FileStore, true);
            }
            for (int i = 0; i < count; i++)
            {
                endpoint.PersistWorkItem(new WorkItemData() { Id = i.ToString() });
            }
        }

        private FileSystemWorkItemEndpoint CreateEndpoint(string direction)
        {
            var options = new FileSystemWorkItemEndpointOptions() { FileStore = string.Format(@".\Store\{0}\", direction) };
            options.Name = direction;
            IOptions<FileSystemWorkItemEndpointOptions> wrappedOptions =  Microsoft.Extensions.Options.Options.Create(options);
            FileSystemWorkItemEndpoint e = ActivatorUtilities.CreateInstance<FileSystemWorkItemEndpoint>(Services, wrappedOptions);
            return e;
        }
    }
}