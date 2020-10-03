using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Clients;
using MigrationTools.Clients.AzureDevops.ObjectModel.Clients;
using MigrationTools.Clients.AzureDevops.ObjectModel.Enrichers;
using MigrationTools.Clients.AzureDevops.ObjectModel.FieldMaps;
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
                    // Field Mapps
                    services.AddTransient<FieldBlankMap>();
                    services.AddTransient<FieldLiteralMap>();
                    services.AddTransient<FieldMergeMap>();
                    services.AddTransient<FieldToFieldMap>();
                    services.AddTransient<FieldtoFieldMultiMap>();
                    services.AddTransient<FieldToTagFieldMap>();
                    services.AddTransient<FieldValuetoTagMap>();
                    services.AddTransient<FieldToFieldMap>();
                    services.AddTransient<FieldValueMap>();
                    services.AddTransient<MultiValueConditionalMap>();
                    services.AddTransient<RegexFieldMap>();
                    services.AddTransient<TreeToTagFieldMap>();

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

                    // Enrichers
                    services.AddSingleton<WorkItemLinkEnricher>();
                    services.AddSingleton<EmbededImagesRepairEnricher>();
                    services.AddSingleton<GitRepositoryEnricher>();
                    services.AddSingleton<NodeStructureEnricher>();

                    // Core
                    services.AddTransient<IMigrationClient, MigrationClient>();
                    services.AddTransient<IWorkItemMigrationClient, WorkItemMigrationClient>();
                    services.AddTransient<ITestPlanMigrationClient, TestPlanMigrationClient>();
                    services.AddTransient<IWorkItemQueryBuilder, WorkItemQueryBuilder>();
                    services.AddTransient<IWorkItemQuery, WorkItemQuery>();
                });

            await hostBuilder.RunMigrationTools(args);
        }
    }
}