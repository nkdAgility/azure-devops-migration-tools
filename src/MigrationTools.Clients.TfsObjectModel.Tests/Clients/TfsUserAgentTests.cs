using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Clients;
using MigrationTools.Services;

namespace MigrationTools.Clients.TfsObjectModel.Tests.Clients
{
    [TestClass]
    [DoNotParallelize]
    public class TfsUserAgentTests
    {
        [TestMethod, TestCategory("L0")]
        public void Install_Rest_PutsTheToolFirstAndKeepsTheLibraryValues()
        {
            TfsUserAgent.Install();

            var userAgent = VssClientHttpRequestSettings.Default.UserAgent;
            Assert.AreEqual(MigrationUserAgent.ProductName, userAgent[0].Product.Name);
            StringAssert.Contains(userAgent[1].Comment, MigrationUserAgent.RunId);
            Assert.IsTrue(userAgent.Count > 2, "the client library's own product values should stay after ours");
            StringAssert.StartsWith(TfsUserAgent.Rest, MigrationUserAgent.Value);
        }

        [TestMethod, TestCategory("L0")]
        public void Install_Soap_PutsTheToolBeforeTheLibraryValue()
        {
            TfsUserAgent.Install();

            StringAssert.StartsWith(TfsConnection.ApplicationName, MigrationUserAgent.Value + " ");
            Assert.IsTrue(TfsConnection.ApplicationName.Length > MigrationUserAgent.Value.Length + 1, "the library's SOAP user agent should stay after ours");
        }

        [TestMethod, TestCategory("L0")]
        public void Install_Twice_DoesNotDuplicate()
        {
            TfsUserAgent.Install();
            TfsUserAgent.Install();

            var rest = VssClientHttpRequestSettings.Default.UserAgent;
            Assert.AreEqual(1, rest.Count(p => p.Product?.Name == MigrationUserAgent.ProductName));
            Assert.AreEqual(1, rest.Count(p => p.Comment != null && p.Comment.Contains(MigrationUserAgent.InfoUrl)));
            Assert.AreEqual(1, Regex.Matches(TfsUserAgent.Soap, Regex.Escape(MigrationUserAgent.ProductName + "/")).Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Install_AfterTheLabelChanges_ReplacesTheOldValue()
        {
            var version = MigrationUserAgent.Version;
            var label = MigrationUserAgent.Label;
            try
            {
                MigrationUserAgent.Configure("1.2.3", "first");
                TfsUserAgent.Install();
                MigrationUserAgent.Configure("1.2.3", "second");
                TfsUserAgent.Install();

                StringAssert.Contains(TfsUserAgent.Soap, "second");
                Assert.IsFalse(TfsUserAgent.Soap.Contains("first"));
                StringAssert.Contains(TfsUserAgent.Rest, "second");
                Assert.IsFalse(TfsUserAgent.Rest.Contains("first"));
            }
            finally
            {
                MigrationUserAgent.Configure(version, label);
                TfsUserAgent.Install();
            }
        }
    }
}
