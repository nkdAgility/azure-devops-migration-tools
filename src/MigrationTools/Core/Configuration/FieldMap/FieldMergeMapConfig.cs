using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MigrationTools.Core.Configuration.FieldMap
{
   public class FieldMergeMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public string sourceField1 { get; set; }
        public string sourceField2 { get; set; }
        public string targetField { get; set; }
        public string formatExpression { get; set; }
        public string doneMatch { get; set; } = "##DONE##"; //Depricated TODO remove
        public string FieldMap
        {
            get
            {
                return "FieldMergeMap";
            }
        }
    }
}
