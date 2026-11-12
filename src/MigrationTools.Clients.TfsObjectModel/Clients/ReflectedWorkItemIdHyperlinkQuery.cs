using System.Collections.Generic;
using System.Linq;

namespace MigrationTools.Clients
{
    /// <summary>
    /// WIQL cannot filter on a hyperlink's location, so the lookup selects the candidates that can carry the marker
    /// hyperlink and reads their links.
    /// </summary>
    public static class ReflectedWorkItemIdHyperlinkQuery
    {
        public static string Build(IEnumerable<string> workItemTypes)
        {
            var typeList = string.Join(", ", workItemTypes.Select(t => "'" + t.Replace("'", "''") + "'"));
            return "SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @TeamProject"
                + " AND [System.HyperLinkCount] > 0"
                + " AND [System.WorkItemType] IN (" + typeList + ")";
        }
    }
}
