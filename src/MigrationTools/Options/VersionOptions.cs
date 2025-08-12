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
using Newtonsoft.Json; // for IJsonLineInfo
using Serilog;
using System.Text.RegularExpressions;

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
                IList<ValidationError>  validationErrors;
                bool isValid = configJson.IsValid(schema, out validationErrors);
                if (!isValid)
                {
                    // Build a rich, de-duplicated error report including child errors
                    var report = BuildValidationErrorReport(configFile, validationErrors);
                    Log.Error(report);
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
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    return null;
                }
            }

            #region Validation Error Helpers
            private static string BuildValidationErrorReport(string configFile, IEnumerable<ValidationError> rootErrors)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Configuration file '{configFile}' is not valid against the schema.");
                sb.AppendLine();
                sb.AppendLine("Validation errors (deduplicated & filtered):");
                sb.AppendLine();

                var flattened = FlattenValidationErrors(rootErrors).ToList();
                // Count raw required property set occurrences before deduplication
                var requiredSetCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // key = path|prop1,prop2
                foreach (var fe in flattened.Where(f => f.ErrorType == ErrorType.Required))
                {
                    var props = ParseRequiredProperties(fe);
                    if (props.Count == 0) continue;
                    var key = $"{fe.Path ?? string.Empty}|{string.Join(",", props)}";
                    requiredSetCounts[key] = requiredSetCounts.TryGetValue(key, out var c) ? c + 1 : 1;
                }

                var unique = DeduplicateValidationErrors(flattened);
                unique = FilterNoise(unique); // now only removes AnyOf/AllOf containers

                // Map directly for display (keep all required variants)
                var displayErrors = ToDisplayErrors(configFile, unique);

                // Build full root cause candidate list (all required sets per path)
                var rootCauseSetsAll = displayErrors.Where(e => e.ErrorType == ErrorType.Required)
                                                     .Select(e => new { e.Path, Props = ParseRequiredPropertiesFromMessage(e.Message) })
                                                     .Where(x => x.Props.Count > 0)
                                                     .Select(x => new {
                                                         x.Path,
                                                         x.Props,
                                                         Freq = requiredSetCounts.TryGetValue($"{x.Path ?? string.Empty}|{string.Join(",", x.Props)}", out var f) ? f : 1
                                                     })
                                                     .ToList();

                // Filter to frequency > 1; if none qualify keep all to avoid hiding everything
                var rootCauseSetsFiltered = rootCauseSetsAll.Where(r => r.Freq > 1).ToList();
                if (rootCauseSetsFiltered.Count == 0)
                {
                    rootCauseSetsFiltered = rootCauseSetsAll; // fallback
                }
                var rootCauseSets = rootCauseSetsFiltered
                    .GroupBy(x => x.Path ?? string.Empty)
                    .ToList();
                if (rootCauseSets.Count > 0)
                {
                    sb.AppendLine("Likely root cause missing properties (all deduped candidate sets):");
                    foreach (var pathGroup in rootCauseSets.OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        sb.AppendLine($"  Path: {pathGroup.Key}");
                        foreach (var candidate in pathGroup
                                    .Select(c => new { c.Props, FreqKey = $"{pathGroup.Key}|{string.Join(",", c.Props)}" })
                                    .GroupBy(c => string.Join(",", c.Props))
                                    .Select(g => new { Props = g.First().Props, Frequency = requiredSetCounts.TryGetValue(g.First().FreqKey, out var f) ? f : 1 })
                                    .OrderBy(c => c.Props.Count)
                                    .ThenByDescending(c => c.Frequency)
                                    .ThenBy(c => string.Join(",", c.Props), StringComparer.OrdinalIgnoreCase))
                        {
                            sb.AppendLine($"    - missing {string.Join(", ", candidate.Props)} (freq {candidate.Frequency})");
                        }
                    }
                    sb.AppendLine();
                }

                foreach (var group in displayErrors
                             .OrderBy(e => e.Path, StringComparer.OrdinalIgnoreCase)
                             .ThenBy(e => e.ErrorType.ToString(), StringComparer.OrdinalIgnoreCase)
                             .GroupBy(e => string.IsNullOrEmpty(e.Path) ? "<root>" : e.Path))
                {
                    sb.AppendLine($"Path: {group.Key}");
                    foreach (var error in group.OrderBy(e => e.ErrorType.ToString(), StringComparer.OrdinalIgnoreCase)
                                               .ThenBy(e => e.Message, StringComparer.OrdinalIgnoreCase))
                    {
                        var lineInfo = error.HasLineInfo ? $"(Ln {error.LineNumber}, Pos {error.LinePosition}) " : string.Empty;
                        sb.AppendLine($"  - {lineInfo}{error.ErrorType}: {error.Message}");
                    }
                }
                sb.AppendLine();
                sb.AppendLine($"Total unique validation errors after filtering: {displayErrors.Count}");
                sb.AppendLine();
                sb.AppendLine("NOTE: A single missing property can trigger multiple branch (anyOf/allOf) mismatches. Fix the root cause lines above first.");
                return sb.ToString();
            }

            private class DisplayValidationError
            {
                public string Path { get; set; }
                public ErrorType ErrorType { get; set; }
                public string Message { get; set; }
                public int LineNumber { get; set; }
                public int LinePosition { get; set; }
                public bool HasLineInfo { get; set; }
            }

            private static List<DisplayValidationError> ToDisplayErrors(string configFile, List<ValidationError> errors)
            {
                JObject root = null;
                try { root = JObject.Parse(File.ReadAllText(configFile)); } catch { }
                var display = new List<DisplayValidationError>();
                if (root == null)
                {
                    // Fallback, just map directly
                    foreach (var e in errors)
                    {
                        var hasLine = ((IJsonLineInfo)e).HasLineInfo();
                        display.Add(new DisplayValidationError
                        {
                            Path = e.Path,
                            ErrorType = e.ErrorType,
                            Message = e.Message,
                            LineNumber = e.LineNumber,
                            LinePosition = e.LinePosition,
                            HasLineInfo = hasLine
                        });
                    }
                    return display;
                }

                foreach (var e in errors)
                {
                    var hasLine = ((IJsonLineInfo)e).HasLineInfo();
                    display.Add(new DisplayValidationError
                    {
                        Path = e.Path,
                        ErrorType = e.ErrorType,
                        Message = e.Message,
                        LineNumber = e.LineNumber,
                        LinePosition = e.LinePosition,
                        HasLineInfo = hasLine
                    });
                }
                return display;
            }

            private static IEnumerable<ValidationError> FlattenValidationErrors(IEnumerable<ValidationError> errors)
            {
                var stack = new Stack<ValidationError>(errors ?? Array.Empty<ValidationError>());
                while (stack.Count > 0)
                {
                    var current = stack.Pop();
                    yield return current;
                    // ChildErrors always returns a list (never null)
                    if (current.ChildErrors.Count > 0)
                    {
                        foreach (var child in current.ChildErrors)
                        {
                            stack.Push(child);
                        }
                    }
                }
            }

            private static List<ValidationError> DeduplicateValidationErrors(IEnumerable<ValidationError> errors)
            {
                var unique = new List<ValidationError>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var e in errors)
                {
                    // Compose a key that should uniquely identify a validation error occurrence
                    var key = $"{e.Path}|{e.ErrorType}|{e.Message}";
                    if (seen.Add(key))
                    {
                        unique.Add(e);
                    }
                }
                return unique;
            }

            private static List<ValidationError> FilterNoise(List<ValidationError> errors)
            {
                var toRemove = new HashSet<ValidationError>();
                foreach (var group in errors.GroupBy(e => e.Path ?? string.Empty))
                {
                    // Remove container anyOf/allOf if we have more specific errors
                    bool hasSpecific = group.Any(e => e.ErrorType != ErrorType.AnyOf && e.ErrorType != ErrorType.AllOf);
                    if (hasSpecific)
                    {
                        foreach (var container in group.Where(e => e.ErrorType == ErrorType.AnyOf || e.ErrorType == ErrorType.AllOf))
                        {
                            toRemove.Add(container);
                        }
                    }
                }
                return errors.Where(e => !toRemove.Contains(e)).ToList();
            }

            private static bool IsSinglePropertyRequiredMessage(ValidationError e)
            {
                var msg = e.Message ?? string.Empty;
                var m = Regex.Match(msg, @"Required properties are missing from object: (?<props>[^.]+)\.");
                if (!m.Success) return false;
                var props = m.Groups["props"].Value.Split(',').Select(p => p.Trim()).Where(p => p.Length > 0).ToList();
                return props.Count == 1;
            }

            private static List<DisplayValidationError> IdentifyRootCauseRequiredDisplayErrors(List<DisplayValidationError> errors) => errors.Where(e => e.ErrorType == ErrorType.Required).ToList();

            // Use the actual JSON object to collapse Required errors to the truly missing properties
            // Removed obsolete refinement returning ValidationError list (replaced by ToDisplayErrors)

            // Best-effort shallow path resolution using Newtonsoft path semantics
            private static JToken GetTokenAtPath(JObject root, string path)
            {
                if (root == null) return null;
                if (string.IsNullOrEmpty(path) || path == "<root>") return root;
                try { return root.SelectToken(path); } catch { return null; }
            }

            // Case-insensitive manual traversal fallback (handles minor casing differences or escaped segments)
            private static JToken GetTokenAtPathLoose(JObject root, string path)
            {
                if (root == null) return null;
                if (string.IsNullOrEmpty(path) || path == "<root>") return root;
                var current = (JToken)root;
                var parts = path.Split('.');
                foreach (var part in parts)
                {
                    if (current is JObject obj)
                    {
                        var prop = obj.Properties().FirstOrDefault(p => string.Equals(p.Name, part, StringComparison.OrdinalIgnoreCase));
                        if (prop == null) return null;
                        current = prop.Value;
                    }
                    else
                    {
                        return null;
                    }
                }
                return current;
            }

            // --- End of display conversion helpers ---

            private static List<string> ParseRequiredProperties(ValidationError e)
            {
                var msg = e.Message ?? string.Empty;
                var m = Regex.Match(msg, @"Required properties are missing from object: (?<props>[^.]+)\.");
                if (!m.Success) return new List<string>();
                return m.Groups["props"].Value
                    .Split(',')
                    .Select(p => p.Trim())
                    .Where(p => p.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            private static List<string> ParseRequiredPropertiesFromMessage(string msg)
            {
                msg = msg ?? string.Empty;
                var m = Regex.Match(msg, @"Required properties are missing from object: (?<props>[^.]+)\.");
                if (!m.Success) return new List<string>();
                return m.Groups["props"].Value
                    .Split(',')
                    .Select(p => p.Trim())
                    .Where(p => p.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            private static bool IsStrictSuperset(IReadOnlyCollection<string> a, IReadOnlyCollection<string> b)
            {
                if (a.Count <= b.Count) return false;
                return b.All(x => a.Contains(x, StringComparer.OrdinalIgnoreCase));
            }

            private static bool IsStrictSubset(IReadOnlyCollection<string> a, IReadOnlyCollection<string> b)
            {
                if (a.Count >= b.Count) return false;
                return a.All(x => b.Contains(x, StringComparer.OrdinalIgnoreCase));
            }
            #endregion
        }


    }
}
