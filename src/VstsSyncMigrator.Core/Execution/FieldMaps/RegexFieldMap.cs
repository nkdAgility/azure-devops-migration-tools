using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.ComponentContext;
using System.Text.RegularExpressions;
using System.Diagnostics;
using VstsSyncMigrator.Engine.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine
{
    public class RegexFieldMap : IFieldMap
    {
        private RegexFieldMapConfig config;
        private MigrationEngine _me;
        private WorkItemStoreContext _targetStore;
        private WorkItemStoreContext _sourceStore;

        public RegexFieldMap(RegexFieldMapConfig config)
        {
            this.config = config;
        }

        public void Init(MigrationEngine me, WorkItemStoreContext sourceStore, WorkItemStoreContext targetStore) {
            _me = me;
            _targetStore = targetStore;
            _sourceStore = sourceStore;
        }

        public string Name
        {
            get
            {
                return "RegexFieldMap";
            }
        }

        public void Execute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(config.sourceField) && source.Fields[config.sourceField].Value != null && target.Fields.Contains(config.targetField))
            {
                if (Regex.IsMatch(source.Fields[config.sourceField].Value.ToString(), config.pattern))
                {
                    String Action = "Update";
                    String TargetId = target.Id.ToString();
                    if (target.Id == 0) {
                        Action = "Set";
                        TargetId = "NewWI";
                    }
                    // Theo: extended with search of ReflectedWorkItemID; example with shared steps: "pattern": "<compref [^<>]* ref=.(\\d*)[^ ]"; "replacement": "ReflectedWorkItemID($1)"; "sourceField": "Steps", "targetField": "Steps", "WorkItemTypeName": "Test Case"
                    if (config.replacement == "ReflectedWorkItemID($1)") {
                        foreach (Match match in Regex.Matches(source.Fields[config.sourceField].Value.ToString(), config.pattern)) {
                            int OldSharedStepId = Int32.Parse(match.Groups[1].ToString());
                            WorkItem SharedStep = source.Store.GetWorkItem(OldSharedStepId);
                            WorkItem targetSharedStep = _targetStore.FindReflectedWorkItem(SharedStep, _me.ReflectedWorkItemIdFieldName, false);
                            if(targetSharedStep == null) {
                                throw new Exception(String.Format("Regex convert of {0}: cannot convert WorkItem.Id = {1}. New referenced Id does not exist on target",source.Id,OldSharedStepId));
                            }
                            target.Fields[config.targetField].Value = (target.Fields[config.targetField].Value).ToString().Replace("\""+OldSharedStepId.ToString()+ "\"", "\""+targetSharedStep.Fields["Id"].Value.ToString()+ "\"");
                            Trace.WriteLine(string.Format("  [{6}] field tagged {0}:{1} with ReflectedWorkItemID {4} to {2}:{3} referencing new ReflectedWorkItemID {5}", source.Id, config.sourceField, TargetId, config.targetField, OldSharedStepId, targetSharedStep.Fields["Id"].Value.ToString(),Action));
                        }
                    } else {
                        target.Fields[config.targetField].Value = Regex.Replace(source.Fields[config.sourceField].Value.ToString(), config.pattern, config.replacement);
                        Trace.WriteLine(string.Format("  [{6}] field tagged {0}:{1} to {2}:{3} with regex pattern of {4} resulting in {5}", source.Id, config.sourceField, TargetId, config.targetField, config.pattern, target.Fields[config.targetField].Value,Action));
                    }
                }
            }
        }
    }
}