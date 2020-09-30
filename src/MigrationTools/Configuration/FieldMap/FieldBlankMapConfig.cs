using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTools.Configuration.FieldMap
{
   public class FieldBlankMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public string targetField { get; set; }
        public string FieldMap
        {
            get
            {
                return "FieldBlankMap";
            }
        }
    }
}
