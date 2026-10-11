using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tests.Tools.Infrastructure
{
    [TestClass]
    public class EmbededImageFileNameExtractorTests
    {
        [TestMethod, TestCategory("L0")]
        public void ShouldReturnFileNameFromSimpleUrl()
        {
            var fileName = EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName=screenshot.png");

            Assert.AreEqual("screenshot.png", fileName);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldMatchTheParameterNameCaseInsensitively()
        {
            var fileName = EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?fileName=screenshot.png");

            Assert.AreEqual("screenshot.png", fileName);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldDecodePercentEncodedSpaces()
        {
            var fileName = EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName=my%20image.png");

            Assert.AreEqual("my image.png", fileName);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldDecodePercentEncodedBracketsAndAmpersands()
        {
            var fileName = EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName=image%20%281%29%20%26%20more.png");

            Assert.AreEqual("image (1) & more.png", fileName);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldDecodeNonAsciiFileNames()
        {
            var fileName = EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName=%C3%A9l%C3%A9phant.png");

            Assert.AreEqual("éléphant.png", fileName);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldStopAtTheNextQueryParameter()
        {
            var fileName = EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "http://tfs:8080/tfs/WorkItemTracking/v1.0/AttachFileHandler.ashx?FileName=screenshot.png&ContentType=image/png");

            Assert.AreEqual("screenshot.png", fileName);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldHandleHtmlEncodedQuerySeparators()
        {
            var fileName = EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "http://tfs:8080/tfs/WorkItemTracking/v1.0/AttachFileHandler.ashx?FileID=99&amp;FileName=screenshot.png&amp;ContentType=image/png");

            Assert.AreEqual("screenshot.png", fileName);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldNotMistakeFileNameGuidForFileName()
        {
            var fileName = EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "http://tfs:8080/tfs/WorkItemTracking/v1.0/AttachFileHandler.ashx?FileNameGuid=abc-123&FileName=screenshot.png");

            Assert.AreEqual("screenshot.png", fileName);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldKeepEqualsSignsInsideTheValue()
        {
            var fileName = EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName=report%3Dfinal.png");

            Assert.AreEqual("report=final.png", fileName);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldStripDirectorySegmentsRevealedByDecoding()
        {
            var fileName = EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName=folder%2Fscreenshot.png");

            Assert.AreEqual("screenshot.png", fileName);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldStripTraversalSegmentsRevealedByDecoding()
        {
            var fileName = EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName=..%2F..%2Fevil.png");

            Assert.AreEqual("evil.png", fileName);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldStripBackslashSegmentsRevealedByDecoding()
        {
            var fileName = EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName=..%5C..%5Cevil.png");

            Assert.AreEqual("evil.png", fileName);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldReturnNullWhenTheNameIsOnlyTraversal()
        {
            var fileName = EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName=..%2F..%2F");

            Assert.IsNull(fileName);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldReturnNullWhenThereIsNoFileNameParameter()
        {
            Assert.IsNull(EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234"));
            Assert.IsNull(EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?ContentType=image/png"));
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldReturnNullWhenTheFileNameParameterIsEmpty()
        {
            Assert.IsNull(EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName="));
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldReturnNullForNullOrEmptyUrl()
        {
            Assert.IsNull(EmbededImageFileNameExtractor.GetFileNameFromUrl(null));
            Assert.IsNull(EmbededImageFileNameExtractor.GetFileNameFromUrl(string.Empty));
            Assert.IsNull(EmbededImageFileNameExtractor.GetFileNameFromUrl("   "));
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldRejectWindowsReservedDeviceNames()
        {
            Assert.IsNull(EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName=CON.png"));
            Assert.IsNull(EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName=nul"));
            Assert.IsNull(EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName=com1.jpg"));
            Assert.IsNull(EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName=LPT9.gif"));
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldNotRejectNamesThatMerelyStartWithAReservedName()
        {
            Assert.AreEqual("console.png", EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName=console.png"));
            Assert.AreEqual("aux_data.png", EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName=aux_data.png"));
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldIgnoreAFragmentFollowingTheFileName()
        {
            var fileName = EmbededImageFileNameExtractor.GetFileNameFromUrl(
                "https://dev.azure.com/org/_apis/wit/attachments/1234?FileName=screenshot.png#anchor");

            Assert.AreEqual("screenshot.png", fileName);
        }
    }
}
