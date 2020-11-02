using Microsoft.Extensions.DependencyInjection;

namespace MigrationTools.Processors
{
    public static class ProcessorExtensions
    {
        public static void AddMigrationToolProcessors(this IServiceCollection context)
        {
            context.AddTransient<WorkItemMigrationProcessor>();
        }
    }
}