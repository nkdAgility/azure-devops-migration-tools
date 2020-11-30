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
    /// <summary>
    /// Azure DevOps Processor that migrates Taskgroups, Build- and Release Pipelines.
    /// </summary>
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
        /// <summary>
        /// Executes Method for migrating Taskgroups or Pipelines, depinding on whhat is set in the config.
        /// </summary>
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

        /// <summary>
        /// Ugly Method to get the RESP API URLs right
        /// </summary>
        /// <param name="organisation"></param>
        /// <param name="project"></param>
        /// <param name="apiPathAttribute">REST API Path</param>
        /// <param name="apiNameAttribute">Name of Object</param>
        /// <returns>API URL</returns>
        public string getModUrl(string organisation, string project, ApiPathAttribute apiPathAttribute, ApiNameAttribute apiNameAttribute)
        {
            string schema = string.Empty;
            string modUrl = string.Empty;

            if (apiNameAttribute.Name == "Release Piplines")
            {
                if (organisation.Contains("dev.azure.com"))
                {
                    if (organisation.Contains("https://"))
                    {
                        organisation = organisation.Replace("https://", "");
                        schema = "https://";
                    }
                    else if (organisation.Contains("http://"))
                    {
                        organisation = organisation.Replace("http://", "");
                        schema = "http://";
                    }
                    else
                    {
                        throw new Exception("The configured Organization has a wrong format");
                    }
                    organisation = schema + "vsrm." + organisation;
                    modUrl = organisation + project + "/_apis/" + apiPathAttribute.Path;
                }
                else if (organisation.Contains("visualstudio.com"))
                {
                    string domain = string.Empty;
                    if (organisation.Contains("https://"))
                    {
                        organisation = organisation.Replace("https://", "");
                        int num = organisation.IndexOf(".visualstudio.com");
                        domain = organisation.Substring(0, num);
                        organisation = organisation.Replace(domain, "");
                        schema = "https://";
                    }
                    else if (organisation.Contains("http://"))
                    {
                        organisation = organisation.Replace("http://", "");
                        int num = organisation.IndexOf(".visualstudio.com");
                        domain = organisation.Substring(0, num);
                        organisation = organisation.Replace(domain, "");
                        schema = "http://";
                    }
                    else
                    {
                        throw new Exception("The configured Organization has a wrong format");
                    }
                    organisation = schema + domain + ".vsrm" + organisation;
                    modUrl = organisation + project + "/_apis/" + apiPathAttribute.Path;
                }
                else
                {
                    modUrl = organisation + project + "/_apis/" + apiPathAttribute.Path;
                }
            }
            else
            {
                modUrl = organisation + project + "/_apis/" + apiPathAttribute.Path;
            }
            return modUrl;
        }

        /// <summary>
        /// Create a new instance of HttpClient including Heades
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns>HttpClient</returns>
        private HttpClient GetHttpClient(string accessToken)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", accessToken))));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            return client;
        }

        /// <summary>
        /// Generic Method to get API Definitions (Taskgroups, Build- or Release Pipelines)
        /// </summary>
        /// <typeparam name="DefinitionType">Type of Definition. Can be: Taskgroup, Build- or Release Pipeline</typeparam>
        /// <param name="organisation"></param>
        /// <param name="project"></param>
        /// <param name="accessToken"></param>
        /// <returns>List of API Definitions </returns>
        private IList<DefinitionType> GetApiDefinitions<DefinitionType>(string organisation, string project, string accessToken) where DefinitionType : RestApiDefinition, new()
        {
            var apiNameAttribute = typeof(DefinitionType).GetCustomAttributes(typeof(ApiNameAttribute), false).OfType<ApiNameAttribute>().FirstOrDefault();
            var apiPathAttribute = typeof(DefinitionType).GetCustomAttributes(typeof(ApiPathAttribute), false).OfType<ApiPathAttribute>().FirstOrDefault();
            if (apiPathAttribute == null)
            {
                throw new ArgumentNullException($"On the class defintion of '{typeof(DefinitionType).Name}' is the attribute 'ApiName' misssing. Please add the 'ApiName' Attribute to your class");
            }
            string baseUrl = getModUrl(organisation, project, apiPathAttribute, apiNameAttribute);
            var initialDefinitions = new List<DefinitionType>();

            HttpClient client = GetHttpClient(accessToken);

            string httpResponse = client.GetStringAsync(baseUrl).Result;

            if (httpResponse != null)
            {
                var definitions = JsonConvert.DeserializeObject<RestResultDefinition<DefinitionType>>(httpResponse);

                if (!typeof(DefinitionType).ToString().Contains("TaskGroup"))
                {
                    foreach (RestApiDefinition definition in definitions.Value)
                    {
                        //Nessecary because getting all Pipelines doesn't include all of their properties
                        string responseMessage = client.GetStringAsync(baseUrl + "/" + definition.Id).Result;
                        var test = JsonConvert.DeserializeObject<RestResultDefinition<DefinitionType>>(responseMessage);
                        initialDefinitions.Add(JsonConvert.DeserializeObject<DefinitionType>(responseMessage));
                    }
                }
                else
                {
                    initialDefinitions = definitions.Value.ToList();
                }

            }
            return initialDefinitions;
        }

        /// <summary>
        /// Method to create an Api Definition
        /// </summary>
        /// <typeparam name="DefinitionType">Type of Definition. Can be: Taskgroup, Build- or Release Pipeline</typeparam>
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
            string baseUrl = getModUrl(Target.Organisation, Target.Project, apiPathAttribute, apiNameAttribute) + "?api-version=5.1-preview";

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