using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tools;
using Moq;

namespace MigrationTools.Tests.Tools
{
    [TestClass]
    public class TfsGitRepositoryToolTests
    {
        private static GitRepository Repo(string project, string name) => new GitRepository
        {
            Id = Guid.NewGuid(),
            Name = name,
            ProjectReference = new TeamProjectReference { Id = Guid.NewGuid(), Name = project }
        };

        [TestMethod, TestCategory("L0")]
        public void ShouldUseCrossProjectRepo_RepoFromFormerProject_TargetHasSameName_ReturnsTrue()
        {
            var source = Repo("OldProject", "MyRepo");
            var targets = new List<GitRepository> { Repo("Target", "MyRepo") };
            Assert.IsTrue(TfsGitRepositoryTool.ShouldUseCrossProjectRepo(source, new Dictionary<string, string>(), targets));
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldUseCrossProjectRepo_NotMapped_NoTargetMatch_ReturnsFalse()
        {
            var source = Repo("OldProject", "MyRepo");
            var targets = new List<GitRepository> { Repo("Target", "Other") };
            Assert.IsFalse(TfsGitRepositoryTool.ShouldUseCrossProjectRepo(source, new Dictionary<string, string>(), targets));
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldUseCrossProjectRepo_NullRepo_ReturnsFalse()
        {
            Assert.IsFalse(TfsGitRepositoryTool.ShouldUseCrossProjectRepo(null, new Dictionary<string, string>(), new List<GitRepository>()));
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldUseCrossProjectRepo_MappedByName_ReturnsTrue()
        {
            var source = Repo("OldProject", "MyRepo");
            var map = new Dictionary<string, string> { { "MyRepo", "NewRepo" } };
            Assert.IsTrue(TfsGitRepositoryTool.ShouldUseCrossProjectRepo(source, map, new List<GitRepository>()));
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldUseCrossProjectRepo_MappedByProjectQualifiedName_ReturnsTrue()
        {
            var source = Repo("OldProject", "MyRepo");
            var map = new Dictionary<string, string> { { "oldproject/myrepo", "NewRepo" } };
            Assert.IsTrue(TfsGitRepositoryTool.ShouldUseCrossProjectRepo(source, map, new List<GitRepository>()));
        }

        [TestMethod, TestCategory("L0")]
        public void GetTargetRepoName_QualifiedMappingTakesPrecedence()
        {
            var info = new TfsGitRepositoryInfo("abc", null, Repo("OldProject", "MyRepo"));
            var map = new Dictionary<string, string> { { "MyRepo", "ByName" }, { "OldProject/MyRepo", "ByProject" } };
            Assert.AreEqual("ByProject", TfsGitRepositoryTool.GetTargetRepoName(map, info));
        }

        [TestMethod, TestCategory("L0")]
        public void GetTargetRepoName_NoMapping_ReturnsSourceName()
        {
            var info = new TfsGitRepositoryInfo("abc", null, Repo("OldProject", "MyRepo"));
            Assert.AreEqual("MyRepo", TfsGitRepositoryTool.GetTargetRepoName(new Dictionary<string, string>(), info));
        }

        [TestMethod, TestCategory("L0")]
        public void GetTargetRepoName_TfvcRepoWithoutProject_UsesNameMapping()
        {
            var info = new TfsGitRepositoryInfo("abc", null, new GitRepository { Name = "Proj" });
            var map = new Dictionary<string, string> { { "Proj", "Mapped" } };
            Assert.AreEqual("Mapped", TfsGitRepositoryTool.GetTargetRepoName(map, info));
        }

        [TestMethod, TestCategory("L0")]
        public void CreateByName_DuplicateNameAcrossCollection_ReturnsNoRepo()
        {
            var source = new TfsGitRepositoryInfo("abc", null, Repo("OldProject", "MyRepo"));
            var collection = new List<GitRepository> { Repo("ProjectA", "Shared"), Repo("ProjectB", "Shared") };
            var result = TfsGitRepositoryInfo.Create("Shared", source, collection);
            Assert.IsNull(result.GitRepo);
            Assert.IsNull(result.RepoID);
            Assert.AreEqual("abc", result.CommitID);
        }

        [TestMethod, TestCategory("L0")]
        public void CreateByName_SingleMatch_ReturnsRepo()
        {
            var source = new TfsGitRepositoryInfo("abc", null, Repo("OldProject", "MyRepo"));
            var only = Repo("ProjectA", "Shared");
            var collection = new List<GitRepository> { only, Repo("ProjectB", "Other") };
            var result = TfsGitRepositoryInfo.Create("Shared", source, collection);
            Assert.AreSame(only, result.GitRepo);
            Assert.AreEqual(only.Id.ToString(), result.RepoID);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldUseCrossProjectRepo_TargetNameDiffersOnlyByCase_ReturnsTrue()
        {
            var source = Repo("OldProject", "MyRepo");
            var targets = new List<GitRepository> { Repo("Target", "myrepo") };
            Assert.IsTrue(TfsGitRepositoryTool.ShouldUseCrossProjectRepo(source, new Dictionary<string, string>(), targets));
        }

        [TestMethod, TestCategory("L0")]
        public void CreateByName_NameDiffersOnlyByCase_ReturnsRepo()
        {
            var source = new TfsGitRepositoryInfo("abc", null, Repo("OldProject", "MyRepo"));
            var target = Repo("Target", "myrepo");
            var result = TfsGitRepositoryInfo.Create("MyRepo", source, new List<GitRepository> { target });
            Assert.AreSame(target, result.GitRepo);
        }

        private static TfsGitRepositoryTool Tool(TfsGitTargetArtifactCache artifacts)
        {
            return new TfsGitRepositoryTool(
                Microsoft.Extensions.Options.Options.Create(new TfsGitRepositoryToolOptions()),
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<TfsGitRepositoryTool>>().Object,
                new Mock<ITelemetryLogger>().Object)
            { TargetArtifacts = artifacts };
        }

        [TestMethod, TestCategory("L0")]
        public void VerifyTargetExists_DefaultsToTrue()
        {
            Assert.IsTrue(new TfsGitRepositoryToolOptions().VerifyTargetExists);
        }

        [TestMethod, TestCategory("L0")]
        public void TargetHasArtifact_CommitExists_ReturnsTrueAndRecordsNothing()
        {
            var repo = Repo("Target", "MyRepo");
            var tool = Tool(new TfsGitTargetArtifactCache((r, c) => r == repo.Id && c == "abc", (r, b) => false));
            Assert.IsTrue(tool.TargetHasArtifact(false, repo, "abc", "vstfs:///Git/Commit/x", "1"));
            Assert.IsEmpty(tool.LinksLeftOnSource);
        }

        [TestMethod, TestCategory("L0")]
        public void TargetHasArtifact_CommitMissing_ReturnsFalseAndCountsRepo()
        {
            var repo = Repo("Target", "MyRepo");
            var tool = Tool(new TfsGitTargetArtifactCache((r, c) => false, (r, b) => false));
            Assert.IsFalse(tool.TargetHasArtifact(false, repo, "abc", "vstfs:///Git/Commit/x", "1"));
            Assert.IsFalse(tool.TargetHasArtifact(false, repo, "def", "vstfs:///Git/Commit/y", "2"));
            Assert.AreEqual(2, tool.LinksLeftOnSource["Target/MyRepo"]);
        }

        [TestMethod, TestCategory("L0")]
        public void TargetHasArtifact_SameMissingLinkOnLaterRevision_CountedOnce()
        {
            var repo = Repo("Target", "MyRepo");
            var tool = Tool(new TfsGitTargetArtifactCache((r, c) => false, (r, b) => false));
            Assert.IsFalse(tool.TargetHasArtifact(false, repo, "abc", "vstfs:///Git/Commit/x", "1"));
            Assert.IsFalse(tool.TargetHasArtifact(false, repo, "abc", "vstfs:///Git/Commit/x", "1"));
            Assert.AreEqual(1, tool.LinksLeftOnSource["Target/MyRepo"]);
        }

        [TestMethod, TestCategory("L0")]
        public void TargetHasArtifact_BranchExists_ChecksDecodedBranchName()
        {
            var repo = Repo("Target", "MyRepo");
            string asked = null;
            var tool = Tool(new TfsGitTargetArtifactCache((r, c) => false, (r, b) => { asked = b; return true; }));
            Assert.IsTrue(tool.TargetHasArtifact(true, repo, "GBfeature%2Fx", "vstfs:///Git/Ref/x", "1"));
            Assert.AreEqual("feature/x", asked);
        }

        [TestMethod, TestCategory("L0")]
        public void TargetHasArtifact_BranchMissing_ReturnsFalse()
        {
            var repo = Repo("Target", "MyRepo");
            var tool = Tool(new TfsGitTargetArtifactCache((r, c) => true, (r, b) => false));
            Assert.IsFalse(tool.TargetHasArtifact(true, repo, "GBdeleted", "vstfs:///Git/Ref/x", "1"));
        }

        [TestMethod, TestCategory("L0")]
        public void ArtifactCache_SameCommit_QueriesServerOnce()
        {
            var repo = Guid.NewGuid();
            int calls = 0;
            var cache = new TfsGitTargetArtifactCache((r, c) => { calls++; return false; }, (r, b) => true);
            Assert.IsFalse(cache.CommitExists(repo, "abc"));
            Assert.IsFalse(cache.CommitExists(repo, "ABC"));
            Assert.AreEqual(1, calls);
            cache.CommitExists(Guid.NewGuid(), "abc");
            Assert.AreEqual(2, calls, "the cache is per repository");
        }

        [TestMethod, TestCategory("L0")]
        public void ArtifactCache_SameBranch_QueriesServerOnce()
        {
            var repo = Guid.NewGuid();
            int calls = 0;
            var cache = new TfsGitTargetArtifactCache((r, c) => true, (r, b) => { calls++; return true; });
            Assert.IsTrue(cache.BranchExists(repo, "GBmain"));
            Assert.IsTrue(cache.BranchExists(repo, "GBmain"));
            Assert.AreEqual(1, calls);
        }

        [TestMethod, TestCategory("L0")]
        public void ArtifactCache_UnknownAnswer_IsNotCachedAndTreatedAsMissing()
        {
            int calls = 0;
            var cache = new TfsGitTargetArtifactCache((r, c) => { calls++; return calls == 1 ? (bool?)null : true; }, (r, b) => true);
            var repo = Guid.NewGuid();
            Assert.IsFalse(cache.CommitExists(repo, "abc"));
            Assert.IsTrue(cache.CommitExists(repo, "abc"));
            Assert.AreEqual(2, calls);
        }

        [TestMethod, TestCategory("L0")]
        public void BranchNameFromRefId_NonBranchRef_ReturnsNull()
        {
            Assert.IsNull(TfsGitTargetArtifactCache.BranchNameFromRefId("GTv1.0"));
            Assert.AreEqual("a/b", TfsGitTargetArtifactCache.BranchNameFromRefId("GBa%2fb"));
        }

        [TestMethod, TestCategory("L0")]
        public void CreateByName_NoMatch_ReturnsNoRepo()
        {
            var source = new TfsGitRepositoryInfo("abc", null, Repo("OldProject", "MyRepo"));
            var result = TfsGitRepositoryInfo.Create("Missing", source, new List<GitRepository> { Repo("ProjectA", "Shared") });
            Assert.IsNull(result.GitRepo);
        }
    }
}
