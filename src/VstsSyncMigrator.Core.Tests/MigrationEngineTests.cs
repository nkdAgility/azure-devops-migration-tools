using System;
using MigrationTools.Core.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VstsSyncMigrator.Engine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Core.Engine.Containers;
using MigrationTools.Services;
using Microsoft.ApplicationInsights;
using MigrationTools;
using MigrationTools.Sinks.TfsObjectModel.FieldMaps;

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
            ecb = new EngineConfigurationBuilder();
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
