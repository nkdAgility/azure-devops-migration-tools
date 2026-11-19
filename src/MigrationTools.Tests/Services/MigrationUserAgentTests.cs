using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Services;

namespace MigrationTools.Tests.Services
{
    [TestClass]
    public class MigrationUserAgentTests
    {
        [TestMethod, TestCategory("L0")]
        public void BuildValue_WithoutLabel_NamesToolVersionRunAndInfoUrl()
        {
            Assert.AreEqual("AzureDevOpsMigrationTools/16.2.1 (run abc123; +https://devopsmigration.io)",
                MigrationUserAgent.BuildValue("16.2.1", "abc123", null));
        }

        [TestMethod, TestCategory("L0")]
        public void BuildValue_WithLabel_PutsLabelAfterRunId()
        {
            Assert.AreEqual("AzureDevOpsMigrationTools/16.2.1-Preview.3 (run abc123; wave-1; +https://devopsmigration.io)",
                MigrationUserAgent.BuildValue("16.2.1-Preview.3", "abc123", "wave-1"));
        }

        [TestMethod, TestCategory("L0")]
        public void BuildValue_VersionWithSpacesOrSlashes_KeepsOnlyTokenCharacters()
        {
            Assert.AreEqual("AzureDevOpsMigrationTools/0.0.0-local (run r; +https://devopsmigration.io)",
                MigrationUserAgent.BuildValue("0.0.0 -local/", "r", null));
        }

        [TestMethod, TestCategory("L0")]
        public void BuildValue_EmptyVersion_FallsBackToZero()
        {
            StringAssert.StartsWith(MigrationUserAgent.BuildValue(null, "r", null), "AzureDevOpsMigrationTools/0.0.0 ");
        }

        [DataTestMethod, TestCategory("L0")]
        [DataRow(null, null)]
        [DataRow("", null)]
        [DataRow("   ", null)]
        [DataRow("wave-1", "wave-1")]
        [DataRow("Wave 1 dry run", "Wave-1-dry-run")]
        [DataRow("  padded  ", "padded")]
        [DataRow("a(b);c/d\\e\"f", "abcdef")]
        [DataRow("https://dev.azure.com/contoso", "httpsdev.azure.comcontoso")]
        [DataRow("((;))", null)]
        [DataRow("ünïcödé", "ncd")]
        public void SanitiseLabel_KeepsOnlyLettersDigitsDashDotUnderscore(string label, string expected)
        {
            Assert.AreEqual(expected, MigrationUserAgent.SanitiseLabel(label));
        }

        [TestMethod, TestCategory("L0")]
        public void SanitiseLabel_LongerThanLimit_IsCutToLimit()
        {
            var result = MigrationUserAgent.SanitiseLabel(new string('x', 100));
            Assert.AreEqual(MigrationUserAgent.MaxLabelLength, result.Length);
        }

        [TestMethod, TestCategory("L0")]
        public void SanitiseLabel_CutAtSeparator_DropsTrailingSeparator()
        {
            var result = MigrationUserAgent.SanitiseLabel(new string('x', MigrationUserAgent.MaxLabelLength - 1) + " yyyy");
            Assert.AreEqual(new string('x', MigrationUserAgent.MaxLabelLength - 1), result);
        }

        [TestMethod, TestCategory("L0")]
        public void RunId_IsTwelveHexCharactersAndStableForTheProcess()
        {
            Assert.IsTrue(Regex.IsMatch(MigrationUserAgent.RunId, "^[0-9a-f]{12}$"));
            Assert.AreEqual(MigrationUserAgent.RunId, MigrationUserAgent.RunId);
        }

        [TestMethod, TestCategory("L0")]
        public void Value_CarriesTheRunId()
        {
            StringAssert.Contains(MigrationUserAgent.Value, $"(run {MigrationUserAgent.RunId}");
        }

        [TestMethod, TestCategory("L0")]
        public void ProductValues_ParseAsAValidUserAgentHeader()
        {
            using var request = new HttpRequestMessage();
            foreach (var value in MigrationUserAgent.ProductValues)
            {
                request.Headers.UserAgent.Add(value);
            }
            Assert.AreEqual(MigrationUserAgent.Value, request.Headers.UserAgent.ToString());
        }

        [TestMethod, TestCategory("L0")]
        public void Apply_AddsProductAndCommentToTheClient()
        {
            using var client = new HttpClient();
            MigrationUserAgent.Apply(client);

            var values = client.DefaultRequestHeaders.UserAgent.ToList();
            Assert.AreEqual(2, values.Count);
            Assert.AreEqual(MigrationUserAgent.ProductName, values[0].Product.Name);
            StringAssert.Contains(values[1].Comment, MigrationUserAgent.RunId);
        }

        [TestMethod, TestCategory("L0")]
        public void Apply_Twice_DoesNotDuplicate()
        {
            using var client = new HttpClient();
            MigrationUserAgent.Apply(client);
            MigrationUserAgent.Apply(client);

            Assert.AreEqual(1, client.DefaultRequestHeaders.UserAgent.Count(p => p.Product?.Name == MigrationUserAgent.ProductName));
        }

        [TestMethod, TestCategory("L0")]
        public void Apply_KeepsAnExistingUserAgent()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Other/1.0");
            MigrationUserAgent.Apply(client);

            Assert.AreEqual("Other", client.DefaultRequestHeaders.UserAgent.First().Product.Name);
            Assert.AreEqual(3, client.DefaultRequestHeaders.UserAgent.Count);
        }
    }
}
