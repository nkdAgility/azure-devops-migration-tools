using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsMigrationTools.CommandLine
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
