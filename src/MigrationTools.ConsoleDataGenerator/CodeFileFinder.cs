using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTools.ConsoleDataGenerator
{

    public class CodeFileFinder
    {
        private string codePath = "";

        public CodeFileFinder(string path)
        {
            codePath = path;
        }

        public string FindCodeFile(Type typeToFind)
        {
            string assemblyName = typeToFind.Assembly.GetName().Name;
            string codePathToSearch = Path.Combine(codePath, assemblyName);
            List<string> files = Directory.GetFiles(codePathToSearch, $"*{typeToFind.Name}*.cs", SearchOption.AllDirectories).ToList();
            if (files.Count > 0)
            {
               var fileInfo = new FileInfo(files[0]);

                return files[0].Replace("../../../../..", "").Replace("\\", "/");
            }
            return "";
        }
    }
}
