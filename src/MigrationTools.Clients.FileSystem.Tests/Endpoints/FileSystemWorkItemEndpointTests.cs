using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Enrichers;
using MigrationTools.Shadows;
using MigrationTools.Tools;
using MigrationTools.Tools.Interfaces;
using MigrationTools.Tools.Shadows;

namespace MigrationTools.Endpoints.Tests
{
    [TestClass()]
    public class FileSystemWorkItemEndpointTests
    {

        public IServiceProvider Services { get; private set; }

        [TestInitialize]
        public void Setup()
        {
            Services = GetServices();
        }

        [TestMethod, TestCategory("L3")]
        public void ConfiguredTest()
        {
            FileSystemWorkItemEndpoint e = (FileSystemWorkItemEndpoint)Services.GetKeyedService<IEndpoint>("Source");
            CleanAndAdd(e, 10);
            Assert.AreEqual(10, e.Count);
        }

        [TestMethod, TestCategory("L3")]
        public void EmptyTest()
        {
            FileSystemWorkItemEndpoint e = (FileSystemWorkItemEndpoint)Services.GetKeyedService<IEndpoint>("Source");
            CleanAndAdd(e, 0);
            Assert.AreEqual(0, e.Count);
        }

        [TestMethod, TestCategory("L3")]
        public void FilterAllTest()
        {
            FileSystemWorkItemEndpoint e1 = (FileSystemWorkItemEndpoint)Services.GetKeyedService<IEndpoint>("Source");
            CleanAndAdd(e1, 10);
            FileSystemWorkItemEndpoint e2 = (FileSystemWorkItemEndpoint)Services.GetKeyedService<IEndpoint>("Target");
            CleanAndAdd(e2, 10);

            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(0, e1.Count);
        }

        [TestMethod, TestCategory("L3")]
        public void FilterHalfTest()
        {
            FileSystemWorkItemEndpoint e1 = (FileSystemWorkItemEndpoint)Services.GetKeyedService<IEndpoint>("Source");
            CleanAndAdd(e1, 20);
            FileSystemWorkItemEndpoint e2 = (FileSystemWorkItemEndpoint)Services.GetKeyedService<IEndpoint>("Target");
            CleanAndAdd(e2, 10);
            e1.Filter(e2.GetWorkItems());
            Assert.AreEqual(10, e1.Count);
        }

        [TestMethod, TestCategory("L3")]
        public void PersistWorkItemExistsTest()
        {
            FileSystemWorkItemEndpoint e1 = (FileSystemWorkItemEndpoint)Services.GetKeyedService<IEndpoint>("Source");
            CleanAndAdd(e1, 20);
            FileSystemWorkItemEndpoint e2 = (FileSystemWorkItemEndpoint)Services.GetKeyedService<IEndpoint>("Target");
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
            FileSystemWorkItemEndpoint e1 = (FileSystemWorkItemEndpoint)Services.GetKeyedService<IEndpoint>("Source");
            CleanAndAdd(e1, 20);
            FileSystemWorkItemEndpoint e2 = (FileSystemWorkItemEndpoint)Services.GetKeyedService<IEndpoint>("Target");
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

        protected IServiceProvider GetServices()
        {
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();

            services.AddSingleton<ProcessorEnricherContainer>();
            services.AddSingleton<EndpointEnricherContainer>();
            services.AddSingleton<CommonTools>();
            services.AddSingleton<IFieldMappingTool, MockFieldMappingTool>();
            services.AddSingleton<IWorkItemTypeMappingTool, MockWorkItemTypeMappingTool>();
            services.AddSingleton<IStringManipulatorTool, StringManipulatorTool>();

            services.AddKeyedSingleton(typeof(IEndpoint), "Source", (sp, key) =>
            {
                IOptions<FileSystemWorkItemEndpointOptions> options = Microsoft.Extensions.Options.Options.Create(new FileSystemWorkItemEndpointOptions()
                {
                    FileStore = @".\Store\Source\"
                });
                var endpoint = ActivatorUtilities.CreateInstance(sp, typeof(FileSystemWorkItemEndpoint), options);
                return endpoint;
            });

            services.AddKeyedSingleton(typeof(IEndpoint), "Target", (sp, key) =>
            {
                IOptions<FileSystemWorkItemEndpointOptions> options = Microsoft.Extensions.Options.Options.Create(new FileSystemWorkItemEndpointOptions()
                {
                    FileStore = @".\Store\Target\"
                });
                var endpoint = ActivatorUtilities.CreateInstance(sp, typeof(FileSystemWorkItemEndpoint), options);
                return endpoint;
            });


            return services.BuildServiceProvider();
        }

    }
}
