using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTools.Core.Configuration.Processing
{
    public interface ITfsProcessingConfig
    {
        /// <summary>
        /// Active the processor if it true. 
        /// </summary>
        bool Enabled { get; set; }
        [JsonIgnoreAttribute]
        string Processor { get; }
        
        /// <summary>
        /// Indicates, if this processor can be added to the list of current processors or not.
        /// Some processors are not compatible with each other.
        /// </summary>
        /// <param name="otherProcessors">List of already configured processors.</param>
        bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors);
    }
}
