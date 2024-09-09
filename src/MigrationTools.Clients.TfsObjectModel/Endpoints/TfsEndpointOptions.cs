using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            else if (!Uri.IsWellFormedUriString(options.Collection.ToString(), UriKind.Absolute))
            {
                errors.Add("The Collection property must be a valid URL.");
            }

            // Validate Project - Must not be null or empty
            if (string.IsNullOrWhiteSpace(options.Project))
            {
                errors.Add("The Project property must not be null or empty.");
            }

            // Validate ReflectedWorkItemIdField - Must not be null or empty
            if (string.IsNullOrWhiteSpace(options.ReflectedWorkItemIdField))
            {
                errors.Add("The ReflectedWorkItemIdField property must not be null or empty.");
            }

            // Validate LanguageMaps - Must exist
            if (options.LanguageMaps == null)
            {
                errors.Add("The LanguageMaps property must exist.");
            }

            // Validate Authentication - Must exist
            if (options.Authentication == null)
            {
                errors.Add("The Authentication property must exist.");
            }
            else
            {
                // Validate Authentication properties based on AuthenticationMode
                switch (options.Authentication.AuthenticationMode)
                {
                    case AuthenticationMode.AccessToken:
                        if (string.IsNullOrWhiteSpace(options.Authentication.AccessToken))
                        {
                            errors.Add("The AccessToken must not be null or empty when AuthenticationMode is set to 'AccessToken'.");
                        }
                        break;

                    case AuthenticationMode.Windows:
                        if (options.Authentication.NetworkCredentials == null)
                        {
                            errors.Add("The NetworkCredentials must be provided when AuthenticationMode is set to 'Windows'.");
                        }
                        break;
                    case AuthenticationMode.Prompt:
                        break;
                    default:
                        errors.Add($"The AuthenticationMode '{options.Authentication.AuthenticationMode}' is not supported.");
                        break;
                }
            }

            // Return failure if there are errors, otherwise success
            if (errors.Count > 0)
            {
                return ValidateOptionsResult.Fail(string.Join(Environment.NewLine, errors));
            }

            return ValidateOptionsResult.Success;
        }
    }
}
