using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors
{
    public class TfsExportUsersForMappingProcessorOptions : ProcessorOptions
    {

        public string WIQLQuery { get; set; }

        /// `OnlyListUsersInWorkItems` 
        /// </summary>
        /// <default>true</default>
        public bool OnlyListUsersInWorkItems { get; set; } = true;

    }
}