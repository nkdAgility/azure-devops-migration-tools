using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MigrationTools._EngineV1.DataContracts
{
    public class WorkItemData
    {
        // Using Get Only properties here and return the content of the FieldsCollection to make sure the correct data is returned in case
        // WorkItemData is used for the Revision of a WorkItem. It also seems to improve the initial query phase as less properties have to be set.
        // From a design point of view it maybe would be better to provide an Interface and have separate classes for WorkItemData and RevisionData.
        // This would make clearer what type of WorkItemData is in use at the moment.

        private string _id;

        public string Id
        {
            get
            {
                if (!string.IsNullOrEmpty(_id))
                {
                    return _id;
                }

                _id = this.Fields["ID"].ToString();
                return _id;
            }
            set
            {
                var val = int.Parse(value);
                _id = value;
                this.Fields["ID"] = val;
            }
        }

        public string Type
        {
            get => this.Fields["Work Item Type"] as string;
            set => this.Fields["Work Item Type"] = value;
        }

        public string Title
        {
            get => this.Fields["Title"] as string;
            set => this.Fields["Title"] = value;
        }

        public int Rev => (int)this.Fields["Rev"];
        public DateTime RevisedDate => (DateTime)this.Fields["Revised Date"];
        public string ProjectName { get; set; }

        [JsonIgnore]
        public object internalObject { get; set; }

        public Dictionary<string, object> Fields { get; set; }
        public IReadOnlyDictionary<int, RevisionItem> Revisions { get; set; }
    }
}