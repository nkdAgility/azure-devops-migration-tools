using System;
using System.Collections.Generic;

namespace MigrationTools.Configuration.Processing
{
    public class WorkItemPostProcessingConfig : IProcessorConfig
    {
        public string QueryBit { get; set; }
        public IList<int> WorkItemIDs { get; set; }
        /// <inheritdoc />
        public bool Enabled { get; set; }
        /// <inheritdoc />
        public string Processor
        {
            get { return "WorkItemPostProcessingContext"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }

        public WorkItemPostProcessingConfig()
        {
            QueryBit = "AND [TfsMigrationTool.ReflectedWorkItemId] = '' ";
        }
    }
}