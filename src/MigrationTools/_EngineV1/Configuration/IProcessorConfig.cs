using System.Collections.Generic;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools._EngineV1.Configuration
{
    public interface IProcessorConfig : IOptions
    {
        /// <summary>
        /// Indicates, if this processor can be added to the list of current processors or not.
        /// Some processors are not compatible with each other.
        /// </summary>
        /// <param name="otherProcessors">List of already configured processors.</param>
        bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors);
    }
}
