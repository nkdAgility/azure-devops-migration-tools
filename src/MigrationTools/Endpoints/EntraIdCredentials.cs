using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.Diagnostics;
using Azure.Identity;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.OAuth;

namespace MigrationTools.Endpoints
{
    /// <summary>
    /// Gets Microsoft Entra ID access tokens for Azure DevOps from the ambient identity, so no PAT has to be
    /// stored. DefaultAzureCredential tries, in order: environment service principal (AZURE_CLIENT_ID etc.),
    /// workload identity, managed identity, Visual Studio, Azure CLI, Azure PowerShell and azd logins.
    /// Set AZURE_TOKEN_CREDENTIALS (for example to AzureCliCredential) to use only that credential.
    /// </summary>
    public static class EntraIdCredentials
    {
        /// <summary>The well-known Azure DevOps resource id; tokens are requested for its default scope.</summary>
        public const string AzureDevOpsScope = "499b84ac-1321-427f-aa17-267ca6975798/.default";

        private static Lazy<TokenCredential> _default = NewDefault();

        private static Lazy<TokenCredential> NewDefault() => new Lazy<TokenCredential>(() =>
        {
            EnsureSelectionListener();
            return new DefaultAzureCredential();
        });

        // DefaultAzureCredential reports the credential it settled on only through its event source (#3225).
        private static AzureEventSourceListener _selectionListener;
        private static string _selectedCredential;

        /// <summary>The credential DefaultAzureCredential selected, when known (for example "AzureCliCredential").</summary>
        public static string SelectedCredential => _tokenCredential != null ? _tokenCredential.GetType().Name : _selectedCredential;

        private static void EnsureSelectionListener()
        {
            if (_selectionListener != null) { return; }
            _selectionListener = new AzureEventSourceListener((e, message) =>
            {
                if (e.EventName == "DefaultAzureCredentialCredentialSelected" && e.Payload != null && e.Payload.Count > 0)
                {
                    var name = e.Payload[0]?.ToString();
                    _selectedCredential = name?.Substring(name.LastIndexOf('.') + 1);
                }
            }, System.Diagnostics.Tracing.EventLevel.Informational);
        }

        // Some credentials (Azure CLI) start a process per call, so a token is reused until close to expiry.
        private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(5);
        private static readonly object _cacheLock = new object();
        private static AccessToken? _cached;
        private static TokenCredential _tokenCredential;

        /// <summary>Overridable for tests; defaults to a shared DefaultAzureCredential.</summary>
        public static TokenCredential TokenCredential
        {
            get => _tokenCredential;
            set { lock (_cacheLock) { _tokenCredential = value; _cached = null; _generation++; _inFlight = null; } }
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
                AccessToken fresh;
                try
                {
                    fresh = Current.GetToken(RequestContext, cancellationToken);
                }
                catch (Exception ex) when (IsCredentialFailure(ex))
                {
                    throw CredentialFailure(ex);
                }
                _cached = fresh;
                return fresh.Token;
            }
        }

        // No usable sign-in (none available, or AZURE_TOKEN_CREDENTIALS names an unknown or unavailable credential):
        // the run cannot continue, so stop it with a readable reason rather than a processor crash.
        private static bool IsCredentialFailure(Exception ex) => ex is AuthenticationFailedException || ex is InvalidOperationException;

        internal static MigrationTools.Exceptions.MigrationRunStoppedException CredentialFailure(Exception ex)
        {
            var pinned = _tokenCredential == null ? Environment.GetEnvironmentVariable(TokenCredentialsVariable) : null;
            var hint = string.IsNullOrEmpty(pinned)
                ? " Sign in first (for example 'az login'), or set " + TokenCredentialsVariable + " to the credential to use."
                : $" {TokenCredentialsVariable} is set to '{pinned}': check that value, and that this credential is signed in on this machine.";
            return new MigrationTools.Exceptions.MigrationRunStoppedException("Could not get a Microsoft Entra ID token: " + ex.Message.Split('\n')[0].Trim() + hint, ex);
        }

        /// <summary>
        /// Drops the cached token and, unless a credential was set explicitly, the DefaultAzureCredential instance,
        /// so the next request probes the credential chain again. Used before retrying a rejected first connection.
        /// </summary>
        public static void Reset()
        {
            lock (_cacheLock)
            {
                _cached = null;
                _selectedCredential = null;
                if (_tokenCredential == null) { _default = NewDefault(); }
                // A fetch already running belongs to the discarded credential: stop new callers joining it and
                // stop it publishing its token when it completes.
                _generation++;
                _inFlight = null;
            }
        }

        /// <summary>
        /// Runs <paramref name="connect"/>, retrying up to <paramref name="retries"/> more times when it fails with an
        /// exception <paramref name="isRejected"/> accepts. Before each retry the token cache and credential chain are
        /// reset and the caller waits <paramref name="delay"/> times the attempt number. Any other failure, or the
        /// last rejection, is thrown unchanged.
        /// </summary>
        public static T ConnectWithRetry<T>(Func<T> connect, Func<Exception, bool> isRejected, int retries, TimeSpan delay,
            Action<Exception, int, TimeSpan> onRetry, Action<TimeSpan> sleep = null)
        {
            sleep = sleep ?? Thread.Sleep;
            for (var attempt = 0; ; attempt++)
            {
                try
                {
                    return connect();
                }
                catch (Exception ex) when (attempt < retries && isRejected(ex))
                {
                    var wait = TimeSpan.FromTicks(delay.Ticks * (attempt + 1));
                    onRetry?.Invoke(ex, attempt + 2, wait);
                    Reset();
                    sleep(wait);
                }
            }
        }

