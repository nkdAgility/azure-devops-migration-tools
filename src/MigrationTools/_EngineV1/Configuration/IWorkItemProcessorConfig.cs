using System.Collections.Generic;

namespace MigrationTools._EngineV1.Configuration
{
    public interface IWorkItemProcessorConfig : IProcessorConfig
    {
        public string WIQLQuery { get; set; }


        public IList<int> WorkItemIDs { get; set; }
        public bool FilterWorkItemsThatAlreadyExistInTarget { get; set; }
        public bool PauseAfterEachWorkItem { get; set; }
        public int WorkItemCreateRetryLimit { get; set; }
    }
}