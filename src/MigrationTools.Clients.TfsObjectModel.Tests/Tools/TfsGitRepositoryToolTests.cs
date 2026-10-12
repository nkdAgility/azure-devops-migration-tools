using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tools;

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

        [TestMethod, TestCategory("L0")]
        public void CreateByName_NoMatch_ReturnsNoRepo()
        {
            var source = new TfsGitRepositoryInfo("abc", null, Repo("OldProject", "MyRepo"));
            var result = TfsGitRepositoryInfo.Create("Missing", source, new List<GitRepository> { Repo("ProjectA", "Shared") });
            Assert.IsNull(result.GitRepo);
        }
    }
}
