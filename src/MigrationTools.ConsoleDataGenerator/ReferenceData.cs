using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.CircuitBreaker;

namespace MigrationTools.ConsoleDataGenerator.ReferenceData
{
    public class ClassGroup
    {
        public ClassGroup() {
            Items = new List<DataItem>();
        }
        public string Name { get; set; }
        public List<DataItem> Items { get; set; }
    }
    public class DataItem
    {
        public ClassData classData { get; set; }
        public JekyllData jekyllData { get; set; }

        public DataItem()
        {
            classData = new ClassData();
            jekyllData = new JekyllData();
        }

    }

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
        public string? Architecture { get; set; }
        public List<OptionsItem> Options { get;  set; }
        public string Status { get; internal set; }
        public string ProcessingTarget { get; internal set; }
        public string ClassFile { get; internal set; }
        public string OptionsClassFile { get; internal set; }
    }

    public class JekyllData
    {
        public List<string> Redirect_from { get;  set; }
        public string layout { get;  set; }
        public bool toc { get;  set; }
        public string Permalink { get;  set; }
        public string title { get; internal set; }
        public List<string> categories { get; internal set; }
        public string notes { get; internal set; }
        public string introduction { get; internal set; }

        public JekyllData()
        {
            Redirect_from = new List<string>();
            categories = new List<string>();
        }
       
    }

    public class OptionsItem
    {
        public string ParameterName { get; internal set; }
        public object Type { get; internal set; }
        public object Description { get; internal set; }
        public object DefaultValue { get; internal set; }
    }

    public class ConfigurationSample
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string SampleFor { get;  set; }
    }
}
