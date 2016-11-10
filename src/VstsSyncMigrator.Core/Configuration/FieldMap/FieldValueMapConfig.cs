using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VstsSyncMigrator.Engine.ComponentContext;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
   public class FieldValueMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public string sourceField { get; set; }
        public string targetField { get; set; }
        public Dictionary<string, string> valueMapping { get; set; }


        public Type FieldMap
        {
            get
            {
                return typeof(FieldValueMap);
            }
        }

    }
}
