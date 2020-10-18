using System.Collections.Generic;
using System.Collections.ObjectModel;
using MigrationTools.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using Newtonsoft.Json;

namespace MigrationTools.Processors
{
    public class ProcessorOptions : IProcessorOptions
    {
        /// <summary>
        /// Active the processor if it true.
        /// </summary>
        public bool Enabled { get; set; }

        public Collection<IEndpointOptions> Endpoints { get; }
        public Collection<ProcessorEnricherOptions> Enrichers { get; }

        [JsonIgnoreAttribute]
        public string Processor { get; }

        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}