using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
   public class TreeToTagMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public int toSkip { get; set; }
        public int timeTravel { get; set; }

        public Type FieldMap
        {
            get
            {
                return typeof(TreeToTagFieldMap);
            }
        }
    }
}
