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
using MigrationTools.Tools.Infra;
using Microsoft.Extensions.Configuration;

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
        List<Assembly> asses = currentDomain.GetAssemblies().ToList();
        Console.WriteLine("-----------");
        foreach (var item in currentDomain.GetAssemblies())
        {
            Console.WriteLine(item.FullName);
        }
        Console.WriteLine("-----------");

        List<Type> newTypes = asses
                .Where(a => !a.IsDynamic && a.FullName.StartsWith("MigrationTools"))
                .SelectMany(a => a.GetTypes()).ToList();



        List<ClassData> classDataList = new List<ClassData>();
        // V1
        classDataList.AddRange(cdLoader.GetClassData(newTypes, newTypes, typeof(MigrationTools._EngineV1.Containers.IOldProcessor), "v1", "Processors", true, "Config"));
        classDataList.AddRange(cdLoader.GetClassData(newTypes, newTypes, typeof(IFieldMapConfig), "v1", "FieldMaps", false));
        // V2
        classDataList.AddRange(cdLoader.GetClassData(newTypes, newTypes, typeof(MigrationTools.Processors.IProcessor), "v2", "Processors"));
        classDataList.AddRange(cdLoader.GetClassData(newTypes, newTypes, typeof(IProcessorEnricher), "v2", "ProcessorEnrichers"));
       
        classDataList.AddRange(cdLoader.GetClassData(newTypes, newTypes, typeof(IFieldMapConfig), "v2", "FieldMaps", false));
        classDataList.AddRange(cdLoader.GetClassData(newTypes, newTypes, typeof(IEndpoint), "v2", "Endpoints"));
        classDataList.AddRange(cdLoader.GetClassData(newTypes, newTypes, typeof(IEndpointEnricher), "v2", "EndpointEnrichers"));

        classDataList.AddRange(cdLoader.GetClassData(newTypes, newTypes, typeof(ITool), "v1", "Tools"));

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
            $"/Reference/{classData.Architecture}/{classData.TypeName}/{classData.OptionsClassName}/"
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