using System;
using System.Collections.Generic;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public class WorkItemTrackingProcessorOptions : ProcessorOptions
    {
        public bool ReplayRevisions { get; set; }
        public bool CollapseRevisions { get; set; }
        public int WorkItemCreateRetryLimit { get; set; }
        
    }
}
