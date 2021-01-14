using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.EndpointEnrichers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MigrationTools.Endpoints
{
    public class AzureDevOpsEndpoint : Endpoint, IAzureDevOpsEndpointOptions
    {
        private IAzureDevOpsEndpointOptions _Options;

        public string AccessToken => _Options.AccessToken;
        public string Organisation => _Options.Organisation;
        public string Project => _Options.Project;
        public string ReflectedWorkItemIdField => _Options.ReflectedWorkItemIdField;
        public AuthenticationMode AuthenticationMode => _Options.AuthenticationMode;

        public override int Count => 0;

        public AzureDevOpsEndpoint(EndpointEnricherContainer endpointEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Endpoint> logger) : base(endpointEnrichers, services, telemetry, logger)
        {
        }

        public override void Configure(IEndpointOptions options)
        {
            base.Configure(options);
            Log.LogDebug("AzureDevOpsEndpoint::Configure");
            _Options = (IAzureDevOpsEndpointOptions)options;
            if (string.IsNullOrEmpty(_Options.Organisation))
            {
                throw new ArgumentNullException(nameof(_Options.Organisation));
            }
            if (string.IsNullOrEmpty(_Options.Project))
            {
                throw new ArgumentNullException(nameof(_Options.Project));
            }
            if (string.IsNullOrEmpty(_Options.AccessToken))
            {
                throw new ArgumentNullException(nameof(_Options.AccessToken));
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

            HttpClient client = new HttpClient();
            client.BaseAddress = baseUrl.Uri;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _Options.AccessToken))));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
            var builder = new UriBuilder(_Options.Organisation);
            builder.Path += _Options.Project + "/_apis/" + apiPathAttribute.Path + "/";

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
                builder.Query = "api-version=5.1-preview.1";
            }
            return builder;
        }

        /// <summary>
        /// Generic Method to get API Definitions (Taskgroups, Variablegroups, Build- or Release Pipelines)
        /// </summary>
        /// <typeparam name="DefinitionType">Type of Definition. Can be: Taskgroup, Build- or Release Pipeline</typeparam>
        /// <returns>List of API Definitions </returns>
        public async Task<IEnumerable<DefinitionType>> GetApiDefinitionsAsync<DefinitionType>()
            where DefinitionType : RestApiDefinition, new()
        {
            var initialDefinitions = new List<DefinitionType>();

            HttpClient client = GetHttpClient<DefinitionType>();
            
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
                        var response = await client.GetAsync(definition.Id);
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
        /// <typeparam name="DefinitionType"></typeparam>
        /// <param name="definitionsToBeMigrated"></param>
        /// <returns>List of Mappings</returns>
        public async Task<List<Mapping>> CreateApiDefinitionsAsync<DefinitionType>(IEnumerable<DefinitionType> definitionsToBeMigrated)
            where DefinitionType : RestApiDefinition, new()
        {
            var migratedDefinitions = new List<Mapping>();

            foreach (RestApiDefinition definitionToBeMigrated in definitionsToBeMigrated)
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
                var result = await client.PostAsync("", content);

                var responseContent = await result.Content.ReadAsStringAsync();
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    Log.LogError("Error migrating {DefinitionType} {DefinitionName}. Please migrate it manually. {ErrorText}", typeof(DefinitionType).Name, definitionToBeMigrated.Name, responseContent);
                    continue;
                }
                else
                {
                    var targetObject = JsonConvert.DeserializeObject<DefinitionType>(responseContent);
                    migratedDefinitions.Add(new Mapping()
                    {
                        Name = definitionToBeMigrated.Name,
                        SId = definitionToBeMigrated.Id,
                        TId = targetObject.Id
                    });

                }
            }
            Log.LogInformation("{MigratedCount} of {TriedToMigrate} {DefinitionType}(s) got migrated..", migratedDefinitions.Count, definitionsToBeMigrated.Count(), typeof(DefinitionType).Name);
            return migratedDefinitions;
        }
    }
}