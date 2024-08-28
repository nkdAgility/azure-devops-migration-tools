using System;
using System.Text.Json.Serialization;
using MigrationTools.Options;

namespace MigrationTools.Enrichers
{
    public abstract class ProcessorEnricherOptions : IProcessorEnricherOptions
    {
        [JsonIgnore]
        public string OptionFor => $"{GetType().Name.Replace("Options", "")}";

        [JsonIgnore]
        public ConfigurationMetadata ConfigurationMetadata => new ConfigurationMetadata
        {
            PathToInstance = null,
            ObjectName = $"ProcessorEnricherType",
            OptionFor = OptionFor,
            PathToDefault = $"MigrationTools::ProcessorEnricherDefaults:{OptionFor}",
            PathToSample = $"MigrationTools::ProcessorEnricherSamples:{OptionFor}"
        };

        /// <summary>
        /// If enabled this will run this migrator
        /// </summary>
        /// <default>true</default>
        public bool Enabled { get; set; }

        /// <summary>
        /// For internal use
        /// </summary>
        public string RefName { get; set; }

        
    }
}