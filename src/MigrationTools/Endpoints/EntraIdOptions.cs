using System;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.Options.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Endpoints
{
    /// <summary>
    /// The Microsoft Entra ID credential an endpoint signs in with.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntraIdCredentialType
    {
        /// <summary>A Credential value in the configuration that is not one of the credentials below; reported by validation.</summary>
        Unknown = -1,
        /// <summary>The first sign-in found on the machine: environment service principal, workload identity, managed identity, Visual Studio, Azure CLI, Azure PowerShell or azd. AZURE_TOKEN_CREDENTIALS limits it to one of them.</summary>
        Default = 0,
        /// <summary>The account signed in with 'az login'.</summary>
        AzureCli,
        /// <summary>A user who signs in once in a browser; later runs reuse the saved sign-in without prompting.</summary>
        Interactive,
        /// <summary>A user who signs in once with a device code on any device; later runs reuse the saved sign-in without prompting.</summary>
        DeviceCode,
        /// <summary>An app registration (service principal) with a certificate.</summary>
        Certificate,
        /// <summary>An app registration (service principal) with a client secret.</summary>
        ClientSecret,
        /// <summary>The managed identity of the Azure machine the tool runs on.</summary>
        ManagedIdentity,
    }

    /// <summary>
    /// Chooses the Microsoft Entra ID identity an endpoint signs in with when AuthenticationMode is EntraId. Leave it out to use the first sign-in found on the machine (for example 'az login').
    /// </summary>
    public class EntraIdOptions
    {
        /// <summary>
        /// The credential to sign in with: Default, AzureCli, Interactive, DeviceCode, Certificate, ClientSecret or ManagedIdentity.
        /// </summary>
        /// <default>Default</default>
        public string Credential { get; set; } = nameof(EntraIdCredentialType.Default);

        /// <summary>
        /// Credential as a type; Unknown when it is not one of the credential names. Credential is a string rather
        /// than the enum so a mistyped name reaches Validate and gets its message instead of failing the schema or binding.
        /// </summary>
        [JsonIgnore]
        public EntraIdCredentialType CredentialType =>
            string.IsNullOrWhiteSpace(Credential)
                ? EntraIdCredentialType.Default
                : Enum.TryParse(Credential.Trim(), true, out EntraIdCredentialType parsed) && !int.TryParse(Credential, out _) && parsed != EntraIdCredentialType.Unknown
                    ? parsed
                    : EntraIdCredentialType.Unknown;

        /// <summary>
        /// The Microsoft Entra tenant (GUID or domain). Required for Certificate and ClientSecret; optional for AzureCli, Interactive and DeviceCode.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TenantId { get; set; }

        /// <summary>
        /// The application (client) ID. Required for Certificate and ClientSecret; for ManagedIdentity, the user-assigned identity; for Interactive and DeviceCode, a public client app of your own.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ClientId { get; set; }

        /// <summary>
        /// Certificate credential: thumbprint of a certificate with a private key in CurrentUser\My or LocalMachine\My. Set this or CertificatePath.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CertificateThumbprint { get; set; }

        /// <summary>
        /// Certificate credential: path to a PFX or PEM file holding the certificate and private key. Set this or CertificateThumbprint.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CertificatePath { get; set; }

        /// <summary>
        /// Certificate credential: password of the file at CertificatePath. Set it through the environment variable rather than the config file.
        /// </summary>
        [JsonConverter(typeof(DefaultOnlyConverter<string>), "** removed as a secret ***")]
        public string CertificatePassword { get; set; }

        /// <summary>
        /// ClientSecret credential: the app's client secret. Set it through the environment variable rather than the config file.
        /// </summary>
        [JsonConverter(typeof(DefaultOnlyConverter<string>), "** removed as a secret ***")]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Interactive and DeviceCode: the account to sign in as (for example user@contoso.com).
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LoginHint { get; set; }

        /// <summary>
        /// Interactive and DeviceCode: where the saved sign-in (account and tenant, no secret) is kept. Defaults to a file per endpoint under the user's local application data folder.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AuthenticationRecordPath { get; set; }

        private bool IsUserSignIn => CredentialType == EntraIdCredentialType.Interactive || CredentialType == EntraIdCredentialType.DeviceCode;

        /// <summary>
        /// Every problem with the settings for the chosen credential, each as one message naming the endpoint, the setting and what to do.
        /// </summary>
        public IReadOnlyList<string> Validate(string endpointName)
        {
            var errors = new List<string>();
            var setting = $"MigrationTools:Endpoints:{endpointName}:Authentication:EntraId";
            void Need(string name, string value)
            {
                if (string.IsNullOrWhiteSpace(value)) { errors.Add($"Endpoint '{endpointName}': the {CredentialType} Entra ID credential needs {name}. Set {setting}:{name}."); }
            }
            void Unused(string name, string value)
            {
                if (!string.IsNullOrWhiteSpace(value)) { errors.Add($"Endpoint '{endpointName}': the {CredentialType} Entra ID credential does not use {name}. Remove {setting}:{name}, or change Credential."); }
            }
            void Guid(string name, string value, bool allowDomain)
            {
                if (string.IsNullOrWhiteSpace(value) || System.Guid.TryParse(value, out _)) { return; }
                if (allowDomain && value.Contains(".") && !value.Contains(" ")) { return; }
                errors.Add($"Endpoint '{endpointName}': {name} '{value}' is not a {(allowDomain ? "tenant ID (GUID) or domain name" : "GUID")}. Copy it from the app registration's Overview page.");
            }

            if (CredentialType == EntraIdCredentialType.Unknown)
            {
                var known = Enum.GetNames(typeof(EntraIdCredentialType)).Where(n => n != nameof(EntraIdCredentialType.Unknown));
                errors.Add($"Endpoint '{endpointName}': {setting}:Credential '{Credential}' is not a known Entra ID credential. Use one of {string.Join(", ", known)}.");
                return errors;
            }

            switch (CredentialType)
            {
                case EntraIdCredentialType.Default:
                    Unused(nameof(TenantId), TenantId);
                    Unused(nameof(ClientId), ClientId);
                    break;
                case EntraIdCredentialType.AzureCli:
                    Unused(nameof(ClientId), ClientId);
                    break;
                case EntraIdCredentialType.Interactive:
                case EntraIdCredentialType.DeviceCode:
                    break;
                case EntraIdCredentialType.Certificate:
                    Need(nameof(TenantId), TenantId);
                    Need(nameof(ClientId), ClientId);
                    var hasThumbprint = !string.IsNullOrWhiteSpace(CertificateThumbprint);
                    var hasPath = !string.IsNullOrWhiteSpace(CertificatePath);
                    if (hasThumbprint == hasPath)
                    {
                        errors.Add($"Endpoint '{endpointName}': the Certificate Entra ID credential needs exactly one of CertificateThumbprint or CertificatePath. Set one of them under {setting}.");
                    }
                    if (hasThumbprint) { Unused(nameof(CertificatePassword), CertificatePassword); }
                    Unused(nameof(ClientSecret), ClientSecret);
                    break;
                case EntraIdCredentialType.ClientSecret:
                    Need(nameof(TenantId), TenantId);
                    Need(nameof(ClientId), ClientId);
                    Need(nameof(ClientSecret), ClientSecret);
                    break;
                case EntraIdCredentialType.ManagedIdentity:
                    Unused(nameof(TenantId), TenantId);
                    break;
            }

            if (CredentialType != EntraIdCredentialType.Certificate)
            {
                Unused(nameof(CertificateThumbprint), CertificateThumbprint);
                Unused(nameof(CertificatePath), CertificatePath);
                Unused(nameof(CertificatePassword), CertificatePassword);
            }
            if (CredentialType != EntraIdCredentialType.ClientSecret && CredentialType != EntraIdCredentialType.Certificate)
            {
                Unused(nameof(ClientSecret), ClientSecret);
            }
            if (!IsUserSignIn)
            {
                Unused(nameof(LoginHint), LoginHint);
                Unused(nameof(AuthenticationRecordPath), AuthenticationRecordPath);
            }

            Guid(nameof(TenantId), TenantId, allowDomain: true);
            Guid(nameof(ClientId), ClientId, allowDomain: false);
            return errors;
        }
    }
}
