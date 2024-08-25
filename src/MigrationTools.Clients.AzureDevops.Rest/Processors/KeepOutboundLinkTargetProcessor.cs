using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts.WorkItems;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using Newtonsoft.Json;
using MigrationTools.Processors.Infrastructure;
using Microsoft.Extensions.Options;
using MigrationTools.Tools;

namespace MigrationTools.Clients.AzureDevops.Rest.Processors
{
    internal class KeepOutboundLinkTargetProcessor : Processor
    {
        private KeepOutboundLinkTargetProcessorOptions _options;

        public KeepOutboundLinkTargetProcessor(IOptions<KeepOutboundLinkTargetProcessorOptions> options, CommonTools commonTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(options, commonTools, processorEnrichers, services, telemetry, logger)
        {
        }

        public new KeepOutboundLinkTargetProcessorOptions Options => (KeepOutboundLinkTargetProcessorOptions)base.Options;
        public new AzureDevOpsEndpoint Source => (AzureDevOpsEndpoint)base.Source;

        public new AzureDevOpsEndpoint Target => (AzureDevOpsEndpoint)base.Target;

        protected override void InternalExecute()
        {
            Log.LogInformation("Processor::InternalExecute::Start");
            EnsureConfigured();
            ProcessorEnrichers.ProcessorExecutionBegin(this);
            AddLinksToWorkItems().GetAwaiter().GetResult();
            ProcessorEnrichers.ProcessorExecutionEnd(this);
            Log.LogInformation("Processor::InternalExecute::End");
        }

        private void EnsureConfigured()
        {
            Log.LogInformation("Processor::EnsureConfigured");
            if (_options == null)
            {
                throw new Exception("You must call Configure() first");
            }
            if (Source is not AzureDevOpsEndpoint)
            {
                throw new Exception("The Source endpoint configured must be of type AzureDevOpsEndpoint");
            }
            if (Target is not AzureDevOpsEndpoint)
            {
                throw new Exception("The Target endpoint configured must be of type AzureDevOpsEndpoint");
            }
        }

        private async Task AddLinksToWorkItems()
        {
            var wiqlClient = Source.GetHttpClient("wit/wiql");
            WiqlResponse workItems = await GetWorkItemsBasedOnWiql(wiqlClient, _options.WIQLQuery);

            var client = Source.GetHttpClient("wit/workitemsbatch");

            var chunks = workItems.WorkItems.Chunk(200);
            var foundLinks = new List<string>();

            var targetClient = Target.GetHttpClient("wit/workitems/");

            var wiqlTargetClient = Target.GetHttpClient("wit/wiql");
            var uniqueRelationTargets = new HashSet<string>();
            foreach (var chunk in chunks)
            {
                var batch = new WorkItemBatchRequest()
                {
                    Ids = chunk.Select(w => w.Id).ToList()
                };
                var items = await client.PostAsJsonAsync("", batch);
                if (items.IsSuccessStatusCode != true)
                {
                    var content = await items.Content.ReadAsStringAsync();
                    Log.LogError($"Failed to get workitems from batch request {content}");
                    items.EnsureSuccessStatusCode();
                }

                var result = await items.Content.ReadAsAsync<WorkItemBatchResult>();
                foreach (var workitem in result.Value)
                {
                    if (workitem.Relations == null)
                    {
                        continue;
                    }
                    var workItemLocation = GetOrgAndProject(workitem.Url);
                    WorkItem targetItem = null;
                    var adds = new List<AddLink>();
                    foreach (var relation in workitem.Relations)
                    {
                        var relationValues = GetOrgAndProject(relation.Url);
                        if (relationValues.project != _options.TargetLinksToKeepProject)
                        {
                            continue;
                        }
                        if (targetItem is null)
                        {
                            Log.LogInformation("Source Workitem: " + workitem.Url);
                            targetItem = await GetReflectedWorkItem(wiqlTargetClient, targetClient, workitem.Id);
                        }

                        var linkToAdd = relation.Rel;
                        if (relation.Rel is "System.LinkTypes.Remote.Dependency-Reverse")
                        {
                            linkToAdd = "System.LinkTypes.Dependency-Reverse";
                        }
                        var linkToWorkitem = $"{relationValues.org}/{relationValues.project}_workitems/edit/{relationValues.targetId}";
                        uniqueRelationTargets.Add(linkToWorkitem);
                        var link = $"{workitem.Url},{linkToAdd},{relation.Url}";
                        Log.LogInformation($"Adding {link}");
                        adds.Add(
                            new AddLink
                            {
                                Value = new Relation
                                {
                                    Rel = linkToAdd,
                                    Url = relation.Url,
                                    Attributes = new Attributes
                                    {
                                        Comment = "Created from migration tool"
                                    }
                                }
                            });
                    }
                    if (adds.Count == 0)
                    {
                        continue;
                    }

                    if (_options.DryRun)
                    {
                        continue;
                    }

                    var serializedDoc = JsonConvert.SerializeObject(adds);
                    var requestContent = new StringContent(serializedDoc, Encoding.UTF8, "application/json-patch+json");
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), targetItem.Url);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", Target.Options.AccessToken))));
                    request.Headers.Add("Accept", $"application/json; api-version=6.0");
                    request.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    request.Content = requestContent;
                    var response = await targetClient.SendAsync(request);
                    if (response.IsSuccessStatusCode != true)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        Log.LogError($"Failed to add link to target workitem {content}");
                        response.EnsureSuccessStatusCode();
                    }
                }
            }

