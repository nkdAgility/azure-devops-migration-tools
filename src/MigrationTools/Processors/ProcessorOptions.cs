using System;
using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Processors
{
    public abstract class ProcessorOptions : IProcessorOptions
    {
        /// <summary>
        /// Active the processor if it true.
        /// </summary>
        public bool Enabled { get; set; }

        [JsonConverter(typeof(OptionsJsonConvertor))]
        public List<IEndpointOptions> Endpoints { get; set; }

        public List<IProcessorEnricherOptions> ProcessorEnrichers { get; set; }

        [Obsolete("Avoid using! V1 Architecture")]
        public abstract string Processor { get; }

        public abstract Type ToConfigure { get; }

        public abstract IProcessorOptions GetDefault();

        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }

        public abstract void SetDefaults();
    }
}