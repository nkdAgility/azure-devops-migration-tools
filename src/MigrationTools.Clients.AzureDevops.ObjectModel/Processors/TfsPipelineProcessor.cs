using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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

        List<Pipeline> sourcePipelines = new List<Pipeline>();
        List<Pipeline> targetPipelines = new List<Pipeline>();


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
            Stopwatch stopwatch = Stopwatch.StartNew();
            GetPipelines(Source.Organisation, Source.Project, Source.AccessToken);

            if (_Options.MigrateBuildPipelines)
            {
                CreatePipelines();
            }

            if (_Options.MigrateReleasePipelines)
            {

            }
            stopwatch.Stop();
            Log.LogDebug("DONE in {Elapsed} ", stopwatch.Elapsed.ToString("c"));
        }

        private void GetPipelines(string organisation, string project, string accessToken)
        {
            string baseUrl = organisation + "/" + project + "/_apis/build/definitions";
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + accessToken));

            WebClient client = new WebClient();
            client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
            string httpResponse = client.DownloadString(baseUrl);

            if (httpResponse != null)
            {
                Pipelines pipelines = JsonConvert.DeserializeObject<Pipelines>(httpResponse);

                foreach (Pipeline pipeline in pipelines.Value)
                {
                    //Nessecary because getting all Pipelines doesn't include all of their properties 
                    string responseMessage = client.DownloadString(baseUrl + "/" + pipeline.Id);

                    Pipeline newPipeline = JsonConvert.DeserializeObject<Pipeline>(responseMessage);

                    if (organisation == Source.Organisation)
                    {
                        sourcePipelines.Add(newPipeline);
                    }
                    else
                    {
                        targetPipelines.Add(newPipeline);
                    }
                }
            }
        }

        private void CreatePipelines()
        {
            List<Pipeline> pipelinesToBeMigrated = new List<Pipeline>();
            GetPipelines(Target.Organisation, Target.Project, Target.AccessToken);

            //Filter out Pipelines that already exsit
            foreach (Pipeline sourcePipeline in sourcePipelines)
            {
                int exsits = 0;
                foreach (Pipeline targetPipeline in targetPipelines)
                {
                    if (targetPipeline.Name == sourcePipeline.Name)
                    {
                        exsits++;
                    }
                }
                if (exsits == 0)
                {
                    pipelinesToBeMigrated.Add(sourcePipeline);
                }
            }
            Log.LogInformation("From {sourcePipelines} source Pipelines {pipelinesToBeMigrated} Pipelines are going to be migrated..", sourcePipelines.Count, pipelinesToBeMigrated.Count);
            string baseUrl = Target.Organisation + "/" + Target.Project + "/_apis/build/definitions";
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + Target.AccessToken));

            WebClient client = new WebClient();
            client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;

            foreach (Pipeline pipelineToBeMigrated in pipelinesToBeMigrated)
            {
                pipelineToBeMigrated.Links = null;
                pipelineToBeMigrated.AuthoredBy = null;
                pipelineToBeMigrated.Queue = null;
                pipelineToBeMigrated.Url = null;
                pipelineToBeMigrated.Uri = null;
                pipelineToBeMigrated.Revision = 0;
                pipelineToBeMigrated.Id = 0;
                pipelineToBeMigrated.Project = null;

                Log.LogInformation("Processing Pipeline '{pipelineToBeMigrated}'..", pipelineToBeMigrated.Name);
                string body = JsonConvert.SerializeObject(pipelineToBeMigrated);
                client.UploadString(baseUrl, body);

            }
        }
    }
}
