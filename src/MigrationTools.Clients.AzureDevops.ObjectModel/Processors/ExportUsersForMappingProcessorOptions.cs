using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public class ExportUsersForMappingProcessorOptions : ProcessorOptions
    {

        public string WIQLQuery { get; set; }

        /// `OnlyListUsersInWorkItems` 
        /// </summary>
        /// <default>true</default>
        public bool OnlyListUsersInWorkItems { get; set; } = true;

    }
}