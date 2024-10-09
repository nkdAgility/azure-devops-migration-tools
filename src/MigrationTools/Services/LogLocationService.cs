using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace MigrationTools.Services
{
   public class LogLocationService
    {
       private static string logDate = DateTime.Now.ToString("yyyyMMddHHmmss");


       private static string _logsPath = CreateLogsPath();

        public static string GetLogPath()
        {
            // Check if the string has been generated.
            if (_logsPath == null)
            {
                // Generate the string at runtime (e.g., any custom logic).
                _logsPath = CreateLogsPath();
            }

            // Return the _logsPath string.
            return _logsPath;
        }

        private static string CreateLogsPath()
        {
            string exportPath;
            string assPath = Assembly.GetEntryAssembly().Location;
            exportPath = Path.Combine(Path.GetDirectoryName(assPath), "logs", logDate);
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }

            return exportPath;
        }
    }
}
