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
    /// Gets Microsoft Entra ID access tokens for Azure DevOps for one endpoint, so no PAT has to be stored. Each
    /// endpoint has its own instance (credential, cached token and in-flight fetch), so Source and Target can sign
    /// in as different identities or in different tenants. The credential comes from the endpoint's
    /// <see cref="EntraIdOptions"/>; with none, DefaultAzureCredential uses the first sign-in found on the machine.
    /// </summary>
    public sealed class EntraIdCredentials
    {
        /// <summary>The well-known Azure DevOps resource id; tokens are requested for its default scope.</summary>
        public const string AzureDevOpsScope = "499b84ac-1321-427f-aa17-267ca6975798/.default";

        /// <summary>
        /// Azure.Identity's variable for limiting DefaultAzureCredential to one credential (for example
        /// "AzureCliCredential") or to "dev"/"prod" credentials, so an unattended run cannot pick another sign-in (#3225).
        /// </summary>
        public const string TokenCredentialsVariable = "AZURE_TOKEN_CREDENTIALS";

        // Some credentials (Azure CLI) start a process per call, so a token is reused until close to expiry.
        private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(5);

        private readonly EntraIdOptions _options;
        private readonly Func<EntraIdOptions, string, TokenCredential> _factory;
        private readonly object _cacheLock = new object();
        private TokenCredential _credential;
        private AccessToken? _cached;
        private Task<AccessToken> _inFlight;
        // Bumped by Reset(), so a fetch started before it cannot publish its token.
        private int _generation;

        public EntraIdCredentials(EntraIdOptions options, string endpointName, Func<EntraIdOptions, string, TokenCredential> factory = null)
        {
            _options = options ?? new EntraIdOptions();
            _factory = factory ?? EntraIdCredentialFactory.Create;
            NameEndpoint(endpointName);
        }

        public string EndpointName { get; private set; } = UnnamedEndpoint;

        private const string UnnamedEndpoint = "endpoint";

        /// <summary>
        /// Sets the endpoint name used in messages, once: options are bound before the endpoint knows its name, so
        /// the first caller that knows it (the endpoint connecting) names the instance.
        /// </summary>
        public EntraIdCredentials NameEndpoint(string endpointName)
        {
            lock (_cacheLock)
            {
                if (EndpointName == UnnamedEndpoint && !string.IsNullOrWhiteSpace(endpointName)) { EndpointName = endpointName; }
            }
            return this;
        }

        public EntraIdCredentialType CredentialType => _options.CredentialType;

        private bool IsDefault => _options.CredentialType == EntraIdCredentialType.Default;

        private TokenCredential Current
        {
            get
            {
                lock (_cacheLock)
                {
                    if (_credential == null)
                    {
                        if (IsDefault) { SelectionListener.Ensure(); }
                        try
                        {
                            _credential = _factory(_options, EndpointName);
                        }
                        catch (Exception ex) when (IsCredentialFailure(ex))
                        {
                            // DefaultAzureCredential rejects an unsupported AZURE_TOKEN_CREDENTIALS when it is built, not when it is asked for a token.
                            throw CredentialFailure(ex);
                        }
                    }
                    return _credential;
                }
            }
        }

        /// <summary>The credential in use, for example "AzureCliCredential". For Default, the one DefaultAzureCredential selected, when known.</summary>
        public string SelectedCredential => IsDefault ? SelectionListener.Selected : _options.CredentialType.ToString();

        private static TokenRequestContext RequestContext => new TokenRequestContext(new[] { AzureDevOpsScope });

        /// <summary>A token valid for at least a few more minutes, from the cache when possible.</summary>
        public string GetAccessToken(CancellationToken cancellationToken = default)
        {
            lock (_cacheLock)
            {
                if (_cached is AccessToken cached && cached.ExpiresOn - RefreshMargin > DateTimeOffset.UtcNow)
                {
                    return cached.Token;
                }
                // Chosen inside the lock, so a renewal after a 401 cannot discard this credential between choosing and using it.
                var credential = Current;
                AccessToken fresh;
                try
                {
                    fresh = credential.GetToken(RequestContext, cancellationToken);
                }
                catch (Exception ex) when (IsCredentialFailure(ex))
                {
                    throw CredentialFailure(ex);
                }
                _cached = fresh;
                Serilog.Log.Debug("Endpoint '{EndpointName}' Entra ID token renewed: {TokenDescription}", EndpointName, Describe(fresh));
                return fresh.Token;
            }
        }

        // No usable sign-in: the run cannot continue, so stop it with a readable reason rather than a processor crash.
        // Any other InvalidOperationException is a genuine bug and must surface as such, not as a sign-in hint.
        internal bool IsCredentialFailure(Exception ex) =>
            ex is AuthenticationFailedException
            || ex is CredentialUnavailableException
            || ex is AuthenticationRequiredException
            || (ex is InvalidOperationException
                && IsDefault
                && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(TokenCredentialsVariable))
                && ex.Message.IndexOf(TokenCredentialsVariable, StringComparison.OrdinalIgnoreCase) >= 0);

        internal MigrationTools.Exceptions.MigrationRunStoppedException CredentialFailure(Exception ex)
        {
            var reason = EntraIdCredentialFactory.Reason(ex);
            var prefix = $"Endpoint '{EndpointName}': could not get a Microsoft Entra ID token with {_options.CredentialType}";
            string message;
            switch (_options.CredentialType)
            {
                case EntraIdCredentialType.AzureCli:
                    message = $"{prefix}: the Azure CLI is not signed in or not installed ({reason}). Run 'az login'{(string.IsNullOrWhiteSpace(_options.TenantId) ? "" : $" --tenant {_options.TenantId}")}.";
                    break;
                case EntraIdCredentialType.Interactive:
                case EntraIdCredentialType.DeviceCode:
                    message = ex is AuthenticationRequiredException
                        ? $"{prefix}: the saved sign-in has expired or been revoked, and a run never prompts part way through. Delete {EntraIdCredentialFactory.AuthenticationRecordPath(_options, EndpointName)} and run again to sign in."
                        : $"{prefix}: {reason}. Delete {EntraIdCredentialFactory.AuthenticationRecordPath(_options, EndpointName)} and run again to sign in.";
                    break;
                case EntraIdCredentialType.Certificate:
                case EntraIdCredentialType.ClientSecret:
                    message = $"{prefix}: {reason}. Check TenantId, ClientId and that this {(_options.CredentialType == EntraIdCredentialType.Certificate ? "certificate" : "secret")} is registered on the app and has not expired.";
                    break;
                case EntraIdCredentialType.ManagedIdentity:
                    message = $"{prefix}: no managed identity is available on this machine ({reason}). Run on an Azure resource with a managed identity, or choose another Credential.";
                    break;
                default:
                    var pinned = Environment.GetEnvironmentVariable(TokenCredentialsVariable);
                    var hint = string.IsNullOrEmpty(pinned)
                        ? " Sign in first (for example 'az login'), or set " + TokenCredentialsVariable + " to the credential to use."
                        : $" {TokenCredentialsVariable} is set to '{pinned}': check that value, and that this credential is signed in on this machine.";
                    message = $"{prefix}: {reason}.{hint}";
                    break;
            }
            return new MigrationTools.Exceptions.MigrationRunStoppedException(message, ex);
        }

        /// <summary>
        /// Drops the cached token and, for Default, the DefaultAzureCredential instance, so the next request probes
        /// the credential chain again. Used before retrying a rejected first connection.
        /// </summary>
        public void Reset()
        {
            lock (_cacheLock)
            {
                _cached = null;
                if (IsDefault)
                {
                    _credential = null;
                    SelectionListener.Clear();
                }
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
        public T ConnectWithRetry<T>(Func<T> connect, Func<Exception, bool> isRejected, int retries, TimeSpan delay,
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
        public string DescribeCurrentToken()
        {
            AccessToken? cached;
            lock (_cacheLock) { cached = _cached; }
            return cached is AccessToken token ? Describe(token) : "no token acquired";
        }

        private string Describe(AccessToken token)
        {
            var pinned = IsDefault ? Environment.GetEnvironmentVariable(TokenCredentialsVariable) : null;
            return DescribeToken(token.Token, token.ExpiresOn, SelectedCredential, pinned);
        }

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

        /// <summary>
        /// Asks the credential for a new token; used after the server rejected the current one. Concurrent callers
        /// (many requests failing with 401 at once) share a single fetch rather than each starting one.
        /// </summary>
        public Task<string> GetFreshAccessTokenAsync(CancellationToken cancellationToken = default)
            => GetFreshAccessTokenAsync(null, null, cancellationToken);

        /// <param name="rejectedToken">The token the server just rejected. When renewal returns this same token (a revoked
        /// sign-in, or a policy the cached token no longer meets), the run stops and says to sign in again, rather than
        /// resending a token that cannot work.</param>
        /// <param name="claims">The claims challenge from the 401's WWW-Authenticate header (continuous access evaluation);
        /// requesting with it makes the credential get a new token instead of returning its cached one.</param>
        public async Task<string> GetFreshAccessTokenAsync(string rejectedToken, string claims, CancellationToken cancellationToken = default)
        {
            Task<AccessToken> fetch;
            lock (_cacheLock)
            {
                fetch = _inFlight;
                if (fetch == null)
                {
                    // An app credential keeps its tokens in an in-memory MSAL cache and would hand back the token the
                    // server just rejected; a new instance starts with an empty cache. Default is rebuilt too, because
                    // it can select an app credential (EnvironmentCredential, ManagedIdentityCredential). The explicit
                    // user sign-ins (Azure CLI, browser, device code) use a shared cache that refreshes itself.
                    if (IsDefault || EntraIdCredentialFactory.IsExperimental(_options.CredentialType))
                    {
                        _credential = null;
                        if (IsDefault) { SelectionListener.Clear(); }
                    }
                    // The cached token is the one the server rejected: drop it, and give this fetch its own
                    // generation so only its result is cached, whatever its expiry, and no earlier fetch overwrites it.
                    _cached = null;
                    _generation++;
                    var started = FetchAndCacheAsync(Current, _generation, rejectedToken, claims);
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

        private async Task<AccessToken> FetchAndCacheAsync(TokenCredential credential, int generation, string rejectedToken, string claims)
        {
            AccessToken fresh;
            try
            {
                var context = string.IsNullOrEmpty(claims)
                    ? RequestContext
                    : new TokenRequestContext(new[] { AzureDevOpsScope }, claims: claims);
                fresh = await credential.GetTokenAsync(context, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex) when (IsCredentialFailure(ex))
            {
                throw CredentialFailure(ex);
            }
            if (!string.IsNullOrEmpty(rejectedToken) && string.Equals(fresh.Token, rejectedToken, StringComparison.Ordinal))
            {
                throw RejectedTokenFailure(fresh);
            }
            lock (_cacheLock)
            {
                if (generation == _generation)
                {
                    _cached = fresh;
                }
            }
            Serilog.Log.Debug("Endpoint '{EndpointName}' Entra ID token renewed after a rejected request: {TokenDescription}", EndpointName, Describe(fresh));
            return fresh;
        }

        internal MigrationTools.Exceptions.MigrationRunStoppedException RejectedTokenFailure(AccessToken token)
        {
            string signInAgain;
            switch (_options.CredentialType)
            {
                case EntraIdCredentialType.AzureCli:
                    signInAgain = $"Run 'az login'{(string.IsNullOrWhiteSpace(_options.TenantId) ? "" : $" --tenant {_options.TenantId}")} and run again.";
                    break;
                case EntraIdCredentialType.Interactive:
                case EntraIdCredentialType.DeviceCode:
                    signInAgain = $"Delete {EntraIdCredentialFactory.AuthenticationRecordPath(_options, EndpointName)} and run again to sign in.";
                    break;
                case EntraIdCredentialType.Default:
                    signInAgain = "Sign in again with the tool the log shows it used (for example 'az login'), and run again.";
                    break;
                default:
                    signInAgain = "Check the app registration still exists, its certificate or secret is valid, and it still has access to the organisation.";
                    break;
            }
            return new MigrationTools.Exceptions.MigrationRunStoppedException(
                $"Endpoint '{EndpointName}': Azure DevOps rejected the Microsoft Entra ID token before it expired, and getting a new one returned the same token ({Describe(token)}). The sign-in has probably been revoked, or a conditional access policy no longer allows it. {signInAgain}");
        }

        /// <summary>
        /// The claims a continuous access evaluation challenge asks for, from a WWW-Authenticate header such as
        /// <c>Bearer error="insufficient_claims", claims="eyJ..."</c>; null when the header carries none.
        /// </summary>
        public static string ClaimsFromChallenge(string wwwAuthenticate)
        {
            if (string.IsNullOrEmpty(wwwAuthenticate)) { return null; }
            var match = System.Text.RegularExpressions.Regex.Match(wwwAuthenticate, "claims=\"([^\"]+)\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!match.Success) { return null; }
            var value = match.Groups[1].Value;
            try
            {
                var padded = value.Replace('-', '+').Replace('_', '/');
                padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
                return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
            }
            catch (FormatException)
            {
                // Some servers send the JSON itself rather than base64.
                return value;
            }
        }

        // The latest claims challenge the TFS client libraries saw, consumed by the next renewal.
        private string _pendingClaims;

        internal void NoteChallenge(string wwwAuthenticate)
        {
            var claims = ClaimsFromChallenge(wwwAuthenticate);
            if (claims != null) { lock (_cacheLock) { _pendingClaims = claims; } }
        }

        internal string TakePendingClaims()
        {
            lock (_cacheLock)
            {
                var claims = _pendingClaims;
                _pendingClaims = null;
                return claims;
            }
        }

        /// <summary>
        /// VssCredentials for the Azure DevOps client libraries. The token is refreshed when the server
        /// rejects it, so long-running migrations survive the roughly one hour token lifetime.
        /// </summary>
        public VssCredentials CreateVssCredentials()
            => new VssCredentials(new EntraIdFederatedCredential(this));

        internal sealed class EntraIdFederatedCredential : FederatedCredential
        {
            private readonly EntraIdCredentials _owner;

            public EntraIdFederatedCredential(EntraIdCredentials owner) : base(new VssOAuthAccessToken(owner.GetAccessToken()))
            {
                _owner = owner;
            }

            public override VssCredentialsType CredentialType => VssCredentialsType.OAuth;

            public override bool IsAuthenticationChallenge(IHttpResponse webResponse)
            {
                var challenge = webResponse != null && (webResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized || webResponse.StatusCode == System.Net.HttpStatusCode.Found);
                if (challenge && webResponse.Headers != null && webResponse.Headers.TryGetValues("WWW-Authenticate", out var values))
                {
                    foreach (var value in values) { _owner.NoteChallenge(value); }
                }
                return challenge;
            }

            protected override IssuedTokenProvider OnCreateTokenProvider(Uri serverUrl, IHttpResponse response)
                => new EntraIdTokenProvider(this, _owner, serverUrl);
        }

        internal sealed class EntraIdTokenProvider : IssuedTokenProvider
        {
            private readonly EntraIdCredentials _owner;

            public EntraIdTokenProvider(IssuedTokenCredential credential, EntraIdCredentials owner, Uri serverUrl) : base(credential, serverUrl, serverUrl)
            {
                _owner = owner;
            }

            public override bool GetTokenIsInteractive => false;

            protected override async Task<IssuedToken> OnGetTokenAsync(IssuedToken failedToken, CancellationToken cancellationToken)
            {
                var rejected = (failedToken as VssOAuthAccessToken)?.Value;
                return new VssOAuthAccessToken(await _owner.GetFreshAccessTokenAsync(rejected, _owner.TakePendingClaims(), cancellationToken).ConfigureAwait(false));
            }
        }

        // DefaultAzureCredential reports the credential it settled on only through Azure.Identity's process-wide
        // event source (#3225), so with Default on more than one endpoint the name may belong to either.
        private static class SelectionListener
        {
            private static readonly object Lock = new object();
            private static AzureEventSourceListener _listener;

            public static string Selected { get; private set; }

            public static void Clear() => Selected = null;

            public static void Ensure()
            {
                lock (Lock)
                {
                    if (_listener != null) { return; }
                    _listener = new AzureEventSourceListener((e, message) =>
                    {
                        if (e.EventName == "DefaultAzureCredentialCredentialSelected" && e.Payload != null && e.Payload.Count > 0)
                        {
                            var name = e.Payload[0]?.ToString();
                            Selected = name?.Substring(name.LastIndexOf('.') + 1);
                        }
                    }, System.Diagnostics.Tracing.EventLevel.Informational);
                }
            }
        }
    }
}
