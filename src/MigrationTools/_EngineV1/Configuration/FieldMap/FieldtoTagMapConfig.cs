using System;
using MigrationTools.Options;

namespace MigrationTools._EngineV1.Configuration.FieldMap
{
    public class FieldtoTagMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public string sourceField { get; set; }
        public string formatExpression { get; set; }

        public string FieldMap
        {
            get
            {
                return "FieldToTagFieldMap";
            }
        }

    }
}