using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools;
using MigrationTools.Core.Configuration.Tests;
using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Tests
{
    [TestClass()]
    public class MigrationHostTests
    {
        [TestMethod()]
        public void MigrationHostTest()
        {
            
            MigrationHost mh = new MigrationHost(null, null, new EngineConfigurationBuilderStub());

        }


    }
}