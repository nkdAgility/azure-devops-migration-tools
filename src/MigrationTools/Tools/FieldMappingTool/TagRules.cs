using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MigrationTools.Tools
{
    /// <summary>
    /// The tag matching shared by the tag field maps. An entry matches a tag by exact, case-sensitive
    /// name, or, when it starts with <c>regex:</c>, by the .NET regular expression that follows.
    /// </summary>
    public static class TagRules
    {
        public const string RegexPrefix = "regex:";
        private const char Separator = ';';

        public static List<string> Split(string tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
            {
                return new List<string>();
            }
            return tags.Split(Separator)
                .Select(tag => tag.Trim())
                .Where(tag => tag.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        public static string Join(IEnumerable<string> tags)
        {
            return string.Join(Separator.ToString(), tags);
        }

        /// <summary>
        /// Throws <see cref="ArgumentException"/> naming the entry when an entry is empty or its pattern does not compile.
        /// </summary>
        public static void Validate(IEnumerable<string> entries, string settingName)
        {
            foreach (var entry in entries ?? Enumerable.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(entry) || (IsPattern(entry) && Pattern(entry).Length == 0))
                {
                    throw new ArgumentException($"{settingName} contains an empty entry.");
                }
                if (IsPattern(entry))
                {
                    try
                    {
                        _ = new Regex(Pattern(entry));
                    }
                    catch (ArgumentException ex)
                    {
                        throw new ArgumentException($"{settingName} entry '{entry}' is not a valid regular expression: {ex.Message}", ex);
                    }
                }
            }
        }

        public static bool Matches(string tag, string entry)
        {
            if (IsPattern(entry))
            {
                return Regex.IsMatch(tag, Pattern(entry));
            }
            return string.Equals(tag, entry.Trim(), StringComparison.Ordinal);
        }

        public static bool MatchesAny(string tag, IEnumerable<string> entries)
        {
            return entries != null && entries.Any(entry => Matches(tag, entry));
        }

        /// <summary>
        /// A tag is kept when it matches no exclude entry and, if there are include entries, matches one of them.
        /// </summary>
        public static TagFilterResult Filter(IEnumerable<string> tags, IList<string> includeTags, IList<string> excludeTags)
        {
            var result = new TagFilterResult();
            foreach (var tag in tags)
            {
                var included = includeTags == null || includeTags.Count == 0 || MatchesAny(tag, includeTags);
                if (included && !MatchesAny(tag, excludeTags))
                {
                    result.Kept.Add(tag);
                }
                else
                {
                    result.Dropped.Add(tag);
                }
            }
            return result;
        }

        /// <summary>
        /// Each tag is renamed by the first mapping whose source matches it. A pattern source is replaced
        /// with <see cref="Regex.Replace(string, string, string)"/>, so the target may use groups such as <c>$1</c>.
        /// </summary>
        public static TagMapResult Map(IEnumerable<string> tags, IList<TagMapping> mappings)
        {
            var result = new TagMapResult();
            foreach (var tag in tags)
            {
                var mapping = mappings?.FirstOrDefault(m => Matches(tag, m.source));
                var newTag = tag;
                if (mapping != null)
                {
                    newTag = (IsPattern(mapping.source)
                        ? Regex.Replace(tag, Pattern(mapping.source), mapping.target)
                        : mapping.target).Trim();
                    if (newTag.Length == 0)
                    {
                        newTag = tag;
                    }
                }
                if (newTag.Length > 0 && !result.Tags.Contains(newTag, StringComparer.Ordinal))
                {
                    result.Tags.Add(newTag);
                }
                if (!string.Equals(newTag, tag, StringComparison.Ordinal))
                {
                    result.Renamed.Add(new KeyValuePair<string, string>(tag, newTag));
                }
            }
            return result;
        }

        private static bool IsPattern(string entry)
        {
            return entry.StartsWith(RegexPrefix, StringComparison.Ordinal);
        }

        private static string Pattern(string entry)
        {
            return entry.Substring(RegexPrefix.Length);
        }
    }

    public class TagFilterResult
    {
        public List<string> Kept { get; } = new List<string>();
        public List<string> Dropped { get; } = new List<string>();
    }

    public class TagMapResult
    {
        public List<string> Tags { get; } = new List<string>();
        public List<KeyValuePair<string, string>> Renamed { get; } = new List<KeyValuePair<string, string>>();
    }
}
