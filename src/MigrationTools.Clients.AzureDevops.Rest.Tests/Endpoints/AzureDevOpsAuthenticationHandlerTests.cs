using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MigrationTools.Endpoints.Tests
{
    [TestClass]
    public class AzureDevOpsAuthenticationHandlerTests
    {
        private sealed class Server : HttpMessageHandler
        {
            private readonly Queue<HttpStatusCode> _responses;
            public readonly List<string> Authorizations = new List<string>();
            public readonly List<string> Bodies = new List<string>();
            public string Challenge;

            public Server(params HttpStatusCode[] responses) { _responses = new Queue<HttpStatusCode>(responses); }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Authorizations.Add(request.Headers.Authorization?.ToString());
                Bodies.Add(request.Content == null ? null : await request.Content.ReadAsStringAsync());
                var response = new HttpResponseMessage(_responses.Dequeue());
                if (response.StatusCode == HttpStatusCode.Unauthorized && Challenge != null) { response.Headers.TryAddWithoutValidation("WWW-Authenticate", Challenge); }
                return response;
            }
        }

        private static HttpClient Client(Server server, AuthenticationMode mode, List<string> renewals)
            => new HttpClient(new AzureDevOpsAuthenticationHandler(() => "current", mode,
                (rejected, claims, ct) => { renewals.Add($"{rejected}|{claims}"); return Task.FromResult("renewed-" + renewals.Count); }, server));

        // Returns the token it was given until it is asked with claims, as a credential's cache does after a revocation.
        private sealed class CachingCredential : Azure.Core.TokenCredential
        {
            public readonly List<string> ClaimsSeen = new List<string>();
            private Azure.Core.AccessToken Token(Azure.Core.TokenRequestContext context)
            {
                ClaimsSeen.Add(context.Claims);
                return new Azure.Core.AccessToken(context.Claims == null ? "cached" : "fresh", System.DateTimeOffset.UtcNow.AddHours(1));
            }
            public override Azure.Core.AccessToken GetToken(Azure.Core.TokenRequestContext context, CancellationToken ct) => Token(context);
            public override System.Threading.Tasks.ValueTask<Azure.Core.AccessToken> GetTokenAsync(Azure.Core.TokenRequestContext context, CancellationToken ct) => new System.Threading.Tasks.ValueTask<Azure.Core.AccessToken>(Token(context));
        }

        private static HttpClient Client(Server server, EntraIdCredentials credentials)
            => new HttpClient(new AzureDevOpsAuthenticationHandler(() => credentials.GetAccessToken(), AuthenticationMode.EntraId,
                (rejected, claims, ct) => credentials.GetFreshAccessTokenAsync(rejected, claims, ct), server));

        [TestMethod, TestCategory("L0")]
        public async Task EntraId_StopsWhenTheServerRejectsAnUnexpiredTokenAndRenewalReturnsTheSameOne()
        {
            var server = new Server(HttpStatusCode.Unauthorized, HttpStatusCode.OK);
            var credentials = new EntraIdCredentials(new EntraIdOptions { Credential = "AzureCli" }, "Source", (o, n) => new CachingCredential());

            var ex = await Assert.ThrowsExactlyAsync<MigrationTools.Exceptions.MigrationRunStoppedException>(
                () => Client(server, credentials).GetAsync("https://dev.azure.com/org/_apis/x"));

            StringAssert.Contains(ex.Message, "Endpoint 'Source': Azure DevOps rejected the Microsoft Entra ID token before it expired");
            StringAssert.Contains(ex.Message, "Run 'az login'");
            CollectionAssert.AreEqual(new[] { "Bearer cached" }, server.Authorizations, "The rejected token is never sent again.");
        }

        [TestMethod, TestCategory("L0")]
        public async Task EntraId_PassesTheClaimsChallengeSoTheRetryGetsAFreshToken()
        {
            var claims = "{\"access_token\":{\"nbf\":{\"essential\":true,\"value\":\"1700000000\"}}}";
            var server = new Server(HttpStatusCode.Unauthorized, HttpStatusCode.OK)
            {
                Challenge = $"Bearer realm=\"\", error=\"insufficient_claims\", claims=\"{System.Convert.ToBase64String(Encoding.UTF8.GetBytes(claims))}\""
            };
            var credential = new CachingCredential();
            var credentials = new EntraIdCredentials(new EntraIdOptions { Credential = "Interactive" }, "Target", (o, n) => credential);

            var response = await Client(server, credentials).GetAsync("https://dev.azure.com/org/_apis/x");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            CollectionAssert.AreEqual(new[] { "Bearer cached", "Bearer fresh" }, server.Authorizations);
            CollectionAssert.AreEqual(new string[] { null, claims }, credential.ClaimsSeen, "The renewal asks with the decoded claims.");
            Assert.AreEqual("fresh", credentials.GetAccessToken(), "The fresh token replaces the rejected one.");
        }

        [TestMethod, TestCategory("L0")]
        public void ClaimsFromChallenge_DecodesBase64AndIgnoresHeadersWithoutClaims()
        {
            Assert.AreEqual("{\"a\":1}", EntraIdCredentials.ClaimsFromChallenge("Bearer error=\"insufficient_claims\", claims=\"eyJhIjoxfQ==\""));
            Assert.AreEqual("{\"a\":1}", EntraIdCredentials.ClaimsFromChallenge("Bearer claims=\"eyJhIjoxfQ\""), "Unpadded base64url.");
            Assert.IsNull(EntraIdCredentials.ClaimsFromChallenge("Bearer authorization_uri=\"https://login.microsoftonline.com/x\""));
            Assert.IsNull(EntraIdCredentials.ClaimsFromChallenge(null));
        }

        [TestMethod, TestCategory("L0")]
        public async Task EntraId_RenewsTheTokenOnceAndResendsTheRequestAfterA401()
        {
            var server = new Server(HttpStatusCode.Unauthorized, HttpStatusCode.OK);
            var renewals = new List<string>();

            var response = await Client(server, AuthenticationMode.EntraId, renewals)
                .PostAsync("https://dev.azure.com/org/_apis/x", new StringContent("{\"a\":1}", Encoding.UTF8, "application/json"));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            CollectionAssert.AreEqual(new[] { "Bearer current", "Bearer renewed-1" }, server.Authorizations);
            CollectionAssert.AreEqual(new[] { "{\"a\":1}", "{\"a\":1}" }, server.Bodies, "The resent request carries the same body.");
            CollectionAssert.AreEqual(new[] { "current|" }, renewals, "Renewal is told which token was rejected; no challenge means no claims.");
        }

        [TestMethod, TestCategory("L0")]
        public async Task EntraId_ReturnsASecond401ToTheCaller()
        {
            var server = new Server(HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized);
            var renewals = new List<string>();

            var response = await Client(server, AuthenticationMode.EntraId, renewals).GetAsync("https://dev.azure.com/org/_apis/x");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.AreEqual(2, server.Authorizations.Count);
            Assert.AreEqual(1, renewals.Count);
        }

        private sealed class RedirectingServer : HttpMessageHandler
        {
            public int Calls;
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Calls++;
                // As the redirect handler does before following a redirect to another host.
                request.Headers.Authorization = null;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }
        }

        [TestMethod, TestCategory("L0")]
        public async Task EntraId_DoesNotRenewWhenARedirectRemovedTheToken()
        {
            var server = new RedirectingServer();
            var renewals = new List<string>();
            var client = new HttpClient(new AzureDevOpsAuthenticationHandler(() => "current", AuthenticationMode.EntraId,
                (rejected, claims, ct) => { renewals.Add(rejected); return Task.FromResult("renewed"); }, server));

            var response = await client.GetAsync("https://dev.azure.com/org/_apis/x");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.AreEqual(1, server.Calls);
            Assert.AreEqual(0, renewals.Count, "The 401 came from the redirect target, not from our token.");
        }

        [TestMethod, TestCategory("L0")]
        public async Task AccessToken_NeverRetries()
        {
            var server = new Server(HttpStatusCode.Unauthorized);
            var renewals = new List<string>();

            var response = await Client(server, AuthenticationMode.AccessToken, renewals).GetAsync("https://dev.azure.com/org/_apis/x");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.AreEqual(1, server.Authorizations.Count);
            Assert.AreEqual(0, renewals.Count);
        }
    }
}
