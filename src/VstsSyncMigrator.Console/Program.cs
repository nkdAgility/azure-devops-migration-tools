using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MigrationTools;
using MigrationTools.Host;
using MigrationTools.Clients.AzureDevops.ObjectModel.FieldMaps;
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
                    services.AddSingleton<NodeStructuresMigrationContext>();
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

                    //Engine
                    services.AddSingleton<IMigrationEngine, MigrationEngine>();

                   
                });
                var host = hostBuilder.Build();
            var startupService = host.InitializeMigrationSetup(args);
            if(startupService == null)
            {
                return;
            }
            await host.RunAsync();
            startupService.RunExitLogic();
        }
    }
}
