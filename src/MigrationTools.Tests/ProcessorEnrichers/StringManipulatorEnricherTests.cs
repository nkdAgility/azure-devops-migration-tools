using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers;
using MigrationTools.Processors;
using MigrationTools.Tests;
using MigrationTools.TestExtensions;
using Microsoft.Extensions.Options;

namespace MigrationTools.ProcessorEnrichers.Tests
{
    [TestClass()]
    public class StringManipulatorEnricherTests
    {

        [TestMethod(), TestCategory("L1")]
        public void StringManipulatorEnricher_ConfigureTest()
        {
            var options = new StringManipulatorEnricherOptions();
            options.Enabled = true;
            options.MaxStringLength = 10;
                options.Manipulators = new List<RegexStringManipulator>
                {
                    new RegexStringManipulator
                    {
                        Enabled = true,
                        Pattern = ".*",
                        Replacement = "Test",
                        Description = "Test"
                    }
                };

            var x = GetStringManipulatorEnricher(options);

            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L1")]
        public void StringManipulatorEnricher_RegexTest()
        {
           var options = new StringManipulatorEnricherOptions();
            options.Enabled = true;
                    options.MaxStringLength = 10;
                    options.Manipulators = new List<RegexStringManipulator>
                {
                    new RegexStringManipulator
                    {
                        Enabled = true,
                        Pattern = "Test",
                        Replacement = "Test 2",
                        Description = "Test"
                    }
                };

            var x = GetStringManipulatorEnricher(options);

            var fieldItem = new FieldItem
            {
                FieldType = "String",
                internalObject = null,
                ReferenceName = "Custom.Test",
                Name = "Test",
                Value = "Test"
            };

            x.ProcessorExecutionWithFieldItem(null, fieldItem);

            Assert.AreEqual("Test 2", fieldItem.Value);
        }

        [TestMethod(), TestCategory("L1")]
        public void StringManipulatorEnricher_LengthShorterThanMaxTest()
        {
            var options = new StringManipulatorEnricherOptions();
            options.Enabled = true;
                options.MaxStringLength = 10;
            var x = GetStringManipulatorEnricher(options);

            var fieldItem = new FieldItem
            {
                FieldType = "String",
                internalObject = null,
                ReferenceName = "Custom.Test",
                Name = "Test",
                Value = "Test"
            };

            x.ProcessorExecutionWithFieldItem(null, fieldItem);

            Assert.AreEqual(4, fieldItem.Value.ToString().Length);
        }

        [TestMethod(), TestCategory("L1")]
        public void StringManipulatorEnricher_LengthLongerThanMaxTest()
        {
            var options = new StringManipulatorEnricherOptions();
            options.Enabled = true;
            options.MaxStringLength = 10;
            var x = GetStringManipulatorEnricher(options);

            var fieldItem = new FieldItem
            {
                FieldType = "String",
                internalObject = null,
                ReferenceName = "Custom.Test",
                Name = "Test",
                Value = "Test Test Test Test Test Test Test Test Test Test Test Test Test"
            };

            x.ProcessorExecutionWithFieldItem(null, fieldItem);

            Assert.AreEqual(10, fieldItem.Value.ToString().Length);
        }

        private static StringManipulatorEnricher GetStringManipulatorEnricher()
        {
            var options = new StringManipulatorEnricherOptions();
            options.SetDefaults();
           
            return GetStringManipulatorEnricher(options);
        }

        private static StringManipulatorEnricher GetStringManipulatorEnricher(StringManipulatorEnricherOptions options)
        {
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();
            services.AddSingleton<StringManipulatorEnricher>();
            services.Configure<StringManipulatorEnricherOptions>(o =>
            {
                o.Enabled = options.Enabled;
                o.MaxStringLength = options.MaxStringLength;
                o.Manipulators = options.Manipulators;
            });
            return services.BuildServiceProvider().GetService<StringManipulatorEnricher>();
        }
    }
}