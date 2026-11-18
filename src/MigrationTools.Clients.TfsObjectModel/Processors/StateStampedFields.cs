using System;
using System.Collections.Generic;

namespace MigrationTools.Processors
{
    /// <summary>
    /// The fields Azure DevOps stamps with the server clock and the current identity when a revision changes state
    /// (#3270). Each must carry the source revision's value in the same save that changes the state: there is no way to
    /// correct it afterwards.
    /// </summary>
    public static class StateStampedFields
    {
        public const string CommonClosedDate = "Microsoft.VSTS.Common.ClosedDate";
        public const string SystemClosedDate = "System.ClosedDate";
        public const string CommonActivatedDate = "Microsoft.VSTS.Common.ActivatedDate";
        public const string CommonResolvedDate = "Microsoft.VSTS.Common.ResolvedDate";
        public const string CommonActivatedBy = "Microsoft.VSTS.Common.ActivatedBy";
        public const string CommonResolvedBy = "Microsoft.VSTS.Common.ResolvedBy";
        public const string CommonClosedBy = "Microsoft.VSTS.Common.ClosedBy";

        /// <summary>The state-stamped fields other than ClosedDate, which is resolved per work item type by <see cref="ResolveClosedDateField"/>.</summary>
        public static readonly IReadOnlyList<string> Fields = new[]
        {
            CommonActivatedDate, CommonResolvedDate, CommonActivatedBy, CommonResolvedBy, CommonClosedBy
        };

        /// <summary>Returns the ClosedDate field to use, preferring Microsoft.VSTS.Common.ClosedDate, or null if neither exists.</summary>
        public static string ResolveClosedDateField(bool hasCommonClosedDate, bool hasSystemClosedDate)
        {
            if (hasCommonClosedDate) return CommonClosedDate;
            if (hasSystemClosedDate) return SystemClosedDate;
            return null;
        }

        /// <summary>
        /// True when the target value is the source value. Empty (null, DBNull or "") equals empty, so a source revision
        /// without the value requires the target to be empty too. Dates compare as the same instant whatever their kind;
        /// other values compare as ordinal strings.
        /// </summary>
        public static bool SameValue(object sourceValue, object targetValue)
        {
            var sourceEmpty = IsEmpty(sourceValue);
            var targetEmpty = IsEmpty(targetValue);
            if (sourceEmpty || targetEmpty) return sourceEmpty && targetEmpty;
            if (sourceValue is DateTime sd && targetValue is DateTime td)
            {
                return sd.ToUniversalTime() == td.ToUniversalTime();
            }
            return string.Equals(Convert.ToString(sourceValue), Convert.ToString(targetValue), StringComparison.Ordinal);
        }

        public static bool IsEmpty(object value)
        {
            return value == null || value is DBNull || (value is string s && s.Length == 0);
        }

        /// <summary>
        /// True for TF26194 ("The value for field '...' cannot be changed"): the work item object model refuses the value.
        /// </summary>
        public static bool IsFieldCannotBeChangedError(Exception ex)
        {
            for (var e = ex; e != null; e = e.InnerException)
            {
                if (e.Message != null && e.Message.Contains("TF26194")) return true;
            }
            return false;
        }
    }
}
