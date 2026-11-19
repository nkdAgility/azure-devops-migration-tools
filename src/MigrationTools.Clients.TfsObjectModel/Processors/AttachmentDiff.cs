using System;
using System.Collections.Generic;
using System.Linq;

namespace MigrationTools.Processors
{
    public static class AttachmentDiff
    {
        /// <summary>
        /// The first source attachment, by name and size, that the target does not have as often as the source (#3299).
        /// </summary>
        /// <returns>The attachment's name, or null when the target has them all.</returns>
        public static string FirstMissing(IEnumerable<(string Name, long Length)> source, IEnumerable<(string Name, long Length)> target)
        {
            var available = target
                .GroupBy(a => (Name: a.Name?.ToLowerInvariant(), a.Length))
                .ToDictionary(g => g.Key, g => g.Count());
            foreach (var attachment in source)
            {
                var key = (Name: attachment.Name?.ToLowerInvariant(), attachment.Length);
                if (!available.TryGetValue(key, out var count) || count == 0)
                {
                    return attachment.Name ?? string.Empty;
                }
                available[key] = count - 1;
            }
            return null;
        }
    }
}
