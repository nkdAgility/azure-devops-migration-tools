using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSTS.DataBulkEditor.Engine;
using VSTS.DataBulkEditor.Engine.Configuration;

namespace _VSTS.DataBulkEditor.Engine.Tests
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
