using System;
using Microsoft.Extensions.DependencyInjection;
using VstsSyncMigrator.Engine;
using MigrationTools.Processors;


namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        [Obsolete("This is the v1 Archtiecture, we are movign to V2", false)]
        public static void AddMigrationToolServicesForClientLegacyCore(this IServiceCollection context)
        {
            context.AddSingleton<WorkItemMigrationContext>();
            context.AddSingleton<TestConfigurationsMigrationContext>();
            context.AddSingleton<TestPlansAndSuitesMigrationContext>();
            context.AddSingleton<TestVariablesMigrationContext>();
            context.AddSingleton<WorkItemPostProcessingContext>();
            context.AddSingleton<WorkItemPostProcessingContext>();
            context.AddSingleton<ExportUsersForMappingContext>();
            context.AddSingleton<CreateTeamFolders>();
            context.AddSingleton<ExportProfilePictureFromADContext>();
            context.AddSingleton<ExportTeamList>();
            context.AddSingleton<FixGitCommitLinks>();
            context.AddSingleton<ImportProfilePictureContext>();
            context.AddSingleton<WorkItemDelete>();
            context.AddSingleton<WorkItemBulkEditProcessor>();
            context.AddSingleton<WorkItemUpdateAreasAsTagsContext>();

        }
    }
}