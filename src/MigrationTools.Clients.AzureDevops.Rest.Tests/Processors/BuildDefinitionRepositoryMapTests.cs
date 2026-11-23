using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts.Pipelines;

namespace MigrationTools.Processors.Tests
{
    [TestClass]
    public class BuildDefinitionRepositoryMapTests
    {
        private static readonly List<GitRepository> SourceRepositories = new() { new GitRepository { Id = "src-1", Name = "app" } };
        private static readonly List<GitRepository> TargetRepositories = new() { new GitRepository { Id = "tgt-1", Name = "app" }, new GitRepository { Id = "tgt-2", Name = "renamed" } };

        private static BuildDefinition Definition(Properties properties) => new()
        {
            Name = "build",
            Repository = new Repository { Id = "src-1", Name = "app", Type = "TfsGit", Properties = properties }
        };

        [TestMethod, TestCategory("L0")]
        public void Apply_RepositoryWithProperties_MapsToTargetRepository()
        {
            var definition = Definition(new Properties());

            var (name, mapped) = BuildDefinitionRepositoryMap.Apply(definition, SourceRepositories, TargetRepositories, new Dictionary<string, string>());

            Assert.IsTrue(mapped);
            Assert.AreEqual("app", name);
            Assert.AreEqual("tgt-1", definition.Repository.Id);
        }

        [TestMethod, TestCategory("L0")]
        public void Apply_RepositoryWithoutProperties_MapsToTargetRepository()
        {
            var definition = Definition(null);

            var (_, mapped) = BuildDefinitionRepositoryMap.Apply(definition, SourceRepositories, TargetRepositories, new Dictionary<string, string>());

            Assert.IsTrue(mapped);
            Assert.AreEqual("tgt-1", definition.Repository.Id);
        }

        [TestMethod, TestCategory("L0")]
        public void Apply_RepositoryNameMap_MapsToRenamedTargetRepository()
        {
            var definition = Definition(null);

            var (name, mapped) = BuildDefinitionRepositoryMap.Apply(definition, SourceRepositories, TargetRepositories, new Dictionary<string, string> { ["app"] = "renamed" });

            Assert.IsTrue(mapped);
            Assert.AreEqual("renamed", name);
            Assert.AreEqual("tgt-2", definition.Repository.Id);
            Assert.AreEqual("renamed", definition.Repository.Name);
        }

        [TestMethod, TestCategory("L0")]
        public void Apply_TargetRepositoryMissing_ReportsNotFoundAndLeavesIdUnchanged()
        {
            var definition = Definition(null);

            var (name, mapped) = BuildDefinitionRepositoryMap.Apply(definition, SourceRepositories, new List<GitRepository>(), new Dictionary<string, string>());

            Assert.IsFalse(mapped);
            Assert.AreEqual("app", name);
            Assert.AreEqual("src-1", definition.Repository.Id);
        }

        [TestMethod, TestCategory("L0")]
        public void Apply_TargetRepositoryMissingWithProperties_ReportsNotFound()
        {
            var definition = Definition(new Properties());

            var (_, mapped) = BuildDefinitionRepositoryMap.Apply(definition, SourceRepositories, new List<GitRepository>(), new Dictionary<string, string>());

            Assert.IsFalse(mapped);
        }

        [TestMethod, TestCategory("L0")]
        public void UsesAzureReposGit_GitHubRepository_IsFalse()
        {
            var definition = new BuildDefinition { Repository = new Repository { Type = "GitHub" } };

            Assert.IsFalse(BuildDefinitionRepositoryMap.UsesAzureReposGit(definition));
            Assert.IsTrue(BuildDefinitionRepositoryMap.UsesAzureReposGit(Definition(null)));
        }
    }
}
