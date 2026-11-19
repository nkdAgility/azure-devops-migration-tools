using System.Collections.Generic;
using System.Linq;

namespace MigrationTools.Tools
{
    public enum ChildLinkAction
    {
        Add,
        AlreadyLinked,
        ReplaceParent,
        DeferUntilParentSaved,
        SkipChildHasFinalParentElsewhere,
    }

    /// <summary>
    /// Decides how to add a child (Hierarchy-Forward) link from a parent to a child that may already have a parent.
    /// A work item can have only one parent (TF201036), and the parent the child has in the source now wins:
    /// a historical child link replayed after the child moved elsewhere is skipped rather than moving the child back (#3292).
    /// Every existing parent link on the child is considered, including unsaved ones, so duplicates never throw (#3293).
    /// </summary>
    public static class ChildLinkDecision
    {
        public static ChildLinkAction Decide(
            int targetParentId,
            IEnumerable<int> targetChildParentIds,
            int sourceParentId,
            IEnumerable<int> sourceChildFinalParentIds,
            bool inline)
        {
            var currentParents = targetChildParentIds.ToList();
            if (targetParentId != 0 && currentParents.Contains(targetParentId))
            {
                return ChildLinkAction.AlreadyLinked;
            }
            if (currentParents.Count == 0)
            {
                return ChildLinkAction.Add;
            }
            if (!sourceChildFinalParentIds.Contains(sourceParentId))
            {
                return ChildLinkAction.SkipChildHasFinalParentElsewhere;
            }
            return inline && targetParentId == 0 ? ChildLinkAction.DeferUntilParentSaved : ChildLinkAction.ReplaceParent;
        }
    }
}
