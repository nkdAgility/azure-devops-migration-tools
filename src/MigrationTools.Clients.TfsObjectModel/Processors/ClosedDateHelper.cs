using System;
using System.Collections.Generic;
using System.Linq;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Pure decision logic for preserving rule-stamped dates (ClosedDate #3171, ActivatedDate) during revision replay.
    /// </summary>
    public static class ClosedDateHelper
    {
        public const string CommonClosedDate = "Microsoft.VSTS.Common.ClosedDate";
        public const string SystemClosedDate = "System.ClosedDate";
        public const string CommonActivatedDate = "Microsoft.VSTS.Common.ActivatedDate";
        public const string CommonResolvedDate = "Microsoft.VSTS.Common.ResolvedDate";

        /// <summary>
        /// Date fields, other than ClosedDate, that the server stamps with the save time when a revision changes
        /// state, even with BypassRules. They are read-only in that revision, so they are corrected after the save.
        /// </summary>
        public static readonly IReadOnlyList<string> StateStampedDateFields = new[] { CommonActivatedDate, CommonResolvedDate };

        /// <summary>
        /// Returns the target fields whose saved value differs from the source revision's value
        /// (by <see cref="NeedsUpdate"/>), given (target field, source value, target value) triples.
        /// </summary>
        public static List<string> FieldsNeedingFixUp(IEnumerable<(string Field, object SourceValue, object TargetValue)> fields)
        {
            return fields.Where(f => NeedsUpdate(f.SourceValue, f.TargetValue)).Select(f => f.Field).ToList();
        }

        /// <summary>Returns the ClosedDate field to use, preferring Microsoft.VSTS.Common.ClosedDate, or null if neither exists.</summary>
        public static string ResolveField(bool hasCommonClosedDate, bool hasSystemClosedDate)
        {
            if (hasCommonClosedDate) return CommonClosedDate;
            if (hasSystemClosedDate) return SystemClosedDate;
            return null;
        }

        /// <summary>
        /// True when the target must be set to the source value: the source revision has a ClosedDate
        /// (the item is closed in that revision) and the target value differs. A null/empty source value
        /// never triggers an update, so items that are not closed are left alone.
        /// </summary>
        public static bool NeedsUpdate(object sourceValue, object targetValue)
        {
            if (sourceValue == null || sourceValue is DBNull || (sourceValue is string s && s.Length == 0))
            {
                return false;
            }
            if (targetValue == null || targetValue is DBNull)
            {
                return true;
            }
            if (sourceValue is DateTime sd && targetValue is DateTime td)
            {
                return sd.ToUniversalTime() != td.ToUniversalTime();
            }
            return !Equals(sourceValue, targetValue);
        }
    }
}
