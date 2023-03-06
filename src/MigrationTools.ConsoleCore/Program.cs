using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Clients.AzureDevops.Rest.FieldMaps;
using MigrationTools.Host;

namespace MigrationTools.ConsoleCore
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = MigrationToolHost.CreateDefaultBuilder(args);
            if (hostBuilder is null)
            {
                return;
            }

            hostBuilder
                .ConfigureServices((context, services) =>
                {
                    // Field Mapps
                    services.AddTransient<FieldSkipMap>();
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
            await hostBuilder.RunMigrationTools(args);
        }
    }
}