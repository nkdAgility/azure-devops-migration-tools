using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.DistributedTask.Pipelines;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    class TfsPipelineProcessor : Processor
    {
        private TfsPipelineProcessorOptions _Options;

        public TfsPipelineProcessor(ProcessorEnricherContainer processorEnrichers, EndpointContainer endpoints, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(processorEnrichers, endpoints, services, telemetry, logger)
        {
        }

        public TfsEndpoint Source => (TfsEndpoint)Endpoints.Source;

        public TfsEndpoint Target => (TfsEndpoint)Endpoints.Target;

        public override void Configure(IProcessorOptions options)
        {
            base.Configure(options);
            Log.LogInformation("TfsTeamSettingsProcessor::Configure");
            _Options = (TfsPipelineProcessorOptions)options;
        }

        protected override void InternalExecute()
        {
            Log.LogInformation("Processor::InternalExecute::Start");
            EnsureConfigured();
            ProcessorEnrichers.ProcessorExecutionBegin(this);
            MigratePipelines()
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
            if (!(Endpoints.Source is Endpoint))
            {
                throw new Exception("The Source endpoint configured must be of type WorkItemEndpoint");
            }
            if (!(Endpoints.Target is Endpoint))
            {
                throw new Exception("The Target endpoint configured must be of type WorkItemEndpoint");
            }
        }

        private void MigratePipelines()
        {
            if (_Options.MigrateBuildPipelines)
            {
            }

            if (_Options.MigrateReleasePipelines)
            {

            }
        }
    }
}
