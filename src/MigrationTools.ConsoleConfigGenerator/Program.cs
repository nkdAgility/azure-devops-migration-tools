using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.Common.Internal;
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
        private static string docsPath = "../../../../../docs/";
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
            ProcessAllFiles();
        }

        private static void ProcessAllFiles()
        {
            List<string> files = new List<string>
            {
                "index-template.md",
                "Reference/v1/index-template.md",
                "Reference/v2/index-template.md"
            };
            foreach (string file in files)
            {
                string templatemd = string.Empty;
                string filepath = System.IO.Path.Combine(docsPath, file);
                if (System.IO.File.Exists(filepath))
                {
                    templatemd = System.IO.File.ReadAllText(filepath);
                    templatemd = ProcessImports(templatemd, file.Contains("Reference") ? referencePath : docsPath   );
                    System.IO.File.WriteAllText(filepath.Replace("-template", ""), templatemd);
                }
                //ProcessImports
                Console.WriteLine(file);
            }
        }

        private static void Process(List<Type> targetTypes, List<Type> allTypes, Type type, string apiVersion, string folder, bool findConfig = true, string configEnd = "Options")
        {
            string masterTemplate = System.IO.Path.Combine(referencePath, "template.md");
            var founds = targetTypes.Where(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).OrderBy(t => t.Name).ToList();
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
            templatemd = ProcessBreadcrumbs(apiVersion, folder, null, templatemd);
            string typesTableMd = GetTypesTable(types, folder, apiVersion);
            File.WriteAllText(System.IO.Path.Combine(docsPath, $"table-{folder}-{apiVersion}.md"), typesTableMd);
            templatemd = templatemd.Replace("<ItemList>", typesTableMd);
            File.WriteAllText(System.IO.Path.Combine(referencePath, apiVersion, folder, "index.md"), templatemd);
        }

        private static void ProcessItemFile(List<Type> targetTypes, List<Type> allTypes, string apiVersion, string folder, string masterTemplate, Type item, bool findConfig = true, string configEnd = "Options")
        {
            Type typeOption = item;
            string objectName = item.Name;
            if (findConfig)
            {
                objectName = objectName.Replace("Context", "");
                typeOption = allTypes.Where(t => t.Name == $"{objectName}{configEnd}" && !t.IsAbstract && !t.IsInterface).SingleOrDefault();
            } else
            {
                Console.WriteLine("No config");
            }
            string templatemd = GetTemplate(apiVersion, folder, referencePath, masterTemplate, item);
            Console.WriteLine("Processing:" + item.Name);
            if (typeOption != null)
            {
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
                    options.SetExampleConfigDefaults();
                    targetItem = options;
                }
                if (targetItem != null)
                {
                    Console.WriteLine("targetItem");
                    JObject joptions = (JObject)JToken.FromObject(targetItem);
                    templatemd = ProcessOptions(targetItem, joptions, templatemd);
                    jsonSample = DeployJsonSample(targetItem, apiVersion, folder, referencePath, item);
                }
                templatemd = templatemd.Replace("<ExampleJson>", jsonSample);
            } else
            {
                templatemd = templatemd.Replace("<ExampleJson>", "Not currently runnable. Needs a little work");
            }
            templatemd = ProcessImports(templatemd, referencePath);
            templatemd = templatemd.Replace("<Description>", GetTypeData(item));
            templatemd = templatemd.Replace("<ClassName>", item.Name);
            templatemd = templatemd.Replace("<TypeName>", folder);
            templatemd = ProcessBreadcrumbs(apiVersion, folder, item, templatemd);
            string filename = $"../../../../../docs/Reference/{apiVersion}/{folder}/{item.Name}.md";
            File.WriteAllText(filename, templatemd);
        }

        private static string GetTypeData(Type item, string element = "summary")
        {
            string query = "missng XML code comments";
            try
            {
                // Query the data and write out a subset of contacts
                query = (from c in GetXDocument(item).Root.Descendants("member")
                                where c.Attribute("name").Value == $"T:{item.FullName}"
                                select c.Element(element).Value).SingleOrDefault();
            }
            catch (Exception)
            {
                
            }
            
            if (query != null)
            {
                Console.WriteLine($"- Description Loaded: {item.FullName}");
            }
            else
            {
                query = "missng XML code comments";
                // Console.WriteLine($"- Description FAILED: {item.FullName}");
            }
            return query.Replace(Environment.NewLine, "").Replace("\r", "").Replace("\n", "").Replace("            ", " ").Trim();
        }

        private static string GetPropertyData(object targetObject, JObject joptions, JProperty jproperty, string element)
        {
            return GetPropertyData(targetObject.GetType(), joptions, jproperty, element);
        }

        private static string GetPropertyData(Type targetType, JObject joptions, JProperty jproperty, string element)
        {
            var optionsType = targetType.GetProperty(jproperty.Name).DeclaringType;
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
            return query.Replace(Environment.NewLine, "").Replace("\r", "").Replace("\n", "").Replace("            ", " ").Trim();
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

            return defaultvalue.Replace(Environment.NewLine, "").Replace("\r", "").Replace("\n", "").Replace("            ", " ").Trim();
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
                var jpropertys = joptions.Properties().OrderBy(t => t.Name);
                foreach (JProperty jproperty in jpropertys)
                {
                    string PropertyValue = GetPropertyData(options, joptions, jproperty, "summary");
                    properties.AppendLine(string.Format("| {0} | {1} | {2} | {3} |", jproperty.Name, GetPropertyType(options, jproperty), PropertyValue, GetPropertyData(options, joptions, jproperty, "default")));
                }
                templatemd = templatemd.Replace("<Options>", properties.ToString());
            }
            else
            {
                templatemd = templatemd.Replace("<Options>", "Options not yet implmeneted");
            }
            return templatemd;
        }

        private static string GetTypesTable(List<Type> types, string typeCatagoryName, string apiVersion)
        {
            StringBuilder properties = new StringBuilder();
            properties.AppendLine($"| {typeCatagoryName} | Status | Target    | Usage                              |");
            properties.AppendLine("|------------------------|---------|---------|------------------------------------------|");
            foreach (var item in types)
            {

                string typeDocSummery = GetTypeData(item);
                string typeDocdDatatype = GetTypeData(item, "processingtarget");
                string typeDocdStatus = GetTypeData(item, "status");
                properties.AppendLine($"| [{item.Name}](/docs/Reference/{apiVersion}/{typeCatagoryName}/{item.Name}.md) | {typeDocdStatus} | {typeDocdDatatype} | {typeDocSummery} |");
            }
            return properties.ToString();
        }

        private static object GetPropertyType(object options, JProperty jproperty)
        {
            return options.GetType().GetProperty(jproperty.Name).PropertyType.Name.Replace("`1", "");
        }

        private static string ProcessImports(string templatemd, string referencePath)
        {
            var match = new Regex(@"<Import:(.*)>");
            MatchCollection matches = match.Matches(templatemd);
            foreach (Match item in matches)
            {
                string importPath = Path.Combine(referencePath, item.Groups[1].Value);
                string importFile = System.IO.File.ReadAllText(importPath);
                templatemd = templatemd.Replace(item.Value, importFile);
            }
            return templatemd;
        }

        private static string ProcessBreadcrumbs(string apiVersion, string folder, Type item, string templatemd)
        {
            string breadcrumbs = $"[Overview](/docs/index.md) > [Reference](/docs/Reference/index.md) > [API {apiVersion}](/docs/Reference/{apiVersion}/index.md) > [{folder}](/docs/Reference/{apiVersion}/{folder}/index.md)";
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