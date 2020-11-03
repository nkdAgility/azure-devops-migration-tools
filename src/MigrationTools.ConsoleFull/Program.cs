using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools;
using MigrationTools.Clients;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.FieldMaps.AzureDevops.ObjectModel;
using MigrationTools.Host;
using VstsSyncMigrator.Engine;

namespace VstsSyncMigrator.ConsoleApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = MigrationToolHost.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // New v2 Architecture fpr testing
                    services.AddMigrationToolServicesForFileSystem();
                    services.AddMigrationToolServicesForAzureDevOpsObjectModel();

                    // v1 Architecture
                    services.AddMigrationToolLegacyServicesForAzureDevOpsObjectModel();

                    //Processors
                    services.AddSingleton<WorkItemMigrationContext>();
                    services.AddSingleton<TeamMigrationContext>();
                    services.AddSingleton<TestConfigurationsMigrationContext>();
                    services.AddSingleton<TestPlandsAndSuitesMigrationContext>();
                    services.AddSingleton<TestVeriablesMigrationContext>();
                    services.AddSingleton<WorkItemPostProcessingContext>();
                    services.AddSingleton<WorkItemQueryMigrationContext>();
                    services.AddSingleton<CreateTeamFolders>();
                    services.AddSingleton<ExportProfilePictureFromADContext>();
                    services.AddSingleton<ExportTeamList>();
                    services.AddSingleton<FixGitCommitLinks>();
                    services.AddSingleton<ImportProfilePictureContext>();
                    services.AddSingleton<WorkItemDelete>();
                    services.AddSingleton<WorkItemUpdate>();
                    services.AddSingleton<WorkItemUpdateAreasAsTagsContext>();
                });

            await hostBuilder.RunMigrationTools(args);
        }
    }
}