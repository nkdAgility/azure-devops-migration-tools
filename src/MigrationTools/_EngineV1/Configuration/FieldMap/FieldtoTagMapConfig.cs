﻿using System;
using MigrationTools.Options;

namespace MigrationTools._EngineV1.Configuration.FieldMap
{
    /// <summary>
    /// Want to take a field and convert its value to a tag? Done...
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
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

        public void SetExampleConfigDefaults()
        {
            WorkItemTypeName = "*";
            sourceField = "Custom.ProjectName";
            formatExpression = "Project: {0}";
        }
    }
}