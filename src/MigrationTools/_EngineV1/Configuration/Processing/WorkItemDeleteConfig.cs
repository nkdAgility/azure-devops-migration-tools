using System.Collections.Generic;

namespace MigrationTools.Configuration.Processing
{
    public class WorkItemDeleteConfig : IWorkItemProcessorConfig
    {
        public bool Enabled { get; set; }

        public string Processor
        {
            get { return "WorkItemDelete"; }
        }

        public WorkItemDeleteConfig()
        {
            Enabled = false;
            WIQLQueryBit = @"AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            WIQLOrderBit = "[System.ChangedDate] desc";
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
    }
}