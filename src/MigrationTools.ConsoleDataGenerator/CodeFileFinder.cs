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
            
            // First try to find an exact match
            List<string> exactMatches = Directory.GetFiles(codePathToSearch, $"{typeToFind.Name}.cs", SearchOption.AllDirectories).ToList();
            if (exactMatches.Count > 0)
            {
                string relativePath = Path.GetRelativePath(codePath, exactMatches[0]).Replace("\\", "/");
                return relativePath;
            }
            
            // Fallback to pattern match if exact match not found
            List<string> files = Directory.GetFiles(codePathToSearch, $"*{typeToFind.Name}*.cs", SearchOption.AllDirectories).ToList();
            if (files.Count > 0)
            {
                // Prefer files that match exactly, then files that start with the type name
                var preferredFile = files
                    .OrderBy(f => Path.GetFileNameWithoutExtension(f) != typeToFind.Name ? 1 : 0) // Exact matches first
                    .ThenBy(f => !Path.GetFileNameWithoutExtension(f).StartsWith(typeToFind.Name) ? 1 : 0) // Then starts with
                    .First();

                string relativePath = Path.GetRelativePath(codePath, preferredFile).Replace("\\", "/");
                return relativePath;
            }
            return "";
        }
    }
}
