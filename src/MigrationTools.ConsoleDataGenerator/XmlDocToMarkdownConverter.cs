using System;
using System.Text.RegularExpressions;
using Elmah.Io.Client;

namespace MigrationTools.ConsoleDataGenerator
{
    /// <summary>
    /// Converts XML documentation comments to Markdown format.
    /// Handles common XML doc tags like &lt;c&gt;, &lt;see cref&gt;, &lt;para&gt;, &lt;list&gt;, etc.
    /// </summary>
    public static class XmlDocToMarkdownConverter
    {
        /// <summary>
        /// Converts XML documentation text to Markdown format.
        /// </summary>
        /// <param name="xmlDoc">The XML documentation text to convert.</param>
        /// <returns>Markdown formatted text.</returns>
        public static string Convert(string xmlDoc)
        {
            if (string.IsNullOrEmpty(xmlDoc))
                return xmlDoc;

            if (xmlDoc.Contains("example by using"))
            {
                Console.WriteLine("Stop here");
            }

            var result = xmlDoc;

            // Handle <para>...</para> -> paragraph breaks (do this first to preserve structure)
            result = Regex.Replace(result, @"<para>(.*?)</para>", "\n\n$1\n\n", RegexOptions.Singleline);

            // Handle <c>...</c> -> `...`
            result = Regex.Replace(result, @"<c>(.*?)</c>", "`$1`", RegexOptions.Singleline);

            // Handle <code>...</code> -> ```...```
            result = Regex.Replace(result, @"<code>(.*?)</code>", "```$1```", RegexOptions.Singleline);

            // Handle <see cref="..."/> -> `...` (extract just the type/member name)
            result = Regex.Replace(result, @"<see cref=""([^""]+)""\s*/>", match =>
            {
                var cref = match.Groups[1].Value;
                // Extract just the last part after the last dot for readability
                var simpleName = cref.Contains('.') ? cref.Split('.').Last() : cref;
                return $"`{simpleName}`";
            });

            // Handle <see langword="..."/> -> `...`
            result = Regex.Replace(result, @"<see langword=""([^""]+)""\s*/>", "`$1`");

            // Handle <paramref name="..."/> -> `...`
            result = Regex.Replace(result, @"<paramref name=""([^""]+)""\s*/>", "`$1`");

            // Handle <typeparamref name="..."/> -> `...`
            result = Regex.Replace(result, @"<typeparamref name=""([^""]+)""\s*/>", "`$1`");

            // Handle <list type="bullet">...</list> with <item><description>...</description></item>
            result = Regex.Replace(result, @"<list type=""bullet"">(.*?)</list>", match =>
            {
                var listContent = match.Groups[1].Value;
                // Convert <item><description>...</description></item> to - ...
                var items = Regex.Replace(listContent, @"<item><description>(.*?)</description></item>", "- $1", RegexOptions.Singleline);
                return $"\n{items}\n";
            }, RegexOptions.Singleline);

            // Handle <list type="number">...</list> with <item><description>...</description></item>
            result = Regex.Replace(result, @"<list type=""number"">(.*?)</list>", match =>
            {
                var listContent = match.Groups[1].Value;
                var itemMatches = Regex.Matches(listContent, @"<item><description>(.*?)</description></item>", RegexOptions.Singleline);
                var items = "";
                for (int i = 0; i < itemMatches.Count; i++)
                {
                    items += $"{i + 1}. {itemMatches[i].Groups[1].Value}\n";
                }
                return $"\n{items}";
            }, RegexOptions.Singleline);

            // Handle <returns>...</returns> -> **Returns:** ...
            result = Regex.Replace(result, @"<returns>(.*?)</returns>", "**Returns:** $1", RegexOptions.Singleline);

            // Handle <example>...</example> -> **Example:** ...
            result = Regex.Replace(result, @"<example>(.*?)</example>", "**Example:**\n$1", RegexOptions.Singleline);

            // Handle <remarks>...</remarks> -> **Remarks:** ...
            result = Regex.Replace(result, @"<remarks>(.*?)</remarks>", "**Remarks:** $1", RegexOptions.Singleline);

            // Handle <exception cref="...">...</exception> -> **Throws `...`:** ...
            result = Regex.Replace(result, @"<exception cref=""([^""]+)"">(.*?)</exception>", match =>
            {
                var exceptionType = match.Groups[1].Value.Split('.').Last();
                var description = match.Groups[2].Value;
                return $"**Throws `{exceptionType}`:** {description}";
            }, RegexOptions.Singleline);

            // Remove any remaining XML tags
            result = Regex.Replace(result, @"<[^>]*>", "", RegexOptions.Singleline);

            // Clean up extra whitespace and newlines (but preserve individual spaces between words)
            result = Regex.Replace(result, @"[ \t]+", " "); // Multiple spaces/tabs to single space
            result = Regex.Replace(result, @"\n\s*\n", "\n\n"); // Multiple newlines to double newlines
            result = result.Trim();

            return result;
        }

        /// <summary>
        /// Converts XML documentation text to a single-line Markdown format suitable for summaries.
        /// Removes paragraph breaks and list formatting for inline use.
        /// </summary>
        /// <param name="xmlDoc">The XML documentation text to convert.</param>
        /// <returns>Single-line Markdown formatted text.</returns>
        public static string ConvertToSingleLine(string xmlDoc)
        {
            if (string.IsNullOrEmpty(xmlDoc))
                return xmlDoc;

            // First apply full conversion
            var result = Convert(xmlDoc);
            
            // Then collapse to single line
            result = Regex.Replace(result, @"\n+", " "); // Replace newlines with spaces
            result = Regex.Replace(result, @"\s+", " "); // Multiple spaces to single space
            
            return result.Trim();
        }
    }
}
