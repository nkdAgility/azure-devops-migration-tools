using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;
using MigrationTools.Services;
using MigrationTools.Services.Shadows;
using MigrationTools.Shadows;

namespace MigrationTools.Tests
{
    public static class ServiceProviderHelper
    {
        public static ServiceCollection GetMigrationToolServicesForUnitTests()
        {
            var configuration = new ConfigurationBuilder().Build();
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();
            return services;
        }

        public static ServiceProvider GetServices()
        {
            var configuration = new ConfigurationBuilder().Build();
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();

            services.AddMigrationToolServices(configuration);
            services.AddMigrationToolServicesForClientAzureDevOpsObjectModel(configuration);
            services.AddMigrationToolServicesForClientLegacyAzureDevOpsObjectModel();
            services.AddMigrationToolServicesLegacy();
            AddTfsEndpoint(services, "Source", "migrationSource1");
            AddTfsEndpoint(services, "Target", "migrationTarget1");

            AddTfsTeamEndpoint(services, "TfsTeamSettingsSource", "migrationSource1");
            AddTfsTeamEndpoint(services, "TfsTeamSettingsTarget", "migrationTarget1");

            services.AddSingleton<IMigrationToolVersionInfo, FakeMigrationToolVersionInfo>();
            services.AddSingleton<IMigrationToolVersion, FakeMigrationToolVersion>();

            return services.BuildServiceProvider();
        }

        private static void AddTfsTeamEndpoint(IServiceCollection services, string name, string project)
        {
            services.AddKeyedSingleton<TfsTeamSettingsEndpoint>(name, (sp, key) =>
            {
                var options = GetTfsEndPointOptions(project);
                var endpoint = ActivatorUtilities.CreateInstance<TfsTeamSettingsEndpoint>(sp, options);
                return endpoint;
            });
        }

        private static TfsTeamSettingsEndpointOptions GetTfsTeamEndPointOptions(string project)
        {
            return new TfsTeamSettingsEndpointOptions()
            {
                Collection = new Uri("https://dev.azure.com/nkdagility-preview/"),
                Project = project,
                Authentication = new Endpoints.Infrastructure.TfsAuthenticationOptions()
                {
                    AuthenticationMode = AuthenticationMode.AccessToken,
                    AccessToken = TestingConstants.AccessToken,
                }
            };
        }

        private static void AddTfsEndpoint(IServiceCollection services, string name, string project)
        {
            services.AddKeyedSingleton<TfsEndpoint>(name, (sp, key) =>
            {
                var options = GetTfsEndPointOptions(project);
                var endpoint = ActivatorUtilities.CreateInstance<TfsEndpoint>(sp, options);
                return endpoint;
            });
        }

        private static TfsEndpointOptions GetTfsEndPointOptions(string project)
        {
            return new TfsEndpointOptions()
            {
                Collection = new Uri("https://dev.azure.com/nkdagility-preview/"),
                Project = project,
                Authentication = new Endpoints.Infrastructure.TfsAuthenticationOptions()
                {
                    AuthenticationMode = AuthenticationMode.AccessToken,
                    AccessToken = TestingConstants.AccessToken,
                }
            };
        }
    }
}