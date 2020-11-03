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
                    services.AddSingleton<TestVariablesMigrationContext>();
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
                    services.AddSingleton<TfsWorkItemLinkEnricher>();
                    services.AddSingleton<TfsEmbededImagesEnricher>();
                    services.AddSingleton<TfsGitRepositoryEnricher>();
                    services.AddSingleton<TfsNodeStructureEnricher>();

                    // Core
                    services.AddTransient<IMigrationClient, TfsMigrationClient>();
                    services.AddTransient<IWorkItemMigrationClient, TfsWorkItemMigrationClient>();
                    services.AddTransient<ITestPlanMigrationClient, TfsTestPlanMigrationClient>();
                    services.AddTransient<IWorkItemQueryBuilder, WorkItemQueryBuilder>();
                    services.AddTransient<IWorkItemQuery, TfsWorkItemQuery>();
                });

            await hostBuilder.RunMigrationTools(args);
        }
    }
}