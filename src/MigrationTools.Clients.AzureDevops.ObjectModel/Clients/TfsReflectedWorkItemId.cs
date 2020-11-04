using System;
using System.Text.RegularExpressions;
using MigrationTools.DataContracts;

namespace MigrationTools.Clients
{
    public class TfsReflectedWorkItemId : ReflectedWorkItemId
    {
        private Uri _Connection;
        private string _ProjectName;
        private string _WorkItemId;
        private static readonly Regex ReflectedIdRegex = new Regex(@"^(?<org>[\S ]+)\/(?<project>[\S ]+)\/_workitems\/edit\/(?<id>\d+)", RegexOptions.Compiled);

        public TfsReflectedWorkItemId(WorkItemData workItem) : base(workItem.Id)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _WorkItemId = workItem.Id;
            _ProjectName = workItem.ProjectName;
            _Connection = workItem.ToWorkItem().Store.TeamProjectCollection.Uri;
        }

        public TfsReflectedWorkItemId(string ReflectedWorkItemId) : base(ReflectedWorkItemId)
        {
            if (ReflectedWorkItemId is null)
            {
                throw new ArgumentNullException(nameof(ReflectedWorkItemId));
            }

            var match = ReflectedIdRegex.Match(ReflectedWorkItemId);
            if (match.Success)
            {
                _WorkItemId = match.Groups["id"].Value;
                _ProjectName = match.Groups["project"].Value;
                _Connection = new Uri(match.Groups["org"].Value);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}/_workitems/edit/{2}", _Connection.ToString().TrimEnd('/'), _ProjectName, _WorkItemId);
        }
    }
}