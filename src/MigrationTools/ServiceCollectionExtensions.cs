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
using MigrationTools.Options;
using MigrationTools.ProcessorEnrichers;
using MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers;
using MigrationTools.Processors;
using Serilog;

namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServices(this IServiceCollection context, IConfiguration configuration, string configFile = "configuration.json")
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



            switch (VersionOptions.ConfigureOptions.GetMigrationConfigVersion(configuration))
            {
                case MigrationConfigSchema.v1:
                    context.AddSingleton<StringManipulatorEnricher>().AddSingleton<IOptions<StringManipulatorEnricherOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<StringManipulatorEnricherOptions>(StringManipulatorEnricherOptions.ConfigurationSectionName)));
                    context.AddSingleton<WorkItemTypeMappingEnricher>().AddSingleton<IOptions<WorkItemTypeMappingEnricherOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<WorkItemTypeMappingEnricherOptions>(WorkItemTypeMappingEnricherOptions.ConfigurationSectionName)));
                    context.AddSingleton< GitRepoMappingTool>().AddSingleton<IOptions<GitRepoMappingToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<GitRepoMappingToolOptions>(GitRepoMappingToolOptions.ConfigurationSectionName)));
                    break;
                case MigrationConfigSchema.v160:
                    context.AddSingleton<StringManipulatorEnricher>().AddOptions<StringManipulatorEnricherOptions>().Bind(configuration.GetSection(StringManipulatorEnricherOptions.ConfigurationSectionName));
                    context.AddSingleton<WorkItemTypeMappingEnricher>().AddOptions<WorkItemTypeMappingEnricherOptions>().Bind(configuration.GetSection(WorkItemTypeMappingEnricherOptions.ConfigurationSectionName));
                    context.AddSingleton<GitRepoMappingTool>().AddOptions<GitRepoMappingToolOptions>().Bind(configuration.GetSection(GitRepoMappingToolOptions.ConfigurationSectionName));
                    break;
            }
            context.AddSingleton<ProcessorContainer>()
                .AddSingleton<IConfigureOptions<ProcessorContainerOptions>, ProcessorContainerOptions.ConfigureOptions>()
                ;
            context.AddSingleton < IWritableOptions<ProcessorContainerOptions>>(sp => {
                return new WritableOptions<ProcessorContainerOptions>(sp.GetRequiredService<IOptionsMonitor<ProcessorContainerOptions>>(),ProcessorContainerOptions.ConfigurationSectionName, configFile);
            });


            context.AddSingleton<FieldMappingTool>().AddSingleton<IConfigureOptions<FieldMappingToolOptions>, FieldMappingToolOptions.ConfigureOptions>();
            context.AddSingleton<VersionOptions>().AddSingleton<IConfigureOptions<VersionOptions>, VersionOptions.ConfigureOptions>();
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

            //Engine
            context.AddSingleton<IMigrationEngine, MigrationEngine>();
        }
    }
}