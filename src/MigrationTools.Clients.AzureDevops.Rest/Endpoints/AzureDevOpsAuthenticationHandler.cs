using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MigrationTools.Endpoints
{
    /// <summary>
    /// Sets the Authorization header on every request rather than once per HttpClient, so a reused client
    /// picks up a renewed Microsoft Entra token instead of sending an expired one.
    /// </summary>
    internal sealed class AzureDevOpsAuthenticationHandler : DelegatingHandler
    {
        private readonly Func<string> _resolveToken;
        private readonly AuthenticationMode _authenticationMode;

        public AzureDevOpsAuthenticationHandler(Func<string> resolveToken, AuthenticationMode authenticationMode) : base(new HttpClientHandler())
        {
            _resolveToken = resolveToken;
            _authenticationMode = authenticationMode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Authorization == null)
            {
                request.Headers.Authorization = _authenticationMode == AuthenticationMode.EntraId
                    ? new AuthenticationHeaderValue("Bearer", _resolveToken())
                    : new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + _resolveToken())));
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
