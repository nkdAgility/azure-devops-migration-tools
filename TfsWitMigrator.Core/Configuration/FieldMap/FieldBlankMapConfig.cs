using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSTS.DataBulkEditor.Engine.ComponentContext;

namespace VSTS.DataBulkEditor.Engine.Configuration.FieldMap
{
   public class FieldBlankMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public string targetField { get; set; }
        public Type FieldMap
        {
            get
            {
                return typeof(FieldBlankMap);
            }
        }
    }
}
