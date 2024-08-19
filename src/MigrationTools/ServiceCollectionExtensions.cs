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
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools;
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



            switch (VersionOptions.ConfigureOptions.GetMigrationConfigVersion(configuration).schema)
            {
                case MigrationConfigSchema.v1:
                    context.AddSingleton<StringManipulatorTool>().AddSingleton<IOptions<StringManipulatorToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<StringManipulatorToolOptions>(StringManipulatorToolOptions.ConfigurationSectionName)));
                    context.AddSingleton<WorkItemTypeMappingTool>().AddSingleton<IOptions<WorkItemTypeMappingToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<WorkItemTypeMappingToolOptions>(WorkItemTypeMappingToolOptions.ConfigurationSectionName)));
                    context.AddSingleton< GitRepoMappingTool>().AddSingleton<IOptions<GitRepoMappingToolOptions>>(Microsoft.Extensions.Options.Options.Create(configuration.GetSectionCommonEnrichers_v15<GitRepoMappingToolOptions>(GitRepoMappingToolOptions.ConfigurationSectionName)));
                    break;
                case MigrationConfigSchema.v160:
                    context.AddSingleton<StringManipulatorTool>().AddOptions<StringManipulatorToolOptions>().Bind(configuration.GetSection(StringManipulatorToolOptions.ConfigurationSectionName));
                    context.AddSingleton<WorkItemTypeMappingTool>().AddOptions<WorkItemTypeMappingToolOptions>().Bind(configuration.GetSection(WorkItemTypeMappingToolOptions.ConfigurationSectionName));
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
            context.AddSingleton<StaticTools>();


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