using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VstsSyncMigrator.Engine.ComponentContext;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
   public class MultiValueConditionalMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public Dictionary<string,string> sourceFieldsAndValues { get; set; }
        public Dictionary<string, string> targetFieldsAndValues { get; set; }
        public Type FieldMap
        {
            get
            {
                return typeof(MultiValueConditionalMap);
            }
        }
    }
}
