using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Rewrites shared parameter references in a Test Case's Microsoft.VSTS.TCM.LocalDataSource JSON.
    /// </summary>
    public static class SharedParameterDataSource
    {
        private static readonly Regex SingleId = new("(\"(?:sharedParameterDataSetId|dataSetId)\"\\s*:\\s*)(\\d+)", RegexOptions.Compiled);
        private static readonly Regex IdList = new("(\"sharedParameterDataSetIds\"\\s*:\\s*\\[)([\\d,\\s]*)(\\])", RegexOptions.Compiled);
        private static readonly Regex Number = new("\\d+", RegexOptions.Compiled);

        /// <summary>
        /// Replaces every source shared parameter ID in <c>sharedParameterDataSetId</c>, <c>sharedParameterDataSetIds</c>
        /// and <c>parameterMap[].dataSetId</c> with its target ID in one pass, so an ID that is both a target and a
        /// source ID is translated only once. IDs without a mapping are left unchanged.
        /// </summary>
        public static string Remap(string localDataSource, IReadOnlyDictionary<string, string> sourceToTarget)
        {
            if (string.IsNullOrEmpty(localDataSource) || sourceToTarget.Count == 0)
            {
                return localDataSource;
            }
            string Map(string id) => sourceToTarget.TryGetValue(id, out var target) ? target : id;
            var result = SingleId.Replace(localDataSource, m => m.Groups[1].Value + Map(m.Groups[2].Value));
            return IdList.Replace(result, m => m.Groups[1].Value + Number.Replace(m.Groups[2].Value, n => Map(n.Value)) + m.Groups[3].Value);
        }
    }
}
