using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using Serilog;
using static System.Collections.Specialized.BitVector32;

namespace MigrationTools.Processors.Infrastructure
{
    public class ProcessorContainerOptions 
    {
        public const string ConfigurationSectionName = "MigrationTools:Processors";

        public List<IProcessorConfig> Processors { get; set; } = new List<IProcessorConfig>();

        public class ConfigureOptions : IConfigureOptions<ProcessorContainerOptions>
        {
            private readonly IConfiguration _configuration;

            public ConfigureOptions(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public void Configure(ProcessorContainerOptions options)
            {
                switch (VersionOptions.ConfigureOptions.GetMigrationConfigVersion(_configuration).schema)
                {
                    case MigrationConfigSchema.v160:
                        BindProcessorOptions(options, ConfigurationSectionName, "ProcessorType");
                        break;
                    case MigrationConfigSchema.v1:
                        BindProcessorOptions(options, "Processors", "$type");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                        break;
                }
            }

            private void BindProcessorOptions(ProcessorContainerOptions options, string sectionName, string objectTypePropertyName)
            {
                _configuration.GetSection(sectionName).Bind(options);

                foreach (var processorSection in _configuration.GetSection(sectionName).GetChildren())
                {
                    var processorTypeString = processorSection.GetValue<string>(objectTypePropertyName);
                    if (processorTypeString == null)
                    {
                        Log.Warning("There was no value for {optionTypeName} from {sectionKey}", objectTypePropertyName, processorSection.Key);
                        throw new Exception();
                    }
                    var processorType = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<IProcessorOptions>().WithNameString(processorTypeString);
                    if (processorType == null)
                    {
                        Log.Warning("There was no match for {optionTypeName} from {sectionKey}", objectTypePropertyName, processorSection.Key);
                        throw new Exception();
                    }

                    IProcessorOptions processorOption = Activator.CreateInstance(processorType) as IProcessorOptions;
                    // get sefaults and bind
                    _configuration.GetSection(processorOption.ConfigurationMetadata.PathToInstance).Bind(processorOption);
                    // Bind collection item
                    processorSection.Bind(processorOption);

                    // Bind enrichers for each processor
                    var enrichersSection = processorSection.GetSection("Enrichers");
                    var enrichers = enrichersSection?.ToMigrationToolsList(child => child.GetMigrationToolsOption<IProcessorEnricherOptions>("EnricherType"));
                    if (processorOption.Enrichers == null)
                    {
                        processorOption.Enrichers = new List<IProcessorEnricherOptions>();
                    }
                    processorOption.Enrichers.AddRange(enrichers);
                    options.Processors.Add(processorOption);
                }
            }
        }

    }
}