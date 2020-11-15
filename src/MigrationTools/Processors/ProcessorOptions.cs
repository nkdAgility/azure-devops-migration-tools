using System;
using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public abstract class ProcessorOptions : IProcessorOptions
    {
        /// <summary>
        /// If set to `true` then the processor will run. Set to `false` and the processor will not run.
        /// </summary>
        public bool Enabled { get; set; }

        [Obsolete("Avoid using! V1 Architecture")]
        public abstract string Processor { get; }

        public List<IProcessorEnricherOptions> ProcessorEnrichers { get; set; }
        public IEndpointOptions Source { get; set; }
        public IEndpointOptions Target { get; set; }
        public abstract Type ToConfigure { get; }

        public abstract IProcessorOptions GetDefault();

        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }

        public abstract void SetDefaults();
    }
}