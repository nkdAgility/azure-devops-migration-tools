using System.Collections.Generic;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Processors.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace MigrationTools.Processors
{
    public class WorkItemPostProcessingProcessorOptions : ProcessorOptions, IWorkItemProcessorConfig
    {

        /// <summary>
        /// A list of work items to import
        /// </summary>
        /// <default>[]</default>
        public IList<int> WorkItemIDs { get; set; }


        /// <summary>
        /// A work item query based on WIQL to select only important work items. To migrate all leave this empty. See [WIQL Query Bits](#wiql-query-bits)
        /// </summary>
        /// <default>AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request')</default>
        [Required]
        public string WIQLQuery { get; set; }


        /// <summary>
        /// This loads all of the work items already saved to the Target and removes them from the Source work item list prior to commencing the run.
        /// While this may take some time in large data sets it reduces the time of the overall migration significantly if you need to restart.
        /// </summary>
        /// <default>true</default>
        public bool FilterWorkItemsThatAlreadyExistInTarget { get; set; }

        /// <summary>
        /// Pause after each work item is migrated
        /// </summary>
        /// <default>false</default>
        public bool PauseAfterEachWorkItem { get; set; }

        /// <summary>
        /// **beta** If set to a number greater than 0 work items that fail to save will retry after a number of seconds equal to the retry count.
        /// This allows for periodic network glitches not to end the process.
        /// </summary>
        /// <default>5</default>
        public int WorkItemCreateRetryLimit { get; set; }


        public WorkItemPostProcessingProcessorOptions()
        {
            WIQLQuery = @"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject AND [@ReflectedWorkItemIdFieldName] = ''  AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan','Shared Steps','Shared Parameter','Feedback Request') ORDER BY [System.ChangedDate] desc";
        }
    }
}