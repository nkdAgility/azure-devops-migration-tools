using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tests.Tools.Infrastructure
{
    [TestClass]
    public class EmbededImageUrlExtractorTests
    {
        private const string SourceUrl = "https://dev.azure.com/source/_apis/wit/attachments/1234?FileName=screenshot.png";
        private const string OtherSourceUrl = "https://dev.azure.com/source/_apis/wit/attachments/5678?FileName=diagram.png";

        [TestMethod, TestCategory("L0")]
        public void ShouldExtractUrlFromHtmlImage()
        {
            var urls = EmbededImageUrlExtractor.ExtractImageUrls($"<div><img src=\"{SourceUrl}\" alt=\"a picture\"></div>");

            CollectionAssert.AreEqual(new[] { SourceUrl }, urls.ToArray());
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldExtractUrlFromMarkdownImage()
        {
            var urls = EmbededImageUrlExtractor.ExtractImageUrls($"Some text ![a picture]({SourceUrl}) more text");

            CollectionAssert.AreEqual(new[] { SourceUrl }, urls.ToArray());
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldNotCaptureMarkdownAltText()
        {
            var urls = EmbededImageUrlExtractor.ExtractImageUrls($"![{OtherSourceUrl}]({SourceUrl})");

            CollectionAssert.AreEqual(new[] { SourceUrl }, urls.ToArray());
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldExtractUrlFromMarkdownImageWithTitle()
        {
            var urls = EmbededImageUrlExtractor.ExtractImageUrls($"![a picture]({SourceUrl} \"The title\")");

            CollectionAssert.AreEqual(new[] { SourceUrl }, urls.ToArray());
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldExtractUrlFromMarkdownImageWithAngleBrackets()
        {
            var urls = EmbededImageUrlExtractor.ExtractImageUrls($"![a picture](<{SourceUrl}>)");

            CollectionAssert.AreEqual(new[] { SourceUrl }, urls.ToArray());
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldExtractUrlFromMarkdownImageWithSingleQuotedTitle()
        {
            var urls = EmbededImageUrlExtractor.ExtractImageUrls($"![a picture]({SourceUrl} 'The title')");

            CollectionAssert.AreEqual(new[] { SourceUrl }, urls.ToArray());
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldExtractUrlContainingBalancedParentheses()
        {
            const string urlWithParens = "https://dev.azure.com/source/_apis/wit/attachments/1234?FileName=image_(1).png";
            var urls = EmbededImageUrlExtractor.ExtractImageUrls($"![a picture]({urlWithParens})");

            CollectionAssert.AreEqual(new[] { urlWithParens }, urls.ToArray());
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldExtractUrlFromMarkdownImageWithEmptyAltText()
        {
            var urls = EmbededImageUrlExtractor.ExtractImageUrls($"![]({SourceUrl})");

            CollectionAssert.AreEqual(new[] { SourceUrl }, urls.ToArray());
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldIgnoreMarkdownLinksThatAreNotImages()
        {
            var urls = EmbededImageUrlExtractor.ExtractImageUrls($"[just a link]({SourceUrl})");

            Assert.IsEmpty(urls);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldExtractBothHtmlAndMarkdownFromTheSameField()
        {
            var urls = EmbededImageUrlExtractor.ExtractImageUrls(
                $"<img src=\"{SourceUrl}\"> and ![a picture]({OtherSourceUrl})");

            CollectionAssert.AreEquivalent(new[] { SourceUrl, OtherSourceUrl }, urls.ToArray());
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldReturnTheSameUrlOnlyOnceWhenRepeated()
        {
            var urls = EmbededImageUrlExtractor.ExtractImageUrls(
                $"<img src=\"{SourceUrl}\"> and again ![a picture]({SourceUrl})");

            CollectionAssert.AreEqual(new[] { SourceUrl }, urls.ToArray());
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldReturnEmptyForNullOrEmptyFieldValue()
        {
            Assert.IsEmpty(EmbededImageUrlExtractor.ExtractImageUrls(null));
            Assert.IsEmpty(EmbededImageUrlExtractor.ExtractImageUrls(string.Empty));
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldReturnEmptyWhenThereAreNoImages()
        {
            Assert.IsEmpty(EmbededImageUrlExtractor.ExtractImageUrls("Just some plain text with no images."));
        }

        [TestMethod, TestCategory("L0")]
        public void ReplacingTheExtractedUrlShouldPreserveMarkdownAltText()
        {
            const string newUrl = "https://dev.azure.com/target/_apis/wit/attachments/9999?FileName=screenshot.png";
            string fieldValue = $"![a picture]({SourceUrl})";

            var urls = EmbededImageUrlExtractor.ExtractImageUrls(fieldValue);
            foreach (var url in urls)
            {
                fieldValue = fieldValue.Replace(url, newUrl);
            }

            Assert.AreEqual($"![a picture]({newUrl})", fieldValue);
        }

        [TestMethod, TestCategory("L0")]
        public void ReplacingTheExtractedUrlShouldPreserveHtmlAttributes()
        {
            const string newUrl = "https://dev.azure.com/target/_apis/wit/attachments/9999?FileName=screenshot.png";
            string fieldValue = $"<img src=\"{SourceUrl}\" alt=\"a picture\" width=\"200\">";

            var urls = EmbededImageUrlExtractor.ExtractImageUrls(fieldValue);
            foreach (var url in urls)
            {
                fieldValue = fieldValue.Replace(url, newUrl);
            }

            Assert.AreEqual($"<img src=\"{newUrl}\" alt=\"a picture\" width=\"200\">", fieldValue);
        }
    }
}
