using System.Collections.Generic;
using Newtonsoft.Json;

namespace MigrationTools._EngineV1.Configuration
{
    public interface IProcessorConfig
    {
        /// <summary>
        /// Active the processor if it true.
        /// </summary>
        bool Enabled { get; set; }

        [JsonIgnore]
        string Processor { get; }

        /// <summary>
        /// Indicates, if this processor can be added to the list of current processors or not.
        /// Some processors are not compatible with each other.
        /// </summary>
        /// <param name="otherProcessors">List of already configured processors.</param>
        bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors);
    }
}