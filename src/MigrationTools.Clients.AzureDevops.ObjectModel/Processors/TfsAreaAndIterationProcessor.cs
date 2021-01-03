using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    /// <summary>
    /// The `TfsAreaAndIterationProcessor` migrates all of the Area nd Iteraion paths.
    /// </summary>
    public class TfsAreaAndIterationProcessor : Processor
    {
        private TfsAreaAndIterationProcessorOptions _Options;
        private TfsNodeStructure nodeStructureEnricher;

        public TfsAreaAndIterationProcessor(
                            ProcessorEnricherContainer processorEnrichers,
                            IEndpointFactory endpointFactory,
                            IServiceProvider services,
                            ITelemetryLogger telemetry,
                            ILogger<Processor> logger)
            : base(processorEnrichers, endpointFactory, services, telemetry, logger)
        {
        }

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
            nodeStructureEnricher = Services.GetRequiredService<TfsNodeStructure>();
            nodeStructureEnricher.Configure(new TfsNodeStructureOptions() { Enabled = true, NodeBasePaths = _Options.NodeBasePaths, PrefixProjectToNodes = _Options.PrefixProjectToNodes });
            nodeStructureEnricher.ProcessorExecutionBegin(null);
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
            if (Source is not TfsEndpoint)
            {
                throw new Exception("The Source endpoint configured must be of type TfsEndpoint");
            }
            if (Target is not TfsEndpoint)
            {
                throw new Exception("The Target endpoint configured must be of type TfsEndpoint");
            }
        }
    }
}