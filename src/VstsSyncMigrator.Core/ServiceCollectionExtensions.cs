using System;
using Microsoft.Extensions.DependencyInjection;
using VstsSyncMigrator.Engine;

namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        [Obsolete("This is the v1 Archtiecture, we are movign to V2", false)]
        public static void AddMigrationToolServicesForClientLegacyCore(this IServiceCollection context)
        {
            context.AddSingleton<WorkItemMigrationContext>();
            context.AddSingleton<TeamMigrationContext>();
            context.AddSingleton<TestConfigurationsMigrationContext>();
            context.AddSingleton<TestPlandsAndSuitesMigrationContext>();
            context.AddSingleton<TestVeriablesMigrationContext>();
            context.AddSingleton<WorkItemPostProcessingContext>();
            context.AddSingleton<WorkItemQueryMigrationContext>();
            context.AddSingleton<CreateTeamFolders>();
            context.AddSingleton<ExportProfilePictureFromADContext>();
            context.AddSingleton<ExportTeamList>();
            context.AddSingleton<FixGitCommitLinks>();
            context.AddSingleton<ImportProfilePictureContext>();
            context.AddSingleton<WorkItemDelete>();
            context.AddSingleton<WorkItemUpdate>();
            context.AddSingleton<WorkItemUpdateAreasAsTagsContext>();
        }
    }
}