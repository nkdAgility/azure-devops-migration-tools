using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using MigrationTools.Exceptions;
using Serilog;

namespace MigrationTools.Endpoints
{
    /// <summary>
    /// Builds the Azure.Identity credential for an endpoint's <see cref="EntraIdOptions"/>.
    /// </summary>
    public static class EntraIdCredentialFactory
    {
        /// <summary>One OS-protected MSAL cache shared by every endpoint and run; MSAL keeps accounts and tenants apart inside it.</summary>
        public const string TokenCacheName = "MigrationTools";

        public static TokenCredential Create(EntraIdOptions options, string endpointName)
        {
            if (IsExperimental(options.CredentialType))
            {
                Log.Warning("Endpoint '{EndpointName}': the {Credential} Entra ID credential is experimental and has not yet been validated in a full migration. Try it on a test project first.", endpointName, options.CredentialType);
            }
            switch (options.CredentialType)
            {
                case EntraIdCredentialType.Default:
                    return new DefaultAzureCredential();
                case EntraIdCredentialType.AzureCli:
                    return new AzureCliCredential(new AzureCliCredentialOptions { TenantId = options.TenantId });
                case EntraIdCredentialType.Interactive:
                case EntraIdCredentialType.DeviceCode:
                    return CreateUserSignIn(options, endpointName);
                case EntraIdCredentialType.Certificate:
                    return new ClientCertificateCredential(options.TenantId, options.ClientId, LoadCertificate(options, endpointName));
                case EntraIdCredentialType.ClientSecret:
                    return new ClientSecretCredential(options.TenantId, options.ClientId, options.ClientSecret);
                case EntraIdCredentialType.ManagedIdentity:
                    return string.IsNullOrWhiteSpace(options.ClientId)
                        ? new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned)
                        : new ManagedIdentityCredential(ManagedIdentityId.FromUserAssignedClientId(options.ClientId));
                default:
                    throw new MigrationRunStoppedException($"Endpoint '{endpointName}': Entra ID Credential '{options.CredentialType}' is not known.");
            }
        }

        /// <summary>The app identities, which are built and unit-tested but not yet validated in a full migration.</summary>
        public static bool IsExperimental(EntraIdCredentialType type) =>
            type == EntraIdCredentialType.Certificate || type == EntraIdCredentialType.ClientSecret || type == EntraIdCredentialType.ManagedIdentity;

