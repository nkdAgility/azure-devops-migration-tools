using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MigrationTools.Engine;
using MigrationTools.Host;
using MigrationTools.Clients.AzureDevops.Rest;
using MigrationTools.Clients.AzureDevops.Rest.FieldMaps;
using MigrationTools;

namespace MigrationTools.ConsoleCore
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = MigrationToolHost.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Field Mapps
                    services.AddTransient<FieldBlankMap>();
                    services.AddTransient<FieldLiteralMap>();
                    services.AddTransient<FieldMergeMap>();
                    services.AddTransient<FieldToFieldMap>();
                    services.AddTransient<FieldtoFieldMultiMap>();
                    services.AddTransient<FieldToTagFieldMap>();
                    services.AddTransient<FieldValuetoTagMap>();
                    services.AddTransient<MultiValueConditionalMap>();
                    services.AddTransient<RegexFieldMap>();
                    services.AddTransient<TreeToTagFieldMap>();

                    // Processors

                    // Core
                    // services.AddTransient<IMigrationClient, MigrationRestClient>();

                });
            var host = hostBuilder.Build();
            var startupService = host.InitializeMigrationSetup(args);
            if (startupService == null)
            {
                return;
            }
            await host.RunAsync();
            startupService.RunExitLogic();
        }
    }
}
