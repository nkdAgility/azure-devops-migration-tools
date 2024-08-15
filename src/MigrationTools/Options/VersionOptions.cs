using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MigrationTools.Options
{
    public enum MigrationConfigVersion
    {
        before16,
        v16
    }

    public class VersionOptions
    {
        public MigrationConfigVersion ConfigVersion { get; set; }

        public class ConfigureOptions : IConfigureOptions<VersionOptions>
        {
            private readonly IConfiguration _configuration;

            public ConfigureOptions(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public void Configure(VersionOptions options)
            {
                options.ConfigVersion = GetMigrationConfigVersion(_configuration);

            }

            public static MigrationConfigVersion GetMigrationConfigVersion(IConfiguration configuration)
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
                    return MigrationConfigVersion.before16;
                }
                else
                {
                    return MigrationConfigVersion.v16;
                }
            }
        }


    }
}
