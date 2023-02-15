using System.Collections.Generic;

namespace MigrationTools._EngineV1.Configuration.Processing
{
    public class WorkItemUpdateAreasAsTagsConfig : IProcessorConfig
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
            get { return "WorkItemUpdateAreasAsTagsContext"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}