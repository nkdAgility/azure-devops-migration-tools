using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using MigrationTools.Clients.AzureDevops.Rest.Processors;
using MigrationTools.Endpoints;
using MigrationTools.Processors;

namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServicesForClientAzureDevopsRest(this IServiceCollection context, IConfiguration configuration)
        {
            //TfsPipelines
            context.AddTransient<AzureDevOpsPipelineProcessor>();
            context.AddTransient<ProcessDefinitionProcessor>();

            context.AddTransient<OutboundLinkCheckingProcessor>();
            context.AddTransient<KeepOutboundLinkTargetProcessor>();

        }
    }
}