using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts.Pipelines;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using Newtonsoft.Json;

namespace MigrationTools.Processors
{
    internal partial class AzureDevOpsPipelineProcessor : Processor
    {
        private AzureDevOpsPipelineProcessorOptions _Options;

        public AzureDevOpsPipelineProcessor(ProcessorEnricherContainer processorEnrichers, EndpointContainer endpoints, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(processorEnrichers, endpoints, services, telemetry, logger)
        {
        }

        public AzureDevOpsEndpoint Source => (AzureDevOpsEndpoint)Endpoints.Source;

        public AzureDevOpsEndpoint Target => (AzureDevOpsEndpoint)Endpoints.Target;

        public override void Configure(IProcessorOptions options)
        {
            base.Configure(options);
            Log.LogInformation("TfsTeamSettingsProcessor::Configure");
            _Options = (AzureDevOpsPipelineProcessorOptions)options;
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

            if (_Options.MigrateTaskGroups)
            {
                createTaskGroups();
            }
            if (_Options.MigrateBuildPipelines)
            {
                createPipelineDefinitions(PipelineType.build);
            }

            if (_Options.MigrateReleasePipelines)
            {
                createPipelineDefinitions(PipelineType.release);
            }
            stopwatch.Stop();
            Log.LogDebug("DONE in {Elapsed} ", stopwatch.Elapsed.ToString("c"));
        }

        private HttpClient getHttpClient(string accessToken)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", accessToken))));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            return client;
        }

        private IList<TaskGroup> getTaskGroups(string organisation, string project, string accessToken)
        {
            string baseUrl = organisation + "/" + project + "/_apis/distributedtask/taskgroups";
            var taskGroups = new List<TaskGroup>();

            HttpClient client = getHttpClient(accessToken);
            string httpResponse = client.GetStringAsync(baseUrl).Result;

            if (httpResponse != null)
            {
                TaskGroups receivedTaskGroups = JsonConvert.DeserializeObject<TaskGroups>(httpResponse);

                foreach (TaskGroup taskGroup in receivedTaskGroups.Value)
                {
                    taskGroups.Add(taskGroup);
                }
            }
            return taskGroups;
        }

        private IList<ReleaseBuildDefinitionAbstract> getPipelineDefinitions(string organisation, string project, string accessToken, PipelineType pipelineType)
        {
            string baseUrl = organisation + "/" + project + "/_apis/" + pipelineType.ToString() + "/definitions";
            var pipelineDefinitions = new List<ReleaseBuildDefinitionAbstract>();

            HttpClient client = getHttpClient(accessToken);

            string httpResponse = client.GetStringAsync(baseUrl).Result;

            if (httpResponse != null)
            {
                if (pipelineType == PipelineType.build)
                {
                    BuildDefinitions pipelines = JsonConvert.DeserializeObject<BuildDefinitions>(httpResponse);

                    foreach (BuildDefinition pipeline in pipelines.Value)
                    {
                        //Nessecary because getting all Pipelines doesn't include all of their properties
                        string responseMessage = client.GetStringAsync(baseUrl + "/" + pipeline.Id).Result;
                        pipelineDefinitions.Add(JsonConvert.DeserializeObject<BuildDefinition>(responseMessage));
                    }
                }
                else
                {
                    ReleaseDefinitions pipelines = JsonConvert.DeserializeObject<ReleaseDefinitions>(httpResponse);

                    foreach (ReleaseDefinition pipeline in pipelines.Value)
                    {
                        //Nessecary because getting all Pipelines doesn't include all of their properties
                        string responseMessage = client.GetStringAsync(baseUrl + "/" + pipeline.Id).Result;
                        pipelineDefinitions.Add(JsonConvert.DeserializeObject<ReleaseDefinition>(responseMessage));
                    }
                }
            }
            return pipelineDefinitions;
        }

