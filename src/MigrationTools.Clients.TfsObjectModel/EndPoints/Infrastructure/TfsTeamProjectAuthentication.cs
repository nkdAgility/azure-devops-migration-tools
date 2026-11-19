using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MigrationTools.Options;
using MigrationTools.Options.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;


namespace MigrationTools.Endpoints.Infrastructure
{
    public class TfsAuthenticationOptions : IValidateOptions<TfsAuthenticationOptions>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [Required]
        public AuthenticationMode AuthenticationMode { get; set; }

        [JsonProperty( NullValueHandling = NullValueHandling.Ignore)]
        public NetworkCredentials NetworkCredentials { get; set; }

        [JsonConverter(typeof(DefaultOnlyConverter<string>), "** removed as a secret ***")]
        public string AccessToken { get; set; }

        /// <summary>
        /// The Microsoft Entra ID identity to sign in with when AuthenticationMode is EntraId. Leave it out to use the first sign-in found on the machine, for example 'az login'.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public EntraIdOptions EntraId { get; set; }

        private EntraIdCredentials _entraIdCredentials;
        private readonly object _entraIdLock = new object();

        /// <summary>This endpoint's own Entra ID credentials, created on first use.</summary>
        public EntraIdCredentials GetEntraIdCredentials(string endpointName = null)
        {
            lock (_entraIdLock)
            {
                _entraIdCredentials = _entraIdCredentials ?? new EntraIdCredentials(EntraId, endpointName);
                return _entraIdCredentials.NameEndpoint(endpointName);
            }
        }

        /// <summary>
        /// The token to send as the password of Basic auth for REST calls: the PAT, or a fresh Microsoft Entra
        /// token in EntraId mode (Azure DevOps accepts both in place of each other). Null when neither applies.
        /// </summary>
        public string ResolveAccessToken()
        {
            return AuthenticationMode == AuthenticationMode.EntraId ? GetEntraIdCredentials().GetAccessToken() : AccessToken;
        }

        /// <summary>
        /// Credentials for a VssConnection the processors open themselves: in EntraId mode the endpoint's renewing
        /// credential (a new token on 401), otherwise the PAT. Null when neither applies.
        /// </summary>
        public Microsoft.VisualStudio.Services.Common.VssCredentials CreateRestCredentials()
        {
            if (AuthenticationMode == AuthenticationMode.EntraId) { return GetEntraIdCredentials().CreateVssCredentials(); }
            return string.IsNullOrEmpty(AccessToken) ? null : new Microsoft.VisualStudio.Services.Common.VssBasicCredential(string.Empty, AccessToken);
        }

        public ValidateOptionsResult Validate(string name, TfsAuthenticationOptions options)
        {
            var errors = new List<string>();
            if (options.AuthenticationMode == AuthenticationMode.EntraId && options.EntraId != null)
            {
                errors.AddRange(options.EntraId.Validate(name));
            }
            else if (options.EntraId != null)
            {
                Log.Warning("Endpoint '{EndpointName}': the EntraId settings are ignored because AuthenticationMode is {AuthenticationMode}.", name, options.AuthenticationMode);
            }
            // Validate Authentication properties based on AuthenticationMode
            switch (options.AuthenticationMode)
            {
                case AuthenticationMode.AccessToken:
                    if (string.IsNullOrWhiteSpace(options.AccessToken))
                    {
                        errors.Add("The AccessToken must not be null or empty when AuthenticationMode is set to 'AccessToken'. You must provide a PAT to use 'AccessToken' as the authentication mode. You can set this through the config at 'MigrationTools:Endpoints:{name}:Authentication:AccessToken', or you can set an environment variable of 'MigrationTools__Endpoints__{name}__Authentication__AccessToken'. Check the docs on https://devopsmigration.io/Reference/Endpoints/TfsTeamProjectEndpoint/");
                    }
                    break;

                case AuthenticationMode.Windows:
                    if (options.NetworkCredentials == null)
                    {
                        errors.Add("The NetworkCredentials must be provided when AuthenticationMode is set to 'Windows'.");
                    } else
                    {
                        ValidateOptionsResult result = options.NetworkCredentials.Validate(name, options.NetworkCredentials);
                        if (!result.Succeeded)
                        {
                            errors.AddRange(result.Failures);
                        }
                    }
                    break;
                case AuthenticationMode.Prompt:
                case AuthenticationMode.EntraId:
                    break;
                default:
                    errors.Add($"The AuthenticationMode '{options.AuthenticationMode}' is not supported.");
                    break;
            }
            if (errors.Any())
            {
                Log.Debug("TfsAuthenticationOptions::Validate::Fail");
                return ValidateOptionsResult.Fail(errors);
            }
            Log.Debug("TfsAuthenticationOptions::Validate::Success");
            return ValidateOptionsResult.Success;
        }
    }
}
