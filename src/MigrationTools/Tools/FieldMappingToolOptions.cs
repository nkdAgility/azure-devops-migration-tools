using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    public class FieldMappingToolOptions : ToolOptions
    {
        public List<IFieldMapOptions> FieldMaps { get; set; } = new List<IFieldMapOptions>();


        public class ConfigureOptions : IConfigureOptions<FieldMappingToolOptions>
        {
            private readonly IConfiguration _configuration;

            public ConfigureOptions(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public void Configure(FieldMappingToolOptions options)
            {
                IConfigurationSection cfg = _configuration.GetSection(options.ConfigurationMetadata.PathToInstance);
                options.Enabled = cfg.GetValue(nameof(Enabled), defaultValue: false);
                options.FieldMaps = _configuration.GetSection(options.ConfigurationMetadata.PathToInstance + ":FieldMaps")
                    ?.ToMigrationToolsList(child => child.GetMigrationToolsOption<IFieldMapOptions>("FieldMapType"));
            }
        }
    }
}
