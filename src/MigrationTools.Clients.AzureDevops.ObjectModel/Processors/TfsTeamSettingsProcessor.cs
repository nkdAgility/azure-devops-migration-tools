using System;
using Microsoft.Extensions.Logging;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public class TfsTeamSettingsProcessor : Processor
    {
        private TfsTeamSettingsProcessorOptions _Options;

        public TfsTeamSettingsProcessor(ProcessorEnricherContainer processorEnricherContainer, EndpointContainer endpointContainer, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(processorEnricherContainer, endpointContainer, services, telemetry, logger)
        {
        }

        public override void Configure(IProcessorOptions config)
        {
            _Options = (TfsTeamSettingsProcessorOptions)config;
        }

        protected override void InternalExecute()
        {
            if (_Options == null)
            {
                throw new Exception("You must call Configure() first");
            }
        }
    }
}