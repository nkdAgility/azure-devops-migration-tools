using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.DataContracts;
using MigrationTools.Core.Sinks;
using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Sinks.AzureDevOps
{
    public class AzureDevOpsWorkItemSink : IWorkItemSink
    {
        public AzureDevOpsWorkItemSink(IHost host, ILogger<MigrationEngine> log, TelemetryClient telemetry, EngineConfiguration config)
        {

        }

        public IEnumerable<WorkItemData> GetWorkItems()
        {
            throw new NotImplementedException();
        }

        public WorkItemData PersistWorkItem(WorkItemData workItem)
        {
            throw new NotImplementedException();
        }
    }
}
