using System.Linq;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Clients;
using MigrationTools.Services;

namespace MigrationTools.Clients.TfsObjectModel.Tests.Clients
{
    [TestClass]
    public class TfsUserAgentTests
    {
        [TestMethod, TestCategory("L0")]
        public void Install_PutsTheToolFirstAndKeepsTheLibraryValues()
        {
            TfsUserAgent.Install();

            var userAgent = VssClientHttpRequestSettings.Default.UserAgent;
            Assert.AreEqual(MigrationUserAgent.ProductName, userAgent[0].Product.Name);
            StringAssert.Contains(userAgent[1].Comment, MigrationUserAgent.RunId);
            Assert.IsTrue(userAgent.Count > 2, "the client library's own product values should stay after ours");
        }

        [TestMethod, TestCategory("L0")]
        public void Install_Twice_DoesNotDuplicate()
        {
            TfsUserAgent.Install();
            TfsUserAgent.Install();

            var userAgent = VssClientHttpRequestSettings.Default.UserAgent;
            Assert.AreEqual(1, userAgent.Count(p => p.Product?.Name == MigrationUserAgent.ProductName));
            Assert.AreEqual(1, userAgent.Count(p => p.Comment != null && p.Comment.Contains(MigrationUserAgent.InfoUrl)));
        }

        [TestMethod, TestCategory("L0")]
        public void Current_StartsWithTheToolValue()
        {
            TfsUserAgent.Install();

            StringAssert.StartsWith(TfsUserAgent.Current, MigrationUserAgent.Value);
        }
    }
}
