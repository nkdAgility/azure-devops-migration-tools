using System;
using System.Configuration;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools._EngineV1.Containers;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.ProcessorEnrichers;
using MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers;
using MigrationTools.Processors;
using MigrationTools.Services;

namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServices(this IServiceCollection context, IConfiguration configuration)
        {
            context.AddMigrationToolsEndpoint();
            //Containers
            context.AddTransient<ProcessorEnricherContainer>();
            context.AddTransient<EndpointEnricherContainer>();
            // Processors
            context.AddTransient<WorkItemTrackingProcessor>();
            // Endpoint Enrichers
            //context.AddTransient<AppendMigrationToolSignatureFooter>();
            //context.AddTransient<FilterWorkItemsThatAlreadyExistInTarget>();
            //context.AddTransient<SkipToFinalRevisedWorkItemType>();



            switch (configuration.GetMigrationConfigVersion())
            {
                case ConfigurationExtensions.MigrationConfigVersion.v15:
                    context.AddSingleton<StringManipulatorEnricher>().AddOptions<StringManipulatorEnricherOptions>().Bind(configuration.GetSectionCommonEnrichers_v15< StringManipulatorEnricherOptions>(StringManipulatorEnricherOptions.ConfigurationSectionName));
                    context.AddSingleton<WorkItemTypeMappingEnricher>().AddOptions<WorkItemTypeMappingEnricherOptions>().Bind(configuration.GetSectionCommonEnrichers_v15<WorkItemTypeMappingEnricherOptions>(WorkItemTypeMappingEnricherOptions.ConfigurationSectionName));
                    break;
                case ConfigurationExtensions.MigrationConfigVersion.v16:
                    context.AddSingleton<StringManipulatorEnricher>().AddOptions<StringManipulatorEnricherOptions>().Bind(configuration.GetSection(StringManipulatorEnricherOptions.ConfigurationSectionName));
                    context.AddSingleton<WorkItemTypeMappingEnricher>().AddOptions<WorkItemTypeMappingEnricherOptions>().Bind(configuration.GetSection(WorkItemTypeMappingEnricherOptions.ConfigurationSectionName));
                    break;
            }
            context.AddSingleton<StaticEnrichers>();


            //context.AddTransient<WorkItemAttachmentEnricher>();
            //context.AddTransient<WorkItemCreatedEnricher>();
            //context.AddTransient<WorkItemEmbedEnricher>();
            //context.AddTransient<WorkItemFieldTableEnricher>();
            //context.AddTransient<WorkItemLinkEnricher>();
            // processor Enrichers
            context.AddTransient<PauseAfterEachItem>();

            
        }

        [Obsolete("This is the v1 Archtiecture, we are movign to V2", false)]
        public static void AddMigrationToolServicesLegacy(this IServiceCollection context)
        {
            // Services
            context.AddSingleton<ITelemetryLogger, TelemetryClientAdapter>();



            // Containers
            context.AddSingleton<ProcessorContainer>();
            context.AddSingleton<GitRepoMapContainer>();
            context.AddSingleton<FieldMapContainer>();
            context.AddSingleton<ChangeSetMappingContainer>();
            //Engine
            context.AddSingleton<FieldMapContainer>();
            context.AddSingleton<ProcessorContainer>();
            context.AddSingleton<GitRepoMapContainer>();
            context.AddSingleton<ChangeSetMappingContainer>();
            context.AddSingleton<IMigrationEngine, MigrationEngine>();
        }
    }
}