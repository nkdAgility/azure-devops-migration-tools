using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tools;

namespace MigrationTools.Tests.Tools
{
    [TestClass()]
    public class TfsGitRepositoryInfoTests
    {
        /// <summary>
        /// Helper method to create a mock ExternalLink using reflection to avoid complex TFS object model setup
        /// </summary>
        private ExternalLink CreateMockExternalLink(string uri)
        {
            // Try to create a mock RegisteredLinkType using reflection
            // First try the two-parameter constructor
            var linkTypeConstructor = typeof(RegisteredLinkType).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(string), typeof(string) },
                null);

            RegisteredLinkType linkType = null;
            
            if (linkTypeConstructor != null)
            {
                linkType = (RegisteredLinkType)linkTypeConstructor.Invoke(new object[] { "MockLinkType", "Mock Link Type" });
            }
            else
            {
                // Try alternative constructors if the two-parameter one doesn't exist
                var constructors = typeof(RegisteredLinkType).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                
                foreach (var ctor in constructors)
                {
                    var parameters = ctor.GetParameters();
                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                    {
                        linkType = (RegisteredLinkType)ctor.Invoke(new object[] { "MockLinkType" });
                        break;
                    }
                    else if (parameters.Length == 0)
                    {
                        linkType = (RegisteredLinkType)ctor.Invoke(new object[] { });
                        break;
                    }
                }
                
                // If still null, try to find any constructor with minimal parameters
                if (linkType == null && constructors.Length > 0)
                {
                    var simplestCtor = constructors[0];
                    var ctorParams = simplestCtor.GetParameters();
                    var args = new object[ctorParams.Length];
                    for (int i = 0; i < ctorParams.Length; i++)
                    {
                        args[i] = ctorParams[i].ParameterType.IsValueType ? Activator.CreateInstance(ctorParams[i].ParameterType) : null;
                    }
                    linkType = (RegisteredLinkType)simplestCtor.Invoke(args);
                }
            }

            if (linkType == null)
            {
                throw new InvalidOperationException("Could not create RegisteredLinkType instance via reflection. Available constructors may have changed.");
            }

