using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Shadows;
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

            string value = "Test";
            string? newValue = x.ProcessString(value);

            Assert.AreEqual("Test 2", newValue);
        }

        [TestMethod(), TestCategory("L1")]
        public void StringManipulatorTool_LengthShorterThanMaxTest()
        {
            var options = new StringManipulatorToolOptions();
            options.Enabled = true;
            options.MaxStringLength = 10;
            var x = GetStringManipulatorTool(options);

            string value = "Test";
            string? newValue = x.ProcessString(value);

            Assert.AreEqual(4, newValue.Length);
        }

        [TestMethod(), TestCategory("L1")]
        public void StringManipulatorTool_LengthLongerThanMaxTest()
        {
            var options = new StringManipulatorToolOptions();
            options.Enabled = true;
            options.MaxStringLength = 10;
            var x = GetStringManipulatorTool(options);

            string value = "Test Test Test Test Test Test Test Test Test Test Test Test Test";
            string? newValue = x.ProcessString(value);

            Assert.AreEqual(10, newValue.Length);
        }

        [DataTestMethod(), TestCategory("L1")]
        [DataRow(null, null)]
        [DataRow("", "")]
        [DataRow("lorem", "lorem")]
        public void StringManipulatorTool_Disabled(string? value, string? expected)
        {
            var options = new StringManipulatorToolOptions();
            options.Enabled = false;
            options.MaxStringLength = 15;
            options.Manipulators = new List<RegexStringManipulator>
                {
                    new RegexStringManipulator
                    {
                        Enabled = true,
                        Pattern = "(^.*$)",
                        Replacement = "$1 $1",
                        Description = "Test"
                    }
                };
            var x = GetStringManipulatorTool(options);

            string? newValue = x.ProcessString(value);
            Assert.AreEqual(expected, newValue);
        }

        [DataTestMethod(), TestCategory("L1")]
        [DataRow(null, null)]
        [DataRow("", " ")]
        [DataRow("lorem", "lorem lorem")]
        [DataRow("lorem ipsum", "lorem ipsum lor")]
        public void StringManipulatorTool_Process(string? value, string? expected)
        {
            var options = new StringManipulatorToolOptions();
            options.Enabled = true;
            options.MaxStringLength = 15;
            options.Manipulators = new List<RegexStringManipulator>
                {
                    new RegexStringManipulator
                    {
                        Enabled = true,
                        Pattern = "(^.*$)",
                        Replacement = "$1 $1",
                        Description = "Test"
                    }
                };
            var x = GetStringManipulatorTool(options);

            string? newValue = x.ProcessString(value);
            Assert.AreEqual(expected, newValue);
        }

        [DataTestMethod(), TestCategory("L1")]
        [DataRow(null, null)]
        [DataRow("", " 1 2")]
        [DataRow("lorem", "lorem 1 2")]
        public void StringManipulatorTool_MultipleManipulators(string? value, string? expected)
        {
            var options = new StringManipulatorToolOptions();
            options.Enabled = true;
            options.MaxStringLength = 15;
            options.Manipulators = new List<RegexStringManipulator>
                {
                    new RegexStringManipulator
                    {
                        Enabled = true,
                        Pattern = "(^.*$)",
                        Replacement = "$1 1",
                        Description = "Add 1"
                    },
                    new RegexStringManipulator
                    {
                        Enabled = true,
                        Pattern = "(^.*$)",
                        Replacement = "$1 2",
                        Description = "Add 2"
                    }
                };
            var x = GetStringManipulatorTool(options);

            string? newValue = x.ProcessString(value);
            Assert.AreEqual(expected, newValue);
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
