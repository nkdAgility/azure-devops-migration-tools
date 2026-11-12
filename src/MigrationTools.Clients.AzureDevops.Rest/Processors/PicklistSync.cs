using System;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.DataContracts.Process;

namespace MigrationTools.Processors
{
    public enum PicklistFieldAction
    {
        /// <summary>Create the picklist on the target, then the field linked to it.</summary>
        CreateListAndField,
        /// <summary>The target field already uses a list of the same type; add the source values it lacks.</summary>
        AddMissingItems,
        NoChange,
        /// <summary>The target field exists without a picklist. Converting it would mean deleting the field and its data.</summary>
        TargetFieldIsNotPicklist,
        /// <summary>The target list holds a different type, which the service cannot change.</summary>
        TypeMismatch
    }

    public class PicklistFieldPlan
    {
        public PicklistFieldAction Action { get; set; }
        public List<string> MissingItems { get; set; } = new List<string>();
        public List<string> TargetOnlyItems { get; set; } = new List<string>();
        public bool IsSuggestedDiffers { get; set; }
    }

    /// <summary>
    /// Decides how a source picklist field reaches the target without a destructive change: a field created without its
    /// list becomes plain text (#3241), and an existing target field or list is only ever added to.
    /// </summary>
    public static class PicklistSync
    {
        public static PicklistFieldPlan Plan(WorkItemPicklist sourceList, WorkItemField targetField, WorkItemPicklist targetList)
        {
            if (targetField == null)
            {
                return new PicklistFieldPlan { Action = PicklistFieldAction.CreateListAndField };
            }
            if (!targetField.IsPicklist || targetList == null)
            {
                return new PicklistFieldPlan { Action = PicklistFieldAction.TargetFieldIsNotPicklist };
            }
            if (!string.Equals(sourceList.Type, targetList.Type, StringComparison.OrdinalIgnoreCase))
            {
                return new PicklistFieldPlan { Action = PicklistFieldAction.TypeMismatch };
            }

            var sourceItems = sourceList.Items ?? new List<string>();
            var targetItems = targetList.Items ?? new List<string>();
            var plan = new PicklistFieldPlan
            {
                MissingItems = sourceItems.Where(s => !targetItems.Contains(s, StringComparer.OrdinalIgnoreCase)).ToList(),
                TargetOnlyItems = targetItems.Where(t => !sourceItems.Contains(t, StringComparer.OrdinalIgnoreCase)).ToList(),
                IsSuggestedDiffers = sourceList.IsSuggested != targetList.IsSuggested
            };
            plan.Action = plan.MissingItems.Count > 0 ? PicklistFieldAction.AddMissingItems : PicklistFieldAction.NoChange;
            return plan;
        }

        /// <summary>
        /// Picklist names are unique in an organisation (VS402848), and a source name may already be taken on the
        /// target, so a new list gets a fresh name the way the web portal names them.
        /// </summary>
        public static WorkItemPicklist NewTargetList(WorkItemPicklist sourceList)
        {
            var list = sourceList.CloneAsNew();
            list.Name = $"picklist_{Guid.NewGuid()}";
            return list;
        }

        /// <summary>
        /// A picklist field's allowed values on a work item type are its list's items. Sending them writes them into the
        /// target's list, which fails when the types differ (VS403060) and would bypass the non-destructive list sync.
        /// </summary>
        /// <param name="targetTypeDiffers">The target field kept a different type, so the source default may not be valid there.</param>
        public static WorkItemTypeField WithoutListValues(WorkItemTypeField sourceField, bool isPicklist, bool targetTypeDiffers = false)
        {
            var dropValues = isPicklist && sourceField.AllowedValues != null;
            var dropDefault = targetTypeDiffers && !string.IsNullOrEmpty(sourceField.DefaultValue);
            if (!dropValues && !dropDefault)
            {
                return sourceField;
            }
            var field = sourceField.CloneAsNew();
            if (dropValues)
            {
                field.AllowedValues = null;
            }
            if (dropDefault)
            {
                field.DefaultValue = null;
            }
            return field;
        }

        public static bool TypesDiffer(WorkItemField sourceField, WorkItemField targetField)
        {
            return sourceField != null && targetField != null && !string.Equals(sourceField.Type, targetField.Type, StringComparison.OrdinalIgnoreCase);
        }

        public static WorkItemPicklist WithItemsAdded(WorkItemPicklist targetList, IEnumerable<string> missingItems)
        {
            return new WorkItemPicklist
            {
                Id = targetList.Id,
                Name = targetList.Name,
                Type = targetList.Type,
                IsSuggested = targetList.IsSuggested,
                Items = (targetList.Items ?? new List<string>()).Concat(missingItems).ToList()
            };
        }
    }
}
