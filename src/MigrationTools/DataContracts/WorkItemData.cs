using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MigrationTools.DataContracts
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

                _id = GetField("ID")?.ToString();
                return _id;
            }
            set
            {
                var val = int.Parse(value);
                _id = value;
                SetField<int>("ID", val);
            }
        }

        public string Type
        {
            get => GetField("Work Item Type").ToString();
            set => SetField("Work Item Type", value);
        }

        public string Title
        {
            get => GetField("Title").ToString();
            set => SetField("Title", value);
        }

        public int Rev => int.Parse(GetField("Rev").ToString());
        public DateTime RevisedDate => DateTime.Parse(GetField("Revised Date").ToString());
        public string ProjectName { get; set; }

        public Dictionary<string, object> Fields { get; set; }

        public WorkItemData()
        {
            Fields = new Dictionary<string, object>();
        }

        public SortedDictionary<int, RevisionItem> Revisions { get; set; }

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

        [JsonIgnore]
        public object internalObject { get; set; }
    }
}