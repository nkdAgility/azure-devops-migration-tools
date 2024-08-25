using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Clients;
using MigrationTools._EngineV1.Containers;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.FieldMaps.AzureDevops.ObjectModel;
using MigrationTools.Options;
using MigrationTools.Processors;
using MigrationTools.Tools;
using Serilog;

namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServicesForClientTfs_Tools(this IServiceCollection context, IConfiguration configuration)
        {
            switch (VersionOptions.ConfigureOptions.GetMigrationConfigVersion(configuration).schema)
            {
                case MigrationConfigSchema.v1:



                    context.AddSingleton<TfsAttachmentTool>().AddSingleton<IOptions<TfsAttachmentToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsAttachmentToolOptions>()));

                    context.AddSingleton<TfsUserMappingTool>().AddSingleton<IOptions<TfsUserMappingToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsUserMappingToolOptions>()));

                    context.AddSingleton<TfsValidateRequiredFieldTool>().AddSingleton<IOptions<TfsValidateRequiredFieldToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsValidateRequiredFieldToolOptions>()));

                    context.AddSingleton<TfsWorkItemLinkTool>().AddSingleton<IOptions<TfsWorkItemLinkToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsWorkItemLinkToolOptions>()));

                    context.AddSingleton<TfsWorkItemEmbededLinkTool>().AddSingleton<IOptions<TfsWorkItemEmbededLinkToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsWorkItemEmbededLinkToolOptions>()));

                    context.AddSingleton<TfsEmbededImagesTool>().AddSingleton<IOptions<TfsEmbededImagesToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsEmbededImagesToolOptions>()));

                    context.AddSingleton<TfsGitRepositoryTool>().AddSingleton<IOptions<TfsGitRepositoryToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsGitRepositoryToolOptions>()));

                    context.AddSingleton<TfsNodeStructureTool>().AddSingleton<IOptions<TfsNodeStructureToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsNodeStructureToolOptions>()));

                    context.AddSingleton<TfsRevisionManagerTool>().AddSingleton<IOptions<TfsRevisionManagerToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsRevisionManagerToolOptions>()));

                    context.AddSingleton<TfsTeamSettingsTool>().AddSingleton<IOptions<TfsTeamSettingsToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsTeamSettingsToolOptions>()));

                    break;
                case MigrationConfigSchema.v160:
                    context.AddConfiguredEndpoints(configuration);
                    context.AddSingleton<TfsAttachmentTool>().AddMigrationToolsOptions<TfsAttachmentToolOptions>(configuration);
                    context.AddSingleton<TfsUserMappingTool>().AddMigrationToolsOptions<TfsUserMappingToolOptions>(configuration);
                    context.AddSingleton<TfsValidateRequiredFieldTool>().AddMigrationToolsOptions<TfsValidateRequiredFieldToolOptions>(configuration);
                    context.AddSingleton<TfsWorkItemLinkTool>().AddMigrationToolsOptions<TfsWorkItemLinkToolOptions>(configuration);
                    context.AddSingleton<TfsWorkItemEmbededLinkTool>().AddMigrationToolsOptions<TfsWorkItemEmbededLinkToolOptions>(configuration);
                    context.AddSingleton<TfsEmbededImagesTool>().AddMigrationToolsOptions<TfsEmbededImagesToolOptions>(configuration);
                    context.AddSingleton<TfsGitRepositoryTool>().AddMigrationToolsOptions<TfsGitRepositoryToolOptions>(configuration);
                    context.AddSingleton<TfsNodeStructureTool>().AddMigrationToolsOptions<TfsNodeStructureToolOptions>(configuration);
                    context.AddSingleton<TfsRevisionManagerTool>().AddMigrationToolsOptions<TfsRevisionManagerToolOptions>(configuration);
                    context.AddSingleton<TfsTeamSettingsTool>().AddMigrationToolsOptions<TfsTeamSettingsToolOptions>(configuration);
                    break;
            }

            context.AddSingleton<TfsStaticTools>();
            
        }

        [Obsolete("This is the v1 Archtiecture, we are movign to V2", false)]
        public static void AddMigrationToolServicesForClientLegacyCore(this IServiceCollection context)
        {
            context.AddSingleton<TfsWorkItemMigrationProcessor>();
            context.AddSingleton<TestConfigurationsMigrationProcessor>();
            context.AddSingleton<TestPlansAndSuitesMigrationProcessor>();
            context.AddSingleton<TestVariablesMigrationProcessor>();
            context.AddSingleton<WorkItemPostProcessingProcessor>();
            context.AddSingleton<ExportUsersForMappingProcessor>();
            context.AddSingleton<CreateTeamFolders>();
            context.AddSingleton<ExportProfilePictureFromADProcessor>();
            context.AddSingleton<ExportTeamListProcessor>();
            context.AddSingleton<ImportProfilePictureProcessor>();
            context.AddSingleton<WorkItemDeleteProcessor>();
            context.AddSingleton<WorkItemBulkEditProcessor>();
            context.AddSingleton<WorkItemUpdateAreasAsTagsProcessor>();

        }


        public static void AddMigrationToolServicesForClientAzureDevOpsObjectModel(this IServiceCollection context, IConfiguration configuration)
        {
            //context.AddMigrationToolsEndPoints<TfsEndpointOptions, TfsEndpoint>(configuration, "TfsEndpoints");
            //context.AddMigrationToolsEndPoints<TfsWorkItemEndpointOptions, TfsWorkItemEndpoint>(configuration, "TfsWorkItemEndpoints");
            //context.AddMigrationToolsEndPoints<TfsTeamSettingsEndpointOptions, TfsTeamSettingsEndpoint>(configuration, "TfsTeamSettingsEndpoints");

            //Processors
            context.AddTransient<TfsTeamSettingsProcessor>();
            context.AddTransient<TfsSharedQueryProcessor>();

            context.AddMigrationToolServicesForClientTfs_Tools(configuration);

            // EndPoint Enrichers
            // context.AddTransient<TfsWorkItemAttachmentEnricher>().AddOptions<TfsWorkItemAttachmentEnricherOptions>().Bind(configuration.GetSection(TfsWorkItemAttachmentEnricherOptions.ConfigurationSectionName));
        }

        [Obsolete("This is the v1 Archtiecture, we are movign to V2", false)]
        public static void AddMigrationToolServicesForClientLegacyAzureDevOpsObjectModel(this IServiceCollection context)
        {
            // Field Mapps
            context.AddTransient< FieldSkipMap>();
            context.AddTransient< FieldLiteralMap>();
            context.AddTransient< FieldMergeMap>();
            context.AddTransient< FieldToFieldMap>();
            context.AddTransient< FieldToFieldMultiMap>();
            context.AddTransient< FieldToTagFieldMap>();
            context.AddTransient< FieldValuetoTagMap>();
            context.AddTransient< FieldToFieldMap>();
            context.AddTransient< FieldValueMap>();
            context.AddTransient< MultiValueConditionalMap>();
            context.AddTransient< RegexFieldMap>();
            context.AddTransient< TreeToTagFieldMap>();
            
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