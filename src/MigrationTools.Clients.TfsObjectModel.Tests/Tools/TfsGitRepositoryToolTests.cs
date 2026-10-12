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
    }
}
