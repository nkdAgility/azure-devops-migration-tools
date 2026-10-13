using System;
using System.Net.Http.Headers;
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

        /// <summary>Overridable for tests; defaults to a shared DefaultAzureCredential.</summary>
        public static TokenCredential TokenCredential { get; set; }

        private static TokenCredential Current => TokenCredential ?? _default.Value;

        public static string GetAccessToken(CancellationToken cancellationToken = default)
            => Current.GetToken(new TokenRequestContext(new[] { AzureDevOpsScope }), cancellationToken).Token;

        public static async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
            => (await Current.GetTokenAsync(new TokenRequestContext(new[] { AzureDevOpsScope }), cancellationToken).ConfigureAwait(false)).Token;

        public static AuthenticationHeaderValue GetAuthorizationHeader()
            => new AuthenticationHeaderValue("Bearer", GetAccessToken());

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
                => new VssOAuthAccessToken(await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false));
        }
    }
}
