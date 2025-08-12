using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.CircuitBreaker;

namespace MigrationTools.ConsoleDataGenerator.ReferenceData
{
    // Removed ClassGroup and DataItem classes as they are no longer needed
    // since we're only generating YAML files and not markdown

    public class ClassData
    {
        public ClassData()
        {
            Options = new List<OptionsItem>();
            ConfigurationSamples = new List<ConfigurationSample>();
        }

        public string? OptionsClassName { get; set; }
        public string? OptionsClassFullName { get; set; }
        public List<ConfigurationSample> ConfigurationSamples { get; set; }
        public string? Description { get; set; }
        public string? ClassName { get; set; }
        public string? TypeName { get; set; }
        public List<OptionsItem> Options { get; set; }
        public string Status { get; internal set; }
        public string ProcessingTarget { get; internal set; }
        public string ClassFile { get; internal set; }
        public string OptionsClassFile { get; internal set; }
        // Removed Notes property as we no longer handle notes files
    }

    // Removed ClassGroup, DataItem, JekyllData, and NotesInfo classes as they are no longer needed
    // since we're only generating YAML files and not markdown

    public class OptionsItem
    {
        public string ParameterName { get; internal set; }
        public object Type { get; internal set; }
        public object Description { get; internal set; }
        public object DefaultValue { get; internal set; }
        public bool IsRequired { get; set; } // Add this property to track required fields
    // Holds the actual System.Type for richer schema generation (not serialized to YAML/JSON samples directly)
    public Type DotNetType { get; internal set; }
    }

    public class ConfigurationSample
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string SampleFor { get; set; }
    }
}
