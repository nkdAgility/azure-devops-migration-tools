using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Exceptions;
using MigrationTools.Tools;
using Moq;
using Newtonsoft.Json;

namespace MigrationTools.Tests.Tools
{
    [TestClass]
    public class TfsIdentityBatchReaderTests
    {
        private const string Type5Error = "Server was unable to process request. ---> There was an error generating the XML document. ---> "
            + "Instance validation error: '5' is not a valid value for Microsoft.TeamFoundation.Framework.Server.ApplicationGroupSpecialType.";

        // Keys starting "U" are users, "G" groups, "T" groups the batched read cannot serialise.
        private sealed class FakeServer
        {
            public List<int> BatchSizes { get; } = [];

            public IEnumerable<IdentityItemData> ReadUsers(string[] keys)
            {
                BatchSizes.Add(keys.Length);
                if (keys.Any(k => k.StartsWith("T")))
                {
                    throw new InvalidOperationException(Type5Error);
                }
                return keys.Where(k => k.StartsWith("U")).Select(k => new IdentityItemData { Sid = k, AccountName = k, DisplayName = "User " + k });
            }
        }

        private static List<string> Keys(int users, int groups, int type5)
        {
            var keys = Enumerable.Range(0, users).Select(i => $"U{i}")
                .Concat(Enumerable.Range(0, groups).Select(i => $"G{i}"))
                .Concat(Enumerable.Range(0, type5).Select(i => $"T{i}"))
                .ToList();
            // Spread the type-5 groups through the list, as they are in a real membership list.
            return keys.OrderBy(k => (k.GetHashCode() & 0x7fffffff) % 997).ThenBy(k => k).ToList();
        }

        private static TfsIdentityBatchReader<string> Reader(FakeServer server, Func<string, (FallbackIdentityKind, IdentityItemData)> fallback, ILogger log = null) =>
            new(server.ReadUsers, fallback, k => k, log);

        [TestMethod, TestCategory("L0")]
        public void ReadUsers_ReadsInBatchesNotOneAtATime()
        {
            var server = new FakeServer();
            var reader = Reader(server, _ => throw new AssertFailedException("no fallback expected"));

            var users = reader.ReadUsers(Keys(427, 0, 0));

            Assert.HasCount(427, users);
            Assert.AreEqual(5, reader.Calls);
            CollectionAssert.AreEqual(new[] { 100, 100, 100, 100, 27 }, server.BatchSizes);
        }

        [TestMethod, TestCategory("L0")]
        public void ReadUsers_Type5GroupsInBatches_KeepsEveryUser()
        {
            // The customer's run: 7 groups of ApplicationGroupSpecialType 5 among 427 identities.
            var server = new FakeServer();
            var fallbackCalls = new List<string>();
            var reader = Reader(server, k => { fallbackCalls.Add(k); return (FallbackIdentityKind.NotAUser, null); });

            var users = reader.ReadUsers(Keys(400, 20, 7));

            Assert.HasCount(400, users);
            CollectionAssert.AreEquivalent(Enumerable.Range(0, 7).Select(i => $"T{i}").ToList(), fallbackCalls);
            CollectionAssert.AreEquivalent(fallbackCalls, reader.SkippedNonUsers);
            Assert.IsEmpty(reader.Unreadable);
            Assert.IsLessThan(427 / 2, reader.Calls, $"expected far fewer calls than identities, got {reader.Calls}");
        }

        [TestMethod, TestCategory("L0")]
        public void ReadUsers_FailingIdentityThatIsAUser_IsKeptFromFallback()
        {
            var server = new FakeServer();
            var reader = Reader(server, k => (FallbackIdentityKind.User, new IdentityItemData { Sid = k, AccountName = "t@example.com", DisplayName = "T" }));

            var users = reader.ReadUsers(Keys(10, 0, 1));

            Assert.HasCount(11, users);
            Assert.IsTrue(users.Any(u => u.AccountName == "t@example.com"));
        }

        [TestMethod, TestCategory("L0")]
        public void ReadUsers_FallbackFails_IsReportedUnreadableAndOthersKept()
        {
            var server = new FakeServer();
            var reader = Reader(server, _ => throw new InvalidOperationException("boom"));

            var users = reader.ReadUsers(Keys(10, 3, 1));

            Assert.HasCount(10, users);
            CollectionAssert.AreEqual(new[] { "T0" }, reader.Unreadable);
        }

        [TestMethod, TestCategory("L0")]
        public void ReadUsers_FallbackFindsNothing_IsReportedUnreadable()
        {
            var reader = Reader(new FakeServer(), _ => (FallbackIdentityKind.Unreadable, null));

            reader.ReadUsers(Keys(2, 0, 1));

            CollectionAssert.AreEqual(new[] { "T0" }, reader.Unreadable);
        }

        [TestMethod, TestCategory("L0")]
        public void ReadUsers_IgnoresNullResults()
        {
            var reader = new TfsIdentityBatchReader<string>(keys => keys.Select(k => k == "U0" ? null : new IdentityItemData { Sid = k }), _ => (FallbackIdentityKind.Unreadable, null), k => k, null);

            var users = reader.ReadUsers(["U0", "U1"]);

            Assert.HasCount(1, users);
        }

