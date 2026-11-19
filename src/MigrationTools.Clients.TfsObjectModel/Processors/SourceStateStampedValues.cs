using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Services.WebApi;

namespace MigrationTools.Processors
{
    /// <summary>
    /// The state-stamped values of every revision of one source work item, read over REST, used to check what the work
    /// item object model reads for the same revision (#3305). A long run could read a value as empty from the object model
    /// although the source revision has it, and the empty value was saved and passed the check. The REST values are what
    /// Azure DevOps holds, so the target is always given those.
    /// </summary>
    public sealed class SourceStateStampedValues
    {
        private readonly IReadOnlyDictionary<int, IDictionary<string, object>> _revisions;

        /// <param name="revisions">Each revision number and its REST fields (as returned by the work item revisions API).</param>
        public SourceStateStampedValues(int workItemId, IReadOnlyDictionary<int, IDictionary<string, object>> revisions)
        {
            WorkItemId = workItemId;
            _revisions = revisions ?? throw new ArgumentNullException(nameof(revisions));
        }

        public int WorkItemId { get; }

        public bool HasRevision(int revision) => _revisions.ContainsKey(revision);

        /// <summary>
        /// The value of <paramref name="field"/> in <paramref name="revision"/>, in the form the object model uses: a
        /// date as a <see cref="DateTime"/>, an identity as <c>Display Name &lt;unique name&gt;</c>. Null when the
        /// revision has no value; REST leaves out fields without one.
        /// </summary>
        public object Value(int revision, string field)
        {
            if (!_revisions.TryGetValue(revision, out var fields))
            {
                throw new InvalidOperationException($"source work item {WorkItemId} revision {revision} was not returned by Azure DevOps, so its state-stamped values cannot be checked.");
            }
            return fields != null && fields.TryGetValue(field, out var value) ? Normalise(value) : null;
        }

        /// <summary>Converts a REST field value to the form the object model reads.</summary>
        public static object Normalise(object value)
        {
            switch (value)
            {
                case null:
                    return null;
                case IdentityRef identity:
                    return FormatIdentity(identity.DisplayName, identity.UniqueName);
                case DateTimeOffset offset:
                    return offset.UtcDateTime;
                case DateTime date:
                    return date.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : date;
                case string s:
                    return StateStampedFields.IsEmpty(s) ? null : s;
                default:
                    return value;
            }
        }

        /// <summary><c>Display Name &lt;unique name&gt;</c>, or just the one that is set.</summary>
        public static string FormatIdentity(string displayName, string uniqueName)
        {
            var name = (displayName ?? "").Trim();
            var unique = (uniqueName ?? "").Trim();
            if (unique.Length == 0 || string.Equals(name, unique, StringComparison.OrdinalIgnoreCase)) return name.Length > 0 ? name : (unique.Length > 0 ? unique : null);
            if (name.Length == 0) return unique;
            return $"{name} <{unique}>";
        }

        /// <summary>
        /// The value to replay for one state-stamped field: what the object model read, unless Azure DevOps holds a
        /// different value for the same revision, in which case that value. <paramref name="problem"/> describes the
        /// difference (for the log) or is null when they agree. Two identities that are the same account in a different
        /// form (a bare display name and a qualified one) agree, and the object model's form is kept, as before.
        /// </summary>
        public static object Reconcile(object objectModelValue, object restValue, bool isIdentity, out string problem)
        {
            problem = null;
            var omEmpty = StateStampedFields.IsEmpty(objectModelValue);
            var restEmpty = StateStampedFields.IsEmpty(restValue);
            if (omEmpty && restEmpty) return null;
            if (omEmpty)
            {
                problem = $"the work item object model read it as empty, but Azure DevOps has '{restValue}'";
                return restValue;
            }
            if (restEmpty)
            {
                problem = $"the work item object model read '{objectModelValue}', but Azure DevOps has no value";
                return null;
            }
            if (StateStampedFields.SameValue(restValue, objectModelValue, isIdentity)) return objectModelValue;
            if (isIdentity && SameDisplayName(objectModelValue, restValue)) return objectModelValue;
            problem = $"the work item object model read '{objectModelValue}', but Azure DevOps has '{restValue}'";
            return restValue;
        }

        /// <summary>
        /// True when the value Azure DevOps stored (read over REST, normalised) is the value that was saved. An identity
        /// saved as a bare display name is stored qualified, so the same display name matches.
        /// </summary>
        public static bool SameStoredValue(object savedValue, object storedValue, bool isIdentity)
        {
            if (StateStampedFields.SameValue(savedValue, storedValue, isIdentity)) return true;
            return isIdentity && !StateStampedFields.IsEmpty(savedValue) && !StateStampedFields.IsEmpty(storedValue) && SameDisplayName(savedValue, storedValue);
        }

        private static bool SameDisplayName(object a, object b)
        {
            var (an, au) = StateStampedFields.SplitIdentity(Convert.ToString(a));
            var (bn, bu) = StateStampedFields.SplitIdentity(Convert.ToString(b));
            // Only when one side is bare: the object model leaves a display name unqualified unless several accounts share it.
            return (au == null || bu == null) && string.Equals(an, bn, StringComparison.Ordinal);
        }

        /// <summary>Builds the lookup from REST revisions, keyed by System.Rev.</summary>
        public static SourceStateStampedValues From(int workItemId, IEnumerable<(int Rev, IDictionary<string, object> Fields)> revisions)
        {
            var map = new Dictionary<int, IDictionary<string, object>>();
            foreach (var (rev, fields) in revisions) map[rev] = fields;
            return new SourceStateStampedValues(workItemId, map);
        }
    }
}
