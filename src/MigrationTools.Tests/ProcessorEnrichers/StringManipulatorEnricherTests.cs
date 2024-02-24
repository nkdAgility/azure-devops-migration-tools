using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers;
using MigrationTools.Processors;
using MigrationTools.Tests;

namespace MigrationTools.ProcessorEnrichers.Tests
{
    [TestClass()]
    public class StringManipulatorEnricherTests
    {
        private ServiceProvider Services;

        [TestInitialize]
        public void Setup()
        {
            Services = ServiceProviderHelper.GetWorkItemMigrationProcessor();
        }

        [TestMethod(), TestCategory("L0"), TestCategory("Generic.Processor")]
        public void StringManipulatorEnricher_ConfigureTest()
        {
            var y = new StringManipulatorEnricherOptions
            {
                Enabled = true,
                MaxStringLength = 10,
                Manipulators = new List<RegexStringManipulator>
                {
                    new RegexStringManipulator
                    {
                        Enabled = true,
                        Pattern = ".*",
                        Replacement = "Test",
                        Description = "Test"
                    }
                }
                
            };
            var x = Services.GetRequiredService<StringManipulatorEnricher>();
            x.Configure(y);
            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L1"), TestCategory("ProcessorEnrichers")]
        public void StringManipulatorEnricher_RegexTest()
        {
            var y = new StringManipulatorEnricherOptions
            {
                Enabled = true,
                MaxStringLength = 10,
                Manipulators = new List<RegexStringManipulator>
                {
                    new RegexStringManipulator
                    {
                        Enabled = true,
                        Pattern = "Test",
                        Replacement = "Test 2",
                        Description = "Test"
                    }
                }

            };
            var x = Services.GetRequiredService<StringManipulatorEnricher>();
            x.Configure(y);

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

        [TestMethod(), TestCategory("L1"), TestCategory("ProcessorEnrichers")]
        public void StringManipulatorEnricher_LengthShorterThanMaxTest()
        {
            var y = new StringManipulatorEnricherOptions
            {
                Enabled = true,
                MaxStringLength = 10,
            };
            var x = Services.GetRequiredService<StringManipulatorEnricher>();
            x.Configure(y);

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

        [TestMethod(), TestCategory("L1"), TestCategory("ProcessorEnrichers")]
        public void StringManipulatorEnricher_LengthLongerThanMaxTest()
        {
            var y = new StringManipulatorEnricherOptions
            {
                Enabled = true,
                MaxStringLength = 10,
            };
            var x = Services.GetRequiredService<StringManipulatorEnricher>();
            x.Configure(y);

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
    }
}