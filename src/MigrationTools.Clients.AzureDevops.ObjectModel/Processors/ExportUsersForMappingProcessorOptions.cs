using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public class ExportUsersForMappingProcessorOptions : IProcessorConfig
    {
        public bool Enabled { get; set; }

        /// <summary>
        /// A list of enrichers that can augment the proccessing of the data
        /// </summary>
        public List<IProcessorEnricher> Enrichers { get; set; }

        public string WIQLQuery { get; set; }
        

        /// `OnlyListUsersInWorkItems` 
        /// </summary>
        /// <default>true</default>
        public bool OnlyListUsersInWorkItems { get; set; } = true;

        public string Processor
        {
            get { return "ExportUsersForMappingProcessor"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}