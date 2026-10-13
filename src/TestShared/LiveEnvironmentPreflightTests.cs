using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MigrationTools.Tests
{
    /// <summary>
    /// Checks the live test environment before anything else, so a credential or setup problem shows up as one
    /// clear, actionable failure instead of many unrelated-looking ones. Runs only when live tests are enabled.
    /// </summary>
    [TestClass]
    public class LiveEnvironmentPreflightTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory("L3"), Priority(0)]
        [RequiresAzureDevOps]
        public void LiveEnvironment_SourceProjectIsReachable() => CheckProject(TestingConstants.SourceCollection, "migrationSource1");

        [TestMethod, TestCategory("L3"), Priority(0)]
        [RequiresAzureDevOps]
        public void LiveEnvironment_TargetProjectIsReachable() => CheckProject(TestingConstants.TargetCollection, "migrationTarget1");

        private void CheckProject(Uri collection, string project)
        {
            TestContext?.WriteLine("Live test environment: " + TestingConstants.Describe());

            string token;
            try
            {
                token = TestingConstants.ResolveAccessToken();
            }
            catch (Exception ex)
            {
                Assert.Fail(Signpost(collection, $"Could not get a Microsoft Entra token ({ex.GetType().Name}).",
                    "Run 'az login' with an account that has access to the test orgs (or sign in to Visual Studio), " +
                    "or set MigrationTools__Tests__AccessToken to a PAT and unset MigrationTools__Tests__UseEntraId."));
                return;
            }

            HttpStatusCode status;
            using (var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + token)));
                var url = new Uri(collection, $"_apis/projects/{Uri.EscapeDataString(project)}?api-version=7.1");
                try
                {
                    using (var response = client.GetAsync(url).GetAwaiter().GetResult())
                        status = response.StatusCode;
                }
                catch (HttpRequestException ex)
                {
                    Assert.Fail(Signpost(collection, $"Could not reach the org ({ex.Message}).",
                        "Check the org URL in MigrationTools__Tests__SourceCollection / TargetCollection and your network."));
                    return;
                }
            }

            switch (status)
            {
                case HttpStatusCode.OK:
                    return;
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                case HttpStatusCode.Redirect: // Azure DevOps redirects to sign-in for an invalid credential
                case HttpStatusCode.NonAuthoritativeInformation:
                    Assert.Fail(Signpost(collection, $"The credential was rejected (HTTP {(int)status}).",
                        "The PAT may be expired or revoked, or the identity is not a member of this org. " +
                        "A stale AZDEVOPS_SYSTEM_TEST_PAT in your environment overrides other settings; remove it if you use Entra. " +
                        "For Entra, check that 'az account show' is the account added to the org."));
                    return;
                case HttpStatusCode.NotFound:
                    Assert.Fail(Signpost(collection, $"Project '{project}' was not found (or the org does not exist).",
                        "Run infra/test-env/Setup-TestEnvironment.ps1 -LocalOnly -SourceOrg <org> -TargetOrg <org> to create the test projects."));
                    return;
                default:
                    Assert.Fail(Signpost(collection, $"Unexpected HTTP {(int)status} from Azure DevOps.", "Retry; if it persists, check https://status.dev.azure.com."));
                    return;
            }
        }

        private static string Signpost(Uri collection, string problem, string fix)
            => $"LIVE TEST ENVIRONMENT PROBLEM at {collection}: {problem}{Environment.NewLine}" +
               $"  How to fix: {fix}{Environment.NewLine}" +
               $"  Configuration: {TestingConstants.Describe()}{Environment.NewLine}" +
               $"  Guide: {TestingConstants.TroubleshootingUrl}";
    }
}
