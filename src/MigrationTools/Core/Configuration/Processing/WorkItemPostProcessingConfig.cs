using System;
using System.Collections.Generic;

namespace MigrationTools.Core.Configuration.Processing
{
    public class WorkItemPostProcessingConfig : ITfsProcessingConfig
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
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }

        public WorkItemPostProcessingConfig()
        {
            QueryBit = "AND [TfsMigrationTool.ReflectedWorkItemId] = '' ";
        }
    }
}