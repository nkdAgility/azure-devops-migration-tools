using System;
using VstsSyncMigrator.Engine.ComponentContext;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
    public class FieldBlankMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public string targetField { get; set; }
        public Type FieldMap
        {
            get
            {
                return typeof(FieldBlankMap);
            }
        }
    }
}
