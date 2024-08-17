using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools.Enrichers;
using MigrationTools.ProcessorEnrichers;
using MigrationTools.Tests;
using VstsSyncMigrator.Engine;

namespace VstsSyncMigrator.Core.Tests
{
    [TestClass]
    public class WorkItemMigrationTests
    {
        private ServiceProvider _services;

        [TestInitialize]
        public void Setup()
        {
            _services = ServiceProviderHelper.GetServices();           
        }

       
    }
}