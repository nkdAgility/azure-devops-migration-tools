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
    [DoNotParallelize] // Changes the process-wide AZURE_TOKEN_CREDENTIALS variable.
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
        private EntraIdCredentials _sut;

        private static EntraIdCredentials Using(TokenCredential credential, EntraIdCredentialType type = EntraIdCredentialType.AzureCli)
            => new EntraIdCredentials(new EntraIdOptions { Credential = type.ToString() }, "Source", (o, n) => credential);

        [TestInitialize]
        public void Setup()
        {
            _fake = new CountingTokenCredential();
            _sut = Using(_fake);
        }

        [TestMethod, TestCategory("L0")]
        public void GetAccessToken_RequestsTheAzureDevOpsScope()
        {
            Assert.AreEqual("token-1", _sut.GetAccessToken());
            CollectionAssert.AreEqual(new[] { EntraIdCredentials.AzureDevOpsScope }, _fake.Scopes[0]);
        }

        [TestMethod, TestCategory("L0")]
        public void GetAccessToken_ReusesAnUnexpiredToken()
        {
            _sut.GetAccessToken();
            Assert.AreEqual("token-1", _sut.GetAccessToken());
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
        public async Task TfsRenewal_UsesTheClaimsTheLast401AskedForOnce()
        {
            var seen = new System.Collections.Generic.List<string>();
            var sut = new EntraIdCredentials(new EntraIdOptions { Credential = "AzureCli" }, "Source", (o, n) => new ClaimsRecordingCredential(seen));
            var provider = new EntraIdCredentials.EntraIdTokenProvider(new Microsoft.VisualStudio.Services.OAuth.VssOAuthAccessTokenCredential("unused"), sut, new Uri("https://dev.azure.com/org"));

            sut.NoteChallenge("Bearer error=\"insufficient_claims\", claims=\"eyJhIjoxfQ==\"");
            var first = await GetTokenFrom(provider, "rejected-1");
            var second = await GetTokenFrom(provider, "rejected-2");

            CollectionAssert.AreEqual(new[] { "{\"a\":1}", null }, seen, "The claims are used for the next renewal only.");
            Assert.AreNotEqual(first, second);
        }

        [TestMethod, TestCategory("L0")]
        public async Task TfsRenewal_StopsWhenRenewalReturnsTheRejectedToken()
        {
            var sut = Using(new SequenceTokenCredential(new AccessToken("same", DateTimeOffset.UtcNow.AddHours(1))), EntraIdCredentialType.DeviceCode);
            var provider = new EntraIdCredentials.EntraIdTokenProvider(new Microsoft.VisualStudio.Services.OAuth.VssOAuthAccessTokenCredential("unused"), sut, new Uri("https://dev.azure.com/org"));

            var ex = await Assert.ThrowsExactlyAsync<MigrationTools.Exceptions.MigrationRunStoppedException>(() => GetTokenFrom(provider, "same"));

            StringAssert.Contains(ex.Message, "rejected the Microsoft Entra ID token before it expired");
            StringAssert.Contains(ex.Message, "and run again to sign in.");
        }

        private static async Task<string> GetTokenFrom(EntraIdCredentials.EntraIdTokenProvider provider, string rejected)
        {
            var method = typeof(EntraIdCredentials.EntraIdTokenProvider).GetMethod("OnGetTokenAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            try
            {
                var token = await (Task<Microsoft.VisualStudio.Services.Common.IssuedToken>)method.Invoke(provider, new object[] { new Microsoft.VisualStudio.Services.OAuth.VssOAuthAccessToken(rejected), CancellationToken.None });
                return ((Microsoft.VisualStudio.Services.OAuth.VssOAuthAccessToken)token).Value;
            }
            catch (System.Reflection.TargetInvocationException ex) { throw ex.InnerException; }
        }

        private sealed class ClaimsRecordingCredential : TokenCredential
        {
            private readonly System.Collections.Generic.List<string> _seen;
            public ClaimsRecordingCredential(System.Collections.Generic.List<string> seen) { _seen = seen; }
            private AccessToken Next(TokenRequestContext context) { _seen.Add(context.Claims); return new AccessToken("token-" + _seen.Count, DateTimeOffset.UtcNow.AddHours(1)); }
            public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken) => Next(requestContext);
            public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken) => new ValueTask<AccessToken>(Next(requestContext));
        }

        [TestMethod, TestCategory("L0")]
        public async Task GetFreshAccessTokenAsync_CachesTheReplacementEvenWhenItExpiresSooner()
        {
            var tokens = new SequenceTokenCredential(
                new AccessToken("rejected", DateTimeOffset.UtcNow.AddHours(2)),
                new AccessToken("replacement", DateTimeOffset.UtcNow.AddMinutes(30)));
            _sut = Using(tokens);

            Assert.AreEqual("rejected", _sut.GetAccessToken());
            Assert.AreEqual("replacement", await _sut.GetFreshAccessTokenAsync());

            Assert.AreEqual("replacement", _sut.GetAccessToken(), "The rejected token must not be sent again.");
        }

        private sealed class SequenceTokenCredential : TokenCredential
        {
            private readonly System.Collections.Generic.Queue<AccessToken> _tokens;
            public SequenceTokenCredential(params AccessToken[] tokens) { _tokens = new System.Collections.Generic.Queue<AccessToken>(tokens); }
            public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken) => _tokens.Dequeue();
            public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken) => new ValueTask<AccessToken>(_tokens.Dequeue());
        }

        [TestMethod, TestCategory("L0")]
        public async Task GetFreshAccessTokenAsync_UsesANewAppCredentialSoItsCachedTokenIsNotReused()
        {
            var built = 0;
            var app = new EntraIdCredentials(new EntraIdOptions { Credential = nameof(EntraIdCredentialType.ClientSecret) }, "Target", (o, n) => { built++; return new CountingTokenCredential(); });
            var user = new EntraIdCredentials(new EntraIdOptions { Credential = nameof(EntraIdCredentialType.AzureCli) }, "Source", (o, n) => { built += 100; return new CountingTokenCredential(); });

            app.GetAccessToken();
            await app.GetFreshAccessTokenAsync();
            await app.GetFreshAccessTokenAsync();
            user.GetAccessToken();
            await user.GetFreshAccessTokenAsync();
            var chain = new EntraIdCredentials(new EntraIdOptions { Credential = nameof(EntraIdCredentialType.Default) }, "Source", (o, n) => { built += 1000; return new CountingTokenCredential(); });
            chain.GetAccessToken();
            await chain.GetFreshAccessTokenAsync();

            Assert.AreEqual(3 + 100 + 2000, built, "Each renewal after a 401 builds a new app or Default credential, since Default can select an app; an explicit user credential is built once.");
        }

        [TestMethod, TestCategory("L0")]
        public async Task GetFreshAccessTokenAsync_ConcurrentCallersShareOneFetch()
        {
            var slow = new SlowTokenCredential();
            _sut = Using(slow);

            var first = _sut.GetFreshAccessTokenAsync();
            var second = _sut.GetFreshAccessTokenAsync();
            slow.Release.SetResult(true);

            CollectionAssert.AreEqual(new[] { "slow-1", "slow-1" }, await Task.WhenAll(first, second));
            Assert.AreEqual(1, slow.Calls);
        }

        [TestMethod, TestCategory("L0")]
        public async Task GetFreshAccessTokenAsync_CallerCanStopWaitingWithoutCancellingOthers()
        {
            var slow = new SlowTokenCredential();
            _sut = Using(slow);
            using (var cts = new CancellationTokenSource())
            {
                var cancelledCaller = _sut.GetFreshAccessTokenAsync(cts.Token);
                var patientCaller = _sut.GetFreshAccessTokenAsync();
                cts.Cancel();

                await Assert.ThrowsExactlyAsync<OperationCanceledException>(() => cancelledCaller);
                // Arrives after a caller gave up but before the fetch finishes: must join it, not start another.
                var lateCaller = _sut.GetFreshAccessTokenAsync();
                slow.Release.SetResult(true);
                Assert.AreEqual("slow-1", await patientCaller);
                Assert.AreEqual("slow-1", await lateCaller);
                Assert.AreEqual(1, slow.Calls);
            }
        }

        [TestMethod, TestCategory("L0")]
        public void FederatedCredential_TreatsUnauthorizedAsAChallenge()
        {
            var credential = new EntraIdCredentials.EntraIdFederatedCredential(_sut);

            Assert.IsTrue(credential.IsAuthenticationChallenge(new FakeResponse(HttpStatusCode.Unauthorized)));
            Assert.IsFalse(credential.IsAuthenticationChallenge(new FakeResponse(HttpStatusCode.OK)));
            Assert.AreEqual(VssCredentialsType.OAuth, credential.CredentialType);
        }

        [TestMethod, TestCategory("L0")]
        public async Task TokenProvider_FetchesAFreshTokenAfterTheServerRejectsTheCurrentOne()
        {
            var credential = new EntraIdCredentials.EntraIdFederatedCredential(_sut);
            Assert.AreEqual(1, _fake.Calls, "The credential starts with one token.");
            var initial = new VssOAuthAccessToken("token-1");

            var provider = new EntraIdCredentials.EntraIdTokenProvider(credential, _sut, new Uri("https://dev.azure.com/example/"));
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
            _sut = Using(new UnavailableTokenCredential());

            var ex = Assert.ThrowsExactly<MigrationTools.Exceptions.MigrationRunStoppedException>(() => _sut.GetAccessToken());

            StringAssert.StartsWith(ex.Message, "Endpoint 'Source': could not get a Microsoft Entra ID token with AzureCli: the Azure CLI is not signed in or not installed (No credential is signed in)");
            StringAssert.Contains(ex.Message, "az login");
            Assert.IsInstanceOfType(ex.InnerException, typeof(Azure.Identity.CredentialUnavailableException));
        }

        [TestMethod, TestCategory("L0")]
        public async Task GetFreshAccessTokenAsync_StopsTheRunWithAReadableReasonWhenNoCredentialIsAvailable()
        {
            _sut = Using(new UnavailableTokenCredential());

            await Assert.ThrowsExactlyAsync<MigrationTools.Exceptions.MigrationRunStoppedException>(() => _sut.GetFreshAccessTokenAsync());
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
            _sut = Using(new BuggyTokenCredential());

            Assert.ThrowsExactly<InvalidOperationException>(() => _sut.GetAccessToken());
        }

        [TestMethod, TestCategory("L0")]
        public void IsCredentialFailure_AcceptsOnlyTheTokenCredentialsVariableRejection()
        {
            var saved = Environment.GetEnvironmentVariable(EntraIdCredentials.TokenCredentialsVariable);
            _sut = Using(_fake, EntraIdCredentialType.Default);
            try
            {
                var rejected = new InvalidOperationException("Invalid value for environment variable AZURE_TOKEN_CREDENTIALS: notacredential.");
                var unrelated = new InvalidOperationException("Collection was modified.");

                Environment.SetEnvironmentVariable(EntraIdCredentials.TokenCredentialsVariable, null);
                Assert.IsFalse(_sut.IsCredentialFailure(rejected), "Not a pinning failure when the variable is unset.");

                Environment.SetEnvironmentVariable(EntraIdCredentials.TokenCredentialsVariable, "NotACredential");
                Assert.IsTrue(_sut.IsCredentialFailure(rejected));
                Assert.IsFalse(_sut.IsCredentialFailure(unrelated));
                Assert.IsTrue(_sut.IsCredentialFailure(new Azure.Identity.CredentialUnavailableException("none")));
            }
            finally
            {
                Environment.SetEnvironmentVariable(EntraIdCredentials.TokenCredentialsVariable, saved);
            }
        }

        [TestMethod, TestCategory("L0")]
        public void GetAccessToken_StopsReadablyWhenTheCredentialCannotBeBuilt()
        {
            var saved = Environment.GetEnvironmentVariable(EntraIdCredentials.TokenCredentialsVariable);
            try
            {
                Environment.SetEnvironmentVariable(EntraIdCredentials.TokenCredentialsVariable, "NotACredential");
                // The real factory: DefaultAzureCredential throws from its constructor for an unsupported value.
                var sut = new EntraIdCredentials(null, "Source");

                var ex = Assert.ThrowsExactly<MigrationTools.Exceptions.MigrationRunStoppedException>(() => sut.GetAccessToken());

                StringAssert.Contains(ex.Message, "Endpoint 'Source'");
                StringAssert.Contains(ex.Message, "AZURE_TOKEN_CREDENTIALS is set to 'NotACredential'");
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
            _sut = Using(_fake, EntraIdCredentialType.Default);
            try
            {
                Environment.SetEnvironmentVariable(EntraIdCredentials.TokenCredentialsVariable, "NotACredential");

                var ex = _sut.CredentialFailure(new InvalidOperationException("Invalid value for environment variable AZURE_TOKEN_CREDENTIALS: notacredential."));

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
            Assert.AreEqual("token-1", _sut.GetAccessToken());
            Assert.AreEqual("token-1", _sut.GetAccessToken());

            _sut.Reset();

            Assert.AreEqual("token-2", _sut.GetAccessToken());
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
            _sut = Using(gated);

            var stale = _sut.GetFreshAccessTokenAsync();
            _sut.Reset();
            gated.Gate.SetResult(new AccessToken("stale", DateTimeOffset.UtcNow.AddHours(2)));
            Assert.AreEqual("stale", await stale, "The caller that started the fetch still gets its result.");

            Assert.AreEqual("sync-1", _sut.GetAccessToken(), "The pre-reset token must not be cached.");
        }

        [TestMethod, TestCategory("L0")]
        public void ConnectWithRetry_RetriesARejectedConnectionWithANewToken()
        {
            var attempts = new List<string>();
            var waits = new List<TimeSpan>();
            var retried = new List<int>();

            var result = _sut.ConnectWithRetry(() =>
            {
                attempts.Add(_sut.GetAccessToken());
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

            Assert.ThrowsExactly<UnauthorizedAccessException>(() => _sut.ConnectWithRetry<string>(() =>
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

            Assert.ThrowsExactly<InvalidOperationException>(() => _sut.ConnectWithRetry<string>(() =>
            {
                calls++;
                throw new InvalidOperationException("not an auth failure");
            }, ex => ex is UnauthorizedAccessException, 2, TimeSpan.FromSeconds(3), null, _ => Assert.Fail("Must not wait.")));

            Assert.AreEqual(1, calls);
        }
    }
}
