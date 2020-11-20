using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Enrichers.Pipelines;
using Newtonsoft.Json;

namespace MigrationTools.Processors
{
    internal class TfsPipelineProcessor : Processor
    {
        private TfsPipelineProcessorOptions _Options;

        public TfsPipelineProcessor(ProcessorEnricherContainer processorEnrichers, EndpointContainer endpoints, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(processorEnrichers, endpoints, services, telemetry, logger)
        {
        }

        public TfsEndpoint Source => (TfsEndpoint)Endpoints.Source;

        public TfsEndpoint Target => (TfsEndpoint)Endpoints.Target;

        private List<BuildDefinition> sourceBuildDefinition = new List<BuildDefinition>();
        private List<BuildDefinition> targetBuildDefinition = new List<BuildDefinition>();
        private List<ReleaseDefinition> sourceReleaseDefinitions = new List<ReleaseDefinition>();
        private List<ReleaseDefinition> targetReleaseDefinitions = new List<ReleaseDefinition>();
        private List<TaskGroup> sourceTaskGroups = new List<TaskGroup>();
        private List<TaskGroup> targetTaskGroups = new List<TaskGroup>();

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

            if (_Options.MigrateTaskGroups)
            {
                CreateTaskGroups();
            }
            if (_Options.MigrateBuildPipelines)
            {
                CreateBuildDefinitions();
            }

            if (_Options.MigrateReleasePipelines)
            {
                CreateReleaseDefinitions();
            }
            stopwatch.Stop();
            Log.LogDebug("DONE in {Elapsed} ", stopwatch.Elapsed.ToString("c"));
        }

        private void GetTaskGroups(string organisation, string project, string accessToken)
        {
            string baseUrl = organisation + "/" + project + "/_apis/distributedtask/taskgroups";
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + accessToken));

            WebClient client = new WebClient();
            client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
            string httpResponse = client.DownloadString(baseUrl);

