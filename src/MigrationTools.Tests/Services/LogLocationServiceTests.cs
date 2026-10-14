using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Services;

namespace MigrationTools.Tests.Services
{
    [TestClass]
    [DoNotParallelize] // Changes the process-wide current directory.
    public class LogLocationServiceTests
    {
        private string _root;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(), "amt-logtests-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            Directory.CreateDirectory(_root);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Directory.Delete(_root, true);
        }

        [TestMethod, TestCategory("L0")]
        public void CreateLogsPath_NoBasePath_UsesCurrentWorkingDirectory()
        {
            string previous = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(_root);
                string path = LogLocationService.CreateLogsPath(null, "20260101000000");
                Assert.AreEqual(Path.Combine(Directory.GetCurrentDirectory(), "logs", "20260101000000"), path);
                Assert.IsTrue(Directory.Exists(path));
            }
            finally
            {
                Directory.SetCurrentDirectory(previous);
            }
        }

        [TestMethod, TestCategory("L0")]
        public void CreateLogsPath_WithBasePath_UsesBasePath()
        {
            string basePath = Path.Combine(_root, "custom");
            string path = LogLocationService.CreateLogsPath(basePath, "20260101000000");
            Assert.AreEqual(Path.Combine(basePath, "logs", "20260101000000"), path);
            Assert.IsTrue(Directory.Exists(path));
        }

        [TestMethod, TestCategory("L0")]
        public void CreateLogsPath_InvalidBasePath_FallsBackToLocalAppData()
        {
            string timestamp = "test-" + Guid.NewGuid().ToString("N");
            string path = LogLocationService.CreateLogsPath("bad\0path", timestamp);
            try
            {
                string expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MigrationTools", "logs", timestamp);
                Assert.AreEqual(expected, path);
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }

        [TestMethod, TestCategory("L0")]
        public void CreateLogsPath_BasePathIsAFile_FallsBackToLocalAppData()
        {
            string file = Path.Combine(_root, "notafolder");
            File.WriteAllText(file, "");
            string timestamp = "test-" + Guid.NewGuid().ToString("N");
            string path = LogLocationService.CreateLogsPath(file, timestamp);
            try
            {
                string expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MigrationTools", "logs", timestamp);
                Assert.AreEqual(expected, path);
                Assert.IsTrue(Directory.Exists(path));
            }
            finally
            {
                Directory.Delete(path, true);
            }
        }
    }
}
