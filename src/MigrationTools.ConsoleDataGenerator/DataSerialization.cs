using MigrationTools.ConsoleDataGenerator.ReferenceData;
using MigrationTools.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace MigrationTools.ConsoleDataGenerator
{
    public class DataSerialization
    {

        private readonly string dataPath;
        private readonly string schemaPath;

        public DataSerialization(string rootPath)
        {
            dataPath = Path.Combine(rootPath, "docs/data/classes");
            schemaPath = Path.Combine(rootPath, "docs/static/schema");
            // No longer need referencePath since we're not generating markdown
        }

        public string WriteYamlDataToDataFolder(ClassData data)
        {
            // Ensure the directory exists
            Directory.CreateDirectory(dataPath);

            string filename = $"reference.{data.TypeName}.{data.ClassName}";
            string filePath = Path.Combine(dataPath, filename.ToLower());
            string yaml = SeraliseDataToYaml(data);
            File.WriteAllText($"{filePath}.yaml", yaml);
            return yaml;
        }

        public string WriteJsonSchemaToDataFolder(ClassData data, List<ClassData> allClassData = null)
        {
            // Ensure the schema directory exists
            Directory.CreateDirectory(schemaPath);

            string filename = $"schema.{data.TypeName}.{data.ClassName}";
            string filePath = Path.Combine(schemaPath, filename.ToLower());
            string jsonSchema = GenerateJsonSchemaForClass(data, allClassData);
            File.WriteAllText($"{filePath}.json", jsonSchema);
            return jsonSchema;
        }

        public static string GenerateJsonSchemaForClass(ClassData data, List<ClassData> allClassData = null)
        {
            try
            {
                // Create a JSON schema using Newtonsoft.Json.Schema
                var generator = new JSchemaGenerator();

                // Configure the generator
                generator.SchemaIdGenerationHandling = SchemaIdGenerationHandling.TypeName;
                generator.SchemaLocationHandling = SchemaLocationHandling.Definitions;

                // Create a basic object schema
                var schema = new JSchema
                {
                    SchemaVersion = new Uri("https://json-schema.org/draft/2020-12/schema"),
                    Id = new Uri($"https://devopsmigration.io/schema/schema.{data.TypeName.ToLower()}.{data.ClassName.ToLower()}.json"),
                    Title = data.ClassName,
                    Description = data.Description,
                    Type = JSchemaType.Object
                };

                // Add properties from the class data
                foreach (var option in data.Options)
                {
                    var propertyName = option.ParameterName.Substring(0, 1).ToLower() + option.ParameterName.Substring(1); // camelCase
                    var propertySchema = new JSchema
                    {
                        Type = GetJsonSchemaType(option.Type.ToString()),
                        Description = option.Description.ToString()
                    };

                    // Special handling for FieldMappingTool's fieldMaps property
                    if (data.ClassName.Equals("FieldMappingTool", StringComparison.OrdinalIgnoreCase) &&
                        propertyName.Equals("fieldMaps", StringComparison.OrdinalIgnoreCase) &&
                        allClassData != null)
                    {
                        // Get all field map classes
                        var fieldMaps = allClassData.Where(cd => cd.TypeName.Equals("FieldMaps", StringComparison.OrdinalIgnoreCase)).ToList();

                        if (fieldMaps.Any())
                        {
                            // Create JSON manually using prefixItems with anyOf (same as working individual schema)
                            var arraySchemaJson = new JObject();
                            arraySchemaJson["type"] = "array";
                            arraySchemaJson["description"] = option.Description.ToString();

                            // Create prefixItems as an array containing the anyOf object
                            var anyOfArray = new JArray();
                            foreach (var fieldMap in fieldMaps)
                            {
                                var fieldMapSchema = CreateSchemaFromClassData(fieldMap);
                                anyOfArray.Add(JObject.Parse(fieldMapSchema.ToString()));
                            }
                            var anyOfObj = new JObject();
                            anyOfObj["anyOf"] = anyOfArray;
                            var prefixItemsArray = new JArray();
                            prefixItemsArray.Add(anyOfObj);
                            arraySchemaJson["prefixItems"] = prefixItemsArray;

                            propertySchema = JSchema.Parse(arraySchemaJson.ToString());
                        }
                    }

                    // Add default value if available and not "missing XML code comments"
                    if (option.DefaultValue != null &&
                        !option.DefaultValue.ToString().Contains("missing XML code comments") &&
                        !string.IsNullOrEmpty(option.DefaultValue.ToString()))
                    {
                        propertySchema.Default = JToken.FromObject(option.DefaultValue);
                    }

                    schema.Properties.Add(propertyName, propertySchema);
                }
                // Add required properties
                var requiredProperties = data.Options
                    .Where(opt => opt.IsRequired)
                    .Select(opt => opt.ParameterName.Substring(0, 1).ToLower() + opt.ParameterName.Substring(1))
                    .ToList();
                foreach (var req in requiredProperties)
                {
                    schema.Required.Add(req);
                }

                return schema.ToString(SchemaVersion.Draft2020_12);
            }
            catch (Exception ex)
            {
                return $"{{ \"error\": \"Failed to generate schema for {data.ClassName}: {ex.Message}\" }}";
            }
        }

        private static JSchemaType GetJsonSchemaType(string dotnetType)
        {
            const string stringType = "string";
            const string booleanType = "boolean";

            return dotnetType.ToLower() switch
            {
                stringType => JSchemaType.String,
                "int32" => JSchemaType.Integer,
                "int64" => JSchemaType.Integer,
                booleanType => JSchemaType.Boolean,
                "double" => JSchemaType.Number,
                "decimal" => JSchemaType.Number,
                "datetime" => JSchemaType.String,
                _ when dotnetType.Contains("List") => JSchemaType.Array,
                _ when dotnetType.Contains("Dictionary") => JSchemaType.Object,
                _ => JSchemaType.String // Default to string for unknown types
            };
        }

        public string GenerateFullConfigurationSchema(List<ClassData> allClassData)
        {
            try
            {
                var fullSchema = new JSchema
                {
                    SchemaVersion = new Uri("https://json-schema.org/draft/2020-12/schema"),
                    Id = new Uri("https://devopsmigration.io/schema/configuration.schema.json"),
                    Title = "Azure DevOps Migration Tools Configuration",
                    Description = "Complete configuration schema for Azure DevOps Migration Tools",
                    Type = JSchemaType.Object
                };

                // Add top-level structure
                var migrationToolsSchema = new JSchema { Type = JSchemaType.Object };

                // Add version property
                migrationToolsSchema.Properties.Add("version", new JSchema
                {
                    Type = JSchemaType.String,
                    Description = "Version of the migration tools configuration format"
                });

                // Add endpoints section
                var endpointsSchema = new JSchema { Type = JSchemaType.Object };
                var endpointClasses = allClassData.Where(cd => cd.TypeName.Equals("Endpoints", StringComparison.OrdinalIgnoreCase)).ToList();

                foreach (var endpointClass in endpointClasses)
                {
                    var endpointSchema = CreateSchemaFromClassData(endpointClass);
                    endpointsSchema.Properties.Add(endpointClass.ClassName.ToLower(), endpointSchema);
                }
                migrationToolsSchema.Properties.Add("endpoints", endpointsSchema);

                // Add processors section
                var processorsSchema = new JSchema { Type = JSchemaType.Array };
                var processorClasses = allClassData.Where(cd => cd.TypeName.Equals("Processors", StringComparison.OrdinalIgnoreCase)).ToList();

                if (processorClasses.Any())
                {
                    // Manually construct prefixItems as an array containing an anyOf object
                    var anyOfArray = new JArray();
                    foreach (var processorClass in processorClasses)
                    {
                        var processorSchema = CreateSchemaFromClassData(processorClass);
                        processorSchema.Properties.Add("processorType", new JSchema
                        {
                            Type = JSchemaType.String,
                            Enum = { processorClass.ClassName }
                        });
                        anyOfArray.Add(JObject.Parse(processorSchema.ToString()));
                    }
                    var anyOfObj = new JObject();
                    anyOfObj["anyOf"] = anyOfArray;
                    var prefixItemsArray = new JArray();
                    prefixItemsArray.Add(anyOfObj);

                    // Use JObject to set prefixItems, then parse back to JSchema
                    var processorsSchemaJson = JObject.Parse(processorsSchema.ToString());
                    processorsSchemaJson["prefixItems"] = prefixItemsArray;
                    processorsSchema = JSchema.Parse(processorsSchemaJson.ToString());
                }
                migrationToolsSchema.Properties.Add("processors", processorsSchema);

                // Add tools section
                var toolsSchema = new JSchema { Type = JSchemaType.Object };
                var toolClasses = allClassData.Where(cd => cd.TypeName.Equals("Tools", StringComparison.OrdinalIgnoreCase)).ToList();

                foreach (var toolClass in toolClasses)
                {
                    // For FieldMappingTool, use the enhanced schema generation with field maps
                    // For other tools, use the simple CreateSchemaFromClassData approach
                    // This avoids parsing issues with the complex prefixItems structure
                    if (toolClass.ClassName.Equals("FieldMappingTool", StringComparison.OrdinalIgnoreCase))
                    {
                        // Generate the enhanced schema but use CreateSchemaFromClassData for main config
                        // to avoid parsing the complex prefixItems structure
                        var enhancedToolSchema = CreateSchemaFromClassData(toolClass);

                        // Add the fieldMaps property manually with the prefixItems structure
                        var fieldMaps = allClassData.Where(cd => cd.TypeName.Equals("FieldMaps", StringComparison.OrdinalIgnoreCase)).ToList();
                        if (fieldMaps.Any())
                        {
                            // Create the fieldMaps property with prefixItems structure like the working individual schema
                            var fieldMapsSchema = new JSchema
                            {
                                Type = JSchemaType.Array,
                                Description = "Gets or sets the list of field mapping configurations to apply."
                            };

                            // Manually construct prefixItems as an array containing an anyOf object
                            var anyOfArray = new JArray();
                            foreach (var fieldMap in fieldMaps)
                            {
                                var fieldMapSchema = CreateSchemaFromClassData(fieldMap);
                                anyOfArray.Add(JObject.Parse(fieldMapSchema.ToString()));
                            }
                            var anyOfObj = new JObject();
                            anyOfObj["anyOf"] = anyOfArray;
                            var prefixItemsArray = new JArray();
                            prefixItemsArray.Add(anyOfObj);

                            // Use JObject to set prefixItems, then parse back to JSchema
                            var fieldMapsSchemaJson = JObject.Parse(fieldMapsSchema.ToString());
                            fieldMapsSchemaJson["prefixItems"] = prefixItemsArray;
                            fieldMapsSchema = JSchema.Parse(fieldMapsSchemaJson.ToString());

                            // Replace the fieldMaps property
                            if (enhancedToolSchema.Properties.ContainsKey("fieldMaps"))
                            {
                                enhancedToolSchema.Properties["fieldMaps"] = fieldMapsSchema;
                            }
                        }

                        toolsSchema.Properties.Add(toolClass.ClassName.ToLower(), enhancedToolSchema);
                    }
                    else
                    {
                        var toolSchema = CreateSchemaFromClassData(toolClass);
                        toolsSchema.Properties.Add(toolClass.ClassName.ToLower(), toolSchema);
                    }
                }
                migrationToolsSchema.Properties.Add("commonTools", toolsSchema);

                fullSchema.Properties.Add("MigrationTools", migrationToolsSchema);

                // Add Serilog section for logging configuration
                var serilogSchema = new JSchema
                {
                    Type = JSchemaType.Object,
                    Description = "Serilog logging configuration"
                };
                serilogSchema.Properties.Add("MinimumLevel", new JSchema
                {
                    Type = JSchemaType.String,
                    Description = "Minimum logging level",
                    Enum = { "Verbose", "Debug", "Information", "Warning", "Error", "Fatal" }
                });
                fullSchema.Properties.Add("Serilog", serilogSchema);

                return fullSchema.ToString(SchemaVersion.Draft2020_12);
            }
            catch (Exception ex)
            {
                return $"{{ \"error\": \"Failed to generate full configuration schema: {ex.Message}\" }}";
            }
        }

        private static JSchema CreateSchemaFromClassData(ClassData classData)
        {
            var schema = new JSchema
            {
                Type = JSchemaType.Object,
                Title = classData.ClassName,
                Description = classData.Description
            };

            foreach (var option in classData.Options)
            {
                var propertyName = option.ParameterName.Substring(0, 1).ToLower() + option.ParameterName.Substring(1); // camelCase
                var propertySchema = new JSchema
                {
                    Type = GetJsonSchemaType(option.Type.ToString()),
                    Description = option.Description.ToString()
                };

                // Add default value if available and not "missing XML code comments"
                if (option.DefaultValue != null &&
                    !option.DefaultValue.ToString().Contains("missing XML code comments") &&
                    !string.IsNullOrEmpty(option.DefaultValue.ToString()))
                {
                    propertySchema.Default = JToken.FromObject(option.DefaultValue);
                }

                schema.Properties.Add(propertyName, propertySchema);
            }

            return schema;
        }

        public string WriteFullConfigurationSchema(List<ClassData> allClassData)
        {
            // Ensure the schema directory exists
            Directory.CreateDirectory(schemaPath);

            string filePath = Path.Combine(schemaPath, "configuration.schema.json");
            string jsonSchema = GenerateFullConfigurationSchema(allClassData);
            File.WriteAllText(filePath, jsonSchema);
            return jsonSchema;
        }


        public string SeraliseDataToJson(object data)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None, // Disable automatic $type handling
                Formatting = Formatting.Indented,        // For better readability
                NullValueHandling = NullValueHandling.Ignore // Ignore null values
            };

            // Add our custom converter if a type is specified
            if (data is IOptions)
            {
                settings.Converters.Add(new ConditionalTypeConverter(data.GetType()));
            }

            return JsonConvert.SerializeObject(data, settings);
        }

        public static string SeraliseDataToYaml(object data)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return serializer.Serialize(data);
        }
    }

    public class ConditionalTypeConverter : JsonConverter
    {
        private readonly Type _typeToInclude;

        public ConditionalTypeConverter(Type typeToInclude)
        {
            _typeToInclude = typeToInclude;
        }

        public override bool CanConvert(Type objectType)
        {
            return true; // This converter applies to all types, but we'll decide whether to include $type inside WriteJson
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            JObject jo = JObject.FromObject(value);

            // Include $type only if the object is of the specified type
            if (value.GetType() == _typeToInclude)
            {
                jo.AddFirst(new JProperty("$type", value.GetType().Name));
            }

            jo.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            // Deserialize normally
            return serializer.Deserialize(reader, objectType);
        }
    }

}
