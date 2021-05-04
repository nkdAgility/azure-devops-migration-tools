using System.Threading.Tasks;
using MigrationTools;
using MigrationTools.Host;

namespace VstsSyncMigrator.ConsoleApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = MigrationToolHost.CreateDefaultBuilder(args);
            if(hostBuilder is null)
            {
                return;
            }

            hostBuilder
                .ConfigureServices((context, services) =>
                {
                    // New v2 Architecture fpr testing
                    services.AddMigrationToolServicesForClientFileSystem();
                    services.AddMigrationToolServicesForClientAzureDevOpsObjectModel(context.Configuration);
                    services.AddMigrationToolServicesForClientAzureDevopsRest(context.Configuration);

                    // v1 Architecture (Legacy)
                    services.AddMigrationToolServicesForClientLegacyAzureDevOpsObjectModel();
                    services.AddMigrationToolServicesForClientLegacyCore();
                });

            await hostBuilder.RunMigrationTools(args);
        }
    }
}