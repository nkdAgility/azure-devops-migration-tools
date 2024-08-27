using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tests;
using MigrationTools.Processors.Infrastructure;
using Microsoft.Extensions.Options;
using MigrationTools.Enrichers;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Tools;
using MigrationTools.Tools.Interfaces;
using MigrationTools.Tools.Shadows;
using MigrationTools.Shadows;
using MigrationTools.Endpoints;
using MigrationTools._EngineV1.Clients;
using MigrationTools.Endpoints.Infrastructure;
using System;

namespace MigrationTools.Processors.Tests
{
    public class TfsProcessorTests
    {
        [Obsolete]
        protected ServiceProvider Services = ServiceProviderHelper.GetServices();

        [TestInitialize]
        public void Setup()
        {
        }

        [Obsolete]
        protected static TfsTeamSettingsProcessorOptions GetTfsTeamSettingsProcessorOptions()
        {
            // Tfs To Tfs
            var migrationConfig = new TfsTeamSettingsProcessorOptions()
            {
                Enabled = true,
                MigrateTeamSettings = true,
                UpdateTeamSettings = true,
                PrefixProjectToNodes = false,
                SourceName = "TfsTeamSettingsSource",
                TargetName = "TfsTeamSettingsTarget"
            };
            return migrationConfig;
        }

        [Obsolete]
        protected static TfsSharedQueryProcessorOptions GetTfsSharedQueryProcessorOptions()
        {
            // Tfs To Tfs
            var migrationConfig = new TfsSharedQueryProcessorOptions()
            {
                Enabled = true,
                PrefixProjectToNodes = false,
                SourceName = "Source",
                TargetName = "Target"
            };
            return migrationConfig;
        }

        protected TfsSharedQueryProcessor GetTfsSharedQueryProcessor(TfsSharedQueryProcessorOptions options = null)
        {
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();

            services.AddSingleton<ProcessorEnricherContainer>();
            services.AddSingleton<EndpointEnricherContainer>();
            services.AddSingleton<CommonTools>();
            services.AddSingleton<IFieldMappingTool, MockFieldMappingTool>();
            services.AddSingleton<IWorkItemTypeMappingTool, MockWorkItemTypeMappingTool>();
            services.AddSingleton<IStringManipulatorTool, StringManipulatorTool>();

            services.AddSingleton<TfsSharedQueryProcessor>();

            AddEndpoint(services, options != null ? options.SourceName : "Source", "migrationSource1");
            AddEndpoint(services, options != null ? options.SourceName : "Target", "migrationTarget1");
            services.Configure<TfsSharedQueryProcessorOptions>(o =>
            {
                o.Enabled = options != null ? options.Enabled : true;
                o.SourceName = options != null ? options.SourceName : "Source";
                o.TargetName = options != null ? options.SourceName : "Target";
                o.Enrichers = options != null ? options.Enrichers : null;
                o.ProcessorEnrichers = options != null ? options.ProcessorEnrichers : null;
                o.RefName = options != null ? options.RefName : null;
                /// Add custom
                o.SourceToTargetFieldMappings = options != null ? options.SourceToTargetFieldMappings : null;
                o.PrefixProjectToNodes = options != null ? options.PrefixProjectToNodes : false;
                o.SharedFolderName = options != null ? options.SharedFolderName : "Shared Queries";
            });

            return services.BuildServiceProvider().GetService<TfsSharedQueryProcessor>();
        }

        private static void AddEndpoint(IServiceCollection services, string name, string project)
        {
            services.AddKeyedSingleton(typeof(IEndpoint), name, (sp, key) =>
            {
                var options = GetTfsTeamProjectEndpointOptions(project);
                var endpoint = ActivatorUtilities.CreateInstance(sp, typeof(TfsTeamProjectEndpoint), options);
                return endpoint;
            });
        }
        private static IOptions<TfsTeamProjectEndpointOptions> GetTfsTeamProjectEndpointOptions(string project)
        {
            IOptions<TfsTeamProjectEndpointOptions> options = Microsoft.Extensions.Options.Options.Create(new TfsTeamProjectEndpointOptions()
            {
                Collection = new System.Uri("https://dev.azure.com/nkdagility-preview/"),
                Project = "migrationSource1",
                Authentication = new TfsAuthenticationOptions()
                {
                 AuthenticationMode = AuthenticationMode.AccessToken,
                 AccessToken = TestingConstants.AccessToken
                },
                AllowCrossProjectLinking = false,
                LanguageMaps = new TfsLanguageMapOptions()
                {
                    AreaPath = "Area",
                    IterationPath = "Iteration"
                },
            });
            return options;
        }
    }
}