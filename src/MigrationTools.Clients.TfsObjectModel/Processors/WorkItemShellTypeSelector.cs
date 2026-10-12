using System;
using System.Collections.Generic;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Chooses the work item type used to create the target shell for a work item that is being migrated.
    /// </summary>
    public static class WorkItemShellTypeSelector
    {
        /// <summary>
        /// Result of selecting the shell type.
        /// </summary>
        public sealed class Selection
        {
            /// <summary>The (mapped) target type to create the shell as.</summary>
            public string ShellType { get; set; }
            /// <summary>The (mapped) target type of the first revision.</summary>
            public string FirstType { get; set; }
            /// <summary>The (mapped) target type of the final revision.</summary>
            public string FinalType { get; set; }
            /// <summary>True when the first-revision type was not usable and the final type was used instead.</summary>
            public bool FellBackToFinalType { get; set; }
        }

        /// <summary>
        /// Maps a source type name through the work item type mappings, if one exists.
        /// </summary>
        public static string MapType(string sourceType, IDictionary<string, string> mappings)
        {
            if (sourceType != null && mappings != null && mappings.TryGetValue(sourceType, out var mapped))
            {
                return mapped;
            }
            return sourceType;
        }

        /// <summary>
        /// Selects the shell type. The first revision's (mapped) type is used, unless it is not usable in the
        /// target (missing or disabled) and the work item changed type, in which case the final (mapped) type is used.
        /// </summary>
        public static Selection Select(string firstSourceType, string finalSourceType, IDictionary<string, string> mappings, Func<string, bool> isUsableInTarget)
        {
            var first = MapType(firstSourceType, mappings);
            var final = MapType(finalSourceType, mappings);
            var selection = new Selection { ShellType = first, FirstType = first, FinalType = final };
            if (isUsableInTarget != null
                && !string.Equals(first, final, StringComparison.OrdinalIgnoreCase)
                && !isUsableInTarget(first)
                && isUsableInTarget(final))
            {
                selection.ShellType = final;
                selection.FellBackToFinalType = true;
            }
            return selection;
        }

        /// <summary>
        /// Selects the target type for a replayed revision. The revision's (mapped) type is used, unless it differs from
        /// the current target type and is not usable in the target, in which case the current type is kept.
        /// </summary>
        public static string SelectRevisionType(string revisionSourceType, string currentTargetType, IDictionary<string, string> mappings, Func<string, bool> isUsableInTarget)
        {
            var mapped = MapType(revisionSourceType, mappings);
            if (isUsableInTarget != null
                && !string.Equals(mapped, currentTargetType, StringComparison.OrdinalIgnoreCase)
                && !isUsableInTarget(mapped))
            {
                return currentTargetType;
            }
            return mapped;
        }
    }
}
