using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MigrationTools.Processors.Infrastructure
{
    /// <summary>
    /// Rewrites the WIQL of a shared query so that it points at the target project. Team-scoped macros such as
    /// <c>@CurrentIteration('[Project]\Team')</c> and <c>@TeamAreas</c> carry the project name in square brackets,
    /// which the plain <c>'Project</c> replacement does not reach.
    /// </summary>
    public static class SharedQueryWiqlRewriter
    {
        private static readonly Regex TeamReference = new Regex(@"'\[(?<project>[^\]]+)\]\\(?<team>[^']+)'", RegexOptions.Compiled);
        private static readonly Regex TeamId = new Regex(@"\s*<id:[^>]*>$", RegexOptions.Compiled);

        public static string Rewrite(string wiql, string sourceProject, string targetProject, bool prefixProjectToNodes, IDictionary<string, string> fieldMappings)
        {
            // the ' should only items at the start of areapath etc.
            var fixedQueryText = wiql.Replace($"'{sourceProject}", $"'{targetProject}");

            if (prefixProjectToNodes)
            {
                // we need to inject the team name as a folder in the structure too
                fixedQueryText = fixedQueryText.Replace($"{targetProject}\\", $"{targetProject}\\{sourceProject}\\");
            }

            // team context, e.g. @CurrentIteration('[Source]\Team') becomes @CurrentIteration('[Target]\Team')
            // the server may append the source team id, e.g. '[Source]\Team <id:guid>', which is meaningless in the target
            fixedQueryText = TeamReference.Replace(fixedQueryText, m => m.Groups["project"].Value == sourceProject
                ? $"'[{targetProject}]\\{TeamId.Replace(m.Groups["team"].Value, string.Empty)}'"
                : m.Value);

            if (fieldMappings != null)
            {
                foreach (var sourceField in fieldMappings.Keys)
                {
                    fixedQueryText = fixedQueryText.Replace(sourceField, fieldMappings[sourceField]);
                }
            }
            return fixedQueryText;
        }

        /// <summary>
        /// Points every reference to a team in the given project at another team of that project.
        /// </summary>
        public static string ReplaceTeam(string wiql, string project, string fromTeam, string toTeam)
        {
            return wiql.Replace($"'[{project}]\\{fromTeam}'", $"'[{project}]\\{toTeam}'");
        }

        /// <summary>
        /// Returns the team names referenced in team-scoped macros for the given project.
        /// </summary>
        public static IEnumerable<string> GetReferencedTeams(string wiql, string project)
        {
            return TeamReference.Matches(wiql).Cast<Match>()
                .Where(m => m.Groups["project"].Value == project)
                .Select(m => m.Groups["team"].Value)
                .Distinct();
        }
    }
}
