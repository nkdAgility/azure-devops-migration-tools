using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MigrationTools.Clients;
using MigrationTools.Endpoints;
using MigrationTools.FieldMaps.AzureDevops.ObjectModel;
using MigrationTools.Processors;
using MigrationTools.Tools;

namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServicesForClientTfs_Tools(this IServiceCollection context, IConfiguration configuration)
        {
            context.AddSingleton<TfsCommonTools>();
            context.AddSingleton<TfsAttachmentTool>().AddMigrationToolsOptions<TfsAttachmentToolOptions>(configuration);
            context.AddSingleton<TfsUserMappingTool>().AddMigrationToolsOptions<TfsUserMappingToolOptions>(configuration);
            context.AddSingleton<TfsValidateRequiredFieldTool>().AddMigrationToolsOptions<TfsValidateRequiredFieldToolOptions>(configuration);
            context.AddSingleton<TfsWorkItemLinkTool>().AddMigrationToolsOptions<TfsWorkItemLinkToolOptions>(configuration);
            context.AddSingleton<TfsWorkItemEmbededLinkTool>().AddMigrationToolsOptions<TfsWorkItemEmbededLinkToolOptions>(configuration);
            context.AddSingleton<TfsEmbededImagesTool>().AddMigrationToolsOptions<TfsEmbededImagesToolOptions>(configuration);
            context.AddSingleton<TfsGitRepositoryTool>().AddMigrationToolsOptions<TfsGitRepositoryToolOptions>(configuration);
            context.AddSingleton<TfsNodeStructureTool>().AddMigrationToolsOptions<TfsNodeStructureToolOptions, TfsNodeStructureToolOptionsValidator>(configuration);
            context.AddSingleton<TfsRevisionManagerTool>().AddMigrationToolsOptions<TfsRevisionManagerToolOptions>(configuration);
            context.AddSingleton<TfsTeamSettingsTool>().AddMigrationToolsOptions<TfsTeamSettingsToolOptions>(configuration);
            context.AddSingleton<TfsChangeSetMappingTool>().AddMigrationToolsOptions<TfsChangeSetMappingToolOptions>(configuration);
            context.AddSingleton<TfsWorkItemTypeValidatorTool>().AddMigrationToolsOptions<TfsWorkItemTypeValidatorToolOptions, TfsWorkItemTypeValidatorToolOptionsValidator>(configuration);
            context.PostConfigure<TfsWorkItemTypeValidatorToolOptions>(options => options.Normalize());
        }

        public static void AddMigrationToolServicesForClientTfs_Processors(this IServiceCollection context)
        {
            context.AddSingleton<TfsWorkItemMigrationProcessor>();
            context.AddTransient<IValidateOptions<TfsWorkItemMigrationProcessorOptions>, TfsWorkItemMigrationProcessorOptionsValidator>();

            context.AddSingleton<TfsTestConfigurationsMigrationProcessor>();
            context.AddSingleton<TfsTestPlansAndSuitesMigrationProcessor>();
            context.AddSingleton<TfsTestVariablesMigrationProcessor>();
            context.AddSingleton<TfsWorkItemOverwriteProcessor>();
            context.AddSingleton<TfsExportUsersForMappingProcessor>();
            context.AddSingleton<TfsCreateTeamFoldersProcessor>();
            context.AddSingleton<TfsExportProfilePictureFromADProcessor>();
            context.AddSingleton<TfsExportTeamListProcessor>();
            context.AddSingleton<TfsImportProfilePictureProcessor>();
            context.AddSingleton<TfsWorkItemDeleteProcessor>();
            context.AddSingleton<TfsWorkItemBulkEditProcessor>();
            context.AddSingleton<TfsWorkItemOverwriteAreasAsTagsProcessor>();

        }


        public static void AddMigrationToolServicesForClientAzureDevOpsObjectModel(this IServiceCollection context, IConfiguration configuration)
        {
            //Processors
            context.AddTransient<TfsTeamSettingsProcessor>();
            context.AddTransient<TfsSharedQueryProcessor>();

            context.AddConfiguredEndpoints(configuration);

            context.AddMigrationToolServicesForClientTfs_Tools(configuration);

            // EndPoint Enrichers
            // context.AddTransient<TfsWorkItemAttachmentEnricher>().AddOptions<TfsWorkItemAttachmentEnricherOptions>().Bind(configuration.GetSection(TfsWorkItemAttachmentEnricherOptions.ConfigurationSectionName));
        }

        [Obsolete("This is the v1 Archtiecture, we are movign to V2", false)]
        public static void AddMigrationToolServicesForClientLegacyAzureDevOpsObjectModel(this IServiceCollection context)
        {
            // Field Mapps
            context.AddTransient<FieldSkipMap>();
            context.AddTransient<FieldLiteralMap>();
            context.AddTransient<FieldMergeMap>();
            context.AddTransient<FieldToFieldMap>();
            context.AddTransient<FieldToFieldMultiMap>();
            context.AddTransient<FieldToTagFieldMap>();
            context.AddTransient<FieldValuetoTagMap>();
            context.AddTransient<FieldToFieldMap>();
            context.AddTransient<FieldValueMap>();
            context.AddTransient<MultiValueConditionalMap>();
            context.AddTransient<RegexFieldMap>();
            context.AddTransient<TreeToTagFieldMap>();
            context.AddTransient<FieldCalculationMap>();

            // Core
            context.AddTransient<IMigrationClient, TfsTeamProjectEndpoint>();
            context.AddTransient<IWorkItemMigrationClient, TfsWorkItemMigrationClient>();
            context.AddTransient<ITestPlanMigrationClient, TfsTestPlanMigrationClient>();
            context.AddTransient<IWorkItemQueryBuilderFactory, WorkItemQueryBuilderFactory>();
            context.AddTransient<IWorkItemQueryBuilder, WorkItemQueryBuilder>();
            context.AddTransient<IWorkItemQuery, TfsWorkItemQuery>();
        }
    }
}
