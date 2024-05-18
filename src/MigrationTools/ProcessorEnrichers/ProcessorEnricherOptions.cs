using System;
using System.Text.Json.Serialization;

namespace MigrationTools.Enrichers
{
    public abstract class ProcessorEnricherOptions : IProcessorEnricherOptions
    {
        /// <summary>
        /// If enabled this will run this migrator
        /// </summary>
        /// <default>true</default>
        public bool Enabled { get; set; }

        public abstract Type ToConfigure { get; }

        /// <summary>
        /// For internal use
        /// </summary>
        public string RefName { get; set; }

        public abstract void SetDefaults();
    }
}