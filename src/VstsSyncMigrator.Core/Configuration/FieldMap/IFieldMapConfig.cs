using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
    public interface IFieldMapConfig
    {
        string WorkItemTypeName { get; set; }
        [JsonIgnoreAttribute]
        Type FieldMap { get; }

    }
}
