using System;
using System.Collections.Generic;
using MigrationTools.Enrichers;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Configuration options for the WorkItemTrackingProcessor, which handles migration of work items between endpoints.
    /// </summary>
    public class WorkItemTrackingProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to replay all revisions during migration.
        /// </summary>
        public bool ReplayRevisions { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to collapse revisions into a single work item.
        /// </summary>
        public bool CollapseRevisions { get; set; }
        
        /// <summary>
        /// Gets or sets the number of times to retry work item creation if it fails.
        /// </summary>
        public int WorkItemCreateRetryLimit { get; set; }
        
    }
}