        /// <summary>
        /// Who the current token is for and when it is valid, for diagnostics. Never contains the token itself.
        /// </summary>
        public static string DescribeCurrentToken()
        {
            AccessToken? cached;
            lock (_cacheLock) { cached = _cached; }
            if (!(cached is AccessToken token)) { return "no token acquired"; }
            var pinned = _tokenCredential == null ? Environment.GetEnvironmentVariable(TokenCredentialsVariable) : null;
            return DescribeToken(token.Token, token.ExpiresOn, SelectedCredential, pinned);
        }

        /// <summary>
        /// Azure.Identity's variable for limiting DefaultAzureCredential to one credential (for example
        /// "AzureCliCredential") or to "dev"/"prod" credentials, so an unattended run cannot pick another sign-in (#3225).
        /// </summary>
        public const string TokenCredentialsVariable = "AZURE_TOKEN_CREDENTIALS";

        internal static string DescribeToken(string jwt, DateTimeOffset expiresOn, string credential, string pinnedTo = null)
        {
            var source = string.IsNullOrEmpty(credential) ? "unknown credential" : credential;
            if (!string.IsNullOrEmpty(pinnedTo)) { source += $" ({TokenCredentialsVariable}={pinnedTo})"; }
            var claims = TryReadClaims(jwt);
            if (claims == null) { return $"{source}, expires {expiresOn:u} (not a readable JWT)"; }
            string Claim(string name) => claims.TryGetValue(name, out var v) ? v?.ToString() : null;
            string Time(string name) => long.TryParse(Claim(name), out var s) ? DateTimeOffset.FromUnixTimeSeconds(s).ToString("u") : "?";
            var who = Claim("upn") ?? Claim("unique_name") ?? Claim("appid") ?? Claim("oid") ?? "?";
            return $"{source}, identity {who}, tenant {Claim("tid") ?? "?"}, audience {Claim("aud") ?? "?"}, not before {Time("nbf")}, expires {Time("exp")}, now {DateTimeOffset.UtcNow:u}";
        }

        private static System.Collections.Generic.Dictionary<string, object> TryReadClaims(string jwt)
        {
            try
            {
                var parts = jwt?.Split('.');
                if (parts == null || parts.Length < 2) { return null; }
                var payload = parts[1].Replace('-', '+').Replace('_', '/');
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                return Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, object>>(json);
            }
            catch (Exception) { return null; }
        }

        private static Task<AccessToken> _inFlight;

        /// <summary>
        /// Asks the credential for a new token; used after the server rejected the current one. Concurrent callers
        /// (many requests failing with 401 at once) share a single fetch rather than each starting one.
        /// </summary>
        public static async Task<string> GetFreshAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            Task<AccessToken> fetch;
            lock (_cacheLock)
            {
                fetch = _inFlight;
                if (fetch == null)
                {
                    var started = FetchAndCacheAsync(Current, _generation);
                    _inFlight = started;
                    fetch = started;
                    // Cleared when the fetch itself finishes (success or failure), never when a caller stops
                    // waiting, so a cancelled caller cannot let a second fetch start alongside the first.
                    started.ContinueWith(_ =>
                    {
                        lock (_cacheLock)
                        {
                            if (ReferenceEquals(_inFlight, started)) { _inFlight = null; }
                        }
                    }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                }
            }
            return (await WaitAsync(fetch, cancellationToken).ConfigureAwait(false)).Token;
        }

        // The shared fetch ignores any one caller's cancellation (that would fail every waiter); each caller can
        // still stop waiting on its own token.
        private static async Task<AccessToken> WaitAsync(Task<AccessToken> fetch, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled) { return await fetch.ConfigureAwait(false); }
            var cancelled = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (cancellationToken.Register(() => cancelled.TrySetResult(true)))
            {
                if (await Task.WhenAny(fetch, cancelled.Task).ConfigureAwait(false) != fetch)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            return await fetch.ConfigureAwait(false);
        }

        // Bumped by Reset() and by setting TokenCredential, so a fetch started before either cannot publish its token.
        private static int _generation;

        private static async Task<AccessToken> FetchAndCacheAsync(TokenCredential credential, int generation)
        {
            AccessToken fresh;
            try
            {
                fresh = await credential.GetTokenAsync(RequestContext, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex) when (IsCredentialFailure(ex))
            {
                throw CredentialFailure(ex);
            }
            lock (_cacheLock)
            {
                if (generation == _generation && (!(_cached is AccessToken current) || fresh.ExpiresOn >= current.ExpiresOn))
                {
                    _cached = fresh;
                }
            }
            return fresh;
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
