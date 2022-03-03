﻿using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools._EngineV1.Clients;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.FieldMaps.AzureDevops.ObjectModel;
using MigrationTools.ProcessorEnrichers;
using MigrationTools.Processors;

namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServicesForClientAzureDevOpsObjectModel(this IServiceCollection context, IConfiguration configuration)
        {
            context.AddEndPoints<TfsEndpointOptions, TfsEndpoint>(configuration, "TfsEndpoints");
            context.AddEndPoints<TfsWorkItemEndpointOptions, TfsWorkItemEndpoint>(configuration, "TfsWorkItemEndpoints");
            context.AddEndPoints<TfsTeamSettingsEndpointOptions, TfsTeamSettingsEndpoint>(configuration, "TfsTeamSettingsEndpoints");

            //Processors
            context.AddTransient<TfsTeamSettingsProcessor>();
            context.AddTransient<TfsSharedQueryProcessor>();
            context.AddTransient<TfsAreaAndIterationProcessor>();

            // Enrichers
            context.AddSingleton<TfsValidateRequiredField>();
            context.AddSingleton<TfsWorkItemLinkEnricher>();
            context.AddSingleton<TfsWorkItemEmbededLinkEnricher>();
            context.AddSingleton<TfsEmbededImagesEnricher>();
            context.AddSingleton<TfsGitRepositoryEnricher>();
            context.AddSingleton<TfsNodeStructure>();
            context.AddSingleton<TfsRevisionManager>();
            // EndPoint Enrichers
            context.AddTransient<TfsWorkItemAttachmentEnricher>();
        }

        [Obsolete("This is the v1 Archtiecture, we are movign to V2", false)]
        public static void AddMigrationToolServicesForClientLegacyAzureDevOpsObjectModel(this IServiceCollection context)
        {
            // Field Mapps
            context.AddTransient<FieldBlankMap>();
            context.AddTransient<FieldLiteralMap>();
            context.AddTransient<FieldMergeMap>();
            context.AddTransient<FieldToFieldMap>();
            context.AddTransient<FieldtoFieldMultiMap>();
            context.AddTransient<FieldToTagFieldMap>();
            context.AddTransient<FieldValuetoTagMap>();
            context.AddTransient<FieldToFieldMap>();
            context.AddTransient<FieldValueMap>();
            context.AddTransient<MultiValueConditionalMap>();
            context.AddTransient<RegexFieldMap>();
            context.AddTransient<TreeToTagFieldMap>();
            
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