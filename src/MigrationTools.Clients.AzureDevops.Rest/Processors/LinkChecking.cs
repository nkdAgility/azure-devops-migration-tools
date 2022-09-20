using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts.WorkItems;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Processors;

namespace MigrationTools.Clients.AzureDevops.Rest.Processors
{
    internal class LinkChecking : Processor
    {
        private LinkCheckingOptions _Options;

        private Dictionary<string, int> _orgsAndProjects = new();

        public LinkChecking(
                    ProcessorEnricherContainer processorEnrichers,
                    IEndpointFactory endpointFactory,
                    IServiceProvider services,
                    ITelemetryLogger telemetry,
                    ILogger<Processor> logger)
            : base(processorEnrichers, endpointFactory, services, telemetry, logger)
        {
        }

        public new AzureDevOpsEndpoint Source => (AzureDevOpsEndpoint)base.Source;

        public new AzureDevOpsEndpoint Target => (AzureDevOpsEndpoint)base.Target;

        public override void Configure(IProcessorOptions options)
        {
            base.Configure(options);
            Log.LogInformation("AzureDevOpsPipelineProcessor::Configure");
            _Options = (LinkCheckingOptions)options;
        }

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
            if (_Options == null)
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
            var wiqlClient = Source.GetHttpClient<Wiql>();
            var query = new WiqlRequest
            {
                Query = "Select [System.Id], [System.Title], [System.State] From WorkItems Where [System.TeamProject] = @project and not [System.WorkItemType] contains 'Test Suite, Test Plan,Shared Steps,Shared Parameter,Feedback Request'"
            };

            var wiqlResult = await wiqlClient.PostAsJsonAsync("", query);

            if (wiqlResult.IsSuccessStatusCode != true)
            {
                return;
            }

            var workItems = await wiqlResult.Content.ReadAsAsync<WiqlResponse>();

            var client = Source.GetHttpClient<WorkItemBatchResult>();

            var chunks = workItems.WorkItems.Chunk(200);

            foreach (var chunk in chunks)
            {
                var batch = new WorkItemBatchRequest()
                {
                    Ids = chunk.Select(w => w.Id).ToList()
                };
                var items = await client.PostAsJsonAsync("", batch);
                if (items.StatusCode == System.Net.HttpStatusCode.OK)
                {
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

                            Log.LogInformation($"    Type: {relation.Attributes.Name}, url: {relation.Url}");
                            var value = $"{relationValues.org}{relationValues.project}{relation.Attributes.Name}";
                            if (_orgsAndProjects.ContainsKey(value) == false)
                            {
                                _orgsAndProjects.Add(value, 0);
                            }

                            _orgsAndProjects[value]++;

                        }
                    }
                }
            }

            Log.LogInformation("");
            Log.LogInformation("---------------------------");

            foreach (var value in _orgsAndProjects)
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
