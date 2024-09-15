using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MigrationTools.Options;
using MigrationTools.Options.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace MigrationTools.Endpoints.Infrastructure
{
    public class TfsAuthenticationOptions : IValidateOptions<TfsAuthenticationOptions>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationMode AuthenticationMode { get; set; }

        public NetworkCredentials NetworkCredentials { get; set; }

        [JsonConverter(typeof(DefaultOnlyConverter<string>), "** removed as a secret ***")]
        public string AccessToken { get; set; }

        public ValidateOptionsResult Validate(string name, TfsAuthenticationOptions options)
        {
            var errors = new List<string>();
            // Validate Authentication properties based on AuthenticationMode
            switch (options.AuthenticationMode)
            {
                case AuthenticationMode.AccessToken:
                    if (string.IsNullOrWhiteSpace(options.AccessToken))
                    {
                        errors.Add("The AccessToken must not be null or empty when AuthenticationMode is set to 'AccessToken'.");
                    }
                    break;

                case AuthenticationMode.Windows:
                    if (options.NetworkCredentials == null)
                    {
                        errors.Add("The NetworkCredentials must be provided when AuthenticationMode is set to 'Windows'.");
                    }
                    break;
                case AuthenticationMode.Prompt:
                    break;
                default:
                    errors.Add($"The AuthenticationMode '{options.AuthenticationMode}' is not supported.");
                    break;
            }
            if (errors.Any())
            {
                return ValidateOptionsResult.Fail(errors);
            }
            return ValidateOptionsResult.Success;
        }
    }
}
