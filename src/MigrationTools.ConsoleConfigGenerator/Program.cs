using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using MigrationTools.Processors;
using MigrationTools.Tests;

namespace VstsSyncMigrator.ConsoleApp
{
    //https://gist.github.com/formix/515d3d11ee7c1c252f92
    public class Program
    {
        public static AppDomain domain = AppDomain.CreateDomain("MigrationTools");

        public static async Task Main(string[] args)
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;

            domain.Load("MigrationTools");
            domain.Load("MigrationTools.Clients.AzureDevops.ObjectModel");
            domain.Load("MigrationTools.Clients.FileSystem");
            //AppDomain.CurrentDomain.Load("MigrationTools.Clients.AzureDevops.Rest");
            List<Type> types = domain.GetAssemblies()
                  .Where(a => !a.IsDynamic && a.FullName.StartsWith("MigrationTools"))
                  .SelectMany(a => a.GetTypes()).ToList();
            Process(types, typeof(IEndpointEnricher), "EndpointEnrichers");
            Process(types, typeof(IProcessorEnricher), "ProcessorEnrichers");
            Process(types, typeof(IProcessor), "Processors");
        }

        private static void Process(List<Type> types, Type type, string folder)
        {
            string referencePath = "../../../../../docs/v2/Reference/";
            string masterTemplate = System.IO.Path.Combine(referencePath, "template.md");
            var founds = types.Where(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).ToList();
            foreach (var item in founds)
            {
                var jsonSample = DeployJsonSample(types, type, folder, referencePath, item);

                string typeTemplatename = string.Format("{0}-template.md", item.Name);
                string templateFile = Path.Combine(referencePath, folder, typeTemplatename);
                string templatemd;
                if (System.IO.File.Exists(templateFile))
                {
                    templatemd = System.IO.File.ReadAllText(templateFile);
                }
                else
                {
                    templatemd = System.IO.File.ReadAllText(masterTemplate);
                }

                templatemd = templatemd.Replace("<ClassName>", item.Name);
                templatemd = templatemd.Replace("<Description>", "No description, create a template");
                templatemd = templatemd.Replace("<Options>", "Options not yet implmeneted");
                templatemd = templatemd.Replace("<ExampleJson>", jsonSample);
                //
                File.WriteAllText(string.Format("../../../../../docs/v2/Reference/{0}/{1}.md", folder, item.Name), templatemd);
            }
        }

        private static string DeployJsonSample(List<Type> types, Type type, string folder, string referencePath, Type item)
        {
            var typeOption = types.Where(t => t.Name == string.Format("{0}Options", item.Name) && !t.IsAbstract && !t.IsInterface).SingleOrDefault();
            string json;
            if (!(typeOption is null))
            {
                var instance = (IOptions)Activator.CreateInstance(typeOption);
                instance.SetDefaults();
                json = TestHelpers.SaveObjectAsJson(instance);
                string jsonFilename = string.Format("{0}.json", item.Name);
                string jsonFilePath = Path.Combine(referencePath, folder, jsonFilename);
                File.WriteAllText(jsonFilePath, json);
            }
            else
            {
                json = "!No Support for V1";
            }
            return json;
        }
    }
}