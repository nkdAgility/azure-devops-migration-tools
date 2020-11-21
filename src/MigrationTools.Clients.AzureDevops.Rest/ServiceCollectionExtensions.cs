using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Processors;

namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServicesForClientAzureDevopsRest(this IServiceCollection context)
        {
            //TfsPipelines
            context.AddTransient<TfsPipelineProcessor>();
        }
    }
}