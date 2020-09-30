using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Core.DataContracts
{
   public class WorkItemData
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }

        public object InternalWorkItem { get; set; }

    }

}
