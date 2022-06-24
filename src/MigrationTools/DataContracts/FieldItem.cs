using System;
using Newtonsoft.Json;

namespace MigrationTools.DataContracts
{
    public class FieldItem
    {
        private object _value;

        [JsonIgnore]
        public object internalObject { get; set; }

        public string ReferenceName { get; set; }
        public string Name { get; set; }
        public object Value { 
            get {
                if (Name == "System.RevisedDate" && _value.ToString() == "Microsoft.TeamFoundation.WorkItemTracking.Common.ServerDefaultFieldValue")
                    return DateTime.Parse("01/01/9999 00:00:0");
                else
                    return _value;
            }
            set {
                _value = value;
            }
        }
    }
}