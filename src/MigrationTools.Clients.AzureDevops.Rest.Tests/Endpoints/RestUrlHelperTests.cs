using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MigrationTools.Endpoints.Tests
{
    [TestClass]
    public class RestUrlHelperTests
    {
        [TestMethod, TestCategory("L0")]
        [DataRow("Custom.Field#1", "Custom.Field%231")]
        [DataRow("Custom.My Field", "Custom.My%20Field")]
        [DataRow("Custom.100%Done", "Custom.100%25Done")]
        [DataRow("Custom.A?B", "Custom.A%3FB")]
        [DataRow("Custom.A/B", "Custom.A%2FB")]
        [DataRow("Custom.A&B=C", "Custom.A%26B%3DC")]
        [DataRow("Custom.A{0}", "Custom.A%7B0%7D")]
        [DataRow("System.Title", "System.Title")]
        public void EscapeSegment_EncodesReservedCharacters(string input, string expected)
        {
            Assert.AreEqual(expected, RestUrlHelper.EscapeSegment(input));
        }

        [TestMethod, TestCategory("L0")]
        public void EscapeSegment_NullIsEmpty()
        {
            Assert.AreEqual(string.Empty, RestUrlHelper.EscapeSegment(null));
        }

        [TestMethod, TestCategory("L0")]
        public void AppendSegment_EncodesFieldWithHash()
        {
            var url = RestUrlHelper.AppendSegment("https://dev.azure.com/org/_apis/work/processes/p1/workItemTypes/Custom.Bug/fields", "Custom.Ticket#");
            Assert.AreEqual("https://dev.azure.com/org/_apis/work/processes/p1/workItemTypes/Custom.Bug/fields/Custom.Ticket%23", url);
            var uri = new Uri(url);
            Assert.AreEqual(string.Empty, uri.Fragment, "'#' must not start a fragment");
            StringAssert.EndsWith(uri.AbsoluteUri, "/fields/Custom.Ticket%23");
        }

        [TestMethod, TestCategory("L0")]
        public void AppendSegment_DoesNotDuplicateSlash()
        {
            Assert.AreEqual("https://x/a/b%20c", RestUrlHelper.AppendSegment("https://x/a/", "b c"));
        }

        [TestMethod, TestCategory("L0")]
        public void FormatRoute_EncodesEachParameter()
        {
            var path = RestUrlHelper.FormatRoute("/_apis/work/processes/{0}/workItemTypes/{1}/fields", "p 1", "Custom.Bug#2");
            Assert.AreEqual("/_apis/work/processes/p%201/workItemTypes/Custom.Bug%232/fields", path);
        }

        [TestMethod, TestCategory("L0")]
        public void FormatRoute_SurvivesUriBuilderRoundTrip()
        {
            // Mirrors how AzureDevOpsEndpoint builds the base address.
            var builder = new UriBuilder("https://dev.azure.com/org/");
            builder.Path += RestUrlHelper.FormatRoute("_apis/work/processes/{0}/workItemTypes/{1}/fields", "p1", "Custom.A#B 100%");
            builder.Query = "api-version=6.0";
            var url = builder.Uri.AbsoluteUri.Replace(builder.Query, "");
            var uri = new Uri(url);
            Assert.AreEqual(string.Empty, uri.Fragment);
            StringAssert.Contains(uri.AbsoluteUri, "/workItemTypes/Custom.A%23B%20100%25/fields");
        }
    }
}
