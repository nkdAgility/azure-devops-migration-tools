using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using MigrationTools.Endpoints.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Endpoints
{
    /// <summary>
    /// Configuration options for connecting to an Azure DevOps organization endpoint. Provides authentication and project access settings for Azure DevOps REST API operations.
    /// </summary>
    public class AzureDevOpsEndpointOptions : EndpointOptions
    {
        /// <summary>
        /// Authentication mode to use when connecting to Azure DevOps. Typically uses AccessToken for modern Azure DevOps organizations.
        /// </summary>
        [Required]
        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationMode AuthenticationMode { get; set; }

        /// <summary>
        /// Personal Access Token (PAT) or other authentication token for accessing the Azure DevOps organization. Required for API authentication.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// The token to send as the password of Basic auth: the PAT, or a fresh Microsoft Entra token when
        /// AuthenticationMode is EntraId (Azure DevOps accepts either).
        /// </summary>
        public string ResolveAccessToken()
        {
            return AuthenticationMode == AuthenticationMode.EntraId ? EntraIdCredentials.GetAccessToken() : AccessToken;
        }

        /// <summary>
        /// URL of the Azure DevOps organization (e.g., "https://dev.azure.com/myorganization/"). Must include the full organization URL.
        /// </summary>
        [Required]
        public string Organisation { get; set; }

        /// <summary>
        /// Name of the Azure DevOps project within the organization to connect to. This is the project that will be used for migration operations.
        /// </summary>
        [Required]
        public string Project { get; set; }

        /// <summary>
        /// Name of the custom field used to store the reflected work item ID for tracking migrated items. Typically "Custom.ReflectedWorkItemId".
        /// </summary>
        [Required]
        public string ReflectedWorkItemIdField { get; set; }
    }

    /// <summary>
    /// An AccessToken is required only in AccessToken mode; EntraId gets its token from the ambient identity.
    /// </summary>
    public class AzureDevOpsEndpointOptionsValidator : IValidateOptions<AzureDevOpsEndpointOptions>
    {
        public ValidateOptionsResult Validate(string name, AzureDevOpsEndpointOptions options)
        {
            switch (options.AuthenticationMode)
            {
                case AuthenticationMode.AccessToken:
                    if (string.IsNullOrWhiteSpace(options.AccessToken))
                    {
                        return ValidateOptionsResult.Fail($"The AccessToken must not be empty when AuthenticationMode is 'AccessToken'. Set it at 'MigrationTools:Endpoints:{name}:AccessToken' or the environment variable 'MigrationTools__Endpoints__{name}__AccessToken', or use AuthenticationMode 'EntraId'.");
                    }
                    return ValidateOptionsResult.Success;
                case AuthenticationMode.EntraId:
                    return ValidateOptionsResult.Success;
                default:
                    return ValidateOptionsResult.Fail($"The AuthenticationMode '{options.AuthenticationMode}' is not supported by the Azure DevOps REST endpoint. Use 'AccessToken' or 'EntraId'.");
            }
        }
    }
}