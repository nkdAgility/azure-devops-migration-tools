using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using MigrationTools.Endpoints.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using TfsUrlParser;

namespace MigrationTools.Endpoints
{

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TfsProductVersion
    {
        /// <summary>
        /// Represents legacy Team Foundation Server versions prior to 2013. Not technically supported but may work.
        /// </summary>
        OnPremisesClassic,

        /// <summary>
        /// Represents on-premises Team Foundation Server 2013+ and Azure DevOps Server (default).
        /// </summary>
        OnPremises,

        /// <summary>
        /// Represents Azure DevOps Services (cloud version).
        /// </summary>
        Cloud
    }

    /// <summary>
    /// Configuration options for connecting to a Team Foundation Server (TFS) or Azure DevOps Server endpoint. Provides authentication and project access settings for on-premises TFS operations.
    /// </summary>
    public class TfsEndpointOptions : EndpointOptions
    {
        /// <summary>
        /// URI of the TFS collection (e.g., "http://tfsserver:8080/tfs/DefaultCollection"). Must be a valid absolute URL pointing to the TFS collection.
        /// </summary>
        [JsonProperty(Order = -3)]
        [Required]
        public Uri Collection { get; set; }

        /// <summary>
        /// Name of the TFS project within the collection to connect to. This is the project that will be used for migration operations.
        /// </summary>
        [JsonProperty(Order = -2)]
        [Required]
        public string Project { get; set; }

        /// <summary>
        /// Authentication configuration for connecting to the TFS server. Supports various authentication modes including Windows authentication and access tokens.
        /// </summary>
        [Required]
        public TfsAuthenticationOptions Authentication { get; set; }

        /// <summary>
        /// Name of the custom field used to store the reflected work item ID for tracking migrated items. Typically "Custom.ReflectedWorkItemId".
        /// </summary>
        [JsonProperty(Order = -1)]
        public string ReflectedWorkItemIdField { get; set; } = "Custom.ReflectedWorkItemId";

        /// <summary>
        /// When true, allows work items to link to items in different projects within the same collection. Default is false for security and organizational clarity.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool AllowCrossProjectLinking { get; set; } = false;

        /// <summary>
        /// Language mapping configuration for translating area and iteration path names between different language versions of TFS.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TfsLanguageMapOptions LanguageMaps { get; set; } = new TfsLanguageMapOptions() { AreaPath = "Area", IterationPath = "Iteration" };

        /// <summary>
        /// Specifies the TFS product version for compatibility and feature support. Default is OnPremises for TFS 2013+ and Azure DevOps Server.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TfsProductVersion ProductVersion { get; set; } = TfsProductVersion.OnPremises;
    }

    public class TfsEndpointOptionsValidator : IValidateOptions<TfsEndpointOptions>
    {
        public ValidateOptionsResult Validate(string name, TfsEndpointOptions options)
        {
            var errors = new List<string>();

            // Validate Collection - Required and must be a valid URL
            if (options.Collection == null)
            {
                errors.Add("The Collection property must not be null.");
            }
            else
            {
                Uri output;
                if (!Uri.TryCreate(Uri.UnescapeDataString(options.Collection.ToString()), UriKind.Absolute, out output))
                {
                    errors.Add("The Collection property must be a valid URL.");
                }

            }

            // Validate Project - Must not be null or empty
            if (string.IsNullOrWhiteSpace(options.Project))
            {
                errors.Add("The Project property must not be null or empty.");
            }

            // Validate ReflectedWorkItemIdField - Must not be null or empty
            if (string.IsNullOrWhiteSpace(options.ReflectedWorkItemIdField))
            {
                errors.Add("The ReflectedWorkItemIdField property must not be null or empty. Check the docs on https://devopsmigration.io/setup/reflectedworkitemid/");
            }

            // Validate LanguageMaps - Must exist
            if (options.LanguageMaps == null)
            {
                errors.Add("The LanguageMaps property must exist.");
            }
            else
            {
                ValidateOptionsResult lmr = options.LanguageMaps.Validate(name, options.LanguageMaps);
                if (lmr != ValidateOptionsResult.Success)
                {
                    errors.AddRange(lmr.Failures);
                }
            }

            // Validate Authentication - Must exist
            if (options.Authentication == null)
            {
                errors.Add("The Authentication property must exist.");
            }
            else
            {
                ValidateOptionsResult lmr = options.Authentication.Validate(name, options.Authentication);
                if (lmr != ValidateOptionsResult.Success)
                {
                    errors.AddRange(lmr.Failures);
                }
            }

            // Return failure if there are errors, otherwise success
            if (errors.Any())
            {
                Log.Debug("TfsEndpointOptionsValidator::Validate::Fail");
                return ValidateOptionsResult.Fail(errors);
            }
            Log.Debug("TfsEndpointOptionsValidator::Validate::Success");
            return ValidateOptionsResult.Success;
        }
    }
}
