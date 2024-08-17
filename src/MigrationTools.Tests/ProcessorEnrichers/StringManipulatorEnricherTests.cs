using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.Processors;
using MigrationTools.Tests;
using MigrationTools.TestExtensions;
using Microsoft.Extensions.Options;
using MigrationTools.Tools;

namespace MigrationTools.ProcessorEnrichers.Tests
{
    [TestClass()]
    public class StringManipulatorToolTests
    {

        [TestMethod(), TestCategory("L1")]
        public void StringManipulatorTool_ConfigureTest()
        {
            var options = new StringManipulatorToolOptions();
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

            var x = GetStringManipulatorTool(options);

            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L1")]
        public void StringManipulatorTool_RegexTest()
        {
           var options = new StringManipulatorToolOptions();
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

            var x = GetStringManipulatorTool(options);

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
        public void StringManipulatorTool_LengthShorterThanMaxTest()
        {
            var options = new StringManipulatorToolOptions();
            options.Enabled = true;
                options.MaxStringLength = 10;
            var x = GetStringManipulatorTool(options);

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
        public void StringManipulatorTool_LengthLongerThanMaxTest()
        {
            var options = new StringManipulatorToolOptions();
            options.Enabled = true;
            options.MaxStringLength = 10;
            var x = GetStringManipulatorTool(options);

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

        private static StringManipulatorTool GetStringManipulatorTool()
        {
            var options = new StringManipulatorToolOptions();
            options.SetDefaults();
           
            return GetStringManipulatorTool(options);
        }

        private static StringManipulatorTool GetStringManipulatorTool(StringManipulatorToolOptions options)
        {
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();
            services.AddSingleton<StringManipulatorTool>();
            services.Configure<StringManipulatorToolOptions>(o =>
            {
                o.Enabled = options.Enabled;
                o.MaxStringLength = options.MaxStringLength;
                o.Manipulators = options.Manipulators;
            });
            return services.BuildServiceProvider().GetService<StringManipulatorTool>();
        }
    }
}