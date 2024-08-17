using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using MigrationTools.Options;

namespace MigrationTools.ConsoleDataGenerator
{
    public class CodeDocumentation
    {
        string documentationPath;

        public CodeDocumentation(string path) {
            documentationPath = path;
        }


        public string GetTypeData(Type item, string element = "summary")
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

        public string GetPropertyData(object targetObject, JObject joptions, JProperty jproperty, string element)
        {
            return GetPropertyData(targetObject.GetType(), joptions, jproperty, element);
        }

        public string GetPropertyData(Type targetType, JObject joptions, JProperty jproperty, string element)
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

        public string GetPropertyDefault(IOldOptions options, JObject joptions, JProperty jproperty)
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

        public object GetPropertyType(object options, JProperty jproperty)
        {
            return options.GetType().GetProperty(jproperty.Name).PropertyType.Name.Replace("`1", "").Replace("`2", "").Replace("`", "");
        }

        internal XDocument GetXDocument(Type item)
        {
            string xmlDataPath = Path.Combine(documentationPath, string.Format($"{item.Assembly.GetName().Name}.xml"));
            return XDocument.Load(xmlDataPath);
        }

    }
}
