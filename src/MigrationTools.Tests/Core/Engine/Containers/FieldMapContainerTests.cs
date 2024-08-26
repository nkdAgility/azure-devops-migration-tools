using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;
using MigrationTools.Tests;
using MigrationTools.Tools;
using MigrationTools.Tools.Shadows;

namespace MigrationTools.Engine.Containers.Tests
{
    [TestClass()]
    public class FieldMapContainerTests
    {
        private IOptions<FieldMappingToolOptions> CreateFieldMappingToolOptions()
        {
           var options = new FieldMappingToolOptions();
            options.Enabled = true;
            var opts = Microsoft.Extensions.Options.Options.Create(options);
            return opts;
        }

        private IServiceProvider CreateServiceProvider()
        {
            return ServiceProviderHelper.GetWorkItemMigrationProcessor();
        }

        [TestMethod(), TestCategory("L0")]
        public void FieldMapContainerTest()
        {
            var config = CreateFieldMappingToolOptions();

            Assert.AreEqual(0, config.Value.FieldMaps.Count);

            var testSimple = new MockSimpleFieldMapOptions
            {
                ApplyTo = new List<string> { "*" },
            };
            config.Value.FieldMaps.Add(testSimple);

            Assert.AreEqual(1, config.Value.FieldMaps.Count);
           var fieldMappTool = ActivatorUtilities.CreateInstance<FieldMappingTool>(CreateServiceProvider(), config, new NullLogger<FieldMappingTool>());
            Assert.AreEqual(1, fieldMappTool.Count);
        }
    }
}