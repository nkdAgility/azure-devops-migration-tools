using System;
using System.Collections.Generic;

namespace MigrationTools.Core.Configuration.Processing
{
    public class WorkItemUpdateConfig : ITfsProcessingConfig
    {
        public bool WhatIf { get; set; }

        public string QueryBit { get; set; }

        public bool Enabled { get; set; }

        public string Processor
        {
            get { return "WorkItemUpdate"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }

        public WorkItemUpdateConfig()
        {
            QueryBit = @"AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')";
        }
    }
}