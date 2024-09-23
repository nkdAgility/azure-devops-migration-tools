using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using MigrationTools.Endpoints.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TfsUrlParser;

namespace MigrationTools.Endpoints
{
    public class TfsEndpointOptions : EndpointOptions
    {
        [JsonProperty(Order = -3)]
        [Required]
        public Uri Collection { get; set; }
        [JsonProperty(Order = -2)]
        [Required]
        public string Project { get; set; }

        [Required]
        public TfsAuthenticationOptions Authentication { get; set; }

        [JsonProperty(Order = -1)]
        [Required]
        public string ReflectedWorkItemIdField { get; set; }
        public bool AllowCrossProjectLinking { get; set; }

        [Required]
        public TfsLanguageMapOptions LanguageMaps { get; set; }
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
                errors.Add("The ReflectedWorkItemIdField property must not be null or empty. Check the docs on https://nkdagility.com/learn/azure-devops-migration-tools/setup/reflectedworkitemid/");
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
                return ValidateOptionsResult.Fail(errors);
            }

            return ValidateOptionsResult.Success;
        }
    }
}
