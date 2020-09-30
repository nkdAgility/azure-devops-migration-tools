using System;
using System.Collections.Generic;

namespace MigrationTools.Configuration.Processing
{
    public class WorkItemDeleteConfig : IProcessorConfig
    {
        public bool Enabled { get; set; }

        public string Processor
        {
            get { return "WorkItemDelete"; }
        }

        public WorkItemDeleteConfig()
        {
            Enabled = false;
            QueryBit = @"AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            OrderBit = "[System.ChangedDate] desc";
        }

        public string QueryBit { get; set; }
        /// <inheritdoc />
        public string OrderBit { get; set; }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}