            // Create ExternalLink using the mock link type
            return new ExternalLink(linkType, uri);
        }

        [TestMethod(), TestCategory("L0")]
        public void CreateFromGit_ValidLinkWithThreeParts_LegacyFormat_ShouldSucceed()
        {
            // Arrange - Legacy format with %2f encoding
            var validLink = "vstfs:///Git/Commit/25f94570-e3e7-4b79-ad19-4b434787fd5a%2f50477259-3058-4dff-ba4c-e8c179ec5327%2f41dd2754058348d72a6417c0615c2543b9b55535";
            var externalLink = CreateMockExternalLink(validLink);
            var possibleRepos = new List<GitRepository>
            {
                new GitRepository { Id = Guid.Parse("50477259-3058-4dff-ba4c-e8c179ec5327") }
            };

            // Act
            var result = TfsGitRepositoryInfo.CreateFromGit(externalLink, possibleRepos);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("50477259-3058-4dff-ba4c-e8c179ec5327", result.RepoID);
            Assert.AreEqual("41dd2754058348d72a6417c0615c2543b9b55535", result.CommitID);
        }

        [TestMethod(), TestCategory("L0")]
        public void CreateFromGit_ValidLinkWithMultipleCommitParts_LegacyFormat_ShouldSucceed()
        {
            // Arrange - Legacy format with extended commit ID
            var validLink = "vstfs:///Git/Commit/25f94570-e3e7-4b79-ad19-4b434787fd5a%2f50477259-3058-4dff-ba4c-e8c179ec5327%2f41dd2754058348d72a6417c0615c2543b9b55535%2fextra%2fparts";
            var externalLink = CreateMockExternalLink(validLink);
            var possibleRepos = new List<GitRepository>
            {
                new GitRepository { Id = Guid.Parse("50477259-3058-4dff-ba4c-e8c179ec5327") }
            };

            // Act
            var result = TfsGitRepositoryInfo.CreateFromGit(externalLink, possibleRepos);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("50477259-3058-4dff-ba4c-e8c179ec5327", result.RepoID);
            Assert.AreEqual("41dd2754058348d72a6417c0615c2543b9b55535%2fextra%2fparts", result.CommitID);
        }

        [TestMethod(), TestCategory("L0")]
        public void CreateFromGit_ValidLinkWithSlashes_NewFormat_ShouldSucceed()
        {
            // Arrange - New Azure DevOps format with forward slashes
            var newFormatLink = "vstfs:///Git/Commit/MyProject/MyRepo/41dd2754058348d72a6417c0615c2543b9b55535";
            var externalLink = CreateMockExternalLink(newFormatLink);
            var possibleRepos = new List<GitRepository>
            {
                new GitRepository 
                { 
                    Id = Guid.Parse("50477259-3058-4dff-ba4c-e8c179ec5327"),
                    Name = "MyRepo"
                }
            };

            // Act
            var result = TfsGitRepositoryInfo.CreateFromGit(externalLink, possibleRepos);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("50477259-3058-4dff-ba4c-e8c179ec5327", result.RepoID);
            Assert.AreEqual("41dd2754058348d72a6417c0615c2543b9b55535", result.CommitID);
            Assert.IsNotNull(result.GitRepo);
            Assert.AreEqual("MyRepo", result.GitRepo.Name);
        }

        [TestMethod(), TestCategory("L0")]
        public void CreateFromGit_InvalidLinkWithOnePart_LegacyFormat_ShouldReturnNull()
        {
            // Arrange - Legacy format with insufficient parts
            var invalidLink = "vstfs:///Git/Commit/25f94570-e3e7-4b79-ad19-4b434787fd5a";
            var externalLink = CreateMockExternalLink(invalidLink);
            var possibleRepos = new List<GitRepository>();

            // Act
            var result = TfsGitRepositoryInfo.CreateFromGit(externalLink, possibleRepos);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod(), TestCategory("L0")]
        public void CreateFromGit_InvalidLinkWithTwoParts_LegacyFormat_ShouldReturnNull()
        {
            // Arrange - Legacy format with only 2 parts (missing commitId)
            var invalidLink = "vstfs:///Git/Commit/25f94570-e3e7-4b79-ad19-4b434787fd5a%2f50477259-3058-4dff-ba4c-e8c179ec5327";
            var externalLink = CreateMockExternalLink(invalidLink);
            var possibleRepos = new List<GitRepository>();

            // Act
            var result = TfsGitRepositoryInfo.CreateFromGit(externalLink, possibleRepos);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod(), TestCategory("L0")]
        public void CreateFromGit_InvalidLinkWithTwoParts_NewFormat_ShouldReturnNull()
        {
            // Arrange - New format with insufficient parts
            var invalidLink = "vstfs:///Git/Commit/MyProject/MyRepo";
            var externalLink = CreateMockExternalLink(invalidLink);
            var possibleRepos = new List<GitRepository>();

            // Act
            var result = TfsGitRepositoryInfo.CreateFromGit(externalLink, possibleRepos);

            // Assert
            Assert.IsNull(result);
        }

        private static GitRepository Repo(string project, string name, string id)
        {
            return new GitRepository
            {
                Id = Guid.Parse(id),
                Name = name,
                ProjectReference = new Microsoft.TeamFoundation.Core.WebApi.TeamProjectReference { Name = project }
            };
        }

        [TestMethod(), TestCategory("L0")]
        public void CreateFromGit_NewFormat_SameRepoNameInTwoProjects_ShouldPickRepoFromLinkProject()
        {
            var link = CreateMockExternalLink("vstfs:///Git/Commit/ProjectB/Shared/41dd2754058348d72a6417c0615c2543b9b55535");
            var repos = new List<GitRepository>
            {
                Repo("ProjectA", "Shared", "11111111-1111-1111-1111-111111111111"),
                Repo("ProjectB", "Shared", "22222222-2222-2222-2222-222222222222"),
            };

            var result = TfsGitRepositoryInfo.CreateFromGit(link, repos);

            Assert.IsNotNull(result);
            Assert.AreEqual("22222222-2222-2222-2222-222222222222", result.RepoID);
        }

        [TestMethod(), TestCategory("L0")]
        public void CreateFromGit_NewFormat_AmbiguousRepoName_ShouldNotThrowAndReturnNoRepo()
        {
            var link = CreateMockExternalLink("vstfs:///Git/Commit/OtherProject/Shared/41dd2754058348d72a6417c0615c2543b9b55535");
            var repos = new List<GitRepository>
            {
                Repo("ProjectA", "Shared", "11111111-1111-1111-1111-111111111111"),
                Repo("ProjectB", "Shared", "22222222-2222-2222-2222-222222222222"),
            };

            var result = TfsGitRepositoryInfo.CreateFromGit(link, repos);

            Assert.IsNotNull(result);
            Assert.IsNull(result.GitRepo);
            Assert.IsNull(result.RepoID);
        }

        [TestMethod(), TestCategory("L0")]
        public void CreateFromGit_NewFormat_EncodedNames_ShouldMatch()
        {
            var link = CreateMockExternalLink("vstfs:///Git/Commit/My%20Project/My%20Repo/41dd2754058348d72a6417c0615c2543b9b55535");
            var repos = new List<GitRepository> { Repo("My Project", "My Repo", "33333333-3333-3333-3333-333333333333") };

            var result = TfsGitRepositoryInfo.CreateFromGit(link, repos);

            Assert.AreEqual("33333333-3333-3333-3333-333333333333", result.RepoID);
        }

        [TestMethod(), TestCategory("L0")]
        public void CreateFromGit_NewFormat_RefLink_ShouldKeepLegacyEncodingInId()
        {
            var link = CreateMockExternalLink("vstfs:///Git/Ref/Proj/Repo/GBfeature/x");
            var repos = new List<GitRepository> { Repo("Proj", "Repo", "44444444-4444-4444-4444-444444444444") };

            var result = TfsGitRepositoryInfo.CreateFromGit(link, repos);

            Assert.AreEqual("44444444-4444-4444-4444-444444444444", result.RepoID);
            Assert.AreEqual("GBfeature%2fx", result.CommitID);
        }

        [TestMethod(), TestCategory("L0")]
        public void CreateFromGit_LegacyFormat_UpperCaseEncoding_ShouldSucceed()
        {
            var link = CreateMockExternalLink("vstfs:///Git/Commit/25f94570-e3e7-4b79-ad19-4b434787fd5a%2F50477259-3058-4dff-ba4c-e8c179ec5327%2F41dd2754058348d72a6417c0615c2543b9b55535");
            var repos = new List<GitRepository> { Repo("P", "R", "50477259-3058-4dff-ba4c-e8c179ec5327") };

            var result = TfsGitRepositoryInfo.CreateFromGit(link, repos);

            Assert.IsNotNull(result.GitRepo);
            Assert.AreEqual("41dd2754058348d72a6417c0615c2543b9b55535", result.CommitID);
        }

        [TestMethod(), TestCategory("L0")]
        public void Create_GitLinkInNewFormat_ShouldRouteToGitParser()
        {
            var link = CreateMockExternalLink("vstfs:///Git/Commit/Proj/Repo/41dd2754058348d72a6417c0615c2543b9b55535");
            var repos = new List<GitRepository> { Repo("Proj", "Repo", "55555555-5555-5555-5555-555555555555") };

            var result = TfsGitRepositoryInfo.Create(link, repos, null, "Proj");

            Assert.AreEqual("55555555-5555-5555-5555-555555555555", result.RepoID);
        }

        [TestMethod(), TestCategory("L0")]
        public void CreateFromGit_EmptyLink_ShouldReturnNull()
        {
            // Arrange
            var emptyLink = "vstfs:///Git/Commit/";
            var externalLink = CreateMockExternalLink(emptyLink);
            var possibleRepos = new List<GitRepository>();

            // Act
            var result = TfsGitRepositoryInfo.CreateFromGit(externalLink, possibleRepos);

            // Assert
            Assert.IsNull(result);
        }
    }
}
