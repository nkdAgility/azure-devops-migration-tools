using System;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;

namespace MigrationTools.Endpoints
{
    public class TfsWorkItemEndPoint : WorkItemEndpoint
    {
        public TfsWorkItemEndPoint(IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemEndpoint> logger) : base(services, telemetry, logger)
        {
        }

        public override void PersistWorkItem(WorkItemData2 source)
        {
            throw new System.NotImplementedException();
        }
    }
}