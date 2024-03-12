using System;
using System.Collections.Generic;

namespace MigrationTools.DataContracts
{
    public class RevisionItem
    {
        public int WorkItemId { get; set; }
        public int Index { get; set; }
        public int Number { get; set; }
        public DateTime ChangedDate { get; set; }

        public DateTime OriginalChangedDate { get; set; }
        public string Type { get; set; }
        public Dictionary<string, FieldItem> Fields { get; set; }
        public Dictionary<string, EnricherData> EnricherData { get; set; }
    }
}