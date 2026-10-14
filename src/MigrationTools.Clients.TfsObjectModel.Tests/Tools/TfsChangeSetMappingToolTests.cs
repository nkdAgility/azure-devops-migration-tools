using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tools;
using Moq;

namespace MigrationTools.Tests.Tools
{
    [TestClass]
    public class TfsChangeSetMappingToolTests
    {
        private static TfsChangeSetMappingTool CreateTool(TfsChangeSetMappingToolOptions options, Dictionary<string, string> settings)
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            var services = new ServiceCollection().AddSingleton<IConfiguration>(configuration).BuildServiceProvider();
            return new TfsChangeSetMappingTool(
                Microsoft.Extensions.Options.Options.Create(options),
                services,
                new Mock<ILogger<TfsChangeSetMappingTool>>().Object,
                new Mock<ITelemetryLogger>().Object);
        }

        [TestMethod, TestCategory("L0")]
        public void ChangeSetMappingFile_LoadsMappings()
        {
            var file = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(file, new[] { "1-abc", "2-def" });
                var tool = CreateTool(new TfsChangeSetMappingToolOptions { Enabled = true, ChangeSetMappingFile = file }, new Dictionary<string, string>());
                Assert.AreEqual(2, tool.Count);
                Assert.AreEqual("abc", tool.Items[1]);
            }
            finally
            {
                File.Delete(file);
            }
        }

        [TestMethod, TestCategory("L0")]
        public void LegacyFileSetting_IsUsedWhenChangeSetMappingFileNotSet()
        {
            var file = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(file, new[] { "42-commit" });
                var tool = CreateTool(new TfsChangeSetMappingToolOptions { Enabled = true },
                    new Dictionary<string, string> { { "MigrationTools:CommonTools:TfsChangeSetMappingTool:File", file } });
                Assert.AreEqual(1, tool.Count);
                Assert.AreEqual("commit", tool.Items[42]);
            }
            finally
            {
                File.Delete(file);
            }
        }

        [TestMethod, TestCategory("L0")]
        public void ChangeSetMappingFile_TakesPrecedenceOverLegacyFile()
        {
            var file = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(file, new[] { "7-new" });
                var tool = CreateTool(new TfsChangeSetMappingToolOptions { Enabled = true, ChangeSetMappingFile = file },
                    new Dictionary<string, string> { { "MigrationTools:CommonTools:TfsChangeSetMappingTool:File", "does-not-exist.json" } });
                Assert.AreEqual("new", tool.Items[7]);
            }
            finally
            {
                File.Delete(file);
            }
        }
    }
}
