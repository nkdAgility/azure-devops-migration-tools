using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Options;

namespace MigrationTools.Processors
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
                        _configuration.GetSection(ConfigurationSectionName).Bind(options);
                        options.Processors = _configuration.GetSection(ProcessorContainerOptions.ConfigurationSectionName)?.ToMigrationToolsList(child => child.GetMigrationToolsOption<IProcessorConfig>("ProcessorType"));
                        foreach (var processor in options.Processors)
                        {
                            // Bind enrichers for each processor
                            var enrichersSection = _configuration.GetSection($"MigrationTools:Processors:{options.Processors.IndexOf(processor)}:Enrichers");
                            var enrichers = enrichersSection?.ToMigrationToolsList(child => child.GetMigrationToolsOption<IProcessorEnricher>("EnricherType"));
                            if (processor.Enrichers != null)
                            {
                                processor.Enrichers = new List<IProcessorEnricher>();
                            }
                            processor.Enrichers.AddRange(enrichers);
                        }
                        break;
                    case MigrationConfigSchema.v1:
                        options.Processors = _configuration.GetSection("Processors")?.ToMigrationToolsList(child => child.GetMigrationToolsOption<IProcessorConfig>("$type"));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                        break;
                }
            }
        }

    }
}