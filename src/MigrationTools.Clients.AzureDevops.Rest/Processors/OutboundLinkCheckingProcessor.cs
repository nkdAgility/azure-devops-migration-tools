using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.DataContracts.WorkItems;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools;

namespace MigrationTools.Clients.AzureDevops.Rest.Processors
{
    internal class OutboundLinkCheckingProcessor : Processor
    {
        private OutboundLinkCheckingProcessorOptions _options;

        public OutboundLinkCheckingProcessor(IOptions<ProcessorOptions> options, CommonTools commonTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(options, commonTools, processorEnrichers, services, telemetry, logger)
        {
        }

        public new OutboundLinkCheckingProcessorOptions Options => (OutboundLinkCheckingProcessorOptions)base.Options;

        public new AzureDevOpsEndpoint Source => (AzureDevOpsEndpoint)base.Source;

        public new AzureDevOpsEndpoint Target => (AzureDevOpsEndpoint)base.Target;


        protected override void InternalExecute()
        {
            Log.LogInformation("Processor::InternalExecute::Start");
            EnsureConfigured();
            ProcessorEnrichers.ProcessorExecutionBegin(this);
            FindAllOrgsAndProjects().GetAwaiter().GetResult();
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

        private async Task FindAllOrgsAndProjects()
        {
            var wiqlClient = Source.GetHttpClient("wit/wiql");
            var query = new WiqlRequest
            {
                Query = _options.WIQLQuery
            };

            var wiqlResult = await wiqlClient.PostAsJsonAsync("", query);

            if (wiqlResult.IsSuccessStatusCode != true)
            {
                var content = await wiqlResult.Content.ReadAsStringAsync();
                Log.LogError($"Failed to get workitemids based on wiql query {content}");
                wiqlResult.EnsureSuccessStatusCode();
            }

            var workItems = await wiqlResult.Content.ReadAsAsync<WiqlResponse>();

            var client = Source.GetHttpClient("wit/workitemsbatch");

            var chunks = workItems.WorkItems.Chunk(200);
            Dictionary<string, int> orgsAndProjects = new();
            var foundLinks = new List<string>();
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
                    bool logSource = true;
                    foreach (var relation in workitem.Relations)
                    {
                        var relationValues = GetOrgAndProject(relation.Url);
                        if (relationValues.project == workItemLocation.project)
                        {
                            continue;
                        }
                        if (logSource)
                        {
                            Log.LogInformation("Source Workitem: " + workitem.Url);
                            logSource = false;
                        }
                        var link = $"{workitem.Url},{relation.Attributes.Name},{relation.Url}";
                        foundLinks.Add(link);
                        Log.LogInformation($"    Type: {relation.Attributes.Name}, url: {relation.Url}");
                        var value = $"{relationValues.org}{relationValues.project}{relation.Attributes.Name}";
                        if (orgsAndProjects.ContainsKey(value) == false)
                        {
                            orgsAndProjects.Add(value, 0);
                        }

                        orgsAndProjects[value]++;

                    }
                }
            }
            var fileInfo = new FileInfo(_options.ResultFileName);
            if (fileInfo.Directory.Exists == false)
            {
                fileInfo.Directory.Create();
            }
            File.WriteAllLines(_options.ResultFileName, foundLinks);
            Log.LogInformation("");
            Log.LogInformation("---------------------------");

            foreach (var value in orgsAndProjects)
            {
                Log.LogInformation($"{value.Key}: {value.Value}");
            }
        }

        private (string org, string project) GetOrgAndProject(string url)
        {
            var uri = new Uri(url);
            var baseUrl = uri.Host;
            var org = baseUrl;
            var project = "project";
            var segments = uri.Segments;

            if (baseUrl == "dev.azure.com")
            {
                org = segments[1];
                project = segments[2];
            }
            else
            {
                project = segments[1];
            }
            return (org, project);
        }
    }

    public static class ListExtensions
    {
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }
    }
}
