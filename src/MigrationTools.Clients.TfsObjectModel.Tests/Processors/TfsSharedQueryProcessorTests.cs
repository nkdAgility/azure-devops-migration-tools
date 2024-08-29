using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Endpoints;
using MigrationTools.Tests;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class TfsSharedQueryProcessorTests : TfsProcessorTests
    {
        [TestMethod(), TestCategory("L0")]
        public void TfsSharedQueryProcessorTest()
        {
            var x = GetTfsSharedQueryProcessor();
            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L0")]
        public void TfsSharedQueryProcessorConfigureTest()
        {
            var y = new TfsSharedQueryProcessorOptions
            {
                Enabled = true,
                PrefixProjectToNodes = false,
                RefName = "fortyTwo",
                SourceName = "Source",
                TargetName = "Target"
            };
            var x = GetTfsSharedQueryProcessor(y);
            Assert.IsNotNull(x);
            Assert.AreEqual("fortyTwo", x.Options.RefName);
        }

        [TestMethod(), TestCategory("L0")]
        public void TfsSharedQueryProcessorRunTest()
        {
            var y = new TfsSharedQueryProcessorOptions
            {
                Enabled = true,
                PrefixProjectToNodes = false,
                SourceName = "Source",
                TargetName = "Target"
            };
            var x = GetTfsSharedQueryProcessor(y);
            x.Execute();
            Assert.IsNotNull(x);
        }
    }
}