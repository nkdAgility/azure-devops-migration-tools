using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Endpoints;

namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServicesForClientFileSystem(this IServiceCollection context, IConfiguration configuration)
        {
            context.AddConfiguredEndpoints(configuration);
            context.AddTransient<FileSystemWorkItemEndpoint>();
        }
    }
}