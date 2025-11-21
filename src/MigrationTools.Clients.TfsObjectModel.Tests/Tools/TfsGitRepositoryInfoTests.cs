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
            // Create a mock RegisteredLinkType using reflection
            var linkTypeConstructor = typeof(RegisteredLinkType).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { typeof(string), typeof(string) },
                null);

            var linkType = (RegisteredLinkType)linkTypeConstructor.Invoke(new object[] { "MockLinkType", "Mock Link Type" });

            // Create ExternalLink using the mock link type
            return new ExternalLink(linkType, uri);
        }

        [TestMethod(), TestCategory("L0")]
        public void CreateFromGit_ValidLinkWithThreeParts_ShouldSucceed()
        {
            // Arrange
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
        public void CreateFromGit_ValidLinkWithMultipleCommitParts_ShouldSucceed()
        {
            // Arrange
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
        public void CreateFromGit_InvalidLinkWithOnePart_ShouldReturnNull()
        {
            // Arrange
            var invalidLink = "vstfs:///Git/Commit/25f94570-e3e7-4b79-ad19-4b434787fd5a";
            var externalLink = CreateMockExternalLink(invalidLink);
            var possibleRepos = new List<GitRepository>();

            // Act
            var result = TfsGitRepositoryInfo.CreateFromGit(externalLink, possibleRepos);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod(), TestCategory("L0")]
        public void CreateFromGit_InvalidLinkWithTwoParts_ShouldReturnNull()
        {
            // Arrange
            var invalidLink = "vstfs:///Git/Commit/25f94570-e3e7-4b79-ad19-4b434787fd5a%2f50477259-3058-4dff-ba4c-e8c179ec5327";
            var externalLink = CreateMockExternalLink(invalidLink);
            var possibleRepos = new List<GitRepository>();

            // Act
            var result = TfsGitRepositoryInfo.CreateFromGit(externalLink, possibleRepos);

            // Assert
            Assert.IsNull(result);
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
