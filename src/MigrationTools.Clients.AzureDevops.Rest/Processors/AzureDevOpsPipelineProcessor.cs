using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.DataContracts.Pipelines;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using Newtonsoft.Json;

namespace MigrationTools.Processors
{
    public partial class AzureDevOpsPipelineProcessor : Processor
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
            Log.LogInformation("AzureDevOpsPipelineProcessor::Configure");
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
                CreateApiDefinitions<TaskGroup>();
            }
            if (_Options.MigrateBuildPipelines)
            {
                CreateApiDefinitions<BuildDefinition>();
            }

            if (_Options.MigrateReleasePipelines)
            {
                CreateApiDefinitions<ReleaseDefinition>();
            }
            stopwatch.Stop();
            Log.LogDebug("DONE in {Elapsed} ", stopwatch.Elapsed.ToString("c"));
        }

        private HttpClient GetHttpClient(string accessToken)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", accessToken))));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            return client;
        }

        private IList<DefinitionType> GetApiDefinitions<DefinitionType>(string organisation, string project, string accessToken) where DefinitionType : RestApiDefinition, new()
        {
            var apiPathAttribute = typeof(DefinitionType).GetCustomAttributes(typeof(ApiPathAttribute), false).OfType<ApiPathAttribute>().FirstOrDefault();
            if (apiPathAttribute == null)
            {
                throw new ArgumentNullException($"On the class defintion of '{typeof(DefinitionType).Name}' is the attribute 'ApiName' misssing. Please add the 'ApiName' Attribute to your class");
            }
            string baseUrl = $"{organisation}/{project}/_apis/{apiPathAttribute.Path}";
            var initialDefinitions = new List<DefinitionType>();

            HttpClient client = GetHttpClient(accessToken);

            string httpResponse = client.GetStringAsync(baseUrl).Result;

            if (httpResponse != null)
            {
                var definitions = JsonConvert.DeserializeObject<RestResultDefinition<DefinitionType>>(httpResponse);

                foreach (RestApiDefinition definition in definitions.Value)
                {
                    //Nessecary because getting all Pipelines doesn't include all of their properties
                    string responseMessage = client.GetStringAsync(baseUrl + "/" + definition.Id).Result;
                    initialDefinitions.Add(JsonConvert.DeserializeObject<DefinitionType>(responseMessage));
                }
            }
            return initialDefinitions;
        }

        private void CreateApiDefinitions<DefinitionType>() where DefinitionType : RestApiDefinition, new()
        {
            Log.LogInformation("Fetching Definitions...");
            var sourceDefinitions = GetApiDefinitions<DefinitionType>(Source.Organisation, Source.Project, Source.AccessToken);
            var targetDefinitions = GetApiDefinitions<DefinitionType>(Target.Organisation, Target.Project, Target.AccessToken);
            var apiPathAttribute = typeof(DefinitionType).GetCustomAttributes(typeof(ApiPathAttribute), false).OfType<ApiPathAttribute>().FirstOrDefault();
            var apiNameAttribute = typeof(DefinitionType).GetCustomAttributes(typeof(ApiNameAttribute), false).OfType<ApiNameAttribute>().FirstOrDefault();
            if (apiPathAttribute == null)
            {
                throw new ArgumentNullException($"On the class defintion of '{typeof(DefinitionType).Name}' is the attribute 'ApiName' misssing. Please add the 'ApiName' Attribute to your class");
            }

            //Filter out Pipelines that already exist
            var definitionsToBeMigrated = sourceDefinitions.Where(s => !targetDefinitions.Any(t => t.Name == s.Name));

            Log.LogInformation($"From {sourceDefinitions.Count} source {apiNameAttribute.Name} {definitionsToBeMigrated.Count()} {apiNameAttribute.Name} are going to be migrated..");
            string baseUrl = $"{Target.Organisation}/{Target.Project}/_apis/{apiPathAttribute.Path}?api-version=5.1-preview";

            foreach (RestApiDefinition definitionToBeMigrated in definitionsToBeMigrated)
            {
                var client = GetHttpClient(Target.AccessToken);

                var objectToMigrate = definitionToBeMigrated.ResetObject();
                Log.LogInformation($"Processing {apiNameAttribute.Name} {objectToMigrate.Name}..");
                string body = JsonConvert.SerializeObject(objectToMigrate);

                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var result = client.PostAsync(baseUrl, content).Result;

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    Log.LogError($"Error migrating {apiNameAttribute.Name} {objectToMigrate.Name}. Please migrate it manually.");
                }
            }
        }
    }
}