        [TestMethod, TestCategory("L0")]
        public void ReadUsers_LogsProgressPerBatch()
        {
            var log = new Mock<ILogger>();
            var reader = Reader(new FakeServer(), _ => (FallbackIdentityKind.Unreadable, null), log.Object);

            reader.ReadUsers(Keys(250, 0, 0));

            log.Verify(l => l.Log(LogLevel.Information, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("/250 identities")),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Exactly(3));
        }

        [TestMethod, TestCategory("L0")]
        public void IsTeamFoundationGroup_TellsGroupsFromEntraUsers()
        {
            Assert.IsTrue(TfsUserMappingTool.IsTeamFoundationGroup(new IdentityDescriptor(TfsUserMappingTool.TeamFoundationGroupDescriptorType, "S-1-9-1551374245-1")));
            Assert.IsFalse(TfsUserMappingTool.IsTeamFoundationGroup(new IdentityDescriptor("Microsoft.IdentityModel.Claims.ClaimsIdentity", "tenant\\user@example.com")));
            Assert.IsFalse(TfsUserMappingTool.IsTeamFoundationGroup(null));
        }
    }
    [TestClass]
    public class TfsUserMappingToolValidationScopeTests
    {
        private string _dir;

        [TestInitialize]
        public void Setup()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"TfsUserMappingScope_{Guid.NewGuid()}");
            Directory.CreateDirectory(_dir);
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_dir, true);

        private static readonly List<IdentityItemData> Targets =
        [
            new() { DisplayName = "Alice Target", AccountName = "alice@target.com", MailAddress = "alice@target.com" },
            new() { DisplayName = "Bob Target", AccountName = "bob@target.com", MailAddress = "bob@target.com" },
        ];

        private TfsUserMappingTool CreateTool(MappingTargetValidationScope scope, bool stop = true)
        {
            var file = Path.Combine(_dir, "map.json");
            File.WriteAllText(file, JsonConvert.SerializeObject(new Dictionary<string, string>
            {
                ["Alice Source"] = "Alice Target",
                ["Ghost Source"] = "Nobody At All",
            }));
            var options = new TfsUserMappingToolOptions
            {
                Enabled = true,
                UserMappingFile = file,
                ValidateMappingTargets = scope,
                StopIfMappingTargetsDoNotResolve = stop
            };
            return new TfsUserMappingTool(Microsoft.Extensions.Options.Options.Create(options), new Mock<IServiceProvider>().Object,
                new Mock<ILogger<TfsUserMappingTool>>().Object, new Mock<ITelemetryLogger>().Object);
        }

        [TestMethod, TestCategory("L0")]
        public void All_UnresolvedTargetStopsRun()
        {
            var tool = CreateTool(MappingTargetValidationScope.All);
            Assert.ThrowsExactly<MigrationRunStoppedException>(() => tool.ValidateMappingTargets(() => Targets, null, null));
        }

        [TestMethod, TestCategory("L0")]
        public void All_UnresolvedTargetWithStopOff_Continues()
        {
            var tool = CreateTool(MappingTargetValidationScope.All, stop: false);
            tool.ValidateMappingTargets(() => Targets, null, null);
        }

        [TestMethod, TestCategory("L0")]
        public void Referenced_IgnoresUnreferencedBadMapping()
        {
            var tool = CreateTool(MappingTargetValidationScope.Referenced);
            tool.ValidateMappingTargets(() => Targets, null, ["Alice Source"]);
        }

        [TestMethod, TestCategory("L0")]
        public void Referenced_ReferencedBadMappingStopsRun()
        {
            var tool = CreateTool(MappingTargetValidationScope.Referenced);
            Assert.ThrowsExactly<MigrationRunStoppedException>(() => tool.ValidateMappingTargets(() => Targets, null, ["Ghost Source"]));
        }

        [TestMethod, TestCategory("L0")]
        public void Referenced_NothingReferenced_DoesNotLoadTargets()
        {
            var tool = CreateTool(MappingTargetValidationScope.Referenced);
            tool.ValidateMappingTargets(() => throw new AssertFailedException("targets should not be loaded"), null, ["Someone Unmapped"]);
        }

        [TestMethod, TestCategory("L0")]
        public void Skip_DoesNotLoadTargetsOrStop()
        {
            var tool = CreateTool(MappingTargetValidationScope.Skip);
            tool.ValidateMappingTargets(() => throw new AssertFailedException("targets should not be loaded"), null, null);
        }

        [TestMethod, TestCategory("L0")]
        public void FilterToReferencedMappings_MatchesQualifiedKeysAndValues()
        {
            var map = new Dictionary<string, string>
            {
                ["Alice <alice@src.com>"] = "A",
                ["Bob"] = "B",
                ["Carol"] = "C",
            };

            var byName = TfsUserMappingTool.FilterToReferencedMappings(map, ["alice", "Bob <bob@src.com>"]);

            CollectionAssert.AreEquivalent(new[] { "Alice <alice@src.com>", "Bob" }, byName.Keys.ToList());
        }

        [TestMethod, TestCategory("L1")]
        public void ValidateMappingTargets_DefaultsAllAndBindsFromString()
        {
            Assert.AreEqual(MappingTargetValidationScope.All, new TfsUserMappingToolOptions().ValidateMappingTargets);
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { ["ValidateMappingTargets"] = "Referenced" })
                .Build();
            var options = new TfsUserMappingToolOptions();
            configuration.Bind(options);
            Assert.AreEqual(MappingTargetValidationScope.Referenced, options.ValidateMappingTargets);
        }
    }
}

