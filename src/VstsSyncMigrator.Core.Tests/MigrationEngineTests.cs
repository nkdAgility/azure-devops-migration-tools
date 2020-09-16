using System;
using MigrationTools.Core.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VstsSyncMigrator.Engine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Core.Engine.Containers;
using MigrationTools.Services;

namespace _VstsSyncMigrator.Engine.Tests
{
    [TestClass]
    public class MigrationEngineTests
    {
        IEngineConfigurationBuilder ecb;
        IHost host;

        [TestInitialize]
        public void Setup()
        {
            ecb = new EngineConfigurationBuilder();
            host= new HostBuilder().ConfigureServices((context, services) =>
            {
                services.AddSingleton<IDetectOnlineService, DetectOnlineService>();
                services.AddSingleton<IDetectVersionService, DetectVersionService>();
                services.AddSingleton<IEngineConfigurationBuilder, EngineConfigurationBuilder>();
                services.AddSingleton<EngineConfiguration>(ecb.BuildDefault());
                services.AddSingleton<ProcessorContainer>();
                services.AddSingleton<TypeDefinitionMapContainer>();
                services.AddSingleton<GitRepoMapContainer>();
                services.AddSingleton<ChangeSetMappingContainer>();
                services.AddSingleton<MigrationEngine>();
            }).Build();
        }

        [TestMethod]
        public void TestEngineCreation()
        {

            MigrationEngine me = host.Services.GetRequiredService<MigrationEngine>();
        }

        [TestMethod]
        public void TestEngineExecuteEmptyProcessors()
        {
            EngineConfiguration ec = ecb.BuildDefault();
            ec.Processors.Clear();
            MigrationEngine me = new MigrationEngine(host, ec);
            me.Run();

        }

        [TestMethod]
        public void TestEngineExecuteEmptyFieldMaps()
        {
            EngineConfiguration ec = ecb.BuildDefault();
            ec.Processors.Clear();
            ec.FieldMaps.Clear();
            MigrationEngine me = new MigrationEngine(host, ec);
            me.Run();
        }

        [TestMethod]
        public void TestEngineExecuteProcessors()
        {
            EngineConfiguration ec = ecb.BuildDefault();
            ec.FieldMaps.Clear();
            MigrationEngine me = new MigrationEngine(host, ec);
            me.Run();
        }

    }
}
