using System;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Pure decision logic for preserving ClosedDate during revision replay (#3171).
    /// </summary>
    public static class ClosedDateHelper
    {
        public const string CommonClosedDate = "Microsoft.VSTS.Common.ClosedDate";
        public const string SystemClosedDate = "System.ClosedDate";

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
