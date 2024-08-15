using System;
using System.Configuration;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Containers;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.ProcessorEnrichers;
using MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers;
using MigrationTools.Processors;
using Serilog;

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
                case ConfigurationExtensions.MigrationConfigVersion.before16:
                    Console.WriteLine("!!ACTION REQUIRED!! You are using a deprecated version of the configuration, please update to v16. backward compatability will be removed in a future version.");
                    context.AddSingleton<StringManipulatorEnricher>().AddSingleton<IOptions<StringManipulatorEnricherOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<StringManipulatorEnricherOptions>(StringManipulatorEnricherOptions.ConfigurationSectionName)));
                    context.AddSingleton<WorkItemTypeMappingEnricher>().AddSingleton<IOptions<WorkItemTypeMappingEnricherOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<WorkItemTypeMappingEnricherOptions>(WorkItemTypeMappingEnricherOptions.ConfigurationSectionName)));
                    context.AddSingleton< GitRepoMappingTool>().AddSingleton<IOptions<GitRepoMappingToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<GitRepoMappingToolOptions>(GitRepoMappingToolOptions.ConfigurationSectionName)));
                    break;
                case ConfigurationExtensions.MigrationConfigVersion.v16:
                    context.AddSingleton<StringManipulatorEnricher>().AddOptions<StringManipulatorEnricherOptions>().Bind(configuration.GetSection(StringManipulatorEnricherOptions.ConfigurationSectionName));
                    context.AddSingleton<WorkItemTypeMappingEnricher>().AddOptions<WorkItemTypeMappingEnricherOptions>().Bind(configuration.GetSection(WorkItemTypeMappingEnricherOptions.ConfigurationSectionName));
                    context.AddSingleton<GitRepoMappingTool>().AddOptions<GitRepoMappingToolOptions>().Bind(configuration.GetSection(GitRepoMappingToolOptions.ConfigurationSectionName));
                    break;
            }
            context.AddSingleton<ProcessorContainer>().AddSingleton<IConfigureOptions<ProcessorContainerOptions>, ProcessorContainerOptions.ConfigureOptions>();
            context.AddSingleton<FieldMappingTool>().AddSingleton<IConfigureOptions<FieldMappingToolOptions>, FieldMappingToolOptions.ConfigureOptions>();
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
            //Engine
            context.AddSingleton<IMigrationEngine, MigrationEngine>();
        }
    }
}