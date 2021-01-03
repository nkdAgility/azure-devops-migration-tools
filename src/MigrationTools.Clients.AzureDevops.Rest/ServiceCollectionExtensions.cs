using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Endpoints;
using MigrationTools.Processors;

namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServicesForClientAzureDevopsRest(this IServiceCollection context, IConfiguration configuration)
        {
            var endPoints = configuration.GetSection("Endpoints");
            var children = endPoints.GetChildren();
            foreach (var child in children)
            {
                var sections = child.GetChildren();
                if (sections.First(s => s.Key == "Type").Value == "AzureDevOpsEndpointOptions")
                {
                    var name = sections.First(s => s.Key == "Name").Value;
                    context.AddEndpoint(name, (provider) =>
                    {
                        var options = child.Get<AzureDevOpsEndpointOptions>();
                        var endpoint = provider.GetRequiredService<AzureDevOpsEndpoint>();
                        endpoint.Configure(options);
                        return endpoint;
                    });
                }
            }
            // Endpoint
            context.AddTransient<AzureDevOpsEndpoint>();
            //TfsPipelines
            context.AddTransient<AzureDevOpsPipelineProcessor>();
        }
    }
}