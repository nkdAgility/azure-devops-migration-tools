using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Options;

namespace MigrationTools.Processors.Infrastructure
{
    public abstract class ProcessorOptions : IProcessorOptions
    {
        public string ConfigurationSectionPath => $"MigrationTools:ProcessorDefaults:{ConfigurationOptionFor}";
        public string ConfigurationCollectionPath => $"MigrationTools:Processors";
        public string ConfigurationObjectName => $"ProcessorType";
        public string ConfigurationOptionFor => $"{GetType().Name.Replace("Options", "")}";
        public string ConfigurationSamplePath => $"MigrationTools:ProcessorSamples:{ConfigurationOptionFor}";

        /// <summary>
        /// If set to `true` then the processor will run. Set to `false` and the processor will not run.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// A list of enrichers that can augment the proccessing of the data
        /// </summary>
        public List<IProcessorEnricher> Enrichers { get; set; }

        /// <summary>
        /// List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
        /// </summary>
        public List<IProcessorEnricherOptions> ProcessorEnrichers { get; set; }

        public string SourceName { get; set; }
        public string TargetName { get; set; }

        /// <summary>
        /// `Refname` will be used in the future to allow for using named Options without the need to copy all of the options.
        /// </summary>
        public string RefName { get; set; }

       

        public IProcessorOptions GetSample()
        {
            throw new NotImplementedException();
            return null; 
        }

        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}