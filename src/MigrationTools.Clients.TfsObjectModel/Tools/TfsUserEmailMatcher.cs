using System;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.DataContracts;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Matches source users to target users by email address, optionally tolerating a domain change
    /// (for example <c>first.last@old.com</c> to <c>first.last@new.com</c>).
    /// </summary>
    public static class TfsUserEmailMatcher
    {
        /// <summary>
        /// Returns the single target user whose email matches the source user's email, or null when there is
        /// no match or the match is ambiguous. Matching is attempted in order: exact address, address after
        /// applying <paramref name="domainMappings"/>, and (when <paramref name="matchByLocalPart"/> is set) local part only.
        /// </summary>
        public static IdentityItemData FindTarget(
            IdentityItemData sourceUser,
            IEnumerable<IdentityItemData> targetUsers,
            IDictionary<string, string> domainMappings,
            bool matchByLocalPart)
        {
            var sourceMail = sourceUser?.MailAddress;
            if (string.IsNullOrWhiteSpace(sourceMail) || targetUsers == null)
            {
                return null;
            }
            var targets = targetUsers.Where(t => !string.IsNullOrWhiteSpace(t?.MailAddress)).ToList();

            var exact = targets.Where(t => t.MailAddress.Equals(sourceMail, StringComparison.OrdinalIgnoreCase)).Take(2).ToList();
            if (exact.Count > 0)
            {
                return exact.Count == 1 ? exact[0] : null;
            }

            var rewritten = RewriteDomain(sourceMail, domainMappings);
            if (rewritten != null)
            {
                var match = Single(targets, t => t.MailAddress.Equals(rewritten, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    return match;
                }
            }

            if (matchByLocalPart)
            {
                var local = LocalPart(sourceMail);
                if (!string.IsNullOrEmpty(local))
                {
                    return Single(targets, t => string.Equals(LocalPart(t.MailAddress), local, StringComparison.OrdinalIgnoreCase));
                }
            }
            return null;
        }

        /// <summary>
        /// Rewrites the domain of <paramref name="email"/> using <paramref name="domainMappings"/> (source domain to target domain,
        /// case-insensitive). Returns null when no mapping applies, or when keys differing only by case map to different domains.
        /// </summary>
        public static string RewriteDomain(string email, IDictionary<string, string> domainMappings)
        {
            if (string.IsNullOrWhiteSpace(email) || domainMappings == null || domainMappings.Count == 0)
            {
                return null;
            }
            var at = email.LastIndexOf('@');
            if (at <= 0 || at == email.Length - 1)
            {
                return null;
            }
            var domain = email.Substring(at + 1);
            var targetDomains = domainMappings
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Value)
                    && domain.Equals(kvp.Key.Trim().TrimStart('@'), StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Value.Trim().TrimStart('@'))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(2)
                .ToList();
            return targetDomains.Count == 1 ? email.Substring(0, at + 1) + targetDomains[0] : null;
        }

        private static string LocalPart(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }
            var at = email.LastIndexOf('@');
            return at > 0 ? email.Substring(0, at) : null;
        }

        private static IdentityItemData Single(List<IdentityItemData> targets, Func<IdentityItemData, bool> predicate)
        {
            var candidates = targets.Where(predicate).Take(2).ToList();
            return candidates.Count == 1 ? candidates[0] : null;
        }
    }
}
