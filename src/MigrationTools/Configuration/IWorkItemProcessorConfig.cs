using System.Collections.Generic;

namespace MigrationTools.Configuration
{
    interface IWorkItemProcessorConfig : IProcessorConfig
    {
        public string WIQLQueryBit { get; set; }
        /// <inheritdoc />
        public string WIQLOrderBit { get; set; }
        public IList<int> WorkItemIDs { get; set; }
        public bool FilterWorkItemsThatAlreadyExistInTarget { get; set; }
        public bool PauseAfterEachWorkItem { get; set; }
        public int WorkItemCreateRetryLimit { get; set; }
    }
}
