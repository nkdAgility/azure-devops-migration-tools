using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MigrationTools.DataContracts
{
    public class WorkItemData
    {
        public string Id { get; set; }
        public string Type { get; set; }

        [JsonIgnoreAttribute]
        public int LatestRevisionNumber
        {
            get
            {
                return Revisions[Revisions.Count - 1].Number;
            }
        }

        [JsonIgnoreAttribute]
        public RevisionItem LatestRevision
        {
            get
            {
                return Revisions[Revisions.Count - 1];
            }
        }

        [JsonIgnoreAttribute]
        public DateTime LatestRevDate
        {
            get
            {
                return Revisions[Revisions.Count - 1].ChangedDate;
            }
        }

        public List<RevisionItem> Revisions { get; set; }
    }
}