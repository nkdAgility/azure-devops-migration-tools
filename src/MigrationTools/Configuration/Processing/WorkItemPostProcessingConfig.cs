using System.Collections.Generic;

namespace MigrationTools.Configuration.Processing
{
    public class WorkItemPostProcessingConfig : IWorkItemProcessorConfig
    {
        public IList<int> WorkItemIDs { get; set; }
        /// <inheritdoc />
        public bool Enabled { get; set; }
        /// <inheritdoc />
        public string Processor
        {
            get { return "WorkItemPostProcessingContext"; }
        }

        public string WIQLQueryBit { get; set; }
        public string WIQLOrderBit { get; set; }
        public bool FilterWorkItemsThatAlreadyExistInTarget { get; set; }
        public bool PauseAfterEachWorkItem { get; set; }
        public int WorkItemCreateRetryLimit { get; set; }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }

        public WorkItemPostProcessingConfig()
        {
            WIQLQueryBit = "AND [TfsMigrationTool.ReflectedWorkItemId] = '' ";
        }
    }
}