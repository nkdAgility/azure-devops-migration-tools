using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MigrationTools._EngineV1.DataContracts
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

        [JsonIgnore]
        public object internalObject { get; set; }

        public Dictionary<string, object> Fields { get; set; }
        public List<RevisionItem> Revisions { get; set; }
    }
}