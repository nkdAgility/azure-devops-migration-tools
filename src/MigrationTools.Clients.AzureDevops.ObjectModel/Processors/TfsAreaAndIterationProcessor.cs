using System;
using Microsoft.Extensions.Logging;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    /// <summary>
    /// The `TfsAreaAndIterationProcessor` migrates all of the Area nd Iteraion paths.
    /// </summary>
    /// <status>Beta</status>
    /// <processingtarget>Work Items</processingtarget>
    public class TfsAreaAndIterationProcessor : Processor
    {
        private TfsAreaAndIterationProcessorOptions _options;
        private TfsNodeStructure _nodeStructureEnricher;

        public TfsAreaAndIterationProcessor(
                            TfsNodeStructure tfsNodeStructure,
                            ProcessorEnricherContainer processorEnrichers,
                            IEndpointFactory endpointFactory,
                            IServiceProvider services,
                            ITelemetryLogger telemetry,
                            ILogger<Processor> logger)
            : base(processorEnrichers, endpointFactory, services, telemetry, logger)
        {
            _nodeStructureEnricher = tfsNodeStructure;
        }

        public override void Configure(IProcessorOptions options)
        {
            base.Configure(options);
            Log.LogInformation("TfsAreaAndIterationProcessor::Configure");
            _options = (TfsAreaAndIterationProcessorOptions)options;
        }

        protected override void InternalExecute()
        {
            Log.LogInformation("Processor::InternalExecute::Start");
            EnsureConfigured();
            ProcessorEnrichers.ProcessorExecutionBegin(this);
            var nodeStructurOptions = new TfsNodeStructureOptions()
                                        {
                                            Enabled = true,
                                            NodeBasePaths = _options.NodeBasePaths,
                                            AreaMaps = _options.AreaMaps,
                                            IterationMaps = _options.IterationMaps,
                                        };
            _nodeStructureEnricher.Configure(nodeStructurOptions);
            _nodeStructureEnricher.ProcessorExecutionBegin(null);
            ProcessorEnrichers.ProcessorExecutionEnd(this);
            Log.LogInformation("Processor::InternalExecute::End");
        }

        private void EnsureConfigured()
        {
            Log.LogInformation("Processor::EnsureConfigured");
            if (_options == null)
            {
                throw new Exception("You must call Configure() first");
            }
            if (Source is not TfsWorkItemEndpoint)
            {
                throw new Exception("The Source endpoint configured must be of type TfsWorkItemEndpoint");
            }
            if (Target is not TfsWorkItemEndpoint)
            {
                throw new Exception("The Target endpoint configured must be of type TfsWorkItemEndpoint");
            }
        }
    }
}