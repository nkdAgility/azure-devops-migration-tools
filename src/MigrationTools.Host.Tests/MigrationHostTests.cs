using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools._EngineV1.Configuration;

namespace MigrationTools.Host.Tests
{
    [TestClass()]
    public class MigrationHostTests
    {
        private IHost host;

        [TestInitialize]
        public void Setup()
        {
            host = MigrationToolHost.CreateDefaultBuilder(new string[0]).Build();
        }

        [TestMethod, TestCategory("L0")]
        public void MigrationHostTest()
        {
            IMigrationEngine mh = host.Services.GetRequiredService<IMigrationEngine>();
        }

        [TestMethod, TestCategory("L0")]
        [Ignore("need to ignore for now, untill we get some generic field maps")]
        public void TestEngineExecuteEmptyProcessors()
        {
            EngineConfiguration ec = host.Services.GetRequiredService<EngineConfiguration>();
            ec.Processors.Clear();
            IMigrationEngine me = host.Services.GetRequiredService<IMigrationEngine>();
            me.Run();
        }

        [TestMethod, TestCategory("L1")]
        public void TestEngineExecuteEmptyFieldMaps()
        {
            EngineConfiguration ec = host.Services.GetRequiredService<EngineConfiguration>();
            ec.Processors.Clear();
            ec.FieldMaps.Clear();
            IMigrationEngine me = host.Services.GetRequiredService<IMigrationEngine>();
            me.Run();
        }

        [TestMethod, TestCategory("L3"), TestCategory("L2")]
        public void TestEngineExecuteProcessors()
        {
            EngineConfiguration ec = host.Services.GetRequiredService<EngineConfiguration>();
            ec.FieldMaps.Clear();
            IMigrationEngine me = host.Services.GetRequiredService<IMigrationEngine>();
            me.Run();
        }
    }
}