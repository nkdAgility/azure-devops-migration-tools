﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Processors.Infrastructure
{
    public abstract class ProcessorOptions : IProcessorOptions
    {
        [JsonIgnore]
        public string OptionFor => $"{GetType().Name.Replace("Options", "")}";

        [JsonIgnore]
        public ConfigurationMetadata ConfigurationMetadata => new ConfigurationMetadata
        {
            IsCollection = true,
            PathToInstance = $"MigrationTools:Processors",
            ObjectName = $"ProcessorType",
            OptionFor = OptionFor,
            PathToDefault = $"MigrationTools:ProcessorDefaults:{OptionFor}",
            PathToSample = $"MigrationTools:ProcessorSamples:{OptionFor}"
        };

        /// <summary>
        /// If set to `true` then the processor will run. Set to `false` and the processor will not run.
        /// </summary>
        [Required]
        public bool Enabled { get; set; }

        /// <summary>
        /// List of Enrichers that can be used to add more features to this processor. Only works with Native Processors and not legacy Processors.
        /// </summary>
        public List<IProcessorEnricherOptions> Enrichers { get; set; }

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