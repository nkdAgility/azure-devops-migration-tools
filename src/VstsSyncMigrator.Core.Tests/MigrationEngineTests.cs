using System;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools;
using MigrationTools.CommandLine;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.Engine.Containers;
using MigrationTools.Services;
using MigrationTools.Sinks.TfsObjectModel.FieldMaps;
using VstsSyncMigrator.Engine;

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
            services.AddSingleton<TelemetryClient>(new TelemetryClient());
            services.AddSingleton<ITelemetryLogger, TestTelemetryLogger>();
            services.AddSingleton<MigrationEngine>();

            services.AddSingleton<ExecuteOptions>((p) => null);

            _services = services.BuildServiceProvider();

    }

        [TestMethod]
        public void TestEngineCreation()
        {

            MigrationEngine me = _services.GetRequiredService<MigrationEngine>();
        }

        [TestMethod]
        public void TestEngineExecuteEmptyProcessors()
        {
            EngineConfiguration ec = _services.GetRequiredService<EngineConfiguration>();
            ec.Processors.Clear();
            MigrationEngine me = _services.GetRequiredService<MigrationEngine>();
            me.Run();

        }

        [TestMethod]
        public void TestEngineExecuteEmptyFieldMaps()
        {
            EngineConfiguration ec = _services.GetRequiredService<EngineConfiguration>();
            ec.Processors.Clear();
            ec.FieldMaps.Clear();
            MigrationEngine me = _services.GetRequiredService<MigrationEngine>();
            me.Run();
        }

        [TestMethod]
        public void TestEngineExecuteProcessors()
        {
            EngineConfiguration ec = _services.GetRequiredService<EngineConfiguration>();
            ec.FieldMaps.Clear();
            MigrationEngine me = _services.GetRequiredService<MigrationEngine>();
            me.Run();
        }

    }
}
