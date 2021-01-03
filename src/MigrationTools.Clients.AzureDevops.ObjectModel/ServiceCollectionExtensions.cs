using System;
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
            var endPoints = configuration.GetSection("Endpoints");
            var children = endPoints.GetChildren();
            foreach (var child in children)
            {
                var sections = child.GetChildren();
                if (sections.First(s => s.Key == "Type").Value == "TfsEndpointOptions")
                {
                    var name = sections.First(s => s.Key == "Name").Value;
                    context.AddEndpoint(name, (provider) =>
                    {
                        var options = child.Get<TfsEndpointOptions>();
                        var endpoint = provider.GetRequiredService<TfsEndpoint>();
                        endpoint.Configure(options);
                        return endpoint;
                    });
                }
                else if (sections.First(s => s.Key == "Type").Value == "TfsWorkItemEndpointOptions")
                {
                    var name = sections.First(s => s.Key == "Name").Value;
                    context.AddEndpoint(name, (provider) =>
                    {
                        var options = child.Get<TfsWorkItemEndpointOptions>();
                        var endpoint = provider.GetRequiredService<TfsWorkItemEndpoint>();
                        endpoint.Configure(options);
                        return endpoint;
                    });
                }
                else if (sections.First(s => s.Key == "Type").Value == "TfsTeamSettingsEndpointOptions")
                {
                    var name = sections.First(s => s.Key == "Name").Value;
                    context.AddEndpoint(name, (provider) =>
                    {
                        var options = child.Get<TfsTeamSettingsEndpointOptions>();
                        var endpoint = provider.GetRequiredService<TfsTeamSettingsEndpoint>();
                        endpoint.Configure(options);
                        return endpoint;
                    });
                }
            }
            // Generic Endpoint
            context.AddTransient<TfsEndpoint>();
            //TfsWorkItem
            context.AddTransient<TfsWorkItemEndpoint>();
            //TfsTeamSettings
            context.AddTransient<TfsTeamSettingsEndpoint>();
            context.AddTransient<TfsTeamSettingsProcessor>();
            //TfsSharedQueries
            context.AddTransient<TfsSharedQueryProcessor>();
            //TfsAreaAndIterationProcessor
            context.AddTransient<TfsAreaAndIterationProcessor>();
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
            context.AddSingleton<TfsNodeStructure>();
            // EndPoint Enrichers
            context.AddTransient<TfsWorkItemAttachmentEnricher>();
            // Core
            context.AddTransient<IMigrationClient, TfsMigrationClient>();
            context.AddTransient<IWorkItemMigrationClient, TfsWorkItemMigrationClient>();
            context.AddTransient<ITestPlanMigrationClient, TfsTestPlanMigrationClient>();
            context.AddTransient<IWorkItemQueryBuilder, WorkItemQueryBuilder>();
            context.AddTransient<IWorkItemQuery, TfsWorkItemQuery>();
        }
    }
}