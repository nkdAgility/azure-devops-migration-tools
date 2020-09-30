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
        public int Rev { get; set; }
    public DateTime RevisedDate { get; set; }
    public int Revision { get; set; }
        public string ProjectName { get; set; }
        public object InternalWorkItem { get; set; }

    }

}
