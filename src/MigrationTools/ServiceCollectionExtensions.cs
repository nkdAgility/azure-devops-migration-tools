using System;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools._EngineV1.Containers;
using MigrationTools.Configuration;
using MigrationTools.EndPoints;
using MigrationTools.Enrichers;
using MigrationTools.Processors;

namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServices(this IServiceCollection context)
        {
            //Containers
            context.AddTransient<EndpointContainer>();
            context.AddTransient<ProcessorEnricherContainer>();
            context.AddTransient<EndpointEnricherContainer>();
            // Processors
            context.AddTransient<WorkItemMigrationProcessor>();
            // Endpoint Enrichers
            context.AddTransient<AppendMigrationToolSignatureFooter>();
            context.AddTransient<FilterWorkItemsThatAlreadyExistInTarget>();
            context.AddTransient<SkipToFinalRevisedWorkItemType>();
            // WorkItem Endpoint Enrichers
            context.AddTransient<WorkItemAttachmentEnricher>();
            context.AddTransient<WorkItemCreatedEnricher>();
            context.AddTransient<WorkItemEmbedEnricher>();
            context.AddTransient<WorkItemFieldTableEnricher>();
            context.AddTransient<WorkItemLinkEnricher>();
            // processor Enrichers
            context.AddTransient<WorkItemAttachmentEnricher>();
            context.AddTransient<PauseAfterEachItem>();
        }

        [Obsolete("This is the v1 Archtiecture, we are movign to V2", false)]
        public static void AddMigrationToolServicesLegacy(this IServiceCollection context)
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