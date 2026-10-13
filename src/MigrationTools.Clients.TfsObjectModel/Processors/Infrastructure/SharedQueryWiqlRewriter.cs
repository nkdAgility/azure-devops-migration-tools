using System;
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
        // Only the argument of a team-scoped macro is a team reference; other '[x]\y' string literals are left alone.
        private static readonly Regex TeamReference = new Regex(@"(?<macro>@(?:CurrentIteration|TeamAreas)\s*\(\s*)'\[(?<project>[^\]]+)\]\\(?<team>[^']+)'", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex TeamId = new Regex(@"\s*<id:[^>]*>$", RegexOptions.Compiled);

        public static string Rewrite(string wiql, string sourceProject, string targetProject, bool prefixProjectToNodes, IDictionary<string, string> fieldMappings)
        {
            // the ' should only items at the start of areapath etc.; WIQL is case-insensitive
            var fixedQueryText = Regex.Replace(wiql, "'" + Regex.Escape(sourceProject), "'" + targetProject.Replace("$", "$$"), RegexOptions.IgnoreCase);

            if (prefixProjectToNodes)
            {
                // we need to inject the team name as a folder in the structure too
                fixedQueryText = fixedQueryText.Replace($"{targetProject}\\", $"{targetProject}\\{sourceProject}\\");
            }

            // team context, e.g. @CurrentIteration('[Source]\Team') becomes @CurrentIteration('[Target]\Team')
            // the server may append the source team id, e.g. '[Source]\Team <id:guid>', which is meaningless in the target
            fixedQueryText = TeamReference.Replace(fixedQueryText, m => IsProject(m, sourceProject)
                ? $"{m.Groups["macro"].Value}'[{targetProject}]\\{TeamId.Replace(m.Groups["team"].Value, string.Empty)}'"
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
        /// Points every team-scoped macro that refers to a team in the given project at another team of that project.
        /// </summary>
        public static string ReplaceTeam(string wiql, string project, string fromTeam, string toTeam)
        {
            return TeamReference.Replace(wiql, m => IsProject(m, project) && string.Equals(m.Groups["team"].Value, fromTeam, StringComparison.OrdinalIgnoreCase)
                ? $"{m.Groups["macro"].Value}'[{project}]\\{toTeam}'"
                : m.Value);
        }

        /// <summary>
        /// Returns the team names referenced in team-scoped macros for the given project.
        /// </summary>
        public static IEnumerable<string> GetReferencedTeams(string wiql, string project)
        {
            return TeamReference.Matches(wiql).Cast<Match>()
                .Where(m => IsProject(m, project))
                .Select(m => TeamId.Replace(m.Groups["team"].Value, string.Empty))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsProject(Match match, string project)
        {
            return string.Equals(match.Groups["project"].Value, project, StringComparison.OrdinalIgnoreCase);
        }
    }
}
