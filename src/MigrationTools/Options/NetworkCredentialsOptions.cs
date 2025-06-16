using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using MigrationTools.Options.Infrastructure;
using Newtonsoft.Json;
using Serilog;

namespace MigrationTools.Options
{
    /// <summary>
    /// Configuration options for network credentials used when connecting to source and target systems. This class is marked as obsolete and should not be used in new implementations.
    /// </summary>
    [Obsolete]
    public class NetworkCredentialsOptions
    {
        /// <summary>
        /// Gets or sets the network credentials for connecting to the source system.
        /// </summary>
        public NetworkCredentials Source { get; set; }
        
        /// <summary>
        /// Gets or sets the network credentials for connecting to the target system.
        /// </summary>
        public NetworkCredentials Target { get; set; }
    }

    /// <summary>
    /// Represents network authentication credentials including domain, username, and password for connecting to TFS or other network-based systems.
    /// </summary>
    public class NetworkCredentials : IValidateOptions<NetworkCredentials>
    {
        /// <summary>
        /// Gets or sets the domain name for Windows authentication (e.g., "CONTOSO" or "contoso.com").
        /// </summary>
        public string Domain { get; set; }
        
        /// <summary>
        /// Gets or sets the username for authentication. Should not include the domain prefix.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password for authentication. This value is masked in JSON serialization for security.
        /// </summary>
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
                Log.Debug("NetworkCredentials::Validate::Fail");
                return ValidateOptionsResult.Fail(errors);
            }
            Log.Debug("NetworkCredentials::Validate::Success");
            return ValidateOptionsResult.Success;
        }
    }
}
