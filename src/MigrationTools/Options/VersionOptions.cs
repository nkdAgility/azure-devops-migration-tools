﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MigrationTools.Options
{
    public enum MigrationConfigSchema
    {
        v1,
        v150,
        v160,
        Empty
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
                (MigrationConfigSchema, string) result = GetMigrationConfigVersion(_configuration);
                options.ConfigVersionString = result.Item2;
                options.ConfigVersion = Version.Parse(options.ConfigVersionString);
                options.ConfigSchemaVersion = result.Item1;
                
            }

                public static (MigrationConfigSchema schema, string str) GetMigrationConfigVersion(IConfiguration configuration)
            {
                if (configuration.GetChildren().Any())
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
                        if (configVersion < Version.Parse("15.0"))
                        {
                            return (MigrationConfigSchema.v1, configVersionString);
                        }
                        else
                        {
                            return (MigrationConfigSchema.v150, configVersionString);
                        }
                    }
                    else
                    {
                        return (MigrationConfigSchema.v160, configVersionString);
                    }
                } else
                {
                    return (MigrationConfigSchema.Empty, "0.0");
                }
                
            }

            public static bool IsConfigValid(IConfiguration configuration)
            {
                var isValid = true;
                switch (GetMigrationConfigVersion(configuration).schema)
                {
                    case MigrationConfigSchema.v1:
                        isValid = false;
                        break;
                    case MigrationConfigSchema.v160:
                        // This is the corect version
                        break;
                    default:
                        isValid = false;
                        break;
                }
                return isValid;

            }
        }


    }
}
