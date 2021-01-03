using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;

namespace MigrationTools.Engine.Containers.Tests
{
    [TestClass()]
    public class ProcessorContainerTests
    {
        private IOptions<EngineConfiguration> CreateEngineConfiguration()
        {
            var ecb = new EngineConfigurationBuilder(new NullLogger<EngineConfigurationBuilder>());
            var ec = ecb.CreateEmptyConfig();
            var opts = Microsoft.Extensions.Options.Options.Create(ec);
            return opts;
        }

        private IServiceProvider CreateServiceProvider()
        {
            ServiceCollection sc = new ServiceCollection();
            sc.AddTransient<SimpleProcessorMock>();
            IServiceProvider sp = sc.BuildServiceProvider();
            return sp;
        }

        [TestMethod(), TestCategory("L0")]
        public void ProcessorContainerTest()
        {
            var config = CreateEngineConfiguration();
            var testSimple = new SimpleProcessorConfigMock();

            Assert.AreEqual(0, config.Value.Processors.Count);

            testSimple.Enabled = true;
            config.Value.Processors.Add(testSimple);

            Assert.AreEqual(1, config.Value.Processors.Count);

            var processorContainer = new ProcessorContainer(CreateServiceProvider(), config, new NullLogger<ProcessorContainer>());

            Assert.AreEqual(1, processorContainer.Count);
        }
    }
}