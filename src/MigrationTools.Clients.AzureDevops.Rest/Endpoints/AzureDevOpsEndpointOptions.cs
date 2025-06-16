using System.ComponentModel.DataAnnotations;
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
        [Required]
        public string AccessToken { get; set; }

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


        //public override void SetDefaults()
        //{
        //    base.SetDefaults();
        //    AccessToken = MigrationTools.Tests.TestingConstants.AccessToken;
        //    Organisation = "https://dev.azure.com/nkdagility-preview/";
        //    Project = "NeedToSetThis";
        //    ReflectedWorkItemIdField = "Custom.ReflectedWorkItemId";
        //}
    }
}