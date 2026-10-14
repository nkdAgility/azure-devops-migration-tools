using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MigrationTools.Endpoints.Tests
{
    [TestClass]
    public class EntraIdCredentialsTests
    {
        private sealed class CountingTokenCredential : TokenCredential
        {
            public int Calls;
            public readonly List<string[]> Scopes = new List<string[]>();

            private AccessToken Next(TokenRequestContext context)
            {
                Scopes.Add(context.Scopes);
                var n = Interlocked.Increment(ref Calls);
                return new AccessToken("token-" + n, DateTimeOffset.UtcNow.AddHours(1));
            }

            public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken) => Next(requestContext);

            public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
                => new ValueTask<AccessToken>(Next(requestContext));
        }

        private sealed class FakeResponse : IHttpResponse
        {
            public FakeResponse(HttpStatusCode status) { StatusCode = status; }
            public IHttpHeaders Headers => null;
            public HttpStatusCode StatusCode { get; }
        }

        private CountingTokenCredential _fake;

        [TestInitialize]
        public void Setup()
        {
            _fake = new CountingTokenCredential();
            EntraIdCredentials.TokenCredential = _fake;
        }

        [TestCleanup]
        public void Cleanup() => EntraIdCredentials.TokenCredential = null;

        [TestMethod, TestCategory("L0")]
        public void GetAccessToken_RequestsTheAzureDevOpsScope()
        {
            Assert.AreEqual("token-1", EntraIdCredentials.GetAccessToken());
            CollectionAssert.AreEqual(new[] { EntraIdCredentials.AzureDevOpsScope }, _fake.Scopes[0]);
        }

        [TestMethod, TestCategory("L0")]
        public void GetAccessToken_ReusesAnUnexpiredToken()
        {
            EntraIdCredentials.GetAccessToken();
            Assert.AreEqual("token-1", EntraIdCredentials.GetAccessToken());
            Assert.AreEqual(1, _fake.Calls);
        }

        private sealed class SlowTokenCredential : TokenCredential
        {
            public int Calls;
            public readonly TaskCompletionSource<bool> Release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            {
                var n = Interlocked.Increment(ref Calls);
                await Release.Task;
                return new AccessToken("slow-" + n, DateTimeOffset.UtcNow.AddHours(1));
            }
        }

        [TestMethod, TestCategory("L0")]
        public async Task GetFreshAccessTokenAsync_ConcurrentCallersShareOneFetch()
        {
            var slow = new SlowTokenCredential();
            EntraIdCredentials.TokenCredential = slow;

            var first = EntraIdCredentials.GetFreshAccessTokenAsync();
            var second = EntraIdCredentials.GetFreshAccessTokenAsync();
            slow.Release.SetResult(true);

            CollectionAssert.AreEqual(new[] { "slow-1", "slow-1" }, await Task.WhenAll(first, second));
            Assert.AreEqual(1, slow.Calls);
        }

        [TestMethod, TestCategory("L0")]
        public async Task GetFreshAccessTokenAsync_CallerCanStopWaitingWithoutCancellingOthers()
        {
            var slow = new SlowTokenCredential();
            EntraIdCredentials.TokenCredential = slow;
            using (var cts = new CancellationTokenSource())
            {
                var cancelledCaller = EntraIdCredentials.GetFreshAccessTokenAsync(cts.Token);
                var patientCaller = EntraIdCredentials.GetFreshAccessTokenAsync();
                cts.Cancel();

                await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => cancelledCaller);
                // Arrives after a caller gave up but before the fetch finishes: must join it, not start another.
                var lateCaller = EntraIdCredentials.GetFreshAccessTokenAsync();
                slow.Release.SetResult(true);
                Assert.AreEqual("slow-1", await patientCaller);
                Assert.AreEqual("slow-1", await lateCaller);
                Assert.AreEqual(1, slow.Calls);
            }
        }

        [TestMethod, TestCategory("L0")]
        public void FederatedCredential_TreatsUnauthorizedAsAChallenge()
        {
            var credential = new EntraIdCredentials.EntraIdFederatedCredential();

            Assert.IsTrue(credential.IsAuthenticationChallenge(new FakeResponse(HttpStatusCode.Unauthorized)));
            Assert.IsFalse(credential.IsAuthenticationChallenge(new FakeResponse(HttpStatusCode.OK)));
            Assert.AreEqual(VssCredentialsType.OAuth, credential.CredentialType);
        }

        [TestMethod, TestCategory("L0")]
        public async Task TokenProvider_FetchesAFreshTokenAfterTheServerRejectsTheCurrentOne()
        {
            var credential = new EntraIdCredentials.EntraIdFederatedCredential();
            Assert.AreEqual(1, _fake.Calls, "The credential starts with one token.");
            var initial = new VssOAuthAccessToken("token-1");

            var provider = new EntraIdCredentials.EntraIdTokenProvider(credential, new Uri("https://dev.azure.com/example/"));
            Assert.IsFalse(provider.GetTokenIsInteractive);

            // Simulates the client library reacting to a 401 for the expired token.
            var refreshed = await provider.GetTokenAsync(initial, CancellationToken.None);

            Assert.IsInstanceOfType(refreshed, typeof(VssOAuthAccessToken));
            Assert.AreEqual("token-2", ((VssOAuthAccessToken)refreshed).Value);
            Assert.AreEqual(2, _fake.Calls);
        }

        private static string Jwt(string payloadJson)
        {
            string B64(string s) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(s)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            return B64("{\"alg\":\"none\"}") + "." + B64(payloadJson) + ".signature-secret";
        }

        [TestMethod, TestCategory("L0")]
        public void DescribeToken_ReportsIdentityAndValidityButNeverTheToken()
        {
            var jwt = Jwt("{\"upn\":\"user@example.com\",\"tid\":\"tenant-1\",\"aud\":\"499b84ac\",\"nbf\":1700000000,\"exp\":1700003600}");

            var description = EntraIdCredentials.DescribeToken(jwt, DateTimeOffset.UtcNow, "AzureCliCredential");

            StringAssert.Contains(description, "AzureCliCredential");
            StringAssert.Contains(description, "user@example.com");
            StringAssert.Contains(description, "tenant-1");
            StringAssert.Contains(description, "2023-11-14 22:13:20Z");
            Assert.IsFalse(description.Contains("signature-secret"));
            Assert.IsFalse(description.Contains(jwt.Split('.')[1]));
        }

        [TestMethod, TestCategory("L0")]
        public void DescribeToken_ShowsWhenTheCredentialIsPinned()
        {
            var jwt = Jwt("{\"upn\":\"user@example.com\"}");

            var description = EntraIdCredentials.DescribeToken(jwt, DateTimeOffset.UtcNow, "AzureCliCredential", "AzureCliCredential");

            StringAssert.StartsWith(description, "AzureCliCredential (AZURE_TOKEN_CREDENTIALS=AzureCliCredential)");
        }

        private sealed class UnavailableTokenCredential : TokenCredential
        {
            public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
                => throw new Azure.Identity.CredentialUnavailableException("No credential is signed in.");

            public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
                => throw new Azure.Identity.CredentialUnavailableException("No credential is signed in.");
        }

        [TestMethod, TestCategory("L0")]
        public void GetAccessToken_StopsTheRunWithAReadableReasonWhenNoCredentialIsAvailable()
        {
            EntraIdCredentials.TokenCredential = new UnavailableTokenCredential();

            var ex = Assert.ThrowsExactly<MigrationTools.Exceptions.MigrationRunStoppedException>(() => EntraIdCredentials.GetAccessToken());

            StringAssert.StartsWith(ex.Message, "Could not get a Microsoft Entra ID token: No credential is signed in.");
            StringAssert.Contains(ex.Message, "az login");
            Assert.IsInstanceOfType(ex.InnerException, typeof(Azure.Identity.CredentialUnavailableException));
        }

        [TestMethod, TestCategory("L0")]
        public async Task GetFreshAccessTokenAsync_StopsTheRunWithAReadableReasonWhenNoCredentialIsAvailable()
        {
            EntraIdCredentials.TokenCredential = new UnavailableTokenCredential();

            await Assert.ThrowsExactlyAsync<MigrationTools.Exceptions.MigrationRunStoppedException>(() => EntraIdCredentials.GetFreshAccessTokenAsync());
        }

        private sealed class BuggyTokenCredential : TokenCredential
        {
            public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
                => throw new InvalidOperationException("Collection was modified.");

            public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
                => throw new InvalidOperationException("Collection was modified.");
        }

        [TestMethod, TestCategory("L0")]
        public void GetAccessToken_DoesNotTurnUnrelatedErrorsIntoASignInStop()
        {
            EntraIdCredentials.TokenCredential = new BuggyTokenCredential();

            Assert.ThrowsExactly<InvalidOperationException>(() => EntraIdCredentials.GetAccessToken());
        }

        [TestMethod, TestCategory("L0")]
        public void IsCredentialFailure_AcceptsOnlyTheTokenCredentialsVariableRejection()
        {
            var saved = Environment.GetEnvironmentVariable(EntraIdCredentials.TokenCredentialsVariable);
            try
            {
                EntraIdCredentials.TokenCredential = null;
                var rejected = new InvalidOperationException("Invalid value for environment variable AZURE_TOKEN_CREDENTIALS: notacredential.");
                var unrelated = new InvalidOperationException("Collection was modified.");

                Environment.SetEnvironmentVariable(EntraIdCredentials.TokenCredentialsVariable, null);
                Assert.IsFalse(EntraIdCredentials.IsCredentialFailure(rejected), "Not a pinning failure when the variable is unset.");

                Environment.SetEnvironmentVariable(EntraIdCredentials.TokenCredentialsVariable, "NotACredential");
                Assert.IsTrue(EntraIdCredentials.IsCredentialFailure(rejected));
                Assert.IsFalse(EntraIdCredentials.IsCredentialFailure(unrelated));
                Assert.IsTrue(EntraIdCredentials.IsCredentialFailure(new Azure.Identity.CredentialUnavailableException("none")));
            }
            finally
            {
                Environment.SetEnvironmentVariable(EntraIdCredentials.TokenCredentialsVariable, saved);
            }
        }

        [TestMethod, TestCategory("L0")]
        public void CredentialFailure_NamesThePinnedCredential()
        {
            var saved = Environment.GetEnvironmentVariable(EntraIdCredentials.TokenCredentialsVariable);
            try
            {
                EntraIdCredentials.TokenCredential = null;
                Environment.SetEnvironmentVariable(EntraIdCredentials.TokenCredentialsVariable, "NotACredential");

                var ex = EntraIdCredentials.CredentialFailure(new InvalidOperationException("Invalid value for environment variable AZURE_TOKEN_CREDENTIALS: notacredential."));

                StringAssert.Contains(ex.Message, "AZURE_TOKEN_CREDENTIALS is set to 'NotACredential'");
            }
            finally
            {
                Environment.SetEnvironmentVariable(EntraIdCredentials.TokenCredentialsVariable, saved);
            }
        }

        [TestMethod, TestCategory("L0")]
        public void DescribeToken_HandlesATokenThatIsNotAJwt()
        {
            var description = EntraIdCredentials.DescribeToken("opaque-secret", DateTimeOffset.UtcNow, null);

            StringAssert.Contains(description, "not a readable JWT");
            Assert.IsFalse(description.Contains("opaque-secret"));
        }

        [TestMethod, TestCategory("L0")]
        public void Reset_MakesTheNextRequestAskTheCredentialAgain()
        {
            Assert.AreEqual("token-1", EntraIdCredentials.GetAccessToken());
            Assert.AreEqual("token-1", EntraIdCredentials.GetAccessToken());

            EntraIdCredentials.Reset();

            Assert.AreEqual("token-2", EntraIdCredentials.GetAccessToken());
        }

        private sealed class GatedTokenCredential : TokenCredential
        {
            public readonly TaskCompletionSource<AccessToken> Gate = new TaskCompletionSource<AccessToken>(TaskCreationOptions.RunContinuationsAsynchronously);
            public int SyncCalls;

            public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
                => new AccessToken("sync-" + Interlocked.Increment(ref SyncCalls), DateTimeOffset.UtcNow.AddHours(1));

            public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
                => new ValueTask<AccessToken>(Gate.Task);
        }

        [TestMethod, TestCategory("L0")]
        public async Task Reset_StopsAFetchStartedBeforeItFromPublishingItsToken()
        {
            var gated = new GatedTokenCredential();
            EntraIdCredentials.TokenCredential = gated;

            var stale = EntraIdCredentials.GetFreshAccessTokenAsync();
            EntraIdCredentials.Reset();
            gated.Gate.SetResult(new AccessToken("stale", DateTimeOffset.UtcNow.AddHours(2)));
            Assert.AreEqual("stale", await stale, "The caller that started the fetch still gets its result.");

            Assert.AreEqual("sync-1", EntraIdCredentials.GetAccessToken(), "The pre-reset token must not be cached.");
        }

        [TestMethod, TestCategory("L0")]
        public void ConnectWithRetry_RetriesARejectedConnectionWithANewToken()
        {
            var attempts = new List<string>();
            var waits = new List<TimeSpan>();
            var retried = new List<int>();

            var result = EntraIdCredentials.ConnectWithRetry(() =>
            {
                attempts.Add(EntraIdCredentials.GetAccessToken());
                if (attempts.Count == 1) { throw new UnauthorizedAccessException("401"); }
                return "connected";
            }, ex => ex is UnauthorizedAccessException, 2, TimeSpan.FromSeconds(3), (ex, attempt, wait) => retried.Add(attempt), waits.Add);

            Assert.AreEqual("connected", result);
            CollectionAssert.AreEqual(new[] { "token-1", "token-2" }, attempts, "The retry must use a newly acquired token.");
            CollectionAssert.AreEqual(new[] { 2 }, retried);
            CollectionAssert.AreEqual(new[] { TimeSpan.FromSeconds(3) }, waits);
        }

        [TestMethod, TestCategory("L0")]
        public void ConnectWithRetry_GivesUpAfterTheRetryLimit()
        {
            var calls = 0;
            var waits = new List<TimeSpan>();

            Assert.ThrowsExactly<UnauthorizedAccessException>(() => EntraIdCredentials.ConnectWithRetry<string>(() =>
            {
                calls++;
                throw new UnauthorizedAccessException("401");
            }, ex => ex is UnauthorizedAccessException, 2, TimeSpan.FromSeconds(3), null, waits.Add));

            Assert.AreEqual(3, calls);
            CollectionAssert.AreEqual(new[] { TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(6) }, waits);
        }

        [TestMethod, TestCategory("L0")]
        public void ConnectWithRetry_DoesNotRetryOtherFailures()
        {
            var calls = 0;

            Assert.ThrowsExactly<InvalidOperationException>(() => EntraIdCredentials.ConnectWithRetry<string>(() =>
            {
                calls++;
                throw new InvalidOperationException("not an auth failure");
            }, ex => ex is UnauthorizedAccessException, 2, TimeSpan.FromSeconds(3), null, _ => Assert.Fail("Must not wait.")));

            Assert.AreEqual(1, calls);
        }
    }
}
