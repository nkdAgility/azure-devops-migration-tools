using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
                    services.AddSingleton<AzureDevOpsObjectModelWorkItemLinkEnricher>();
                    services.AddSingleton<AzureDevOpsObjectModelEmbededImagesRepairEnricher>();
                    services.AddSingleton<AzureDevOpsObjectModelGitRepositoryEnricher>();
                    services.AddSingleton<AzureDevOpsObjectModelNodeStructureEnricher>();

                    // Core
                    services.AddTransient<IMigrationClient, AzureDevOpsObjectModelMigrationClient>();
                    services.AddTransient<IWorkItemMigrationClient, AzureDevOpsObjectModelWorkItemMigrationClient>();
                    services.AddTransient<ITestPlanMigrationClient, AzureDevOpsObjectModelTestPlanMigrationClient>();
                    services.AddTransient<IWorkItemQueryBuilder, WorkItemQueryBuilder>();
                    services.AddTransient<IWorkItemQuery, AzureDevOpsObjectModelWorkItemQuery>();
                });

            await hostBuilder.RunMigrationTools(args);
        }
    }
}