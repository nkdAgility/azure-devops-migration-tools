using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class WorkItemPostProcessingConfig : ITfsProcessingConfig
    {
        public string QueryBit { get; set; }
        public IList<int> WorkItemIDs { get; set; }
        /// <inheritdoc />
        public bool Enabled { get; set; }
        /// <inheritdoc />
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