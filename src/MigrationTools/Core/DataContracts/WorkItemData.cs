using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Core.DataContracts
{
   public class WorkItemData
    {
        public string id { get; set; }
        public string title { get; set; }
        public String Type { get; set; }
        public IDictionary<string, object> Fields { get; set; }

    }

}
