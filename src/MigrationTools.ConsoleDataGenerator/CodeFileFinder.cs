namespace MigrationTools.ConsoleDataGenerator
{

    public class CodeFileFinder
    {
        private string codePath;

        public CodeFileFinder(string path)
        {
            codePath = path;
        }

        public string FindCodeFile(Type typeToFind)
        {
            string assemblyName = typeToFind.Assembly.GetName().Name;
            string codePathToSearch = Path.Combine(codePath, "src", assemblyName);
            List<string> files = Directory.GetFiles(codePathToSearch, $"*{typeToFind.Name}*.cs", SearchOption.AllDirectories).ToList();
            if (files.Count > 0)
            {
                var fileInfo = new FileInfo(files[0]);

                // Return the relative path from codePath to files[0], using forward slashes
                string relativePath = Path.GetRelativePath(codePath, files[0]).Replace("\\", "/");
                return relativePath;
            }
            return "";
        }
    }
}
