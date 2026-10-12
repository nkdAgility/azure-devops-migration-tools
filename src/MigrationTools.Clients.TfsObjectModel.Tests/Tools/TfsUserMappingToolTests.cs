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

        private static List<IdentityMapData> CreateMappingList() =>
        [
            null,
            new IdentityMapData { Source = new IdentityItemData { DisplayName = "No Target" }, Target = null },
            new IdentityMapData
            {
                Source = new IdentityItemData { DisplayName = "Old Name", AccountName = "DOMAIN\\olduser" },
                Target = new IdentityItemData { DisplayName = "New Name", AccountName = "newuser@example.com" }
            }
        ];

        [TestMethod, TestCategory("L0")]
        [DataRow("Old Name")]
        [DataRow("old name")]
        [DataRow("DOMAIN\\olduser")]
        [DataRow("domain\\OLDUSER")]
        public void FindIdentityMapTarget_MatchesDisplayOrAccountNameIgnoringCase(string sourceValue)
        {
            Assert.AreEqual("New Name", TfsUserMappingTool.FindIdentityMapTarget(CreateMappingList(), sourceValue));
        }

        [TestMethod, TestCategory("L0")]
        [DataRow("Unknown User")]
        [DataRow("No Target")]
        [DataRow("")]
        [DataRow(null)]
        public void FindIdentityMapTarget_NoUsableMapping_ReturnsNull(string sourceValue)
        {
            Assert.IsNull(TfsUserMappingTool.FindIdentityMapTarget(CreateMappingList(), sourceValue));
        }

        [TestMethod, TestCategory("L0")]
        public void ToUserMapDictionary_MapsDisplayAndAccountNamesIgnoringCase()
        {
            var result = TfsUserMappingTool.ToUserMapDictionary(CreateMappingList());

            Assert.HasCount(2, result);
            Assert.AreEqual("New Name", result["old name"]);
            Assert.AreEqual("New Name", result["domain\\olduser"]);
        }

        [TestMethod, TestCategory("L0")]
        public void FindIdentityMapTarget_EmptyTargetDisplayName_UsesAccountName()
        {
            var mappings = new List<IdentityMapData>
            {
                new IdentityMapData
                {
                    Source = new IdentityItemData { DisplayName = "Old Name" },
                    Target = new IdentityItemData { DisplayName = "", AccountName = "newuser@example.com" }
                }
            };

            Assert.AreEqual("newuser@example.com", TfsUserMappingTool.FindIdentityMapTarget(mappings, "Old Name"));
            Assert.AreEqual("newuser@example.com", TfsUserMappingTool.ToUserMapDictionary(mappings)["Old Name"]);
        }

        [TestMethod, TestCategory("L0")]
        public void DeserializeIdentityMapData_StringSourceAndTarget_ReadsAsDisplayNames()
        {
            var filePath = Path.Combine(_testDirectory, "strings.json");
            File.WriteAllText(filePath, "[{\"Source\":\"Old Name\",\"Target\":\"New Name\"}]");

            var result = TfsUserMappingTool.DeserializeIdentityMapData(filePath, _logger);

            Assert.HasCount(1, result);
            Assert.AreEqual("New Name", TfsUserMappingTool.FindIdentityMapTarget(result, "old name"));
        }

        private TfsUserMappingTool CreateTool(string userMappingFile, bool useIdentityMapDataFormat)
        {
            var options = new TfsUserMappingToolOptions
            {
                Enabled = true,
                UserMappingFile = userMappingFile,
                UseIdentityMapDataFormat = useIdentityMapDataFormat
            };
            return new TfsUserMappingTool(
                Microsoft.Extensions.Options.Options.Create(options),
                new Mock<IServiceProvider>().Object,
                _logger,
                new Mock<ITelemetryLogger>().Object);
        }

        private string WriteExportedIdentityMapFile()
        {
            var filePath = Path.Combine(_testDirectory, "exported.json");
            var exported = CreateMappingList();
            exported.Add(new IdentityMapData { Source = new IdentityItemData { DisplayName = "Unmapped User" }, Target = null });
            TfsUserMappingTool.SerializeIdentityMapData(filePath, exported, _logger);
            return filePath;
        }

        [TestMethod, TestCategory("L1")]
        public void ExportedDictionary_WithDuplicateDisplayNames_RoundTripsThroughMappingFile()
        {
            var jsOld = new IdentityItemData { DisplayName = "John Smith", AccountName = "john.smith@old.com" };
            var js2Old = new IdentityItemData { DisplayName = "John Smith", AccountName = "jsmith@old.com" };
            var janeOld = new IdentityItemData { DisplayName = "Jane Doe", AccountName = "jane.doe@old.com" };
            var jsNew = new IdentityItemData { DisplayName = "John Smith", AccountName = "john.smith@new.com" };
            var js2New = new IdentityItemData { DisplayName = "John Smith", AccountName = "jsmith@new.com" };
            var janeNew = new IdentityItemData { DisplayName = "Jane D", AccountName = "jane.doe@new.com" };
            var usersToMap = new List<IdentityMapData>
            {
                new() { Source = jsOld, Target = jsNew },
                new() { Source = js2Old, Target = js2New },
                new() { Source = janeOld, Target = janeNew },
            };
            var filePath = Path.Combine(_testDirectory, "exported-dictionary.json");
            TfsUserMappingTool.SerializeUserMap(filePath,
                MigrationTools.Processors.TfsExportUsersForMappingProcessor.BuildSimpleDictionary(usersToMap, [jsOld, js2Old, janeOld], [jsNew, js2New, janeNew]), _logger);

            var map = CreateTool(filePath, useIdentityMapDataFormat: false).UserMappings.Value;

            // The client object model returns "Display Name <unique name>" only for display names that are not unique.
            Assert.AreEqual("John Smith <john.smith@new.com>", TfsUserIdentityReference.FindMappedTarget(map, "John Smith <john.smith@old.com>"));
            Assert.AreEqual("John Smith <jsmith@new.com>", TfsUserIdentityReference.FindMappedTarget(map, "John Smith <JSmith@old.com>"));
            Assert.AreEqual("Jane D", TfsUserIdentityReference.FindMappedTarget(map, "Jane Doe"));
            Assert.IsNull(TfsUserIdentityReference.FindMappedTarget(map, "John Smith"));
            Assert.IsTrue(TfsUserIdentityReference.IsAmbiguous(map, "John Smith"));
            Assert.IsFalse(TfsUserIdentityReference.IsAmbiguous(map, "Jane Doe"));
        }

        [TestMethod, TestCategory("L1")]
        public void UserMappings_FlagOnWithExportedArrayFile_LoadsDetailedMappings()
        {
            var tool = CreateTool(WriteExportedIdentityMapFile(), useIdentityMapDataFormat: true);

            Assert.IsTrue(tool.DetectedFormatIsIdentityMapData.Value);
            Assert.AreEqual("New Name", tool.UserMappings.Value["old name"]);
            Assert.AreEqual("New Name", tool.UserMappings.Value["DOMAIN\\olduser"]);
            Assert.IsFalse(tool.UserMappings.Value.ContainsKey("Unmapped User"));
        }

        [TestMethod, TestCategory("L1")]
        public void UserMappings_FlagOnWithDictionaryFile_LoadsDictionary()
        {
            var filePath = Path.Combine(_testDirectory, "dictionary.json");
            TfsUserMappingTool.SerializeUserMap(filePath, new Dictionary<string, string> { ["Old Name"] = "New Name" }, _logger);

            var tool = CreateTool(filePath, useIdentityMapDataFormat: true);

            Assert.IsFalse(tool.DetectedFormatIsIdentityMapData.Value);
            Assert.AreEqual("New Name", tool.UserMappings.Value["old name"]);
        }

        [TestMethod, TestCategory("L1")]
        public void UserMappings_FlagOffWithDictionaryFile_KeepsLegacyBehaviour()
        {
            var filePath = Path.Combine(_testDirectory, "dictionary.json");
            TfsUserMappingTool.SerializeUserMap(filePath, new Dictionary<string, string> { ["Old Name"] = "New Name" }, _logger);

            var tool = CreateTool(filePath, useIdentityMapDataFormat: false);

            Assert.IsFalse(tool.DetectedFormatIsIdentityMapData.Value);
            Assert.IsEmpty(tool.IdentityMappings.Value);
            Assert.AreEqual("New Name", tool.UserMappings.Value["Old Name"]);
        }

        [TestMethod, TestCategory("L1")]
        public void UserMappings_FlagOffWithArrayFile_DoesNotLoadDetailedMappings()
        {
            var tool = CreateTool(WriteExportedIdentityMapFile(), useIdentityMapDataFormat: false);

            Assert.IsFalse(tool.DetectedFormatIsIdentityMapData.Value);
            Assert.IsEmpty(tool.UserMappings.Value);
        }

        [TestMethod, TestCategory("L0")]
        public void DeserializeIdentityMapData_InvalidEntry_KeepsValidEntries()
        {
            var filePath = Path.Combine(_testDirectory, "partly-invalid.json");
            File.WriteAllText(filePath, "[{\"Source\":{\"DisplayName\":\"Bad\",\"Sid\":{}},\"Target\":\"X\"},{\"Source\":\"Old Name\",\"Target\":\"New Name\"}]");

            var result = TfsUserMappingTool.DeserializeIdentityMapData(filePath, _logger);

            Assert.HasCount(1, result);
            Assert.AreEqual("New Name", TfsUserMappingTool.FindIdentityMapTarget(result, "Old Name"));
        }

        [TestMethod, TestCategory("L0")]
        public void DeserializeIdentityMapData_GuideFormatWithFriendlyNameAndLowercaseTarget_Maps()
        {
            var filePath = Path.Combine(_testDirectory, "guide.json");
            File.WriteAllText(filePath, "[{\"Source\":{\"FriendlyName\":\"Martin Hinshelwood nkdAgility.com\",\"AccountName\":\"martin@nkdagility.com\"},"
                + "\"target\":{\"FriendlyName\":\"Hinshelwood, Martin\",\"AccountName\":\"martin@othercompany.com\"}}]");

            var result = TfsUserMappingTool.DeserializeIdentityMapData(filePath, _logger);

            Assert.AreEqual("Hinshelwood, Martin", TfsUserMappingTool.FindIdentityMapTarget(result, "Martin Hinshelwood nkdAgility.com"));
            Assert.AreEqual("Hinshelwood, Martin", TfsUserMappingTool.FindIdentityMapTarget(result, "martin@nkdagility.com"));
        }

        [TestMethod, TestCategory("L0")]
        public void IsIdentityMapDataFormat_DetectsArrayAndDictionaryFiles()
        {
            var arrayFile = Path.Combine(_testDirectory, "array.json");
            var dictionaryFile = Path.Combine(_testDirectory, "dictionary.json");
            TfsUserMappingTool.SerializeIdentityMapData(arrayFile, CreateMappingList(), _logger);
            TfsUserMappingTool.SerializeUserMap(dictionaryFile, new Dictionary<string, string> { ["Old Name"] = "New Name" }, _logger);

            Assert.IsTrue(TfsUserMappingTool.IsIdentityMapDataFormat(arrayFile, _logger));
            Assert.IsFalse(TfsUserMappingTool.IsIdentityMapDataFormat(dictionaryFile, _logger));
        }
    }
}
