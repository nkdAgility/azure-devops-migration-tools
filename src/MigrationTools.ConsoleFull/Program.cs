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

            //for (int i = 0; i < 120; i++)
            //{
            //    System.Console.WriteLine("Waiting " + i);
            //    System.Threading.Thread.Sleep(60 * 1000);
            //}

            await hostBuilder.RunMigrationTools(args);
        }
    }
}