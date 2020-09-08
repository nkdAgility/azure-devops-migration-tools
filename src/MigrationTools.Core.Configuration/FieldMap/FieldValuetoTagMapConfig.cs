using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsMigrationTools.Core.Configuration.FieldMap
{
   public class FieldValuetoTagMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public string sourceField { get; set; }
        public string pattern { get; set; }
        public string formatExpression { get; set; }
        public string FieldMap
        {
            get
            {
                return "FieldValuetoTagMap";
            }
        }
    }
}