            if (httpResponse != null)
            {
                TaskGroups taskGroups = JsonConvert.DeserializeObject<TaskGroups>(httpResponse);

                foreach (TaskGroup taskGroup in taskGroups.Value)
                {
                    if (organisation == Source.Organisation)
                    {
                        sourceTaskGroups.Add(taskGroup);
                    }
                    else
                    {
                        targetTaskGroups.Add(taskGroup);
                    }
                }
            }
        }

        private void GetBuildDefinitions(string organisation, string project, string accessToken)
        {
            string baseUrl = organisation + "/" + project + "/_apis/build/definitions";
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + accessToken));

            WebClient client = new WebClient();
            client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
            string httpResponse = client.DownloadString(baseUrl);

            if (httpResponse != null)
            {
                BuildDefinitions pipelines = JsonConvert.DeserializeObject<BuildDefinitions>(httpResponse);

                foreach (BuildDefinition pipeline in pipelines.Value)
                {
                    //Nessecary because getting all Pipelines doesn't include all of their properties
                    string responseMessage = client.DownloadString(baseUrl + "/" + pipeline.Id);

                    BuildDefinition newPipeline = JsonConvert.DeserializeObject<BuildDefinition>(responseMessage);

                    if (organisation == Source.Organisation)
                    {
                        sourceBuildDefinition.Add(newPipeline);
                    }
                    else
                    {
                        targetBuildDefinition.Add(newPipeline);
                    }
                }
            }
        }

        private void GetReleaseDefinitions(string organisation, string project, string accessToken)
        {
            string baseUrl = organisation + "/" + project + "/_apis/release/definitions";
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + accessToken));

            WebClient client = new WebClient();
            client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
            string httpResponse = client.DownloadString(baseUrl);

            if (httpResponse != null)
            {
                ReleaseDefinitions pipelines = JsonConvert.DeserializeObject<ReleaseDefinitions>(httpResponse);

                foreach (ReleaseDefinition pipeline in pipelines.Value)
                {
                    //Nessecary because getting all Pipelines doesn't include all of their properties
                    string responseMessage = client.DownloadString(baseUrl + "/" + pipeline.Id);

                    ReleaseDefinition newPipeline = JsonConvert.DeserializeObject<ReleaseDefinition>(responseMessage);

                    if (organisation == Source.Organisation)
                    {
                        sourceReleaseDefinitions.Add(newPipeline);
                    }
                    else
                    {
                        targetReleaseDefinitions.Add(newPipeline);
                    }
                }
            }
        }

        private void CreateTaskGroups()
        {
            List<TaskGroup> taskGroupsToBeMigrated = new List<TaskGroup>();
            Log.LogInformation("Fetching TaskGroups...");
            GetTaskGroups(Source.Organisation, Source.Project, Source.AccessToken);
            GetTaskGroups(Target.Organisation, Target.Project, Target.AccessToken);

            //Filter out Pipelines that already exsit
            foreach (TaskGroup sourceTaskGroup in sourceTaskGroups)
            {
                int exsits = 0;
                foreach (TaskGroup targetTaskGroup in targetTaskGroups)
                {
                    if (targetTaskGroup.Name == sourceTaskGroup.Name)
                    {
                        exsits++;
                    }
                }
                if (exsits == 0)
                {
                    taskGroupsToBeMigrated.Add(sourceTaskGroup);
                }
            }
            Log.LogInformation("From {sourceTaskGroupss} source Task Groups {taskGroupsToBeMigrated} Task Groups are going to be migrated..", taskGroupsToBeMigrated.Count, taskGroupsToBeMigrated.Count);
            string baseUrl = Target.Organisation + "/" + Target.Project + "/_apis/distributedtask/taskgroups?api-version=5.1-preview";
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + Target.AccessToken));

            foreach (TaskGroup taskGroupToBeMigrated in taskGroupsToBeMigrated)
            {
                WebClient client = new WebClient();
                client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                Log.LogInformation("Processing TaskGroup '{TaskGroup}'..", taskGroupToBeMigrated.Name);
                string body = JsonConvert.SerializeObject(taskGroupToBeMigrated);
                try
                {
                    client.UploadString(baseUrl, "POST", body);
                }
                catch
                {
                    Log.LogError("Error migrating TaskGroup '{TaskGroup}'. Please migrate it manually.", taskGroupToBeMigrated.Name);
                }
            }
        }

        private void CreateBuildDefinitions()
        {
            List<BuildDefinition> pipelinesToBeMigrated = new List<BuildDefinition>();
            Log.LogInformation("Fetching Build Pipelines...");
            GetBuildDefinitions(Source.Organisation, Source.Project, Source.AccessToken);
            GetBuildDefinitions(Target.Organisation, Target.Project, Target.AccessToken);

            //Filter out Pipelines that already exsit
            foreach (BuildDefinition sourcePipeline in sourceBuildDefinition)
            {
                int exsits = 0;
                foreach (BuildDefinition targetPipeline in targetBuildDefinition)
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
            Log.LogInformation("From {sourcePipelines} source Pipelines {pipelinesToBeMigrated} Pipelines are going to be migrated..", sourceBuildDefinition.Count, pipelinesToBeMigrated.Count);
            string baseUrl = Target.Organisation + "/" + Target.Project + "/_apis/build/definitions?api-version=5.1-preview";
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + Target.AccessToken));

            foreach (BuildDefinition pipelineToBeMigrated in pipelinesToBeMigrated)
            {
                WebClient client = new WebClient();
                client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

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
                try
                {
                    client.UploadString(baseUrl, "POST", body);
                }
                catch
                {
                    Log.LogError("Error migrating Pipeling '{pipelineToBeMigrated}'. Please migrate it manually.", pipelineToBeMigrated.Name);
                }
            }
        }

        private void CreateReleaseDefinitions()
        {
            List<ReleaseDefinition> pipelinesToBeMigrated = new List<ReleaseDefinition>();
            Log.LogInformation("Fetching Release Pipelines...");
            GetReleaseDefinitions(Source.Organisation, Source.Project, Source.AccessToken);
            GetReleaseDefinitions(Target.Organisation, Target.Project, Target.AccessToken);

            //Filter out Pipelines that already exsit
            foreach (ReleaseDefinition sourcePipeline in sourceReleaseDefinitions)
            {
                int exsits = 0;
                foreach (ReleaseDefinition targetPipeline in targetReleaseDefinitions)
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
            Log.LogInformation("From {sourcePipelines} source Pipelines {pipelinesToBeMigrated} Pipelines are going to be migrated..", sourceReleaseDefinitions.Count, pipelinesToBeMigrated.Count);
            string baseUrl = Target.Organisation + "/" + Target.Project + "/_apis/release/definitions?api-version=5.1-preview";
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(":" + Target.AccessToken));

            foreach (ReleaseDefinition pipelineToBeMigrated in pipelinesToBeMigrated)
            {
                WebClient client = new WebClient();
                client.Headers[HttpRequestHeader.Authorization] = "Basic " + credentials;
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                pipelineToBeMigrated.Links = null;
                pipelineToBeMigrated.Revision = 0;
                pipelineToBeMigrated.Artifacts = null;
                pipelineToBeMigrated.Url = null;
                pipelineToBeMigrated.Links = null;
                pipelineToBeMigrated.Id = 0;
                pipelineToBeMigrated.VariableGroups = null;

                Log.LogInformation("Processing Pipeline '{pipelineToBeMigrated}'..", pipelineToBeMigrated.Name);
                string body = JsonConvert.SerializeObject(pipelineToBeMigrated);
                try
                {
                    client.UploadString(baseUrl, "POST", body);
                }
                catch
                {
                    Log.LogError("Error migrating Pipeling '{pipelineToBeMigrated}'. Please migrate it manually.", pipelineToBeMigrated.Name);
                }
            }
        }
    }
}