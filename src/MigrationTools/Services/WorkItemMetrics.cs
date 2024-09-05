using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace MigrationTools.Services
{
   public class WorkItemMetrics
    {
        public static readonly string meterName = "MigrationTools.WorkItems";

        private readonly Meter WorkItemMeter;
        public Counter<long> WorkItemsProcessedCount { get; private set ; }
        public Counter<long> RevisionsProcessedCount { get; private set; }
        public Histogram<double> ProcessingDuration { get; private set; }

        public Histogram<double> RevisionsPerWorkItem { get; private set; }

        public WorkItemMetrics(IMeterFactory meterFactory, IConfiguration configuration)
        {
            WorkItemMeter = meterFactory.Create(meterName);
            WorkItemsProcessedCount = WorkItemMeter.CreateCounter<long>("work_items_processed_total");
            ProcessingDuration = WorkItemMeter.CreateHistogram<double>("work_item_processing_duration");
            RevisionsPerWorkItem = WorkItemMeter.CreateHistogram<double>("work_item_revisions");
            RevisionsProcessedCount = WorkItemMeter.CreateCounter<long>("work_item_revisions_total");
        }
    }
}