        internal static X509Certificate2 LoadCertificate(EntraIdOptions options, string endpointName)
        {
            if (!string.IsNullOrWhiteSpace(options.CertificatePath))
            {
                if (!File.Exists(options.CertificatePath))
                {
                    throw new MigrationRunStoppedException($"Endpoint '{endpointName}': the certificate file '{options.CertificatePath}' does not exist. Check CertificatePath.");
                }
                X509Certificate2 fromFile;
                try
                {
                    fromFile = new X509Certificate2(options.CertificatePath, options.CertificatePassword);
                }
                catch (System.Security.Cryptography.CryptographicException ex)
                {
                    throw new MigrationRunStoppedException($"Endpoint '{endpointName}': the certificate file '{options.CertificatePath}' could not be opened ({ex.Message.Trim()}). Check the file and CertificatePassword.", ex);
                }
                return RequirePrivateKey(fromFile, endpointName, options.CertificatePath);
            }

            var thumbprint = new string(options.CertificateThumbprint.Where(Uri.IsHexDigit).ToArray());
            foreach (var location in new[] { StoreLocation.CurrentUser, StoreLocation.LocalMachine })
            {
                using (var store = new X509Store(StoreName.My, location))
                {
                    store.Open(OpenFlags.ReadOnly);
                    var found = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false);
                    if (found.Count > 0)
                    {
                        return RequirePrivateKey(found[0], endpointName, $"{location}\\My");
                    }
                }
            }
            throw new MigrationRunStoppedException($"Endpoint '{endpointName}': no certificate with thumbprint {thumbprint} is in CurrentUser\\My or LocalMachine\\My. Import the certificate with its private key, or check CertificateThumbprint.");
        }

        private static X509Certificate2 RequirePrivateKey(X509Certificate2 certificate, string endpointName, string where)
        {
            if (!certificate.HasPrivateKey)
            {
                throw new MigrationRunStoppedException($"Endpoint '{endpointName}': the certificate {certificate.Thumbprint} in {where} has no private key, so it cannot sign in. Import it from a PFX that includes the key.");
            }
            return certificate;
        }

        /// <summary>
        /// Where an Interactive or DeviceCode endpoint keeps its saved sign-in: the configured path, or a file per
        /// endpoint, credential and tenant under the user's local application data folder.
        /// </summary>
        public static string AuthenticationRecordPath(EntraIdOptions options, string endpointName)
        {
            if (!string.IsNullOrWhiteSpace(options.AuthenticationRecordPath)) { return options.AuthenticationRecordPath; }
            var tenant = string.IsNullOrWhiteSpace(options.TenantId) ? "organizations" : options.TenantId;
            var name = $"{endpointName}-{options.CredentialType}-{tenant}";
            name = new string(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MigrationTools", "auth");
            return Path.Combine(folder, name + ".json");
        }

        private static TokenCredential CreateUserSignIn(EntraIdOptions options, string endpointName)
        {
            var recordPath = AuthenticationRecordPath(options, endpointName);
            var record = LoadRecord(recordPath, options.LoginHint, endpointName);
            var cache = new TokenCachePersistenceOptions { Name = TokenCacheName };

            TokenCredential credential;
            if (options.CredentialType == EntraIdCredentialType.Interactive)
            {
                var interactive = new InteractiveBrowserCredentialOptions
                {
                    TokenCachePersistenceOptions = cache,
                    AuthenticationRecord = record,
                    DisableAutomaticAuthentication = true,
                    LoginHint = options.LoginHint,
                };
                if (!string.IsNullOrWhiteSpace(options.TenantId)) { interactive.TenantId = options.TenantId; }
                if (!string.IsNullOrWhiteSpace(options.ClientId)) { interactive.ClientId = options.ClientId; }
                credential = new InteractiveBrowserCredential(interactive);
            }
            else
            {
                var deviceCode = new DeviceCodeCredentialOptions
                {
                    TokenCachePersistenceOptions = cache,
                    AuthenticationRecord = record,
                    DisableAutomaticAuthentication = true,
                    DeviceCodeCallback = (info, _) =>
                    {
                        Log.Information("Endpoint '{EndpointName}' Entra ID sign-in: {DeviceCodeMessage}", endpointName, info.Message);
                        return Task.CompletedTask;
                    },
                };
                if (!string.IsNullOrWhiteSpace(options.TenantId)) { deviceCode.TenantId = options.TenantId; }
                if (!string.IsNullOrWhiteSpace(options.ClientId)) { deviceCode.ClientId = options.ClientId; }
                credential = new DeviceCodeCredential(deviceCode);
            }

            if (record == null)
            {
                Log.Information("Endpoint '{EndpointName}': signing in to Microsoft Entra ID with {Credential}. This is needed once; later runs reuse the saved sign-in.", endpointName, options.CredentialType);
                record = SignIn(credential, endpointName, options.CredentialType);
                SaveRecord(record, recordPath);
                Log.Information("Endpoint '{EndpointName}': signed in as {Account}; sign-in saved to {RecordPath}.", endpointName, record.Username, recordPath);
            }
            return credential;
        }

        private static AuthenticationRecord SignIn(TokenCredential credential, string endpointName, EntraIdCredentialType type)
        {
            var context = new TokenRequestContext(new[] { EntraIdCredentials.AzureDevOpsScope });
            try
            {
                return credential is InteractiveBrowserCredential browser
                    ? browser.Authenticate(context, CancellationToken.None)
                    : ((DeviceCodeCredential)credential).Authenticate(context, CancellationToken.None);
            }
            catch (Exception ex) when (ex is AuthenticationFailedException || ex is OperationCanceledException)
            {
                throw new MigrationRunStoppedException($"Endpoint '{endpointName}': {type} sign-in was cancelled, timed out or failed ({Reason(ex)}). Run again and complete the sign-in.", ex);
            }
        }

        private static AuthenticationRecord LoadRecord(string path, string loginHint, string endpointName)
        {
            if (!File.Exists(path)) { return null; }
            try
            {
                AuthenticationRecord record;
                using (var stream = File.OpenRead(path))
                {
                    record = AuthenticationRecord.Deserialize(stream);
                }
                if (!string.IsNullOrWhiteSpace(loginHint) && !string.Equals(record.Username, loginHint, StringComparison.OrdinalIgnoreCase))
                {
                    Log.Information("Endpoint '{EndpointName}': the saved sign-in is for {Account}, not {LoginHint}; signing in again.", endpointName, record.Username, loginHint);
                    return null;
                }
                return record;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Text.Json.JsonException || ex is InvalidOperationException)
            {
                Log.Warning("Endpoint '{EndpointName}': the saved sign-in at {RecordPath} could not be read ({Reason}); signing in again.", endpointName, path, ex.Message);
                return null;
            }
        }

        private static void SaveRecord(AuthenticationRecord record, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            using (var stream = File.Create(path))
            {
                record.Serialize(stream);
            }
        }

        /// <summary>
        /// The reason for an Azure.Identity failure, in one line without a final full stop. Several credentials say only
        /// "authentication failed:" and carry the AADSTS reason in the MSAL inner exception, so that is added.
        /// </summary>
        internal static string Reason(Exception ex)
        {
            var reason = FirstLine(ex.Message);
            if (reason.EndsWith(":") && ex.InnerException != null) { reason += " " + FirstLine(ex.InnerException.Message); }
            return reason.TrimEnd('.');
        }

        /// <summary>
        /// The first line of an Azure.Identity message, joined to the next one when it only introduces it
        /// ("ClientSecretCredential authentication failed:" followed by the AADSTS reason).
        /// </summary>
        internal static string FirstLine(string message)
        {
            var lines = (message ?? string.Empty).Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToArray();
            if (lines.Length == 0) { return string.Empty; }
            var line = lines[0].EndsWith(":") && lines.Length > 1 ? lines[0] + " " + lines[1] : lines[0];
            // Entra appends request ids to the reason; they help support, not the reader of a one-line message.
            var ids = line.IndexOf(" Trace ID:", StringComparison.Ordinal);
            return ids > 0 ? line.Substring(0, ids).Trim() : line;
        }
    }
}
