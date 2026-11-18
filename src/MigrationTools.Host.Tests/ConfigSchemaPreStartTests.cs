using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Host;

namespace MigrationTools.Host.Tests
{
    [TestClass]
    public class ConfigSchemaPreStartTests
    {
        private static readonly string SchemaSource = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs", "static", "schema", "configuration.schema.json"));
        private string _dir;

        [TestInitialize]
        public void Setup()
        {
            _dir = Path.Combine(Path.GetTempPath(), $"ConfigSchemaPreStart_{Guid.NewGuid()}");
            Directory.CreateDirectory(_dir);
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(_dir, true);

        private string Config(string scope)
        {
            var path = Path.Combine(_dir, "config.json");
            File.WriteAllText(path, "{ \"MigrationTools\": { \"Version\": \"16.0\", \"Endpoints\": {}, \"Processors\": [], "
                + "\"CommonTools\": { \"TfsUserMappingTool\": { \"Enabled\": true, \"ValidateMappingTargets\": \"" + scope + "\" } } } }");
            return path;
        }

        private void WithSchema(Action test)
        {
            var target = Path.Combine(AppContext.BaseDirectory, "configuration.schema.json");
            var existed = File.Exists(target);
            if (!existed)
            {
                File.Copy(SchemaSource, target);
            }
            try { test(); }
            finally { if (!existed) { File.Delete(target); } }
        }

        [TestMethod, TestCategory("L1")]
        public void Execute_MisspeltEnum_IsRejectedBeforeTheHostStarts()
        {
            WithSchema(() => Assert.IsFalse(MigrationToolHost.IsExecutableConfigSchemaValid(["execute", "--config", Config("Referneced")], Config("Referneced"))));
        }

        [TestMethod, TestCategory("L1")]
        public void Execute_EnumInAnyCase_IsAccepted()
        {
            WithSchema(() => Assert.IsTrue(MigrationToolHost.IsExecutableConfigSchemaValid(["execute"], Config("referenced"))));
        }

        [TestMethod, TestCategory("L1")]
        public void Execute_InvalidJson_IsRejectedWithoutThrowing()
        {
            var path = Path.Combine(_dir, "broken.json");
            File.WriteAllText(path, "{ \"MigrationTools\": { ");
            WithSchema(() => Assert.IsFalse(MigrationToolHost.IsExecutableConfigSchemaValid(["execute"], path)));
        }

        [TestMethod, TestCategory("L0")]
        public void OtherCommandsAndMissingFiles_AreNotChecked()
        {
            Assert.IsTrue(MigrationToolHost.IsExecutableConfigSchemaValid(["upgrade"], Config("Referneced")));
            Assert.IsTrue(MigrationToolHost.IsExecutableConfigSchemaValid(["execute"], Path.Combine(_dir, "missing.json")));
        }
    }
}
