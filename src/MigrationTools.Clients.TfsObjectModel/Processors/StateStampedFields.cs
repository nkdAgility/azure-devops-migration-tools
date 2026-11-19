using System;
using System.Collections.Generic;
using System.Linq;

namespace MigrationTools.Processors
{
    public enum IdentityMatch
    {
        Same,
        Different,
        /// <summary>The accounts behind the values could not be read, or a display name is shared, so neither can be proven.</summary>
        Unproven
    }

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
        /// <summary>Stamped with the server clock on every state change, and when the work item is created.</summary>
        public const string CommonStateChangeDate = "Microsoft.VSTS.Common.StateChangeDate";

        /// <summary>The state-stamped fields other than ClosedDate, which is resolved per work item type by <see cref="ResolveClosedDateField"/>.</summary>
        public static readonly IReadOnlyList<string> Fields = new[]
        {
            CommonActivatedDate, CommonResolvedDate, CommonStateChangeDate, CommonActivatedBy, CommonResolvedBy, CommonClosedBy
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

        /// <summary>
        /// As <see cref="SameValue(object, object)"/>, but for an identity field compares the identity rather than its
        /// presentation. When both sides carry a unique name (<c>Display Name &lt;unique name&gt;</c>) those must match,
        /// ignoring case; when neither does, the display names must match. A bare unique name (an email) matches the
        /// qualified form of the same account. A bare display name never matches a qualified one, because the object
        /// model qualifies only display names that several accounts share, so the account cannot be proven.
        /// </summary>
        public static bool SameValue(object sourceValue, object targetValue, bool isIdentity)
        {
            if (!isIdentity) return SameValue(sourceValue, targetValue);
            var sourceEmpty = IsEmpty(sourceValue);
            var targetEmpty = IsEmpty(targetValue);
            if (sourceEmpty || targetEmpty) return sourceEmpty && targetEmpty;
            var (sourceName, sourceUnique) = SplitIdentity(Convert.ToString(sourceValue));
            var (targetName, targetUnique) = SplitIdentity(Convert.ToString(targetValue));
            if (sourceUnique != null && targetUnique != null)
            {
                return string.Equals(sourceUnique, targetUnique, StringComparison.OrdinalIgnoreCase);
            }
            if (sourceUnique == null && targetUnique == null)
            {
                return string.Equals(sourceName, targetName, StringComparison.Ordinal);
            }
            // One side is bare. A bare unique name (for example an email in a user mapping) identifies the account. A bare
            // display name does not: the object model qualifies a name only when several accounts share it, so the
            // account cannot be proven and the values are treated as different (the work item fails clearly).
            var bare = sourceUnique == null ? sourceName : targetName;
            var unique = sourceUnique ?? targetUnique;
            return string.Equals(bare, unique, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Compares an expected identity with a saved one as accounts (#3290). Where <see cref="SameValue(object, object, bool)"/>
        /// cannot prove a match because one side is a bare display name and the other is qualified (for example a user
        /// mapping value <c>Display &lt;domain\upn&gt;</c> read back from the object model as <c>Display</c>), the bare name is
        /// resolved with <paramref name="accountsNamed"/>: the unique names of every account with that display name, or
        /// null when they cannot be read. The values are the same only when every such account carries the qualified
        /// unique name, and different only when none does; otherwise the result is <see cref="IdentityMatch.Unproven"/>.
        /// </summary>
        public static IdentityMatch CompareIdentity(object expected, object saved, Func<string, IReadOnlyList<IReadOnlyCollection<string>>> accountsNamed)
        {
            if (SameValue(expected, saved, isIdentity: true)) return IdentityMatch.Same;
            if (IsEmpty(expected) || IsEmpty(saved)) return IdentityMatch.Different;
            var (expectedName, expectedUnique) = SplitIdentity(Convert.ToString(expected));
            var (savedName, savedUnique) = SplitIdentity(Convert.ToString(saved));
            var expectedExplicit = expectedUnique != null;
            var savedExplicit = savedUnique != null;
            // A bare account (an email or domain\account) is taken as the account, but it may also be a display name, so
            // unless both unique names were written explicitly a difference is checked through the lookup.
            expectedUnique ??= LooksLikeAccount(expectedName) ? expectedName : null;
            savedUnique ??= LooksLikeAccount(savedName) ? savedName : null;
            if (expectedUnique != null && savedUnique != null)
            {
                if (SameUniqueName(expectedUnique, savedUnique)) return IdentityMatch.Same;
                if (expectedExplicit && savedExplicit) return IdentityMatch.Different;
                if (!expectedExplicit && !savedExplicit) return CompareBareAccounts(expectedName, savedName, accountsNamed);
            }
            else if (expectedUnique == null && savedUnique == null) return IdentityMatch.Different;
            var savedIsBare = !savedExplicit && (expectedExplicit || savedUnique == null);
            var bareName = savedIsBare ? savedName : expectedName;
            var unique = savedIsBare ? expectedUnique : savedUnique;
            var bareLooksLikeAccount = savedIsBare ? savedUnique != null : expectedUnique != null && !expectedExplicit;
            var accounts = accountsNamed?.Invoke(bareName);
            // An account-shaped value that names no display name is the account itself, and it is not this one.
            if (accounts == null) return IdentityMatch.Unproven;
            if (accounts.Count == 0) return bareLooksLikeAccount ? IdentityMatch.Different : IdentityMatch.Unproven;
            var carrying = accounts.Count(a => a != null && a.Any(u => SameUniqueName(u, unique)));
            if (carrying == accounts.Count) return IdentityMatch.Same;
            return carrying == 0 ? IdentityMatch.Different : IdentityMatch.Unproven;
        }

        /// <summary>
        /// True when two unique names name the same account, ignoring case. A <c>domain\upn</c> form matches the bare
        /// <c>upn</c> when the upn is an email, which is unique on its own; the domain part of an Entra ID identity is the
        /// tenant, which the identity service does not always return in the same form.
        /// </summary>
        public static bool SameUniqueName(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;
            a = a.Trim();
            b = b.Trim();
            if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase)) return true;
            var upnA = AfterDomain(a);
            var upnB = AfterDomain(b);
            return upnA.Contains("@") && string.Equals(upnA, upnB, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Two different account-shaped bare values: either may be a display name, so each is looked up in turn against
        /// the other. Different only when neither names a display name; unproven when a lookup cannot be read.
        /// </summary>
        private static IdentityMatch CompareBareAccounts(string expected, string saved, Func<string, IReadOnlyList<IReadOnlyCollection<string>>> accountsNamed)
        {
            var results = new List<IdentityMatch>();
            foreach (var (name, other) in new[] { (saved, expected), (expected, saved) })
            {
                var accounts = accountsNamed?.Invoke(name);
                if (accounts == null) return IdentityMatch.Unproven;
                if (accounts.Count == 0) continue;
                var carrying = accounts.Count(a => a != null && a.Any(u => SameUniqueName(u, other)));
                results.Add(carrying == accounts.Count ? IdentityMatch.Same : carrying == 0 ? IdentityMatch.Different : IdentityMatch.Unproven);
            }
            // Different only when every reading of the two values says so; readings that disagree prove nothing.
            if (results.Count == 0 || results.All(r => r == IdentityMatch.Different)) return IdentityMatch.Different;
            return results.All(r => r == IdentityMatch.Same) ? IdentityMatch.Same : IdentityMatch.Unproven;
        }

        private static bool LooksLikeAccount(string value) =>
            !string.IsNullOrEmpty(value) && !value.Contains(" ") && (value.Contains("@") || value.Contains("\\"));

        private static string AfterDomain(string unique)
        {
            var slash = unique.LastIndexOf('\\');
            return slash >= 0 ? unique.Substring(slash + 1) : unique;
        }

        /// <summary>Splits <c>Display Name &lt;unique name&gt;</c>; a value without the angle brackets has no unique name.</summary>
        public static (string DisplayName, string UniqueName) SplitIdentity(string value)
        {
            value = (value ?? "").Trim();
            var open = value.LastIndexOf('<');
            if (open >= 0 && value.EndsWith(">", StringComparison.Ordinal))
            {
                var unique = value.Substring(open + 1, value.Length - open - 2).Trim();
                if (unique.Length > 0) return (value.Substring(0, open).Trim(), unique);
            }
            return (value, null);
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
