using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemPostProcessingConfig : ITfsProcessingConfig
    {
        public string QueryBit { get; set; }
        public IList<int> WorkItemIDs { get; set; }
        public bool Enabled { get; set; }

        public Type Processor
        {
            get { return typeof(WorkItemPostProcessingContext); }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}