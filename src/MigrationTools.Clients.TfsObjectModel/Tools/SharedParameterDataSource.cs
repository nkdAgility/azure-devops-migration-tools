using System.Text.RegularExpressions;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Rewrites shared parameter references in a Test Case's Microsoft.VSTS.TCM.LocalDataSource JSON.
    /// </summary>
    public static class SharedParameterDataSource
    {
        /// <summary>
        /// Replaces <c>"dataSetId":sourceId</c> entries in the parameter map with the target ID.
        /// </summary>
        public static string RemapDataSetId(string localDataSource, string sourceId, string targetId)
        {
            if (string.IsNullOrEmpty(localDataSource))
            {
                return localDataSource;
            }
            return Regex.Replace(localDataSource, "(\"dataSetId\"\\s*:\\s*)" + Regex.Escape(sourceId) + "(?!\\d)", "${1}" + targetId);
        }
    }
}
