using System.Collections.Generic;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools._EngineV1.Configuration;

namespace MigrationTools.Processors
{
    public class WorkItemUpdateAreasAsTagsProcessorOptions : IProcessorConfig
    {
        /// <summary>
        /// This is a required parameter. That define the root path of the iteration. To get the full path use `\` 
        /// </summary>
        /// <default>\</default>
        public string AreaIterationPath { get; set; }

        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public string Processor
        {
            get { return typeof(WorkItemUpdateAreasAsTagsProcessor).Name; }
        }

        public List<IProcessorEnricher> Enrichers { get ; set ; }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}