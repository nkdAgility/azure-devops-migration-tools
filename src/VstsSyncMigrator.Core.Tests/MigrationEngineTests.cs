using System;
using MigrationTools.Core.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VstsSyncMigrator.Engine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Core.Engine.Containers;
using MigrationTools.Services;
using Microsoft.ApplicationInsights;

namespace _VstsSyncMigrator.Engine.Tests
{
    [TestClass]
    public class MigrationEngineTests
    {
        IEngineConfigurationBuilder ecb;
        IServiceProvider services;

        [TestInitialize]
        public void Setup()
        {
            ecb = new EngineConfigurationBuilder();
            var serviceColl = new ServiceCollection();
                serviceColl.AddSingleton<IDetectOnlineService, DetectOnlineService>();
            serviceColl.AddSingleton<IDetectVersionService, DetectVersionService>();
            serviceColl.AddSingleton<IEngineConfigurationBuilder, EngineConfigurationBuilder>();
            serviceColl.AddSingleton<EngineConfiguration>(ecb.BuildDefault());
            serviceColl.AddSingleton<TelemetryClient>(new TelemetryClient());
            serviceColl.AddSingleton<ProcessorContainer>();
            serviceColl.AddSingleton<TypeDefinitionMapContainer>();
            serviceColl.AddSingleton<GitRepoMapContainer>();
            serviceColl.AddSingleton<ChangeSetMappingContainer>();
            serviceColl.AddSingleton<MigrationEngine>();
            services = serviceColl.BuildServiceProvider();

    }

        [TestMethod]
        public void TestEngineCreation()
        {

            MigrationEngine me = services.GetRequiredService<MigrationEngine>();
        }

        [TestMethod]
        public void TestEngineExecuteEmptyProcessors()
        {
            EngineConfiguration ec = services.GetRequiredService<EngineConfiguration>();
            ec.Processors.Clear();
            MigrationEngine me = services.GetRequiredService<MigrationEngine>();
            me.Run();

        }

        [TestMethod]
        public void TestEngineExecuteEmptyFieldMaps()
        {
            EngineConfiguration ec = services.GetRequiredService<EngineConfiguration>();
            ec.Processors.Clear();
            ec.FieldMaps.Clear();
            MigrationEngine me = services.GetRequiredService<MigrationEngine>();
            me.Run();
        }

        [TestMethod]
        public void TestEngineExecuteProcessors()
        {
            EngineConfiguration ec = services.GetRequiredService<EngineConfiguration>();
            ec.FieldMaps.Clear();
            MigrationEngine me = services.GetRequiredService<MigrationEngine>();
            me.Run();
        }

    }
}
