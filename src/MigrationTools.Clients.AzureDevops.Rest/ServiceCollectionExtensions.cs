using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Clients.AzureDevops.Rest.Processors;
using MigrationTools.Endpoints;
using MigrationTools.Processors;

namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServicesForClientAzureDevopsRest(this IServiceCollection context, IConfiguration configuration)
        {
            context.AddEndPoints<AzureDevOpsEndpointOptions, AzureDevOpsEndpoint>(configuration, "AzureDevOpsEndpoints");

            //TfsPipelines
            context.AddTransient<AzureDevOpsPipelineProcessor>();
            context.AddTransient<ProcessDefinitionProcessor>();

            context.AddTransient<OutboundLinkCheckingProcessor>();

        }
    }
}