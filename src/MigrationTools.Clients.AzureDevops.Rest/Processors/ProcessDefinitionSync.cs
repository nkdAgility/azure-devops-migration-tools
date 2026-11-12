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

        /// <summary>
        /// Lists the controls, groups and system controls whose visibility on the target differs from the source (#3242).
        /// Controls and system controls match on their field reference name, groups on their label within the page of
        /// the same label. Anything already matching is left out, so applying the result again changes nothing.
        /// </summary>
        public static List<LayoutVisibilityChange> FindVisibilityChanges(WorkItemLayout source, WorkItemLayout target)
        {
            var changes = new List<LayoutVisibilityChange>();
            if (source == null || target == null)
            {
                return changes;
            }

            var targetGroups = GroupsOf(target).ToList();
            foreach (var (sourcePage, _, sourceGroup) in GroupsOf(source))
            {
                var targetGroup = targetGroups.FirstOrDefault(t =>
                    string.Equals(t.Page.Label, sourcePage.Label, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(t.Group.Label, sourceGroup.Label, StringComparison.OrdinalIgnoreCase));
                if (targetGroup.Group != null && targetGroup.Group.Visible != sourceGroup.Visible)
                {
                    changes.Add(new LayoutVisibilityChange(LayoutVisibilityTarget.Group, sourceGroup.Label, sourceGroup.Visible,
                        targetGroup.Page.Id, targetGroup.Section.Id, targetGroup.Group.Id, null));
                }

                foreach (var sourceControl in sourceGroup.Controls ?? Enumerable.Empty<WorkItemControl>())
                {
                    var match = targetGroups
                        .SelectMany(t => (t.Group.Controls ?? Enumerable.Empty<WorkItemControl>()).Select(c => new { t.Group, Control = c }))
                        .FirstOrDefault(x => string.Equals(x.Control.Id, sourceControl.Id, StringComparison.OrdinalIgnoreCase));
                    if (match != null && match.Control.Visible != sourceControl.Visible)
                    {
                        changes.Add(new LayoutVisibilityChange(LayoutVisibilityTarget.Control, sourceControl.Label ?? sourceControl.Id, sourceControl.Visible,
                            null, null, match.Group.Id, match.Control.Id));
                    }
                }
            }

            foreach (var sourceSystem in source.SystemControls ?? Enumerable.Empty<SystemControl>())
            {
                var targetSystem = (target.SystemControls ?? Enumerable.Empty<SystemControl>())
                    .FirstOrDefault(t => string.Equals(t.Id, sourceSystem.Id, StringComparison.OrdinalIgnoreCase));
                if (targetSystem != null && targetSystem.Visible != sourceSystem.Visible)
                {
                    changes.Add(new LayoutVisibilityChange(LayoutVisibilityTarget.SystemControl, sourceSystem.Label ?? sourceSystem.Id, sourceSystem.Visible,
                        null, null, null, targetSystem.Id));
                }
            }
            return changes;
        }

        /// <summary>
        /// Inherited groups keep their id across processes derived from the same parent; fall back to the label on the
        /// page of the same label.
        /// </summary>
        public static WorkItemGroup FindTargetInheritedGroup(WorkItemLayout target, string pageLabel, WorkItemGroup source)
        {
            if (target == null || source == null)
            {
                return null;
            }
            var groups = GroupsOf(target).ToList();
            return groups.Select(g => g.Group).FirstOrDefault(g => string.Equals(g.Id, source.Id, StringComparison.OrdinalIgnoreCase))
                ?? groups.Where(g => string.Equals(g.Page.Label, pageLabel, StringComparison.OrdinalIgnoreCase))
                    .Select(g => g.Group)
                    .FirstOrDefault(g => string.Equals(g.Label, source.Label, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<(WorkItemPage Page, WorkItemSection Section, WorkItemGroup Group)> GroupsOf(WorkItemLayout layout)
        {
            return from page in layout.Pages ?? Enumerable.Empty<WorkItemPage>()
                   from section in page.Sections ?? Enumerable.Empty<WorkItemSection>()
                   from grp in section.Groups ?? Enumerable.Empty<WorkItemGroup>()
                   where grp != null
                   select (page, section, grp);
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

    public enum LayoutVisibilityTarget
    {
        Control,
        Group,
        SystemControl,
    }

    public sealed class LayoutVisibilityChange
    {
        public LayoutVisibilityChange(LayoutVisibilityTarget target, string label, bool visible, string pageId, string sectionId, string groupId, string controlId)
        {
            Target = target;
            Label = label;
            Visible = visible;
            PageId = pageId;
            SectionId = sectionId;
            GroupId = groupId;
            ControlId = controlId;
        }

        public LayoutVisibilityTarget Target { get; }
        public string Label { get; }
        public bool Visible { get; }
        public string PageId { get; }
        public string SectionId { get; }
        public string GroupId { get; }
        public string ControlId { get; }
    }
}
