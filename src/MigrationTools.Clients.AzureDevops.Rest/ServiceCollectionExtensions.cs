using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Endpoints;
using MigrationTools.Processors;

namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServicesForClientAzureDevopsRest(this IServiceCollection context)
        {
            // Endpoint
            context.AddTransient<AzureDevOpsEndpoint>();
            //TfsPipelines
            context.AddTransient<AzureDevOpsPipelineProcessor>();
        }
    }
}