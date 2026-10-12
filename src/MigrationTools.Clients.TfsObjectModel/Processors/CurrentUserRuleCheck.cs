using System;
using System.Collections.Generic;
using System.Linq;

namespace MigrationTools.Processors
{
    /// <summary>A target process rule, reduced to what <see cref="CurrentUserRuleCheck"/> needs.</summary>
    public sealed class ProcessRuleInfo
    {
        public string WorkItemType { get; set; }
        public string RuleName { get; set; }
        public bool IsDisabled { get; set; }
        /// <summary>True for rules added in an inherited process; false for the parent (system) process's own rules.</summary>
        public bool IsCustom { get; set; }
        public List<ProcessRuleActionInfo> Actions { get; set; } = new List<ProcessRuleActionInfo>();
    }

    public sealed class ProcessRuleActionInfo
    {
        public string ActionType { get; set; }
        public string TargetField { get; set; }
    }

    /// <summary>
    /// Finds target process rules that copy the current user into an identity field. Azure DevOps applies these on
    /// every save while their condition holds, even with BypassRules, so replayed revisions get the migration account
    /// (#2869). Pure logic, so it can be unit tested.
    /// </summary>
    public static class CurrentUserRuleCheck
    {
        private static readonly HashSet<string> CurrentUserActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "copyFromCurrentUser",
            "copyFromServerCurrentUser",
        };

        public sealed class Finding
        {
            public string WorkItemType { get; set; }
            public string RuleName { get; set; }
            public string Field { get; set; }
        }

        public static List<Finding> Find(IEnumerable<ProcessRuleInfo> rules, Func<string, bool> isIdentityField)
        {
            if (rules == null) return new List<Finding>();
            if (isIdentityField == null) throw new ArgumentNullException(nameof(isIdentityField));
            return rules
                .Where(r => r != null && !r.IsDisabled && r.IsCustom)
                .SelectMany(r => (r.Actions ?? new List<ProcessRuleActionInfo>())
                    .Where(a => a != null && CurrentUserActions.Contains(a.ActionType ?? string.Empty)
                        && !string.IsNullOrEmpty(a.TargetField) && isIdentityField(a.TargetField))
                    .Select(a => new Finding { WorkItemType = r.WorkItemType, RuleName = r.RuleName, Field = a.TargetField }))
                .GroupBy(f => (f.WorkItemType, f.RuleName, f.Field))
                .Select(g => g.First())
                .OrderBy(f => f.WorkItemType, StringComparer.OrdinalIgnoreCase)
                .ThenBy(f => f.RuleName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
