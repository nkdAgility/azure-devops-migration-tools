using System.Reflection;
using System.IO;
using Microsoft.Extensions.Configuration;
using MigrationTools.ConsoleDataGenerator.ReferenceData;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints.Infrastructure;
using MigrationTools.Enrichers;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.ConsoleDataGenerator;
class Program
{
    private static IConfiguration configuration = GetConfiguration();
    private static string rootPath = GetRepositoryRoot();
    private static DataSerialization saveData = new DataSerialization(rootPath);
    private static ClassDataLoader cdLoader = new ClassDataLoader(rootPath, saveData, configuration);
    private static MarkdownLoader mdLoader = new MarkdownLoader(Path.Combine(rootPath, "docs/Reference/"));


    static void Main(string[] args)
    {

        AppDomain.CurrentDomain.AssemblyLoad += (sender, args) =>
        {
            Console.WriteLine("Loaded assembly: " + args.LoadedAssembly.FullName);
        };


        string dir = AppDomain.CurrentDomain.BaseDirectory;
        Console.WriteLine(dir);
        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.Load("MigrationTools");
        currentDomain.Load("MigrationTools.Clients.TfsObjectModel");
        currentDomain.Load("MigrationTools.Clients.AzureDevops.Rest");
        currentDomain.Load("MigrationTools.Clients.FileSystem");
        //currentDomain.Load("Microsoft.Extensions.Options");

        Assembly assembly = Assembly.LoadFrom(System.IO.Path.Combine(dir, "Microsoft.Extensions.Options.dll"));


        Console.WriteLine("Assemblies");

        Console.WriteLine("-----------");
        foreach (var item in currentDomain.GetAssemblies())
        {
            Console.WriteLine(item.FullName);
        }
        Console.WriteLine("-----------");
        List<Type> allMigrationTypes = currentDomain.GetMigrationToolsTypes().ToList();

        List<ClassData> classDataList = new List<ClassData>();

        classDataList.AddRange(cdLoader.GetClassDataFromOptions<IProcessorOptions>(allMigrationTypes, "Processors"));
        classDataList.AddRange(cdLoader.GetClassDataFromOptions<IToolOptions>(allMigrationTypes, "Tools"));
        classDataList.AddRange(cdLoader.GetClassDataFromOptions<IFieldMapOptions>(allMigrationTypes, "FieldMaps"));
        classDataList.AddRange(cdLoader.GetClassDataFromOptions<IProcessorEnricherOptions>(allMigrationTypes, "ProcessorEnrichers"));
        classDataList.AddRange(cdLoader.GetClassDataFromOptions<IEndpointOptions>(allMigrationTypes, "Endpoints"));
        classDataList.AddRange(cdLoader.GetClassDataFromOptions<IEndpointEnricherOptions>(allMigrationTypes, "EndpointEnrichers"));



        Console.WriteLine("-----------");
        Console.WriteLine("Output");
        Console.WriteLine("-----------");
        foreach (var classData in classDataList)
        {
            Console.Write($"Out: {classData.ClassName}");
            
            // Add notes information to the ClassData
            var notesInfo = mdLoader.GetMarkdownForTopic(classData, "notes");
            classData.Notes = new NotesInfo
            {
                Exists = notesInfo.Exists,
                Path = notesInfo.Path,
                Markdown = notesInfo.Markdown
            };
            
            saveData.WriteYamlDataToDataFolder(classData);
            Console.Write($" [Yaml]");
            JekyllData jekyllData = GetJekyllData(classData);

            saveData.WriteMarkdownDataToCollectionFolder(classData, jekyllData);
            Console.Write($" [Markdown]");
            Console.WriteLine();
        }
        Console.WriteLine("-----------");

    }

    private static JekyllData GetJekyllData(ClassData classData)
    {
        JekyllData data = new JekyllData();
        data.Permalink = $"/Reference/{classData.TypeName}/{classData.ClassName}/";
        data.layout = "reference";
        data.toc = true;
        data.title = classData.ClassName;
        data.categories.Add(classData.TypeName);
        data.categories.Add(classData.Architecture);
        data.Topics.Add(mdLoader.GetMarkdownForTopic(classData, "notes"));
        data.Topics.Add(mdLoader.GetMarkdownForTopic(classData, "introduction"));
        List<string> posibleOldUrls = new List<string>()
        {
            $"/Reference/{classData.TypeName}/{classData.OptionsClassName}/"
        };
        foreach (var possible in posibleOldUrls)
        {
            if (possible != data.Permalink)
            {
                data.Redirect_from.Add(possible);
            }
        }
        return data;
    }

    private static IConfiguration GetConfiguration()
    {
        // Create a new ConfigurationBuilder
        var configurationBuilder = new ConfigurationBuilder();
        // Set the base path for the configuration (optional)
        configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
        // Add configuration sources - try current directory first, then repository root
        configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        configurationBuilder.AddJsonFile("../../appsettings.json", optional: true, reloadOnChange: true);
        // Build the configuration
        return configurationBuilder.Build();
    }

    private static string GetRepositoryRoot()
    {
        // Start from current directory and walk up to find the repository root
        string currentDir = Directory.GetCurrentDirectory();
        
        while (currentDir != null)
        {
            // Look for indicators of repository root (like .git, src folder, or MigrationTools.sln)
            if (Directory.Exists(Path.Combine(currentDir, ".git")) ||
                File.Exists(Path.Combine(currentDir, "MigrationTools.sln")))
            {
                return currentDir;
            }
            
            string parentDir = Directory.GetParent(currentDir)?.FullName;
            if (parentDir == currentDir) // Reached root of filesystem
                break;
            currentDir = parentDir;
        }
        
        // Fallback: try the original relative paths
        if (Directory.Exists("../../src"))
            return "../../";
        else if (Directory.Exists("../../../../../src"))
            return "../../../../../";
        
        throw new DirectoryNotFoundException("Could not locate repository root directory. Please run from project directory or bin directory.");
    }


}
