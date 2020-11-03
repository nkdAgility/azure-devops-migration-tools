using System;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Configuration;
using MigrationTools.Engine.Containers;
using MigrationTools.Enrichers;
using MigrationTools.Processors;

namespace MigrationTools
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServices(this IServiceCollection context)
        {
            AddMigrationToolEndpointEnrichers(context);
            AddMigrationToolProcessorEnrichers(context);
            AddMigrationToolProcessors(context);
        }

        public static void AddMigrationToolProcessors(this IServiceCollection context)
        {
            context.AddTransient<WorkItemMigrationProcessor>();
        }

        public static void AddMigrationToolEndpointEnrichers(this IServiceCollection context)
        {
            context.AddTransient<AppendMigrationToolSignatureFooter>();
            context.AddTransient<FilterWorkItemsThatAlreadyExistInTarget>();
            context.AddTransient<SkipToFinalRevisedWorkItemType>();
            //Following are Abstract
            context.AddTransient<WorkItemAttachmentEnricher>();
            context.AddTransient<WorkItemCreatedEnricher>();
            context.AddTransient<WorkItemEmbedEnricher>();
            context.AddTransient<WorkItemFieldTableEnricher>();
            context.AddTransient<WorkItemLinkEnricher>();
        }

        public static void AddMigrationToolProcessorEnrichers(this IServiceCollection context)
        {
            context.AddTransient<WorkItemAttachmentEnricher>();
            context.AddTransient<PauseAfterEachItem>();
        }

        [Obsolete("This is the v1 Archtiecture, we are movign to V2", false)]
        public static void AddMigrationToolLegacyServices(this IServiceCollection context)
        {
            // Services
            context.AddSingleton<ITelemetryLogger, TelemetryClientAdapter>();

            // Config
            context.AddSingleton<IEngineConfigurationBuilder, EngineConfigurationBuilder>();
            context.AddSingleton<EngineConfiguration, EngineConfigurationWrapper>();

            //Engine
            context.AddSingleton<FieldMapContainer>();
            context.AddSingleton<ProcessorContainer>();
            context.AddSingleton<TypeDefinitionMapContainer>();
            context.AddSingleton<GitRepoMapContainer>();
            context.AddSingleton<ChangeSetMappingContainer>();
            context.AddSingleton<IMigrationEngine, MigrationEngine>();
        }
    }
}