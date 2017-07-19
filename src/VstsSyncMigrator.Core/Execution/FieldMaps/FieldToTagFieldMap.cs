using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.ComponentContext;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VstsSyncMigrator.Engine.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine
{
    public class FieldToTagFieldMap : IFieldMap
    {
        private FieldtoTagMapConfig config;
        private MigrationEngine _me;
        private WorkItemStoreContext _targetStore;
        private WorkItemStoreContext _sourceStore;

        public FieldToTagFieldMap(FieldtoTagMapConfig config)
        {
            this.config = config;
        }

        public string Name
        {
            get
            {
                return "FieldToTagFieldMap";
            }
        }

        public void Init(MigrationEngine me, WorkItemStoreContext sourceStore, WorkItemStoreContext targetStore) {
            _me = me;
            _targetStore = targetStore;
            _sourceStore = sourceStore;
        }

        public void Execute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(this.config.sourceField))
            {
                List<string> newTags = target.Tags.Split(char.Parse(@";")).ToList();
                // to tag
                if (source.Fields[this.config.sourceField].Value != null)
                {
                    string value = source.Fields[this.config.sourceField].Value.ToString();
                    if (string.IsNullOrEmpty(config.formatExpression))
                    {
                        newTags.Add(value);
                    }
                    else
                    {
                        newTags.Add(string.Format(config.formatExpression, value));
                    }

                    target.Tags = string.Join(";", newTags.ToArray());
                    Trace.WriteLine(string.Format("  [UPDATE] field tagged {0}:{1} to {2}:Tag with foramt of {3}", source.Id, this.config.sourceField, target.Id, config.formatExpression));
                }
                
            }
        }
    }
}