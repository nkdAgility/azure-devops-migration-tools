using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemUpdateAreasAsTagsConfig : ITfsProcessingConfig
    {
        public string AreaIterationPath { get; set; }
        public bool Enabled { get; set; }

        public Type Processor
        {
            get { return typeof(WorkItemUpdateAreasAsTagsContext); }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}