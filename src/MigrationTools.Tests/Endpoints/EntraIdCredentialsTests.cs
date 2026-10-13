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
    }
}
