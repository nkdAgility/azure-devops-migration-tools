using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Endpoints;
using MigrationTools.Tests;

namespace MigrationTools.Processors.Tests
{
    public class TfsProcessorTests
    {
        protected ServiceProvider Services = ServiceProviderHelper.GetServices();

        [TestInitialize]
        public void Setup()
        {
        }

        protected static TfsTeamSettingsProcessorOptions GetTfsTeamSettingsProcessorOptions()
        {
            // Tfs To Tfs
            var migrationConfig = new TfsTeamSettingsProcessorOptions()
            {
                Enabled = true,
                MigrateTeamSettings = true,
                UpdateTeamSettings = true,
                PrefixProjectToNodes = false,
                Endpoints = new List<IEndpointOptions>()
                    {
                         {GetTfsEndPointOptions(EndpointDirection.Source, "migrationSource1")},
                         {GetTfsEndPointOptions(EndpointDirection.Target, "migrationTarget1")}
                    }
            };
            return migrationConfig;
        }

        protected static TfsSharedQueryProcessorOptions GetTfsSharedQueryProcessorOptions()
        {
            // Tfs To Tfs
            var migrationConfig = new TfsSharedQueryProcessorOptions()
            {
                Enabled = true,
                PrefixProjectToNodes = false,
                Endpoints = new List<IEndpointOptions>()
                    {
                         {GetTfsEndPointOptions(EndpointDirection.Source, "migrationSource1")},
                         {GetTfsEndPointOptions(EndpointDirection.Target, "migrationTarget1")}
                    }
            };
            return migrationConfig;
        }

        protected static TfsEndpointOptions GetTfsEndPointOptions(EndpointDirection direction, string project)
        {
            return new TfsTeamSettingsEndpointOptions()
            {
                Direction = direction,
                Organisation = "https://dev.azure.com/nkdagility-preview/",
                Project = project,
                AuthenticationMode = AuthenticationMode.AccessToken,
                AccessToken = TestingConstants.AccessToken,
            };
        }
    }
}