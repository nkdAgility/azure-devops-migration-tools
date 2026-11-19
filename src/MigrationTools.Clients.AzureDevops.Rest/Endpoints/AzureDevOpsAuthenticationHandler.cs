using System;
using System.Net;
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
        private readonly Func<CancellationToken, Task<string>> _renewToken;
        private readonly AuthenticationMode _authenticationMode;

        /// <param name="renewToken">EntraId mode: gets a new token after the server rejected the current one.</param>
        public AzureDevOpsAuthenticationHandler(Func<string> resolveToken, AuthenticationMode authenticationMode, Func<CancellationToken, Task<string>> renewToken = null, HttpMessageHandler inner = null)
            : base(inner ?? new HttpClientHandler())
        {
            _resolveToken = resolveToken;
            _renewToken = renewToken;
            _authenticationMode = authenticationMode;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var ownsHeader = request.Headers.Authorization == null;
            if (ownsHeader)
            {
                request.Headers.Authorization = _authenticationMode == AuthenticationMode.EntraId
                    ? new AuthenticationHeaderValue("Bearer", _resolveToken())
                    : new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + _resolveToken())));
            }
            var content = request.Content == null ? null : await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Unauthorized || !ownsHeader || _authenticationMode != AuthenticationMode.EntraId || _renewToken == null)
            {
                return response;
            }

            // The token expired or was revoked between the margin check and the server: renew once and resend.
            // A second 401 is an access problem, not an expired token, so it goes back to the caller.
            var retry = Clone(request, content);
            retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _renewToken(cancellationToken).ConfigureAwait(false));
            response.Dispose();
            return await base.SendAsync(retry, cancellationToken).ConfigureAwait(false);
        }

        private static HttpRequestMessage Clone(HttpRequestMessage request, byte[] content)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };
            foreach (var header in request.Headers)
            {
                if (header.Key != "Authorization") { clone.Headers.TryAddWithoutValidation(header.Key, header.Value); }
            }
            if (content != null)
            {
                clone.Content = new ByteArrayContent(content);
                foreach (var header in request.Content.Headers) { clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value); }
            }
            return clone;
        }
    }
}
