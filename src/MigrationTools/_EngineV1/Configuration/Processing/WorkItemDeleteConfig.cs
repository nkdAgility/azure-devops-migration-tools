using System.Collections.Generic;
using MigrationTools.Enrichers;

namespace MigrationTools._EngineV1.Configuration.Processing
{
    public class WorkItemDeleteConfig : IWorkItemProcessorConfig
    {
        public bool Enabled { get; set; }

        /// <summary>
        /// A list of enrichers that can augment the proccessing of the data
        /// </summary>
        public List<IProcessorEnricher> Enrichers { get; set; }

        public string Processor
        {
            get { return "WorkItemDelete"; }
        }

        public WorkItemDeleteConfig()
        {
            Enabled = false;
            WIQLQuery = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc";
        }

        public string WIQLQuery { get; set; }
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