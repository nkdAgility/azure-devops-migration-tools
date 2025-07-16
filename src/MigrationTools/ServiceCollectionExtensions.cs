﻿using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools
{
    public static partial class ServiceCollectionExtensions
    {
        public static OptionsBuilder<TOptions> AddMigrationToolsOptions<TOptions>(this IServiceCollection services, IConfiguration configuration) where TOptions : class
        {
            IOptions options = (IOptions)Activator.CreateInstance<TOptions>();
            return services.AddOptions<TOptions>().Bind(configuration.GetSection(options.ConfigurationMetadata.PathToInstance)).ValidateOnStart();
        }

        public static OptionsBuilder<TOptions> AddMigrationToolsOptions<TOptions, TOptionsValidator>(this IServiceCollection services, IConfiguration configuration) where TOptions : class, IOptions where TOptionsValidator : class, IValidateOptions<TOptions>, new()
        {
            IOptions options = (IOptions)Activator.CreateInstance<TOptions>();
            services.AddSingleton<IValidateOptions<TOptions>, TOptionsValidator>();
            return services.AddOptions<TOptions>().Bind(configuration.GetSection(options.ConfigurationMetadata.PathToInstance));
        }

        public static void AddMigrationToolServices(this IServiceCollection context, IConfiguration configuration, string configFile = "configuration.json")
        {
            // Infra
            context.AddSingleton<OptionsConfigurationBuilder>();
            context.AddSingleton<OptionsConfigurationUpgrader>();

            context.AddConfiguredEndpoints(configuration);
            //Containers
            context.AddTransient<ProcessorEnricherContainer>();
            context.AddTransient<EndpointEnricherContainer>();
            // Processors
            context.AddTransient<WorkItemTrackingProcessor>();
            // Endpoint Enrichers
            //context.AddTransient<AppendMigrationToolSignatureFooter>();
            //context.AddTransient<FilterWorkItemsThatAlreadyExistInTarget>();
            //context.AddTransient<SkipToFinalRevisedWorkItemType>();

            context.AddSingleton<IStringManipulatorTool, StringManipulatorTool>().AddMigrationToolsOptions<StringManipulatorToolOptions>(configuration);
            context.AddSingleton<IWorkItemTypeMappingTool, WorkItemTypeMappingTool>().AddMigrationToolsOptions<WorkItemTypeMappingToolOptions>(configuration);

            // context.AddSingleton<GitRepoMappingTool>().AddMigrationToolsOptions<GitRepoMappingToolOptions>(configuration);

            context.AddSingleton<ProcessorContainer>()
                .AddSingleton<IConfigureOptions<ProcessorContainerOptions>, ProcessorContainerOptions.ConfigureOptions>()
                ;
            //context.AddSingleton < IWritableOptions<ProcessorContainerOptions>>(sp => {
            //    return new WritableOptions<ProcessorContainerOptions>(sp.GetRequiredService<IOptionsMonitor<ProcessorContainerOptions>>(),ProcessorContainerOptions.ConfigurationSectionName, configFile);
            //});

            context.AddSingleton<IFieldMappingTool, FieldMappingTool>().AddSingleton<IConfigureOptions<FieldMappingToolOptions>, FieldMappingToolOptions.ConfigureOptions>();
            context.AddSingleton<VersionOptions>().AddSingleton<IConfigureOptions<VersionOptions>, VersionOptions.ConfigureOptions>();
            context.AddSingleton<CommonTools>();

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
