using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Helpers;
using MigrationTools.Options;
using MigrationTools.Processors;
using MigrationTools.Tests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VstsSyncMigrator.ConsoleApp
{
    //https://gist.github.com/formix/515d3d11ee7c1c252f92
    public class Program
    {
        public static AppDomain domain = AppDomain.CreateDomain("MigrationTools");
        private static string referencePath = "../../../../../docs/Reference/";

        public static void Main(string[] args)
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;

            domain.Load("MigrationTools");
            domain.Load("MigrationTools.Clients.AzureDevops.ObjectModel");
            domain.Load("MigrationTools.Clients.AzureDevops.Rest");
            domain.Load("MigrationTools.Clients.FileSystem");
            domain.Load("VstsSyncMigrator.Core");
            //AppDomain.CurrentDomain.Load("MigrationTools.Clients.AzureDevops.Rest");

            List<Type> newTypes = domain.GetAssemblies()
                  .Where(a => !a.IsDynamic && a.FullName.StartsWith("MigrationTools"))
                  .SelectMany(a => a.GetTypes()).ToList();
            List<Type> oldTypes = domain.GetAssemblies()
                 .Where(a => !a.IsDynamic && a.FullName.StartsWith("VstsSyncMigrator"))
                 .SelectMany(a => a.GetTypes()).ToList();
            List<Type> allTypes = newTypes.Concat(oldTypes).ToList();


            Console.WriteLine("--------------------------");
            Console.WriteLine("---------EndpointEnrichers");
            Process(newTypes, allTypes, typeof(IEndpointEnricher), "v2", "EndpointEnrichers");
            Console.WriteLine("--------------------------");
            Console.WriteLine("---------Endpoints");
            // Process(types, typeof(IEndpoint), "Endpoints");
            Console.WriteLine("--------------------------");
            Console.WriteLine("---------ProcessorEnrichers");
            Process(newTypes, allTypes, typeof(IProcessorEnricher), "v2", "ProcessorEnrichers");
            Console.WriteLine("--------------------------");
            Console.WriteLine("---------Processors");
            Process(newTypes, allTypes, typeof(MigrationTools.Processors.IProcessor), "v2", "Processors");
            Process(oldTypes, allTypes, typeof(MigrationTools._EngineV1.Containers.IProcessor), "v1", "Processors", true, "Config");
            Console.WriteLine("--------------------------");
            Console.WriteLine("---------FieldMaps");
            Process(newTypes, allTypes, typeof(IFieldMapConfig), "v1", "FieldMaps", false);
            Process(newTypes, allTypes, typeof(IFieldMapConfig), "v2", "FieldMaps", false);
            Console.WriteLine("--------------------------");
        }

        private static void Process(List<Type> targetTypes, List<Type> allTypes, Type type, string apiVersion, string folder, bool findConfig = true, string configEnd = "Options")
        {
            string masterTemplate = System.IO.Path.Combine(referencePath, "template.md");
            var founds = targetTypes.Where(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).ToList();
            ProcessIndexFile(founds, apiVersion, folder, masterTemplate);
            // Each File
            foreach (var item in founds)
            {
                ProcessItemFile(targetTypes, allTypes, apiVersion, folder, masterTemplate, item, findConfig, configEnd);
            }
        }

        private static void ProcessIndexFile(List<Type> types, string apiVersion, string folder, string masterTemplate)
        {
            string templatemd = GetTemplate(apiVersion, folder, referencePath, masterTemplate, null);
            Console.WriteLine("Processing: index.md");
            templatemd = ProcessBreadcrumbs(folder, null, templatemd);
            templatemd = ProcessTypes(types, templatemd, folder);
            string filename = $"../../../../../docs/Reference/{apiVersion}/{folder}/index.md";
            File.WriteAllText(filename, templatemd);
        }

        private static void ProcessItemFile(List<Type> targetTypes, List<Type> allTypes, string apiVersion, string folder, string masterTemplate, Type item, bool findConfig = true, string configEnd = "Options")
        {
            Type typeOption = item;

            if (findConfig)
            {
                string objectName = item.Name;
                objectName = objectName.Replace("Context", "");
                typeOption = allTypes.Where(t => t.Name == $"{objectName}{configEnd}" && !t.IsAbstract && !t.IsInterface).SingleOrDefault();
            }

            if (typeOption != null)
            {
                string templatemd = GetTemplate(apiVersion, folder, referencePath, masterTemplate, item);
                Console.WriteLine("Processing:" + item.Name);
                string jsonSample = "";
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
                    targetItem = options;
                }
                if (targetItem != null)
                {
                    Console.WriteLine("targetItem");
                    JObject joptions = (JObject)JToken.FromObject(targetItem);
                    templatemd = ProcessOptions(targetItem, joptions, templatemd);
                    jsonSample = DeployJsonSample(targetItem, apiVersion, folder, referencePath, item);

                }
                templatemd = templatemd.Replace("<Description>", GetTypeSummary(item));
                templatemd = ProcessSamples(jsonSample, templatemd, referencePath);
                templatemd = templatemd.Replace("<ClassName>", item.Name);
                templatemd = templatemd.Replace("<TypeName>", folder);
                templatemd = ProcessBreadcrumbs(folder, item, templatemd);
                string filename = $"../../../../../docs/Reference/{apiVersion}/{folder}/{item.Name}.md";
                File.WriteAllText(filename, templatemd);
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

        private static string GetPropertyData(object options, JObject joptions, JProperty jproperty, string element)
        {
            var optionsType = options.GetType().GetProperty(jproperty.Name).DeclaringType;
            // Query the data and write out a subset of contacts
            var query = (from c in GetXDocument(optionsType).Root.Descendants("member")
                         where c.Attribute("name").Value == $"P:{optionsType.FullName}.{jproperty.Name}"
                         select c.Element(element)?.Value).SingleOrDefault();
            if (query != null)
            {
                Console.WriteLine($"- - {element} Loaded: {jproperty.Name}");
            }
            else
            {
                // Console.WriteLine($"- Description FAILED: {item.FullName}");
                query = "missng XML code comments";
            }
            return query.Replace(Environment.NewLine, "").Trim();
        }

        private static string GetPropertyDefault(IOptions options, JObject joptions, JProperty jproperty)
        {
            var optionsType = options.GetType().GetProperty(jproperty.Name).DeclaringType;
            // Query the data and write out a subset of contacts
            var properyXml = (from c in GetXDocument(optionsType).Root.Descendants("member")
                              where c.Attribute("name").Value == $"P:{optionsType.FullName}.{jproperty.Name}"
                              select c).SingleOrDefault();
            string defaultvalue = null;
            if (properyXml != null)
            {
                defaultvalue = properyXml.Element("default")?.Value;
            }

            if (!string.IsNullOrEmpty(defaultvalue))
            {
                Console.WriteLine($"- - Default Loaded: {jproperty.Name}");
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

        private static string ProcessOptions(object options, JObject joptions, string templatemd)
        {
            StringBuilder properties = new StringBuilder();
            if (!(joptions is null))
            {
                properties.AppendLine("| Parameter name         | Type    | Description                              | Default Value                            |");
                properties.AppendLine("|------------------------|---------|------------------------------------------|------------------------------------------|");
                var jpropertys = joptions.Properties();
                foreach (JProperty jproperty in jpropertys)
                {
                    properties.AppendLine(string.Format("| {0} | {1} | {2} | {3} |", jproperty.Name, GetPropertyType(options, jproperty), GetPropertyData(options, joptions, jproperty, "summary"), GetPropertyData(options, joptions, jproperty, "default")));
                }
                templatemd = templatemd.Replace("<Options>", properties.ToString());
            }
            else
            {
                templatemd = templatemd.Replace("<Options>", "Options not yet implmeneted");
            }
            return templatemd;
        }

        private static string ProcessTypes(List<Type> types, string templatemd, string typeCatagoryName)
        {
            StringBuilder properties = new StringBuilder();
            properties.AppendLine($"| {typeCatagoryName} | Data Type    | Description                              | Default Value                            |");
            properties.AppendLine("|------------------------|---------|------------------------------------------|------------------------------------------|");
            foreach (var item in types)
            {
                //JObject joptions = (JObject)JToken.FromObject(item);
                //var jproperty = joptions.Properties();
                properties.AppendLine(string.Format("| [{0}](/{0}.md) | {1} | {2} | {3} |", item.Name, "", "", ""));
            }
            templatemd = templatemd.Replace("<ItemList>", properties.ToString());
            return templatemd;
        }

        private static object GetPropertyType(object options, JProperty jproperty)
        {
            return options.GetType().GetProperty(jproperty.Name).PropertyType.Name.Replace("`1", "");
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
                templatemd = templatemd.Replace(string.Format($"<Import:{item.Value}>"), importFile);
            }
            return templatemd;
        }

        private static string ProcessBreadcrumbs(string folder, Type item, string templatemd)
        {
            string breadcrumbs = $"[Overview](.././index.md) > [Reference](../index.md) > [{folder}](./index.md)";
            if (item != null)
            {
                breadcrumbs += $"> **{item.Name}**";
            }
            templatemd = templatemd.Replace("<Breadcrumbs>", breadcrumbs);
            return templatemd;
        }

        private static string GetTemplate( string apiVersion,string folder, string referencePath, string masterTemplate, Type item)
        {
            string typeTemplatename = string.Format("{0}-template.md", item != null ? item.Name : "index");
            string templateFile = Path.Combine(referencePath, apiVersion, folder, typeTemplatename);
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

        private static string DeployJsonSample(object options, string apiVersion, string folder, string referencePath, Type item)
        {
            string json;
            json = NewtonsoftHelpers.SerializeObject(options, TypeNameHandling.Objects);
            string jsonFilename = string.Format("{0}.json", item.Name);
            string jsonFilePath = Path.Combine(referencePath, apiVersion, folder, jsonFilename);
            File.WriteAllText(jsonFilePath, json.Replace(TestingConstants.AccessToken, "6i4jyylsadahtdjniaydxnjsi4zsz3qsword2y5ngzzsdfewaostq"));
            return json;
        }
    }
}