using System;
using System.Collections.Generic;

namespace MigrationTools.Processors
{
    /// <summary>
    /// The state-stamped values of every revision of one work item, read through the work item object model's revision
    /// collection, used as a second, independent read of the same revisions (#3305). A long run could read a value as
    /// empty although the revision has it, and the empty value was saved and passed the check. Only the object model is
    /// used, so this works against every TFS version the tool supports, including those without a REST API.
    /// </summary>
    public sealed class SourceStateStampedValues
    {
        private readonly IReadOnlyDictionary<int, IDictionary<string, object>> _revisions;

        /// <param name="revisions">Each revision number (System.Rev) and its field values by reference name.</param>
        public SourceStateStampedValues(int workItemId, IReadOnlyDictionary<int, IDictionary<string, object>> revisions)
        {
            WorkItemId = workItemId;
            _revisions = revisions ?? throw new ArgumentNullException(nameof(revisions));
        }

        public int WorkItemId { get; }

        public bool HasRevision(int revision) => _revisions.ContainsKey(revision);

        /// <summary>The value of <paramref name="field"/> in <paramref name="revision"/>, or null when it has none.</summary>
        public object Value(int revision, string field)
        {
            if (!_revisions.TryGetValue(revision, out var fields))
            {
                throw new InvalidOperationException($"work item {WorkItemId} revision {revision} could not be read, so its state-stamped values cannot be checked.");
            }
            return fields != null && fields.TryGetValue(field, out var value) && !StateStampedFields.IsEmpty(value) ? value : null;
        }

        /// <summary>
        /// The value to replay for one state-stamped field from two reads of the same revision. A failed read can lose a
        /// value but never invents one, so when one read is empty and the other is not, the value is used and
        /// <paramref name="problem"/> says so (for a warning). Two different values cannot be decided: that throws, and
        /// the work item fails clearly instead of being saved with either. Two identities that are the same account in
        /// a different form (a bare display name and a qualified one) agree, and the first read's form is kept.
        /// </summary>
        public static object Reconcile(object firstRead, object secondRead, bool isIdentity, out string problem)
        {
            problem = null;
            var firstEmpty = StateStampedFields.IsEmpty(firstRead);
            var secondEmpty = StateStampedFields.IsEmpty(secondRead);
            if (firstEmpty && secondEmpty) return null;
            if (firstEmpty)
            {
                problem = $"one read of the revision gave an empty value, another gave '{secondRead}'";
                return secondRead;
            }
            if (secondEmpty)
            {
                problem = $"one read of the revision gave an empty value, another gave '{firstRead}'";
                return firstRead;
            }
            if (SameStoredValue(firstRead, secondRead, isIdentity)) return firstRead;
            throw new InvalidOperationException($"two reads of the same source revision gave different values ('{firstRead}' and '{secondRead}'), so the value to migrate cannot be decided");
        }

        /// <summary>
        /// True when the value read back from the server is the value that was saved. An identity saved as a bare display
        /// name can be read back qualified, so the same display name matches.
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

        /// <summary>Builds the lookup from revisions keyed by System.Rev.</summary>
        public static SourceStateStampedValues From(int workItemId, IEnumerable<(int Rev, IDictionary<string, object> Fields)> revisions)
        {
            var map = new Dictionary<int, IDictionary<string, object>>();
            foreach (var (rev, fields) in revisions) map[rev] = fields;
            return new SourceStateStampedValues(workItemId, map);
        }
    }
}
