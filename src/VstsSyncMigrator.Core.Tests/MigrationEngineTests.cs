using System;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools;
using MigrationTools.CommandLine;
using MigrationTools.Configuration;
using MigrationTools.Engine.Containers;
using MigrationTools.Services;
using MigrationTools.Clients.AzureDevops.ObjectModel.FieldMaps;
using VstsSyncMigrator.Engine;
using MigrationTools;
using MigrationTools.Clients;
using MigrationTools.Clients.AzureDevops.ObjectModel.Clients;
using Serilog;

namespace _VstsSyncMigrator.Engine.Tests
{
    [TestClass]
    public class MigrationEngineTests
    {
        IEngineConfigurationBuilder ecb;
        IServiceProvider _services;

        [TestInitialize]
        public void Setup()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

            ecb = new EngineConfigurationBuilder(new NullLogger<EngineConfigurationBuilder>());
            var services = new ServiceCollection();
        
            // Field Mapps
            services.AddTransient<FieldBlankMap>();
            services.AddTransient<FieldLiteralMap>();
            services.AddTransient<FieldMergeMap>();
            services.AddTransient<FieldValueMap>();
            services.AddTransient<FieldToFieldMap>();
            services.AddTransient<FieldtoFieldMultiMap>();
            services.AddTransient<FieldToTagFieldMap>();
            services.AddTransient<FieldValuetoTagMap>();
            services.AddTransient<MultiValueConditionalMap>();
            services.AddTransient<RegexFieldMap>();
            services.AddTransient<TreeToTagFieldMap>();
            //Services
            services.AddSingleton<IDetectOnlineService, DetectOnlineService>();
            services.AddSingleton<IDetectVersionService, DetectVersionService>();
            //Containers
            services.AddSingleton<FieldMapContainer>();
            services.AddSingleton<ProcessorContainer>();
            services.AddSingleton<TypeDefinitionMapContainer>();
            services.AddSingleton<GitRepoMapContainer>();
            services.AddSingleton<ChangeSetMappingContainer>();

            //
            services.AddSingleton<IEngineConfigurationBuilder, EngineConfigurationBuilder>();
            services.AddSingleton<EngineConfiguration>(ecb.BuildDefault());
            services.AddSingleton<ITelemetryLogger, TelemetryLoggerMock>();
            services.AddLogging();

            //Clients
            services.AddTransient<IMigrationClient, MigrationClient>();
            services.AddTransient<IWorkItemMigrationClient, WorkItemMigrationClient>();
            services.AddTransient<IWorkItemQueryBuilder, WorkItemQueryBuilder>();


            services.AddSingleton<IMigrationEngine, MigrationEngine>();


            services.AddSingleton<ExecuteOptions>((p) => null);

            _services = services.BuildServiceProvider();

    }

        [TestMethod]
        public void TestEngineCreation()
        {

            IMigrationEngine me = _services.GetRequiredService<IMigrationEngine>();
        }

        [TestMethod]
        public void TestEngineExecuteEmptyProcessors()
        {
            EngineConfiguration ec = _services.GetRequiredService<EngineConfiguration>();
            ec.Processors.Clear();
            IMigrationEngine me = _services.GetRequiredService<IMigrationEngine>();
            me.Run();

        }

        [TestMethod]
        public void TestEngineExecuteEmptyFieldMaps()
        {
            EngineConfiguration ec = _services.GetRequiredService<EngineConfiguration>();
            ec.Processors.Clear();
            ec.FieldMaps.Clear();
            IMigrationEngine me = _services.GetRequiredService<IMigrationEngine>();
            me.Run();
        }

        [TestMethod]
        public void TestEngineExecuteProcessors()
        {
            EngineConfiguration ec = _services.GetRequiredService<EngineConfiguration>();
            ec.FieldMaps.Clear();
            IMigrationEngine me = _services.GetRequiredService<IMigrationEngine>();
            me.Run();
        }

    }
}
