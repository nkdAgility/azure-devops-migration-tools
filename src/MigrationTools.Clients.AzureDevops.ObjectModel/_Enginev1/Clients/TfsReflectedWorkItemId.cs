using System;
using System.Text.RegularExpressions;
using MigrationTools.DataContracts;
using Serilog;

namespace MigrationTools._EngineV1.Clients
{
    public class TfsReflectedWorkItemId : ReflectedWorkItemId
    {
        private Uri _Connection;
        private string _ProjectName;
        private string _WorkItemId;
        private static readonly Regex ReflectedIdRegex = new Regex(@"^(?<org>[\S ]+)\/(?<project>[\S ]+)\/_workitems\/edit\/(?<id>\d+)", RegexOptions.Compiled);

        public TfsReflectedWorkItemId(WorkItemData workItem) : base(workItem)
        {
            if (workItem is null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _WorkItemId = workItem.Id;
            _ProjectName = workItem.ProjectName;
            _Connection = workItem.ToWorkItem().Store.TeamProjectCollection.Uri;
        }

        public TfsReflectedWorkItemId(int workItemId, string tfsProject, Uri tfsTeamProjectCollection) : base(workItemId)
        {
            if (workItemId == 0)
            {
                throw new ArgumentNullException(nameof(workItemId));
            }

            _WorkItemId = workItemId.ToString();
            _ProjectName = tfsProject;
            _Connection = tfsTeamProjectCollection;
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
                Log.Verbose("TfsReflectedWorkItemId: Match Sucess from {ReflectedWorkItemId}: {@ReflectedWorkItemIdObject}", ReflectedWorkItemId, this);
                _Connection = new Uri(match.Groups[1].Value);
                _ProjectName = match.Groups[2].Value;
                _WorkItemId = match.Groups[3].Value;
            }
            else
            {
                Log.Error("TfsReflectedWorkItemId: Unable to match ReflectedWorkItemId({ReflectedWorkItemId}) as id! ", ReflectedWorkItemId);
                throw new Exception("Unable to Parse ReflectedWorkItemId. Check Log...");
            }
        }

        public override string ToString()
        {
            if (_Connection is null)
            {
                throw new ArgumentNullException(nameof(_Connection));
            }
            if (_ProjectName is null)
            {
                throw new ArgumentNullException(nameof(_ProjectName));
            }
            if (_WorkItemId is null)
            {
                throw new ArgumentNullException(nameof(_WorkItemId));
            }
            return string.Format("{0}/{1}/_workitems/edit/{2}", _Connection.ToString().TrimEnd('/'), _ProjectName, _WorkItemId);
        }
    }
}