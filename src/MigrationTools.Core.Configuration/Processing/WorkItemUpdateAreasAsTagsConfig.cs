using System;
using System.Collections.Generic;

namespace MigrationTools.Core.Configuration.Processing
{
    public class WorkItemUpdateAreasAsTagsConfig : ITfsProcessingConfig
    {
        public string AreaIterationPath { get; set; }
        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public string Processor
        {
            get { return "WorkItemUpdateAreasAsTagsContext"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}