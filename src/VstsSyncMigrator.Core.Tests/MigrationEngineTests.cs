using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VstsSyncMigrator.Engine;
using VstsSyncMigrator.Engine.Configuration;

namespace _VstsSyncMigrator.Engine.Tests
{
    [TestClass]
    public class MigrationEngineTests
    {
        [TestMethod]
        public void TestEngineCreation()
        {
            EngineConfiguration ec = EngineConfiguration.GetDefault();
            MigrationEngine me = new MigrationEngine(ec);
        }

        [TestMethod]
        public void TestEngineExecuteEmptyProcessors()
        {
            EngineConfiguration ec = EngineConfiguration.GetDefault();
            ec.Processors.Clear();
            MigrationEngine me = new MigrationEngine(ec);
            me.Run();

        }

        [TestMethod]
        public void TestEngineExecuteEmptyFieldMaps()
        {
            EngineConfiguration ec = EngineConfiguration.GetDefault();
            ec.Processors.Clear();
            ec.FieldMaps.Clear();
            MigrationEngine me = new MigrationEngine(ec);
            me.Run();
        }


    }
}