        private void createTaskGroups()
        {
            Log.LogInformation("Fetching TaskGroups...");
            var sourceTaskGroups = getTaskGroups(Source.Organisation, Source.Project, Source.AccessToken);
            var targetTaskGroups = getTaskGroups(Target.Organisation, Target.Project, Target.AccessToken);

            //Filter out Pipelines that already exsit
            var taskGroupsToBeMigrated = sourceTaskGroups.Where(s => !targetTaskGroups.Any(t => t.Name == s.Name));

            Log.LogInformation("From {sourceTaskGroupss} source Task Groups {taskGroupsToBeMigrated} Task Groups are going to be migrated..", sourceTaskGroups.Count, taskGroupsToBeMigrated.Count());
            string baseUrl = Target.Organisation + "/" + Target.Project + "/_apis/distributedtask/taskgroups?api-version=5.1-preview";

            foreach (TaskGroup taskGroupToBeMigrated in taskGroupsToBeMigrated)
            {
                HttpClient client = getHttpClient(Target.AccessToken);

                Log.LogInformation("Processing TaskGroup '{TaskGroup}'..", taskGroupToBeMigrated.Name);
                string body = JsonConvert.SerializeObject(taskGroupToBeMigrated);

                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var result = client.PostAsync(baseUrl, content).Result;

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    Log.LogError("Error migrating Pipeling '{pipelineToBeMigrated}'. Please migrate it manually.", taskGroupToBeMigrated.Name);
                }
            }
        }

        private void createPipelineDefinitions(PipelineType pipelineType)
        {
            Log.LogInformation("Fetching Pipelines...");
            var sourceDefinitions = getPipelineDefinitions(Source.Organisation, Source.Project, Source.AccessToken, pipelineType);
            var targetDefinitions = getPipelineDefinitions(Target.Organisation, Target.Project, Target.AccessToken, pipelineType);

            //Filter out Pipelines that already exist
            var pipelinesToBeMigrated = sourceDefinitions.Where(s => !targetDefinitions.Any(t => t.Name == s.Name));

            Log.LogInformation($"From {sourceDefinitions.Count} source {pipelineType} Pipelines {pipelinesToBeMigrated.Count()} Pipelines are going to be migrated..");
            string baseUrl = Target.Organisation + "/" + Target.Project + "/_apis/build/definitions?api-version=5.1-preview";

            if (pipelineType == PipelineType.build)
            {
                foreach (BuildDefinition pipelineToBeMigrated in pipelinesToBeMigrated)
                {
                    var client = getHttpClient(Target.AccessToken);

                    pipelineToBeMigrated.Links = null;
                    pipelineToBeMigrated.AuthoredBy = null;
                    pipelineToBeMigrated.Queue = null;
                    pipelineToBeMigrated.Url = null;
                    pipelineToBeMigrated.Uri = null;
                    pipelineToBeMigrated.Revision = 0;
                    pipelineToBeMigrated.Id = 0;
                    pipelineToBeMigrated.Project = null;
                    pipelineToBeMigrated.Repository.Id = null;

                    Log.LogInformation("Processing Pipeline '{pipelineToBeMigrated}'..", pipelineToBeMigrated.Name);
                    string body = JsonConvert.SerializeObject(pipelineToBeMigrated);

                    var content = new StringContent(body, Encoding.UTF8, "application/json");
                    var result = client.PostAsync(baseUrl, content).Result;

                    if (result.StatusCode != HttpStatusCode.OK)
                    {
                        Log.LogError("Error migrating Pipeling '{pipelineToBeMigrated}'. Please migrate it manually.", pipelineToBeMigrated.Name);
                    }
                }
            }

            if (pipelineType == PipelineType.release)
            {
                foreach (ReleaseDefinition pipelineToBeMigrated in pipelinesToBeMigrated)
                {
                    HttpClient client = getHttpClient(Target.AccessToken);

                    pipelineToBeMigrated.Links = null;
                    pipelineToBeMigrated.Revision = 0;
                    pipelineToBeMigrated.Artifacts = null;
                    pipelineToBeMigrated.Url = null;
                    pipelineToBeMigrated.Links = null;
                    pipelineToBeMigrated.Id = 0;
                    pipelineToBeMigrated.VariableGroups = null;

                    Log.LogInformation("Processing Pipeline '{pipelineToBeMigrated}'..", pipelineToBeMigrated.Name);
                    string body = JsonConvert.SerializeObject(pipelineToBeMigrated);

                    var content = new StringContent(body, Encoding.UTF8, "application/json");
                    var result = client.PostAsync(baseUrl, content).Result;

                    if (result.StatusCode != HttpStatusCode.OK)
                    {
                        Log.LogError("Error migrating Pipeling '{pipelineToBeMigrated}'. Please migrate it manually.", pipelineToBeMigrated.Name);
                    }
                }
            }
        }
    }
}