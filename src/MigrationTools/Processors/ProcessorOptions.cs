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
        public string Processor => ToConfigure.Name;

        /// <summary>
        /// List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
        /// </summary>
        public List<IProcessorEnricherOptions> ProcessorEnrichers { get; set; }

        /// <summary>
        /// This is the `IEndpoint` that will be used as the source of the Migration. Can be null for a write only processor.
        /// </summary>
        public IEndpointOptions Source { get; set; }

        /// <summary>
        /// This is the `IEndpoint` that will be used as the Target of the Migration. Can be null for a write only processor.
        /// </summary>
        public IEndpointOptions Target { get; set; }

        public abstract Type ToConfigure { get; }

        /// <summary>
        /// `Refname` will be used in the future to allow for using named Options without the need to copy all of the options.
        /// </summary>
        public string RefName { get; set; }

        public abstract IProcessorOptions GetDefault();

        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }

        public abstract void SetDefaults();
    }
}