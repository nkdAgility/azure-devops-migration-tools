using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using MigrationTools.Options;
using System.Reflection;

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
            string query = "missing XML code comments";
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
                query = "missing XML code comments";
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
            var propertyInfo = targetType.GetProperty(jproperty.Name);
            var optionsType = propertyInfo.DeclaringType;
            
            // First try to get documentation from the declaring type
            var query = GetPropertyDocumentationFromType(optionsType, jproperty.Name, element);
            
            // If not found, traverse inheritance hierarchy and interfaces
            if (query == null)
            {
                query = GetPropertyDocumentationFromInheritanceChain(propertyInfo, jproperty.Name, element);
            }
            
            if (query != null)
            {
                Console.WriteLine($"- - {element} Loaded: {jproperty.Name}");
            }
            else
            {
                // Console.WriteLine($"- Description FAILED: {item.FullName}");
                query = "missing XML code comments";
            }
            return query.Replace(Environment.NewLine, "").Replace("\r", "").Replace("\n", "").Replace("            ", " ").Trim();
        }

        public string GetPropertyDefault(IOptions options, JObject joptions, JProperty jproperty)
        {
            var propertyInfo = options.GetType().GetProperty(jproperty.Name);
            var optionsType = propertyInfo.DeclaringType;
            
            // First try to get default from the declaring type
            var properyXml = GetPropertyXmlFromType(optionsType, jproperty.Name);
            
            // If not found, traverse inheritance hierarchy and interfaces
            if (properyXml == null)
            {
                properyXml = GetPropertyXmlFromInheritanceChain(propertyInfo, jproperty.Name);
            }
            
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
                defaultvalue = "missing XML code comments";
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

        /// <summary>
        /// Gets property documentation from a specific type's XML documentation
        /// </summary>
        private string GetPropertyDocumentationFromType(Type type, string propertyName, string element)
        {
            try
            {
                var query = (from c in GetXDocument(type).Root.Descendants("member")
                             where c.Attribute("name").Value == $"P:{type.FullName}.{propertyName}"
                             select c.Element(element)?.Value).SingleOrDefault();
                return query;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets property XML element from a specific type's XML documentation
        /// </summary>
        private XElement GetPropertyXmlFromType(Type type, string propertyName)
        {
            try
            {
                var properyXml = (from c in GetXDocument(type).Root.Descendants("member")
                                  where c.Attribute("name").Value == $"P:{type.FullName}.{propertyName}"
                                  select c).SingleOrDefault();
                return properyXml;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Traverses inheritance hierarchy and interfaces to find property documentation
        /// </summary>
        private string GetPropertyDocumentationFromInheritanceChain(System.Reflection.PropertyInfo propertyInfo, string propertyName, string element)
        {
            var declaringType = propertyInfo.DeclaringType;
            
            // Check interfaces first
            foreach (var interfaceType in declaringType.GetInterfaces())
            {
                // Check if this interface defines the property
                var interfaceProperty = interfaceType.GetProperty(propertyName);
                if (interfaceProperty != null)
                {
                    var documentation = GetPropertyDocumentationFromType(interfaceType, propertyName, element);
                    if (documentation != null)
                    {
                        return documentation;
                    }
                }
            }
            
            // Check base classes
            var baseType = declaringType.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                var baseProperty = baseType.GetProperty(propertyName);
                if (baseProperty != null)
                {
                    var documentation = GetPropertyDocumentationFromType(baseType, propertyName, element);
                    if (documentation != null)
                    {
                        return documentation;
                    }
                }
                baseType = baseType.BaseType;
            }
            
            return null;
        }

        /// <summary>
        /// Traverses inheritance hierarchy and interfaces to find property XML element
        /// </summary>
        private XElement GetPropertyXmlFromInheritanceChain(System.Reflection.PropertyInfo propertyInfo, string propertyName)
        {
            var declaringType = propertyInfo.DeclaringType;
            
            // Check interfaces first
            foreach (var interfaceType in declaringType.GetInterfaces())
            {
                // Check if this interface defines the property
                var interfaceProperty = interfaceType.GetProperty(propertyName);
                if (interfaceProperty != null)
                {
                    var xmlElement = GetPropertyXmlFromType(interfaceType, propertyName);
                    if (xmlElement != null)
                    {
                        return xmlElement;
                    }
                }
            }
            
            // Check base classes
            var baseType = declaringType.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                var baseProperty = baseType.GetProperty(propertyName);
                if (baseProperty != null)
                {
                    var xmlElement = GetPropertyXmlFromType(baseType, propertyName);
                    if (xmlElement != null)
                    {
                        return xmlElement;
                    }
                }
                baseType = baseType.BaseType;
            }
            
            return null;
        }

    }
}
