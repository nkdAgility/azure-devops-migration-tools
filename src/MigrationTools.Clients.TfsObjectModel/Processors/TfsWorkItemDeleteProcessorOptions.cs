using System.Collections.Generic;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Configuration options for the TFS Work Item Delete Processor that removes work items from the target system based on specified criteria.
    /// </summary>
    public class TfsWorkItemDeleteProcessorOptions : ProcessorOptions, IWorkItemProcessorConfig
    {

        /// <summary>
        /// Initializes a new instance of the TfsWorkItemDeleteProcessorOptions class with default settings.
        /// </summary>
        public TfsWorkItemDeleteProcessorOptions()
        {
            Enabled = false;
            WIQLQuery = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc";
        }

        /// <summary>
        /// Gets or sets the WIQL query used to select work items for deletion. Should return a list of work item IDs.
        /// </summary>
        /// <default>SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc</default>
        public string WIQLQuery { get; set; }
        
        /// <summary>
        /// Gets or sets a specific list of work item IDs to delete. When specified, takes precedence over the WIQL query.
        /// </summary>
        public IList<int> WorkItemIDs { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to filter out work items that already exist in the target before deletion. Typically used to avoid deleting items that shouldn't be removed.
        /// </summary>
        public bool FilterWorkItemsThatAlreadyExistInTarget { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to pause after each work item is deleted for review or debugging purposes.
        /// </summary>
        public bool PauseAfterEachWorkItem { get; set; }
        
        /// <summary>
        /// Gets or sets the number of retry attempts for work item deletion operations when they fail due to transient errors.
        /// </summary>
        public int WorkItemCreateRetryLimit { get; set; }

    }
}