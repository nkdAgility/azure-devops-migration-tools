using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MigrationTools.Options
{
    public enum MigrationConfigSchema
    {
        v1,
        v160
    }

    public class VersionOptions
    {
        public MigrationConfigSchema ConfigSchemaVersion { get; set; }
        public Version ConfigVersion { get; set; }
        public string ConfigVersionString { get; set; }

        public class ConfigureOptions : IConfigureOptions<VersionOptions>
        {
            private readonly IConfiguration _configuration;

            public ConfigureOptions(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public void Configure(VersionOptions options)
            {
                options.ConfigVersionString = _configuration.GetValue<string>("MigrationTools:Version");
                options.ConfigVersion = Version.Parse(options.ConfigVersionString);
                options.ConfigSchemaVersion = GetMigrationConfigVersion(_configuration);
                
            }

            public static MigrationConfigSchema GetMigrationConfigVersion(IConfiguration configuration)
            {
                bool isOldFormat = false;
                string configVersionString = configuration.GetValue<string>("MigrationTools:Version");
                if (string.IsNullOrEmpty(configVersionString))
                {
                    isOldFormat = true;
                    configVersionString = configuration.GetValue<string>("Version");
                }
                if (string.IsNullOrEmpty(configVersionString))
                {
                    configVersionString = "0.0";
                }
                Version.TryParse(configVersionString, out Version configVersion);
                if (configVersion < Version.Parse("16.0") || isOldFormat)
                {
                    Console.WriteLine("!!ACTION REQUIRED!! You are using a deprecated version of the configuration, please update to v16. backward compatability will be removed in a future version.");
                    return MigrationConfigSchema.v1;
                }
                else
                {
                    return MigrationConfigSchema.v160;
                }
            }
        }


    }
}
