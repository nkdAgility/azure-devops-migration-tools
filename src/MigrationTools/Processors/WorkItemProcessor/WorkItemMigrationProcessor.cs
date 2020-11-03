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
            Endpoints.ConfigureEndpoints(config.Endpoints);
            Enrichers.ConfigureEnrichers(config.Enrichers);
        }

        protected override void InternalExecute()
        {
            Log.LogInformation("Processor Starting");
            EnsureConfigured();
            BeginProcessorExecution();
            AfterProcessorLoadSource();
            // var source = Endpoints.Where(e => e.Direction == ).SingleOrDefault()
            // FOR EACH DATA ITEM

            EndProcessorExecution();
            Log.LogInformation("Finishing ");
        }

        private void AfterProcessorLoadSource()
        {
            Log.LogInformation("Processor AfterProcessorLoadSource");
        }

        private void EnsureConfigured()
        {
            Log.LogInformation("Processor EnsureConfigured");
            if (_config == null)
            {
                throw new Exception("You must call Configure() first");
            }
        }

        private void BeginProcessorExecution()
        {
            Log.LogInformation("Processor BeginProcessorExecution");

            foreach (var item in Enrichers)
            {
                item.BeginProcessorExecution();
            }
        }

        private void EndProcessorExecution()
        {
            Log.LogInformation("Processor EndProcessorExecution");
            foreach (var item in Enrichers)
            {
                item.EndProcessorExecution();
            }
        }
    }
}