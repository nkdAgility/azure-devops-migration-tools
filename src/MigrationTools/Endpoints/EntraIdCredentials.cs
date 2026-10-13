using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.OAuth;

namespace MigrationTools.Endpoints
{
    /// <summary>
    /// Gets Microsoft Entra ID access tokens for Azure DevOps from the ambient identity, so no PAT has to be
    /// stored. DefaultAzureCredential tries, in order: environment service principal (AZURE_CLIENT_ID etc.),
    /// workload identity, managed identity, Visual Studio, Azure CLI, Azure PowerShell and azd logins.
    /// </summary>
    public static class EntraIdCredentials
    {
        /// <summary>The well-known Azure DevOps resource id; tokens are requested for its default scope.</summary>
        public const string AzureDevOpsScope = "499b84ac-1321-427f-aa17-267ca6975798/.default";

        private static readonly Lazy<TokenCredential> _default = new Lazy<TokenCredential>(() => new DefaultAzureCredential());

        // Some credentials (Azure CLI) start a process per call, so a token is reused until close to expiry.
        private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(5);
        private static readonly object _cacheLock = new object();
        private static AccessToken? _cached;
        private static TokenCredential _tokenCredential;

        /// <summary>Overridable for tests; defaults to a shared DefaultAzureCredential.</summary>
        public static TokenCredential TokenCredential
        {
            get => _tokenCredential;
            set { lock (_cacheLock) { _tokenCredential = value; _cached = null; } }
        }

        private static TokenCredential Current => _tokenCredential ?? _default.Value;

        private static TokenRequestContext RequestContext => new TokenRequestContext(new[] { AzureDevOpsScope });

        /// <summary>A token valid for at least a few more minutes, from the cache when possible.</summary>
        public static string GetAccessToken(CancellationToken cancellationToken = default)
        {
            lock (_cacheLock)
            {
                if (_cached is AccessToken cached && cached.ExpiresOn - RefreshMargin > DateTimeOffset.UtcNow)
                {
                    return cached.Token;
                }
                var fresh = Current.GetToken(RequestContext, cancellationToken);
                _cached = fresh;
                return fresh.Token;
            }
        }

        /// <summary>Always asks the credential for a new token; used after the server rejected the current one.</summary>
        public static async Task<string> GetFreshAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            var fresh = await Current.GetTokenAsync(RequestContext, cancellationToken).ConfigureAwait(false);
            lock (_cacheLock) { _cached = fresh; }
            return fresh.Token;
        }

        /// <summary>
        /// VssCredentials for the Azure DevOps client libraries. The token is refreshed when the server
        /// rejects it, so long-running migrations survive the roughly one hour token lifetime.
        /// </summary>
        public static VssCredentials CreateVssCredentials()
            => new VssCredentials(new EntraIdFederatedCredential());

        internal sealed class EntraIdFederatedCredential : FederatedCredential
        {
            public EntraIdFederatedCredential() : base(new VssOAuthAccessToken(GetAccessToken())) { }

            public override VssCredentialsType CredentialType => VssCredentialsType.OAuth;

            public override bool IsAuthenticationChallenge(IHttpResponse webResponse)
                => webResponse != null && (webResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized || webResponse.StatusCode == System.Net.HttpStatusCode.Found);

            protected override IssuedTokenProvider OnCreateTokenProvider(Uri serverUrl, IHttpResponse response)
                => new EntraIdTokenProvider(this, serverUrl);
        }

        internal sealed class EntraIdTokenProvider : IssuedTokenProvider
        {
            public EntraIdTokenProvider(IssuedTokenCredential credential, Uri serverUrl) : base(credential, serverUrl, serverUrl) { }

            public override bool GetTokenIsInteractive => false;

            protected override async Task<IssuedToken> OnGetTokenAsync(IssuedToken failedToken, CancellationToken cancellationToken)
                => new VssOAuthAccessToken(await GetFreshAccessTokenAsync(cancellationToken).ConfigureAwait(false));
        }
    }
}
