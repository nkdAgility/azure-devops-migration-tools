using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MigrationTools.DataContracts;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Handles user mapping entries written as <c>Display Name &lt;upn&gt;</c>, which Azure DevOps accepts for identity
    /// fields and which disambiguate users that share a display name.
    /// </summary>
    public static class TfsUserIdentityReference
    {
        private static readonly Regex Pattern = new(@"^\s*(?<name>[^<>]*?)\s*<(?<unique>[^<>]+)>\s*$", RegexOptions.Compiled);

        /// <summary>
        /// Parses <c>Display Name &lt;upn&gt;</c>. Returns false for a plain display name.
        /// </summary>
        public static bool TryParse(string value, out string displayName, out string uniqueName)
        {
            displayName = null;
            uniqueName = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            var match = Pattern.Match(value);
            if (!match.Success || string.IsNullOrWhiteSpace(match.Groups["unique"].Value))
            {
                return false;
            }
            displayName = match.Groups["name"].Value;
            uniqueName = match.Groups["unique"].Value.Trim();
            return true;
        }

        /// <summary>
        /// Formats a user as <c>Display Name &lt;upn&gt;</c>, using the account name, or the email when there is no account name.
        /// Returns the display name alone when neither is available.
        /// </summary>
        public static string Format(IdentityItemData user)
        {
            if (user == null)
            {
                return null;
            }
            var unique = !string.IsNullOrWhiteSpace(user.AccountName) ? user.AccountName : user.MailAddress;
            return string.IsNullOrWhiteSpace(unique) ? user.DisplayName : $"{user.DisplayName} <{unique}>";
        }

        /// <summary>
        /// Finds the mapped target for a work item identity value. Tries the exact key, then keys written as
        /// <c>Display Name &lt;upn&gt;</c> with the same unique name, then a single such key with the same display name.
        /// Returns null when there is no mapping or the display name matches more than one key.
        /// </summary>
        public static string FindMappedTarget(IDictionary<string, string> userMap, string sourceValue)
        {
            if (userMap == null || string.IsNullOrEmpty(sourceValue))
            {
                return null;
            }
            if (userMap.TryGetValue(sourceValue, out var exact))
            {
                return exact;
            }

            var sourceIsQualified = TryParse(sourceValue, out var sourceName, out var sourceUnique);
            var qualifiedKeys = userMap
                .Select(kvp => TryParse(kvp.Key, out var name, out var unique) ? new { Name = name, Unique = unique, kvp.Value } : null)
                .Where(k => k != null)
                .ToList();

            if (sourceIsQualified)
            {
                var byUnique = qualifiedKeys.Where(k => string.Equals(k.Unique, sourceUnique, StringComparison.OrdinalIgnoreCase)).Take(2).ToList();
                if (byUnique.Count == 1)
                {
                    return byUnique[0].Value;
                }
                if (userMap.TryGetValue(sourceName, out var byPlainName))
                {
                    return byPlainName;
                }
            }

            var name = sourceIsQualified ? sourceName : sourceValue;
            var byName = qualifiedKeys.Where(k => string.Equals(k.Name, name, StringComparison.OrdinalIgnoreCase)).Take(2).ToList();
            return byName.Count == 1 ? byName[0].Value : null;
        }

        /// <summary>
        /// Returns the display names that occur more than once in <paramref name="users"/>, ignoring case.
        /// </summary>
        public static HashSet<string> DuplicateDisplayNames(IEnumerable<IdentityItemData> users)
        {
            return new HashSet<string>(
                (users ?? []).Where(u => !string.IsNullOrEmpty(u?.DisplayName))
                    .GroupBy(u => u.DisplayName, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key),
                StringComparer.OrdinalIgnoreCase);
        }
    }
}
