using System.Collections.Generic;
using MigrationTools.Enrichers;

namespace MigrationTools._EngineV1.Configuration.Processing
{
    public class FakeProcessorConfig : IProcessorConfig
    {
        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <summary>
        /// A list of enrichers that can augment the proccessing of the data
        /// </summary>
        public List<IProcessorEnricher> Enrichers { get; set; }

        /// <inheritdoc />
        public string Processor
        {
            get { return "FakeProcessor"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}