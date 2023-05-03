using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.CircuitBreaker;

namespace MigrationTools.ConsoleDataGenerator
{
    public class ReferenceDataGroup
    {
        public ReferenceDataGroup() {
            Items = new List<ReferenceDataItem>();
        }
        public string Name { get; set; }
        public List<ReferenceDataItem> Items { get; set; }
    }
    public class ReferenceDataItem
    {
        public ReferenceDataItem()
        {
            Options = new List<ReferenceDataOptionsItem>();
        }

        public string? OptionsClass { get;  set; }
        public string? ConfigurationSample { get;  set; }
        public string? Description { get;  set; }
        public string? ClassName { get;  set; }
        public string? TypeName { get;  set; }
        public string? Architecture { get;  set; }
        public List<ReferenceDataOptionsItem> Options { get; internal set; }
    }

    public class ReferenceDataOptionsItem
    {
        public string ParameterName { get; internal set; }
        public object Type { get; internal set; }
        public object Description { get; internal set; }
        public object DefaultValue { get; internal set; }
    }


}
