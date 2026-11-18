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
        /// Finds the mapped target for a work item identity value. Tries the exact key; for a qualified value, a key with the
        /// same unique name and then a plain key with the same display name; for a plain value, a single qualified key with the
        /// same display name.
        /// Returns null when there is no mapping or the display name matches more than one key.
        /// </summary>
        public static string FindMappedTarget(IDictionary<string, string> userMap, string sourceValue)
        {
            // Replay calls this for every identity field of every revision, so an exact hit stays a single lookup.
            if (userMap != null && !string.IsNullOrEmpty(sourceValue) && userMap.TryGetValue(sourceValue, out var exact))
            {
                return exact;
            }
            var key = FindMappedKey(userMap, sourceValue);
            return key == null ? null : userMap[key];
        }

        /// <summary>
        /// The mapping key that <see cref="FindMappedTarget"/> uses for <paramref name="sourceValue"/>, or null when none applies.
        /// </summary>
        public static string FindMappedKey(IDictionary<string, string> userMap, string sourceValue)
        {
            if (userMap == null || string.IsNullOrEmpty(sourceValue))
            {
                return null;
            }
            if (userMap.ContainsKey(sourceValue))
            {
                return userMap.Keys.First(k => KeysEqual(userMap, k, sourceValue));
            }

            var sourceIsQualified = TryParse(sourceValue, out var sourceName, out var sourceUnique);
            var qualifiedKeys = userMap.Keys
                .Select(key => TryParse(key, out var name, out var unique) ? new { Key = key, Name = name, Unique = unique } : null)
                .Where(k => k != null)
                .ToList();

            if (sourceIsQualified)
            {
                var byUnique = qualifiedKeys.Where(k => string.Equals(k.Unique, sourceUnique, StringComparison.OrdinalIgnoreCase)).Take(2).ToList();
                if (byUnique.Count == 1)
                {
                    return byUnique[0].Key;
                }
                // A qualified source names one user, so a qualified key with another unique name must not match it.
                return userMap.ContainsKey(sourceName) ? userMap.Keys.First(k => KeysEqual(userMap, k, sourceName)) : null;
            }

            var byName = qualifiedKeys.Where(k => string.Equals(k.Name, sourceValue, StringComparison.OrdinalIgnoreCase)).Take(2).ToList();
            return byName.Count == 1 ? byName[0].Key : null;
        }

        // The dictionary decides key equality (it is case-insensitive), so the stored key is the one it would look up.
        private static bool KeysEqual(IDictionary<string, string> userMap, string storedKey, string lookup) =>
            userMap is Dictionary<string, string> d ? d.Comparer.Equals(storedKey, lookup) : string.Equals(storedKey, lookup, StringComparison.Ordinal);

        /// <summary>
        /// Returns true when a plain display name matches more than one <c>Display Name &lt;upn&gt;</c> key and no other key.
        /// </summary>
        public static bool IsAmbiguous(IDictionary<string, string> userMap, string sourceValue)
        {
            if (userMap == null || string.IsNullOrEmpty(sourceValue) || TryParse(sourceValue, out _, out _) || userMap.ContainsKey(sourceValue))
            {
                return false;
            }
            return userMap.Keys.Count(k => TryParse(k, out var name, out _) && string.Equals(name, sourceValue, StringComparison.OrdinalIgnoreCase)) > 1;
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
