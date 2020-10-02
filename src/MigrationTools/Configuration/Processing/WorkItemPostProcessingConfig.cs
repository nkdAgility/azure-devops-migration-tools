using System;
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

        public string WIQLQueryBit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string WIQLOrderBit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool FilterWorkItemsThatAlreadyExistInTarget { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool PauseAfterEachWorkItem { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int WorkItemCreateRetryLimit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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