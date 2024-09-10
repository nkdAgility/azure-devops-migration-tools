using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;

namespace MigrationTools.Services
{
    public class ProcessorMetrics
    {
        public static readonly string meterName = "MigrationTools.Processors";

        private readonly Meter WorkItemMeter;

        public Histogram<double> Duration { get; private set; }
        public object QueueSize { get; private set; }

        public ProcessorMetrics(IMeterFactory meterFactory)
        {
            WorkItemMeter = meterFactory.Create(meterName);
            Duration = WorkItemMeter.CreateHistogram<double>("processor_duration");
            QueueSize = WorkItemMeter.CreateUpDownCounter<long>("processor_queue_size");
        }
    }
}
