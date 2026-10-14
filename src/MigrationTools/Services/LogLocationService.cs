using System;
using System.IO;

namespace MigrationTools.Services
{
    /// <summary>
    /// Resolves the folder that the log files are written to: <c>{base}/logs/{timestamp}</c>.
    /// The base is the folder passed with <c>--logPath</c>, or the current working directory.
    /// If the base folder cannot be written to, the logs go to <c>%LOCALAPPDATA%/MigrationTools</c> instead.
    /// </summary>
    public class LogLocationService
    {
        private static readonly string logDate = DateTime.Now.ToString("yyyyMMddHHmmss");
        private static string _basePath;
        private static string _logsPath;

        /// <summary>
        /// Sets the base folder for the logs. Call before the first <see cref="GetLogPath"/>.
        /// Null or empty uses the current working directory.
        /// </summary>
        public static void SetBasePath(string basePath)
        {
            _basePath = basePath;
            _logsPath = null;
        }

        public static string GetLogPath()
        {
            if (_logsPath == null)
            {
                _logsPath = CreateLogsPath(_basePath, logDate);
            }
            return _logsPath;
        }

        public static string CreateLogsPath(string basePath, string timestamp)
        {
            try
            {
                string root = string.IsNullOrWhiteSpace(basePath) ? Directory.GetCurrentDirectory() : Path.GetFullPath(basePath);
                string exportPath = Path.Combine(root, "logs", timestamp);
                Directory.CreateDirectory(exportPath);
                // The folder can exist but not allow files to be created, so prove a file can be written.
                string probe = Path.Combine(exportPath, ".write-test");
                File.WriteAllText(probe, string.Empty);
                File.Delete(probe);
                return exportPath;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException || ex is ArgumentException || ex is NotSupportedException)
            {
                string fallback = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MigrationTools", "logs", timestamp);
                Directory.CreateDirectory(fallback);
                return fallback;
            }
        }
    }
}
