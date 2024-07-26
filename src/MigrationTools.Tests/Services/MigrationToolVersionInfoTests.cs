using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTools.Tests
{
    [TestClass()]
    public class MigrationToolVersionInfoTests
    {

        [TestMethod(), TestCategory("L0")]
        public void GetRunningVersionTest_Release()
        {
            IMigrationToolVersionInfo mtvi = new FakeMigrationToolVersionInfo("15.4.4", "15.4.4.0", "v15.4.4");
            IMigrationToolVersion mtv = new MigrationToolVersion(mtvi);
            var result = mtv.GetRunningVersion();
            Assert.AreEqual("15.4.4", result.versionString);
        }

        [TestMethod(), TestCategory("L0")]
        public void GetRunningVersionTest_Preview()
        {
            IMigrationToolVersionInfo mtvi = new FakeMigrationToolVersionInfo("15.1.5-Preview.3", "15.1.5.3", "v15.1.5-Preview.3");
            IMigrationToolVersion mtv = new MigrationToolVersion(mtvi);
            var result = mtv.GetRunningVersion();
            Assert.AreEqual("15.1.5-Preview.3", result.versionString);
        }

        [TestMethod(), TestCategory("L0")]
        public void GetRunningVersionTest_Canary()
        {
            IMigrationToolVersionInfo mtvi = new FakeMigrationToolVersionInfo("0.0.0-local+7acec2e6266f5f05b2807264ee8f1db7b94b1949", "0.0.0.0", "v15.1.5-Preview.2-18-g7acec2e");
            IMigrationToolVersion mtv = new MigrationToolVersion(mtvi);
            var result = mtv.GetRunningVersion();
            Assert.AreEqual("15.1.5-Local.2-18-g7acec2e", result.versionString);
        }
    }
}