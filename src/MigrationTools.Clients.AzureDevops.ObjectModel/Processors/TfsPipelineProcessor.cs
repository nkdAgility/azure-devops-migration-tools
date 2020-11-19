using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.DistributedTask.Pipelines;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Enrichers.Pipelines;
using Newtonsoft.Json;

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
            MigratePipelines();
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
                GetData();

                async void GetData()
                {
                    string baseUrl = Source.Organisation + "/" + Source.Project + "/_apis/pipelines";

                    HttpClient client = new HttpClient();
                    var byteArray = Encoding.ASCII.GetBytes(":" + Source.AccessToken);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    HttpResponseMessage res = await client.GetAsync(baseUrl);
                    HttpContent content = res.Content;
                    string data = await content.ReadAsStringAsync();
                    if (data != null)
                    {
                        Pipelines pipelines = JsonConvert.DeserializeObject<Pipelines>(data);
                    }

                }

                if (_Options.MigrateReleasePipelines)
                {

                }
            }
        }
    }
}
