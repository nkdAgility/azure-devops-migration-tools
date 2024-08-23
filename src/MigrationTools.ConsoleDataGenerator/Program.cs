using System.Reflection;
using System.Runtime.Loader;
using System.Security.Policy;
using Microsoft.TeamFoundation.Build.WebApi;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Helpers;
using MigrationTools.Tests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using MigrationTools.Options;
using System.Text;
using MigrationTools.ConsoleDataGenerator.ReferenceData;
using Microsoft.VisualStudio.Services.Common;
using MigrationTools.Tools.Infrastructure;
using Microsoft.Extensions.Configuration;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.ConsoleDataGenerator;
class Program
{
    private static IConfiguration configuration = GetConfiguration();
    private static DataSerialization saveData = new DataSerialization("../../../../../docs/_data/");
    private static CodeDocumentation codeDocs = new CodeDocumentation("../../../../../docs/Reference/Generated/");
    private static ClassDataLoader cdLoader = new ClassDataLoader(saveData, configuration);
    private static MarkdownLoader mdLoader = new MarkdownLoader();
    

    static void Main(string[] args)
    {
       
        string dir = AppDomain.CurrentDomain.BaseDirectory;
        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.Load("MigrationTools");
        currentDomain.Load("MigrationTools.Clients.AzureDevops.ObjectModel");
        currentDomain.Load("MigrationTools.Clients.AzureDevops.Rest");
        currentDomain.Load("MigrationTools.Clients.FileSystem");

        Console.WriteLine("Assemblies");
        List<Type> allMigrationTypes = currentDomain.GetMigrationToolsTypes().ToList();
        Console.WriteLine("-----------");
        foreach (var item in currentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("MigrationTools")))
        {
            Console.WriteLine(item.FullName);
        }
        Console.WriteLine("-----------");


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
        // Add configuration sources
        configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        // Build the configuration
       return configurationBuilder.Build();
    }


}