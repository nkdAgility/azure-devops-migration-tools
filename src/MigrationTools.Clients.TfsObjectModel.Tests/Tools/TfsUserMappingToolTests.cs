using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
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
                UseIdentityMapDataFormat = useIdentityMapDataFormat,
                IdentityFieldsToCheck = new List<string> { AssignedTo, "System.ChangedBy" }
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
                MigrationTools.Processors.TfsExportUsersForMappingProcessor.BuildSimpleDictionary(
                    MigrationTools.Processors.TfsExportUsersForMappingProcessor.SelectUsersToMap(
                        new IdentityMapResult { IdentityMap = usersToMap, SourceUsers = [jsOld, js2Old, janeOld], TargetUsers = [jsNew, js2New, janeNew] }, detailed: false),
                    [jsOld, js2Old, janeOld], [jsNew, js2New, janeNew]), _logger);

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

        private const string AssignedTo = "System.AssignedTo";

        private string WriteDictionaryFile()
        {
            var filePath = Path.Combine(_testDirectory, "dict-map.json");
            TfsUserMappingTool.SerializeUserMap(filePath, new Dictionary<string, string> { ["Old Name"] = "New Name" }, _logger);
            return filePath;
        }

        [TestMethod, TestCategory("L1")]
        [DataRow(false)]
        [DataRow(true)]
        public void MapUserIdentityValue_MappingHit_ReturnsTarget(bool identityFormat)
        {
            var file = identityFormat ? WriteExportedIdentityMapFile() : WriteDictionaryFile();
            var tool = CreateTool(file, useIdentityMapDataFormat: identityFormat);

            Assert.AreEqual("New Name", tool.MapUserIdentityValue(AssignedTo, "Old Name"));
            Assert.AreEqual("New Name", tool.MapUserIdentityValue(AssignedTo, "old name"));
            Assert.AreEqual("New Name", tool.MapUserIdentityValue(AssignedTo.ToLowerInvariant(), "Old Name"));
        }

        [TestMethod, TestCategory("L1")]
        [DataRow(false)]
        [DataRow(true)]
        public void MapUserIdentityValue_MappingMiss_ReturnsOriginal(bool identityFormat)
        {
            var file = identityFormat ? WriteExportedIdentityMapFile() : WriteDictionaryFile();
            var tool = CreateTool(file, useIdentityMapDataFormat: identityFormat);

            Assert.AreEqual("Someone Else", tool.MapUserIdentityValue(AssignedTo, "Someone Else"));
            // Unmapped entry (null target) in the exported file must not blank the value.
            Assert.AreEqual("Unmapped User", tool.MapUserIdentityValue(AssignedTo, "Unmapped User"));
        }

        [TestMethod, TestCategory("L1")]
        [DataRow(false, null)]
        [DataRow(false, "")]
        [DataRow(true, null)]
        [DataRow(true, "")]
        public void MapUserIdentityValue_NullOrEmptyValue_ReturnsOriginalWithoutThrowing(bool identityFormat, string value)
        {
            var file = identityFormat ? WriteExportedIdentityMapFile() : WriteDictionaryFile();
            var tool = CreateTool(file, useIdentityMapDataFormat: identityFormat);

            Assert.AreEqual(value, tool.MapUserIdentityValue(AssignedTo, value));
        }

        [TestMethod, TestCategory("L1")]
        public void MapUserIdentityValue_FieldNotConfigured_ReturnsOriginal()
        {
            var tool = CreateTool(WriteDictionaryFile(), useIdentityMapDataFormat: false);

            Assert.AreEqual("Old Name", tool.MapUserIdentityValue("System.Title", "Old Name"));
        }

        [TestMethod, TestCategory("L1")]
        public void MapUserIdentityValue_ToolDisabled_ReturnsOriginal()
        {
            var tool = CreateTool(WriteDictionaryFile(), useIdentityMapDataFormat: false);
            tool.Options.Enabled = false;

            Assert.AreEqual("Old Name", tool.MapUserIdentityValue(AssignedTo, "Old Name"));
        }

        [TestMethod, TestCategory("L1")]
        public void TryMapUserIdentityValue_ReboxedValueTypeOnUnmappedField_ReportsUnchanged()
        {
            var tool = CreateTool(WriteDictionaryFile(), useIdentityMapDataFormat: false);
            var activated = new DateTime(2020, 10, 7, 14, 50, 44);

            // Field.Value returns a new box on every read; a read-only Activated Date must not be written back (TF26194).
            Assert.IsFalse(tool.TryMapUserIdentityValue("Microsoft.VSTS.Common.ActivatedDate", (object)activated, out var mapped));
            Assert.AreEqual(activated, mapped);
            Assert.IsFalse(tool.TryMapUserIdentityValue("Microsoft.VSTS.Common.ActivatedDate", null, out _));
        }

        [TestMethod, TestCategory("L1")]
        public void TryMapUserIdentityValue_IdentityField_ReportsChangeOnlyWhenMapped()
        {
            var tool = CreateTool(WriteDictionaryFile(), useIdentityMapDataFormat: false);

            Assert.IsTrue(tool.TryMapUserIdentityValue(AssignedTo, "Old Name", out var mapped));
            Assert.AreEqual("New Name", mapped);
            Assert.IsFalse(tool.TryMapUserIdentityValue(AssignedTo, "Someone Else", out mapped));
            Assert.AreEqual("Someone Else", mapped);
        }

        private static IdentityItemData User(string displayName, string mail = null, string account = null) =>
            new IdentityItemData { DisplayName = displayName, MailAddress = mail, AccountName = account ?? displayName };

        [TestMethod, TestCategory("L0")]
        public void MatchSourceUsersToTarget_ByDisplayName_MatchesAndLeavesMissesUnmapped()
        {
            var source = new List<IdentityItemData> { User("Alice"), User("Bob") };
            var target = new List<IdentityItemData> { User("Alice", account: "alice@new") };

            var result = TfsUserMappingTool.MatchSourceUsersToTarget(source, target, false, _logger);

            Assert.HasCount(2, result);
            Assert.AreEqual("alice@new", result[0].Target.AccountName);
            Assert.IsNull(result[1].Target);
        }

        [TestMethod, TestCategory("L0")]
        public void MatchSourceUsersToTarget_DuplicateTargetDisplayNames_Throws()
        {
            var source = new List<IdentityItemData> { User("Alice", "alice@old") };
            var target = new List<IdentityItemData> { User("Alice", "a1@new", "a1"), User("Alice", "a2@new", "a2") };

            Assert.ThrowsExactly<InvalidOperationException>(() => TfsUserMappingTool.MatchSourceUsersToTarget(source, target, false, _logger));
        }

        [TestMethod, TestCategory("L0")]
        public void MatchSourceUsersToTarget_MatchByEmail_ResolvesDuplicateDisplayNames()
        {
            var source = new List<IdentityItemData> { User("Alice", "A2@NEW") };
            var target = new List<IdentityItemData> { User("Alice", "a1@new", "a1"), User("Alice", "a2@new", "a2"), User("NoMail", null, "nm") };

            var result = TfsUserMappingTool.MatchSourceUsersToTarget(source, target, true, _logger);

            Assert.AreEqual("a2", result.Single().Target.AccountName);
        }

        [TestMethod, TestCategory("L0")]
        public void MatchSourceUsersToTarget_MatchByEmail_FallsBackToDisplayName()
        {
            var source = new List<IdentityItemData> { User("Bob", "bob@old") };
            var target = new List<IdentityItemData> { User("Bob", "bob@new", "bob") };

            var result = TfsUserMappingTool.MatchSourceUsersToTarget(source, target, true, _logger);

            Assert.AreEqual("bob", result.Single().Target.AccountName);
        }

        [TestMethod, TestCategory("L0")]
        public void MatchSourceUsersToTarget_EmailDomainMapping_ResolvesDuplicateDisplayNamesAcrossDomainChange()
        {
            var source = new List<IdentityItemData> { User("Alice", "a2@old.com") };
            var target = new List<IdentityItemData> { User("Alice", "a1@new.com", "a1"), User("Alice", "a2@new.com", "a2") };
            var domains = new Dictionary<string, string> { { "old.com", "new.com" } };

            var result = TfsUserMappingTool.MatchSourceUsersToTarget(source, target, true, _logger, domains);

            Assert.AreEqual("a2", result.Single().Target.AccountName);
        }

        [TestMethod, TestCategory("L0")]
        public void GetUsersFromWorkItems_UsesExactCaseInsensitiveFieldNames()
        {
            var revision = new RevisionItem
            {
                Fields = new Dictionary<string, FieldItem>
                {
                    ["a"] = new FieldItem { ReferenceName = "system.assignedto", Value = "Alice" },
                    ["b"] = new FieldItem { ReferenceName = "System.AssignedToExtra", Value = "Substring Match" },
                    ["c"] = new FieldItem { ReferenceName = AssignedTo, Value = null },
                    ["d"] = new FieldItem { ReferenceName = AssignedTo, Value = "ALICE" },
                }
            };
            var workItem = new WorkItemData { Revisions = new SortedDictionary<int, RevisionItem> { [0] = revision } };

            var users = TfsUserMappingTool.GetUsersFromWorkItems(new List<WorkItemData> { workItem }, new List<string> { AssignedTo });

            Assert.HasCount(1, users);
            Assert.Contains("Alice", users);
        }

        [TestMethod, TestCategory("L0")]
        public void SkipValidateAllUsersExistOrAreMapped_DefaultsFalseAndBinds()
        {
            Assert.IsFalse(new TfsUserMappingToolOptions().SkipValidateAllUsersExistOrAreMapped);
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { ["SkipValidateAllUsersExistOrAreMapped"] = "true" })
                .Build();
            var options = new TfsUserMappingToolOptions();
            config.Bind(options);
            Assert.IsTrue(options.SkipValidateAllUsersExistOrAreMapped);
        }
    }
}
