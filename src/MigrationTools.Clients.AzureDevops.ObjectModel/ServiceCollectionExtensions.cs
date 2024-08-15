using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Clients;
using MigrationTools._EngineV1.Containers;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.FieldMaps.AzureDevops.ObjectModel;
using MigrationTools.ProcessorEnrichers;
using MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers;
using MigrationTools.Processors;

namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServicesForClientAzureDevOpsObjectModel(this IServiceCollection context, IConfiguration configuration)
        {
            context.AddMigrationToolsEndPoints<TfsEndpointOptions, TfsEndpoint>(configuration, "TfsEndpoints");
            context.AddMigrationToolsEndPoints<TfsWorkItemEndpointOptions, TfsWorkItemEndpoint>(configuration, "TfsWorkItemEndpoints");
            context.AddMigrationToolsEndPoints<TfsTeamSettingsEndpointOptions, TfsTeamSettingsEndpoint>(configuration, "TfsTeamSettingsEndpoints");

            //Processors
            context.AddTransient<TfsTeamSettingsProcessor>();
            context.AddTransient<TfsSharedQueryProcessor>();
            context.AddTransient<TfsAreaAndIterationProcessor>();

            switch (configuration.GetMigrationConfigVersion())
            {
                case ConfigurationExtensions.MigrationConfigVersion.v15:


                    context.AddSingleton<TfsAttachmentEnricher>().AddSingleton<IOptions<TfsAttachmentEnricherOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsAttachmentEnricherOptions>(TfsAttachmentEnricherOptions.ConfigurationSectionName)));
                    context.AddSingleton<TfsUserMappingEnricher>().AddSingleton<IOptions<TfsUserMappingEnricherOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsUserMappingEnricherOptions>(TfsUserMappingEnricherOptions.ConfigurationSectionName)));
                    context.AddSingleton<TfsValidateRequiredField>().AddSingleton<IOptions<TfsValidateRequiredFieldOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsValidateRequiredFieldOptions>(TfsValidateRequiredFieldOptions.ConfigurationSectionName)));
                    context.AddSingleton<TfsWorkItemLinkEnricher>().AddSingleton<IOptions<TfsWorkItemLinkEnricherOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsWorkItemLinkEnricherOptions>(TfsWorkItemLinkEnricherOptions.ConfigurationSectionName)));
                    context.AddSingleton<TfsWorkItemEmbededLinkEnricher>().AddSingleton<IOptions<TfsWorkItemEmbededLinkEnricherOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsWorkItemEmbededLinkEnricherOptions>(TfsWorkItemEmbededLinkEnricherOptions.ConfigurationSectionName)));
                    context.AddSingleton<TfsEmbededImagesEnricher>().AddSingleton<IOptions<TfsEmbededImagesEnricherOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsEmbededImagesEnricherOptions>(TfsEmbededImagesEnricherOptions.ConfigurationSectionName)));
                    context.AddSingleton<TfsGitRepositoryEnricher>().AddSingleton<IOptions<TfsGitRepositoryEnricherOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsGitRepositoryEnricherOptions>(TfsGitRepositoryEnricherOptions.ConfigurationSectionName)));
                    context.AddSingleton<TfsNodeStructure>().AddSingleton<IOptions<TfsNodeStructureOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsNodeStructureOptions>(TfsNodeStructureOptions.ConfigurationSectionName)));
                    context.AddSingleton<TfsRevisionManager>().AddSingleton<IOptions<TfsRevisionManagerOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsRevisionManagerOptions>(TfsRevisionManagerOptions.ConfigurationSectionName)));
                    context.AddSingleton<TfsTeamSettingsEnricher>().AddSingleton<IOptions<TfsTeamSettingsEnricherOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<TfsTeamSettingsEnricherOptions>(TfsTeamSettingsEnricherOptions.ConfigurationSectionName)));
                    break;
                case ConfigurationExtensions.MigrationConfigVersion.v16:
                    context.AddSingleton<TfsAttachmentEnricher>().AddOptions<TfsAttachmentEnricherOptions>().Bind(configuration.GetSection(TfsAttachmentEnricherOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsUserMappingEnricher>().AddOptions<TfsUserMappingEnricherOptions>().Bind(configuration.GetSection(TfsUserMappingEnricherOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsValidateRequiredField>().AddOptions<TfsValidateRequiredFieldOptions>().Bind(configuration.GetSection(TfsValidateRequiredFieldOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsWorkItemLinkEnricher>().AddOptions<TfsWorkItemLinkEnricherOptions>().Bind(configuration.GetSection(TfsWorkItemLinkEnricherOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsWorkItemEmbededLinkEnricher>().AddOptions<TfsWorkItemEmbededLinkEnricherOptions>().Bind(configuration.GetSection(TfsWorkItemEmbededLinkEnricherOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsEmbededImagesEnricher>().AddOptions<TfsEmbededImagesEnricherOptions>().Bind(configuration.GetSection(TfsEmbededImagesEnricherOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsGitRepositoryEnricher>().AddOptions<TfsGitRepositoryEnricherOptions>().Bind(configuration.GetSection(TfsGitRepositoryEnricherOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsNodeStructure>().AddOptions<TfsNodeStructureOptions>().Bind(configuration.GetSection(TfsNodeStructureOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsRevisionManager>().AddOptions<TfsRevisionManagerOptions>().Bind(configuration.GetSection(TfsRevisionManagerOptions.ConfigurationSectionName));
                    context.AddSingleton<TfsTeamSettingsEnricher>().AddOptions<TfsTeamSettingsEnricherOptions>().Bind(configuration.GetSection(TfsTeamSettingsEnricherOptions.ConfigurationSectionName));
                    break;
            }
            context.AddSingleton<TfsStaticEnrichers>();

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
            context.AddTransient< FieldtoFieldMultiMap>();
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