using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
   public class FieldValuetoTagMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public string sourceField { get; set; }
        public string pattern { get; set; }
        public string formatExpression { get; set; }
        public Type FieldMap
        {
            get
            {
                return typeof(FieldValuetoTagMap);
            }
        }
    }
}
