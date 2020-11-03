using System;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Endpoints;

namespace MigrationTools
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServicesForAzureDevOpsObjectModel(this IServiceCollection context)
        {
            context.AddTransient<TfsWorkItemEndPoint>();
        }

        [Obsolete("This is the v1 Archtiecture, we are movign to V2", false)]
        public static void AddMigrationToolLegacyServicesForAzureDevOpsObjectModel(this IServiceCollection context)
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
            services.AddSingleton<TfsWorkItemLinkEnricher>();
            services.AddSingleton<TfsEmbededImagesEnricher>();
            services.AddSingleton<TfsGitRepositoryEnricher>();
            services.AddSingleton<TfsNodeStructureEnricher>();
            // Core
            services.AddTransient<IMigrationClient, TfsMigrationClient>();
            services.AddTransient<IWorkItemMigrationClient, TfsWorkItemMigrationClient>();
            services.AddTransient<ITestPlanMigrationClient, TfsTestPlanMigrationClient>();
            services.AddTransient<IWorkItemQueryBuilder, WorkItemQueryBuilder>();
            services.AddTransient<IWorkItemQuery, TfsWorkItemQuery>();
        }
    }
}