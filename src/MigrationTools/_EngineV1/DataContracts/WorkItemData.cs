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

                _id = GetField("System.Id")?.ToString(); // ToString() will create a new string while the field value already is a string => as cast is more efficient here
                return _id;
            }
            set
            {
                var val = int.Parse(value);
                _id = value;
                SetField<int>("System.Id", val);
            }
        }

        public string Type
        {
            get => GetField("System.WorkItemType").ToString(); // ToString() will create a new string while the field value already is a string => as cast is more efficient here
            set => SetField("System.WorkItemType", value);
        }

        public string Title
        {
            get => GetField("System.Title").ToString(); // ToString() will create a new string while the field value already is a string => as cast is more efficient here
            set => SetField("System.Title", value);
        }

        public int Rev => int.Parse(GetField("System.Rev").ToString()); // Rev is always an int => cast is more efficient than ToString() and parsing it
        public DateTime ChangedDate => DateTime.Parse(GetField("System.ChangedDate").ToString()); // ChangedDate always is DateTime => cast is more efficient than ToString() and parsing its
        public string ProjectName { get; set; }

        [JsonIgnore]
        public object internalObject { get; set; }

        public Dictionary<string, object> Fields { get; set; }

        public WorkItemData()
        {
            Fields = new Dictionary<string, object>();
        }

        public IReadOnlyDictionary<int, RevisionItem> Revisions { get; set; }

        private object GetField(string field)
        {
            if (Fields is null)
            {
                this.Fields = new Dictionary<string, object>();
            }
            if (!this.Fields.ContainsKey(field))
            {
                return default;
            }
            return this.Fields[field];
        }

        private void SetField<T>(string field, T value)
        {
            if (Fields is null)
            {
                this.Fields = new Dictionary<string, object>();
            }
            if (this.Fields.ContainsKey(field))
            {
                this.Fields[field] = value;
            }
            else
            {
                this.Fields.Add(field, value);
            }
        }
    }
}