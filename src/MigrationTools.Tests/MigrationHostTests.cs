using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools;
using MigrationTools.Core.Configuration;
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

    public class EngineConfigurationBuilderStub : IEngineConfigurationBuilder
    {
        IEngineConfigurationBuilder ecb = new EngineConfigurationBuilder();

        public EngineConfiguration BuildDefault()
        {
           return  ecb.BuildDefault();
        }

        public EngineConfiguration BuildFromFile()
        {
            return ecb.BuildDefault();
        }

        public EngineConfiguration BuildWorkItemMigration()
        {
            return ecb.BuildWorkItemMigration();
        }
    }
}