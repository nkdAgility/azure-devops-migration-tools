using System;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools._EngineV1.Clients;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.FieldMaps.AzureDevops.ObjectModel;
using MigrationTools.ProcessorEnrichers;

namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServicesForClientAzureDevOpsObjectModel(this IServiceCollection context)
        {
            context.AddTransient<TfsWorkItemEndPoint>();
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
            // Enrichers
            context.AddSingleton<TfsValidateRequiredField>();
            context.AddSingleton<TfsWorkItemLinkEnricher>();
            context.AddSingleton<TfsEmbededImagesEnricher>();
            context.AddSingleton<TfsGitRepositoryEnricher>();
            context.AddSingleton<TfsNodeStructureEnricher>();
            // Core
            context.AddTransient<IMigrationClient, TfsMigrationClient>();
            context.AddTransient<IWorkItemMigrationClient, TfsWorkItemMigrationClient>();
            context.AddTransient<ITestPlanMigrationClient, TfsTestPlanMigrationClient>();
            context.AddTransient<IWorkItemQueryBuilder, WorkItemQueryBuilder>();
            context.AddTransient<IWorkItemQuery, TfsWorkItemQuery>();
        }
    }
}