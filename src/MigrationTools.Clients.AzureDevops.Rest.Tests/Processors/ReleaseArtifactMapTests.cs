using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.DataContracts.Pipelines;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MigrationTools.Processors.Tests
{
    [TestClass]
    public class ReleaseArtifactMapTests
    {
        private static ReleaseArtifactTargets Targets() => new()
        {
            SourceProjectName = "src",
            TargetProject = new ArtifactSourceDefinitionUrl { Id = "tgt-project", Name = "tgt" },
            BuildDefinitionMappings = new List<Mapping> { new() { SourceId = "21", TargetId = "7", Name = "build" } },
            SourceRepositories = new List<GitRepository> { new() { Id = "src-repo", Name = "app" } },
            TargetRepositories = new List<GitRepository> { new() { Id = "tgt-repo", Name = "app" }, new() { Id = "tgt-renamed", Name = "renamed" } },
            RepositoryNameMaps = new Dictionary<string, string>(),
            ServiceConnectionMappings = new List<Mapping> { new() { SourceId = "src-conn", TargetId = "tgt-conn", Name = "github" } },
        };

        private static Artifact Artifact(string type, string definitionId, string definitionName, string projectName = "src") => new()
        {
            Type = type,
            Alias = "_alias",
            SourceId = $"src-project:{definitionId}",
            DefinitionReference = new DefinitionReference
            {
                Definition = new ArtifactSourceDefinitionUrl { Id = definitionId, Name = definitionName },
                Project = new ArtifactSourceDefinitionUrl { Id = "src-project", Name = projectName },
            }
        };

        [TestMethod, TestCategory("L0")]
        public void Apply_BuildArtifact_PointsAtTargetBuildAndProject()
        {
            var artifact = Artifact("Build", "21", "build");

            var reason = ReleaseArtifactMap.Apply(artifact, Targets());

            Assert.IsNull(reason);
            Assert.AreEqual("7", artifact.DefinitionReference.Definition.Id);
            Assert.AreEqual("tgt-project", artifact.DefinitionReference.Project.Id);
            Assert.AreEqual("tgt", artifact.DefinitionReference.Project.Name);
            Assert.AreEqual("tgt-project:7", artifact.SourceId);
        }

        [TestMethod, TestCategory("L0")]
        public void Apply_BuildNotInTarget_ReportsWhy()
        {
            var artifact = Artifact("Build", "99", "missing");

            var reason = ReleaseArtifactMap.Apply(artifact, Targets());

            StringAssert.Contains(reason, "\"missing\" is not in the target project");
            Assert.AreEqual("99", artifact.DefinitionReference.Definition.Id);
        }

        [TestMethod, TestCategory("L0")]
        public void Apply_BuildFromAnotherProject_ReportsWhy()
        {
            var artifact = Artifact("Build", "21", "build", projectName: "other");

            var reason = ReleaseArtifactMap.Apply(artifact, Targets());

            StringAssert.Contains(reason, "comes from project \"other\"");
        }

        [TestMethod, TestCategory("L0")]
        public void Apply_GitArtifact_PointsAtTargetRepository()
        {
            var artifact = Artifact("Git", "src-repo", "app");

            var reason = ReleaseArtifactMap.Apply(artifact, Targets());

            Assert.IsNull(reason);
            Assert.AreEqual("tgt-repo", artifact.DefinitionReference.Definition.Id);
            Assert.AreEqual("tgt-project:tgt-repo", artifact.SourceId);
        }

        [TestMethod, TestCategory("L0")]
        public void Apply_GitArtifactWithRepositoryNameMap_PointsAtRenamedRepository()
        {
            var artifact = Artifact("Git", "src-repo", "app");
            var targets = Targets();
            targets.RepositoryNameMaps = new Dictionary<string, string> { ["app"] = "renamed" };

            var reason = ReleaseArtifactMap.Apply(artifact, targets);

            Assert.IsNull(reason);
            Assert.AreEqual("tgt-renamed", artifact.DefinitionReference.Definition.Id);
        }

        [TestMethod, TestCategory("L0")]
        public void Apply_ServiceConnectionArtifact_PointsAtTargetConnection()
        {
            var artifact = new Artifact
            {
                Type = "GitHub",
                Alias = "_gh",
                SourceId = "src-conn:org/repo",
                DefinitionReference = new DefinitionReference { Connection = new ArtifactSourceDefinitionUrl { Id = "src-conn", Name = "github" } }
            };

            var reason = ReleaseArtifactMap.Apply(artifact, Targets());

            Assert.IsNull(reason);
            Assert.AreEqual("tgt-conn", artifact.DefinitionReference.Connection.Id);
            Assert.AreEqual("tgt-conn:org/repo", artifact.SourceId);
        }

        [TestMethod, TestCategory("L0")]
        public void Apply_ServiceConnectionNotMigrated_ReportsWhy()
        {
            var artifact = new Artifact
            {
                Type = "Jenkins",
                DefinitionReference = new DefinitionReference { Connection = new ArtifactSourceDefinitionUrl { Id = "other-conn", Name = "jenkins" } }
            };

            var reason = ReleaseArtifactMap.Apply(artifact, Targets());

            StringAssert.Contains(reason, "service connection \"jenkins\" is not in the target");
        }

        [TestMethod, TestCategory("L0")]
        public void Apply_AzureArtifactsFeed_ReportsWhy()
        {
            var artifact = Artifact("PackageManagement", "feed", "feed");

            var reason = ReleaseArtifactMap.Apply(artifact, Targets());

            StringAssert.Contains(reason, "Azure Artifacts feed");
        }

        [TestMethod, TestCategory("L0")]
        public void Apply_Definition_ReportsFirstUnmappedArtifact()
        {
            var definition = new ReleaseDefinition { Artifacts = new[] { Artifact("Build", "21", "build"), Artifact("Build", "99", "missing") } };

            var unmapped = ReleaseArtifactMap.Apply(definition, Targets());

            Assert.IsNotNull(unmapped);
            Assert.AreEqual("missing", unmapped.Value.Artifact.DefinitionReference.Definition.Name);
        }

        [TestMethod, TestCategory("L0")]
        public void ResetObject_KeepsArtifactsWithUnknownFieldsAndDropsSourceUrl()
        {
            var json = @"{""id"":""1"",""name"":""r"",""environments"":[],""artifacts"":[{""type"":""Git"",""alias"":""_a"",
                ""definitionReference"":{""branches"":{""id"":""main"",""name"":""main""},""artifactSourceDefinitionUrl"":{""id"":""https://source""}}}]}";
            var definition = JsonConvert.DeserializeObject<ReleaseDefinition>(json);

            definition.ResetObject();
            var posted = JObject.FromObject(definition);

            var reference = posted["Artifacts"][0]["DefinitionReference"];
            Assert.AreEqual("main", (string)reference["branches"]["id"]);
            Assert.AreEqual(JTokenType.Null, reference["ArtifactSourceDefinitionUrl"].Type);
        }
    }
}