            var fileInfo = new FileInfo(_options.CleanupFileName);
            if (fileInfo.Directory.Exists == false)
            {
                fileInfo.Directory.Create();
            }

            using StreamWriter file = new(fileInfo.FullName);
            foreach (var target in uniqueRelationTargets)
            {
                file.WriteLine($"{_options.PrependCommand} {target}");
            }
        }

        private async Task<WiqlResponse> GetWorkItemsBasedOnWiql(HttpClient wiqlClient, string wiql)
        {
            var query = new WiqlRequest
            {
                Query = wiql
            };

            var wiqlResult = await wiqlClient.PostAsJsonAsync("", query);

            if (wiqlResult.IsSuccessStatusCode != true)
            {
                var content = await wiqlResult.Content.ReadAsStringAsync();
                Log.LogError($"Failed to get workitemids based on wiql query {content}");
                wiqlResult.EnsureSuccessStatusCode();
            }

            var workItems = await wiqlResult.Content.ReadAsAsync<WiqlResponse>();
            return workItems;
        }

        private async Task<WorkItem> GetReflectedWorkItem(HttpClient wiqlTarget, HttpClient workItemTarget, int sourceWorkItem)
        {
            var refid = string.Format("{0}/{1}/_workitems/edit/{2}", Source.Options.Organisation.TrimEnd('/'), Source.Options.Project, sourceWorkItem.ToString());
            var reflectedWiql = $"Select [System.Id] From WorkItems Where [System.TeamProject] = \"{Target.Options.Project}\" AND [{Target.Options.ReflectedWorkItemIdField}] = \"{refid}\"";
            var wiqlResponse = await GetWorkItemsBasedOnWiql(wiqlTarget, reflectedWiql);
            if (wiqlResponse.WorkItems.Length is not 1)
            {
                return null;
            }

            var workitemResult = await workItemTarget.GetAsync($"{wiqlResponse.WorkItems[0].Url}?$expand=relations");
            workitemResult.EnsureSuccessStatusCode();

            return await workitemResult.Content.ReadAsAsync<WorkItem>();
        }

        private (string org, string project, string targetId) GetOrgAndProject(string url)
        {
            var uri = new Uri(url);
            var baseUrl = uri.Host;
            var org = $"https://{baseUrl}";
            var segments = uri.Segments;

            string project;
            if (baseUrl == "dev.azure.com")
            {
                org = $"https://dev.azure.com/{segments[1]}";
                project = segments[2];
            }
            else
            {
                project = segments[1];
            }
            return (org, project, segments[segments.Length - 1]);
        }
    }
}
