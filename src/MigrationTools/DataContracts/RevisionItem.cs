using System;
using System.Collections.Generic;

namespace MigrationTools.DataContracts
{
    public class RevisionItem
    {
        public int Index { get; set; }
        public int Number { get; set; }
        public DateTime ChangedDate { get; set; }
        public Dictionary<string, object> Fields { get; set; }
        public Dictionary<string, EnricherData> EnricherData { get; set; }
    }
}