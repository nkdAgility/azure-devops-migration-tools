using System;
using System.Collections.Generic;
using System.Linq;

namespace MigrationTools.Clients
{
    /// <summary>
    /// Work item types that cannot have custom fields on an inherited process, so cannot hold the configured reflected
    /// work item ID field. For these types the reflected work item ID is kept in <see cref="ReflectedWorkItemIdField"/>,
    /// which every one of them has and WIQL can match exactly.
    /// </summary>
    public static class NonCustomisableWorkItemTypes
    {
        public const string ReflectedWorkItemIdField = "Microsoft.VSTS.Build.IntegrationBuild";

        public static readonly IReadOnlyList<string> Types =
        [
            "Code Review Request",
            "Code Review Response",
            "Feedback Request",
            "Feedback Response",
            "Shared Parameter",
            "Shared Steps"
        ];

        public static bool Contains(string workItemType) => Types.Contains(workItemType, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The field that holds the reflected work item ID on a target type: the configured field when the type has it,
        /// <see cref="ReflectedWorkItemIdField"/> for a listed type that lacks it and has that field, otherwise
        /// <see langword="null"/>.
        /// </summary>
        public static string ResolveField(string workItemType, string configuredField, Func<string, bool> typeHasField)
        {
            if (typeHasField(configuredField))
            {
                return configuredField;
            }
            if (Contains(workItemType) && typeHasField(ReflectedWorkItemIdField))
            {
                return ReflectedWorkItemIdField;
            }
            return null;
        }

        public static string BuildLookupQuery()
        {
            var typeList = string.Join(", ", Types.Select(t => "'" + t + "'"));
            return $"[{ReflectedWorkItemIdField}] = @idToFind AND [System.WorkItemType] IN ({typeList})";
        }
    }
}
