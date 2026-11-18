using System;
using System.Collections.Generic;
using System.Linq;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Decides how to keep exactly one hyperlink from a migrated work item back to its source work item when
    /// GenerateMigrationHyperlink is on. The link is added in a revision that is saved anyway, never in a save of its own.
    /// </summary>
    public static class MigrationHyperlinkHelper
    {
        /// <summary>The comment that marks the hyperlink added by the migration.</summary>
        public const string Comment = "Migrated from source";

        /// <summary>True when the hyperlink carries the migration comment.</summary>
        public static bool IsMigrationHyperlink(string comment) => string.Equals(comment?.Trim(), Comment, StringComparison.Ordinal);

        /// <summary>
        /// True only for the backlink the migration generates: the migration comment and a location equal to the source work
        /// item's web URL. A copied hyperlink that happens to carry the same comment but points elsewhere is not the backlink.
        /// </summary>
        public static bool IsGeneratedBacklink(string location, string comment, string sourceUrl) =>
            IsMigrationHyperlink(comment) && !string.IsNullOrWhiteSpace(sourceUrl) && Normalise(location) == Normalise(sourceUrl);

        /// <summary>
        /// Works out which existing target hyperlinks to remove and whether to add the link to <paramref name="sourceUrl"/>.
        /// A migration hyperlink that points elsewhere is stale (the source has moved) and is removed, unless the source work
        /// item has that same hyperlink itself (it was copied from the source, for example when the source was itself migrated).
        /// </summary>
        /// <param name="targetHyperlinks">The target's hyperlinks as (location, comment).</param>
        /// <param name="sourceHyperlinkLocations">The locations of the source work item's own hyperlinks.</param>
        /// <param name="sourceUrl">The web URL of the source work item.</param>
        /// <returns>The indexes in <paramref name="targetHyperlinks"/> to remove, and whether to add the link.</returns>
        public static (IReadOnlyList<int> Remove, bool Add) Plan(
            IList<(string Location, string Comment)> targetHyperlinks,
            IEnumerable<string> sourceHyperlinkLocations,
            string sourceUrl)
        {
            var wanted = Normalise(sourceUrl);
            var fromSource = new HashSet<string>((sourceHyperlinkLocations ?? Enumerable.Empty<string>()).Select(Normalise));
            var remove = new List<int>();
            var present = false;
            for (var i = 0; i < targetHyperlinks.Count; i++)
            {
                var location = Normalise(targetHyperlinks[i].Location);
                if (location == wanted)
                {
                    present = true;
                    continue;
                }
                if (IsMigrationHyperlink(targetHyperlinks[i].Comment) && !fromSource.Contains(location))
                {
                    remove.Add(i);
                }
            }
            return (remove, !present);
        }

        private static string Normalise(string location)
        {
            var trimmed = (location ?? string.Empty).Trim().Trim('"');
            return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) ? uri.AbsoluteUri.ToLowerInvariant() : trimmed.ToLowerInvariant();
        }
    }
}
