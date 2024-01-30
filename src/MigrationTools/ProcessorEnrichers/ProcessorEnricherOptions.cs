using System;

namespace MigrationTools.Enrichers
{
    public abstract class ProcessorEnricherOptions : IProcessorEnricherOptions
    {
        /// <summary>
        /// For internal use
        /// </summary>
        public bool Enabled { get; set; }

        public abstract Type ToConfigure { get; }

        /// <summary>
        /// For internal use
        /// </summary>
        public string RefName { get; set; }

        public abstract void SetDefaults();
    }
}