using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Server;
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

        private static Identity User(string sid) => new() { Sid = sid, Type = IdentityType.UnknownIdentityType, AccountName = sid, DisplayName = "User " + sid };
        private static Identity Group(string sid) => new() { Sid = sid, Type = IdentityType.ApplicationGroup, AccountName = sid, DisplayName = "Group " + sid };

        private static IdentityItemData ToItem(Identity i) => new() { Sid = i.Sid, AccountName = i.AccountName, DisplayName = i.DisplayName };

        private sealed class FakeServer
        {
            public Dictionary<string, Identity> Identities { get; } = [];
            public HashSet<string> Type5Groups { get; } = [];
            public List<int> BatchSizes { get; } = [];

            public Identity[] ReadBatch(string[] sids)
            {
                BatchSizes.Add(sids.Length);
                if (sids.Any(Type5Groups.Contains))
                {
                    throw new InvalidOperationException(Type5Error);
                }
                return sids.Select(s => Identities.TryGetValue(s, out var i) ? i : null).ToArray();
            }
        }

        private static FakeServer CreateServer(int users, int groups, IEnumerable<string> type5)
        {
            var server = new FakeServer();
            for (int i = 0; i < users; i++) { server.Identities[$"U{i}"] = User($"U{i}"); }
            for (int i = 0; i < groups; i++) { server.Identities[$"G{i}"] = Group($"G{i}"); }
            foreach (var sid in type5) { server.Type5Groups.Add(sid); }
            return server;
        }

        [TestMethod, TestCategory("L0")]
        public void ReadUsers_ReadsInBatchesNotOneAtATime()
        {
            var server = CreateServer(427, 0, []);
            var reader = new TfsIdentityBatchReader(server.ReadBatch, _ => throw new AssertFailedException("no fallback expected"), ToItem, null);

            var users = reader.ReadUsers(server.Identities.Keys.ToList());

            Assert.HasCount(427, users);
            Assert.AreEqual(5, reader.Calls);
            CollectionAssert.AreEqual(new[] { 100, 100, 100, 100, 27 }, server.BatchSizes);
        }

        [TestMethod, TestCategory("L0")]
        public void ReadUsers_Type5GroupsInBatch_KeepsEveryUserAndReadsGroupsThroughFallback()
        {
            // The customer's run: 7 groups of ApplicationGroupSpecialType 5 among 427 identities.
            var type5 = new[] { "T0", "T1", "T2", "T3", "T4", "T5", "T6" };
            var server = CreateServer(400, 20, type5);
            var sids = server.Identities.Keys.Concat(type5).OrderBy(s => s.GetHashCode()).ToList();
            var fallbackCalls = new List<string>();
            var reader = new TfsIdentityBatchReader(server.ReadBatch,
                sid => { fallbackCalls.Add(sid); return (FallbackIdentityKind.Group, null); }, ToItem, null);

            var users = reader.ReadUsers(sids);

            Assert.HasCount(400, users);
            CollectionAssert.AreEquivalent(type5, fallbackCalls);
            CollectionAssert.AreEquivalent(type5, reader.FallbackGroups);
            Assert.IsEmpty(reader.Unreadable);
            Assert.IsLessThan(sids.Count / 2, reader.Calls, $"expected far fewer calls than identities, got {reader.Calls}");
        }

        [TestMethod, TestCategory("L0")]
        public void ReadUsers_FailingIdentityThatIsAUser_IsKeptFromFallback()
        {
            var server = CreateServer(10, 0, ["X"]);
            var reader = new TfsIdentityBatchReader(server.ReadBatch,
                sid => (FallbackIdentityKind.User, new IdentityItemData { Sid = sid, AccountName = "x@example.com", DisplayName = "X" }), ToItem, null);

            var users = reader.ReadUsers([.. server.Identities.Keys, "X"]);

            Assert.HasCount(11, users);
            Assert.IsTrue(users.Any(u => u.AccountName == "x@example.com"));
        }

        [TestMethod, TestCategory("L0")]
        public void ReadUsers_FallbackFails_IsReportedUnreadableAndOthersKept()
        {
            var server = CreateServer(10, 0, ["X"]);
            var reader = new TfsIdentityBatchReader(server.ReadBatch, _ => throw new InvalidOperationException("boom"), ToItem, null);

            var users = reader.ReadUsers([.. server.Identities.Keys, "X"]);

            Assert.HasCount(10, users);
            CollectionAssert.AreEqual(new[] { "X" }, reader.Unreadable);
        }

        [TestMethod, TestCategory("L0")]
        public void ReadUsers_SkipsGroupsAndNulls()
        {
            var server = CreateServer(3, 4, []);
            var reader = new TfsIdentityBatchReader(server.ReadBatch, _ => (FallbackIdentityKind.Unreadable, null), ToItem, null);

            var users = reader.ReadUsers([.. server.Identities.Keys, "missing"]);

            Assert.HasCount(3, users);
            Assert.IsEmpty(reader.Unreadable);
        }

        [TestMethod, TestCategory("L0")]
        public void ReadUsers_LogsProgressPerBatch()
        {
            var server = CreateServer(250, 0, []);
            var log = new Mock<ILogger>();
            var reader = new TfsIdentityBatchReader(server.ReadBatch, _ => (FallbackIdentityKind.Unreadable, null), ToItem, log.Object);

            reader.ReadUsers(server.Identities.Keys.ToList());

            log.Verify(l => l.Log(LogLevel.Information, It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Read ") && v.ToString().Contains("/250 identities")),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Exactly(3));
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
