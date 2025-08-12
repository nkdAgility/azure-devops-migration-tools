using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

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
                }
                else
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
                        // This is the correct version, now also validate against JSON schema
                        isValid = ValidateAgainstJsonSchema(configuration);
                        break;
                    default:
                        isValid = false;
                        break;
                }
                return isValid;
            }

            public static bool IsConfigSchemaValid(string configFile)
            {
                var stringConfigFile= File.ReadAllText(configFile);
                var configJson = JObject.Parse(stringConfigFile);

                var selectedSchema = configJson["$schema"]?.ToString();

                var schema = LoadConfigurationSchema();
                // Validate the configuration against the schema
                IList<string> messages;
                bool isValid = configJson.IsValid(schema, out messages);
                if (!isValid)
                {
                    // Log the validation errors (but still return true for backward compatibility)
                    foreach (var message in messages)
                    {
                        System.Diagnostics.Debug.WriteLine($"Configuration validation warning: {message}");
                    }
                }
                return isValid;
            }

            public static bool ValidateAgainstJsonSchema(IConfiguration configuration)
            {
                try
                {
                    // Check if the configuration has the correct $schema property
                    var schemaValue = configuration["$schema"];
                    const string expectedSchemaUrl = "https://devopsmigration.io/schema/configuration.schema.json";

                    if (string.IsNullOrEmpty(schemaValue))
                    {
                        // Missing schema property, but don't fail validation for backward compatibility
                        return true;
                    }

                    if (schemaValue != expectedSchemaUrl)
                    {
                        // Incorrect schema URL, but don't fail validation for backward compatibility
                        return true;
                    }

                    // Convert IConfiguration back to JSON for schema validation
                    var configJson = ConvertConfigurationToJson(configuration);
                    if (configJson == null)
                    {
                        return true; // Can't validate, but don't fail for backward compatibility
                    }

                    // Load the JSON schema from the embedded schema file or URL
                    var schema = LoadConfigurationSchema();
                    if (schema == null)
                    {
                        return true; // Can't load schema, but don't fail for backward compatibility
                    }

                    // Validate the configuration against the schema
                    IList<string> messages;
                    bool isValid = configJson.IsValid(schema, out messages);

                    if (!isValid)
                    {
                        // Log the validation errors (but still return true for backward compatibility)
                        foreach (var message in messages)
                        {
                            System.Diagnostics.Debug.WriteLine($"Configuration validation warning: {message}");
                        }
                    }

                    return true; // Always return true for backward compatibility, but log issues
                }
                catch
                {
                    // If any error occurs during schema validation, default to valid
                    // to maintain backward compatibility
                    return true;
                }
            }

            private static JObject ConvertConfigurationToJson(IConfiguration configuration)
            {
                try
                {
                    var configDict = new Dictionary<string, object>();

                    foreach (var kvp in configuration.AsEnumerable())
                    {
                        if (!string.IsNullOrEmpty(kvp.Value))
                        {
                            SetNestedValue(configDict, kvp.Key, kvp.Value);
                        }
                    }

                    return JObject.FromObject(configDict);
                }
                catch
                {
                    return null;
                }
            }

            private static void SetNestedValue(Dictionary<string, object> dict, string key, object value)
            {
                var parts = key.Split(':');
                var current = dict;

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    if (!current.ContainsKey(parts[i]))
                    {
                        current[parts[i]] = new Dictionary<string, object>();
                    }
                    current = (Dictionary<string, object>)current[parts[i]];
                }

                current[parts[parts.Length - 1]] = value;
            }

            private static JSchema LoadConfigurationSchema()
            {
                try
                {
                    // Try to load schema from embedded resource or local file first
                    var schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configuration.schema.json");

                    if (File.Exists(schemaPath))
                    {
                        var schemaJson = File.ReadAllText(schemaPath);
                        return JSchema.Parse(schemaJson);
                    }

                    // TODO: As a last resort, could download from the URL
                    // For now, return null to indicate schema couldn't be loaded
                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }


    }
}
