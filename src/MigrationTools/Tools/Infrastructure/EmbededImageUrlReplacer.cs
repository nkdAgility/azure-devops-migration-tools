using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MigrationTools.Tools.Infrastructure
{
    /// <summary>
    /// Rewrites source image URLs in a field value to their uploaded target URLs.
    /// </summary>
    /// <remarks>
    /// Replacing each URL in turn with <c>string.Replace</c> is not safe. When one source URL is a
    /// prefix of another (<c>.../attachments/{guid}</c> and <c>.../attachments/{guid}?fileName=a.png</c>),
    /// the shorter one is also replaced inside the longer, leaving the longer one's query string
    /// appended to the new URL, which already carries its own <c>?fileName=</c>. Text that an
    /// earlier replacement has already produced could also be matched again. This rewrite is a single
    /// pass that matches the longest URL first and only where the URL is not followed by more URL
    /// characters, so each occurrence is replaced exactly once.
    /// </remarks>
    public static class EmbededImageUrlReplacer
    {
        private const string UrlChar = "[\\w/?=%~+#-]";

        private const string UrlContinuesLookahead =
            "(?!" + UrlChar + "|[.,;:!@$*]" + UrlChar + "|&amp;|&(?![A-Za-z][A-Za-z0-9]*;|#[0-9]+;|#x[0-9A-Fa-f]+;))";

        /// <summary>
        /// Replaces every occurrence of each key in <paramref name="replacements"/> with its value.
        /// Entries whose value is null or whitespace are left unchanged.
        /// </summary>
        public static string ReplaceUrls(string fieldValue, IDictionary<string, string> replacements)
        {
            if (string.IsNullOrEmpty(fieldValue) || replacements == null)
                return fieldValue;

            var active = replacements
                .Where(r => !string.IsNullOrEmpty(r.Key) && !string.IsNullOrWhiteSpace(r.Value))
                .OrderByDescending(r => r.Key.Length)
                .ToList();
            if (active.Count == 0)
                return fieldValue;

            // a URL must not continue past the match, otherwise .../{guid} would match the start
            // of .../{guid}?fileName=a.png or .../{guid}0. prose punctuation only continues a URL
            // when more URL characters follow it (".png" does, ". See" does not), and an ampersand
            // only does as &amp; or a bare &, not as an entity that ends the URL such as &nbsp;.
            string pattern = "(?:" + string.Join("|", active.Select(r => Regex.Escape(r.Key))) + ")" + UrlContinuesLookahead;
            var lookup = active.ToDictionary(r => r.Key, r => r.Value, StringComparer.Ordinal);

            return Regex.Replace(fieldValue, pattern, m => lookup[m.Value]);
        }
    }
}
