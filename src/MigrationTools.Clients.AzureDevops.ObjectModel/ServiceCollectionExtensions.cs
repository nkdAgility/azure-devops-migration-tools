using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            // Enrichers
            context.AddSingleton<TfsAttachmentEnricher>();
            context.AddSingleton<TfsUserMappingEnricher>();
            context.AddSingleton<TfsValidateRequiredField>();
            context.AddSingleton<TfsWorkItemLinkEnricher>();
            context.AddSingleton<TfsWorkItemEmbededLinkEnricher>();
            context.AddSingleton<TfsEmbededImagesEnricher>();
            context.AddSingleton<TfsGitRepositoryEnricher>();
            context.AddSingleton<TfsUserMappingEnricher>();
            context.AddSingleton<TfsNodeStructure>();
            context.AddOptions<TfsNodeStructureOptions>().Bind(configuration.GetSection(TfsNodeStructureOptions.ConfigurationSectionName));


            context.AddSingleton<TfsRevisionManager>();
            context.AddSingleton<TfsTeamSettingsEnricher>();
            // EndPoint Enrichers
            context.AddTransient<TfsWorkItemAttachmentEnricher>();
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