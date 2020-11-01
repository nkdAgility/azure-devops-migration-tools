using System;
using Microsoft.Extensions.Logging;

namespace MigrationTools.Processors
{
    public class WorkItemMigrationProcessor : Processor
    {
        private WorkItemMigrationProcessorOptions _config;

        public WorkItemMigrationProcessor(IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemMigrationProcessor> log) : base(services, telemetry, log)
        {
        }

        public override void Configure(IProcessorOptions config)
        {
            _config = (WorkItemMigrationProcessorOptions)config;
        }

        protected override void InternalExecute()
        {
            Log.LogInformation("Starting ");
            if (_config == null)
            {
                throw new Exception("You must call Configure() first");
            }
            Log.LogInformation("Finishing ");
        }
    }
}