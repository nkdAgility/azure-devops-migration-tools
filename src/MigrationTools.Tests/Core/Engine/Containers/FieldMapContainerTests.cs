using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;

namespace MigrationTools.Engine.Containers.Tests
{
    [TestClass()]
    public class FieldMapContainerTests
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
            sc.AddTransient<SimpleFieldMapMock>();
            IServiceProvider sp = sc.BuildServiceProvider();
            return sp;
        }

        [TestMethod(), TestCategory("L0")]
        public void FieldMapContainerTest()
        {
            var config = CreateEngineConfiguration();

            Assert.AreEqual(0, config.FieldMaps.Count);

            var testSimple = new SimpleFieldMapConfigMock
            {
                WorkItemTypeName = "*"
            };
            config.FieldMaps.Add(testSimple);

            Assert.AreEqual(1, config.FieldMaps.Count);

            var fieldMapContainer = new FieldMapContainer(CreateServiceProvider(), config);
            fieldMapContainer.EnsureConfigured();
            Assert.AreEqual(1, fieldMapContainer.Count);
        }
    }
}