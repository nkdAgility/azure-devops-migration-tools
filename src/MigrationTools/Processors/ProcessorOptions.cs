using System.Collections.Generic;
using MigrationTools.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using Newtonsoft.Json;

namespace MigrationTools.Processors
{
    public abstract class ProcessorOptions : IProcessorOptions
    {
        /// <summary>
        /// Active the processor if it true.
        /// </summary>
        public bool Enabled { get; set; }

        public List<IEndpointOptions> Endpoints { get; }
        public List<ProcessorEnricherOptions> Enrichers { get; }

        [JsonIgnoreAttribute]
        public abstract string Processor { get; }

        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}