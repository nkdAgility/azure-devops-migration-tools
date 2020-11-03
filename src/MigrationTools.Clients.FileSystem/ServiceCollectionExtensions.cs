using System;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Endpoints;

namespace MigrationTools
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServicesForFileSystem(this IServiceCollection context)
        {
            context.AddTransient<FileSystemWorkItemEndpoint>();
        }
    }
}