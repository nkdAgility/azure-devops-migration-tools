using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public class ProcessorContainerOptions : ProcessorEnricherOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:Processors";
        public override Type ToConfigure => typeof(ProcessorContainer);

        public List<IProcessorConfig> Processors { get; set; } = new List<IProcessorConfig>();

        public override void SetDefaults()
        {
            Enabled = false;
        }


        public class ConfigureOptions : IConfigureOptions<ProcessorContainerOptions>
        {
            private readonly IConfiguration _configuration;

            public ConfigureOptions(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public void Configure(ProcessorContainerOptions options)
            {
                switch (_configuration.GetMigrationConfigVersion())
                {
                    case ConfigurationExtensions.MigrationConfigVersion.v16:
                        _configuration.GetSection(ConfigurationSectionName).Bind(options);
                        options.Processors = _configuration.GetSection(ProcessorContainerOptions.ConfigurationSectionName)?.ToMigrationToolsList(child => child.GetMigrationToolsOption<IProcessorConfig>("ProcessorType"));
                        break;
                    case ConfigurationExtensions.MigrationConfigVersion.before16:
                        options.Enabled = true;
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