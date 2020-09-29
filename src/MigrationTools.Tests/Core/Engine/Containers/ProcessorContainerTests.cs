using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.Engine.Containers;
using System;
using System.Text;

namespace MigrationTools.Core.Engine.Containers.Tests
{
    [TestClass()]
    public class ProcessorContainerTests
    {

        private EngineConfiguration CreateEngineConfiguration()
        {
            var ecb = new EngineConfigurationBuilder(new NullLogger<EngineConfigurationBuilder>());
            var ec = ecb.CreateEmptyConfig();
            return ec;
        }

        private IServiceProvider CreateServiceProvider()
        {
            ServiceCollection sc = new ServiceCollection();
            sc.AddTransient<SimpleProcessorMock>();
            IServiceProvider sp = sc.BuildServiceProvider();
            return sp;
        }

        [TestMethod()]
        public void ProcessorContainerTest()
        {
            var config = CreateEngineConfiguration();
            var testSimple = new SimpleConfigMock();

            Assert.AreEqual(0, config.Processors.Count);

            testSimple.Enabled = true;
            config.Processors.Add(testSimple);

            Assert.AreEqual(1, config.Processors.Count);

            var processorContainer = new ProcessorContainer(CreateServiceProvider(), config);

            Assert.AreEqual(1, processorContainer.Count);
        }

    }
}