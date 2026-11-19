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

            public Server(params HttpStatusCode[] responses) { _responses = new Queue<HttpStatusCode>(responses); }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Authorizations.Add(request.Headers.Authorization?.ToString());
                Bodies.Add(request.Content == null ? null : await request.Content.ReadAsStringAsync());
                return new HttpResponseMessage(_responses.Dequeue());
            }
        }

        private static HttpClient Client(Server server, AuthenticationMode mode, List<string> renewals)
            => new HttpClient(new AzureDevOpsAuthenticationHandler(() => "current", mode,
                ct => { renewals.Add("renewed"); return Task.FromResult("renewed-" + renewals.Count); }, server));

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
