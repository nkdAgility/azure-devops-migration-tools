using System;
using Microsoft.Extensions.Logging;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Native TFS Processor, does not work with any other Endpoints.
    /// </summary>
    public class TfsAreaAndIterationProcessor : Processor
    {
        private TfsAreaAndIterationProcessorOptions _Options;

        public TfsAreaAndIterationProcessor(ProcessorEnricherContainer processorEnrichers,
                                        EndpointContainer endpoints,
                                        IServiceProvider services,
                                        ITelemetryLogger telemetry,
                                        ILogger<Processor> logger) : base(processorEnrichers, endpoints, services, telemetry, logger)
        {
        }

        public TfsEndpoint Source => (TfsEndpoint)Endpoints.Source;

        public TfsEndpoint Target => (TfsEndpoint)Endpoints.Target;

        public override void Configure(IProcessorOptions options)
        {
            base.Configure(options);
            Log.LogInformation("TfsAreaAndIterationProcessor::Configure");
            _Options = (TfsAreaAndIterationProcessorOptions)options;
        }

        protected override void InternalExecute()
        {
            Log.LogInformation("Processor::InternalExecute::Start");
            EnsureConfigured();
            ProcessorEnrichers.ProcessorExecutionBegin(this);
            //MigrateTeamSettings();
            ProcessorEnrichers.ProcessorExecutionEnd(this);
            Log.LogInformation("Processor::InternalExecute::End");
        }

        private void EnsureConfigured()
        {
            Log.LogInformation("Processor::EnsureConfigured");
            if (_Options == null)
            {
                throw new Exception("You must call Configure() first");
            }
            if (!(Endpoints.Source is TfsEndpoint))
            {
                throw new Exception("The Source endpoint configured must be of type TfsEndpoint");
            }
            if (!(Endpoints.Target is TfsEndpoint))
            {
                throw new Exception("The Target endpoint configured must be of type TfsEndpoint");
            }
        }
    }
}