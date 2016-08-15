using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VSTS.DataBulkEditor.Engine.Configuration.FieldMap
{
    public interface IFieldMapConfig
    {
        string WorkItemTypeName { get; set; }
        [JsonIgnoreAttribute]
        Type FieldMap { get; }

    }
}
