using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using MigrationTools.Options.Infrastructure;
using Newtonsoft.Json;

namespace MigrationTools.Options
{
    [Obsolete]
    public class NetworkCredentialsOptions
    {
        public NetworkCredentials Source { get; set; }
        public NetworkCredentials Target { get; set; }
    }

    public class NetworkCredentials : IValidateOptions<NetworkCredentials>
    {
        public string Domain { get; set; }
        public string UserName { get; set; }

        [JsonConverter(typeof(DefaultOnlyConverter<string>), "** removed as a secret ***")]
        public string Password { get; set; }

        public ValidateOptionsResult Validate(string name, NetworkCredentials options)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(options.Domain))
            {
                errors.Add("The Domain must not be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(options.UserName))
            {
                errors.Add("The UserName must not be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(options.Password))
            {
                errors.Add("The Password must not be null or empty.");
            }
            if (errors.Any())
            {
                return ValidateOptionsResult.Fail(errors);
            }
            return ValidateOptionsResult.Success;
        }
    }
}
