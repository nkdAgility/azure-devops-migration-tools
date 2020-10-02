using System;
using System.Collections.Generic;

namespace MigrationTools.Configuration.Processing
{
    public class WorkItemUpdateConfig : IWorkItemProcessorConfig
    {
        public bool WhatIf { get; set; }

        public bool Enabled { get; set; }

        public string Processor
        {
            get { return "WorkItemUpdate"; }
        }

        public string WIQLQueryBit { get; set; }
        public string WIQLOrderBit { get; set; }
        public IList<int> WorkItemIDs { get; set; }
        public bool FilterWorkItemsThatAlreadyExistInTarget { get; set; }
        public bool PauseAfterEachWorkItem { get; set; }
        public int WorkItemCreateRetryLimit { get; set; }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }

        public WorkItemUpdateConfig()
        {
            WIQLQueryBit = @"AND [TfsMigrationTool.ReflectedWorkItemId] = '' AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] IN ('Shared Steps', 'Shared Parameter', 'Test Case', 'Requirement', 'Task', 'User Story', 'Bug')";
        }
    }
}