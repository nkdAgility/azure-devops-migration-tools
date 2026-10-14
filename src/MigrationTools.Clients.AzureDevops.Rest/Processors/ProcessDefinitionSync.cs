using System;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.DataContracts.Process;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Decisions the <see cref="ProcessDefinitionProcessor"/> makes when matching source definitions to the target.
    /// </summary>
    public static class ProcessDefinitionSync
    {
        /// <summary>
        /// A target that only has the locked system type cannot have its layout read or changed (VS403115), so a
        /// customised source type needs its own inherited copy created in the target first (#3211).
        /// </summary>
        public static bool NeedsTargetWorkItemTypeCreated(WorkItemType source, WorkItemType target)
        {
            if (target == null || string.IsNullOrEmpty(target.Id))
            {
                return true;
            }
            return target.IsSystem && !source.IsSystem;
        }

        /// <summary>
        /// Custom behaviors get a generated reference name in each organisation, so fall back to the name.
        /// </summary>
        public static WorkItemBehavior FindTargetBehavior(IEnumerable<WorkItemBehavior> targetBehaviors, WorkItemBehavior source)
        {
            var behaviors = (targetBehaviors ?? Enumerable.Empty<WorkItemBehavior>()).Where(b => b != null).ToList();
            return behaviors.FirstOrDefault(b => string.Equals(b.ReferenceName, source.ReferenceName, StringComparison.OrdinalIgnoreCase))
                ?? behaviors.FirstOrDefault(b => string.Equals(b.Name, source.Name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Azure DevOps omits empty arrays, so groups and their controls may be null (#3218).
        /// </summary>
        public static WorkItemGroup FindGroupContainingControl(IEnumerable<WorkItemGroup> groups, string controlId, WorkItemGroup excluding = null)
        {
            return (groups ?? Enumerable.Empty<WorkItemGroup>())
                .Where(g => g != null && g != excluding && (excluding == null || !string.Equals(g.Id, excluding.Id, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault(g => g.Controls != null && g.Controls.Any(c => string.Equals(c.Id, controlId, StringComparison.OrdinalIgnoreCase)));
        }

        public static string ResolveTargetProcessName(IDictionary<string, string> processMaps, string sourceProcessName)
        {
            if (processMaps != null && processMaps.TryGetValue(sourceProcessName, out var mapped) && !string.IsNullOrEmpty(mapped))
            {
                return mapped;
            }
            return sourceProcessName;
        }
    }
}
