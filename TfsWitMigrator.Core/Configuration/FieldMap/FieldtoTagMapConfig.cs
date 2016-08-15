using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSTS.DataBulkEditor.Engine.Configuration.FieldMap
{
   public class FieldtoTagMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public string sourceField { get; set; }
        public string formatExpression { get; set; }
        public Type FieldMap
        {
            get
            {
                return typeof(FieldToTagFieldMap);
            }
        }
    }
}
