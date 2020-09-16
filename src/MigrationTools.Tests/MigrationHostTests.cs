using Microsoft.ApplicationInsights.WindowsServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.Configuration.Tests;
using MigrationTools.Core.Engine;
using MigrationTools.Core.Engine.Containers;
using MigrationTools.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Tests
{
    [TestClass()]
    public class MigrationHostTests
    {
        IEngineConfigurationBuilder ecb;
        IHost host;

        [TestInitialize]
        public void Setup()
        {
            ecb = new EngineConfigurationBuilder();
            host = new HostBuilder().ConfigureServices((context, services) =>
            {
                services.AddSingleton<IDetectOnlineService, DetectOnlineService>();
                services.AddSingleton<IDetectVersionService, DetectVersionService>();
                services.AddSingleton<IEngineConfigurationBuilder, EngineConfigurationBuilderStub>();
                services.AddSingleton<EngineConfiguration>(ecb.BuildDefault());
                services.AddSingleton<ProcessorContainer>();
                services.AddSingleton<TypeDefinitionMapContainer>();
                services.AddSingleton<GitRepoMapContainer>();
                services.AddSingleton<ChangeSetMappingContainer>();
                services.AddSingleton(Telemetry.Current);
                services.AddSingleton<ITeamProjectContext, TeamProjectContextMoc>();
                services.AddSingleton<MigrationEngineCore>();
            }).Build();
        }

        [TestMethod()]
        public void MigrationHostTest()
        {
            MigrationEngineCore mh = host.Services.GetRequiredService<MigrationEngineCore>();


        }


    }
}