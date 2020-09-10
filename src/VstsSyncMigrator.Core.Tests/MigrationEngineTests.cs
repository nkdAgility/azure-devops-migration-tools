using System;
using MigrationTools.Core.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VstsSyncMigrator.Engine;

namespace _VstsSyncMigrator.Engine.Tests
{
    [TestClass]
    public class MigrationEngineTests
    {
        IEngineConfigurationBuilder ecb = new EngineConfigurationBuilder();

        [TestMethod]
        public void TestEngineCreation()
        {
            EngineConfiguration ec = ecb.BuildDefault();
            MigrationEngine me = new MigrationEngine(ec);
        }

        [TestMethod]
        public void TestEngineExecuteEmptyProcessors()
        {
            EngineConfiguration ec = ecb.BuildDefault();
            ec.Processors.Clear();
            MigrationEngine me = new MigrationEngine(ec);
            me.Run();

        }

        [TestMethod]
        public void TestEngineExecuteEmptyFieldMaps()
        {
            EngineConfiguration ec = ecb.BuildDefault();
            ec.Processors.Clear();
            ec.FieldMaps.Clear();
            MigrationEngine me = new MigrationEngine(ec);
            me.Run();
        }

        [TestMethod]
        public void TestEngineExecuteProcessors()
        {
            EngineConfiguration ec = ecb.BuildDefault();
            ec.FieldMaps.Clear();
            MigrationEngine me = new MigrationEngine(ec);
            me.Run();
        }

    }
}
