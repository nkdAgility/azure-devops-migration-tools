using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tests.Tools.Infrastructure
{
    [TestClass]
    public class EmbededImageUrlReplacerTests
    {
        private const string SourceBare = "https://dev.azure.com/org/_apis/wit/attachments/1111";
        private const string SourceWithName = SourceBare + "?fileName=a.png";
        private const string TargetWithName = "https://dev.azure.com/target/_apis/wit/attachments/2222?fileName=a.png";

        [TestMethod, TestCategory("L0")]
        public void ShouldReplaceUrlCarryingFileNameWithTargetUrlOnce()
        {
            string result = EmbededImageUrlReplacer.ReplaceUrls(
                $"<img src=\"{SourceWithName}\">",
                new Dictionary<string, string> { [SourceWithName] = TargetWithName });

            Assert.AreEqual($"<img src=\"{TargetWithName}\">", result);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldNotReplaceShorterUrlInsideLongerOne()
        {
            // the bare url is a prefix of the named one. a sequential string.Replace of the bare url
            // turned the second image into {target}?fileName=a.png?fileName=a.png.
            string field = $"<img src=\"{SourceBare}\"><img src=\"{SourceWithName}\">";
            var replacements = new Dictionary<string, string>
            {
                [SourceBare] = TargetWithName,
                [SourceWithName] = TargetWithName,
            };

            string result = EmbededImageUrlReplacer.ReplaceUrls(field, replacements);

            Assert.AreEqual($"<img src=\"{TargetWithName}\"><img src=\"{TargetWithName}\">", result);        }

        [TestMethod, TestCategory("L0")]
        public void ShouldNotReplaceShorterUrlInsideLongerOneWhenOnlyShorterIsMapped()
        {
            string field = $"<img src=\"{SourceWithName}\">";

            string result = EmbededImageUrlReplacer.ReplaceUrls(field, new Dictionary<string, string> { [SourceBare] = TargetWithName });

            Assert.AreEqual(field, result);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldNotRewriteTextProducedByAnEarlierReplacement()
        {
            // the target url of the first entry contains the source url of the second.
            string first = "https://dev.azure.com/org/_apis/wit/attachments/1111?fileName=x.png";
            string chained = "https://dev.azure.com/org/_apis/wit/attachments/3333?fileName=x.png";
            var replacements = new Dictionary<string, string>
            {
                [first] = chained,
                [chained] = "https://dev.azure.com/org/_apis/wit/attachments/4444?fileName=x.png",
            };

            string result = EmbededImageUrlReplacer.ReplaceUrls($"<img src=\"{first}\">", replacements);

            Assert.AreEqual($"<img src=\"{chained}\">", result);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldReplaceEveryOccurrenceIncludingMarkdownAndPlainLinks()
        {
            string field = $"![a]({SourceWithName}) see {SourceWithName} <a href=\"{SourceWithName}\">x</a>";

            string result = EmbededImageUrlReplacer.ReplaceUrls(field, new Dictionary<string, string> { [SourceWithName] = TargetWithName });

            Assert.AreEqual($"![a]({TargetWithName}) see {TargetWithName} <a href=\"{TargetWithName}\">x</a>", result);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldReplaceHtmlEncodedUrl()
        {
            string encoded = SourceBare + "?fileName=a.png&amp;download=false";

            string result = EmbededImageUrlReplacer.ReplaceUrls($"<img src=\"{encoded}\">", new Dictionary<string, string> { [encoded] = TargetWithName });

            Assert.AreEqual($"<img src=\"{TargetWithName}\">", result);
        }

        [TestMethod, TestCategory("L0")]
        public void ShouldLeaveFieldUnchangedWhenUploadFailed()
        {
            string field = $"<img src=\"{SourceWithName}\">";

            Assert.AreEqual(field, EmbededImageUrlReplacer.ReplaceUrls(field, new Dictionary<string, string> { [SourceWithName] = null }));
            Assert.AreEqual(field, EmbededImageUrlReplacer.ReplaceUrls(field, new Dictionary<string, string>()));
            Assert.IsNull(EmbededImageUrlReplacer.ReplaceUrls(null, new Dictionary<string, string> { [SourceWithName] = TargetWithName }));
        }
    }
}
