﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Helpers.Tests;
using MigrationTools.Options;
using MigrationTools.Services;
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
            var configuration = new ConfigurationBuilder().Build();
            var VersionOptions = new VersionOptions();
            IOptions<VersionOptions> options = Options.Create(VersionOptions);
            var ecb = new EngineConfigurationBuilder(options, new NullLogger<EngineConfigurationBuilder>());
            var services = new ServiceCollection();
            // Core
            services.AddMigrationToolServicesForUnitTests();
            services.AddMigrationToolServicesForClientLegacyCore();
            services.AddMigrationToolServices(configuration);
            services.AddMigrationToolServicesLegacy();
            // Clients
            services.AddMigrationToolServicesForClientAzureDevOpsObjectModel(configuration);
            services.AddMigrationToolServicesForClientLegacyAzureDevOpsObjectModel();

            //
            services.AddSingleton<IEngineConfigurationBuilder, EngineConfigurationBuilder>();
            services.AddOptions();
            services.AddSingleton<EngineConfiguration>(ecb.BuildDefault());

            services.AddSingleton<IMigrationEngine, MigrationEngine>();

            services.AddSingleton<IMigrationToolVersionInfo, FakeMigrationToolVersionInfo>();
            services.AddSingleton<IMigrationToolVersion, FakeMigrationToolVersion>();

            _services = services.BuildServiceProvider();
        }

        [TestMethod, TestCategory("L2")]
        [Ignore("Need to ignore untill new config model live")]
        public void TestEngineExecuteEmptyProcessors()
        {
            EngineConfiguration ec = _services.GetRequiredService<EngineConfiguration>();
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