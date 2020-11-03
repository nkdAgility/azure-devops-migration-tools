using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public class WorkItemMigrationProcessor : Processor
    {
        private WorkItemMigrationProcessorOptions _config;
        public override List<IEndpoint> Endpoints { get; }
        public override List<IProcessorEnricher> Enrichers { get; }

        public WorkItemMigrationProcessor(IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemMigrationProcessor> log) : base(services, telemetry, log)
        {
            Endpoints = new List<IEndpoint>();
            Enrichers = new List<IProcessorEnricher>();
        }

        public override void Configure(IProcessorOptions config)
        {
            _config = (WorkItemMigrationProcessorOptions)config;

            ConfigureEndpoints(config);
            ConfigureEnrichers(config);
        }

        protected void ConfigureEnrichers(IProcessorOptions config)
        {
            if (config.Enrichers is null)
            {
                Log.LogWarning("No Enrichers have been Configured");
            }
            else
            {
                foreach (IProcessorEnricherOptions item in config?.Enrichers)
                {
                    var ep = (WorkItemProcessorEnricher)Services.GetRequiredService(item.ToConfigure);
                    ep.Configure(item);
                    Enrichers.Add(ep);
                }
            }
        }

        protected void ConfigureEndpoints(IProcessorOptions config, bool sourceRequired = true, bool targetRequired = false)
        {
            if (config.Endpoints is null)
            {
                Log.LogWarning("No Endpoints have been Configured");
            }
            else
            {
                ValidateDirection(config, EndpointDirection.Target, targetRequired);
                ValidateDirection(config, EndpointDirection.Source, targetRequired);
                foreach (IEndpointOptions item in config?.Endpoints)
                {
                    var ep = (IWorkItemEndPoint)Services.GetRequiredService(item.ToConfigure);
                    ep.Configure(item);
                    Endpoints.Add(ep);
                }
            }
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