using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Configuration options for the FieldMappingTool, defining the collection of field mappings to apply during work item migration.
    /// </summary>
    public class FieldMappingToolOptions : ToolOptions
    {
        /// <summary>
        /// Gets or sets the list of field mapping configurations to apply.
        /// </summary>
        public List<IFieldMapOptions> FieldMaps { get; set; } = new List<IFieldMapOptions>();


        /// <summary>
        /// Configuration provider for field mapping tool options, handling the loading of field map configurations from configuration sources.
        /// </summary>
        public class ConfigureOptions : IConfigureOptions<FieldMappingToolOptions>
        {
            private readonly IConfiguration _configuration;

            /// <summary>
            /// Initializes a new instance of the ConfigureOptions class.
            /// </summary>
            /// <param name="configuration">The configuration provider</param>
            public ConfigureOptions(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            /// <summary>
            /// Configures the field mapping tool options from the configuration source.
            /// </summary>
            /// <param name="options">The options instance to configure</param>
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
