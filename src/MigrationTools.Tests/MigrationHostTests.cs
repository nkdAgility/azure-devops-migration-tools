using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.Configuration.Tests;
using MigrationTools.Core.Engine;
using MigrationTools.Core.Engine.Containers;
using MigrationTools.Services;

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
            var logger = new NullLogger<EngineConfigurationBuilder>();
            ecb = new EngineConfigurationBuilder(logger);
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
                services.AddSingleton<ITelemetryLogger, TelemetryClientAdapter>();
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