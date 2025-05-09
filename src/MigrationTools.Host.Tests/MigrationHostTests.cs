using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MigrationTools.Host.Tests
{
    [TestClass()]
    public class MigrationHostTests
    {
        private IHost host;

        [TestInitialize]
        public void Setup()
        {
            host = MigrationToolHost.CreateDefaultBuilder(new string[] { "execute", "-c", "configuration.json" }).Build();
        }

        [TestMethod, TestCategory("L2")]
        [Ignore("need to ignore for now, missing a good config file for non-objectmodel")]
        public void MigrationHostTest()
        {
            IMigrationEngine mh = host.Services.GetRequiredService<IMigrationEngine>();
        }

        [TestMethod, TestCategory("L1")]
        [Ignore("need to ignore for now, untill we get some generic field maps")]
        public void TestEngineExecuteEmptyProcessors()
        {


            IMigrationEngine me = host.Services.GetRequiredService<IMigrationEngine>();
            me.Run();
        }

    }
}
