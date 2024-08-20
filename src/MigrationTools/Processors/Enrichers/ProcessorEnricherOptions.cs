using System;
using System.Text.Json.Serialization;

namespace MigrationTools.Enrichers
{
    public abstract class ProcessorEnricherOptions : IProcessorEnricherOptions
    {
        public virtual string ConfigurationSectionPath => $"MigrationTools:ProcessorEnricherDefaults:{ConfigurationOptionFor}";
        public virtual string ConfigurationCollectionPath => $"MigrationTools:Processors:*:Enrichers";
        public virtual string ConfigurationObjectName => $"ProcessorEnricherType";
        public virtual string ConfigurationOptionFor => $"{GetType().Name.Replace("Options", "")}";

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