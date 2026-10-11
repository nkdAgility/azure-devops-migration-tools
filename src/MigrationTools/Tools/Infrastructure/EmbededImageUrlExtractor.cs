using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MigrationTools.Tools.Infrastructure
{
    /// <summary>
    /// Locates embedded image URLs within work item field content.
    /// </summary>
    /// <remarks>
    /// Azure DevOps stores rich text fields such as System.Description and System.History as
    /// either HTML or Markdown. The declared <c>FieldType</c> stays <c>Html</c> in both cases -
    /// only the content format changes - so both formats have to be looked for in the same fields.
    /// </remarks>
    public static class EmbededImageUrlExtractor
    {
        /// <summary>
        /// Matches the URL of an HTML embedded image, e.g. <c>&lt;img src="url"&gt;</c>. The lookbehind
        /// means the whole match is the URL, so there is no capture group.
        /// </summary>
        public const string RegexPatternForHtmlImageUrl = "(?<=<img.*?src=\")[^\"]*";

        /// <summary>
        /// Matches a Markdown embedded image, e.g. <c>![alt](url)</c>, capturing the URL in group 1.
        /// Handles the optional double or single quoted title (<c>![alt](url "title")</c>), the
        /// angle bracket form (<c>![alt](&lt;url&gt;)</c>), and URLs containing one level of
        /// balanced parentheses (<c>![alt](https://host/image_(1).png)</c>).
        /// </summary>
        public const string RegexPatternForMarkdownImageUrl = "!\\[[^\\]]*\\]\\(\\s*<?((?:[^()\\s>]|\\([^()\\s>]*\\))+)>?(?:\\s+(?:\"[^\"]*\"|'[^']*'))?\\s*\\)";

        /// <summary>
        /// Returns every embedded image URL found in the supplied field value, in the order they
        /// were found, with duplicates removed. HTML and Markdown images are both returned.
        /// </summary>
        /// <param name="fieldValue">The raw field value. Null or empty returns an empty list.</param>
        /// <returns>The distinct image URLs, never null.</returns>
        public static IList<string> ExtractImageUrls(string fieldValue)
        {
            var urls = new List<string>();
            if (string.IsNullOrEmpty(fieldValue))
            {
                return urls;
            }

            // Ordinal so that URLs differing only by case are both repaired; the upload cache
            // treats them as the same image, but each distinct spelling still needs replacing.
            var seen = new HashSet<string>(StringComparer.Ordinal);

            AddMatches(fieldValue, RegexPatternForHtmlImageUrl, urls, seen);
            AddMatches(fieldValue, RegexPatternForMarkdownImageUrl, urls, seen);

            return urls;
        }

        private static void AddMatches(string fieldValue, string pattern, IList<string> urls, HashSet<string> seen)
        {
            foreach (Match match in Regex.Matches(fieldValue, pattern))
            {
                if (!match.Success)
                    continue;

                // The Markdown pattern captures the URL in group 1; the HTML pattern uses a
                // lookbehind so the match itself is already the URL.
                string url = match.Groups.Count > 1 && match.Groups[1].Success
                    ? match.Groups[1].Value
                    : match.Value;

                if (string.IsNullOrWhiteSpace(url))
                    continue;

                if (seen.Add(url))
                    urls.Add(url);
            }
        }
    }
}
