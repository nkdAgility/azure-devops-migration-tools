using System.IO;

namespace VstsSyncMigrator.Commands
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