using System.Collections.Generic;
using MigrationTools.Enrichers;

namespace MigrationTools._EngineV1.Configuration.Processing
{
    public class ImportProfilePictureConfig : IProcessorConfig
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
            get { return "ImportProfilePictureContext"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}