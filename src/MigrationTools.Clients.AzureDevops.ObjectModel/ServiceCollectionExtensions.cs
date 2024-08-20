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
                    context.AddSingleton<TfsAttachmentTool>().AddSingleton<IOptions<TfsAttachmentToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsAttachmentToolOptions>(TfsAttachmentToolOptions.ConfigurationSectionName)));

                    context.AddSingleton<TfsUserMappingTool>().AddSingleton<IOptions<TfsUserMappingToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsUserMappingToolOptions>(TfsUserMappingToolOptions.ConfigurationSectionName)));

                    context.AddSingleton<TfsValidateRequiredFieldTool>().AddSingleton<IOptions<TfsValidateRequiredFieldToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsValidateRequiredFieldToolOptions>(TfsValidateRequiredFieldToolOptions.ConfigurationSectionName)));

                    context.AddSingleton<TfsWorkItemLinkTool>().AddSingleton<IOptions<TfsWorkItemLinkToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsWorkItemLinkToolOptions>(TfsWorkItemLinkToolOptions.ConfigurationSectionName)));

                    context.AddSingleton<TfsWorkItemEmbededLinkTool>().AddSingleton<IOptions<TfsWorkItemEmbededLinkToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsWorkItemEmbededLinkToolOptions>(TfsWorkItemEmbededLinkToolOptions.ConfigurationSectionName)));

                    context.AddSingleton<TfsEmbededImagesTool>().AddSingleton<IOptions<TfsEmbededImagesToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsEmbededImagesToolOptions>(TfsEmbededImagesToolOptions.ConfigurationSectionName)));

                    context.AddSingleton<TfsGitRepositoryTool>().AddSingleton<IOptions<TfsGitRepositoryToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsGitRepositoryToolOptions>(TfsGitRepositoryToolOptions.ConfigurationSectionName)));

                    context.AddSingleton<TfsNodeStructureTool>().AddSingleton<IOptions<TfsNodeStructureToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsNodeStructureToolOptions>(TfsNodeStructureToolOptions.ConfigurationSectionName)));

                    context.AddSingleton<TfsRevisionManagerTool>().AddSingleton<IOptions<TfsRevisionManagerToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsRevisionManagerToolOptions>(TfsRevisionManagerToolOptions.ConfigurationSectionName)));

                    context.AddSingleton<TfsTeamSettingsTool>().AddSingleton<IOptions<TfsTeamSettingsToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsTeamSettingsToolOptions>(TfsTeamSettingsToolOptions.ConfigurationSectionName)));

                    break;
                case MigrationConfigSchema.v160:
                    context.AddSingleton<TfsAttachmentTool>().AddOptions<TfsAttachmentToolOptions>().Bind(configuration.GetSection(TfsAttachmentToolOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsUserMappingTool>().AddOptions<TfsUserMappingToolOptions>().Bind(configuration.GetSection(TfsUserMappingToolOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsValidateRequiredFieldTool>().AddOptions<TfsValidateRequiredFieldToolOptions>().Bind(configuration.GetSection(TfsValidateRequiredFieldToolOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsWorkItemLinkTool>().AddOptions<TfsWorkItemLinkToolOptions>().Bind(configuration.GetSection(TfsWorkItemLinkToolOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsWorkItemEmbededLinkTool>().AddOptions<TfsWorkItemEmbededLinkToolOptions>().Bind(configuration.GetSection(TfsWorkItemEmbededLinkToolOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsEmbededImagesTool>().AddOptions<TfsEmbededImagesToolOptions>().Bind(configuration.GetSection(TfsEmbededImagesToolOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsGitRepositoryTool>().AddOptions<TfsGitRepositoryToolOptions>().Bind(configuration.GetSection(TfsGitRepositoryToolOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsNodeStructureTool>().AddOptions<TfsNodeStructureToolOptions>().Bind(configuration.GetSection(TfsNodeStructureToolOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsRevisionManagerTool>().AddOptions<TfsRevisionManagerToolOptions>().Bind(configuration.GetSection(TfsRevisionManagerToolOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsTeamSettingsTool>().AddOptions<TfsTeamSettingsToolOptions>().Bind(configuration.GetSection(TfsTeamSettingsToolOptions.ConfigurationSectionName));
                    break;
            }

            context.AddSingleton<TfsStaticTools>();
        }

        [Obsolete("This is the v1 Archtiecture, we are movign to V2", false)]
        public static void AddMigrationToolServicesForClientLegacyCore(this IServiceCollection context)
        {
            context.AddSingleton<WorkItemMigrationProcessor>();
            context.AddSingleton<TestConfigurationsMigrationProcessor>();
            context.AddSingleton<TestPlansAndSuitesMigrationProcessor>();
            context.AddSingleton<TestVariablesMigrationProcessor>();
            context.AddSingleton<WorkItemPostProcessingProcessor>();
            context.AddSingleton<WorkItemPostProcessingProcessor>();
            context.AddSingleton<ExportUsersForMappingProcessor>();
            context.AddSingleton<CreateTeamFolders>();
            context.AddSingleton<ExportProfilePictureFromADProcessor>();
            context.AddSingleton<ExportTeamListProcessor>();
            context.AddSingleton<FixGitCommitLinksProcessor>();
            context.AddSingleton<ImportProfilePictureProcessor>();
            context.AddSingleton<WorkItemDeleteProcessor>();
            context.AddSingleton<WorkItemBulkEditProcessor>();
            context.AddSingleton<WorkItemUpdateAreasAsTagsProcessor>();

        }


        public static void AddMigrationToolServicesForClientAzureDevOpsObjectModel(this IServiceCollection context, IConfiguration configuration)
        {
            context.AddMigrationToolsEndPoints<TfsEndpointOptions, TfsEndpoint>(configuration, "TfsEndpoints");
            context.AddMigrationToolsEndPoints<TfsWorkItemEndpointOptions, TfsWorkItemEndpoint>(configuration, "TfsWorkItemEndpoints");
            context.AddMigrationToolsEndPoints<TfsTeamSettingsEndpointOptions, TfsTeamSettingsEndpoint>(configuration, "TfsTeamSettingsEndpoints");

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
            context.AddTransient<IMigrationClient, TfsMigrationClient>();
            context.AddTransient<IWorkItemMigrationClient, TfsWorkItemMigrationClient>();
            context.AddTransient<ITestPlanMigrationClient, TfsTestPlanMigrationClient>();
            context.AddTransient<IWorkItemQueryBuilderFactory, WorkItemQueryBuilderFactory>();
            context.AddTransient<IWorkItemQueryBuilder, WorkItemQueryBuilder>();
            context.AddTransient<IWorkItemQuery, TfsWorkItemQuery>();
        }
    }
}