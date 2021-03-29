using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.WebApi;
using MigrationTools.DataContracts;
using MigrationTools.DataContracts.Pipelines;
using MigrationTools.EndpointEnrichers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MigrationTools.Endpoints
{
    public class AzureDevOpsEndpoint : Endpoint<AzureDevOpsEndpointOptions>
    {
        public override int Count => 0;

        public AzureDevOpsEndpoint(EndpointEnricherContainer endpointEnrichers, ITelemetryLogger telemetry, ILogger<AzureDevOpsEndpoint> logger)
            : base(endpointEnrichers, telemetry, logger)
        {
        }

        public override void Configure(AzureDevOpsEndpointOptions options)
        {
            base.Configure(options);
            Log.LogDebug("AzureDevOpsEndpoint::Configure");
            if (string.IsNullOrEmpty(Options.Organisation))
            {
                throw new ArgumentNullException(nameof(Options.Organisation));
            }
            if (string.IsNullOrEmpty(Options.Project))
            {
                throw new ArgumentNullException(nameof(Options.Project));
            }
            if (string.IsNullOrEmpty(Options.AccessToken))
            {
                throw new ArgumentNullException(nameof(Options.AccessToken));
            }
        }

        /// <summary>
        /// Create a new instance of HttpClient including Heades
        /// </summary>
        /// <returns>HttpClient</returns>
        private HttpClient GetHttpClient<DefinitionType>()
            where DefinitionType : RestApiDefinition
        {
            UriBuilder baseUrl = GetUriBuilderBasedOnEndpointAndType<DefinitionType>();
            var versionParameter = baseUrl.Query.Replace("?", "");
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(baseUrl.Uri.ToString().Replace(baseUrl.Query, ""));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", Options.AccessToken))));
            client.DefaultRequestHeaders.Add("Accept", $"application/json; {versionParameter}");
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            return client;
        }

        /// <summary>
        /// Method to get the RESP API URLs right
        /// </summary>
        /// <returns>UriBuilder</returns>
        private UriBuilder GetUriBuilderBasedOnEndpointAndType<DefinitionType>()
            where DefinitionType : RestApiDefinition
        {
            var apiNameAttribute = typeof(DefinitionType).GetCustomAttributes(typeof(ApiNameAttribute), false).OfType<ApiNameAttribute>().FirstOrDefault();
            var apiPathAttribute = typeof(DefinitionType).GetCustomAttributes(typeof(ApiPathAttribute), false).OfType<ApiPathAttribute>().FirstOrDefault();
            if (apiPathAttribute == null)
            {
                throw new ArgumentNullException($"On the class defintion of '{typeof(DefinitionType).Name}' is the attribute 'ApiName' misssing. Please add the 'ApiName' Attribute to your class");
            }
            var builder = new UriBuilder(Options.Organisation);
            builder.Path += Options.Project + "/_apis/" + apiPathAttribute.Path + "/";

            if (apiNameAttribute.Name == "Release Piplines")
            {
                if (builder.Host.Contains("dev.azure.com"))
                {
                    builder.Host = "vsrm." + builder.Host;
                    builder.Query = "api-version=6.0";
                }
                else if (builder.Host.Contains("visualstudio.com"))
                {
                    int num = builder.Host.IndexOf(".visualstudio.com");
                    builder.Host = builder.Host.Substring(0, num) + ".vsrm.visualstudio.com";
                    builder.Query = "api-version=6.0";
                }
                else
                {
                    builder.Query = "api-version=5.1";
                }
            }
            else
            {
                builder.Query = "api-version=5.1-preview";
            }
            return builder;
        }

        /// <summary>
        /// Generic Method to get API Definitions (Taskgroups, Variablegroups, Build- or Release Pipelines)
        /// </summary>
        /// <typeparam name="DefinitionType">
        /// Type of Definition. Can be: Taskgroup, Build- or Release Pipeline
        /// </typeparam>
        /// <returns>List of API Definitions</returns>
        public async Task<IEnumerable<DefinitionType>> GetApiDefinitionsAsync<DefinitionType>()
            where DefinitionType : RestApiDefinition, new()
        {
            var initialDefinitions = new List<DefinitionType>();

            var client = GetHttpClient<DefinitionType>();
            var httpResponse = await client.GetAsync("");

            if (httpResponse != null)
            {
                var definitions = await httpResponse.Content.ReadAsAsync<RestResultDefinition<DefinitionType>>();

                // Taskgroups only have a LIST option, so the following step is not needed
                if (!typeof(DefinitionType).ToString().Contains("TaskGroup"))
                {
                    foreach (RestApiDefinition definition in definitions.Value)
                    {
                        // Nessecary because getting all Pipelines doesn't include all of their properties
                        var response = await client.GetAsync(client.BaseAddress + "/" + definition.Id);
                        var fullDefinition = await response.Content.ReadAsAsync<DefinitionType>();
                        initialDefinitions.Add(fullDefinition);
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
        /// Make HTTP Request to create a Definition
        /// </summary>
        /// <typeparam name="TaskGroup"></typeparam>
        /// <param name="definitionsToBeMigrated"></param>
        /// <returns>List of Mappings</returns>
        public async Task<List<Mapping>> UpdateTaskGroupsAsync(IEnumerable<TaskGroup> targetDefinitions, IEnumerable<TaskGroup> rootDefinitions, IEnumerable<TaskGroup> updatedDefinitions)
        {
            var migratedDefinitions = new List<Mapping>();
            foreach (var definitionToBeMigrated in updatedDefinitions)
            {
                var relatedRootDefinition = rootDefinitions.FirstOrDefault(d => definitionToBeMigrated.Name == d.Name);
                var taskGroupId = rootDefinitions.FirstOrDefault(d => definitionToBeMigrated.Name == d.Name).Id;
                definitionToBeMigrated.ParentDefinitionId = taskGroupId;
                definitionToBeMigrated.Version.IsTest = true;

                var client = GetHttpClient<TaskGroup>();
                definitionToBeMigrated.ResetObject();

                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };
                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = contractResolver,
                };
                string body = JsonConvert.SerializeObject(definitionToBeMigrated, jsonSettings);

                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var result = await client.PostAsync(client.BaseAddress, content);

                var responseContent = await result.Content.ReadAsStringAsync();
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    Log.LogError("Error migrating {DefinitionType}: {DefinitionName}. Please migrate it manually. {ErrorText}", typeof(TaskGroup).Name, definitionToBeMigrated.Name, responseContent);
                    continue;
                }
                else
                {
                    var tempTaskGroup = new TaskGroup()
                    {
                        Comment = "Draft published as preview ",
                        ParentDefinitionRevision = relatedRootDefinition.Revision,
                        Preview = true,
                        TaskGroupId = JsonConvert.DeserializeObject<TaskGroup>(responseContent).Id,
                        TaskGroupRevision = 1
                    };

                    body = JsonConvert.SerializeObject(tempTaskGroup, jsonSettings);

                    content = new StringContent(body, Encoding.UTF8, "application/json");
                    result = await client.PutAsync(client.BaseAddress.ToString() + $"?parentTaskGroupId={taskGroupId}", content);

                    responseContent = await result.Content.ReadAsStringAsync();
                    if (result.StatusCode != HttpStatusCode.OK)
                    {
                        Log.LogError("Error migrating {DefinitionType}: {DefinitionName}. Please migrate it manually. {ErrorText}", typeof(TaskGroup).Name, definitionToBeMigrated.Name, responseContent);
                        continue;
                    }
                    else
                    {
                        relatedRootDefinition.Revision += 1;
                        tempTaskGroup = new TaskGroup()
                        {
                            Comment = "Published TaskGroup",
                            Revision = relatedRootDefinition.Revision,
                            Preview = false,
                            Version = new DataContracts.Pipelines.Version
                            {
                                Major = definitionToBeMigrated.Version.Major,
                                Minor = definitionToBeMigrated.Version.Minor,
                                Patch = definitionToBeMigrated.Version.Patch,
                                IsTest = false
                            }
                        };

                        body = JsonConvert.SerializeObject(tempTaskGroup, jsonSettings);

                        content = new StringContent(body, Encoding.UTF8, "application/json");
                        result = await client.PatchAsync(client.BaseAddress + $"/{taskGroupId}?disablePriorVersions=false", content);

                        responseContent = await result.Content.ReadAsStringAsync();
                        if (result.StatusCode != HttpStatusCode.OK)
                        {
                            Log.LogError("Error migrating {DefinitionType}: {DefinitionName}. Please migrate it manually. {ErrorText}", typeof(TaskGroup).Name, definitionToBeMigrated.Name, responseContent);
                            continue;
                        }
                        else
                        {
                            var targetObject = JsonConvert.DeserializeObject<TaskGroup>(responseContent);
                            migratedDefinitions.Add(new Mapping()
                            {
                                Name = definitionToBeMigrated.Name,
                                SourceId = definitionToBeMigrated.Id,
                                TargetId = targetObject.Id
                            });
                        }
                    }
                }
                Log.LogInformation("{RevisionCount} of {TriedToMigrate} Revisions from {DefinitionName} got migrated..", migratedDefinitions.Count, updatedDefinitions.Count(), definitionToBeMigrated.Name);
            }
            return migratedDefinitions;
        }

        /// <summary>
        /// Make HTTP Request to create a Definition
        /// </summary>
        /// <typeparam name="DefinitionType"></typeparam>
        /// <param name="definitionsToBeMigrated"></param>
        /// <returns>List of Mappings</returns>
        public async Task<List<Mapping>> CreateApiDefinitionsAsync<DefinitionType>(IEnumerable<DefinitionType> definitionsToBeMigrated)
            where DefinitionType : RestApiDefinition, new()
        {
            var migratedDefinitions = new List<Mapping>();

            foreach (var definitionToBeMigrated in definitionsToBeMigrated)
            {
                var client = GetHttpClient<DefinitionType>();
                definitionToBeMigrated.ResetObject();

                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };
                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = contractResolver,
                };
                string body = JsonConvert.SerializeObject(definitionToBeMigrated, jsonSettings);

                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var result = await client.PostAsync(client.BaseAddress, content);

                var responseContent = await result.Content.ReadAsStringAsync();
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    Log.LogError("Error migrating {DefinitionType}: {DefinitionName}. Please migrate it manually. {ErrorText}", typeof(DefinitionType).Name, definitionToBeMigrated.Name, responseContent);
                    continue;
                }
                else
                {
                    var targetObject = JsonConvert.DeserializeObject<DefinitionType>(responseContent);
                    migratedDefinitions.Add(new Mapping()
                    {
                        Name = definitionToBeMigrated.Name,
                        SourceId = definitionToBeMigrated.Id,
                        TargetId = targetObject.Id
                    });
                }
            }
            Log.LogInformation("{MigratedCount} of {TriedToMigrate} {DefinitionType}(s) got migrated..", migratedDefinitions.Count, definitionsToBeMigrated.Count(), typeof(DefinitionType).Name);
            return migratedDefinitions;
        }
    }
}