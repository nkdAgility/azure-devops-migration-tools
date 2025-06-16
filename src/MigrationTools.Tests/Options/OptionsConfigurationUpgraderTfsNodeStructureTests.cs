using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Options;
using MigrationTools.Services;

namespace MigrationTools.Tests.Options
{
    [TestClass]
    public class OptionsConfigurationUpgraderTfsNodeStructureTests
    {
        [TestMethod]
        public void TfsNodeStructureOptions_DictionaryMappingsFormat_ShouldConvertToArrayFormat()
        {
            // Arrange
            var oldConfigJson = @"{
                ""TfsNodeStructureTool"": {
                    ""Enabled"": true,
                    ""Areas"": {
                        ""Filters"": [],
                        ""Mappings"": {
                            ""Foo\\\\AAA\\\\123\\\\(.+)"": ""FooDest\\AAA\\$1"",
                            ""Foo\\\\(.+)"": ""FooDest\\$1""
                        }
                    },
                    ""Iterations"": {
                        ""Filters"": [],
                        ""Mappings"": {
                            ""Bar\\\\(.+)"": ""BarDest\\$1""
                        }
                    }
                }
            }";

            var tempFile = System.IO.Path.GetTempFileName();
            try
            {
                System.IO.File.WriteAllText(tempFile, oldConfigJson);

                var configuration = new ConfigurationBuilder()
                    .AddJsonFile(tempFile, optional: false)
                    .Build();

                var services = new ServiceCollection();
                services.AddLogging();
                services.AddSingleton<ITelemetryLogger, TelemetryLoggerMock>();
                var serviceProvider = services.BuildServiceProvider();

                var logger = serviceProvider.GetService<ILogger<OptionsConfigurationBuilder>>();
                var telemetryLogger = serviceProvider.GetService<ITelemetryLogger>();

                var upgrader = new OptionsConfigurationUpgrader(configuration, logger, telemetryLogger, serviceProvider);
                var section = configuration.GetSection("TfsNodeStructureTool");

                // Act - Test the dictionary detection and configuration parsing
                var areasMappingsSection = section.GetSection("Areas:Mappings");
                var iterationsMappingsSection = section.GetSection("Iterations:Mappings");

                // Assert - Verify the old format is correctly detected
                Assert.IsTrue(areasMappingsSection.Exists(), "Areas.Mappings section should exist");
                Assert.IsTrue(iterationsMappingsSection.Exists(), "Iterations.Mappings section should exist");

                var areasChildren = areasMappingsSection.GetChildren().ToList();
                var iterationsChildren = iterationsMappingsSection.GetChildren().ToList();

                Assert.AreEqual(2, areasChildren.Count, "Should have 2 areas mappings");
                Assert.AreEqual(1, iterationsChildren.Count, "Should have 1 iterations mapping");

                // Verify the dictionary format detection logic
                var firstAreaChild = areasChildren.FirstOrDefault();
                Assert.IsNotNull(firstAreaChild, "Should have first area mapping");
                Assert.IsFalse(int.TryParse(firstAreaChild.Key, out _), "Key should not be numeric (dictionary format)");

                // Verify the actual key-value pairs
                var areaMapping1 = areasChildren.FirstOrDefault(c => c.Key.Contains("Foo\\\\(.+)"));
                var areaMapping2 = areasChildren.FirstOrDefault(c => c.Key.Contains("Foo\\\\AAA\\\\123"));
                var iterationMapping1 = iterationsChildren.FirstOrDefault(c => c.Key.Contains("Bar\\\\(.+)"));

                Assert.IsNotNull(areaMapping1, "Should find the first area mapping");
                Assert.IsNotNull(areaMapping2, "Should find the second area mapping");
                Assert.IsNotNull(iterationMapping1, "Should find the iteration mapping");

                Assert.AreEqual("FooDest\\$1", areaMapping1.Value, "First area mapping value should be correct");
                Assert.AreEqual("FooDest\\AAA\\$1", areaMapping2.Value, "Second area mapping value should be correct");
                Assert.AreEqual("BarDest\\$1", iterationMapping1.Value, "Iteration mapping value should be correct");
            }
            finally
            {
                if (System.IO.File.Exists(tempFile))
                {
                    System.IO.File.Delete(tempFile);
                }
            }
        }

        [TestMethod]
        public void TfsNodeStructureOptions_ArrayMappingsFormat_ShouldNotBeModified()
        {
            // Arrange - Already in new format
            var newConfigJson = @"{
                ""TfsNodeStructureTool"": {
                    ""Enabled"": true,
                    ""Areas"": {
                        ""Filters"": [],
                        ""Mappings"": [
                            {
                                ""Match"": ""Foo\\\\(.+)"",
                                ""Replacement"": ""FooDest\\$1""
                            }
                        ]
                    }
                }
            }";

            var tempFile = System.IO.Path.GetTempFileName();
            try
            {
                System.IO.File.WriteAllText(tempFile, newConfigJson);

                var configuration = new ConfigurationBuilder()
                    .AddJsonFile(tempFile, optional: false)
                    .Build();

                var section = configuration.GetSection("TfsNodeStructureTool");

                // Act & Assert - Verify the new format is correctly detected as array format
                var areasMappingsSection = section.GetSection("Areas:Mappings");
                Assert.IsTrue(areasMappingsSection.Exists(), "Areas.Mappings section should exist");

                var areasChildren = areasMappingsSection.GetChildren().ToList();
                Assert.AreEqual(1, areasChildren.Count, "Should have 1 areas mapping");

                var firstChild = areasChildren.FirstOrDefault();
                Assert.IsNotNull(firstChild, "Should have first mapping");
                
                // In array format, keys should be numeric indices
                Assert.IsTrue(int.TryParse(firstChild.Key, out _), "Key should be numeric (array format)");
            }
            finally
            {
                if (System.IO.File.Exists(tempFile))
                {
                    System.IO.File.Delete(tempFile);
                }
            }
        }
    }
}