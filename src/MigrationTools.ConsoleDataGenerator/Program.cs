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

namespace MigrationTools.ConsoleDataGenerator;
class Program
{
    private static DataSerialization saveData = new DataSerialization("../../../../../docs/_data/");
    private static CodeDocumentation codeDocs = new CodeDocumentation("../../../../../docs/Reference/Generated/");
    private static ClassDataLoader cdLoader = new ClassDataLoader(saveData);

    static void Main(string[] args)
    {
        string dir = AppDomain.CurrentDomain.BaseDirectory;
        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.Load("MigrationTools");
        currentDomain.Load("MigrationTools.Clients.AzureDevops.ObjectModel");
        currentDomain.Load("MigrationTools.Clients.AzureDevops.Rest");
        currentDomain.Load("MigrationTools.Clients.FileSystem");
        currentDomain.Load("VstsSyncMigrator.Core");

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

        List<Type> oldTypes = asses
             .Where(a => !a.IsDynamic && a.FullName.StartsWith("VstsSyncMigrator"))
             .SelectMany(a => a.GetTypes()).ToList();

        List<Type> allTypes = newTypes.Concat(oldTypes).ToList();


        List<ClassData> classDataList = new List<ClassData>();
        // V1
        classDataList.AddRange(cdLoader.GetClassData(oldTypes, allTypes, typeof(MigrationTools._EngineV1.Containers.IProcessor), "v1", "Processors", true, "Config"));
        classDataList.AddRange(cdLoader.GetClassData(newTypes, allTypes, typeof(IFieldMapConfig), "v1", "FieldMaps", false));
        // V2
        classDataList.AddRange(cdLoader.GetClassData(oldTypes, allTypes, typeof(MigrationTools._EngineV1.Containers.IProcessor), "v1", "Processors", true, "Config"));
        classDataList.AddRange(cdLoader.GetClassData(newTypes, allTypes, typeof(IProcessorEnricher), "v2", "ProcessorEnrichers"));
        classDataList.AddRange(cdLoader.GetClassData(newTypes, allTypes, typeof(IFieldMapConfig), "v2", "FieldMaps", false));
        classDataList.AddRange(cdLoader.GetClassData(newTypes, allTypes, typeof(IEndpoint), "v2", "Endpoints"));
        classDataList.AddRange(cdLoader.GetClassData(newTypes, allTypes, typeof(IEndpointEnricher), "v2", "EndpointEnrichers"));

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
        data.Permalink = $"/Reference/{classData.Architecture}/{classData.TypeName}/{classData.ClassName}/";
        data.layout = "reference";
        data.toc = true;
        data.title = classData.ClassName;
        data.categories.Add(classData.TypeName);
        data.categories.Add(classData.Architecture);
        data.notes = LoadMarkdown(classData, "notes");
        data.introduction = LoadMarkdown(classData, "introduction");
        string posibleOldUrl = $"/Reference/{classData.Architecture}/{classData.TypeName}/{classData.OptionsClassName}/";
        if (posibleOldUrl != data.Permalink)
        {
           // data.Redirect_from.Add(posibleOldUrl);
        }
        return data;
    }

    private static string LoadMarkdown(ClassData classData, string topic)
    {
        string notesLocation = "../../../../../docs/Reference/";
        string noteFile = System.IO.Path.Combine(notesLocation, $"{classData.Architecture}/{classData.TypeName}/{classData.ClassName}-{topic}.md");
        string notes = "";
        Console.WriteLine(noteFile);
        if (System.IO.File.Exists(noteFile))
        {
            notes = System.IO.File.ReadAllText(noteFile);
            Console.Write($"[{topic} ]");

        } else
        {
            Console.WriteLine($"[{topic} missing]");
        }
        return notes;
    }

  


}