using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Helpers;
using MigrationTools.Options;
using MigrationTools.Processors;
using Newtonsoft.Json;

namespace VstsSyncMigrator.ConsoleApp
{
    //https://gist.github.com/formix/515d3d11ee7c1c252f92
    public class Program
    {
        public static AppDomain domain = AppDomain.CreateDomain("MigrationTools");
        private static string referencePath = "../../../../../docs/Reference/";

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
            Console.WriteLine("--------------------------");
            Console.WriteLine("---------EndpointEnrichers");
            Process(types, typeof(IEndpointEnricher), "EndpointEnrichers");
            Console.WriteLine("--------------------------");
            Console.WriteLine("---------Endpoints");
            Process(types, typeof(IEndpoint), "Endpoints");
            Console.WriteLine("--------------------------");
            Console.WriteLine("---------ProcessorEnrichers");
            Process(types, typeof(IProcessorEnricher), "ProcessorEnrichers");
            Console.WriteLine("--------------------------");
            Console.WriteLine("---------Processors");
            Process(types, typeof(IProcessor), "Processors");
            Console.WriteLine("--------------------------");
        }

        private static void Process(List<Type> types, Type type, string folder)
        {
            string masterTemplate = System.IO.Path.Combine(referencePath, "template.md");
            var founds = types.Where(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).ToList();
            foreach (var item in founds)
            {
                Console.WriteLine("Processing:" + item.Name);
                var jsonSample = DeployJsonSample(types, type, folder, referencePath, item);
                string templatemd = GetTemplate(folder, referencePath, masterTemplate, item);

                templatemd = templatemd.Replace("<ClassName>", item.Name);
                templatemd = templatemd.Replace("<TypeName>", folder);
                templatemd = ProcessBreadcrumbs(folder, item, templatemd);
                templatemd = templatemd.Replace("<Description>", GetTypeSummary(item));
                templatemd = ProcessOptions(types, item, templatemd);
                templatemd = ProcessSamples(jsonSample, templatemd, referencePath);
                File.WriteAllText(string.Format("../../../../../docs/Reference/{0}/{1}.md", folder, item.Name), templatemd);
            }
        }

        private static string GetTypeSummary(Type item)
        {
            // Query the data and write out a subset of contacts
            var query = (from c in GetXDocument(item).Root.Descendants("member")
                         where c.Attribute("name").Value == $"T:{item.FullName}"
                         select c.Element("summary").Value).SingleOrDefault();
            if (query != null)
            {
                Console.WriteLine($"- Description Loaded: {item.FullName}");
            }
            else
            {
                query = "missng XML code comments";
                // Console.WriteLine($"- Description FAILED: {item.FullName}");
            }
            return query.Replace(Environment.NewLine, "").Trim();
        }

        private static string GetPropertySummary(PropertyInfo property)
        {
            // Query the data and write out a subset of contacts
            var query = (from c in GetXDocument(property.DeclaringType).Root.Descendants("member")
                         where c.Attribute("name").Value == $"P:{property.DeclaringType.FullName}.{property.Name}"
                         select c.Element("summary").Value).SingleOrDefault();
            if (query != null)
            {
                Console.WriteLine($"- - Proptery Loaded: {property.Name}");
            }
            else
            {
                // Console.WriteLine($"- Description FAILED: {item.FullName}");
                query = "missng XML code comments";
            }
            return query.Replace(Environment.NewLine, "").Trim();
        }

        private static string GetPropertyDefault(PropertyInfo property)
        {
            // Query the data and write out a subset of contacts
            var properyXml = (from c in GetXDocument(property.DeclaringType).Root.Descendants("member")
                              where c.Attribute("name").Value == $"P:{property.DeclaringType.FullName}.{property.Name}"
                              select c).SingleOrDefault();
            string defaultvalue = null;
            if (properyXml != null)
            {
                defaultvalue = properyXml.Element("default")?.Value;
            }

            if (!string.IsNullOrEmpty(defaultvalue))
            {
                Console.WriteLine($"- - Default Loaded: {property.Name}");
            }
            else
            {
                // Console.WriteLine($"- Description FAILED: {item.FullName}");
                defaultvalue = "missng XML code comments";
            }

            return defaultvalue.Replace(Environment.NewLine, "").Trim();
        }

        private static XDocument GetXDocument(Type item)
        {
            string xmlDataPath = Path.Combine(referencePath, "Generated", string.Format($"{item.Assembly.GetName().Name}.xml"));
            return XDocument.Load(xmlDataPath);
        }

        private static string ProcessOptions(List<Type> types, Type item, string templatemd)
        {
            var typeOption = types.Where(t => t.Name == string.Format("{0}Options", item.Name) && !t.IsAbstract && !t.IsInterface).SingleOrDefault();
            StringBuilder options = new StringBuilder();
            if (!(typeOption is null))
            {
                options.AppendLine("| Parameter name         | Type    | Description                              | Default Value                            |");
                options.AppendLine("|------------------------|---------|------------------------------------------|------------------------------------------|");
                var propertys = typeOption.GetProperties().Where(p => p.CanWrite);
                foreach (PropertyInfo property in propertys)
                {
                    options.AppendLine(string.Format("| {0} | {1} | {2} | {3} |", property.Name, property.PropertyType.Name.Replace("`1", ""), GetPropertySummary(property), GetPropertyDefault(property)));
                }
                templatemd = templatemd.Replace("<Options>", options.ToString());
            }
            else
            {
                templatemd = templatemd.Replace("<Options>", "Options not yet implmeneted");
            }
            return templatemd;
        }

        private static string ProcessSamples(string jsonSample, string templatemd, string referencePath)
        {
            templatemd = templatemd.Replace("<ExampleJson>", jsonSample);
            var match = new Regex(@"<Import:([\s\S]*)>");
            MatchCollection matches = match.Matches(templatemd);
            foreach (Match item in matches)
            {
                string importPath = Path.Combine(referencePath, item.Value);
                string importFile = System.IO.File.ReadAllText(importPath);
                templatemd.Replace(string.Format($"<Import:{item.Value}>"), importFile);
            }
            return templatemd;
        }

        private static string ProcessBreadcrumbs(string folder, Type item, string templatemd)
        {
            string breadcrumbs = $"[Overview](.././index.md) > [Reference](../index.md) > [{folder}](./index.md) > **{item.Name}**";
            templatemd = templatemd.Replace("<Breadcrumbs>", breadcrumbs);
            return templatemd;
        }

        private static string GetTemplate(string folder, string referencePath, string masterTemplate, Type item)
        {
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

            return templatemd;
        }

        private static string DeployJsonSample(List<Type> types, Type type, string folder, string referencePath, Type item)
        {
            var typeOption = types.Where(t => t.Name == string.Format("{0}Options", item.Name) && !t.IsAbstract && !t.IsInterface).SingleOrDefault();
            string json;
            if (!(typeOption is null))
            {
                var instance = (IOptions)Activator.CreateInstance(typeOption);
                instance.SetDefaults();
                json = NewtonsoftHelpers.SerializeObject(instance, TypeNameHandling.Objects);
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