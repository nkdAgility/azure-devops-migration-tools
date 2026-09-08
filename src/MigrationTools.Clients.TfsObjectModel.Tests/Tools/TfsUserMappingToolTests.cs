using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Tools;
using Moq;
using Newtonsoft.Json;

namespace MigrationTools.Tests.Tools
{
    [TestClass]
    public class TfsUserMappingToolTests
    {
        private ILogger<TfsUserMappingTool> _logger;
        private string _testDirectory;

        [TestInitialize]
        public void Setup()
        {
            _logger = new Mock<ILogger<TfsUserMappingTool>>().Object;
            _testDirectory = Path.Combine(Path.GetTempPath(), $"TfsUserMappingToolTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [TestMethod, TestCategory("L0")]
        public void DeserializeIdentityMapData_ValidFile_ReturnsIdentityMappings()
        {
            // Arrange
            var testData = new List<IdentityMapData>
            {
                new IdentityMapData
                {
                    Source = new IdentityItemData
                    {
                        DisplayName = "Test User",
                        AccountName = "test@example.com",
                        Sid = "S-1-5-21-1234567890-1234567890-1234567890-1001"
                    },
                    Target = new IdentityItemData
                    {
                        DisplayName = "Test User",
                        AccountName = "test@newcompany.com",
                        Sid = "S-1-5-21-9876543210-9876543210-9876543210-1001"
                    }
                },
                new IdentityMapData
                {
                    Source = new IdentityItemData
                    {
                        DisplayName = "Test User B",
                        AccountName = "test.user@example.com",
                        Sid = "S-1-5-21-1234567890-1234567890-1234567890-1701"
                    },
                    Target = new IdentityItemData
                    {
                        DisplayName = "Test User B",
                        AccountName = "test.user.b@newcompany.com",
                        Sid = "S-1-5-21-9876543210-9876543210-9876543210-1901"
                    }
                }
            };

            var filePath = Path.Combine(_testDirectory, "identitymap.json");
            var json = JsonConvert.SerializeObject(testData, Formatting.Indented);
            File.WriteAllText(filePath, json);

            // Act
            var result = TfsUserMappingTool.DeserializeIdentityMapData(filePath, _logger);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(2, result);
            Assert.AreEqual("Test User", result[0].Source.DisplayName);
            Assert.AreEqual("Test User B", result[1].Target.DisplayName);
        }

        [TestMethod, TestCategory("L0")]
        public void DeserializeIdentityMapData_FileNotFound_ReturnsEmptyList()
        {
            // Arrange
            var filePath = Path.Combine(_testDirectory, "nonexistent.json");

            // Act
            var result = TfsUserMappingTool.DeserializeIdentityMapData(filePath, _logger);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [TestMethod, TestCategory("L0")]
        public void DeserializeIdentityMapData_InvalidJson_ReturnsEmptyList()
        {
            // Arrange
            var filePath = Path.Combine(_testDirectory, "invalid.json");
            File.WriteAllText(filePath, "{ invalid json }");

            // Act
            var result = TfsUserMappingTool.DeserializeIdentityMapData(filePath, _logger);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [TestMethod, TestCategory("L0")]
        public void SerializeIdentityMapData_ValidList_WritesCorrectJson()
        {
            // Arrange
            var testData = new List<IdentityMapData>
            {
                new IdentityMapData
                {
                    Source = new IdentityItemData
                    {
                        DisplayName = "Test User",
                        AccountName = "test@example.com",
                        Sid = "S-1-5-21-1234567890-1234567890-1234567890-1001"
                    },
                    Target = new IdentityItemData
                    {
                        DisplayName = "Test User",
                        AccountName = "test@newcompany.com",
                        Sid = "S-1-5-21-9876543210-9876543210-9876543210-1001"
                    }
                }
            };

            var filePath = Path.Combine(_testDirectory, "output.json");

            // Act
            TfsUserMappingTool.SerializeIdentityMapData(filePath, testData, _logger);

            // Assert
            Assert.IsTrue(File.Exists(filePath));
            var json = File.ReadAllText(filePath);
            var deserialized = JsonConvert.DeserializeObject<List<IdentityMapData>>(json);
            Assert.HasCount(1, deserialized);
            Assert.AreEqual("Test User", deserialized[0].Source.DisplayName);
            Assert.AreEqual("Test User", deserialized[0].Target.DisplayName);
        }

        [TestMethod, TestCategory("L0")]
        public void DeserializeUserMap_ValidDictionary_ReturnsMapping()
        {
            // Arrange
            var testData = new Dictionary<string, string>
            {
                { "Source User", "Target User" },
                { "Another Source", "Another Target" }
            };

            var filePath = Path.Combine(_testDirectory, "usermap.json");
            var json = JsonConvert.SerializeObject(testData, Formatting.Indented);
            File.WriteAllText(filePath, json);

            // Act
            var result = TfsUserMappingTool.DeserializeUserMap(filePath, _logger);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(2, result);
            Assert.IsTrue(result.ContainsKey("Source User"));
            Assert.AreEqual("Target User", result["Source User"]);
        }

        [TestMethod, TestCategory("L0")]
        public void DeserializeUserMap_FileNotFound_ReturnsEmptyDictionary()
        {
            // Arrange
            var filePath = Path.Combine(_testDirectory, "nonexistent.json");

            // Act
            var result = TfsUserMappingTool.DeserializeUserMap(filePath, _logger);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [TestMethod, TestCategory("L0")]
        public void SerializeUserMap_ValidDictionary_WritesCorrectJson()
        {
            // Arrange
            var testData = new Dictionary<string, string>
            {
                { "Source User", "Target User" }
            };

            var filePath = Path.Combine(_testDirectory, "usermap_output.json");

            // Act
            TfsUserMappingTool.SerializeUserMap(filePath, testData, _logger);

            // Assert
            Assert.IsTrue(File.Exists(filePath));
            var json = File.ReadAllText(filePath);
            var deserialized = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            Assert.HasCount(1, deserialized);
            Assert.AreEqual("Target User", deserialized["Source User"]);
        }

        [TestMethod, TestCategory("L0")]
        public void DeserializeIdentityMapData_NullContent_ReturnsEmptyList()
        {
            // Arrange
            var filePath = Path.Combine(_testDirectory, "null.json");
            File.WriteAllText(filePath, "null");

            // Act
            var result = TfsUserMappingTool.DeserializeIdentityMapData(filePath, _logger);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [TestMethod, TestCategory("L0")]
        public void DeserializeIdentityMapData_EmptyList_ReturnsEmptyList()
        {
            // Arrange
            var filePath = Path.Combine(_testDirectory, "empty.json");
            File.WriteAllText(filePath, "[]");

            // Act
            var result = TfsUserMappingTool.DeserializeIdentityMapData(filePath, _logger);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }
    }
}
