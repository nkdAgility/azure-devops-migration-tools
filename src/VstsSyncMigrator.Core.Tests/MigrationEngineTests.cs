using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.TestExtensions;
using Serilog;

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
            // Core
            services.AddMigrationToolServicesForUnitTests();
            services.AddMigrationToolServicesForClientLegacyCore();
            services.AddMigrationToolServices();
            services.AddMigrationToolServicesLegacy();
            // Clients
            services.AddMigrationToolServicesForClientAzureDevOpsObjectModel();
            services.AddMigrationToolServicesForClientLegacyAzureDevOpsObjectModel();

            //
            //services.AddSingleton<IEngineConfigurationBuilder, EngineConfigurationBuilder>();
            services.AddOptions();
            services.AddSingleton<EngineConfiguration>(ecb.BuildDefault());

            services.AddSingleton<IMigrationEngine, MigrationEngine>();

            _services = services.BuildServiceProvider();
        }

        [TestMethod, TestCategory("L2")]
        public void TestEngineExecuteEmptyProcessors()
        {
            EngineConfiguration ec = _services.GetRequiredService<EngineConfiguration>();
            ec.Processors.Clear();
            IMigrationEngine me = _services.GetRequiredService<IMigrationEngine>();
            me.Run();
        }

        [TestMethod, TestCategory("L2")]
        public void TestTypeLoadForAborations()
        {
            List<Type> allTypes;
            try
            {
                allTypes = AppDomain.CurrentDomain.GetAssemblies()
               .Where(a => !a.IsDynamic)
               .SelectMany(a => a.GetTypes()).ToList();
            }
            catch (ReflectionTypeLoadException ex)
            {
                allTypes = new List<Type>();
                Log.Error(ex, "Unable to continue! ");
                foreach (Exception item in ex.LoaderExceptions)
                {
                    Log.Error(item, "LoaderException: {Message}", item.Message);
                }
                throw ex;
            }
        }
    }
}