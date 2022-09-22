using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using MigrationTools.DataContracts;
using MigrationTools.DataContracts.Pipelines;
using MigrationTools.DataContracts.Process;
using MigrationTools.EndpointEnrichers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

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
        /// Create a new instance of HttpClient including Headers
        /// </summary>
        /// <param name="route">route that is appended after organization and project to create the full api url</param>
        /// <returns>HttpClient</returns>
        public HttpClient GetHttpClient(string route)
        {
            UriBuilder baseUrl = new UriBuilder(Options.Organisation);
            baseUrl.AppendPathSegments(Options.Project, "_apis", route);
            return CreateHttpClientWithHeaders(baseUrl.Uri.AbsoluteUri.ToString(), "api-version=6.0");
        }

        /// <summary>
        /// Create a new instance of HttpClient including Headers
        /// </summary>
        /// <param name="routeParams">strings that are injected into the route parameters of the definitions url</param>
        /// <returns>HttpClient</returns>
        private HttpClient GetHttpClient<DefinitionType>(params string[] routeParams)
            where DefinitionType : RestApiDefinition
        {
            UriBuilder baseUrl = GetUriBuilderBasedOnEndpointAndType<DefinitionType>(routeParams);
            var versionParameter = baseUrl.Query.Replace("?", "");
            return CreateHttpClientWithHeaders(baseUrl.Uri.AbsoluteUri.ToString().Replace(baseUrl.Query, ""), versionParameter);
        }

        /// <summary>
        /// Create a new instance of HttpClient including Headers
        /// </summary>
        /// <param name="url"></param>
        /// <param name="versionParameter">allows caller to override the default api version (ie. api-version=5.1)</param>
        /// <param name="routeParams">strings that are injected into the route parameters of the definitions url</param>
        /// <returns>HttpClient</returns>
        private HttpClient GetHttpClient(string url, string versionParameter, params object[] routeParams)
        {
            UriBuilder baseUrl = new UriBuilder(string.Format(url, routeParams));

            return CreateHttpClientWithHeaders(baseUrl.Uri.AbsoluteUri.ToString(), versionParameter);
        }

        private HttpClient CreateHttpClientWithHeaders(string url, string versionParameter)
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(url)
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", Options.AccessToken))));
            client.DefaultRequestHeaders.Add("Accept", $"application/json; {versionParameter}");
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            return client;
        }

        /// <summary>
        /// Method to get the RESP API URLs right
        /// </summary>
        /// <param name="routeParameters">strings that are injected into the route parameters of the definitions url</param>
        /// <returns>UriBuilder</returns>
        private UriBuilder GetUriBuilderBasedOnEndpointAndType<DefinitionType>(params string[] routeParameters)
            where DefinitionType : RestApiDefinition
        {
            var apiNameAttribute = typeof(DefinitionType).GetCustomAttributes(typeof(ApiNameAttribute), false).OfType<ApiNameAttribute>().FirstOrDefault();
            var apiPathAttribute = typeof(DefinitionType).GetCustomAttributes(typeof(ApiPathAttribute), false).OfType<ApiPathAttribute>().FirstOrDefault();
            if (apiPathAttribute == null)
            {
                throw new ArgumentNullException($"On the class defintion of '{typeof(DefinitionType).Name}' is the attribute 'ApiName' misssing. Please add the 'ApiName' Attribute to your class");
            }
            var builder = new UriBuilder(Options.Organisation);

            var pathSplit = apiPathAttribute.Path.Split('?');
            //string queryParts = "";
            //if (pathSplit.Length > 1)
            //{
            //    // reassemble query string
            //    queryParts = string.Join("?", pathSplit.Skip(1)).TrimStart('?');
            //}

            string unformatted = (apiPathAttribute.IncludeProject ? "/" + Options.Project : "") + "/_apis/" + pathSplit[0] + (apiPathAttribute.IncludeTrailingSlash ? "/" : "");
            builder.Path += Regex.IsMatch(unformatted, @"{\d}") ? string.Format(unformatted, routeParameters) : unformatted;

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
                builder.Query = "api-version=6.0";
            }
            return builder;
        }

        /// <summary>
        /// Generic Method to get API Definitions (Taskgroups, Variablegroups, Build- or Release Pipelines, ProcessDefinition)
        /// </summary>
        /// <typeparam name="DefinitionType">
        /// Type of Definition. Can be: Taskgroup, Build- or Release Pipeline, ProcessDefinition
        /// </typeparam>
        /// <param name="routeParameters">strings that are injected into the route parameters of the definitions url</param>
        /// <param name="queryString">additional query string parameters passed to the underlying api call</param>
        /// <param name="singleDefinitionQueryString">additional query string parameter passed when pulling the single instance details (ie. $expands, etc)</param>
        /// <param name="queryForDetails">a boolean flag to allow caller to skip the calls for each individual definition details</param>
        /// <returns>List of API Definitions</returns>
        public async Task<IEnumerable<DefinitionType>> GetApiDefinitionsAsync<DefinitionType>(string[] routeParameters = null, string queryString = "", string singleDefinitionQueryString = "", bool queryForDetails = true)
            where DefinitionType : RestApiDefinition, new()
        {
            var initialDefinitions = new List<DefinitionType>();
            routeParameters = routeParameters ?? new string[] { };

            var client = GetHttpClient<DefinitionType>(routeParameters);
            var httpResponse = await client.GetAsync("?" + queryString);

            var apiPathAttribute = typeof(DefinitionType).GetCustomAttributes(typeof(ApiPathAttribute), false).OfType<ApiPathAttribute>().FirstOrDefault();
            if (apiPathAttribute == null)
            {
                throw new ArgumentNullException($"On the class defintion of '{typeof(DefinitionType).Name}' is the attribute 'ApiName' misssing. Please add the 'ApiName' Attribute to your class");
            }

            if (httpResponse != null && httpResponse.StatusCode == HttpStatusCode.OK)
            {
                var definitions = await httpResponse.Content.ReadAsAsync<RestResultDefinition<DefinitionType>>();

                // Taskgroups only have a LIST option, so the following step is not needed
                if (!typeof(DefinitionType).ToString().Contains("TaskGroup") && queryForDetails)
                {
                    var client2 = GetHttpClient<DefinitionType>(routeParameters);
                    foreach (RestApiDefinition definition in definitions.Value)
                    {
                        // Nessecary because getting all Pipelines doesn't include all of their properties
                        var response = await client2.GetAsync(client2.BaseAddress + "/" + definition.Id + "?" + singleDefinitionQueryString);
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var fullDefinition = await response.Content.ReadAsAsync<DefinitionType>();
                            initialDefinitions.Add(fullDefinition);
                        }
                        else
                        {
                            Log.LogError("Failed on call to get single [{definitionName}] with Id [{definitionId}].\r\nUrl: GET {requestUri}\r\nResponse Code:{statusCode}", typeof(DefinitionType).Name, definition.Id, response.RequestMessage.RequestUri.ToString(), httpResponse.StatusCode);
                            throw new Exception(await response.Content.ReadAsStringAsync());
                        }
                    }
                }
                else
                {
                    initialDefinitions = definitions.Value?.ToList();
                }
            }
            else
            {
                throw new Exception($"Failed on call to get list of [{typeof(DefinitionType).Name}].\r\nUrl: GET {httpResponse.RequestMessage.RequestUri.ToString()}\r\nResponse Code:{httpResponse.StatusCode}\r\n{await httpResponse.Content.ReadAsStringAsync()}");

            }
            return initialDefinitions;
        }

        /// <summary>
        /// Get a single instance of a defined type
        /// </summary>
        /// <typeparam name="DefinitionType"></typeparam>
        /// <param name="routeParameters">strings that are injected into the route parameters of the definitions url</param>
        /// <param name="queryString">additional query string parameters passed to the underlying api call</param>
        /// <param name="singleDefinitionQueryString">additional query string parameter passed when pulling the single instance details (ie. $expands, etc)</param>
        /// <param name="queryForDetails">a boolean flag to allow caller to skip the calls for each individual definition details</param>
        /// <returns></returns>
        public async Task<DefinitionType> GetApiDefinitionAsync<DefinitionType>(string[] routeParameters = null, string queryString = "", string singleDefinitionQueryString = "", bool queryForDetails = true)
            where DefinitionType : RestApiDefinition, new()
        {
            var apiPathAttribute = typeof(DefinitionType).GetCustomAttributes(typeof(ApiPathAttribute), false).OfType<ApiPathAttribute>().FirstOrDefault();
            if (apiPathAttribute == null)
            {
                throw new ArgumentNullException($"On the class defintion of '{typeof(DefinitionType).Name}' is the attribute 'ApiName' misssing. Please add the 'ApiName' Attribute to your class");
            }

            routeParameters = routeParameters ?? new string[] { };

            var client = GetHttpClient<DefinitionType>(routeParameters);
            var httpResponse = await client.GetAsync("?" + queryString);
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Failed on call to get individual [{typeof(DefinitionType).Name}].\r\nUrl: GET {httpResponse.RequestMessage.RequestUri.ToString()}\r\n{await httpResponse.Content.ReadAsStringAsync()}");
            }

            return await httpResponse.Content.ReadAsAsync<DefinitionType>();
        }

        /// <summary>
        /// Make HTTP Request to add Revision / Version of Task Group
        /// </summary>
        /// <param name="targetDefinitions"></param>
        /// <param name="rootDefinitions"></param>
        /// <param name="updatedDefinitions"></param>
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
        /// <param name="parentIds">strings that are injected into the route parameters of the definitions url</param>
        /// <returns>List of Mappings</returns>
        public async Task<List<Mapping>> CreateApiDefinitionsAsync<DefinitionType>(IEnumerable<DefinitionType> definitionsToBeMigrated, params string[] parentIds)
            where DefinitionType : RestApiDefinition, new()
        {
            var migratedDefinitions = new List<Mapping>();

            foreach (var definitionToBeMigrated in definitionsToBeMigrated)
            {
                var client = GetHttpClient<DefinitionType>(parentIds);
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
                string body = JsonConvert.SerializeObject(definitionToBeMigrated.ToJson(), jsonSettings);

                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var result = await client.PostAsync(client.BaseAddress, content);

                var responseContent = await result.Content.ReadAsStringAsync();
                if (result.StatusCode != HttpStatusCode.OK && result.StatusCode != HttpStatusCode.Created)
                {

                    Log.LogError("Error migrating {DefinitionType}: {DefinitionName}. Please migrate it manually. \r\nUrl: POST {Url}\r\n{ErrorText}", typeof(DefinitionType).Name, definitionToBeMigrated.Name, result.RequestMessage.RequestUri.ToString(), responseContent);
                    continue;
                }
                else
                {
                    var targetObject = JsonConvert.DeserializeObject<DefinitionType>(responseContent);
                    definitionToBeMigrated.Id = targetObject.Id;
                    migratedDefinitions.Add(new Mapping()
                    {
                        Name = definitionToBeMigrated.Name,
                        SourceId = definitionToBeMigrated.GetSourceId(),
                        TargetId = targetObject.Id
                    });
                }
            }
            Log.LogInformation("{MigratedCount} of {TriedToMigrate} {DefinitionType}(s) got migrated..", migratedDefinitions.Count, definitionsToBeMigrated.Count(), typeof(DefinitionType).Name);
            return migratedDefinitions;
        }

        /// <summary>
        /// Make HTTP Request to update a Definition
        /// </summary>
        /// <typeparam name="DefinitionType"></typeparam>
        /// <param name="definitionsToBeMigrated"></param>
        /// <param name="parentIds">strings that are injected into the route parameters of the definitions url</param>
        /// <returns>List of Mappings</returns>
        public async Task<List<Mapping>> UpdateApiDefinitionsAsync<DefinitionType>(IEnumerable<DefinitionType> definitionsToBeMigrated, params string[] parentIds)
            where DefinitionType : RestApiDefinition, new()
        {
            var migratedDefinitions = new List<Mapping>();
            var apiPathAttribute = typeof(DefinitionType).GetCustomAttributes(typeof(ApiPathAttribute), false).OfType<ApiPathAttribute>().FirstOrDefault();

            foreach (var definitionToBeMigrated in definitionsToBeMigrated)
            {
                var client = GetHttpClient<DefinitionType>(parentIds);

                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };
                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = contractResolver,
                };
                string body = JsonConvert.SerializeObject(definitionToBeMigrated.ToJson(), jsonSettings);

                var content = new StringContent(body, Encoding.UTF8, "application/json");
                string suffix = "";
                if (apiPathAttribute == null || apiPathAttribute.IncludeIdOnUpdate)
                {
                    suffix = "/" + definitionToBeMigrated.Id;
                }
                HttpResponseMessage result = null;
                switch (apiPathAttribute.UpdateVerb)
                {
                    case HttpVerbs.Patch:
                        result = await client.PatchAsync(client.BaseAddress.ToString() + suffix, content);
                        break;
                    default:
                        result = await client.PutAsync(client.BaseAddress.ToString() + suffix, content);
                        break;
                }

                var responseContent = await result.Content.ReadAsStringAsync();
                if (result.StatusCode != HttpStatusCode.OK && result.StatusCode != HttpStatusCode.NotModified)
                {
                    Log.LogError("Error migrating {DefinitionType}: {DefinitionName}. Please migrate it manually. \r\nUrl: {Verb} {Url}\r\n{ErrorText}", typeof(DefinitionType).Name, definitionToBeMigrated.Name, Enum.GetName(typeof(HttpMethod), result.RequestMessage.Method), result.RequestMessage.RequestUri.ToString(), responseContent);
                    continue;
                }
                else if (result.StatusCode == HttpStatusCode.NotModified)
                {
                    Log.LogInformation("{DefinitionType}: {DefinitionName}) was already up to date.", typeof(DefinitionType).Name, definitionToBeMigrated.Name);
                }
                else
                {
                    var targetObject = JsonConvert.DeserializeObject<DefinitionType>(responseContent);
                    definitionToBeMigrated.Id = targetObject.Id;
                    migratedDefinitions.Add(new Mapping()
                    {
                        Name = definitionToBeMigrated.Name,
                        TargetId = targetObject.Id
                    });
                }
            }
            Log.LogInformation("{MigratedCount} of {TriedToMigrate} {DefinitionType}(s) got migrated..", migratedDefinitions.Count, definitionsToBeMigrated.Count(), typeof(DefinitionType).Name);
            return migratedDefinitions;
        }

        /// <summary>
        /// Performs an update or create operation on the target definition
        /// </summary>
        /// <typeparam name="DefinitionType"></typeparam>
        /// <param name="sourceDef"></param>
        /// <param name="targetDef"></param>
        /// <param name="parentIds"></param>
        /// <returns></returns>
        public async Task<DefinitionType> SyncDefinition<DefinitionType>(DefinitionType sourceDef, DefinitionType targetDef, params string[] parentIds)
            where DefinitionType : RestApiDefinition, ISynchronizeable<DefinitionType>, new()
        {
            if (targetDef == null || string.IsNullOrEmpty(targetDef.Id))
            {
                // Could not find targetProc, let's build it
                targetDef = sourceDef.CloneAsNew();

                var result = await CreateApiDefinitionsAsync<DefinitionType>(new DefinitionType[] { targetDef }, parentIds);
                if (result.Count == 0)
                {
                    // TODO: Give them option to bail out?
                }
                else
                {
                    Log.LogInformation("Created target {0} entry for [{1}] in [{2}]", typeof(DefinitionType).Name, targetDef.Name, Options.Name);
                }
            }
            else
            {
                // Go ahead and update the target process details
                targetDef.UpdateWithExisting(sourceDef);

                var result = await UpdateApiDefinitionsAsync<DefinitionType>(new DefinitionType[] { targetDef }, parentIds);
                if (result.Count == 0)
                {
                    // TODO: Give them option to bail out?
                }
                else
                {
                    Log.LogInformation("Updated target {0} entry for [{1}] in [{2}]", typeof(DefinitionType).Name, targetDef.Name, Options.Name);
                }
            }

            return targetDef;
        }

        /// <summary>
        /// Move work item group to another page and section
        /// </summary>
        /// <param name="group"></param>
        /// <param name="processId"></param>
        /// <param name="witRefName"></param>
        /// <param name="pageId"></param>
        /// <param name="sectionId"></param>
        /// <param name="oldPageId"></param>
        /// <param name="oldSectionId"></param>
        /// <returns></returns>
        public async Task<bool> MoveWorkItemGroupToNewPage(WorkItemGroup group, string processId, string witRefName, string pageId, string sectionId, string oldPageId, string oldSectionId)
        {
            return await MoveWorkItemGroup(group, processId, witRefName, pageId, sectionId, oldSectionId, oldPageId);
        }
        /// <summary>
        /// Move a work item group from one Layout->Section to another Layout->Section
        /// </summary>
        /// <param name="group"></param>
        /// <param name="processId"></param>
        /// <param name="witRefName"></param>
        /// <param name="pageId"></param>
        /// <param name="sectionId"></param>
        /// <param name="oldSectionId"></param>
        /// <returns></returns>
        public async Task<bool> MoveWorkItemGroupWithinPage(WorkItemGroup group, string processId, string witRefName, string pageId, string sectionId, string oldSectionId)
        {
            return await MoveWorkItemGroup(group, processId, witRefName, pageId, sectionId, oldSectionId);
        }


        private async Task<bool> MoveWorkItemGroup(WorkItemGroup group, string processId, string witRefName, string pageId, string sectionId, string oldSectionId, string oldPageId = null)
        {
            var client = GetHttpClient(
                $"{new UriBuilder(Options.Organisation)}/_apis/work/processes" +
                $"/{processId}/workItemTypes/{witRefName}/layout/pages/{pageId}/sections/{sectionId}/groups/{group.Id}?" +
                $"removeFromSectionId={oldSectionId}" +
                (!string.IsNullOrEmpty(oldPageId) ? $"&removeFromPageId={oldPageId}" : ""),
                "api-version=6.1-preview.1");

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = contractResolver,
            };
            string body = JsonConvert.SerializeObject(group.ToJson(), jsonSettings);

            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var result = await client.PutAsync(client.BaseAddress, content);

            var responseContent = await result.Content.ReadAsStringAsync();
            if (result.StatusCode != HttpStatusCode.OK && result.StatusCode != HttpStatusCode.Created)
            {
                Log.LogError("Error moving {DefinitionType}: {processId}::{witRefName}::{pageId}::{sectionId}::{groupLabel}. Please migrate it manually. \r\nUrl: PUT {Url}\r\n{ErrorText}", typeof(WorkItemGroup).Name, processId, witRefName, pageId, sectionId, group.Label, result.RequestMessage.RequestUri.ToString(), responseContent);
                return false;
            }
            else
            {
                var targetObject = JsonConvert.DeserializeObject<WorkItemGroup>(responseContent);
                group.Id = targetObject.Id;
                return true;
            }
        }

        /// <summary>
        /// Move a work item control from out Layout->Group to another Layout->Group
        /// </summary>
        /// <param name="control"></param>
        /// <param name="processId"></param>
        /// <param name="witRefName"></param>
        /// <param name="groupId"></param>
        /// <param name="fieldName"></param>
        /// <param name="oldGroupId"></param>
        /// <returns></returns>
        public async Task<bool> MoveWorkItemControlToOtherGroup(WorkItemControl control, string processId, string witRefName, string groupId, string fieldName, string oldGroupId)
        {
            return await MoveWorkItemControl(control, processId, witRefName, groupId, fieldName, oldGroupId);
        }
        /// <summary>
        /// Adds a work item control to an existing Layout->Group
        /// </summary>
        /// <param name="control"></param>
        /// <param name="processId"></param>
        /// <param name="witRefName"></param>
        /// <param name="groupId"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public async Task<bool> AddWorkItemControlToGroup(WorkItemControl control, string processId, string witRefName, string groupId, string fieldName)
        {
            return await MoveWorkItemControl(control, processId, witRefName, groupId, fieldName);
        }


        private async Task<bool> MoveWorkItemControl(WorkItemControl control, string processId, string witRefName, string groupId, string fieldName, string oldGroupId = null)
        {
            var client = GetHttpClient(
                $"{new UriBuilder(Options.Organisation)}/_apis/work/processes" +
                $"/{processId}/workItemTypes/{witRefName}/layout/groups/{groupId}/Controls/{fieldName}?" +
                (!string.IsNullOrEmpty(oldGroupId) ? $"&removeFromGroupId={oldGroupId}" : ""),
                "api-version=6.1-preview.1");

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = contractResolver,
            };
            string body = JsonConvert.SerializeObject(control.ToJson(), jsonSettings);

            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var result = await client.PutAsync(client.BaseAddress, content);

            var responseContent = await result.Content.ReadAsStringAsync();
            if (result.StatusCode != HttpStatusCode.OK && result.StatusCode != HttpStatusCode.Created)
            {
                Log.LogError("Error moving {DefinitionType}: {processId}::{witRefName}::{groupLabel}::{controlLabel}. Please migrate it manually. \r\nUrl: PUT {Url}\r\n{ErrorText}", typeof(WorkItemGroup).Name, processId, witRefName, groupId, control.Label, result.RequestMessage.RequestUri.ToString(), responseContent);
                return false;
            }
            else
            {
                var targetObject = JsonConvert.DeserializeObject<WorkItemControl>(responseContent);
                control.Id = targetObject.Id;
                return true;
            }
        }
    }

    public static class HttpClientExt
    {
        public static void AddToPath(this HttpClient client, string pathToAdd)
        {
            client.BaseAddress = new Uri($"https://{client.BaseAddress.Host}:{client.BaseAddress.Port}{client.BaseAddress.LocalPath}{pathToAdd}{client.BaseAddress.Query}");
        }
    }
}