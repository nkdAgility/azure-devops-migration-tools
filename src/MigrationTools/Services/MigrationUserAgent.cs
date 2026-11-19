using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace MigrationTools.Services
{
    /// <summary>
    /// The user agent that identifies the tool, its version and the run on every request sent to Azure DevOps,
    /// for example <c>AzureDevOpsMigrationTools/16.2.1 (run 3f2a9c41d0b7; wave-1; +https://devopsmigration.io)</c>.
    /// It never carries customer, project or user names, URLs or tokens: the only free text is the sanitised
    /// <c>UserAgentLabel</c> from the configuration.
    /// </summary>
    public static class MigrationUserAgent
    {
        public const string ProductName = "AzureDevOpsMigrationTools";
        public const string InfoUrl = "+https://devopsmigration.io";
        public const int MaxLabelLength = 32;
        public const string LabelConfigurationKey = "MigrationTools:UserAgentLabel";

        private static readonly object Sync = new object();
        private static string _version = "0.0.0";
        private static string _label;

        public static string RunId { get; } = Guid.NewGuid().ToString("N").Substring(0, 12);

        public static string Version => _version;

        public static string Label => _label;

        public static void Configure(string version, string label)
        {
            lock (Sync)
            {
                _version = SanitiseVersion(version);
                _label = SanitiseLabel(label);
            }
        }

        public static ProductInfoHeaderValue Product => new ProductInfoHeaderValue(ProductName, Version);

        public static ProductInfoHeaderValue Comment => new ProductInfoHeaderValue(BuildComment(RunId, Label));

        public static IReadOnlyList<ProductInfoHeaderValue> ProductValues => new[] { Product, Comment };

        public static string Value => BuildValue(Version, RunId, Label);

        public static string BuildValue(string version, string runId, string label)
        {
            return $"{ProductName}/{SanitiseVersion(version)} {BuildComment(runId, SanitiseLabel(label))}";
        }

        /// <summary>
        /// Adds the product values to the request's user agent, unless an earlier handler or client already did.
        /// </summary>
        public static void Apply(HttpHeaders headers)
        {
            var userAgent = headers is HttpRequestHeaders request ? request.UserAgent : null;
            if (userAgent == null || userAgent.Any(p => p.Product?.Name == ProductName))
            {
                return;
            }
            foreach (var value in ProductValues)
            {
                userAgent.Add(value);
            }
        }

        public static void Apply(HttpClient client) => Apply(client.DefaultRequestHeaders);

        public static string SanitiseLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return null;
            }
            var kept = new StringBuilder();
            foreach (var c in label.Trim())
            {
                if (IsLabelChar(c))
                {
                    kept.Append(c);
                }
                else if (char.IsWhiteSpace(c) && kept.Length > 0 && kept[kept.Length - 1] != '-')
                {
                    kept.Append('-');
                }
            }
            var result = kept.ToString().Trim('-', '.', '_');
            if (result.Length > MaxLabelLength)
            {
                result = result.Substring(0, MaxLabelLength).TrimEnd('-', '.', '_');
            }
            return result.Length == 0 ? null : result;
        }

        private static string SanitiseVersion(string version)
        {
            var kept = new string((version ?? string.Empty).Where(c => IsLabelChar(c) || c == '+').ToArray());
            return kept.Length == 0 ? "0.0.0" : kept;
        }

        private static string BuildComment(string runId, string label)
        {
            var parts = new List<string> { $"run {runId}" };
            if (!string.IsNullOrEmpty(label))
            {
                parts.Add(label);
            }
            parts.Add(InfoUrl);
            return $"({string.Join("; ", parts)})";
        }

        private static bool IsLabelChar(char c) =>
            (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '-' || c == '.' || c == '_';
    }
}
