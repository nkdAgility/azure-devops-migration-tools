using System.IO;

namespace MigrationTools.Host.CommandLine
{
    public class CommandBase
    {
        public static string CreateExportPath(string logPath, string CommandName)
        {
            string exportPath = Path.Combine(logPath, CommandName);
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }

            return exportPath;
        }
    }
}