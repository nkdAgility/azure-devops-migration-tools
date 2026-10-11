using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace MigrationTools.Tools.Infrastructure
{
    /// <summary>
    /// Resolves the file name to save an embedded image under from the attachment URL found in a
    /// work item field.
    /// </summary>
    /// <remarks>
    /// Attachment URLs carry the original name in a <c>FileName</c> query parameter that is percent
    /// encoded, and when the URL came from an HTML field the whole URL is HTML encoded on top of
    /// that. Both layers have to be removed to recover the name the user originally uploaded,
    /// and the result has to be reduced to a bare file name before it is combined with a path.
    /// </remarks>
    public static class EmbededImageFileNameExtractor
    {
        private const string FileNameParameter = "FileName";

        /// <summary>
        /// Characters that may not appear in a file name. Built from the running platform's list
        /// unioned with the Windows list and the control characters, so that a name taken from a
        /// URL is rejected consistently no matter which platform the migration runs on.
        /// </summary>
        private static readonly HashSet<char> InvalidFileNameChars = new HashSet<char>(
            Path.GetInvalidFileNameChars()
                .Concat(new[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' })
                .Concat(Enumerable.Range(0, 32).Select(value => (char)value)));

        /// <summary>
        /// Base names Windows reserves for devices, which remain reserved even with an extension
        /// (<c>CON.png</c> is still the console device).
        /// </summary>
        private static readonly HashSet<string> ReservedDeviceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
        };

        /// <summary>
        /// Returns the decoded, sanitised file name held in the URL's <c>FileName</c> query
        /// parameter.
        /// </summary>
        /// <param name="url">The attachment URL as it appears in the work item field.</param>
        /// <returns>
        /// The file name, or null when the URL carries no <c>FileName</c> parameter or nothing
        /// usable survives sanitisation.
        /// </returns>
        public static string GetFileNameFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            // A URL taken from an HTML field still has its entities in place, so "&amp;FileName="
            // has to become "&FileName=" before the query can be split on "&".
            string decodedUrl = WebUtility.HtmlDecode(url);

            string rawFileName = GetQueryParameterValue(decodedUrl, FileNameParameter);
            if (rawFileName == null)
                return null;

            // Query values are percent encoded, so "my%20image.png" has to come back as
            // "my image.png" for the target attachment to keep its original name.
            string fileName = WebUtility.UrlDecode(rawFileName);

            return SanitiseFileName(fileName);
        }

        private static string GetQueryParameterValue(string url, string parameterName)
        {
            int queryStart = url.IndexOf('?');
            if (queryStart < 0 || queryStart == url.Length - 1)
                return null;

            string query = url.Substring(queryStart + 1);

            int fragmentStart = query.IndexOf('#');
            if (fragmentStart >= 0)
                query = query.Substring(0, fragmentStart);

            foreach (string pair in query.Split('&'))
            {
                int separator = pair.IndexOf('=');
                if (separator <= 0)
                    continue;

                // The whole key is compared so that FileNameGuid= is not mistaken for FileName=,
                // and only the first "=" splits the pair so that "=" inside the value survives.
                if (!string.Equals(pair.Substring(0, separator), parameterName, StringComparison.OrdinalIgnoreCase))
                    continue;

                return pair.Substring(separator + 1);
            }

            return null;
        }

        private static string SanitiseFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            // Decoding can reveal separators (%2F, %5C) and traversal segments that were hidden
            // while encoded. Anything resembling a directory is dropped so that the name can never
            // escape the folder it is about to be combined with.
            int lastSeparator = fileName.LastIndexOfAny(new[] { '/', '\\' });
            if (lastSeparator >= 0)
                fileName = fileName.Substring(lastSeparator + 1);

            fileName = new string(fileName.Where(character => !InvalidFileNameChars.Contains(character)).ToArray());

            // Trailing dots and spaces are silently dropped by Windows, so a name that is only
            // made up of them is not usable.
            fileName = fileName.Trim().TrimEnd('.', ' ');

            if (fileName.Length == 0)
                return null;

            // Windows reserves device names even when an extension follows, so CON.png would be
            // opened as the console device rather than created as a file.
            int firstDot = fileName.IndexOf('.');
            string baseName = firstDot >= 0 ? fileName.Substring(0, firstDot) : fileName;
            if (ReservedDeviceNames.Contains(baseName.TrimEnd()))
                return null;

            return fileName;
        }
    }
}
