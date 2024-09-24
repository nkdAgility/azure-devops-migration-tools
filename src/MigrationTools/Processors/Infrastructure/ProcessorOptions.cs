using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Processors.Infrastructure
{
    public abstract class ProcessorOptions : IProcessorOptions, IValidateOptions<ProcessorOptions>
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
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<IProcessorEnricherOptions> Enrichers { get; set; }

        [Required]
        public string SourceName { get; set; }
        [Required]
        public string TargetName { get; set; }

        /// <summary>
        /// `Refname` will be used in the future to allow for using named Options without the need to copy all of the options.
        /// </summary>
        [JsonIgnore]
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

        public ValidateOptionsResult Validate(string name, ProcessorOptions options)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(options.SourceName))
            {
                errors.Add("The `SourceName` field on {name} must contain a value that matches one of the Endpoint names.");
            }
            if (string.IsNullOrWhiteSpace(options.TargetName))
            {
                errors.Add("The `TargetName` field on {name} must contain a value that matches one of the Endpoint names.");
            }

            if (errors.Count > 0)
            {
                return ValidateOptionsResult.Fail(errors);
            }

            return ValidateOptionsResult.Success;
        }
    }
}