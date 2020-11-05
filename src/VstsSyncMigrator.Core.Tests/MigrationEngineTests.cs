using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools;
using MigrationTools._Enginev1.Containers;
using MigrationTools.Clients;
using MigrationTools.CommandLine;
using MigrationTools.Configuration;
using MigrationTools.FieldMaps.AzureDevops.ObjectModel;
using MigrationTools.Services;
using Serilog.Core;

namespace _VstsSyncMigrator.Engine.Tests
{
    [TestClass]
    public class MigrationEngineTests
    {
        private IServiceProvider _services;

        [TestInitialize]
        public void Setup()
        {
            var ecb = new EngineConfigurationBuilder(new NullLogger<EngineConfigurationBuilder>());
            var services = new ServiceCollection();
            services.AddSingleton<LoggingLevelSwitch>();

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
            //services.AddSingleton<IDetectOnlineService, DetectOnlineService>();
            //services.AddSingleton<IDetectVersionService, DetectVersionService>();
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
            services.AddApplicationInsightsTelemetryWorkerService();
            services.AddLogging();

            //Clients
            services.AddTransient<IMigrationClient, TfsMigrationClient>();
            services.AddTransient<IWorkItemMigrationClient, TfsWorkItemMigrationClient>();
            services.AddTransient<IWorkItemQueryBuilder, WorkItemQueryBuilder>();

            services.AddSingleton<IMigrationEngine, MigrationEngine>();

            services.AddSingleton<ExecuteOptions>((p) => null);

            _services = services.BuildServiceProvider();
        }

        [TestMethod]
        public void TestEngineExecuteEmptyProcessors()
        {
            EngineConfiguration ec = _services.GetRequiredService<EngineConfiguration>();
            ec.Processors.Clear();
            IMigrationEngine me = _services.GetRequiredService<IMigrationEngine>();
            me.Run();
        }

    }
}