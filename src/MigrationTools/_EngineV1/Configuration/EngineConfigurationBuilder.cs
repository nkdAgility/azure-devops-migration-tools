using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using MigrationTools._EngineV1.Configuration.FieldMap;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Helpers;
using MigrationTools.Options;
using MigrationTools.Processors;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MigrationTools._EngineV1.Configuration
{
    public class EngineConfigurationBuilder : IEngineConfigurationBuilder, IEngineConfigurationReader, ISettingsWriter
    {
        private readonly ILogger<EngineConfigurationBuilder> _logger;

        public EngineConfigurationBuilder(ILogger<EngineConfigurationBuilder> logger)
        {
            _logger = logger;
        }

        public EngineConfiguration BuildFromFile(string configFile = "configuration.json")
        {
            EngineConfiguration ec;
            try
            {
                string configurationjson = File.ReadAllText(configFile);
                configurationjson = Upgrade118(configFile, configurationjson);
                ec = NewtonsoftHelpers.DeserializeObject<EngineConfiguration>(configurationjson);
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogTrace(ex, "Configuration Error");
                _logger.LogCritical("Your configuration file is malformed and cant be loaded!");
                _logger.LogError(ex.Message);
                _logger.LogError("How to Solve: Malformed Json is usually a result of editing errors. Validate that your {configFile} is valid Json!", configFile);
                Environment.Exit(-1);
                return null;
            }
            catch (JsonReaderException ex)
            {
                _logger.LogTrace(ex, "Configuration Error");
                _logger.LogCritical("Your configuration file was loaded but was unable to be mapped to ");
                _logger.LogError(ex.Message);
                _logger.LogError("How to Solve: Malformed configurations are usually a result of changes between versions. The best way to understand the change is to run 'devops-migration init' to create a new wel formed config and determin where the problem is!");
                Environment.Exit(-1);
                return null;
            }

            //var builder = new ConfigurationBuilder();
            //builder.SetBasePath(Directory.GetCurrentDirectory());
            //builder.AddJsonFile(configFile, optional: false, reloadOnChange: true);
            //IConfigurationRoot configuration = builder.Build();
            //var settings = new EngineConfiguration();
            //configuration.Bind(settings);
            //#if !DEBUG
            string appVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(2);
            if (ec?.Version != appVersion)
            {
                _logger.LogError("The config version {Version} does not match the current app version {appVersion}. There may be compatability issues and we recommend that you generate a new default config and then tranfer the settings accross.", ec.Version, appVersion);
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    _logger.LogInformation("But since you're running in Debug, let's move on");
                }
                else
                {
                    throw new Exception("Version in Config does not match X.X in Application. Please check and revert.");
                }
            }
            //#endif
            return ec;
        }

        private string Upgrade118(string configFile, string configurationjson)
        {
            if (configurationjson.Contains("ObjectType"))
            {
                configurationjson = configurationjson.Replace("ObjectType", "$type");
                File.WriteAllText(configFile, configurationjson);
                _logger.LogWarning("You config file is out of date! In 11.8 we changed `ObjectType` to `$type`! We have updated it for you just now!");
            }

            return configurationjson;
        }

        public EngineConfiguration BuildDefault()
        {
            EngineConfiguration ec = CreateEmptyConfig();
            AddExampleFieldMapps(ec);
            AddWorkItemMigrationDefault(ec);
            AddTestPlansMigrationDefault(ec);
            ec.Processors.Add(new ImportProfilePictureConfig());
            ec.Processors.Add(new ExportProfilePictureFromADConfig());
            ec.Processors.Add(new FixGitCommitLinksConfig() { TargetRepository = "targetProjectName" });
            ec.Processors.Add(new WorkItemUpdateConfig());
            ec.Processors.Add(new WorkItemPostProcessingConfig() { WorkItemIDs = new List<int> { 1, 2, 3 } });
            ec.Processors.Add(new WorkItemDeleteConfig());
            ec.Processors.Add(new WorkItemQueryMigrationConfig() { SourceToTargetFieldMappings = new Dictionary<string, string>() { { "SourceFieldRef", "TargetFieldRef" } } });
            ec.Processors.Add(new TeamMigrationConfig());
            return ec;
        }

        public EngineConfiguration BuildWorkItemMigration()
        {
            EngineConfiguration ec = CreateEmptyConfig();
            AddExampleFieldMapps(ec);
            AddWorkItemMigrationDefault(ec);
            return ec;
        }

        private void AddWorkItemMigrationDefault(EngineConfiguration ec)
        {
            var config = new WorkItemMigrationConfig
            {
                NodeBasePaths = new[] { "Product\\Area\\Path1", "Product\\Area\\Path2" }
            };
            ec.Processors.Add(config);
        }

        public EngineConfiguration CreateEmptyConfig()
        {
            EngineConfiguration ec = new EngineConfiguration
            {
                Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(2),
                FieldMaps = new List<IFieldMapConfig>(),
                WorkItemTypeDefinition = new Dictionary<string, string> {
                    { "sourceWorkItemTypeName", "targetWorkItemTypeName" }
                },
                Processors = new List<IProcessorConfig>(),
            };
            ec.Source = GetMigrationConfigDefault();
            ec.Target = GetMigrationConfigDefault();
            return ec;
        }

        private void AddTestPlansMigrationDefault(EngineConfiguration ec)
        {
            ec.Processors.Add(new TestVariablesMigrationConfig());
            ec.Processors.Add(new TestConfigurationsMigrationConfig());
            ec.Processors.Add(new TestPlansAndSuitesMigrationConfig());
            //ec.Processors.Add(new TestRunsMigrationConfig());
        }

        private void AddExampleFieldMapps(EngineConfiguration ec)
        {
            ec.FieldMaps.Add(new MultiValueConditionalMapConfig()
            {
                WorkItemTypeName = "*",
                sourceFieldsAndValues = new Dictionary<string, string>
                 {
                     { "Field1", "Value1" },
                     { "Field2", "Value2" }
                 },
                targetFieldsAndValues = new Dictionary<string, string>
                 {
                     { "Field1", "Value1" },
                     { "Field2", "Value2" }
                 }
            });
            ec.FieldMaps.Add(new FieldSkipMapConfig()
            {
                WorkItemTypeName = "*",
                targetField = "TfsMigrationTool.ReflectedWorkItemId"
            });
            ec.FieldMaps.Add(new FieldValueMapConfig()
            {
                WorkItemTypeName = "*",
                sourceField = "System.State",
                targetField = "System.State",
                defaultValue = "New",
                valueMapping = new Dictionary<string, string> {
                    { "Approved", "New" },
                    { "New", "New" },
                    { "Committed", "Active" },
                    { "In Progress", "Active" },
                    { "To Do", "New" },
                    { "Done", "Closed" },
                    { "Removed", "Removed" }
                }
            });
            ec.FieldMaps.Add(new FieldtoFieldMapConfig()
            {
                WorkItemTypeName = "*",
                sourceField = "Microsoft.VSTS.Common.BacklogPriority",
                targetField = "Microsoft.VSTS.Common.StackRank"
            });
            ec.FieldMaps.Add(new FieldtoFieldMultiMapConfig()
            {
                WorkItemTypeName = "*",
                SourceToTargetMappings = new Dictionary<string, string>
                {
                    {"SourceField1", "TargetField1" },
                    {"SourceField2", "TargetField2" }
                }
            });
            ec.FieldMaps.Add(new FieldtoTagMapConfig()
            {
                WorkItemTypeName = "*",
                sourceField = "System.State",
                formatExpression = "ScrumState:{0}"
            });
            ec.FieldMaps.Add(new FieldMergeMapConfig()
            {
                WorkItemTypeName = "*",
                sourceFields = new List<string> { "System.Description" , "Microsoft.VSTS.Common.AcceptanceCriteria" },
                targetField = "System.Description",
                formatExpression = @"{0} <br/><br/><h3>Acceptance Criteria</h3>{1}"
            });
            ec.FieldMaps.Add(new RegexFieldMapConfig()
            {
                WorkItemTypeName = "*",
                sourceField = "COMPANY.PRODUCT.Release",
                targetField = "COMPANY.DEVISION.MinorReleaseVersion",
                pattern = @"PRODUCT \d{4}.(\d{1})",
                replacement = "$1"
            });
            ec.FieldMaps.Add(new FieldValuetoTagMapConfig()
            {
                WorkItemTypeName = "*",
                sourceField = "Microsoft.VSTS.CMMI.Blocked",
                pattern = @"Yes",
                formatExpression = "{0}"
            });
            ec.FieldMaps.Add(new TreeToTagMapConfig()
            {
                WorkItemTypeName = "*",
                timeTravel = 1,
                toSkip = 3
            });
        }

        private IMigrationClientConfig GetMigrationConfigDefault()
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => typeof(IMigrationClientConfig).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .FirstOrDefault();
            if (type.BaseType == null)
            {
                throw new InvalidOperationException("No IMigrationClientConfig instance found in scope. Please make sure that you have implemented one!");
            }
            IMigrationClientConfig result = (IMigrationClientConfig)Activator.CreateInstance(type);
            result.PopulateWithDefault();
            return result;
        }

        public EngineConfiguration BuildDefault2()
        {
            EngineConfiguration ec = new EngineConfiguration
            {
                Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(2),
                FieldMaps = new List<IFieldMapConfig>(),
                WorkItemTypeDefinition = new Dictionary<string, string> {
                    { "sourceWorkItemTypeName", "targetWorkItemTypeName" }
                },
                Processors = new List<IProcessorConfig>(),
            };
            ec.Processors.Add(
                new WorkItemTrackingProcessorOptions
                {
                    Enabled = true,
                    CollapseRevisions = false,
                    PrefixProjectToNodes = false,
                    ReplayRevisions = true,
                    WorkItemCreateRetryLimit = 5,
                    ProcessorEnrichers = GetAllTypes<IProcessorEnricherOptions>(),
                    SourceName = "Source",
                    TargetName = "Target",
                });
            return ec;
        }

        public EngineConfiguration BuildWorkItemMigration2()
        {
            throw new NotImplementedException();
        }

        private object GetSpecificType(string typeName)
        {
            AppDomain.CurrentDomain.Load("MigrationTools");
            AppDomain.CurrentDomain.Load("MigrationTools.Clients.InMemory");
            //AppDomain.CurrentDomain.Load("MigrationTools.Clients.FileSystem");
            Type type = AppDomain.CurrentDomain.GetAssemblies()
               .Where(a => a.FullName.StartsWith("MigrationTools"))
               .SelectMany(a => a.GetTypes())
               .Where(t => !t.IsInterface && !t.IsAbstract && t.Name == typeName).SingleOrDefault();
            var option = Activator.CreateInstance(type);
            //type.GetMethod("SetDefaults", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(option, null);
            return option;
        }

        private List<TInterfaceToFind> GetAllTypes<TInterfaceToFind>() where TInterfaceToFind : IOptions
        {
            AppDomain.CurrentDomain.Load("MigrationTools");
            AppDomain.CurrentDomain.Load("MigrationTools.Clients.InMemory");
            //AppDomain.CurrentDomain.Load("MigrationTools.Clients.FileSystem");
            List<Type> types = AppDomain.CurrentDomain.GetAssemblies()
              .Where(a => a.FullName.StartsWith("MigrationTools"))
              .SelectMany(a => a.GetTypes())
              .Where(t => typeof(TInterfaceToFind).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && t.Name != "Object").ToList();
            List<TInterfaceToFind> output = new List<TInterfaceToFind>();
            foreach (Type type in types)
            {
                TInterfaceToFind option = (TInterfaceToFind)Activator.CreateInstance(type);
                option.SetDefaults();
                output.Add(option);
            }
            return output;
        }

        public void WriteSettings(EngineConfiguration engineConfiguration, string settingsFileName)
        {
            string json = NewtonsoftHelpers.SerializeObject(engineConfiguration);
            var engine = JObject.Parse(json);

            var endpoints = new JArray();
            var source = GetSpecificType("InMemoryWorkItemEndpointOptions") as EndpointOptions;
            source.Name = "Source";
            var sourceobj = (JObject)JToken.FromObject(source);
            endpoints.Add(sourceobj);
            var target = GetSpecificType("InMemoryWorkItemEndpointOptions") as EndpointOptions;
            target.Name = "Target";
            var targetobj = (JObject)JToken.FromObject(target);
            endpoints.Add(targetobj);

            var endpoint = new JObject();
            endpoint.Add("InMemoryWorkItemEndpoints", endpoints);
            engine.Add("Endpoints", endpoint);
            //engine.Endpoints.TfsEndpoints = endpoints;
            json = engine.ToString();
            //json = NewtonsoftHelpers.SerializeObject(engine);

            File.WriteAllText(settingsFileName, json);
        }
    }
}