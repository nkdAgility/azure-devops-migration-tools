using System;
using System.Collections.Generic;

namespace MigrationTools.Core.Configuration.Processing
{
    public class WorkItemDeleteConfig : ITfsProcessingConfig
    {
        public bool Enabled { get; set; }

        public string Processor
        {
            get { return "WorkItemDelete"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}