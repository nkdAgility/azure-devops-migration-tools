using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Options;

namespace MigrationTools.Processors.Infrastructure
{
    public interface IProcessorOptions : IProcessorConfig
    {
        /// <summary>
        /// This is the `IEndpoint` that will be used as the source of the Migration. Can be null for a write only processor.
        /// </summary>
        [Required]
        public string SourceName { get; }

        /// <summary>
        /// This is the `IEndpoint` that will be used as the Target of the Migration. Can be null for a read only processor.
        /// </summary>
        [Required]
        public string TargetName { get; }

        /// <summary>
        /// List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
        /// </summary>
        List<IProcessorEnricherOptions> Enrichers { get; set; }

        IProcessorOptions GetSample();
    }
}