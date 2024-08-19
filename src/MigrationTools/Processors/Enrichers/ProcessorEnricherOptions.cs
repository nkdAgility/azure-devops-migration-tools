﻿using System;
using System.Text.Json.Serialization;

namespace MigrationTools.Enrichers
{
    public abstract class ProcessorEnricherOptions : IProcessorEnricherOptions
    {
        [JsonIgnore]
        public virtual string ConfigurationSectionName => $"MigrationTools:ProcessorEnricherDefaults:{OptionFor}";
        [JsonIgnore]
        public virtual string OptionFor => $"{GetType().Name.Replace("Options", "")}";

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