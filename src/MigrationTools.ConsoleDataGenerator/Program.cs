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


        // V1
        //ClassGroup v1Processors = cdLoader.CreateClassGroup(oldTypes, allTypes, typeof(MigrationTools._EngineV1.Containers.IProcessor), "v1", "Processors", true, "Config");

        BuildJekyllDataFile(oldTypes, allTypes, typeof(MigrationTools._EngineV1.Containers.IProcessor), "v1", "Processors", true, "Config");
        BuildJekyllDataFile(newTypes, allTypes, typeof(IFieldMapConfig), "v1", "FieldMaps", false);
        // V2
        BuildJekyllDataFile(oldTypes, allTypes, typeof(MigrationTools._EngineV1.Containers.IProcessor), "v1", "Processors", true, "Config");
        BuildJekyllDataFile(newTypes, allTypes, typeof(IProcessorEnricher), "v2", "ProcessorEnrichers");
        BuildJekyllDataFile(newTypes, allTypes, typeof(IFieldMapConfig), "v2", "FieldMaps", false);
        BuildJekyllDataFile(newTypes, allTypes, typeof(IEndpoint), "v2", "Endpoints");
        BuildJekyllDataFile(newTypes, allTypes, typeof(IEndpointEnricher), "v2", "EndpointEnrichers");
    }

    private static void BuildJekyllDataFile(List<Type> targetTypes, List<Type> allTypes, Type type, string apiVersion, string dataTypeName, bool findConfig = true, string configEnd = "Options")
    {
       Console.WriteLine();
        Console.WriteLine($"BuildJekyllDataFile: {dataTypeName}");
        ClassGroup data = new ClassGroup();
        data.Name = dataTypeName;
        var founds = targetTypes.Where(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface && t.IsPublic).OrderBy(t => t.Name).ToList();
        Console.WriteLine($"----------- Found {founds.Count}");

        // Each File
        foreach (var item in founds)
        {
           Console.WriteLine($"-PROCESS {item.Name}");
            PopulateReferenceData(ref data, targetTypes, allTypes, apiVersion, dataTypeName, item, findConfig, configEnd);
        }
        saveData.SeraliseData(data, apiVersion, dataTypeName);
        Console.WriteLine("-----------");

    }


    private static void PopulateReferenceData(ref ClassGroup data, List<Type> targetTypes, List<Type> allTypes, string apiVersion, string dataTypeName, Type item, bool findConfig = true, string configEnd = "Options")
    {
        Type typeOption = item;
        string objectName = item.Name;
        DataItem dataItem = new DataItem();
        dataItem.classData.ClassName = item.Name;
        dataItem.classData.TypeName = dataTypeName;
        dataItem.classData.Architecture = apiVersion;
        dataItem.jekyllData.Permalink = $"/Reference/{apiVersion}/{dataItem.classData.TypeName}/{dataItem.classData.ClassName}/";

        if (findConfig)
        {
            objectName = objectName.Replace("Context", "");
            typeOption = allTypes.Where(t => t.Name == $"{objectName}{configEnd}" && !t.IsAbstract && !t.IsInterface).SingleOrDefault();
            
        }
        else
        {
            dataItem.classData.OptionsClassName = "";
            dataItem.classData.OptionsClassFullName = "";
            Console.WriteLine("No config");
        }


        if (typeOption != null)
        {
            string posibleOldUrl = $"/Reference/{apiVersion}/{dataItem.classData.TypeName}/{typeOption.Name}/";
            if (posibleOldUrl != dataItem.jekyllData.Permalink)
            {
                dataItem.jekyllData.Redirect_from.Add(posibleOldUrl);
            }            
            dataItem.classData.OptionsClassFullName = typeOption.FullName;
            dataItem.classData.OptionsClassName = typeOption.Name;
            object targetItem = null;
            if (typeOption.GetInterfaces().Contains(typeof(IProcessorConfig)))
            {
                Console.WriteLine("Processing as IProcessorConfig");
                var options = (IProcessorConfig)Activator.CreateInstance(typeOption);
                targetItem = options;
            }
            if (typeOption.GetInterfaces().Contains(typeof(IOptions)))
            {
                Console.WriteLine("Processing as IOptions");
                var options = (IOptions)Activator.CreateInstance(typeOption);
                options.SetDefaults();
                targetItem = options;
            }
            if (typeOption.GetInterfaces().Contains(typeof(IFieldMapConfig)))
            {
                Console.WriteLine("Processing as IFieldMapConfig");
                var options = (IFieldMapConfig)Activator.CreateInstance(typeOption); 
                options.SetExampleConfigDefaults();
                targetItem = options;
            }
            if (targetItem != null)
            {
                Console.WriteLine("targetItem");
                JObject joptions = (JObject)JToken.FromObject(targetItem);

                dataItem.classData.Options = populateOptions(targetItem, joptions);
                dataItem.classData.ConfigurationSamples.Add(new ConfigurationSample() { Name = "default", SampleFor = dataItem.classData.OptionsClassFullName, Sample = saveData.SeraliseDataToJson(targetItem) });
            }

        }
        else
        {

        }
        dataItem.classData.Description = codeDocs.GetTypeData(item);

        saveData.SeraliseData(dataItem, apiVersion, dataTypeName);
        data.Items.Add(dataItem);
    }

    private static List<OptionsItem> populateOptions(object item, JObject joptions)
    {
        List<OptionsItem> options = new List<OptionsItem>();
        if (!(joptions is null))
        {
            var jpropertys = joptions.Properties().OrderBy(t => t.Name);
            foreach (JProperty jproperty in jpropertys)
            {
                OptionsItem optionsItem = new OptionsItem();
                optionsItem.ParameterName = jproperty.Name;
                optionsItem.Type = codeDocs.GetPropertyType(item, jproperty);
                optionsItem.Description = codeDocs.GetPropertyData(item, joptions, jproperty, "summary");
                optionsItem.DefaultValue = codeDocs.GetPropertyData(item, joptions, jproperty, "default");
                options.Add(optionsItem);
            }
        }
        return options;
    }



}