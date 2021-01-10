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
        /// Executes Method for migrating Taskgroups, Variablegroups or Pipelines, depinding on whhat is set in the config.
        /// </summary>
        private void MigratePipelines()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            IEnumerable<Mapping> serviceConnectionMappings = null;
            IEnumerable<Mapping> taskGroupMappings = null;
            IEnumerable<Mapping> variableGroupMappings = null;
            if (_Options.MigrateServiceConnections)
            {
                serviceConnectionMappings = CreateServiceConnections();
            }
            if (_Options.MigrateTaskGroups)
            {
                taskGroupMappings = CreateTaskGroupDefinitions();
            }
            if (_Options.MigrateVariableGroups)
            {
                variableGroupMappings = CreateVariableGroupDefinitions();
            }
            if (_Options.MigrateBuildPipelines)
            {
                CreateBuildPipelines(taskGroupMappings, variableGroupMappings);
            }

            if (_Options.MigrateReleasePipelines)
            {
                CreateReleasePipelines(taskGroupMappings, variableGroupMappings);
            }
            stopwatch.Stop();
            Log.LogDebug("DONE in {Elapsed} ", stopwatch.Elapsed.ToString("c"));
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
        /// Method to get the RESP API URLs right
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
        /// Generic Method to get API Definitions (Taskgroups, Variablegroups, Build- or Release Pipelines)
        /// </summary>
        /// <typeparam name="DefinitionType">Type of Definition. Can be: Taskgroup, Build- or Release Pipeline</typeparam>
        /// <param name="organisation"></param>
        /// <param name="project"></param>
        /// <param name="accessToken"></param>
        /// <returns>List of API Definitions </returns>
        private IEnumerable<DefinitionType> GetApiDefinitions<DefinitionType>(string organisation, string project, string accessToken) where DefinitionType : RestApiDefinition, new()
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

                // Taskgroups only have a LIST option, so the following step is not needed
                if (!typeof(DefinitionType).ToString().Contains("TaskGroup"))
                {
                    foreach (RestApiDefinition definition in definitions.Value)
                    {
                        // Nessecary because getting all Pipelines doesn't include all of their properties
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
        /// Map the taskgroups that are already migrated
        /// </summary>
        /// <typeparam name="DefintionType"></typeparam>
        /// <param name="sourceDefinitions"></param>
        /// <param name="targetDefinitions"></param>
        /// <param name="newMappings"></param>
        /// <returns>Mapping list</returns>
        private IEnumerable<Mapping> AddAllreadySyncedMappings<DefintionType>(IEnumerable<DefintionType> sourceDefinitions, IEnumerable<DefintionType> targetDefinitions, IEnumerable<Mapping> newMappings) where DefintionType : RestApiDefinition, new()
        {
            // This is not safe, because the target project can have a taskgroup with the same name but with different content
            // To make this save we must add a local storage option for the mappings (sid, tid)
            var allMappings = newMappings.ToList();
            var allreadyMigratedDefintions = targetDefinitions.Where(t => newMappings.Any(m => m.TId == t.Id) == false).ToList();
            foreach (var item in allreadyMigratedDefintions)
            {
                var source = sourceDefinitions.FirstOrDefault(d => d.Name == item.Name);
                if (source == null)
                {
                    Log.LogInformation($"The {typeof(DefintionType).Name} {item.Name}({item.Id}) doesn't exsist in the source collection.");
                }
                else
                {
                    allMappings.Add(new()
                    {
                        SId = source.Id,
                        TId = item.Id,
                        Name = item.Name
                    });
                }
            }
            return allMappings;
        }

        /// <summary>
        /// Filter existing Definitions
        /// </summary>
        /// <typeparam name="DefinitionType"></typeparam>
        /// <param name="sourceDefinitions"></param>
        /// <param name="targetDefinitions"></param>
        /// <returns>List of filtered Definitions</returns>
        private IEnumerable<DefinitionType> filteredDefinitions<DefinitionType>(IEnumerable<DefinitionType> sourceDefinitions, IEnumerable<DefinitionType> targetDefinitions) where DefinitionType : RestApiDefinition, new()
        {
            var objectsToMigrate = sourceDefinitions.Where(s => !targetDefinitions.Any(t => t.Name == s.Name));
            Log.LogInformation($"{objectsToMigrate.Count()} of {sourceDefinitions.Count()} source {typeof(DefinitionType).Name}(s) are going to be migrated..");

            return objectsToMigrate;
        }

        /// <summary>
        /// Make HTTP Request to create a Definition
        /// </summary>
        /// <typeparam name="DefinitionType"></typeparam>
        /// <param name="definitionsToBeMigrated"></param>
        /// <returns>List of Mappings</returns>
        private IEnumerable<Mapping> CreateApiDefinitions<DefinitionType>(IEnumerable<DefinitionType> definitionsToBeMigrated) where DefinitionType : RestApiDefinition, new()
        {
            List<DefinitionType> migratedDefinitions = new List<DefinitionType>();
            var apiPathAttribute = typeof(DefinitionType).GetCustomAttributes(typeof(ApiPathAttribute), false).OfType<ApiPathAttribute>().FirstOrDefault();
            var apiNameAttribute = typeof(DefinitionType).GetCustomAttributes(typeof(ApiNameAttribute), false).OfType<ApiNameAttribute>().FirstOrDefault();
            if (apiPathAttribute == null)
            {
                throw new ArgumentNullException($"On the class defintion of '{typeof(DefinitionType).Name}' is the attribute 'ApiName' misssing. Please add the 'ApiName' Attribute to your class");
            }

            string baseUrl = getModUrl(Target.Organisation, Target.Project, apiPathAttribute, apiNameAttribute) + "?api-version=5.1-preview";

            foreach (RestApiDefinition definitionToBeMigrated in definitionsToBeMigrated)
            {
                var client = GetHttpClient(Target.AccessToken);
                var sourceId = definitionToBeMigrated.Id;
                definitionToBeMigrated.ResetObject();
                string body = JsonConvert.SerializeObject(definitionToBeMigrated);

                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var result = client.PostAsync(baseUrl, content).GetAwaiter().GetResult();

                if (result.StatusCode != HttpStatusCode.OK)
                {
                    Log.LogError($"Error migrating {apiNameAttribute.Name} {definitionToBeMigrated.Name}. Please migrate it manually.");
                    continue;
                }
                else
                {
                    var targetObject = JsonConvert.DeserializeObject<DefinitionType>(result.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                    migratedDefinitions.Add(targetObject);
                    yield return new Mapping()
                    {
                        Name = definitionToBeMigrated.Name,
                        SId = sourceId,
                        TId = targetObject.Id
                    };
                }
            }
            Log.LogInformation($"{migratedDefinitions.Count()} of {definitionsToBeMigrated.Count()} {typeof(DefinitionType).Name}(s) got migrated..");
        }

        private IEnumerable<Mapping> CreateBuildPipelines(IEnumerable<Mapping> TaskGroupMapping = null, IEnumerable<Mapping> VariableGroupMapping = null)
        {
            Log.LogInformation($"Processing Build Pipelines..");

            var sourceDefinitions = GetApiDefinitions<BuildDefinition>(Source.Organisation, Source.Project, Source.AccessToken);
            var targetDefinitions = GetApiDefinitions<BuildDefinition>(Target.Organisation, Target.Project, Target.AccessToken);
            var definitionsToBeMigrated = filteredDefinitions(sourceDefinitions, targetDefinitions);

            // Replace taskgroup and variablegroup sIds with tIds
            foreach (var definitionToBeMigrated in definitionsToBeMigrated)
            {
                if (definitionToBeMigrated.HasTaskGroups() && TaskGroupMapping != null)
                {
                    foreach (var phase in definitionToBeMigrated.Process.Phases)
                    {
                        foreach (var step in phase.Steps)
                        {
                            if (step.Task.DefinitionType.ToLower() != "metaTask".ToLower())
                            {
                                continue;
                            }
                            var mapping = TaskGroupMapping
                                .Where(d => d.SId == step.Task.Id.ToString()).FirstOrDefault();
                            if (mapping == null)
                            {
                                Log.LogWarning($"Can't find taskgroup {step.Task.Id.ToString()} in the target collection.");
                            }
                            else
                            {
                                step.Task.Id = mapping.TId;
                            }
                        }
                    }
                }
                else if (definitionToBeMigrated.HasTaskGroups() && TaskGroupMapping == null)
                {
                    Log.LogWarning("You can't migrate pipelines that uses variablegroups if you didn't migrate taskgroups");
                    definitionsToBeMigrated = definitionsToBeMigrated.Where(d => d.HasTaskGroups() == false);
                }

                if (definitionToBeMigrated.HasVariableGroups() && VariableGroupMapping != null)
                {
                    foreach (var variableGroup in definitionToBeMigrated.VariableGroups)
                    {
                        if (variableGroup != null)
                        {
                            continue;
                        }
                        var mapping = VariableGroupMapping
                            .Where(d => d.SId == variableGroup.Id.ToString()).FirstOrDefault();
                        if (mapping == null)
                        {
                            Log.LogWarning($"Can't find variablegroup {variableGroup.Id.ToString()} in the target collection.");
                        }
                        else
                        {
                            variableGroup.Id = mapping.TId;
                        }
                    }
                }
                else if (definitionToBeMigrated.HasTaskGroups() && VariableGroupMapping == null)
                {
                    Log.LogWarning("You can't migrate pipelines that uses taskgroups if you didn't migrate taskgroups");
                    definitionsToBeMigrated = definitionsToBeMigrated.Where(d => d.HasVariableGroups() == false);
                }
            }
            var mappings = CreateApiDefinitions<BuildDefinition>(definitionsToBeMigrated.ToList()).ToList();
            mappings = AddAllreadySyncedMappings(sourceDefinitions, targetDefinitions, mappings).ToList();
            return mappings;
        }

        private IEnumerable<Mapping> CreateReleasePipelines(IEnumerable<Mapping> TaskGroupMapping = null, IEnumerable<Mapping> VariableGroupMapping = null)
        {
            Log.LogInformation($"Processing Release Pipelines..");

            var sourceDefinitions = GetApiDefinitions<ReleaseDefinition>(Source.Organisation, Source.Project, Source.AccessToken);
            var targetDefinitions = GetApiDefinitions<ReleaseDefinition>(Target.Organisation, Target.Project, Target.AccessToken);
            var definitionsToBeMigrated = filteredDefinitions(sourceDefinitions, targetDefinitions);

            // Replace taskgroup and variablegroup sIds with tIds
            foreach (var definitionToBeMigrated in definitionsToBeMigrated)
            {
                if (definitionToBeMigrated.HasTaskGroups() && TaskGroupMapping != null)
                {
                    var Environments = definitionToBeMigrated.Environments;
                    foreach (var environment in Environments)
                    {
                        foreach (var deployPhase in environment.DeployPhases)
                        {
                            foreach (var WorkflowTask in deployPhase.WorkflowTasks)
                            {
                                if (WorkflowTask.DefinitionType.ToLower() != "metaTask".ToLower())
                                {
                                    continue;
                                }
                                var mapping = TaskGroupMapping
                                    .Where(d => d.SId == WorkflowTask.TaskId.ToString()).FirstOrDefault();
                                if (mapping == null)
                                {
                                    Log.LogWarning($"Can't find taskgroup {WorkflowTask.TaskId.ToString()} in the target collection.");
                                }
                                else
                                {
                                    WorkflowTask.TaskId = Guid.Parse(mapping.TId);
                                }
                            }
                        }
                    }
                }
                else if (definitionToBeMigrated.HasTaskGroups() && TaskGroupMapping == null)
                {
                    Log.LogWarning("You can't migrate pipelines that uses taskgroups if you didn't migrate taskgroups");
                    definitionsToBeMigrated = definitionsToBeMigrated.Where(d => d.HasTaskGroups() == false);
                }

                if (definitionToBeMigrated.HasVariableGroups() && VariableGroupMapping != null)
                {
                    var Environments = definitionToBeMigrated.Environments;
                    foreach (var environment in Environments)
                    {
                        List<int> TIds = new List<int>();
                        int count = 0;
                        foreach (var variableGroup in environment.VariableGroups)
                        {
                            count++;
                            var mapping = VariableGroupMapping.Where(d => d.SId == variableGroup.ToString()).FirstOrDefault();
                            if (mapping == null)
                            {
                                Log.LogWarning($"Can't find variablegroups {variableGroup.ToString()} in the target collection.");
                            }
                            else
                            {
                                TIds.Add(Convert.ToInt32(mapping.TId));
                            }
                        }
                        for (int id = 0; id < count; id++)
                        {
                            environment.VariableGroups[id] = TIds.ToArray()[id];
                        }
                    }
                }
                else if (definitionToBeMigrated.HasTaskGroups() && VariableGroupMapping == null)
                {
                    Log.LogWarning("You can't migrate pipelines that uses variablegroups if you didn't migrate variablegroups");
                    definitionsToBeMigrated = definitionsToBeMigrated.Where(d => d.HasTaskGroups() == false);
                }
            }
            var mappings = CreateApiDefinitions<ReleaseDefinition>(definitionsToBeMigrated).ToList();
            mappings = AddAllreadySyncedMappings(sourceDefinitions, targetDefinitions, mappings).ToList();
            return mappings;
        }

        private IEnumerable<Mapping> CreateServiceConnections()
        {
            Log.LogInformation($"Processing Service Connections..");

            var sourceDefinitions = GetApiDefinitions<ServiceConnection>(Source.Organisation, Source.Project, Source.AccessToken);
            var targetDefinitions = GetApiDefinitions<ServiceConnection>(Target.Organisation, Target.Project, Target.AccessToken);
            var mappings = CreateApiDefinitions(filteredDefinitions(sourceDefinitions, targetDefinitions)).ToList();
            mappings = AddAllreadySyncedMappings(sourceDefinitions, targetDefinitions, mappings).ToList();
            return mappings;
        }

        private IEnumerable<Mapping> CreateTaskGroupDefinitions()
        {
            Log.LogInformation($"Processing Taskgroups..");

            var sourceDefinitions = GetApiDefinitions<TaskGroup>(Source.Organisation, Source.Project, Source.AccessToken);
            var targetDefinitions = GetApiDefinitions<TaskGroup>(Target.Organisation, Target.Project, Target.AccessToken);
            var mappings = CreateApiDefinitions(filteredDefinitions(sourceDefinitions, targetDefinitions)).ToList();
            mappings = AddAllreadySyncedMappings(sourceDefinitions, targetDefinitions, mappings).ToList();
            return mappings;
        }

        private IEnumerable<Mapping> CreateVariableGroupDefinitions()
        {
            Log.LogInformation($"Processing Variablegroups..");

            var sourceDefinitions = GetApiDefinitions<VariableGroups>(Source.Organisation, Source.Project, Source.AccessToken);
            var targetDefinitions = GetApiDefinitions<VariableGroups>(Target.Organisation, Target.Project, Target.AccessToken);
            var mappings = CreateApiDefinitions(filteredDefinitions(sourceDefinitions, targetDefinitions)).ToList();
            mappings = AddAllreadySyncedMappings(sourceDefinitions, targetDefinitions, mappings).ToList();
            return mappings;
        }
    }
}