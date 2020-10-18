using System.Collections.Generic;
using MigrationTools.Configuration;
using Newtonsoft.Json;

namespace MigrationTools.Processors
{
    public class ProcessorOptions : IProcessorOptions
    {
        /// <summary>
        /// Active the processor if it true.
        /// </summary>
        public bool Enabled { get; set; }

        [JsonIgnoreAttribute]
        public string Processor { get; }

        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}